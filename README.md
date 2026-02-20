# Critical Thinking for High-Schoolers (ETEC 510) — Unity Prototype

This repository contains a **Unity prototype** for an ETEC 510 design project: **Critical Thinking for High-Schoolers**.

The concept is a **gamified “detective toolkit”** for Grade 10 learners, designed to help students practice critical thinking and media literacy skills in a safe, engaging environment.

Repo status: **early prototype / active development**.

---

## Project idea (1 minute overview)
Students play as a **Lead Detective** working through short case files:
- they receive a **briefing** (video/voice)
- review **evidence** (images, clips, posts, quotes)
- use a **toolkit** to evaluate credibility/bias and make choices
- receive **immediate feedback** (safe mistakes) + **XP/badges**

The design aims to support:
- **reasoning** (evaluate claims and evidence)
- **reflection** (learn from mistakes, explain thinking)
- **agency** (choose actions, branch paths, redemption mechanics)

---

## Target audience and context
- **Learners:** Grade 10 students
- **Course context:** Career Studies (or similar advisory / digital citizenship integration)
- **Learning focus:** critical thinking + credibility checks + “who benefits?” analysis

---

## Learning goals (prototype)
By the end of a short case, learners should be able to:
- separate **claim vs. evidence**
- identify **bias / intent** and consider who benefits
- apply basic **credibility checks** (source, date, manipulation)
- make a decision and explain **why**

---

## Tech stack
- **Unity:** (set this once you decide your editor version; see below)
- **Platform:** desktop-first (school laptops)

---

## How to run locally
### 1) Get the code
- Clone this repo, or fork + clone.

### 2) Open in Unity Hub
In Unity Hub:
- **Add → Add project from disk**
- choose the folder that contains `Assets/`, `Packages/`, `ProjectSettings/`

### 3) Unity editor version
Use the Unity version listed in `ProjectSettings/ProjectVersion.txt`.

Current project version:
- **Unity 6000.3.3f1**

---

## MVP roadmap (what we’re building first)
**MVP goal:** one playable 10–15 minute case demonstrating the full learning loop.

Planned scenes:
- `MainMenu`
- `CaseSelect`
- `Case01_DetectiveRoom`

Core systems (MVP):
- Case content (data-driven: ScriptableObject/JSON)
- Evidence viewer
- Toolkit actions (evaluate, verify, decide)
- Feedback + XP/badges
- Short reflection prompt

---

## Suggested student/teammate contributions (later)
- additional cases (new evidence sets)
- UI/UX polish (clearer feedback, accessibility)
- audio/voice briefing clips
- better scaffolding (sentence starters for reflection)

---

## Project documentation
- ETEC 510 Phase 1 Proposal: (link or add a `/docs` folder later)

---

## License
TBD (add when you’re ready to share/reuse). If this is for course work only, keeping it unlicensed is fine.
