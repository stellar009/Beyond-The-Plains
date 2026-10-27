# Ink Dialogue System for Unity

A complete, production-ready dialogue system for Unity built on top of Inkle's **Ink** narrative scripting language. Drop your `.ink` files in, wire up the prefab, and you have a fully-featured branching dialogue UI with save/load, history, variable inspector, typewriter reveal, and 3D world interaction support.

## Features

| Feature | What it does |
|---|---|
| Full Ink runtime | Supports variables, knots, stitches, choices, gathers, diverts, functions, threads, tags — anything Ink can do. |
| Save / Load | JSON-based save slots in `persistentDataPath`. Auto-saves on `#save` tags. F5/F9 quicksave hotkeys. |
| Choice History | Scrollable panel showing every line so far this session. Choice beats are color-coded. |
| Variable Inspector | Debug panel listing every Ink variable and current story state. |
| Typewriter Effect | Character-by-character reveal with skip-on-input, punctuation pause, and optional tick SFX. |
| **3D NPC Interaction** | `NPCInteractable.cs` lets you click or walk up to NPCs in your 3D scene to start dialogue. Supports click raycast + proximity trigger. |
| Dark Game UI | Sci-fi terminal aesthetic, dark slate + cyan/purple accents, monospace headings. Designed to be reskinned. |

## Quick Start

### 1. Install the Ink runtime

Install the official `ink-unity-integration` package from Inkle's repo:

```
https://github.com/inkle/ink-unity-integration
```

Either add it via the Package Manager (`Window > Package Manager > + > Add package from git URL`) or clone it into `Assets/Plugins/`. Verify installation by running the menu item **Ink Dialogue System > Validate Ink Runtime Installed** — you should see a dialog confirming `Ink.Runtime.Story` is available.

### 2. Drop the scripts in

Copy the `Scripts/` folder into your project (e.g. under `Assets/InkDialogueSystem/`). Make sure the `Editor/InkDialogueSystemMenu.cs` file is inside an `Editor/` folder — Unity strips editor scripts from builds.

### 3. Create the recommended folder layout

Run the menu item **Ink Dialogue System > Create Recommended Folder Layout**. This creates:

```
Assets/Resources/Ink/              ← baked-in .ink.json files
Assets/StreamingAssets/Ink/        ← hot-swappable .ink.json files
Assets/InkDialogueSystem/Scripts/  ← the C# scripts
Assets/InkDialogueSystem/Prefabs/  ← UI prefab goes here
Assets/InkDialogueSystem/Scenes/   ← demo scene
Assets/InkDialogueSystem/Audio/    ← SFX + music clips
```

### 4. Compile the sample story

Open `Ink/sample_story.ink` in [Inky](https://www.inklestudios.com/ink/) (the Ink editor) and export it as JSON, **or** use the ink-unity-integration compiler: right-click the `.ink` file in the Project window and select **Compile Ink**. Place the resulting `sample_story.json` inside `Assets/Resources/Ink/`.

### 5. Build the UI prefab

Create a Canvas and build the layout below. Each panel needs the matching script component:

```
DialogueCanvas (Canvas, CanvasScaler 1920x1080)
└── DialoguePanel (RectTransform, Image dark bg)
    ├── SpeakerLabel (TMP_Text)           → DialogueUIController.speakerLabel
    ├── LineText (TMP_Text)               → DialogueUIController.lineText
    │   └── (TypewriterEffect component)  → required on the same GameObject as LineText
    ├── ChoicesContainer (RectTransform, VerticalLayoutGroup)
    │   └── ChoiceButtonPrefab (Button + TMP_Text + ChoiceButton component)
    ├── ContinueButton (Button)           → DialogueUIController.continueButton
    ├── ChoicePromptRoot (GameObject)     → "Choose:" hint, toggled by controller
    ├── HistoryPanel (DialogueHistoryPanel)
    │   ├── ScrollRect → Viewport → Content (VerticalLayoutGroup)
    │   └── EntryPrefab (TMP_Text)
    └── VariableInspectorPanel (VariableInspectorPanel)
        └── OutputLabel (TMP_Text)
```

### 6. Wire the scene

Create an empty GameObject named `DialogueSystem` and add these components:

- `DialogueManager` — set `defaultStoryName = sample_story`, keep `autoStartOnAwake = true` (or uncheck if you want NPCs to drive story loading)
- `DialogueSaveSystem` — defaults are fine
- `DialogueDirector` — assign the references it asks for

For a 3D game, you'll also place `NPCInteractable` components on NPC GameObjects in your scene (see the **3D Game Setup** section below).

### 7. Play

Hit Play. The sample story should start, the typewriter should reveal the first line, and the F1/H/F5/F9 hotkeys should all work.

## Hotkeys (default)

| Key | Action |
|---|---|
| `Enter` / mouse click | Advance to next line (when no choice is pending) |
| `H` | Toggle dialogue history panel |
| `F1` | Toggle variable inspector |
| `F5` | Quicksave |
| `F9` | Quickload |
| `Shift` (held) | Fast-forward typewriter |
| `Space` / any click | Skip typewriter to full line |

## Ink Tag Reference

Tags are written on the line above the text they apply to:

```ink
#speaker:Narrator
#save:checkpoint_one
#portrait:innkeeper_smile
The door creaks open.
```

| Tag | Effect |
|---|---|
| `#speaker:NAME` | Sets `DialogueLine.speaker`. UI shows `NAME:` prefix. |
| `#portrait:KEY` | Hints the UI which portrait sprite to show. Hook `ApplyPortraitTag` to your sprite atlas. |
| `#save[:SLOT]` | Fires `OnSaveTag`. `DialogueSaveSystem` writes a JSON save to the named slot (default `auto`). |
| `#event:NAME` | Generic tag. Subscribe to `OnLineDisplayed` and filter for `event:` tags to handle game-specific events. |

> **Audio tags removed.** The system previously supported `#audio:CLIP` and `#music:TRACK` tags through an `AudioHookSystem` component. That subsystem has been removed. If you need audio cues, subscribe to `DialogueEvents.OnLineDisplayed` and play clips based on the speaker or any custom tag you invent (e.g. `#sfx:door`).

## File Layout

```
InkDialogueSystem/
├── Scripts/
│   ├── Core/
│   │   ├── DialogueManager.cs        Singleton, wraps Ink.Runtime.Story
│   │   ├── DialogueEvents.cs         Static event bus
│   │   ├── InkFileLoader.cs          Resolves .ink.json from Resources or StreamingAssets
│   │   ├── DialogueSaveSystem.cs     JSON save/load to persistentDataPath
│   │   ├── DialogueDirector.cs       Scene glue + hotkey wiring
│   │   └── NPCInteractable.cs        3D world NPC interaction (click + proximity)
│   ├── UI/
│   │   ├── DialogueUIController.cs   Main dialogue panel controller
│   │   ├── DialogueHistoryPanel.cs   Scrollable history log
│   │   ├── VariableInspectorPanel.cs Debug variable display
│   │   └── ChoiceButton.cs           One choice row
│   ├── Effects/
│   │   └── TypewriterEffect.cs       Char-by-char reveal
│   ├── Data/
│   │   ├── DialogueLine.cs           POCO for one line of dialogue
│   │   ├── DialogueChoice.cs         POCO for one choice
│   │   └── SaveData.cs               JSON-serializable save slot
│   └── Editor/
│       └── InkDialogueSystemMenu.cs  Unity menu items
├── Ink/
│   └── sample_story.ink              Branching demo story
├── Diagrams/                         Architecture PNGs (also in design PDF)
└── Documentation/
    └── InkDialogueSystem_Design.pdf  Full design document
```

## 3D Game Setup

This system is designed to layer on top of a 3D scene. The dialogue UI uses a **Screen Space - Overlay Canvas**, which renders on top of your 3D world — no special camera setup needed. NPC interactions are handled by the `NPCInteractable.cs` script.

### Adding a clickable / approachable NPC

1. Import or model your NPC in the scene.
2. Add a **Collider** to the NPC (BoxCollider, CapsuleCollider, or MeshCollider). This is required for click raycasts to hit it.
3. Add the `NPCInteractable` component.
4. In the Inspector:
   - **Story Name**: e.g. `sample_story`
   - **Start Knot** (optional): a knot name to jump to, e.g. `stranger_intro` — leave empty to start from the top of the story
   - **Interaction Mode**: `Click` / `Proximity` / `Both`
   - **Dialogue UI Root**: drag your `DialoguePanel` GameObject here (it will be hidden until interaction starts, then hidden again when the story ends)

### Setting up proximity trigger

For `Proximity` or `Both` mode:

1. Add a child GameObject to the NPC named `InteractionZone`.
2. Add a Collider to it (BoxCollider or SphereCollider) and **check IsTrigger**.
3. Size it to define the interaction zone (e.g. a 2m sphere around the NPC).
4. The `NPCInteractable` script auto-detects this child by name. Alternatively, drag the trigger Collider into the `Proximity Trigger` field.
5. Make sure your player GameObject has the tag `Player` (or override `IsPlayer()` in the script).

### Interaction prompt UI (optional)

If `Require Key Press` is checked (default), the player must press `E` while inside the trigger zone. To show a "Press E to talk" prompt:

1. Create a small Text - TextMeshPro GameObject as a child of the NPC, positioned above the NPC's head.
2. Drag it into the `Interaction Prompt` field.
3. The script will activate it when the player enters the zone and deactivate it when they leave or start the dialogue.

### Player tag setup

The default `IsPlayer(Collider)` implementation checks for the `Player` tag. To set this up:

1. Select your player GameObject.
2. In the Inspector, at the top, change **Tag** from `Untagged` to `Player` (Unity has this built in).

For custom player detection (e.g. checking for a `PlayerController` component instead of a tag), subclass `NPCInteractable` and override `protected virtual bool IsPlayer(Collider other)`.

### Multiple NPCs, same story

You can place many `NPCInteractable` instances in the scene, each pointing to the same `storyName` but with different `startKnot` values. The `DialogueManager` will load the story once and use `Story.ChoosePathString(knot)` to jump to the right conversation.

### Disabling auto-start

If you don't want dialogue to auto-start when the scene loads (because you want NPCs to drive it), uncheck `Auto Start On Awake` on the `DialogueManager`. The manager will sit idle until an `NPCInteractable` calls `StartDialogue()`.

## Swapping in Your Own Story

Two options:

**Baked-in (recommended for shipping)** — drop your compiled `.json` into `Assets/Resources/Ink/`. Call `DialogueManager.StartStory("your_story_name")` (no extension).

**Hot-swappable (great for modders / live ops)** — drop the `.json` into `Assets/StreamingAssets/Ink/`. Same API. StreamingAssets are not bundled into the player on most platforms, so you can update stories without rebuilding the game.

The `InkFileLoader` tries Resources first, then StreamingAssets — so the same call works for either.

## Save File Location

Saves live at `{Application.persistentDataPath}/InkDialogueSaves/{slotName}.json`.

| Platform | Path |
|---|---|
| Windows | `%userprofile%\AppData\LocalLow\<company>\<product>\InkDialogueSaves\` |
| macOS | `~/Library/Application Support/<company>/<product>/InkDialogueSaves/` |
| Linux | `~/.config/unity3d/<company>/<product>/InkDialogueSaves/` |
| Android | `/storage/emulated/0/Android/data/<package>/files/InkDialogueSaves/` |
| iOS | `<Application>/Library/Caches/InkDialogueSaves/` |

Open the save folder from the editor with **Ink Dialogue System > Open Save Directory**.

## Extending the System

- **Custom tags**: Add a new tag prefix in `DialogueManager.ParseAndDispatchTags` and a new event in `DialogueEvents`. Subscribe anywhere.
- **External functions**: Bind C# functions to Ink with `DialogueManager.BindExternalFunction("MyFunc", args => ...)`. Useful for inventory checks, RNG, save-state queries.
- **Variable observation**: Call `story.ObserveVariables(...)` in `DialogueManager.BindExternalFunctions` to get pushed notifications when specific Ink variables change.
- **Custom save payloads**: Extend `SaveData.customPayloadJson` with your game's inventory/quest state.
- **Replace UI toolkit**: The core (`DialogueManager`, `DialogueEvents`, `Data/`) is toolkit-agnostic. Swap uGUI for UI Toolkit by replacing only the `UI/` folder.

## Dependencies

- Unity 2021.3 LTS or newer
- TextMeshPro (built-in, auto-imported)
- [ink-unity-integration](https://github.com/inkle/ink-unity-integration) (Inkle's official Unity package)

## License & Credits

Ink is © Inkle Ltd. — see [inklestudios.com/ink](https://www.inklestudios.com/ink/). The dialogue system code in this repo is yours to use, modify, and ship.

For full architecture details, API reference, and integration guide, see **`Documentation/InkDialogueSystem_Design.pdf`**.
