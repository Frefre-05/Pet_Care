# Pixel-Care

Pixel-Care is a Mario-inspired game where players complete levels, collect Gold Coins, and use what they earn to take care of their pet. Between levels, players can feed, bathe, and rest their pet so it stays healthy enough for the next challenge. Pixie AI travels with the player as an in-game assistant, helping with navigation, pet-care choices, spending decisions, and recommendations on what to do next.

| Engine | Platforms | Team Members |
| --- | --- | --- |
| Unity 6000.2.5f1 | PC / Mac (editor-ready) | Frederik Sanchez, Francisco Caldas, Marcos Domenech |

## Table of Contents

1. [Project Story](#project-story)
2. [Core Gameplay Loop](#core-gameplay-loop)
3. [Level Progression](#level-progression)
4. [Gold Coin Economy](#gold-coin-economy)
5. [Pet Care System](#pet-care-system)
6. [Pixie AI Assistant](#pixie-ai-assistant)
7. [Controls](#controls)
8. [Saving Progress](#saving-progress)
9. [Getting Started](#getting-started)
10. [Building the Game](#building-the-game)
11. [Credits](#credits)

## Project Story

> The team set out to answer a simple question: *How can a game help players practice taking care of themselves while learning how to spend money in a smarter way?*

Over a semester, the team built:

- A home hub where players care for their pet's needs.
- A level system where Gold Coins help unlock harder challenges.
- Pixie AI, a guide that helps players make smarter navigation, care, and spending decisions.

Along the way, the team learned C#, Animator workflows, Unity UI, and local AI integration using Ollama and Llama 3.2:3B.

## Core Gameplay Loop

1. **Adventure:** Run, jump, dodge hazards, and collect Gold Coins throughout each platforming level.
2. **Cash In:** Gold Coins go into a shared currency bank that carries between scenes. Reaching the end of a level unlocks the next stage.
3. **Care for the Pet:** Back at the House scene, the pet's needs slowly decay. Use beds, baths, food, and potions to help recover those stats.
4. **Ask Pixie:** Press `G` to open Pixie AI for help with controls, navigation, locked levels, shop costs, chores, reports, and what to do next.
5. **Repeat:** When the needs are healthy and enough Gold Coins are banked, pay the entry cost for the next level and do it all again.

## Level Progression

Players move through platforming levels that challenge them with hazards, traps, checkpoints, and finish points. Completing a level unlocks the next stage, so progress feels tied to both skill and preparation. The player is encouraged to collect Gold Coins during each run because those coins help pay for future level access and pet-care needs back at home.

## Gold Coin Economy

Gold Coins connect the platforming side of the game to the pet-care side. Players earn coins by exploring levels and collecting pickups, then spend them on food, potions, shop items, and level progression costs. This makes each spending choice matter: the player has to decide whether to save for the next level or use coins now to keep the pet healthy.

## Pet Care System

The pet has needs that the player must manage between levels, including hunger, energy, hygiene, happiness, and health. Players can feed, bathe, rest, and buy care items for the pet so it stays in good condition. Keeping the pet okay is part of the main loop, because a healthy pet makes it easier to continue progressing through the game.

## Pixie AI Assistant

Pixie AI is the player's built-in guide for Pixel Care. Players can press `G` to ask Pixie about navigation, locked levels, pet needs, spending choices, reports, or what to do next. Pixie uses the current game state to give recommendations, and it can optionally connect to local Ollama with Llama 3.2:3B while still keeping a rule-based fallback available.

## Controls

| Action | Default Input | Notes |
| --- | --- | --- |
| Move | `A/D` or Arrow Keys | Move through levels |
| Jump | `Space` | Jump or double jump |
| Interact | `E` | Beds, baths, shop prompts, NPCs |
| Open Pixie AI | `G` | Opens the in-game assistant/chat window |
| Close Pixie AI | `Esc` | Closes the assistant/chat window |
| Safe save and quit | Type `/saveandquit` in Pixie AI | Saves progress before quitting |
| Navigate Menus | Mouse / Touch | Use buttons and menus |

## Saving Progress

Pixel-Care saves important player progress, including Gold Coins, pet needs, unlocked levels, selected pet, and Pixie history. This lets players move between scenes without losing their progress or the choices they already made.

## Getting Started

1. Install **Unity 6000.2.5f1** (or newer 6.0 minor; earlier 2021/2022 releases will not load the project correctly).
2. Clone this repository, then open the project folder in Unity Hub.
3. Open the `MainMenu` scene to experience the full flow, or jump into `House`, `Tutorial`, or `Level1` for focused testing.
4. Press Play in the editor. Use the Scene view + inspector to wire any missing references (coin text fields, shop buttons, Pixie chat references, etc.).

## Building the game

Set the Build Settings scene order to match the flow (MainMenu -> Tutorial -> House -> Level1 ...). Once set, build for Windows/macOS with the default player options. No custom scripting symbols are required.

Pixie AI does not require an internet connection. Optional local AI advice expects Ollama at `http://localhost:11434/api/generate`; otherwise Pixie's built-in deterministic advice and reports still work.

## Credits

Pixel-Care was built by Frederik Sanchez, Francisco Caldas, and Marcos Domenech.

Created with Unity, C#, Ollama, and Llama 3.2:3B for Pixie AI.

Copyright (c) 2026 Frederik Sanchez, Francisco Caldas, and Marcos Domenech. All rights reserved.
