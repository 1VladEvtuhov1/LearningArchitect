using System;
using UnityEngine;

namespace LearningArchitect.Modules.InterviewArena.Net
{
    public static class BackendJsonParser
    {
        public static bool TryDeserialize<T>(string json, out T value) where T : class
        {
            value = null;
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                value = JsonUtility.FromJson<T>(json);
                return value != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string Serialize<T>(T value) => JsonUtility.ToJson(value);
    }
}
