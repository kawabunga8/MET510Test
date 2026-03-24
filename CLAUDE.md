# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**MET510 Digital Detective Toolkit** is a Unity 6 educational game prototype for ETEC 510. It teaches Grade 10 learners critical thinking and media literacy through gamified detective-style case investigations.

- **Unity version**: 6000.3.9f1
- **Rendering**: Universal Render Pipeline (URP) 17.3.0
- **UI**: TextMesh Pro (TMP)
- **Persistence**: PlayerPrefs

## Running the Project

There is no CLI build. Open in Unity Hub:
1. Add project from disk (select the repo root containing `Assets/`, `Packages/`, `ProjectSettings/`)
2. Use Unity 6000.3.9f1
3. Open `Assets/Scenes/Case01_DetectiveRoom.unity` and press Play to test the MVP case flow

## Architecture

The project uses a strict three-layer data-driven architecture:

```
CaseData (ScriptableObject)  →  CaseSession (runtime state)  →  CaseRunner (TMP UI)
         Cases/                       Runtime/                        UI/
```

### Layer Details

**`CaseData` (`Assets/Scripts/Cases/CaseData.cs`)** — ScriptableObject defining a case.
- Contains `EvidenceItem[]` (image, source, date metadata) and `ChoiceQuestion[]`
- Each `ChoiceQuestion` has `CorrectIndex`, per-choice feedback, and a critical thinking tag (`ClaimVsEvidence`, `CredibilityCheck`, `BiasOrIntent`, `WhoBenefits`)
- Create via Unity Editor menu: `ETEC510 > Case Data`

**`CaseSession` (`Assets/Scripts/Runtime/CaseSession.cs`)** — stateful wrapper over a `CaseData` asset.
- `Answer(int selectedIndex)` → returns `(isCorrect, feedbackString)` and advances `CurrentQuestionIndex`
- `CompleteCase()` → awards XP and calls `ProgressStore.MarkCaseCompleted()`
- Tracks `CorrectCount` and `IsComplete`

**`ProgressStore` (`Assets/Scripts/Runtime/ProgressStore.cs`)** — static persistence utility.
- PlayerPrefs keys: `etec510_xp` (int), `etec510_case_completed_{caseId}` (int 0/1)
- Methods: `GetXp()`, `AddXp(int)`, `IsCaseCompleted(string)`, `MarkCaseCompleted(string)`

**`CaseRunner` (`Assets/Scripts/UI/CaseRunner.cs`)** — MonoBehaviour that wires TMP UI to a `CaseSession`.
- Assign a `CaseData` asset in the Inspector; it creates a `CaseSession` on `Start()`
- Flow: briefing → question loop (option buttons → feedback → Next) → completion screen with XP

## Content

- Case assets live in `Assets/Cases/` as `.asset` files
- Case 01 (`Case01_AIImageCredibility`) is the only case; its `Evidence` array is currently empty
- Scenes are in `Assets/Scenes/`; only `Case01_DetectiveRoom.unity` is wired to `CaseRunner`
- Planned but not yet created: `MainMenu` and `CaseSelect` scenes

## Testing

`com.unity.test-framework` (1.6.0) is in the manifest but no tests exist yet. When adding tests, `CaseSession` and `ProgressStore` are the primary candidates for EditMode unit tests (no MonoBehaviour dependency).
