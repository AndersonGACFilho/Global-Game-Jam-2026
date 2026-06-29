🎭 Vampires (Don't) Bite Me

Project originally created during Global Game Jam 2026, under the theme MASK.

«Status: Post-Jam Solo Remake — Work in Progress»

Originally developed as a 2D side-scroller survival/stealth prototype during the 48-hour Global Game Jam, the project is now being reimagined as a retro first-person stealth horror experience, inspired by classic Doom-like games while keeping the original theme centered around masks, deception and survival.

---

📌 Project Overview

Vampires (Don't) Bite Me is a retro-inspired first-person stealth horror game where the player must survive a masquerade ball that turns into a massacre.

Everyone attending the event wears masks.

The protagonist leaves the ballroom for a few minutes.

When they return...

The guests are being slaughtered.

The mansion has become a hunting ground.

The only way to survive is to deceive the vampires, hide your identity and escape before dawn.

The game focuses on:

- Stealth
- Exploration
- Survival
- Enemy perception
- Resource management
- Environmental storytelling
- Psychological tension

Unlike traditional retro FPS games, combat is not the main focus.

---

📌 Original Jam Information

- Global Game Jam 2026
- Theme: Mask
- Duration: 48 hours
- Engine: Unity
- Language: C#
- Location: Coletivo Centopeia

---

🎭 Theme Interpretation

The Global Game Jam theme was Mask.

Rather than treating masks as cosmetic objects, the project explores them as symbols of:

- Identity
- Survival
- Deception
- Appearance versus reality
- Social roles
- Hidden intentions
- Fear

Masks are both a gameplay mechanic and a narrative device.

They allow the player to temporarily become “one of them”.

---

🧛 Story

The story takes place inside an old mansion during an exclusive masquerade ball.

Guests arrive expecting an elegant celebration.

Music plays.

People dance.

Nobody suspects anything.

The protagonist briefly leaves the ballroom to use the bathroom.

Moments later...

The music stops.

Screams echo through the mansion.

When the protagonist returns, vampires have begun massacring the guests.

The player must explore the mansion, uncover what happened, avoid becoming the next victim and ultimately survive until sunrise or escape the estate alive.

---

🎮 Gameplay

The game is being redesigned around a first-person stealth experience.

Core gameplay loop:

1. Explore the mansion
2. Search for keys and resources
3. Find new masks
4. Blend in with vampires
5. Avoid suspicion
6. Hide when necessary
7. Unlock new areas
8. Escape or survive until dawn

---

🎭 Mask System

Masks are the central mechanic of the game.

Different masks allow the player to temporarily disguise themselves among vampires.

However:

- Masks deteriorate over time.
- Some vampires are harder to fool.
- Suspicious behavior increases detection.
- Broken masks immediately expose the player.

Choosing when to wear or preserve a mask becomes one of the main strategic decisions.

---

👁️ Enemy AI

The vampires are designed around perception, suspicion and planning rather than purely scripted encounters.

Planned AI systems include:

- Patrol behaviors
- Field of view detection
- Line-of-sight checks
- Suspicion system
- Investigation behavior
- Chase state
- Search state
- Memory of last known player position

Enemy behavior is intended to become one of the project's main technical showcases.

---

🧠 AI Architecture

One of the main technical goals of this project is to explore game AI beyond traditional finite state machines.

Enemy decision-making is planned around a hybrid AI architecture combining a macro-level Finite State Machine (FSM) with a custom Hierarchical Task Network (HTN) planner.

The FSM defines the enemy's current high-level mode and selects the corresponding HTN root task. The HTN then decomposes that root task into executable primitive actions based on the current world state.

Perception / Events
↓
Finite State Machine
↓
Select HTN Root Task
↓
HTN Planner
↓
Primitive Action Plan
↓
Plan Runner

Example state-to-root-task mapping:

VampireState.Social
→ Root Task: MaintainMasquerade

VampireState.Suspicious
→ Root Task: InvestigateSuspicion

VampireState.Hunting
→ Root Task: HuntHuman

VampireState.Chasing
→ Root Task: ChaseAndKill

VampireState.Returning
→ Root Task: ReturnToPost

The AI architecture is designed around:

- Finite State Machine — controls the vampire's macro behavior mode.
- HTN Planner — decomposes high-level goals into executable task plans.
- Primitive Actions — executable low-level behaviors.
- Perception System — field of view, line of sight and nearby awareness.
- World State / Blackboard — shared knowledge used by the planner.
- Memory System — stores last known player position and suspicious events.
- Navigation System — moves enemies through the mansion.
- Plan Runner — executes the generated primitive action sequence.

Example high-level behavior structure:

Maintain the Masquerade
│
├── Socialize
├── Patrol
├── Investigate Suspicious Activity
├── Search for Intruders
├── Hunt Human
│   ├── Detect Target
│   ├── Confirm Identity
│   ├── Chase
│   └── Kill
│
└── Return to Social Behavior

The player's mask directly influences the AI by modifying the world state.

For example:

- Wearing a valid mask reduces suspicion.
- Damaged masks increase suspicion.
- Broken masks expose the player.
- Running, entering restricted areas or staying too close to vampires may trigger questioning or pursuit.

This architecture aims to produce believable, reactive and scalable enemy behaviors while also serving as a technical showcase for AI planning techniques used in gameplay programming.

---

🗣️ Future AI / Dialogue Experiments

As a future experiment, the project may include an LLM-assisted interrogation system for moments when the player is wearing a mask and a vampire becomes suspicious at close range.

The LLM would not control gameplay-critical decisions.

Instead, it would be used as a narrative layer to generate contextual vampire dialogue, questions and social pressure.

Planned direction:

- LLM-generated vampire questions based on suspicion, location and player behavior.
- Deterministic suspicion scoring handled by the game systems.
- Fixed gameplay outcomes controlled by FSM, HTN, perception and world state.
- Dialogue choices that can increase or reduce suspicion.
- No LLM authority over death, detection, chase or win/loss conditions.

Example flow:

Vampire gets close
↓
Suspicion score is high
↓
Interrogation starts
↓
LLM generates contextual question
↓
Player chooses response
↓
Deterministic system updates suspicion
↓
AI resumes patrol, investigates further or starts chase

This feature is considered post-vertical-slice scope and may be explored after the core stealth, mask and HTN systems are playable.

---

🏰 Exploration

Players will gradually unlock the mansion while searching for:

- Keys
- Masks
- Documents
- Hidden passages
- Survival resources

The environment itself tells the story through exploration instead of long cutscenes.

---

🧩 Planned Features

Gameplay

- First-person movement
- Interaction system
- Inventory
- Mask durability
- Suspicion mechanic
- Hiding spots
- Locked doors
- Keys
- Environmental puzzles
- Final escape sequence

AI

- Macro FSM
- HTN planner
- Root task selection
- Primitive task/action execution
- Patrol
- Investigation
- Chase
- Search
- Perception
- Memory
- Blackboard/world state
- Plan runner

Environment

- Explorable mansion
- Multiple rooms
- Secret areas
- Interactive props
- Atmospheric lighting

---

🎨 Artistic Direction

The original 2D side-scroller has been replaced by a retro Doom-like / 2.5D presentation.

Visual direction:

- Retro FPS perspective
- Pixel-art inspired rendering
- Low-resolution presentation
- Billboard enemies
- Directional sprites
- Low-poly 3D environments
- Stylized lighting
- Atmospheric fog
- Retro horror aesthetic

The goal is to capture the feeling of classic 90s shooters while focusing on stealth rather than combat.

---

🖥️ Resolution and Visual Presentation

The project targets a fixed low-resolution rendering style to reinforce its retro identity.

Planned specifications:

- Internal resolution: 320×180
- Alternative resolution: 426×240
- Aspect ratio: 16:9
- Integer pixel scaling
- Pixel-perfect presentation
- Low-resolution rendering with modern lighting
- Designed for PC with potential WebGL support

The game window will scale to modern displays while preserving the original pixel density.

---

⚙️ Technical Direction

Current implementation focuses on modular gameplay systems.

Planned architecture:

- First Person Controller
- Player Interaction System
- Inventory System
- Mask System
- Vampire AI
- Macro State Machine
- HTN Planner
- Suspicion System
- Save System
- Dialogue Trigger System
- Environmental Events
- Scene Manager

Characters will primarily use billboard sprites with directional animation.

Large props and architecture will use simple low-poly 3D models to improve depth perception and reduce visual artifacts.

---

🛠️ Technologies

Engine

- Unity

Programming

- C#

Art

- Blender
- Aseprite

Audio

- FMOD
- BeepBox

Documentation

- Notion
- Trello
- Canva

---

🎮 Planned Controls

Action| Key
Move| WASD
Look Around| Mouse
Interact| E
Use / Change Mask| Q
Sprint| Left Shift
Crouch| Left Ctrl
Pause| Esc

---

👥 Credits

Original Global Game Jam Team

- Anderson Gonçalves — Programming & Game Design
- Ana Paula — Art & Design
- João Pedro — Art & Design
- Eduarda — Art & Design
- Dayane Noleto — Art & Design

During the game jam, Anderson was solely responsible for all programming and gameplay implementation.

---

👤 Current Development

Since the end of Global Game Jam 2026, the project has become an independent solo remake.

Current responsibilities:

- Programming
- Gameplay
- Game Design
- Technical Design
- AI Systems
- Architecture
- Implementation

by Anderson Gonçalves

---

📈 Current Status

Current development stage:

- Original jam prototype preserved
- Gameplay redesign in progress
- Artistic direction redefined
- AI systems under development
- Environment redesign
- First playable prototype in progress

---

📸 Media

The repository will be updated with:

- Gameplay screenshots
- Development GIFs
- AI demonstrations
- Prototype videos
- Future playable builds

---

🚀 Running the Project

Clone the repository:

git clone https://github.com/AndersonGACFilho/Vampires--Don-t--bite-me.git

Open the project using Unity Hub.

Select the correct Unity version.

Open the main scene.

Press Play.

---

🔗 Project Links

Game Design Document

https://www.notion.so/Modelo-Game-Design-Document-GDD-2f3e7aeb5ebf802e9f67de17ad16b96d

Moodboard

https://www.canva.com/design/DAG_c6LGVcg/xwptMcac3JszZ9BqyNh9vg/edit

Trello

https://trello.com/invite/b/69767438a75ab1358604a2ac/ATTI3a92b1e4424a68ca56f1d045ba90d96cE1C3F458/global-game-jam-mascaras

Development Notes

https://docs.google.com/document/d/1gSsfs96pag5IdlHf_3TUngOn9RLWpy8zHs_kwGrAUrE

---

🔮 Roadmap

Planned milestones:

- Playable mansion prototype
- Mask system
- Suspicion system
- Vampire AI
- Macro FSM
- HTN planner
- Root task selection
- Inventory
- Exploration mechanics
- Environmental storytelling
- Final chase sequence
- Public demo
- Future LLM-assisted interrogation experiment
- Itch.io release candidate

---

❤️ Acknowledgements

Special thanks to:

- Global Game Jam
- Coletivo Centopeia
- The original jam team
- Everyone who participated in GGJ 2026

Without the original jam, this project would never have existed.

---

📜 License

Originally created during Global Game Jam 2026.

The repository now serves as the official home of the ongoing solo remake.

This project is intended for educational, experimental and portfolio purposes.