# Rogue Wave — Architecture Reference

> **Repository state:** this document describes the codebase as of commit `0bf61611` (dev branch).
> **Scope:** all first-party code — `Assets/_Rogue Wave` (Runtime + Editor), `Assets/Wizards Code/Game State`, `Assets/Wizards Code/Story Teller`, `Assets/Wizards Code/Command Terminal`, `Assets/_Dev`, `Assets/_Marketing` — plus the *roles* of third-party frameworks (NeoFPS, Ink, Steamworks.NET, Feel, etc.). Third-party internals are only described from what is visible in the repo.
> **Method:** static code analysis (no Unity editor available). Runtime wiring that is only visible in scenes/prefabs (not in code) is labelled *inferred* where relevant.

---

## Table of Contents

1. [Project Overview & Tech Stack](#1-project-overview--tech-stack)
2. [High-Level Architecture](#2-high-level-architecture)
3. [Assembly / Module Dependency Map](#3-assembly--module-dependency-map)
4. [Scene & Game Flow](#4-scene--game-flow)
5. [Per-System Analysis](#5-per-system-analysis)
   - 5.1 [Roguelite Meta-Progression (Recipes, Run & Persistent Data, Campaigns, Nanobots)](#51-roguelite-meta-progression)
   - 5.2 [Procedural Level Generation (WFC)](#52-procedural-level-generation-wfc)
   - 5.3 [AI Director & Enemy System](#53-ai-director--enemy-system)
   - 5.4 [Combat & Weapons](#54-combat--weapons)
   - 5.5 [Game Stats, Achievements & Telemetry](#55-game-stats-achievements--telemetry)
   - 5.6 [Story Teller (Ink Narrative)](#56-story-teller-ink-narrative)
   - 5.7 [Save / Profile System](#57-save--profile-system)
   - 5.8 [UI / HUD](#58-ui--hud)
   - 5.9 [Audio](#59-audio)
   - 5.10 [Dev Tools, Editor Tooling & Showcases](#510-dev-tools-editor-tooling--showcases)
6. [Key Architectural Patterns](#6-key-architectural-patterns)
7. [Third-Party Dependencies & Their Role](#7-third-party-dependencies--their-role)
8. [Observations, Risks & Recommendations](#8-observations-risks--recommendations)
9. [Further Reading / File Index](#9-further-reading--file-index)

---

## 1. Project Overview & Tech Stack

**Rogue Wave** is a first-person roguelike wave-shooter prototype developed in Unity. The player fights procedurally generated combat arenas, gathers resources (collected by "nanobots"), and spends them between runs on permanent and temporary upgrades (the "recipe" system). A campaign system sequences levels, an AI director modulates difficulty, and an Ink-driven story layer (currently a tutorial) wraps progression.

| Aspect | Value |
|---|---|
| Engine | Unity **2022.3.52f1 LTS** (`ProjectSettings/ProjectVersion.txt`) |
| Render pipeline | URP (`com.unity.render-pipelines.universal` / shadergraph in `Packages/manifest.json`) |
| Language | C#, organised with Assembly Definitions (asmdefs) |
| First-party C# | ~214 files / ~27.4k lines in `Assets/_Rogue Wave` + `Assets/Wizards Code` (≈260 files including `_Dev`/`_Marketing`) |
| Git | 940+ commits on `dev`; third-party content via submodules (ink-unity-integration, UnityActionHub, unity-discord) |
| Build tooling | SuperUnityBuild (`_Rogue Wave/Development|Release SuperUnityBuildSettings.asset`) |
| Distribution | itch.io (prototype), Steam target (Steamworks integration present, gated by defines) |

### First-party code layout

| Path | Purpose |
|---|---|
| `Assets/_Rogue Wave/Runtime/` | The game: game mode, roguelite systems, level generation, enemies, weapons, UI, audio |
| `Assets/_Rogue Wave/Editor/` | Editor windows and validation tools |
| `Assets/_Rogue Wave/Resources/` | All authored content (ScriptableObjects): recipes, levels, achievements, stats, scenarios, audio, prefabs |
| `Assets/_Rogue Wave/Scenes/` | The 8 shipped scenes (main menu → combat → hub → portal…) |
| `Assets/Wizards Code/Game State/` | Reusable game-state framework: stats, achievements, events, Steamworks, telemetry |
| `Assets/Wizards Code/Story Teller/` | Reusable Ink narrative framework + bundled ink-unity-integration submodule |
| `Assets/Wizards Code/Command Terminal/` | Reusable in-game command-line console framework |
| `Assets/_Dev/` | Dev-only scenes, dev command assembly, level-wave generator window |
| `Assets/_Marketing/` | Marketing scenes, showcase capture tooling, GIF/recording descriptors |

The game is **data-driven**: nearly all content (recipes, levels, tiles, waves, stats, achievements, scenarios, story) is authored as `ScriptableObject` assets under `Resources/` and loaded at runtime (see §5.1–5.5).

---

## 2. High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        NeoFPS (third-party FPS framework)               │
│  FpsSoloGameCustomisable · FpsSoloPlayerController · FpsSoloCharacter   │
│  NeoSceneManager · NeoSerializedGameObject · modular firearms ·         │
│  inventory/health/ammo · PoolManager · SpawnZoneSelector · Loadout      │
└───────────────────────┬─────────────────────────────────────────────────┘
                        │ extends / consumes
┌───────────────────────▼─────────────────────────────────────────────────┐
│                    RogueWaveGameMode (scene orchestrator)               │
│   spawns player · configures loadout from RunData · runs victory/death  │
│   coroutines · tracks stats · owns LevelGenerator & AIDirector hooks    │
└──┬────────────┬──────────────┬──────────────┬──────────────┬────────────┘
   │            │              │              │              │
┌──▼──────┐ ┌───▼────────┐ ┌───▼─────────┐ ┌──▼──────────┐ ┌─▼───────────────┐
│WFC Level│ │ AI Director│ │NanobotManager│ │ Scenario/   │ │  Roguelite      │
│Generator│ │ (+Spawners,│ │ (build &     │ │ Campaign    │ │  Managers       │
│ (tiles, │ │  enemies)  │ │  resource    │ │ Manager     │ │  (Recipe, Run/  │
│ buildings│ │            │ │  economy)   │ │ (story)     │ │  PersistentData)│
└─────────┘ └─────────────┘ └─────────────┘ └─────────────┘ └─────────────────┘
   │              │               │                │                 │
   └──────────────┴───────────────┴────────────────┴─────────────────┤
                                                              ┌──────▼─────────┐
   WizardsCode.GameState: GameStatsManager ── stats/achievements/telemetry      │
   WizardsCode.StoryTeller: StoryManager ── Ink narrative (tutorial/campaign)   │
   WizardsCode.CommandTerminal: Terminal ── in-game console for dev/testing      │
                                                              └────────────────┘
```

**Runtime backbone:** most subsystems expose *static singleton accessors* (`GameStatsManager.Instance`, `AIDirector.Instance`, `RogueLiteManager.*`, `MusicManager.Instance`, `StoryManager.Instance`, `AudioManager.Instance`) and communicate via direct references, C# `event`s, and ScriptableObject `GameEvent`s. Scene flow is driven by `NeoSceneManager` (NeoFPS) and the `RogueWaveGameMode`.

---

## 3. Assembly / Module Dependency Map

Assembly definitions (from `*.asmdef` files; GUID references resolved to assembly names). Arrows mean *references*.

```
                        ┌─────────────────────────────┐
                        │  wizardscode.commandterminal │ (Runtime only)
                        └──────────────┬──────────────┘
                                       │
        ┌──────────────────────────────┼───────────────────────────────┐
        │                              │                               │
┌───────▼──────────┐        ┌──────────▼──────────┐        ┌───────────▼─────────┐
│ WizardsCode.      │        │ WizardsCode.        │        │ WizardsCode.        │
│ GameState         │        │ StoryTeller         │        │ CommandTerminal (as│
│ (stats, events,   │        │ (Ink narrative)     │        │  above)             │
│  Steamworks,      │        │  └─ Ink-Libraries   │        │                     │
│  telemetry)       │        └──────────┬──────────┘        └─────────────────────┘
│  └─ Heathen.Core  │                   │
│  └─ Heathen.Steamworks                │
│  └─ Steamworks.NET │                   │
│  └─ Lumpn.Discord  │                   │
└─────────┬──────────┘                   │
          │        ┌─────────────────────┼──────────────────────────┐
          │        │                     │                          │
┌─────────▼────────▼─────────────────────▼──────────────────────────▼──────────────┐
│                     org.wizardscode.roguewave  (main runtime assembly)           │
│  references: NeoFPS, NeoSaveGames, NeoFpsShared, NaughtyAttributes, TMPro,       │
│  ProceduralToolkit(+FastNoiseLib), Excelsior.UI, protips, SineVFX.Forcefield,    │
│  MoreMountains.Tools(Feel), krypto.fx, Kronnect.TunnelFX,                        │
│  WizardsCode.GameState, WizardsCode.StoryTeller, Ink-Libraries,                  │
│  wizardscode.commandterminal                                                    │
└─────────┬───────────────┬──────────────────┬───────────────────┬─────────────────┘
          │               │                  │                   │
┌─────────▼────────┐ ┌────▼────────────┐ ┌───▼─────────────┐ ┌───▼──────────────────┐
│ org.wizardscode. │ │ org.roguewave.  │ │ org.roguewave.  │ │ WizardsCode.Marketing│
│ roguewave.editor │ │ commands.generic│ │ commands.ingame │ │ (recorder, GIFs)     │
│ (Editor only)    │ │ (dev commands)  │ │ (in-game cmds)  │ └──────────────────────┘
└──────────────────┘ └─────────────────┘ └─────────────────┘
        │                                          │
        └──────────────┬───────────────────────────┘
              ┌────────▼────────┐
              │ org.wizardscode. │
              │ commands.dev     │
              │ WizardsCode.     │
              │ RogueWave.Editor │
              │ (Assets/_Dev)    │
              └──────────────────┘
```

Notable dependency facts:

- **`wizardscode.commandterminal`** is the base of the stack (only depends on NaughtyAttributes), referenced by nearly everything — it provides the console framework.
- **`WizardsCode.GameState`** depends on StoryTeller (for Ink-variable stat sync) and CommandTerminal, and pulls in Steamworks.NET + Heathen + Lumpn.Discord. This means the reusable game-state layer carries Steam/Discord dependencies (gated at compile time by defines `STEAMWORKS_ENABLED`/`DISCORD_ENABLED`).
- **`org.wizardscode.roguewave`** (the game assembly) depends on everything first-party plus the third-party list; it is the hub of the system.
- **Dev command assemblies** (`org.roguewave.commands.generic`, `org.roguewave.commands.ingame`, `org.wizardscode.commands.dev`) depend on the main assembly + terminal + GameState — dev tooling layered *on top* of the game, not mixed in.
- **`WizardsCode.Marketing`** depends on the game assembly and Unity Recorder — marketing showcase scenes can drive the actual game.

---

## 4. Scene & Game Flow

Eight first-party scenes in `Assets/_Rogue Wave/Scenes/`:

| Scene | Role |
|---|---|
| `RogueWave_MainMenu` | Entry; profile select / create; main menu (extends NeoFPS `MainMenu`) |
| `RogueWave_LoadingScene` | Transition/loading scene used by NeoSceneManager |
| `RogueWave_IntroLevel` | Story/intro content (campaign single-shot) |
| `RogueWave_HubScene` | Between levels: spend resources on permanent recipes (HubController) |
| `RogueWave_ReconstructionScene` | Post-death summary ("reconstruction" of the run) |
| `RogueWave_CombatLevel` | Procedurally generated combat arena (`LevelGenerator` builds geometry at runtime) |
| `RogueWave_PortalUsed` | Shown when the player exits a level through a portal |
| `RogueWave_ScenarioPlayer` | Scenario playback (`ScenarioManager`) |

### Run flow (as driven by `RogueWaveGameMode` + `NeoSceneManager`)

```
MainMenu ──create/select profile──▶ Intro/Story ──▶ CombatLevel
                                                     │  time limit reached OR
                                                     │  all boss spawners destroyed
                                                     │  OR achievement triggers extraction
                                                     ▼
                                            Victory coroutine (extraction FX)
                                             ├─ no portal  ─▶ HubScene (spend resources)
                                             └─ portal used ─▶ PortalUsedScene ─▶ next CombatLevel
                                                     │
Death ──▶ ReconstructionScene (run summary) ──▶ next run (reset RunData, keep PersistentData)
```

Key flow details from `RogueWaveGameMode`:

- `OnStart()` → optionally `levelGenerator.Generate(currentLevelDefinition, Campaign.seed)` (WFC).
- `PreSpawnStep()` → increments `runNumber`, merges permanent recipes into `PersistentData`, rebuilds `RunData` for the run, applies starting recipes, shows pre-spawn `LevelMenu` if configured.
- `OnCharacterSpawned()` → applies loadout from `RunData.Loadout` via `ILoadoutBuilder`, seeds `NanobotManager` with run recipes, resets health to max, raises `playerSpawnedEvent`, starts combat music.
- **Victory conditions** (three paths, each with its own coroutine — see §8 risk list):
  1. `DelayedLevelTimerAchievedCoroutine` — `timeInLevel >= currentLevelDefinition.Duration` (timed extraction).
  2. `DelayedLevelClearedCoroutine` — all **boss spawners** destroyed (`bossSpawnersRemaining == 0`).
  3. `DelayedLevelCompleteCoroutine` — player enters the **portal** (`PortalController.onPortalEntered`).
- **Death:** `DelayedDeathAction()` → death FX + music, `SaveGameData("Death")`, `RogueLiteManager.ResetRunData()`, load `ReconstructionScene` (or `MainMenu` from intro).
- Level completion increments `PersistentData.currentGameLevel` and victory/portal stats; `CampaignManager.EnableNextCampaignIfReady()` unlocks the next campaign when achievements are met.

`ScenarioManager` (in `RogueWave_ScenarioPlayer`) loads a `ScenarioDescriptor` into a dummy one-level campaign and can run a scripted terminal script — a deterministic way to exercise specific content.

---

## 5. Per-System Analysis

### 5.1 Roguelite Meta-Progression

The meta-game lives in `Assets/_Rogue Wave/Runtime/Roguelite/` and `Nanobots/`.

#### 5.1.1 RogueLiteManager — `Roguelite/RogueLiteManager.cs`

- Extends `NeoFpsManager<RogueLiteManager>` (a NeoFPS manager singleton); asset instance at `Resources/FpsManager_RogueLite.asset`.
- Static surface: `PersistentData`, `RunData`, `availableProfiles`, `CurrentProfile`, scene names (`MainMenuScene`, `HubScene`, `CombatScene`, `PortalScene`, `reconstructionScene`).
- **Profiles** stored as files in `Application.persistentDataPath/Profiles/` with extensions `.profileData`, `.statsData`, `.campaignData` (see §5.7).
- `CreateNewProfile`, `LoadProfile(index)`, `SaveProfile()` (dirty-flag driven), `ResetRunData()`, `ResetPersistentData()`, `AssignPersistentData()` (used by `CustomRogueLiteData` for bespoke data).
- Editor menu items for profile exploration/deletion/tutorial reset.

#### 5.1.2 Data models

- **`RogueLitePersistentData`** — survives death/runs: `runNumber`, `currentGameLevel`, `currentNanobotLevel`, `RecipeIds` (list of permanent recipe GUIDs), `WeaponBuildOrder` (derived list of weapon/tool recipe IDs), `isDirty` flag. `Add(IRecipe)` handles stackability/max-stack and inserts weapons into the build order (respecting `overridePrimaryWeapon`).
- **`RogueLiteRunData`** — reset on death: `Loadout` (list of `FpsInventoryItemBase` prefabs) and `m_RunRecipeData` (list of `IRecipe`). `Add(IRecipe)` enforces stackability and mirrors weapon build-order insertion (noted in code as duplicate logic).

#### 5.1.3 Recipe system — `Roguelite/Recipe/`

The recipe is the core unit of progression: buying/building an upgrade.

- **`IRecipe`** interface: identity (`UniqueID`, `DisplayName`), cost (`BuyCost`, `BuildCost`), stacking (`IsStackable`, `MaxStack`, `CurrentStack`), gating (`IsAvailable`, `CanOffer`, `Level`, `Dependencies`, `Complements`, `weight`), lifecycle (`Reset()`, `ShouldBuild`, `BuildFinished()`), and feedback assets (audio clips, particles).
- **`AbstractRecipe : ScriptableObject, IRecipe`** — base implementation: description/hero/icon, level, `baseWeight`, dependencies, complements, `baseBuyCost`, build fields (`buildCost`, `timeToBuild`, `cooldown`, `isStackable`, `maxStack`), audio clips, and `uniqueID` (GUID string, editor-generated). Implements weight calculation (base + complement bonuses + stack scaling) and `CanOffer` (dependencies owned, not at max stack).
- **`GenericItemRecipe<T> : AbstractRecipe, IItemRecipe where T : MonoBehaviour`** — recipe that builds a physical pickup (`pickup` field).
- **Build recipes** (produce pickups in-world): `WeaponRecipe` (with `ammoRecipe` link, `usesAmmo`, `overridePrimaryWeapon`), `AmmoRecipe`, `HealthPickupRecipe` (heal % logic used by NanobotManager), `ArmourRecipe`, `ShieldRecipe`, `ToolRecipe`, `ItemRecipe`.
- **Player stat recipes** (`BaseStatRecipe : AbstractRecipe`, abstract `Apply()`): `HealthRecipe` (max health), `MagnetRecipe`, `NanobotStatRecipe`, `ShieldDamageMitigationRecipe`, `SkillRecipe`, `MotionGraphRecipe`. These modify the player directly rather than spawning pickups.
- **Weapon upgrade recipes**: `AmmunitionEffectUpgradeRecipe` (abstract `Apply(RogueWaveBulletAmmoEffect)`), `AmmunitionDamageMultiplierRecipe`.
- **Passive recipes**: `PassiveItemRecipe` (applies a `PassivePickup`/`PassiveWeapon` to the nanobot pawn), `PassiveWeaponStatRecipe`.
- **`RecipeManager`** (static) — lazy-loads all recipes from `Resources/Recipes` into `allRecipes` (keyed by `UniqueID`), keeps a `powerupRecipes` subset, exposes `TryGetRecipe(GUID)` and `GetOffers(quantity, requiredWeaponCount)` which returns weighted-random offer lists (weighted via `WeightedRandom<T>`).

#### 5.1.4 NanobotManager — `Nanobots/NanobotManager.cs`

The in-run build/economy brain (component on the player character):

- Tracks resources via an `IntGameStat` (`RESOURCES`); `CollectResources(amount, ResourceType)` feeds level-up progress (`resourcesForLevel` curve → `currentNanobotLevel` in PersistentData).
- Holds typed lists of the recipes available this run (`AddToRunRecipes(IRecipe)` dispatches by type).
- **Auto-build priority loop** in `Update()`: ammo (low) → health (badly hurt) → shield → armour → weapons → health top-up → ammo near-max → generic items → shield/armour/health max. Builds spawn pickups in front of the player (`BuildRecipe` coroutine, with cost, audio announcements, particles).
- **Level-up / offer flow:** when resources hit the next level, `OfferInGameRewardRecipe()` presents `numInGameRewards` (3) offers from `RecipeManager.GetOffers`; the player picks with keys B/N/M; the chosen recipe is added to `RunData` and `AddToRunRecipes`. Level-ups can stack (`stackedLevelUps`).
- Emits C# events (`onRequestSent`, `onBuildStarted`, `onNanobotLevelUp`, `onStatusChanged`, `onOfferChanged`) consumed by `NanobotManagerUI` and the HUD.
- Death resets nanobot level to 1 (with resource headstart logic in `Start()`).

#### 5.1.5 CampaignManager — `Roguelite/CampaignManager.cs`

- Extends `StoryManager` (so each campaign can drive an Ink story).
- Holds `CampaignDefinition[] m_Campaign`; `Awake()` picks the first incomplete campaign (`DontDestroyOnLoad`), skipping completed `isGlobalSingleShot` campaigns; initializes its Ink story.
- `EnableNextCampaignIfReady()` advances `m_CurrentCampaignIndex` and resets `currentGameLevel` when the current campaign's completion achievements are met.
- `CampaignDefinition` (`Levels/CampaignDefinition.cs`) — ScriptableObject with: campaign metadata, optional `InkStory` `TextAsset`, `seed`, `WfcDefinition[] levels`, `requiredAchievementsForCompletion`, `nextCampaign`, and `IsComplete` (all required achievements unlocked).

---

### 5.2 Procedural Level Generation (WFC)

Located in `Assets/_Rogue Wave/Runtime/Levels/` (+ `Tiles/`, `Building/`).

#### 5.2.1 WfcDefinition — `Levels/WfcDefinition.cs`

ScriptableObject describing a **level** (or a tile layout): metadata (`displayName`, unlock/completion achievements, `extractUponCompletion`, `isReplayable`), size & layout (`seed`, `lotSize`, `mapSize`, `encloseLevel`), tile pools (`defaultTileDefinition`, `wallTileDefinition`, `excludedTileTypes` enum flags, `forbiddenTiles`), enemy waves (`WaveDefinition[] waves`, `waveWait`, `generateNewWaves`, `maxAlive`), generation options (`generateLevelOnSpawn`, `prePlacedTiles`), audio (level ready/complete/death clips), and an optional `LevelWaveGenerationConfiguration` used by the dev wave generator. Provides `challengeRating`, `Duration` (sum of waves; `float.MaxValue` if none), `IsUnlocked`, `Completed`, `Description`.

#### 5.2.2 TileDefinition & TileConstraint — `Levels/Tiles/TileDefinition.cs`

- `TileDefinition` ScriptableObject: UI metadata, `TileType` enum flags (`Undefined, Empty, Wall, Building, Flora, SocialSpace, EnemySpawner, PlayerSpawner, DiscoverableItem, SetAreas, Infrastructure`), ground/materials, `enemySpawnChance`, **directional neighbour constraints** (`xPositiveConstraints`, `xNegativeConstraints`, `zPositiveConstraints`, `zNegativeConstraints` — lists of `TileNeighbour{tileDefinition, constraints}`), and `TileConstraint` (placement bounds as % of map + weight).
- `GetTileCandidates(Direction)` returns the allowable neighbour definitions — this is what WFC propagates.

#### 5.2.3 LevelGenerator — `Levels/LevelGenerator.cs`

Implements Wave Function Collapse:

1. `Generate(levelDefinition, baseCoords, root, seed)` → `GenerateLevel(...)`.
2. `PlaceContainingWalls()` (if `encloseLevel`), then `PlaceFixedTiles()` (pre-placed tiles with constraint-boundary random placement, 50 tries, fallback exhaustive scan).
3. `WaveFunctionCollapse()` — computes candidate sets for each empty cell from neighbour constraints (union of neighbours' `GetTileCandidates`), filters `excludedTileTypes`/`forbiddenTiles`, picks the lowest-entropy cell(s), collapses via `CollapseTile` (weighted by reciprocal neighbour weights). Recurses until no cells remain; defaults leftover cells to `defaultTileDefinition`. **Note:** the implementation collapses one cell per pass and re-scans the whole grid recursively (`WaveFunctionCollapse()` calls itself), which is functionally correct but O(n²·cells)-ish — a performance observation (§8).
4. `GenerateTileContent()` — calls `BaseTile.Generate(x, y, tiles, generator)` on every tile.
5. `isValidateLevel()` — at least one spawn point exists and no interior tile is fully surrounded by barriers; regenerates (up to 3 attempts) with a new seed on failure in non-editor builds (editor: fast-fail with a log).
6. Positions the scene camera over the level; `HideLevelGeometry()` used during extraction FX.

#### 5.2.4 Tiles — `Levels/Tiles/`

- `BaseTile` (MonoBehaviour) — base; `Generate(x, y, tiles, generator)` hook.
- `ConnectedTile` (internal) — auto-connects to neighbours (walls/paths/buildings); subclasses `BarrierTile`, `PathTile`, `BuildingTile`, `ProximitySpawnerTile` (spawns enemies when player is near), `SpawnerTile`.
- `MultiCellTile` — occupies a `TileArea` of cells (e.g. buildings).
- `PlayerSpawnTile`, `DiscoverableItemTile` (spawns a `DiscoverableController`), `WfcTile` (generic WFC-driven tile).

#### 5.2.5 Procedural buildings — `Levels/Building/`

- `BuildingGeneratorComponent` — MonoBehaviour that places buildings from a `PolygonAsset` floorplan on a lot.
- `ProceduralFacadePlanner` / `ProceduralFacadeConstructor` (+ `ProceduralFacadeElements` — walls, windows, doors) and `ProceduralRoofPlanner` / `ProceduralRoofConstructor` — procedural mesh construction (extends NeoFPS/ProceduralToolkit façade/roof planner/constructor base classes).
- `BuildingSurface : BaseSurface` (NeoFPS surface — footstep/impact audio/material), `PolygonAsset` (floorplan ScriptableObject).
- Runtime asset instances live under `Resources/Levels/Buildings/` (floorplans, façade/roof planners & constructors).

#### 5.2.6 Other level objects

- `DestructibleController` — destructible geometry: pooled destruction FX, resource drops (`resourcesDropChance`, `possibleDrops`), `magnetizeResources`, `destructibleDestroyed` stat. Subclassed by `BuildingController` and `DiscoverableController` (loot/info interactables).
- `CrystalPatch`, `PortalController` (with `onPortalEntered` event consumed by `RogueWaveGameMode.RegisterPortal`), `ScenarioDescriptor` (scenario ScriptableObject: display name, description, `LevelDefinition` (WfcDefinition), `Recipes`, `TerminalScript`).

---

### 5.3 AI Director & Enemy System

#### 5.3.1 AIDirector — `Enemies/AIDirector.cs`

Singleton (`AIDirector.Instance`) coordinating enemy pressure:

- **Kill-score pacing:** every `timeSlice` (50 s) it computes `currentKillscore` (challenge rating killed / time slice) and compares with `targetSkillScoreByLevel.Evaluate(nanobotLevel)`; if the player is above target, it sends no reinforcements (respite); below target it issues `RequestAttack` orders to squads then individual enemies, and finally `SpawnEnemies(remainingCR)` from the 3 spawners nearest the player.
- **Squads:** `JoinOrCreateSquad` groups enemies by `SquadRole` (`None/Fodder/Leader`); leaders are found via `Physics.OverlapSphere(10, "Enemy")`. Squad knowledge sharing lets non-sighted members follow a leader who has line of sight.
- **Player location tracking:** `ReportPlayerLocation` keeps the last 3 reported positions; `suspectedTargetLocation` is the average. If no report for `maximumTimeBetweenReports` (30 s), a spawner is asked to spawn a `ScannerController` to re-acquire the player.
- Listens to `RogueWaveGameMode.onSpawnerCreated` / `onEnemySpawned` to register spawners/enemies and to `onSpawnerDestroyed`/`onDeath` to feed `killReports` and maintain squads.
- Exposes dev/debug commands (`Kill(percentage)`, `EnableSpawning/DisableSpawning`, `SpawnEnemiesNearPlayer`, `GetSpawnerAvailableEnemies`) used by the in-game terminal.

#### 5.3.2 BasicEnemyController — `Enemies/BasicEnemyController.cs`

The enemy base (extends NeoFPS `PooledObject`):

- **Data-driven config** (all serialized): metadata (display name, icon, description, strengths/weaknesses), senses (`requireLineOfSight`, `viewDistance`, `sensorMask`, `sensor`), animation (head look), seek behaviour (`returnToSpawner`, `seekDistance`, `optimalDistanceFromPlayer`, destination update frequency), defensive behaviour (spawn defensive units on damage), squad role, death behaviour (explosion), audio juice (barks/drone), visual juice (death FX), loot (`resourcesDropChance`, `resourcesPrefab`), and stats hooks (`enemySpawnedStat`, `enemyKillsStat`).
- **Challenge rating** — computed from defensive (health, LOS, seek distance, defensive spawns), movement (speed, height, optimal distance), and offensive (weapon `IWeaponFiringBehaviour.DamageAmount`) ratings; drives AI director math and wave balancing.
- **Behaviour:** line-of-sight raycasts (frame-throttled), squad LOS sharing, destination selection (`GetDestination` = point near target at `optimalDistanceFromPlayer`, validated with avoidance raycasts), wandering within level bounds, "recharging" back to spawn point, `RequestAttack(position)` for director orders, defensive-unit spawning on damage threshold, head rotation, bark/drone audio.
- **Death:** resource drop chance, `onDeath` event, death FX (`RWPooledExplosion`), kill stat, pool return.
- `Validate()` — editor validation of death behaviours, FX, animation, weapons, defensive units, loot.

#### 5.3.3 Enemy variants & supporting components

- `WaitAndLungeEnemyController : BasicEnemyController` — lunge/wait behaviour.
- `ScannerController` — spawned by AIDirector when player location is stale; reports location.
- `EnemyFirearmController` — drives a `BasicWeaponController` on the enemy.
- `BasicMovementController` — movement model (min/max speed, height, hasArrived) used by enemies and nanobot pawns.
- `ShieldDamageHandler`, `BasicEnemyDamageHandler` — NeoFPS damage handlers for shielded/standard enemies.
- `EnemyDissolve` — dissolve-on-death shader effect.

#### 5.3.4 Spawner — `Levels/Spawner.cs`

A `Spawner` is itself a `BasicEnemyController` (a destructible object):

- Spawns enemies from `currentWave` (`WaveDefinition`) with `spawnEventFrequency`/`SpawnAmount`; respects `maxAlive` unless `ignoreMaxAlive`.
- Boss spawners (`isBossSpawner`) must all be destroyed for level clear (tracked by `RogueWaveGameMode.bossSpawnersRemaining`).
- Optional rotating **shield generators** (`numShieldGenerators`, `shieldGeneratorRPM`) protecting a `ForceFieldController` shield; player must destroy generators to disable the shield.
- `GenerateNewWave()` creates a new runtime `WaveDefinition` when `generateNewWaves` (loosely based on previous wave: spawn frequency faster).
- `onSpawnerDestroyed`, `onEnemySpawned`, `onAllWavesComplete` events; `destroySpawnsOnDeath` kills spawned enemies when the spawner dies.

#### 5.3.5 WaveDefinition — `Levels/WaveDefinition.cs`

ScriptableObject: `EnemySpawnConfiguration[]` (pooled enemy prefab + weight), `spawnEventFrequency`, `numberToSpawn`, `spawnOrder` (`WeightedRandom`/`Sequential`), editor-computed `challengeRating` (weighted enemy CR × spawn rate × 60 s duration). Editor-only wave **balancing** (`BalanceWave` iteratively adjusts frequency/amount and randomizes enemy types to hit a target CR) and `GenerateWave` used by the dev `LevelWaveGeneratorWindow`.

---

### 5.4 Combat & Weapons

Combat extends NeoFPS modular firearms. First-party additions in `Assets/_Rogue Wave/Runtime/Weapons/` and `FX/`.

#### 5.4.1 Weapon behaviour pipeline — `FX/Behaviours/`

- `IWeaponFiringBehaviour` — minimal interface (`DamageOverTime`, `DamageAmount`).
- `BasicWeaponBehaviour` — composable behaviour base: duration, audio, start/stop events, validation.
- `LineWeaponBehaviour : BasicWeaponBehaviour` — line/ray based.
- `HitscanFiringBehaviour : LineWeaponBehaviour` — hitscan firing.
- `TargetingBehaviour : LineWeaponBehaviour` — targeting/lock-on behaviour (used by the AI enemy weapons).

#### 5.4.2 BasicWeaponController — `FX/BasicWeaponController.cs`

Enemy-side weapon controller implementing NeoFPS `IDamageSource`: ammo type (defines FX & damage), range, targeting, idle/lock-on/fire/reload behaviours, damage description, damage filter (which teams are damaged), layers.

#### 5.4.3 Projectiles & special weapons — `Weapons/`

- `Motors/BasicProjectileMotor` + `TrackingProjectileMotor` — projectile movement (straight / homing).
- `Passive/` — nanobot-pawn weapons: `PassiveWeapon : NanobotPawnUpgrade`, `PassiveProjectileWeapon`, `PulseWeapon`, `DestructorBeam`, `PassivePickup` (pickup that applies a passive weapon).
- `ExplodeOnContact`, `ProximityDamage`, `SelfDestructOnContact`, `EnemyDamageTriggerZone`, `ThrownTurretProjectile` (turret deployable), `RogueWaveBulletAmmoEffect : BaseAmmoEffect` (custom ammo effects; upgraded by `AmmunitionEffectUpgradeRecipe`).
- `FX/ProjectileEffectMotor`, `FX/Feel Feedbacks/MMF_LineEffect` (Feel feedback for line FX), `FX/BasicWeaponController` (see above), `FX/ExtractionFXController` (victory/death extraction sequence: hides HUD/geometry, plays FX audio).
- Player weapons/pickups: `Pickups/WeaponPickup : InventoryItemPickup`, `Inventory/InventoryItemPassivePickup`.

#### 5.4.4 Damage & health

- `PlayerDamageHandler : ShieldedArmouredDamageHandler` (NeoFPS) — player shield/armour/health layering.
- `BasicEnemyDamageHandler`, `ShieldDamageHandler` — enemy damage handlers.
- `RWPooledExplosion : PooledExplosion, IDamageSource` — pooled explosion with damage/knockback (used by enemy death, destructibles).
- Health is NeoFPS `BasicHealthManager` on both player and enemies; `RogueWaveGameMode` sets player `initialHealth` and listens to `onIsAliveChanged`.

#### 5.4.5 Upgrades

- `Upgrades/FloatValueModifier` — named float multiplier values.
- `Upgrades/MovementUpgradeManager : MonoBehaviour, IMotionGraphDataOverride` — exposes `moveSpeed`, `moveSpeedAirborne`, `acceleration`, `maxJumpHeight`, `jetpackForce`, `dashSpeed` modifiers and booleans (`canDash`, `canGrapple`, `canJetpack`, `canWallRun`, `canAimHover`…); consumed by `SkillRecipe`s (`MotionGraphRecipe`, `SkillRecipe`) and by `RogueWaveGameMode` (e.g. zeroing `moveSpeed` during portal extraction).

---

### 5.5 Game Stats, Achievements & Telemetry

Located in `Assets/Wizards Code/Game State/Runtime/` (assembly `WizardsCode.GameState`, namespace `RogueWave.GameStats` / `WizardsCode.RogueWave`).

#### 5.5.1 GameStatsManager — `Stats/GameStatsManager.cs`

Singleton (`DontDestroyOnLoad`) that loads **all** `IntGameStat`, `StringGameStat`, and `Achievement` assets from `Resources`:

- `GetIntStat(key)` / `GetStringStat(key)` — linear scans of the loaded stat arrays (noted as optimizable).
- `unlockedAchievements`, `AllAchievementsInCategory(Category)` (categorized cache), `NotDemoLockedAchievements` / `DemoLockedAchievements`.
- **Score** — stats with `contributeToScore` multiply value × `m_ScoreMultiplier`.
- **Reset** — `ResetLocalStatsAndAchievements()` (values back to defaults, achievements locked; editor menu + button).
- **Telemetry** — `SendDataToWebhook(eventName)` builds YAML chunks (summary, player stats, achievements, score, performance, machine, build) and posts them to a Discord webhook (gated by `DISCORD_ENABLED`; different webhooks for player/developer/"Sorra" device). `HandleLog` forwards exceptions to the exception webhook in dev builds.
- **Steamworks** — when `STEAMWORKS_ENABLED` (and not `STEAMWORKS_DISABLED`), periodically `StoreStats()` and dump/verify stats via Heathen Steamworks API.

#### 5.5.2 GameStat<T> — `Stats/GameStat.cs`, `IntGameStat.cs`, `StringGameStat.cs`

- `GameStat<T> : ScriptableObject, IGameStat<T>` — key, display name, description, default value, optional Ink variable sync (`m_InkVariableName` → `StoryManager.SetInkVariable` for ints), `ParameterizedGameEvent<T>` onChange event. `Value` setter fires events/Ink sync.
- `IntGameStat` adds: time formatting, score contribution/multiplier, and tracking companion stats (`increasedAmount`/`decreasedAmount` — e.g. resources spent tracked by a second stat).
- `StringGameStat` — string values (e.g. run log).
- `IGameStat<T>` — the contract (`key`, `Value`, `Add/Subtract`, `ScoreContribution`).

#### 5.5.3 Achievements — `Stats/Achievement.cs`

`Achievement : ScriptableObject, IParameterizedGameEventListener<int>`: category (`Uncategorized, Levelling, Offense, Defense, Objective`), demo-lock flag, key/display/description/hero/icon, tracks an `IntGameStat` to `TargetValue`, raises `AchievementUnlockedEvent`, records unlock time. Demo builds sort demo-unlocked achievements first.

#### 5.5.4 Event system — `Events/`

ScriptableObject event pattern:

- `GameEvent` (no params) + `GameEventListener : MonoBehaviour, IGameEventListener`.
- `ParameterizedGameEvent<T>` + `ParameterizedGameEventListener<T>`, `IntGameEvent : ParameterizedGameEvent<int>` (+ `IntStatEventListener`).
- `AchievementUnlockedEvent` + `AchievementEventListener` / `IAchievementEventListener`.
- Interfaces: `IGameEventListener`, `IParameterizedGameEvent<T>`, `IParameterizedGameListener<T>`.

These are used e.g. by `RogueWaveGameMode` (`playerSpawnedEvent`, `playerDiedEvent`, `playerEscapedEvent`, `playerExitedViaPortalEvent`), stats (`onChangeEvent`), and achievements (`onUnlockEvent`).

#### 5.5.5 GameLog — `Stats/GameLog.cs`

Static in-memory log of `key:value` activity (info/warning/error), echoed to console in editor/dev builds; used extensively by the game systems for run logging and debugging. Cleared at run start/end.

#### 5.5.6 Steamworks — `Steamworks/SteamworksController.cs`

Enables/disables Steamworks defines, holds demo/main `SteamSettings`, optional screenshot automation, and forwards screenshot events — used in dev (`Scenes Dev/Steam Dev.unity`).

#### 5.5.7 FPSCounter — `FPSCounter.cs`

On-screen FPS counter (editor/dev builds) plus average/min/max collection used in telemetry YAML.

#### 5.5.8 Command integration — `CommandTerminal/GameStateCommands.cs`, `SteamworksCommands.cs`

`GameStateCommands` (reset/dump stats) and `SteamworksCommands` (status, screenshot) register console commands.

---

### 5.6 Story Teller (Ink Narrative)

Located in `Assets/Wizards Code/Story Teller/Runtime/` (assembly `WizardsCode.StoryTeller`), bundled with the `ink-unity-integration` submodule (compiler + Ink-Libraries).

#### 5.6.1 StoryManager — `StoryManager.cs`

A `DontDestroyOnLoad` MonoBehaviour (default execution order −10000) that plays Ink stories:

- **Init/Load/Save:** `Init(saveFilename)`, `InitializeStory(TextAsset inkJSON)` (creates `Ink.Runtime.Story`, binds external functions, loads state), `Load()`/`Save(knotName)` — story state JSON persisted to a file named via PlayerPrefs, resume point and scene stored in PlayerPrefs (§5.7). `ResetStory()` deletes save data.
- **Playback:** `Update()` → `ProcessStoryChunk()` → `ActiveStory.Continue()` line loop. Lines are parsed into three categories:
  1. **Directions** — lines containing `>>>` invoke a `DirectionName` command (e.g. `>>> Wait For Duration: 2`). Directions are discovered by **reflection** over `AbstractDirection` subclasses at `Start()` and instantiated per use.
  2. **Dialogue** — lines matching `^(\w*>)` are treated as `ActorName> speech`; the actor is looked up via `IActorController.DisplayName` and prompted with `m_startTalkingCue`; speech is displayed with typewriter effect (`StoryTextController`).
  3. **Narration** — anything else, displayed as-is.
- **UI:** choice buttons instantiated from `m_ChoiceButtonPrefab` into `choicesPanel`; auto-advance single choice option; UI hiding on empty text; `StoryManagedUIElement`s can be hidden during story beats.
- **Scene/knot mapping:** `AddSceneLoadListener(SceneToKnotMapping)` + `ResumeFromKnot(knotName)` — resumes story from a knot when a named scene loads (one-shot by default).
- **Wait states:** `AbstractWaitForDirection` subclasses register with `AddWaitForState`; `isWaiting` blocks story progression until conditions are met.
- **Actors:** `IActorController` (implemented by scenes' actor controllers) with `ActorCue` ScriptableObjects (audio + mark name + `Prompt/Revert`).
- `Start()` uses `GetTypes()` over all loaded assemblies to find `AbstractDirection` subclasses — the extension mechanism.

#### 5.6.2 Directions (built-in) — `Directions/`

| Direction | Behaviour |
|---|---|
| `AudioMixerDirection` | Set audio mixer group volume (db conversion) |
| `ExecuteDirection` | Invoke a method by name on a target object |
| `HideStoryUIDirection` | Hide/show the story UI |
| `PlayAudioDirection` | Play an audio clip |
| `ResumeOnSceneLoadDirection` | Register scene→knot mapping (listen for scene load) |
| `SetSavePointDirection` | Save story state at a knot |
| `WaitForDurationDirection` | Wait N seconds |
| `WaitForReachTargetDirection` | Wait until actor reaches a target |
| `WaitForResumeSignalDirection` | Wait for a resume signal |
| `WaitForSceneLoadDirection` | Wait until a named scene loads, then resume at a knot |

Rogue Wave adds two Rogue Wave-specific directions in `Assets/_Rogue Wave/Runtime/Story/`:
- `PlayNanobotAudioDirection` — play nanobot audio.
- `RogueWaveSavePointDirection : SetSavePointDirection` — RW-specific save point (adds game log/save behaviour).

**Story content:** `Assets/_Rogue Wave/Resources/Story/Tutorial.ink` (+ compiled `Tutorial.json`) — the tutorial narrative.

#### 5.6.3 Story UI — `UI/`

- `StoryTextController` — typewriter dialogue text (speaker-aware, speaking sounds, per-character delays).
- `StoryManagedUIElement` — marker that the story can hide during beats.

---

### 5.7 Save / Profile System

The save system is split across three mechanisms:

1. **RogueLiteManager profiles** (`Roguelite/RogueLiteManager.cs`):
   - Files in `Application.persistentDataPath/Profiles/`, one folder per install; profile = base filename with three extensions:
     - `.profileData` — JSON of `RogueLitePersistentData` (permanent recipes, weapon build order, run/level numbers, nanobot level, dirty flag).
     - `.statsData` — JSON of all `IntGameStat` key/value pairs (`StatsWrapperArray`).
     - `.campaignData` — JSON of the `CampaignManager` (current campaign index + serialized state).
   - Dirty-flag driven: `SaveProfile()` writes only when `PersistentData.isDirty`; `OnSceneLoaded` in `RogueWaveGameMode` saves on entering combat/reconstruction scenes; `OnDestroy` saves as a safety net.
2. **NeoSaveGames** (third-party, `NeoFPS/Core/NeoSaveGames/`) — `NeoSerializedGameObject`/`INeoSerializableComponent` serialization for scene/world state (used by `ResourcesPickup`, `RogueWaveGameMode.WriteProperties/ReadProperties` for the spawn-zone index, etc.).
3. **StoryManager saves** — Ink story state JSON file (filename from PlayerPrefs) + PlayerPrefs resume point & scene; reset via `RogueLiteManager.ResetTutorial` menu item.

`CustomRogueLiteData` (MonoBehaviour) can override the persistent data instance for bespoke/test setups (`AssignPersistentData`).

---

### 5.8 UI / HUD

Located in `Assets/_Rogue Wave/Runtime/UI/`. Built on NeoFPS sample UI (`MenuPanel`, `MainMenu`, `PreSpawnPopupBase`, `OptionsMenuPanel`, `MenuNavControls`, `InstantSwitchTabBase`, `TooltipTrigger`) + TextMeshPro.

- **Main menu flow:** `RW_MainMenu : MainMenu` (root nav controls `RogueWaveRootNavControls`), `SelectProfilePanel` / `CreateNewProfilePanel` (profiles), `OptionsMenuPlaystyle` (difficulty/playstyle settings), `CharacterSetup`.
- **Hub / recipe selector:** `HubController` — shows permanent & temporary recipes, resource count, level/nanobot readouts, "LevelUp" and "RerollOffers" (with cost), `PrepareNanotransfer` (hub → combat transition), `isPermanentRecipesDirty`/`isTemporaryRecipesDirty` caches. `RecipeCard`, `RecipeUpgradePanel`, `AcquiredRecipePanel`, `BuildOrderEditorUI`/`BuildOrderTab`/`SortableRecipeList`/`DraggableListItem`, `WeaponBuildOrder` (drag-reorder build order), `RecipeListUIElement`, `BuildOrderUIElement`, `RecipeTooltipTrigger` (+ editor).
- **HUD:** `HudGameStatusController` (enemy/spawner counts from AIDirector/RogueWaveGameMode), `LevelProgressBar`/`ProgressBar` (time-in-level vs `WfcDefinition.Duration`), `RW_HudAdvancedCrosshair`, `RW_HudDamageMarkers`, `RW_HudHider` (hides HUD during extraction), `DeathPopup`, `HudLevelCompletePopup`, `HudPortalUsedPopup`, `LevelInfoPanel` (pre-spawn info), `NanobotManagerUI` (nanobot status/offers).
- **Stats/achievements:** `ReconstructionController` (post-death summary), `AchievementListController`/`AchievementCategoryController`/`AchievementUIElement`/`AchievementNotification`/`AchievementNotificationManager`, `StatUIElement`.
- **Level select:** `LevelUiController` (map `mapSprite` from spawn-zone data; `OnLevelClicked`) and `LevelMenu : PreSpawnPopupBase` (pre-spawn popup; `Initialise(gameMode, callback)`).
- **Other:** `RW_LoadingScreen`, `Tab Controller`/tabs (`InfoTab`, `EnemyDetailsTab` + `EnemyDetailsUIController`), `ElementEnablementController`, `DeveloperUI`.

---

### 5.9 Audio

Located in `Assets/_Rogue Wave/Runtime/Audio/`.

- **`AudioManager`** (singleton, `DontDestroyOnLoad`): routes audio into mixer groups (`master`, `music`, `nanobots`, `ui`, `ambience`, `spatial`, `twoDimensional`); volume fades (`FadeGroup`, `ResetAll`, `MuteAllExceptNanobots`), 2D/3D one-shots and loops, db↔normalized conversion. Used by enemies (barks/drones), nanobots (announcements), weapons, extraction FX, and story directions.
- **`MusicManager`**: `MusicType` (Menu/Combat/Escape/Death…) with `PlayMenuMusic`, `PlayCombatMusic`, `PlayEscapeMusic`, `PlayDeathMusic`, `StopMusic` — driven by `RogueWaveGameMode` events.
- **`PlayerAudioManager`** — player footstep/impact/voice audio (via NeoFPS surface system).
- **`SavWav`** — WAV save helper (dev tooling).

---

### 5.10 Dev Tools, Editor Tooling & Showcases

#### Command Terminal (in-game console)

- `Assets/Wizards Code/Command Terminal/Runtime/` — reusable console: `Terminal` (UI + state machine), `CommandShell` (command registry with `RegisterCommandAttribute`, runtime levels, arg parsing), `CommandAutocomplete`, `CommandHistory`, `CommandLog`, `WatchedCommand` (repeat commands), built-in commands (`EngineCommands`, `SceneManagementCommands`, `PerformanceManagementCommands`, `TerminalCommands` — `Help`, `Watch`, `Time`, `Run` script files, `SetRuntimeLevel`…), `SceneSetupCommands` (run scripts on scene awake/start/on-demand).
- `Assets/_Rogue Wave/Runtime/DevTools/Command Terminal/` — game-specific command sets:
  - Generic (`org.roguewave.commands.generic`): `RecipeCommands` (list/add recipes), `ResourceCommands` (add/remove resources).
  - In-game (`org.roguewave.commands.ingame`): `EnemyCommands` (spawn/kill enemies, toggle spawning), `LevelManagementCommands`, `NanobotCommands`, `PlayerCommands`, `ScenarioCommands` (load scenarios), `DiscoverableCommands`.
- `Assets/_Dev/Runtime/org.wizardscode.commands.dev` — dev-only commands.

#### Editor tooling — `Assets/_Rogue Wave/Editor/`

| Tool | Purpose |
|---|---|
| `EnemyDataWindow` | Browse/edit enemy data |
| `Levelling/RecipeDataWindow` | Recipe data editor |
| `Levelling/AchievementDataWindow` | Achievement data editor |
| `Levelling/CustomRogueLiteDataEditor` | Inspector for `CustomRogueLiteData` |
| `CSV/CSV.cs` | Export/import recipes (and other ScriptableObjects) to/from CSV |
| `DataValidation` | Validate recipes (menu: Tools/Rogue Wave/Data/Validate Recipes) |
| `Voice Acting/SpeechEditorWindow` | ElevenLabs voice/TTS preview & speech asset generation |
| `UI/RecipeTooltipTriggerEditor` | Tooltip editor for recipe cards |
| `Command Terminal/KeyCommands` | Key-press simulation commands (Esc) |

#### Dev tooling — `Assets/_Dev/`

- `Editor/LevelWaveGeneratorWindow.cs` — generates wave/level definitions from `LevelWaveGenerationConfiguration` (flow curve, challenge ratings, enemies).
- Dev scenes (`Scenes Dev/`): Animation, AudioProcessing, Building, Destructible, Enemy, Level, Level Menu, Nanobot, Pickup, Playtest, Steam, Weapon, Weapon Modeling. Showcase scenes (`Scenes Showcase/`).
- Dev runtime assembly `org.wizardscode.commands.dev`.

#### Showcase & marketing — `Assets/_Marketing/`

- `Runtime/ShowcaseDirector.cs`, `Runtime/Descriptors/GifAssetDescriptor.cs`, `Runtime/RecorderUtils.cs` — automated showcase capture (Unity Recorder → GIFs) for enemies/objects/weapons.
- `Assets/_Rogue Wave/Runtime/Showcase/` — `ShowcaseDirector`, `BasicEnemyShowcase`, `BasicObjectShowcase` (in-game showcase rigs).
- Marketing scenes (`Showcase - Meet the Enemies`, `Stage`, `Technology is Magic`).

#### Other runtime helpers

- `WeightedRandom<T>` — generic weighted-random selector used by recipes and tile collapse.
- `CoroutineHelper` — run coroutines from non-MonoBehaviours.
- `Common/SceneManagement`, `Common/LodController`, `Common/FaceCamera`, `Common/TimeToLive`, `Animation/RotateObject`, `NameGenerator` (random naming for generated content).
- `NeoFpsExtensions.cs` — extension helpers for NeoFPS types (`GetItemPrefab`, `GetHealAmount`, `AddDamageMitigation`, crosshair access, loadout slot options).

---

## 6. Key Architectural Patterns

1. **Data-driven ScriptableObject design (dominant pattern).** Recipes, levels (`WfcDefinition`), tiles (`TileDefinition`), waves (`WaveDefinition`), stats (`GameStat<T>`), achievements, scenarios, campaigns, and buildings are all authored as ScriptableObjects under `Assets/_Rogue Wave/Resources/` and loaded at runtime (`Resources.LoadAll<...>` / `Resources.Load`). Adding content = adding assets, not code.

2. **NeoFPS extension points.** The game is a thin-but-wide extension of NeoFPS: `RogueWaveGameMode : FpsSoloGameCustomisable` (implements `ISpawnZoneSelector`, `ILoadoutBuilder`), player prefab `FpsSoloPlayerController` + `FpsSoloCharacter`, `NeoSceneManager` for scene flow, `NeoSerializedGameObject`/`INeoSerializableComponent` for save, NeoFPS modular firearms, `PoolManager`/`PooledObject` for enemies & pickups, NeoFPS health/inventory/ammo/surfaces. First-party code layers roguelite/level-gen/AI on top.

3. **ScriptableObject event system.** `GameEvent` / `ParameterizedGameEvent<T>` / `AchievementUnlockedEvent` decouple producers (game mode, stats) from consumers (UI, achievements, story). Also extensive use of plain C# `event`s (`onLevelComplete`, `onPortalEntered`, `onEnemySpawned`, nanobot events) and `UnityEvent`s on components.

4. **Static singleton managers.** `GameStatsManager.Instance`, `AIDirector.Instance`, `RogueLiteManager` (static), `MusicManager.Instance`, `StoryManager.Instance`, `AudioManager.Instance` — global access points. (See §8 for maintainability notes.)

5. **Manager + data-model split.** `RogueLiteManager` (manager) ↔ `RogueLitePersistentData`/`RogueLiteRunData` (plain serializable data); `RecipeManager` (static registry) ↔ `IRecipe` ScriptableObjects; `GameStatsManager` ↔ `GameStat<T>` assets.

6. **Command pattern (console).** `RegisterCommandAttribute` + reflection registration into `CommandShell`; commands as static methods with `CommandArg[]`. Extensible by any assembly (game, game-state, dev).

7. **Reflection-driven direction pipeline (Ink).** `AbstractDirection` subclasses auto-discovered at `StoryManager.Start()`; story lines with `>>>` dispatch to direction instances — a plugin point for narrative scripting.

8. **Wave Function Collapse (procedural generation).** Constraint-propagation with lowest-entropy selection and weighted random collapse, driven entirely by ScriptableObject constraints (§5.2).

9. **Weighted random selection.** `WeightedRandom<T>` used by recipe offers, wave enemy selection, and tile collapse.

10. **Assembly-definition module boundaries.** First-party code is split into runtime/editor/commands/marketing assemblies with explicit references (§3), which isolates editor & dev tooling from runtime code.

---

## 7. Third-Party Dependencies & Their Role

| Dependency | Role in Rogue Wave |
|---|---|
| **NeoFPS** (`Assets/NeoFPS/`) | Core FPS framework: player controller/character (`FpsSoloPlayerController`, `FpsSoloCharacter`), game mode base (`FpsSoloGameCustomisable`), scene manager (`NeoSceneManager`), spawn zones & loadout builder, inventory/health/ammo, modular firearms, `PoolManager`/`PooledObject`, damage handlers, HUD base classes, sample UI. |
| **NeoSaveGames** (`Assets/NeoFPS/Core/NeoSaveGames/`) | Scene/world state serialization (`NeoSerializedGameObject`, `INeoSerializableComponent`) for saving level/spawn state. |
| **Ink / ink-unity-integration** (`Assets/Wizards Code/Story Teller/ink-unity-integration/`) | Narrative scripting: compiles `.ink` → JSON at edit time; `Ink.Runtime.Story` runs at runtime; drives tutorial/campaign story. |
| **Steamworks.NET** (`com.rlabrecque.steamworks.net`) | Raw Steam API binding. |
| **Heathen Steamworks** (`com.heathen.steamworkscomplete`) | Higher-level Steam integration (settings, stats & achievements client, screenshots) used by `GameStatsManager`/`SteamworksController` when `STEAMWORKS_ENABLED`. |
| **Lumpn.Discord** (`Assets/Plugins/unity-discord-main`, submodule) | Discord webhook telemetry (stats/exception reports) when `DISCORD_ENABLED`. |
| **Feel (MoreMountains)** (`Assets/Third Party/Feel/`) | Juice/feedback (`MMF_LineEffect` for line FX, MMTools utilities). |
| **ProceduralToolkit** (`com.syomus.proceduraltoolkit`) | Procedural mesh generation for buildings (façades/roofs). |
| **NaughtyAttributes** (`com.dbrizov.naughtyattributes`) | Editor GUI attributes (Foldout, BoxGroup, ShowIf, Button, Expandable, CurveRange…) used throughout. |
| **TextMeshPro** | UI text (dialogue, HUD, terminal). |
| **Excelsior.UI** (`Assets/Third Party/Excelsior/CSFHI/`) | Sci-Fi holo interface assets for UI. |
| **ProTips** (`Assets/Third Party/ProTips/`) | Tooltip system (recipe tooltips). |
| **SineVFX ForceFieldEffects** | Force-field shield visuals (spawner shields). |
| **KriptoFX Realistic Effects Pack** | VFX (explosions, magic effects). |
| **Kronnect TunnelFX** | Tunnel/warp VFX (portals?). |
| **ElevenLabs** (`com.rest.elevenlabs`) | Voice generation for announcer/actors (SpeechEditorWindow). |
| **SuperUnityBuild** (`com.github.superunitybuild.*`) | Multi-platform build pipeline (Development/Release settings assets). |
| **DoubTech ProjectAssetManager** | Asset management/organization. |
| **Unity Recorder** (`com.unity.recorder`) | Showcase capture (marketing GIFs). |
| **Meshy** (`ai.meshy`, in Packages) | AI mesh generation (assets). |
| **ProBuilder / Visual Scripting / Timeline / PostProcessing** | Unity standard packages used in authoring/scenes. |

---

## 8. Observations, Risks & Recommendations

These are **notes** — maintainability observations and recommendations, not a target-state design. They were surfaced to guide future refactoring and onboarding. (Where warranted, they are tracked as separate work items rather than fixed here.)

### 8.1 God-class: `RogueWaveGameMode` (992 lines)

`RogueWaveGameMode` (`Assets/_Rogue Wave/Runtime/Game Management/RogueWaveGameMode.cs`) is the central orchestrator and does far more than "game mode": spawning, loadout configuration, victory/death/extraction coroutines, stats logging, HUD updates, spawner/portal/enemy registration, save triggers, and campaign access. It reaches into `LevelGenerator`, `AIDirector`, `NanobotManager`, `GameStatsManager`, `MusicManager`, `AudioManager`, `RogueLiteManager`, `MovementUpgradeManager`, and UI controllers.

- **Risk:** high coupling; hard to test; every new feature tends to touch it; long coroutines with duplicated logic.
- **Recommendation:** split into focused collaborators: a `RunFlowController` (victory/death state machine), a `LoadoutController` (build loadout from RunData), a `LevelDirector` (spawner/portal/enemy registration), and a `StatsReporter` (game-log/stat wiring). Keep `RogueWaveGameMode` as a thin composition root.

### 8.2 Duplicated victory coroutines

`DelayedLevelTimerAchievedCoroutine`, `DelayedLevelClearedCoroutine`, and `DelayedLevelCompleteCoroutine` share large amounts of near-identical logic (extraction FX, magnet buff, `currentGameLevel++`, victory stat, victory action).

- **Risk:** fixing a bug in one path (e.g. the magnet restore) can be missed in the others; inconsistent behaviour across victory types.
- **Recommendation:** extract a single `LevelCompletedCoroutine(extractionType, victoryAction)` parameterized by extraction type.

### 8.3 Hard-coded stat keys

Stat keys are string literals throughout the codebase: `"RESOURCES"` (in `RogueWaveGameMode.cs`, `NanobotManager.cs`, `RecipeManager.cs`, `ResourceCommands.cs`, `HubController.cs`), `"TOTAL_TIME_IN_RUNS"`, `"RUNS_STARTED"`, `"RUNS_COMPLETED"`, `"DEATH_COUNT"`, `"MAX_NANOBOT_LEVEL"`, `"RESOURCES_SPENT_IN_RUNS"`, `"RUN_LOG"` (in `GameStatsManager.GetDataAsYAML`). The code itself contains `TODO: Remove hard coding of resource stat key` comments.

- **Risk:** typos silently create null stats; renaming a stat asset breaks logic without a compile error.
- **Recommendation:** centralize stat keys in a constants class or a stat-key ScriptableObject catalog; add editor validation for key references.

### 8.4 No first-party unit tests

There are no first-party tests (only third-party package tests under `Library/PackageCache`). All game logic — including pure logic like `RogueLitePersistentData.Add`, `RecipeManager.GetOffers`, `WeightedRandom`, `WfcDefinition.challengeRating`, `LevelGenerator.isValidateLevel` — is untested.

- **Risk:** regressions in progression/economy logic go undetected; refactoring (see 8.1/8.2) is risky.
- **Recommendation:** add a Unity Test Framework (EditMode) test assembly for pure logic (recipe stacking, offers, persistent data serialization, weighted random, WFC validation), starting with the highest-value, most-bug-prone paths.

### 8.5 No CI

No `.github/workflows` exist (only issue templates). There is no automated build, test, or validation on commit.

- **Risk:** regressions ship silently; the README's documented "revert after asset import" workflow is manual.
- **Recommendation:** add a CI workflow that runs Unity in batch mode: compile check (both platforms), run EditMode tests, run the recipe/level data validation (`DataValidation.ValidateRecipes`), and optionally a smoke-play via a dev scene.

### 8.6 Static singleton discovery

Managers are found via `FindObjectOfType`/`FindFirstObjectByType` in `Instance` getters (`GameStatsManager`, `AIDirector`, `StoryManager`, `MusicManager`) with auto-creation fallbacks, and RogueWaveGameMode lazily caches `FindObjectOfType<CampaignManager>()`, `FindAnyObjectByType<AIDirector>()`, `FindObjectOfType<HudGameStatusController>()`.

- **Risk:** hidden scene dependencies; multiple managers in a scene → duplicates destroyed non-deterministically; auto-created instances may be missing serialized config.
- **Recommendation:** prefer explicit scene references / service registration; validate singleton presence in scene at editor time (extend `DataValidation`).

### 8.7 Performance notes

- `WaveFunctionCollapse()` re-scans the full grid and recurses per collapse — O(cells × grid) candidate work; fine at current map sizes but the natural growth path is a priority-queue/entropy-heap implementation.
- `GameStatsManager.GetIntStat/GetStringStat` are linear scans called per-frame in hot paths (e.g. `LogGameState`); `GetDataAsYAML` rebuilds everything on every webhook send. A `Dictionary<string, stat>` cache is an easy win.
- `RecipeManager.GetOfferCandidates` logs a `Debug.Log` per call in editor and explicitly TODO's caching.
- `AIDirector.GetNearbySpawners` re-sorts spawners per call (TODO comment present).
- `StoryManager.FindTarget` uses `GameObject.Find` at runtime (TODO comment present).

### 8.8 Data-model hygiene

- `RogueLitePersistentData.RecipeIds` is a public mutable `List<string>` marked `[Obsolete]` with the note "should not be exposed publicly"; `_weaponBuildOrderBackingField` is public only to ensure serialization ("TODO: write a custom serialiser for this class").
- `RogueLiteRunData` keeps `isDirty` but most setters don't auto-set it ("TODO: Need to wrap values above to automate setting this on change").
- `isDirty` propagation is manual and was the source of several safety-net additions (e.g. setting `isDirty = true` "as a security in case we forgot").
- **Recommendation:** encapsulate collections, add a custom JSON serializer, and centralize dirty-flagging.

### 8.9 Dead### 8.9 Dead / commented-out code

Large blocks of commented-out code remain in active files: `RogueWaveGameMode` (commented resource-grant, `SceneSetupCommands` usage), `NanobotManager` (`TryAllAmmoRecipes`), `WaveDefinition` (editor-only), `StoryManager` (commented Cinemachine/UI discovery), `RogueLiteManager` (editor-only profile creation), `GameStatsManager` (commented embed code). These create confusion about intended behaviour and drift from the actual code.

- **Recommendation:** delete commented-out blocks (git history preserves them); where the intent is uncertain, track it as a work item.

### 8.10 Duplicated logic across managers

Recipe add/stack logic and weapon-build-order insertion are duplicated between `RogueLitePersistentData.Add` and `RogueLiteRunData.Add` (the code contains an explicit `REFACTOR: this code is a duplicate of code in the persistent data class` comment). `NanobotManager.AddToRunRecipes` uses a long if/else dispatch (with its own `TODO: This is messy` comment) rather than polymorphism on `AbstractRecipe`.

- **Recommendation:** move stacking/build-order rules into a single `RecipeCollection` type; dispatch via virtual methods on `AbstractRecipe` (e.g. `OnAddedToRun(NanobotManager)`).

### 8.11 Scene/prefab wiring is invisible to static analysis

Because there is no Unity editor in the analysis environment, runtime wiring that exists only in scenes/prefabs (which component instances are present, serialized references, event hooks set in the inspector, `GameEvent` asset wiring) could not be fully verified. The document labels such wiring as *inferred* where relevant.

- **Risk:** scene-bound configuration may differ from what the code suggests.
- **Recommendation:** when the editor is next available, validate this document's scene/flow section against the actual scene contents; consider adding editor-time scene validation to `DataValidation`.

### 8.12 Environment/build notes

- Git LFS was required for some assets; binary assets (audio, textures, models) were not inspected. Third-party license boundaries were respected — only interface/roles are described here.
- The `wl sync` failure mentioned in the work item's risk section was resolved (git-lfs installed); no impact on this analysis.

---

## 9. Further Reading / File Index

This section indexes the most important files by subsystem for quick navigation. All paths are relative to the repo root.

### Game mode & flow
- `Assets/_Rogue Wave/Runtime/Game Management/RogueWaveGameMode.cs` — orchestrator (see §8.1)
- `Assets/_Rogue Wave/Runtime/Game Management/ScenarioManager.cs` — scenario playback
- `Assets/_Rogue Wave/Runtime/Game Management/NameGenerator.cs` — random names
- `Assets/_Rogue Wave/Runtime/Common/SceneManagement.cs` — scene name/index helpers

### Roguelite
- `Assets/_Rogue Wave/Runtime/Roguelite/RogueLiteManager.cs` — manager, profiles, save/load
- `Assets/_Rogue Wave/Runtime/Roguelite/RogueLitePersistentData.cs` — permanent data model
- `Assets/_Rogue Wave/Runtime/Roguelite/RogueLiteRunData.cs` — per-run data model
- `Assets/_Rogue Wave/Runtime/Roguelite/CampaignManager.cs` — campaign sequencing
- `Assets/_Rogue Wave/Runtime/Roguelite/CustomRogueLiteData.cs` — custom data injection
- `Assets/_Rogue Wave/Runtime/Roguelite/Recipe/` — full recipe system (`AbstractRecipe`, `IRecipe`, `RecipeManager`, `GenericItemRecipe<T>`, build/stat/weapon-upgrade/passive recipe subfolders)
- `Assets/_Rogue Wave/Runtime/Nanobots/NanobotManager.cs` — in-run economy/build AI
- `Assets/_Rogue Wave/Runtime/Nanobots/MagnetController.cs`, `ResourcesPickup.cs` — resource collection
- `Assets/_Rogue Wave/Runtime/Levels/CampaignDefinition.cs` — campaign asset

### Level generation
- `Assets/_Rogue Wave/Runtime/Levels/LevelGenerator.cs` — WFC engine
- `Assets/_Rogue Wave/Runtime/Levels/WfcDefinition.cs` — level asset + `LevelWaveGenerationConfiguration`
- `Assets/_Rogue Wave/Runtime/Levels/Tiles/TileDefinition.cs` — tile asset, constraints
- `Assets/_Rogue Wave/Runtime/Levels/Tiles/` — tile behaviours (BaseTile, ConnectedTile, MultiCellTile, SpawnerTile, PlayerSpawnTile, …)
- `Assets/_Rogue Wave/Runtime/Levels/Building/` — procedural building system
- `Assets/_Rogue Wave/Runtime/Levels/DestructibleController.cs`, `CrystalPatch.cs`, `PortalController.cs`, `ScenarioDescriptor.cs`
- `Assets/_Rogue Wave/Resources/Levels/` — authored level/campaign assets

### Enemies & AI
- `Assets/_Rogue Wave/Runtime/Enemies/AIDirector.cs` — pressure/squad director
- `Assets/_Rogue Wave/Runtime/Enemies/BasicEnemyController.cs` — enemy base
- `Assets/_Rogue Wave/Runtime/Enemies/BasicMovementController.cs`, `ScannerController.cs`, `WaitAndLungeEnemyController.cs`, `EnemyFirearmController.cs`, `EnemyDissolve.cs`, `ShieldDamageHandler.cs`
- `Assets/_Rogue Wave/Runtime/Levels/Spawner.cs` — enemy spawner (also a BasicEnemyController)
- `Assets/_Rogue Wave/Runtime/Levels/WaveDefinition.cs` — wave asset + balancing

### Combat & weapons
- `Assets/_Rogue Wave/Runtime/FX/BasicWeaponController.cs` — enemy weapon controller
- `Assets/_Rogue Wave/Runtime/FX/Behaviours/` — firing/targeting behaviours
- `Assets/_Rogue Wave/Runtime/Weapons/` — projectiles, passive weapons, ammo effects, explosions
- `Assets/_Rogue Wave/Runtime/Damage/` — damage handlers (`PlayerDamageHandler`, `BasicEnemyDamageHandler`, `RWPooledExplosion`)
- `Assets/_Rogue Wave/Runtime/Upgrades/` — `MovementUpgradeManager`, `FloatValueModifier`
- `Assets/_Rogue Wave/Runtime/NeoFpsExtensions.cs` — NeoFPS extension helpers

### Stats, achievements, telemetry (Game State)
- `Assets/Wizards Code/Game State/Runtime/Stats/GameStatsManager.cs`
- `Assets/Wizards Code/Game State/Runtime/Stats/GameStat.cs`, `IntGameStat.cs`, `StringGameStat.cs`, `IGameStat.cs`
- `Assets/Wizards Code/Game State/Runtime/Stats/Achievement.cs`, `IAchievement.cs`, `GameLog.cs`
- `Assets/Wizards Code/Game State/Runtime/Events/` — ScriptableObject event system
- `Assets/Wizards Code/Game State/Runtime/Steamworks/SteamworksController.cs`
- `Assets/Wizards Code/Game State/Runtime/FPSCounter.cs`
- `Assets/Wizards Code/Game State/Runtime/CommandTerminal/` — stats/steam commands

### Story Teller (Ink)
- `Assets/Wizards Code/Story Teller/Runtime/StoryManager.cs` — Ink runtime + directions + UI
- `Assets/Wizards Code/Story Teller/Runtime/Directions/` — built-in directions
- `Assets/Wizards Code/Story Teller/Runtime/Actors/` — actor cues
- `Assets/Wizards Code/Story Teller/Runtime/UI/` — story text/UI management
- `Assets/_Rogue Wave/Runtime/Story/` — RW-specific directions
- `Assets/_Rogue Wave/Resources/Story/Tutorial.ink` — tutorial story

### Command Terminal
- `Assets/Wizards Code/Command Terminal/Runtime/` — framework (`Terminal`, `CommandShell`, `RegisterCommandAttribute`, commands)
- `Assets/_Rogue Wave/Runtime/DevTools/Command Terminal/` — game commands (generic + in-game)
- `Assets/_Dev/Runtime/` — dev commands

### UI / HUD
- `Assets/_Rogue Wave/Runtime/UI/` — menus, hub/recipe selector, tabs
- `Assets/_Rogue Wave/Runtime/UI/Hud/` — in-combat HUD
- `Assets/_Rogue Wave/Runtime/UI/Stats/` — achievements/stats screens
- `Assets/_Rogue Wave/Runtime/UI/Recipe Selector/` — hub screens
- `Assets/_Rogue Wave/Runtime/Map/LevelUiController.cs` — level selection UI

### Audio
- `Assets/_Rogue Wave/Runtime/Audio/AudioManager.cs`, `MusicManager.cs`, `PlayerAudioManager.cs`, `SavWav.cs`

### Editor & dev tools
- `Assets/_Rogue Wave/Editor/` — data windows, CSV, validation, speech editor
- `Assets/_Dev/` — dev scenes, wave generator window, dev commands
- `Assets/_Marketing/` — marketing scenes and showcase capture

### Config / tech-stack sources of truth
- `Packages/manifest.json` — package dependencies
- `ProjectSettings/ProjectVersion.txt` — Unity version
- `.gitmodules` — submodules (ink-unity-integration, UnityActionHub, unity-discord)
- `README.md` — install instructions, third-party asset list, Discord/Steam defines
- `Assets/_Rogue Wave/Runtime/org.wizardscode.roguewave.asmdef` and the other asmdefs listed in §3

---

*Documented from static analysis of commit `0bf61611` (dev). Verified file paths and class names against the repository at that commit. Runtime-only wiring is labelled inferred where it could not be confirmed without the Unity editor.*
