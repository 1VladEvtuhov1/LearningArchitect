import fs from "node:fs/promises";
import path from "node:path";
import vm from "node:vm";

const siteRoot = path.resolve(import.meta.dirname, "..");
const appJsPath = path.join(siteRoot, "app.js");
const contentDir = path.join(siteRoot, "content");
const csvPath = path.join(contentDir, "site-copy.csv");
const jsonPath = path.join(contentDir, "site-copy.runtime.json");

const command = process.argv[2] || "build-json";

await fs.mkdir(contentDir, { recursive: true });

switch (command) {
  case "extract-appjs":
    await extractAppJsContent();
    break;
  case "build-json":
    await buildRuntimeJsonFromCsv();
    break;
  default:
    throw new Error(`Unknown command '${command}'. Use 'extract-appjs' or 'build-json'.`);
}

async function extractAppJsContent() {
  const appSource = await fs.readFile(appJsPath, "utf8");
  const translations = evaluateLiteral(extractLiteral(appSource, "const translations = ", "\n  const modules = ["), "{");
  const modules = evaluateLiteral(extractLiteral(appSource, "const modules = ", "\n\n  const demoState ="), "[");

  const rows = [
    ...flattenLocalizedRows("site", translations.en, translations.ru),
    ...flattenModuleRows(modules)
  ];

  await fs.writeFile(csvPath, encodeCsv(rows), "utf8");
  await fs.writeFile(jsonPath, JSON.stringify({ translations, modules }, null, 2) + "\n", "utf8");
}

async function buildRuntimeJsonFromCsv() {
  const csvSource = await fs.readFile(csvPath, "utf8");
  const rows = parseCsv(csvSource);
  const content = buildContentFromRows(rows);
  await fs.writeFile(jsonPath, JSON.stringify(content, null, 2) + "\n", "utf8");
}

function extractLiteral(source, startMarker, endMarker) {
  const startIndex = source.indexOf(startMarker);
  if (startIndex < 0)
    throw new Error(`Could not find start marker '${startMarker}'.`);

  const fromStart = startIndex + startMarker.length;
  const endIndex = source.indexOf(endMarker, fromStart);
  if (endIndex < 0)
    throw new Error(`Could not find end marker '${endMarker}'.`);

  return source.slice(fromStart, endIndex).trim();
}

function evaluateLiteral(source, expectedFirstCharacter) {
  const normalizedSource = source.replace(/;\s*$/, "");
  if (!normalizedSource.startsWith(expectedFirstCharacter))
    throw new Error(`Expected literal to start with '${expectedFirstCharacter}'.`);

  return vm.runInNewContext(`(${normalizedSource})`, {});
}

function flattenLocalizedRows(prefix, englishNode, russianNode) {
  const rows = [];
  flattenLocalizedNode(rows, prefix, englishNode, russianNode);
  return rows;
}

function flattenLocalizedNode(rows, currentPath, englishNode, russianNode) {
  if (isPrimitive(englishNode) && isPrimitive(russianNode)) {
    rows.push({
      scope: "site",
      key: currentPath,
      shared: "",
      en: normalizeCellValue(englishNode),
      ru: normalizeCellValue(russianNode)
    });
    return;
  }

  if (Array.isArray(englishNode) || Array.isArray(russianNode)) {
    const englishArray = Array.isArray(englishNode) ? englishNode : [];
    const russianArray = Array.isArray(russianNode) ? russianNode : [];
    const itemCount = Math.max(englishArray.length, russianArray.length);
    for (let index = 0; index < itemCount; index++)
      flattenLocalizedNode(rows, `${currentPath}[${index}]`, englishArray[index], russianArray[index]);
    return;
  }

  const englishObject = englishNode && typeof englishNode === "object" ? englishNode : {};
  const russianObject = russianNode && typeof russianNode === "object" ? russianNode : {};
  const keys = new Set([...Object.keys(englishObject), ...Object.keys(russianObject)]);
  for (const key of keys)
    flattenLocalizedNode(rows, `${currentPath}.${key}`, englishObject[key], russianObject[key]);
}

function flattenModuleRows(modules) {
  const rows = [];
  for (const module of modules) {
    const moduleId = module.id;
    flattenSharedModuleNode(rows, moduleId, "", module);
    flattenModuleLocalizedNode(rows, moduleId, "", module.en, module.ru);
  }

  return rows;
}

function flattenSharedModuleNode(rows, moduleId, currentPath, node) {
  if (node == null || typeof node !== "object" || Array.isArray(node))
    return;

  for (const [key, value] of Object.entries(node)) {
    if (key === "en" || key === "ru")
      continue;

    const nextPath = currentPath ? `${currentPath}.${key}` : key;
    if (isPrimitive(value)) {
      rows.push({
        scope: "module",
        key: `${moduleId}.${nextPath}`,
        shared: normalizeCellValue(value),
        en: "",
        ru: ""
      });
      continue;
    }

    if (Array.isArray(value)) {
      for (let index = 0; index < value.length; index++) {
        const arrayPath = `${nextPath}[${index}]`;
        if (isPrimitive(value[index])) {
          rows.push({
            scope: "module",
            key: `${moduleId}.${arrayPath}`,
            shared: normalizeCellValue(value[index]),
            en: "",
            ru: ""
          });
        }
        else {
          flattenSharedModuleNode(rows, moduleId, arrayPath, value[index]);
        }
      }

      continue;
    }

    flattenSharedModuleNode(rows, moduleId, nextPath, value);
  }
}

function flattenModuleLocalizedNode(rows, moduleId, currentPath, englishNode, russianNode) {
  if (isPrimitive(englishNode) && isPrimitive(russianNode)) {
    rows.push({
      scope: "module",
      key: `${moduleId}.${currentPath}`,
      shared: "",
      en: normalizeCellValue(englishNode),
      ru: normalizeCellValue(russianNode)
    });
    return;
  }

  if (Array.isArray(englishNode) || Array.isArray(russianNode)) {
    const englishArray = Array.isArray(englishNode) ? englishNode : [];
    const russianArray = Array.isArray(russianNode) ? russianNode : [];
    const itemCount = Math.max(englishArray.length, russianArray.length);
    for (let index = 0; index < itemCount; index++) {
      const arrayPath = `${currentPath}[${index}]`;
      flattenModuleLocalizedNode(rows, moduleId, arrayPath, englishArray[index], russianArray[index]);
    }

    return;
  }

  const englishObject = englishNode && typeof englishNode === "object" ? englishNode : {};
  const russianObject = russianNode && typeof russianNode === "object" ? russianNode : {};
  const keys = new Set([...Object.keys(englishObject), ...Object.keys(russianObject)]);
  for (const key of keys) {
    const nextPath = currentPath ? `${currentPath}.${key}` : key;
    flattenModuleLocalizedNode(rows, moduleId, nextPath, englishObject[key], russianObject[key]);
  }
}

function buildContentFromRows(rows) {
  const translations = {
    en: {},
    ru: {}
  };

  const moduleMap = new Map();
  const moduleOrder = [];

  for (const row of rows) {
    if (!row.scope || !row.key)
      continue;

    if (row.scope === "site") {
      if (row.en !== "")
        assignPath(translations.en, row.key, row.en);
      if (row.ru !== "")
        assignPath(translations.ru, row.key, row.ru);
      continue;
    }

    if (row.scope !== "module")
      continue;

    const [moduleId, ...pathParts] = row.key.split(".");
    if (!moduleId || pathParts.length === 0)
      continue;

    let moduleEntry = moduleMap.get(moduleId);
    if (!moduleEntry) {
      moduleEntry = { id: moduleId, en: {}, ru: {} };
      moduleMap.set(moduleId, moduleEntry);
      moduleOrder.push(moduleId);
    }

    const localPath = pathParts.join(".");
    if (row.shared !== "")
      assignPath(moduleEntry, localPath, row.shared);
    if (row.en !== "")
      assignPath(moduleEntry.en, localPath, row.en);
    if (row.ru !== "")
      assignPath(moduleEntry.ru, localPath, row.ru);
  }

  const modules = moduleOrder.map(function mapModule(moduleId) {
    return moduleMap.get(moduleId);
  });

  return { translations, modules };
}

function assignPath(target, pathExpression, value) {
  const tokens = tokenizePath(pathExpression);
  let current = target;

  for (let index = 0; index < tokens.length; index++) {
    const token = tokens[index];
    const isLast = index === tokens.length - 1;
    const nextToken = isLast ? null : tokens[index + 1];

    if (isLast) {
      current[token] = value;
      continue;
    }

    if (current[token] == null) {
      current[token] = typeof nextToken === "number" ? [] : {};
    }

    current = current[token];
  }
}

function tokenizePath(pathExpression) {
  const tokens = [];
  const pattern = /([^[.\]]+)|\[(\d+)\]/g;
  let match;
  while ((match = pattern.exec(pathExpression)) !== null) {
    if (match[1] != null)
      tokens.push(match[1]);
    else
      tokens.push(Number.parseInt(match[2], 10));
  }

  return tokens;
}

function encodeCsv(rows) {
  const header = ["scope", "key", "shared", "en", "ru"];
  const lines = [header.map(escapeCsvCell).join(",")];
  for (const row of rows)
    lines.push([row.scope, row.key, row.shared, row.en, row.ru].map(escapeCsvCell).join(","));
  return lines.join("\n") + "\n";
}

function escapeCsvCell(value) {
  const text = normalizeCellValue(value);
  if (/[",\n\r]/.test(text))
    return `"${text.replace(/"/g, "\"\"")}"`;

  return text;
}

function parseCsv(source) {
  const rows = [];
  const records = [];
  let currentField = "";
  let currentRow = [];
  let insideQuotes = false;

  for (let index = 0; index < source.length; index++) {
    const character = source[index];
    const nextCharacter = source[index + 1];

    if (insideQuotes) {
      if (character === "\"" && nextCharacter === "\"") {
        currentField += "\"";
        index++;
        continue;
      }

      if (character === "\"") {
        insideQuotes = false;
        continue;
      }

      currentField += character;
      continue;
    }

    if (character === "\"") {
      insideQuotes = true;
      continue;
    }

    if (character === ",") {
      currentRow.push(currentField);
      currentField = "";
      continue;
    }

    if (character === "\r")
      continue;

    if (character === "\n") {
      currentRow.push(currentField);
      currentField = "";
      records.push(currentRow);
      currentRow = [];
      continue;
    }

    currentField += character;
  }

  if (currentField !== "" || currentRow.length > 0) {
    currentRow.push(currentField);
    records.push(currentRow);
  }

  if (records.length === 0)
    return rows;

  const [header, ...dataRows] = records;
  for (const record of dataRows) {
    if (record.every(function isBlank(value) { return value === ""; }))
      continue;

    const row = {};
    for (let index = 0; index < header.length; index++)
      row[header[index]] = record[index] ?? "";
    rows.push(row);
  }

  return rows;
}

function normalizeCellValue(value) {
  if (value == null)
    return "";

  return String(value).replace(/\r\n/g, "\n");
}

function isPrimitive(value) {
  return value == null || typeof value === "string" || typeof value === "number" || typeof value === "boolean";
}
