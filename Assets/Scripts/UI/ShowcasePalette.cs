using UnityEngine;

namespace LearningArchitect.UI
{
    public static class ShowcasePalette
    {
        public const string AccentHex = "FF8C3C";
        public const string AccentStrongHex = "FFA14F";
        public const string TextPrimaryHex = "E6EAF0";
        public const string TextSecondaryHex = "9CA3AF";
        public const string TextMutedHex = "858C99";
        public const string SuccessHex = "22C55E";
        public const string WarningHex = "F59E0B";
        public const string ErrorHex = "EF4444";

        public static readonly Color BgMain = Hex("0A0D10");
        public static readonly Color BgDeep = Hex("050608");

        public static readonly Color PanelMain = Hex("0D1013");
        public static readonly Color PanelSoft = Hex("12151A");
        public static readonly Color PanelHover = Hex("171B23");

        public static readonly Color AccentMain = Hex(AccentHex);
        public static readonly Color AccentStrong = Hex(AccentStrongHex);
        public static readonly Color TextPrimary = Hex(TextPrimaryHex);
        public static readonly Color TextSecondary = Hex(TextSecondaryHex);
        public static readonly Color TextMuted = Hex(TextMutedHex);

        public static readonly Color Success = Hex(SuccessHex);
        public static readonly Color Warning = Hex(WarningHex);
        public static readonly Color Error = Hex(ErrorHex);

        public static Color AccentSoft(float alpha = 0.15f) => WithAlpha(AccentMain, alpha);
        public static Color AccentGlow(float alpha = 0.35f) => WithAlpha(AccentMain, alpha);
        public static Color BorderMain(float alpha = 0.06f) => new Color(1f, 1f, 1f, alpha);
        public static Color BorderAccent(float alpha = 0.25f) => WithAlpha(AccentMain, alpha);
        public static Color Divider(float alpha = 0.04f) => new Color(1f, 1f, 1f, alpha);

        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }
    }
}
