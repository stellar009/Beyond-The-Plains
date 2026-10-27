// ---------------------------------------------------------------------------
// InkDialogueSystemMenu.cs
// ---------------------------------------------------------------------------
// Editor-only menu items for convenience. Adds entries under the
// "Ink Dialogue System" top-level menu:
//   - Create the recommended folder layout
//   - Open the persistent save directory in the OS file browser
//   - Validate that the Ink-Unity-Integration package is installed
//
// This file MUST live in an Editor/ folder (or be wrapped in #if
// UNITY_EDITOR). It uses UnityEditor APIs that are stripped from builds.
// ---------------------------------------------------------------------------

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using InkDialogueSystem.Core;

namespace InkDialogueSystem.Editor
{
    public static class InkDialogueSystemMenu
    {
        private const string MenuRoot = "Ink Dialogue System/";

        [MenuItem(MenuRoot + "Create Recommended Folder Layout")]
        public static void CreateFolderLayout()
        {
            string[] folders =
            {
                "Assets/Resources/Ink",
                "Assets/StreamingAssets/Ink",
                "Assets/InkDialogueSystem/Scripts",
                "Assets/InkDialogueSystem/Prefabs",
                "Assets/InkDialogueSystem/Scenes",
            };
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    Debug.Log($"[InkDialogueSystem] Created folder: {folder}");
                }
            }
            AssetDatabase.Refresh();
        }

        [MenuItem(MenuRoot + "Open Save Directory")]
        public static void OpenSaveDirectory()
        {
            string path = Path.Combine(Application.persistentDataPath, "InkDialogueSaves");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem(MenuRoot + "Validate Ink Runtime Installed")]
        public static void ValidateInkRuntime()
        {
            // ink-unity-integration ships the Ink.Runtime namespace from
            // the assembly InkRuntimeDriver.dll. If the user forgot to
            // install the package, this catches it early.
            System.Type storyType = null;
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly asm in assemblies)
            {
                storyType = asm.GetType("Ink.Runtime.Story");
                if (storyType != null) break;
            }
            if (storyType == null)
            {
                EditorUtility.DisplayDialog(
                    "Ink Runtime Missing",
                    "The Ink.Runtime.Story type was not found.\n\n" +
                    "Please install the 'ink-unity-integration' package from " +
                    "https://github.com/inkle/ink-unity-integration before using " +
                    "the InkDialogueSystem.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Ink Runtime OK",
                    "Ink.Runtime.Story is available. The dialogue system is ready to use.",
                    "OK");
            }
        }
    }
}
#endif
