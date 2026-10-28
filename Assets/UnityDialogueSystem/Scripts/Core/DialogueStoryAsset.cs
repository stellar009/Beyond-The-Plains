using UnityEngine;
using Ink.Runtime;
using Ink.UnityIntegration;

namespace ZaiGames.DialogueSystem.Core
{
    /// <summary>
    /// Asset wrapper around a compiled ink story. Resolves the JSON
    /// payload (either from an <see cref="InkFile"/> asset, the v2.0
    /// preferred path, or from a fallback TextAsset containing pre-
    /// compiled JSON) and produces a fresh <see cref="Story"/> instance
    /// on demand via <see cref="CreateStory"/>.
    ///
    /// Why a ScriptableObject: the same story source can be referenced
    /// by many runners (NPC A and NPC B can both load story.ink but
    /// each gets its own Story instance), and the asset is data-driven
    /// — designers can swap stories without touching scene prefabs.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueStoryAsset", menuName = "Dialogue/Story Asset", order = 0)]
    public class DialogueStoryAsset : ScriptableObject
    {
        [Header("Story source")]
        [Tooltip("InkFile asset (preferred v2.0 path). The .ink file is compiled to JSON by the Ink ScriptedImporter and exposed via InkFile.storyJson.")]
        [SerializeField] private InkFile inkFile;

        [Tooltip("Fallback: a TextAsset containing pre-compiled ink JSON. Used when InkFile is null (e.g. for runtime-downloaded stories).")]
        [SerializeField] private TextAsset compiledJsonFallback;

        [Header("Options")]
        [Tooltip("If true, CreateStory() always returns a brand-new Story instance. If false, the asset caches one shared instance.")]
        [SerializeField] private bool freshInstanceEachCall = true;

        private Story _cached;

        /// <summary>
        /// Builds a fresh <see cref="Story"/> from the resolved JSON.
        /// Returns null and logs an error if no source is wired up.
        /// </summary>
        public Story CreateStory()
        {
            if (!freshInstanceEachCall && _cached != null)
                return _cached;

            string json = ResolveJson();
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError(
                    $"[{nameof(DialogueStoryAsset)}] No compiled ink JSON on '{name}'. " +
                    "Assign an InkFile (preferred) or a pre-compiled JSON TextAsset.");
                return null;
            }

            try
            {
                _cached = new Story(json);
                return _cached;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{nameof(DialogueStoryAsset)}] Failed to construct Story from JSON on '{name}': {e}");
                return null;
            }
        }

        /// <summary>
        /// Returns the cached shared instance, creating it on first call.
        /// Use this when you want a single long-lived Story per asset
        /// (e.g. global world-state story shared across scenes).
        /// </summary>
        public Story GetSharedStory() => _cached ??= CreateStory();

        public bool HasSource => !string.IsNullOrEmpty(ResolveJson());

        private string ResolveJson()
        {
            // Preferred: InkFile.storyJson (populated by Ink ScriptedImporter in v2.0).
            if (inkFile != null)
            {
                string json = inkFile.storyJson;
                if (!string.IsNullOrEmpty(json))
                    return json;
            }

            // Fallback: pre-compiled JSON in a TextAsset (useful for runtime downloads).
            if (compiledJsonFallback != null && !string.IsNullOrEmpty(compiledJsonFallback.text))
                return compiledJsonFallback.text;

            return null;
        }
    }
}
