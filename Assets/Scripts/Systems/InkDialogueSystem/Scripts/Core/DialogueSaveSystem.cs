// ---------------------------------------------------------------------------
// DialogueSaveSystem.cs
// ---------------------------------------------------------------------------
// Persists and restores dialogue state. Uses Application.persistentDataPath
// (a Unity-managed, OS-appropriate location) plus one JSON file per slot.
//
// Save format = SaveData (see Data/SaveData.cs) serialized via JsonUtility.
// Note: JsonUtility does NOT support List<DialogueLine> nested inside
// SaveData if those types are [Serializable] AND have public fields -- it
// does, in fact, handle it. We rely on that here.
//
// Save points come from #save tags in the .ink file. The DialogueManager
// fires OnSaveTag, which this system listens to and triggers an auto-save
// to the slot named in the tag (default "auto").
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using InkDialogueSystem.Data;

namespace InkDialogueSystem.Core
{
    /// <summary>
    /// Writes and reads SaveData JSON files from disk. Listens to
    /// DialogueEvents.OnSaveTag to perform automatic saves.
    /// </summary>
    public class DialogueSaveSystem : MonoBehaviour
    {
        [Tooltip("Whether to auto-save when the .ink story emits a #save tag.")]
        [SerializeField] private bool autoSaveOnSaveTag = true;

        [Tooltip("If true, the system also writes a quicksave every N seconds of playtime.")]
        [SerializeField] private bool periodicQuicksave = false;

        [Tooltip("Periodic quicksave interval, in seconds.")]
        [SerializeField] private float quicksaveInterval = 60f;

        private float _timeSinceQuicksave = 0f;
        private static string SaveDirectory =>
            Path.Combine(Application.persistentDataPath, "InkDialogueSaves");

        #region Lifecycle

        private void OnEnable()
        {
            DialogueEvents.OnSaveTag += HandleSaveTag;
        }

        private void OnDisable()
        {
            DialogueEvents.OnSaveTag -= HandleSaveTag;
        }

        private void Update()
        {
            if (!periodicQuicksave) return;
            _timeSinceQuicksave += Time.deltaTime;
            if (_timeSinceQuicksave >= quicksaveInterval)
            {
                _timeSinceQuicksave = 0f;
                Save("quicksave");
            }
        }

        #endregion

        #region Public API

        /// <summary>Capture the current DialogueManager state and write it
        /// to <c>{persistentDataPath}/InkDialogueSaves/{slotName}.json</c>.
        /// Overwrites existing saves with the same slot name.</summary>
        public bool Save(string slotName)
        {
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueSaveSystem] No active DialogueManager to save.");
                return false;
            }

            SaveData data = DialogueManager.Instance.BuildSaveData(slotName);
            return WriteSave(data);
        }

        /// <summary>Load a slot and apply it to the active DialogueManager.
        /// Returns true if the load succeeded and the story was rewound to
        /// the saved position.</summary>
        public bool Load(string slotName)
        {
            SaveData data = ReadSave(slotName);
            if (data == null || !data.IsValid())
            {
                Debug.LogWarning($"[DialogueSaveSystem] Save slot '{slotName}' is missing or invalid.");
                return false;
            }

            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueSaveSystem] No active DialogueManager to load into.");
                return false;
            }

            return DialogueManager.Instance.ApplySaveData(data);
        }

        /// <summary>Delete the save file for the given slot.</summary>
        public bool DeleteSave(string slotName)
        {
            string path = GetSavePath(slotName);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        /// <summary>Check whether a save exists for the given slot.</summary>
        public bool SaveExists(string slotName)
        {
            return File.Exists(GetSavePath(slotName));
        }

        /// <summary>List all save slots currently on disk.</summary>
        public List<string> ListSaveSlots()
        {
            List<string> slots = new List<string>();
            if (!Directory.Exists(SaveDirectory)) return slots;
            foreach (string file in Directory.GetFiles(SaveDirectory, "*.json"))
            {
                slots.Add(Path.GetFileNameWithoutExtension(file));
            }
            return slots;
        }

        #endregion

        #region Internals

        private void HandleSaveTag(string slotName)
        {
            if (!autoSaveOnSaveTag) return;
            if (string.IsNullOrEmpty(slotName)) slotName = "auto";
            Save(slotName);
            Debug.Log($"[DialogueSaveSystem] Auto-saved to slot '{slotName}'.");
        }

        private bool WriteSave(SaveData data)
        {
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(GetSavePath(data.slotName), json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueSaveSystem] Failed to write save '{data.slotName}': {ex.Message}");
                return false;
            }
        }

        private SaveData ReadSave(string slotName)
        {
            string path = GetSavePath(slotName);
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueSaveSystem] Failed to read save '{slotName}': {ex.Message}");
                return null;
            }
        }

        private static string GetSavePath(string slotName)
        {
            return Path.Combine(SaveDirectory, slotName + ".json");
        }

        #endregion
    }
}
