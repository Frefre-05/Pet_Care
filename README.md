# Pet-Care

Pet-Care is a 2D platformer / virtual pet mash-up built in Unity 6000.2.5f1. Sprint through Mario-inspired obstacle courses, gather apples, then head back home to feed, bathe, and rest your pet so it can tackle another run. The codebase was created by three 9th graders, so this readme keeps both their proud-kid energy and professional-level documentation to help you explore or extend their work.

| Engine | Platforms | Team Vibe |
| --- | --- | --- |
| Unity 6000.2.5f1 | PC / Mac (editor-ready) | Three 9th graders learning Unity & C# |

---

## Table of Contents

1. [Project Story](#project-story)
2. [Core Gameplay Loop](#core-gameplay-loop)
3. [Feature Highlights](#feature-highlights)
4. [Controls](#controls)
5. [Repository Layout](#repository-layout)
6. [Persistence & Data Model](#persistence--data-model)
7. [Getting Started](#getting-started)
8. [Extending the Project](#extending-the-project)
9. [Credits](#credits)

---

## Project Story

> The team set out to answer a simple question: *what if Mario collected coins to pay for his pet’s dinner?* Over a semester they prototyped a home hub with needs meters, a level ladder that spends currency to unlock harder worlds, and a basic shop run by an NPC. Along the way they taught themselves C#, Animator workflows, and Unity UI. The result is scrappy in places, but the systems are thoughtfully stitched together and everything persists through PlayerPrefs.

---

## Core Gameplay Loop

1. **Adventure:** Use the `PlayerMovement`/`JumpTwice` controllers to run through a level, dodging `MovingTrap`, `TrapDamage`, and `DeathZone` hazards while collecting apples (`AppleCollect`, `Fruit`).
2. **Cash In:** Apples go into a shared currency bank (`AppleCurrency`, `GameManager`) that carries between scenes. Hitting an `EndPoint` unlocks the next stage through `LevelProgress`.
3. **Care for the Pet:** Back at the House scene, five stats (`PetNeeds`) slowly decay. Use the bed (`BedSleepInteractor`), shower (`BathInteractor`), or the `ShopNPC` to buy food or potions (`ShopButtons`, `LevelPurchaseButton`) to recover those stats.
4. **Repeat:** When the needs are healthy and enough apples are banked, pay the entry cost for the next level and do it all again.

---

## Feature Highlights

<details>
<summary><strong>Pet-care simulation</strong></summary>

- `PetNeeds` tracks Hunger, Energy, Hygiene, Happiness, and Health with configurable decay, minibars on `NeedsUI`, and on-screen color feedback from `PetColorChanger`.
- Bed and bath interactors listen for `KeyCode.E` to play simple “sleep/shower” loops before topping the relevant stat back to 100.
- `HeartUI` / `HealthManager` show classic hearts for player lives, while `PlayerHealth` respawns at checkpoints and reloads scenes when hearts run out.
</details>

<details>
<summary><strong>Apple economy & shop</strong></summary>

- Apples are saved under the `APPLE_COUNT` key and mirrored across `AppleCurrency`, `AppleCounter`, `AppleHUD`, and `GameManager`.
- Shop buttons check affordability before granting potions or food, and levels can be sold behind gates using `LevelCostButton` or `LevelPurchaseButton`.
- `PlayerGrowth` and `PetScaleSaver` let apples double as growth milestones, increasing the avatar size every 20 pickups.
</details>

<details>
<summary><strong>Platforming foundation</strong></summary>

- Movement relies on Rigidbody2D physics with optional extras such as double jump (`JumpTwice`), wall sliding (`WallSlide`), and checkpoint respawns (`CheckPoint`, `RespawnOnFall`, `DamageOnFall`).
- Hazards include moving saws (`MovingTrap`), instant-kill pits (`DeathZone`), and manual trap colliders (`TrapDamage`).
- `LevelProgress`, `LevelGateButton`, and `EndPoint` scripts unlock levels in order and subtract the apple “entry fee” when a level is finished.
</details>

<details>
<summary><strong>UI, tutorial, and audio polish</strong></summary>

- `CharacterSelection2` and `CharacterSpawner` handle pet choice, remembering the player’s selection and name for later scenes.
- `TutorialManager` pauses the game at the start of the tutorial and after the first apple to keep new players on track.
- `MenuRegistrar`, `CloseMenuButton`, and `CloseThisPanel` ensure modal menus fade/dim correctly, while `BlackoutFaders` offers smooth scene fades.
- `AudioManager` keeps looping music and plays SFX hooks (jump, checkpoint, etc.), and `HoverTooltip` provides contextual help in the UI.
</details>

---

## Controls

| Action | Default Input | Notes |
| --- | --- | --- |
| Move | `A/D` or Arrow Keys | Feeds `PlayerMovement` / `JumpTwice` |
| Jump | `Space` | Uses Unity “Jump” input; `extraJumps > 0` enables double jump |
| Interact | `E` | Beds, baths, shop prompts, NPCs |
| Navigate Menus | Mouse / Touch | Unity UI buttons, sliders, dropdowns |

---

## Repository Layout

| Path | Description |
| --- | --- |
| `Assets/` | Runtime scripts, prefabs, scenes (drag-and-drop friendly layout). |
| `Packages/` | Unity package manifest (auto-managed). |
| `ProjectSettings/` | Unity project settings; `ProjectVersion.txt` pins the editor to 6000.2.5f1. |

---

## Persistence & Data Model

| Area | Keys / Scripts | Purpose |
| --- | --- | --- |
| Currency & fruits | `GameManager`, `AppleCurrency`, `SaveData`, PlayerPrefs `APPLE_COUNT`, `COLLECTED_FRUITS` | Stores apple totals and CSV fruit IDs so apples stay collected until reset. |
| Pet stats | `PetNeeds`, PlayerPrefs `PET_*` | Saves Hunger/Energy/Hygiene/Happiness/Health on disable to persist between scenes. |
| Progression | `LevelProgress`, `LevelGateButton`, `MAX_UNLOCKED` | Unlocks levels and greys out gates/buttons you cannot afford. |
| Player settings | `CharacterSelection2`, `CharacterSpawner`, `SelectedCharacter`, `PlayerName` | Remembers which pet prefab and name the player picked. |

Because everything uses PlayerPrefs, deleting those keys (or calling the provided reset tools) is enough to wipe the save during testing.

---

## Getting Started

1. Install **Unity 6000.2.5f1** (or newer 6.0 minor; earlier 2021/2022 releases will not load the project correctly).
2. Clone this repository, then open the project folder in Unity Hub.
3. Open the `MainMenu` scene to experience the full flow, or jump into `House`, `Tutorial`, or `Level1` for focused testing.
4. Press Play in the editor. Use the Scene view + inspector to wire any missing references (apple text fields, shop buttons, etc.).

### Building the game

Set the Build Settings scene order to match the flow (MainMenu →  Tutorial → House → Level1 …). Once set, build for Windows/macOS with the default player options. No custom scripting symbols are required.

---

## Extending the Project

- **Add a level:** Duplicate an existing platform scene, keep using `AppleCollect` prefabs (ensure each fruit has a unique ID), and point its `EndPoint` to the next scene plus its `thisLevelIndex` so `LevelProgress` stays in sync.
- **Create new shop items:** Copy a button, adjust its cost, and add a method to `ShopButtons` or a new script that calls into `PetNeeds` or `AppleCurrency`.
- **New needs or buffs:** Expand `PetNeeds` with additional stats, fire `OnNeedsChanged`, then expose new sliders on `NeedsUI`.
- **Better saving:** Swap PlayerPrefs calls for a JSON save if you need more robust persistence; hooks already exist in `SaveData`.

---

## Credits

Built with curiosity and late-night calls by three 9th graders who wanted their virtual pet to literally earn its meals. Thanks for checking it out, and feel free to keep iterating on their foundation!