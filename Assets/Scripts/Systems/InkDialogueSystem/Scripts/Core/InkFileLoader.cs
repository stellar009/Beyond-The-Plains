// ---------------------------------------------------------------------------
// InkFileLoader.cs
// ---------------------------------------------------------------------------
// Loads compiled .ink.json story files into Ink.Runtime.Story objects.
// Two load paths are supported:
//   1. Resources/Ink/<name>.json  -- baked into the build, fast, default.
//   2. StreamingAssets/Ink/<name>.json -- hot-swappable at runtime, great
//      for modders and for swapping stories without rebuilding the player.
//
// Unity's TextAsset is the canonical wrapper; we use Newtonsoft or
// JsonUtility only minimally because Ink's own Story constructor accepts
// a raw JSON string directly.
// ---------------------------------------------------------------------------

using System;
using System.IO;
using UnityEngine;
using Ink.Runtime;
using IOPath = System.IO.Path;

namespace InkDialogueSystem.Core
{
    /// <summary>
    /// Resolves .ink.json story files into live Story objects. Supports
    /// both baked-in Resources and hot-swappable StreamingAssets.
    /// </summary>
    public static class InkFileLoader
    {
        /// <summary>
        /// Load a compiled Ink story by short name (no extension).
        /// Tries Resources first, then StreamingAssets.
        /// </summary>
        /// <param name="storyName">Name without extension, e.g.
        /// "sample_story". The loader will look for "Resources/Ink/
        /// sample_story.json" first, then "StreamingAssets/Ink/
        /// sample_story.json".</param>
        /// <returns>A new Story ready to bind external functions to, or
        /// null if the file could not be found in either location.</returns>
        public static Story LoadStory(string storyName)
        {
            if (string.IsNullOrEmpty(storyName))
            {
                Debug.LogError("[InkFileLoader] storyName is null or empty.");
                return null;
            }

            string json = TryLoadFromResources(storyName) ?? TryLoadFromStreamingAssets(storyName);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError($"[InkFileLoader] Could not find Ink story '{storyName}' " +
                               "in Resources/Ink or StreamingAssets/Ink. Did you compile " +
                               "the .ink file? (Use Inkowski > Compile Ink in Unity).");
                return null;
            }

            try
            {
                return new Story(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InkFileLoader] Failed to parse Ink JSON for '{storyName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>Same as LoadStory but reads raw JSON without parsing,
        /// useful when you want to stash the JSON for save/load later.
        /// </summary>
        public static string LoadStoryJson(string storyName)
        {
            return TryLoadFromResources(storyName) ?? TryLoadFromStreamingAssets(storyName);
        }

        // -----------------------------------------------------------------
        // Internals
        // -----------------------------------------------------------------

        private static string TryLoadFromResources(string storyName)
        {
            // Resources.Load takes a path WITHOUT extension.
            TextAsset asset = Resources.Load<TextAsset>($"Ink/{storyName}");
            if (asset != null)
            {
                return asset.text;
            }
            return null;
        }

        private static string TryLoadFromStreamingAssets(string storyName)
        {
            string path = IOPath.Combine(Application.streamingAssetsPath, "Ink", storyName + ".json");

            // On Android, StreamingAssets live inside an APK archive and
            // cannot be read with File.ReadAllText. UnityWebRequest is the
            // supported cross-platform reader.
#if UNITY_ANDROID && !UNITY_EDITOR
            return ReadStreamingAssetAndroid(path);
#else
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return null;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static string ReadStreamingAssetAndroid(string path)
        {
            using (UnityEngine.Networking.UnityWebRequest req =
                   UnityEngine.Networking.UnityWebRequest.Get(path))
            {
                req.SendWebRequest();
                // Block on the request. Dialogue loading is a one-shot
                // operation at scene start, so a synchronous wait is OK.
                while (!req.isDone) { }
                if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    return req.downloadHandler.text;
                }
                Debug.LogWarning($"[InkFileLoader] Android StreamingAssets read failed: {req.error}");
                return null;
            }
        }
#endif
    }
}
