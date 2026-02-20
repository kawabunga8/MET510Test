# Step 3 — Unity MVP Setup (Case 01 Vertical Slice)

Goal: build **one playable 10–15 minute case** that demonstrates the full learning loop:
Briefing → Evidence → Decision(s) → Feedback → XP saved.

This repo now includes starter scripts for **data-driven cases** under `Assets/Scripts`.

---

## 1) Create folders (if Unity didn’t already)
In the Project window, ensure you have:
- `Assets/Cases/`
- `Assets/Scenes/`

---

## 2) Create the MVP scenes
Create 3 scenes (File → New Scene):
- `MainMenu.unity`
- `CaseSelect.unity`
- `Case01_DetectiveRoom.unity`

Save them into `Assets/Scenes/`.

---

## 3) Create Case 01 data (ScriptableObject)
1. Right click in `Assets/Cases/` → Create → **ETEC510 → Case Data**
2. Name it: `Case01_AIImageCredibility`
3. Fill in:
   - **CaseId:** `case01_ai_image`
   - **Title:** `Case 01: Is This Image Real?`
   - **BriefingText:** short detective briefing
   - Add 4–6 **Evidence** items (title + description; image optional)
   - Add 3–5 **Questions** (multiple choice + feedback)

Recommended question tags:
- ClaimVsEvidence
- CredibilityCheck
- BiasOrIntent
- WhoBenefits

---

## 4) Create simple UI (temporary is fine)
In `Case01_DetectiveRoom`:
- Add a Canvas + basic Text elements for briefing / evidence / question
- Add Buttons for answer options

MVP is allowed to be ugly. Clarity > polish.

---

## 5) Wire the logic (lightweight approach)
Next implementation step (not yet in repo):
- Create a `CaseRunner` MonoBehaviour that:
  - takes a `CaseData` reference
  - creates a `CaseSession`
  - updates UI for current question
  - handles button clicks → `session.Answer(i)`
  - on completion → `session.CompleteCase()`

---

## 6) Confirm persistence
After completing the case:
- XP should increment using `PlayerPrefs` (`etec510_xp`)
- Case completion flag should be set

---

## Definition of Done (Step 3)
- Case 01 can be played end-to-end
- Learner receives feedback after choices
- XP saves and persists between runs
- You can show this in a short screen recording
