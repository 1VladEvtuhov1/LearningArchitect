using System.Collections.Generic;
using LearningArchitect.Shared.Events;
using UnityEditor;
using UnityEngine;

namespace LearningArchitect.EditorTools
{
    [CustomEditor(typeof(GameEvent))]
    public sealed class GameEventEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GameEvent gameEvent = (GameEvent)target;
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Listeners", EditorStyles.boldLabel);

            DrawAssignedListeners(gameEvent);

            if (Application.isPlaying)
                DrawRuntimeListeners(gameEvent);
            else
                EditorGUILayout.HelpBox("Enter Play Mode to see runtime-registered listeners.", MessageType.Info);
        }

        private static void DrawAssignedListeners(GameEvent gameEvent)
        {
            List<GameEventListener> assigned = FindAssignedListeners(gameEvent);
            if (assigned.Count == 0)
            {
                EditorGUILayout.LabelField("Scene / prefab assignments", "(none)");
                return;
            }

            EditorGUILayout.LabelField("Scene / prefab assignments", assigned.Count.ToString());
            for (int i = 0; i < assigned.Count; i++)
            {
                GameEventListener listener = assigned[i];
                if (listener == null)
                    continue;

                EditorGUILayout.ObjectField(listener.gameObject.name, listener, typeof(GameEventListener), true);
            }
        }

        private static void DrawRuntimeListeners(GameEvent gameEvent)
        {
            IReadOnlyList<GameEventListener> runtime = gameEvent.GetRuntimeListeners();
            EditorGUILayout.LabelField("Runtime registered", runtime.Count.ToString());
            for (int i = 0; i < runtime.Count; i++)
            {
                GameEventListener listener = runtime[i];
                if (listener == null)
                    continue;

                EditorGUILayout.ObjectField(listener.gameObject.name, listener, typeof(GameEventListener), true);
            }
        }

        private static List<GameEventListener> FindAssignedListeners(GameEvent gameEvent)
        {
            List<GameEventListener> results = new List<GameEventListener>(8);
            GameEventListener[] listeners = Resources.FindObjectsOfTypeAll<GameEventListener>();
            for (int i = 0; i < listeners.Length; i++)
            {
                GameEventListener listener = listeners[i];
                if (listener == null || listener.Event != gameEvent)
                    continue;

                if (EditorUtility.IsPersistent(listener.gameObject))
                    results.Add(listener);
                else if (listener.gameObject.scene.IsValid())
                    results.Add(listener);
            }

            return results;
        }
    }
}
