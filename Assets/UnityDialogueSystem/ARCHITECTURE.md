# Architecture

## Overview

The system follows a **ScriptableObject-event** architecture (popularised by
Ryan Hipple's Unite 2017 talk). The core principle: **the runner never
references the view; the view subscribes to events the runner raises**.

This makes the system:
- **Decoupled** — swap the uGUI view for UI Toolkit without touching the runner.
- **Testable** — raise events from a test harness and assert on view state.
- **Composable** — multiple views (HUD subtitle bar + full dialogue panel) can
  both subscribe to the same line event.

## Component Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DATA / ASSETS                                  │
│                                                                             │
│  .ink file ──Ink ScriptedImporter──▶ InkFile (SO asset)                     │
│                                          │                                  │
│                                          │ .storyJson (compiled JSON)       │
│                                          ▼                                  │
│                                  DialogueStoryAsset (SO)                    │
│                                          │                                  │
│                                          │ .CreateStory()                   │
│                                          ▼                                  │
│                                  Ink.Runtime.Story                          │
└─────────────────────────────────────────────────────────────────────────────┘
                                                │
                                                │ owned by
                                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CORE LAYER                                     │
│                                                                             │
│                      ┌──────────────────────┐                               │
│                      │   DialogueRunner     │  ◀── Phase: Idle/Typing/      │
│                      │   (MonoBehaviour)    │       WaitingForContinue/     │
│                      └──────────┬───────────┘       WaitingForChoice/Ended │
│                                 │                                           │
│            ┌────────────────────┼────────────────────┐                     │
│            │                    │                    │                     │
│            ▼ raises             ▼ raises             ▼ raises              │
│   DialogueStarted       DialogueLine          DialogueChoices              │
│      GameEvent           GameEvent              GameEvent     (SO)         │
│            │                    │                    │                     │
│            │   event Action SkipRequested           │                     │
│            │   ◀─────────────┐                      │                     │
│            │                 │                      │                     │
│            │   NotifyTypewriterComplete()           │                     │
│            │   ◀─────────────┐                      │                     │
│            ▼                 │                      ▼                     │
│  DialogueEndedGameEvent      │             (consumed by view)             │
└──────────────────────────────┼─────────────────────────────────────────────┘
                               │
                ┌──────────────┴──────────────┐
                │                             │
                ▼                             ▼
┌───────────────────────────┐   ┌───────────────────────────────┐
│      INPUT LAYER          │   │           UI LAYER             │
│                           │   │                               │
│  DialogueInputPoller      │   │  DialogueUIView               │
│  (MonoBehaviour)          │   │  (MonoBehaviour)              │
│                           │   │                               │
│  Polls Keyboard.current,  │   │  Subscribes to SO events      │
│  Mouse.current in Update  │   │                               │
│                           │   │  owns:                        │
│  dispatches to runner:    │   │   ├─ TypewriterEffect         │
│   ├─ Phase.Typing         │   │   │   (Awaitable char reveal) │
│   │   → RequestSkip       │   │   ├─ ChoiceButton[ ]          │
│   ├─ Phase.WaitingFor...  │   │   │   (instantiated per choice)│
│   │   → RequestAdvance    │   │   ├─ SpeakerLabel (TMP)       │
│   └─ Phase.WaitingForChoice│  │   ├─ ContinuePrompt           │
│       → SelectChoice(i)   │   │   └─ Root panel               │
│                           │   │                               │
│                           │   │  subscribes to runner's       │
│                           │   │  SkipRequested C# event       │
│                           │   │  → calls TypewriterEffect     │
│                           │   │    .Complete()                │
└───────────────────────────┘   └───────────────────────────────┘
                                              │
                                              ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              SAVE LAYER                                     │
│                                                                             │
│  DialogueSaveLoad (static)                                                  │
│                                                                             │
│   Save(runner)      → runner.Story.state.ToJson()      → File.WriteAllText  │
│   LoadInto(runner)  → File.ReadAllText              → Story.state.LoadJson  │
│   SaveExists()      → File.Exists(persistentDataPath/dialogue_save.json)    │
│                                                                             │
│  After LoadInto, caller must invoke runner.RestartFromLoadedState() to      │
│  resume the pump from the saved position.                                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Sequence: One Full Line Cycle

```
 Runner                View                InputPoller        TypewriterEffect
   │                     │                     │                     │
   │ Begin()             │                     │                     │
   ├─▶ Story = asset.CreateStory()             │                     │
   ├─▶ raise onStarted   │                     │                     │
   │   ─────────────────▶│ HandleStarted       │                     │
   │                     │ root.SetActive(true)│                     │
   │                     │                     │                     │
   │ PumpAsync:          │                     │                     │
   │ text = Story.Continue()                   │                     │
   │ line = TagParser.Parse(text, tags)        │                     │
   │ Phase = Typing      │                     │                     │
   ├─▶ raise onLinePresented                   │                     │
   │   ─────────────────▶│ HandleLinePresented │                     │
   │                     │   await typewriter.Play(line.Text)        │
   │                     │   ───────────────────────────────────────▶│
   │                     │                     │                     │ reveal
   │                     │                     │                     │  chars
   │                     │                     │  Space pressed      │
   │                     │                     │  Phase == Typing    │
   │                     │                     │   ─┐                │
   │                     │                     │    │ RequestSkip    │
   │                     │                     │ ◀──┘                │
   │ ◀── RequestSkipTypewriter()               │                     │
   │   SkipRequested?.Invoke()                 │                     │
   │   ─────────────────▶│ HandleSkipRequested │                     │
   │                     │   typewriter.Complete()                   │
   │                     │   ───────────────────────────────────────▶│ finish
   │                     │   ◀── await Play() returns ──────────────┤
   │                     │   runner.NotifyTypewriterComplete()      │
   │ ◀── _typewriterDone = true                │                     │
   │                     │                     │                     │
   │ Phase = WaitingForContinue                │                     │
   │   (or WaitingForChoice if currentChoices.Count > 0)             │
   │                     │                     │                     │
   │                     │                     │  Space pressed      │
   │                     │                     │  Phase == Waiting   │
   │                     │                     │  ForContinue        │
   │                     │                     │   ─┐                │
   │                     │                     │    │ RequestAdvance │
   │                     │                     │ ◀──┘                │
   │ ◀── RequestAdvance()                      │                     │
   │   ContinueAsync() → PumpAsync() (next iteration)                │
   │   ⋮                                     │                     │
```

## State Machine

```
                ┌──────────┐  Begin()
                │   Idle   │────────────┐
                └──────────┘            │
                                          ▼
                ┌──────────┐  Story.Continue() returns text
                │  Typing  │◀─────────────────────────────┐
                └────┬─────┘                                │
                     │                                      │
              NotifyTypewriterComplete()                     │
                     │                                      │
                     ▼                                      │
            ┌──────────────┐                                 │
            │ currentChoices│                                │
            │  .Count > 0 ? │                                 │
            └──────┬───────┘                                 │
              yes  │  no                                     │
              ┌────┘  └────┐                                 │
              ▼             ▼                                │
   ┌─────────────────┐  ┌──────────────────┐                │
   │ WaitingForChoice│  │WaitingForContinue│                │
   └────────┬────────┘  └─────────┬────────┘                │
            │                     │                         │
   SelectChoice(i)         RequestAdvance()                  │
            │                     │                         │
            └─────────────────────┴─────────────────────────┘
                                          │
                                          ▼
                              ContinueAsync() → PumpAsync()
                                          │
                              (loops until !canContinue && !choices)
                                          │
                                          ▼
                                   ┌──────────┐
                                   │  Ended   │
                                   └──────────┘
```

## Class Responsibilities

| Class | Responsibility | Depends on |
|---|---|---|
| `DialogueStoryAsset` (SO) | Wraps an `InkFile` + fallback JSON; produces fresh `Story` instances | `Ink.Runtime`, `Ink.UnityIntegration` |
| `DialogueLine` (POCO) | Holds parsed text + tag fields | — |
| `DialogueTagParser` (static) | Parses `#key:value` tags into `DialogueLine` | `DialogueLine` |
| `DialogueRunner` (MB) | Owns the live `Story`; pumps lines; raises SO events; exposes input + save/load API | `DialogueStoryAsset`, the four SO events, `DialogueTagParser` |
| `DialogueStartedGameEvent` (SO) | Channel: runner → listeners (payload: `Story`) | `Ink.Runtime` |
| `DialogueLineGameEvent` (SO) | Channel: runner → listeners (payload: `DialogueLine`) | `DialogueLine` |
| `DialogueChoicesGameEvent` (SO) | Channel: runner → listeners (payload: `List<Choice>`) | `Ink.Runtime` |
| `DialogueEndedGameEvent` (SO) | Channel: runner → listeners (no payload) | — |
| `DialogueInputPoller` (MB) | Polls `Keyboard.current` / `Mouse.current`; dispatches to runner | `DialogueRunner`, `UnityEngine.InputSystem` |
| `DialogueUIView` (MB) | Subscribes to SO events; renders speaker/line/choices; drives typewriter; signals runner | `DialogueRunner`, the four SO events, `TypewriterEffect`, `ChoiceButton`, TMP |
| `TypewriterEffect` (MB) | Char-by-char reveal via `Awaitable`; supports `Complete()` for skip | TMP, Unity 6 Awaitable |
| `ChoiceButton` (MB) | Per-choice prefab; wires up label + click callback | TMP, uGUI Button |
| `DialogueSaveLoad` (static) | File-based save/load via `Story.state.ToJson` / `LoadJson` | `Ink.Runtime`, `DialogueRunner` |
| `DialogueDemo` (MB) | Bootstrap; F5/F9/F12 hotkeys | `DialogueRunner`, `DialogueSaveLoad` |

## Why These Choices

### ScriptableObject events over a singleton manager

A singleton `DialogueManager.Instance.ShowLine(...)` couples every caller to the singleton and makes it impossible to run two stories concurrently (e.g. an ambient NPC barker + a story-critical conversation). SO events scale: the same event asset can be raised by any runner, and any number of views can listen.

### Polling input over an InputActions asset

The user explicitly chose polling. The trade-off: no auto-rebinding UI, no control-scheme switching, but zero asset setup and full transparency — every binding is a `[SerializeField]` on the poller.

### Awaitable over coroutines / UniTask

The user explicitly chose Unity 6 `Awaitable`. Benefits over coroutines: proper `await` syntax, `CancellationToken` support, no `MonoBehaviour` coupling. Benefits over UniTask: no third-party dependency.

### Typewriter owned by the View, not the Runner

The runner only knows about text and "is the typewriter done yet" (a single bool toggled by `NotifyTypewriterComplete()`). This keeps the runner UI-agnostic — the same runner could drive a UI Toolkit view, a 3D world-space text mesh, or a headless test.

### Save/Load via `state.ToJson`, not full Story serialization

`Story.state.ToJson()` saves only the *runtime state* (current path, variables, visit counts). The compiled story JSON (the static narrative) is reloaded from the asset on `Begin()`, then state is layered on top via `LoadJson`. This is the recommended ink pattern and keeps save files tiny (~1–5 KB).

## Extension Points

| Want to... | Hook |
|---|---|
| Add a custom tag | Add a `case` in `DialogueTagParser.Parse`, or read `line.ExtraTags` from your own listener |
| React to ink variable changes | Subscribe to `DialogueStartedGameEvent`, call `story.ObserveVariable(...)` |
| Bind external C# functions | Subscribe to `DialogueStartedGameEvent`, call `story.BindExternalFunction(...)` |
| Add gamepad support | Either replace `DialogueInputPoller` with an InputActions-asset-driven component, or extend it to poll `Gamepad.current` |
| Multiple concurrent dialogues | Add multiple `DialogueRunner` instances, each with its own set of SO event assets |
| Localisation | Subscribe to `DialogueLineGameEvent`, run the line through your localisation lookup before passing to the typewriter |
| Voice-over | Read `line.Audio` from the line event, look up the clip, play through an AudioSource |
