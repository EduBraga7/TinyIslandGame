# AGENTS.md — System Directives for AI Agents

> **Context:** Unity 3D / C# project (*Tiny Island*).  
> **Target Audience:** AI Pair Programmers, Autonomous Agents, and LLM Code Generators.  
> **Core Objective:** Deliver high-feel gameplay with minimum viable code complexity.

---

## 1. Anti-Overengineering & Anti-Abstraction Directives

### 1.1 Direct & Concrete by Default
- **Favor Simplicity:** Write direct, self-contained `MonoBehaviour` scripts. Avoid premature design patterns (e.g., enterprise DI, generic factories, multi-layered interfaces, reflection-based event buses, decoupled state machine frameworks) for simple or single-instance systems.
- **Rule of Three:** Do not extract an abstraction, generic base class, or framework layer unless there are at least **three distinct, concrete use cases** actively present in the codebase today.
- **YAGNI (You Aren't Gonna Need It):** Implement strictly what is required for the immediate task. No speculative helper functions, unused parameter overrides, or boilerplate for hypothetical future expansions.

### 1.2 The Roadmap Mapping Gate (Strict Abstraction Protocol)
A more abstract, decoupled, or architecturally complex system **MAY ONLY BE IMPLEMENTED** if it is **explicitly mapped to a defined future development phase** in `ROADMAP.md` or `GDD.md`.

#### Enforcement Algorithm:
```
IF requested_system requires abstraction/indirection:
    CHECK ROADMAP.md / GDD.md for explicit milestone dependency.
    IF explicitly mapped to an upcoming milestone (e.g., v1.2, v1.3):
        ALLOW minimal viable abstraction. Document mapped milestone in code comments.
    ELSE:
        REJECT abstraction. Implement the most direct, concrete, minimal solution.
```

---

## 2. Requirement Interpretation: Experience First

### 2.1 Intent Over Literal Mechanics
- **Experience > Raw Implementation:** Do not take technical mechanics described by the user as rigid architectural blueprints.
- **Focus on the Desired Player Experience:**
  - *What should the player feel?* (e.g., responsiveness, coziness, momentum, weight, joy of discovery).
  - *What gameplay problem is being addressed?*
  - *What feedback loops make this interaction satisfying?*
- Deliver the intended gameplay feel using the simplest mechanical solution possible, even if the user casually requested a complex subsystem.

---

## 3. Game Design & Mechanic Evaluation

### 3.1 Objective Mechanics (No Subjective Bias)
- **Do not make arbitrary value judgments:** Never decide in isolation that a game mechanic is inherently "good", "bad", "boring", or "annoying" based on generic AI bias.
- Mechanics work through context: tuning, feedback, input responsiveness, and genre expectations.

### 3.2 Mandatory Precedent Research
- When in doubt regarding game feel, mechanical balance, control responsiveness, or interaction viability: **always search and analyze established precedents**.

### 3.3 Mechanical Thematic Relevance Rule
- **Priority:** Precedents MUST be chosen based on **MECHANICAL THEME / GAMEPLAY LOOP**, NOT narrative, setting, or visual aesthetics.
- **Rule:** Prioritize games sharing the exact control scheme, camera perspective, spatial scale, and interaction mechanics.

| Mechanical Target | Valid Precedent (Mechanical Fit) | Invalid Precedent (Superficial/Visual Only) |
| :--- | :--- | :--- |
| **Isometric / Fixed-Camera Movement & Platforming** | *A Short Hike*, *Captain Toad*, *Super Mario 3D World*, *Haven Park* | *Sea of Thieves*, *Monkey Island* (Island setting, zero platforming relevance) |
| **Micro Collect-a-thon & Progression** | *Tinykin*, *A Short Hike*, *Lego Star Wars* (coin drops) | *Far Cry* (Outpost collection, wrong pace/feel) |
| **Tactile Object Interaction / Chests** | *The Legend of Zelda* (chest fanfares), *Luigi's Mansion* | *Pirates of the Caribbean* (Theme match, mechanical mismatch) |

---

## 4. Code Standards & Fast Verification for Unity / C#

- **Inspector Tunability:** Expose physics and feel constants via `[SerializeField] private float` with sensible tooltips and ranges (`[Range(min, max)]`) so parameters can be tuned without recompiling code.
- **Direct Unity APIs:** Prefer standard Unity tools (`Invoke`, Coroutines, standard C# `System.Action` / `UnityEvent`, simple distance checks) over heavy external dependencies.
- **Preserve Documentation & Context:** Maintain existing comments and project structure. Never introduce regressions to existing functional loops.
