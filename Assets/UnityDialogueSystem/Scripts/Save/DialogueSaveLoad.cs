using System;
using System.IO;
using UnityEngine;
using Ink.Runtime;
using ZaiGames.DialogueSystem.Core;
using IOPath = System.IO.Path;

namespace ZaiGames.DialogueSystem.Save
{
    /// <summary>
    /// File-based save/load for the ink story state. Wraps
    /// <c>story.state.ToJson()</c> / <c>story.state.LoadJson(string)</c>
    /// and persists to <see cref="Application.persistentDataPath"/>.
    ///
    /// Usage:
    /// <code>
    /// DialogueSaveLoad.Save(runner);        // F5
    /// DialogueSaveLoad.LoadInto(runner);   // F9 — then call runner.RestartFromLoadedState()
    /// DialogueSaveLoad.SaveExists();       // gate UI prompts
    /// DialogueSaveLoad.DeleteSave();       // new game
    /// </code>
    ///
    /// Thread-safety: must be called on the main thread (uses Unity APIs).
    /// </summary>
    public static class DialogueSaveLoad
    {
        public const string DefaultFileName = "dialogue_save.json";

        public static string DefaultPath => IOPath.Combine(Application.persistentDataPath, DefaultFileName);

        // -----------------------------------------------------------------
        // Runner convenience API
        // -----------------------------------------------------------------

        public static bool Save(DialogueRunner runner, string path = null)
            => runner != null && runner.Story != null && SaveStory(runner.Story, path);

        public static bool LoadInto(DialogueRunner runner, string path = null)
            => runner != null && runner.Story != null && LoadIntoStory(runner.Story, path);

        // -----------------------------------------------------------------
        // Raw Story API
        // -----------------------------------------------------------------

        public static bool SaveStory(Story story, string path = null)
        {
            if (story == null)
            {
                Debug.LogWarning("[DialogueSaveLoad] Cannot save: story is null.");
                return false;
            }
            try
            {
                string json = story.state.ToJson();
                string filePath = string.IsNullOrEmpty(path) ? DefaultPath : path;
                string dir = IOPath.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(filePath, json);
                Debug.Log($"[DialogueSaveLoad] Saved story state ({json.Length} bytes) to {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DialogueSaveLoad] Save failed: {e}");
                return false;
            }
        }

        public static bool LoadIntoStory(Story story, string path = null)
        {
            if (story == null)
            {
                Debug.LogWarning("[DialogueSaveLoad] Cannot load: story is null.");
                return false;
            }
            string filePath = string.IsNullOrEmpty(path) ? DefaultPath : path;
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[DialogueSaveLoad] No save file at {filePath}");
                return false;
            }
            try
            {
                string json = File.ReadAllText(filePath);
                story.state.LoadJson(json);
                Debug.Log($"[DialogueSaveLoad] Loaded story state from {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DialogueSaveLoad] Load failed: {e}");
                return false;
            }
        }

        public static bool SaveExists(string path = null)
            => File.Exists(string.IsNullOrEmpty(path) ? DefaultPath : path);

        public static void DeleteSave(string path = null)
        {
            string filePath = string.IsNullOrEmpty(path) ? DefaultPath : path;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[DialogueSaveLoad] Deleted save file {filePath}");
            }
        }
    }
}
