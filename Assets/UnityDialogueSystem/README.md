# Unity Ink Dialogue System

A production-ready dialogue system for **Unity 6 LTS** built on the official
[`ink-unity-integration` v2.0](https://github.com/inkle/ink-unity-integration)
runtime and the **New Input System**.

## Features

| Feature | Implementation |
|---|---|
| **Branching choices** | Driven by `Story.currentChoices` + choice-button prefab |
| **Line tags** | `#speaker:` / `#portrait:` / `#mood:` / `#audio:` parsed by `DialogueTagParser` |
| **Typewriter reveal** | `TypewriterEffect` using Unity 6 `Awaitable`, skip-on-input |
| **Save / Load** | `story.state.ToJson()` / `LoadJson()` via `DialogueSaveLoad` |
| **Architecture** | Ryan Hipple–style ScriptableObject events; runner never references the view |
| **Input** | Polling `Keyboard.current` / `Mouse.current` (no InputActions asset required) |

## Requirements

- **Unity 6 LTS** (uses `Awaitable`, `Awaitable.WaitForSecondsAsync`, `Awaitable.EndOfFrameAsync`)
- **ink-unity-integration v2.0+** — install via Package Manager → Add package from git URL:
  ```
  https://github.com/inkle/ink-unity-integration.git#upm
  ```
- **TextMeshPro** (`com.unity.ugui` — included with Unity 6)
- **Input System** (`com.unity.inputsystem` ≥ 1.7)
  - In `Project Settings → Player → Active Input Handling`, select **Input System Package (New)** or **Both**.

## Installation

1. Copy the `Scripts/` folder and the `Ink/` folder into your Unity project's `Assets/` directory (e.g. `Assets/DialogueSystem/`).
2. Unity will recompile. The `ZaiGames.DialogueSystem.asmdef` will automatically pull in `Ink.Runtime`, `Ink.UnityIntegration`, `Unity.InputSystem`, and `Unity.TextMeshPro`.
3. Open `Ink/sample_story.ink` — the Ink ScriptedImporter will compile it to a JSON sub-asset and produce an `InkFile` asset next to it.

## Scene Setup (step-by-step)

### Step 1 — Create the ScriptableObject assets

Right-click in the Project window → **Create → Dialogue → ...** and make these five assets in a `DialogueSystem/Events/` folder:

| Asset | Menu path |
|---|---|
| `OnDialogueStarted` | Dialogue / Events / Started |
| `OnLinePresented` | Dialogue / Events / Line Presented |
| `OnChoicesPresented` | Dialogue / Events / Choices Presented |
| `OnDialogueEnded` | Dialogue / Events / Ended |
| `DemoStory` | Dialogue / Story Asset |

For `DemoStory`, drag the auto-generated `sample_story.ink` `InkFile` asset (the one Unity created next to your `.ink` file) into the **Ink File** field. Leave **Compiled Json Fallback** empty.

### Step 2 — Build the UI prefab

Create a Canvas (if you don't have one). Add a `DialogueBox` GameObject with this hierarchy:

```
DialogueBox (Canvas/Panel)
├─ SpeakerLabel      (TMP_Text)
├─ LineText          (TMP_Text + TypewriterEffect component)
├─ ContinuePrompt    (GameObject, e.g. a blinking "▼" image)
└─ ChoiceContainer   (RectTransform, Vertical Layout Group)
```

Create a `ChoiceButton` prefab with:

```
ChoiceButton (Button + ChoiceButton component)
├─ IndexLabel (TMP_Text — shows "1", "2", ...)
└─ Label      (TMP_Text — shows choice text)
```

Drag this prefab into `DialogueUIView.choiceButtonPrefab`.

### Step 3 — Add the runtime components

On an empty `DialogueSystem` GameObject in the scene, add these three components:

1. **`DialogueRunner`**
   - Story Asset → `DemoStory`
   - On Started → `OnDialogueStarted`
   - On Line Presented → `OnLinePresented`
   - On Choices Presented → `OnChoicesPresented`
   - On Ended → `OnDialogueEnded`

2. **`DialogueInputPoller`**
   - Runner → the `DialogueRunner` above (auto-detects if both on same GameObject)
   - Default keys are fine (Space/Enter/Left-click to advance; 1–9 to pick choices)

3. **`DialogueDemo`**
   - Runner → same as above
   - Auto Start On Start → true (or call `runner.Begin()` from your own bootstrap)

### Step 4 — Wire the view

On the `DialogueBox` GameObject, add **`DialogueUIView`**:

- Runner → the runner GameObject
- Root → the `DialogueBox` itself
- Speaker Label → the `SpeakerLabel` TMP_Text
- Line Typewriter → the `TypewriterEffect` on `LineText`
- Choice Container → the `ChoiceContainer` RectTransform
- Choice Button Prefab → your `ChoiceButton` prefab
- Continue Prompt → the `ContinuePrompt` GameObject
- On Started / On Line Presented / On Choices Presented / On Ended → the four SO assets you made in Step 1

### Step 5 — Press Play

- Press **Space** / **Enter** / **Left-click** to advance or skip the typewriter.
- Press **1–4** to pick a choice (top-row digits).
- Press **F5** to save, **F9** to load, **F12** to restart from the beginning.

## File Layout

```
UnityDialogueSystem/
├── Scripts/
│   ├── ZaiGames.DialogueSystem.asmdef
│   ├── Core/
│   │   ├── DialogueLine.cs          // Parsed line data (text + tags)
│   │   ├── DialogueStoryAsset.cs    // SO wrapper around an InkFile
│   │   └── DialogueRunner.cs        // Main driver, Awaitable pump
│   ├── Events/
│   │   ├── DialogueStartedGameEvent.cs
│   │   ├── DialogueLineGameEvent.cs
│   │   ├── DialogueChoicesGameEvent.cs
│   │   └── DialogueEndedGameEvent.cs
│   ├── Tags/
│   │   └── DialogueTagParser.cs     // #speaker:#portrait:#mood:#audio:
│   ├── Input/
│   │   └── DialogueInputPoller.cs   // Keyboard.current / Mouse.current polling
│   ├── UI/
│   │   ├── DialogueUIView.cs        // uGUI + TMP view
│   │   ├── TypewriterEffect.cs      // Awaitable-based char reveal
│   │   └── ChoiceButton.cs          // Choice button prefab script
│   ├── Save/
│   │   └── DialogueSaveLoad.cs      // state.ToJson / LoadJson wrapper
│   └── Demo/
│       └── DialogueDemo.cs          // F5/F9/F12 bootstrap
└── Ink/
    └── sample_story.ink             // Demo narrative with tags + choices
```

## Architecture

See **`ARCHITECTURE.md`** for the full data-flow diagram and a per-class responsibility table.

## Extending the System

### Add a new tag (e.g. `#voice:happy_03.wav`)

In `DialogueTagParser.Parse`, add a case:

```csharp
case "voice":
    line.ExtraTags["voice"] = value;   // or add a typed field to DialogueLine
    break;
```

Then subscribe to `onLinePresented` from your audio controller and play the clip.

### Bind ink variables to C# (e.g. affinity)

In a component that subscribes to `DialogueStartedGameEvent`:

```csharp
private void HandleStarted(Story story)
{
    story.ObserveVariable("affinity", (name, newValue) =>
    {
        Debug.Log($"affinity changed to {newValue}");
    });
}
```

### Replace the polling input with an InputActions asset

Delete `DialogueInputPoller` and create your own component that generates a C# class from an InputActions asset. Call the same runner methods:
- `runner.RequestAdvance()` — on Advance action triggered during `WaitingForContinue`
- `runner.RequestSkipTypewriter()` — on Advance action triggered during `Typing`
- `runner.SelectChoice(i)` — on Choose1/Choose2/... action triggered during `WaitingForChoice`

### Use a different UI toolkit (e.g. UI Toolkit)

Implement a new `MonoBehaviour` that subscribes to the same four SO events (`DialogueStartedGameEvent`, `DialogueLineGameEvent`, `DialogueChoicesGameEvent`, `DialogueEndedGameEvent`) and renders into UXML. The runner doesn't care which view is listening.

## Notes on Save/Load

- Save format: ink's `state.ToJson()` — a string containing the current path, variable values, and visit counts. Very compact (a few KB).
- Save location: `Application.persistentDataPath/dialogue_save.json`. Override by passing a custom `path` to `DialogueSaveLoad.Save`/`LoadInto`.
- To save the entire game state alongside dialogue, serialize your save struct + the ink JSON string into a single wrapper JSON.
- Calling `LoadInto(runner)` only loads state into the live Story; you must then call `runner.RestartFromLoadedState()` to resume the pump from the loaded position.

## Troubleshooting

| Symptom | Cause / Fix |
|---|---|
| `No compiled ink JSON on 'DemoStory'` | The .ink file hasn't been imported yet. Click the `.ink` file in Project view; the Ink ScriptedImporter will compile it and produce an `InkFile` sub-asset. Drag that InkFile into the asset's **Ink File** slot. |
| Typewriter plays but no text appears | Ensure the `TMP_Text` has a font asset assigned and the `TypewriterEffect` is on the same GameObject as the `TMP_Text`. |
| Skip doesn't work | Check that `DialogueUIView.runner` is assigned — the view subscribes to `runner.SkipRequested` in `OnEnable`. |
| Choices don't appear | `choiceContainer` and `choiceButtonPrefab` must both be assigned on `DialogueUIView`. |
| F9 does nothing | `DialogueSaveLoad.SaveExists()` returned false. Press F5 first to create a save. |
| Compile error about `Awaitable` | You're on Unity < 6. Either upgrade or rewrite the `await Awaitable.X` calls as coroutines. |

## License

MIT — do whatever you want. Attribution appreciated but not required.
