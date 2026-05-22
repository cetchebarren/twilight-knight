# Twilight Knight — Unity Action RPG Prototype

Twilight Knight is a massive, systems‑driven action RPG prototype built in Unity using C#.
It explores the full range of modern RPG mechanics — character creation, combat, traversal, AI, progression, quests, dialogue, world interaction, saving/loading, and more.

Nearly every system in Twilight Knight is fully custom‑built, from gameplay architecture to UI and tooling. Only a few specialized utilities are external, such as the modified BlazeAI navigation framework and PolyFew for LOD generation.

This project serves as a scalable sandbox for gameplay architecture, tooling, and system design — and as my personal attempt to learn, practice, and implement as many modern game‑industry features as possible.

Project overview & showcase: https://cetchebarren.github.io/projects/twilight-knight.html

---

## 🧰 Tech Stack

- Unity (C#) — engine, gameplay systems, animation, physics  — *hundreds of custom scripts, tens of thousands of lines of code*
- ScriptableObjects — stats, items, skills, modifiers, loot tables  
- Blaze AI Framework — heavily modified for navigation, perception, and behavior states  
- Custom Unity Tools — editors & inspectors
- JSON Serialization — saving & loading
- Art Tools: Procreate, Photoshop, some Blender

---

## 🎮 Core Systems

### Character Creation
A flexible creator defining the player’s starting appearance.

### Character Controllers
A responsive third‑person controller supporting:

- Walking, running, sprinting, crouching, climbing  
- Dodging with invincibility frames  
- Jumping and falling states, airborne actions  
- Root‑motion and non‑root‑motion animations  

### Combat Mechanics
A complete real‑time combat system including:

- Light and heavy attacks, combo chains and unique contextual actions  
- Blocking, dodging stamina management  
- Physics driven hitbox & damage detection
- Magic system featuring status effects & elemental reactions
- Build variety via stats, and player ability and investment choices

### Enemy AI
Powered by Blaze AI with custom tuning and behaviors:

- Patrol, chase, investigate, search, and combat states  
- Vision perception, altered via stealth system
- Attack patterns and reaction animations  
- Debugging tools for behavior visualization
- Custom event driven script behaviors  

### Progression & Loot
A full RPG progression loop:

- XP, leveling, and stat allocation  
- 28+ unique skills (including over a dozen unique spells)  
- Randomized loot with modifier‑based dynamic naming  
- Weapons, armor, accessories, consumables  
- ScriptableObject‑driven item definitions  

### Inventory & Equipment
A modular equipment system:

- Gear slots (weapon, armors, accessory)  
- Stat modifiers applied on equip/unequip  
- Consumables and hotbar support  

### Quests & Dialogue
A lightweight narrative system:

- Quest states and chains (inactive, active, completed)  
- Rewards, triggers, and world interactions
- Dialogue system, affected by quest context 

### World Interaction
Interactable objects include:

- Chests, Doors, NPCs, Gathering nodes  
- Custom interaction-module based events

### Cutscenes
Sequences for:

- Story moments  
- Boss introductions  
- Environmental reveals  

### Maps & Fast Travel
Two map layers:

- Local Map — nearby points of interest, instant fast travel  
- World Map — regions (scenes), fast‑travel points  

### Saving & Loading
A robust persistence layer storing:

- Player stats  
- Inventory and equipment  
- Quest progress  
- World state  
- Scene transitions  

### UI / UX
A clean, game‑ready interface:

- Health, stamina, mana  
- Hotbar  
- Inventory and equipment screens  
- Skill tree  
- Quest log  
- Dialogue UI  

### Audio System
Includes:

- Footsteps, impacts, spells, UI sounds  
- Ambient audio zones  
- Combat cues and hit reactions  

### Scene Management
- Async loading  
- Transition screens  
- Persistent managers  

### Performance
- Object pooling  
- Culling  
- Lightweight data structures  
- Optimized AI perception  

---

## 📂 Unity Project Structure
```
Assets/
├── Scripts/
│   ├── Player/
│   ├── Combat/
│   ├── AI/
│   ├── Items/
│   ├── Skills/
│   ├── Quests/
│   ├── Dialogue/
│   ├── UI/
│   ├── Saving/
│   └── Utilities/
│
├── ScriptableObjects/
│   ├── Items/
│   ├── Skills/
│   ├── Stats/
│   └── LootTables/
│
├── Animations/
├── Prefabs/
├── Scenes/
├── Audio/
└── Materials/
```

---

## 🎯 Goals of This Project

- Explore the full breadth of RPG systems  
- Build scalable, modular gameplay architecture  
- Prototype combat, AI, and progression loops  
- Create a foundation for future expansions  
- Develop reusable Unity tools and patterns  

---

## 📬 Contact

- Portfolio: https://cetchebarren.github.io  
- GitHub: https://github.com/cetchebarren  
- LinkedIn: https://linkedin.com/in/chad-etchebarren/

---

## 📄 License

All content is © Chad Etchebarren.  
You may reference the structure or architecture, but please do not copy the project’s assets, content, or proprietary systems.
