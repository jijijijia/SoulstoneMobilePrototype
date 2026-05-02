# Data-Driven Migration Guide

## Goal

Move the prototype from hardcoded MonoBehaviour logic to a data-driven architecture built around:

- `CharacterData`
- `WeaponData`
- `SkillData`
- `UpgradeData`
- `EnemyData`

and runtime systems:

- `CharacterSystem`
- `WeaponSystem`
- `SkillSystem`
- `UpgradeSystem`
- `ProgressionSystem`
- `SpawnSystem`
- `BossManager`

## Added Code

New architecture lives in:

- `Assets/Scripts/DataDriven/Core`
- `Assets/Scripts/DataDriven/Data`
- `Assets/Scripts/DataDriven/Runtime`
- `Assets/Scripts/DataDriven/Skills`
- `Assets/Scripts/DataDriven/Systems`

## Migration Order

1. Create data assets for one playable character, one weapon, three skills, two upgrades, and two enemies.
2. Build a new player prefab based on `CharacterSystem` and `DataDrivenPlayerController`.
3. Build new enemy prefabs using `EnemyAgent`.
4. Create a new scene object with `SpawnSystem`.
5. Hook level-up UI to `ProgressionSystem.GenerateChoices()`.
6. Hook HUD to `CharacterSystem` health and `ProgressionSystem` XP/level events.
7. After the new flow works, remove old scripts from the scene incrementally.

## Minimum First Playable Setup

### Character

Create one `CharacterData` asset with:

- base health
- move speed
- damage
- pickup radius
- starting weapon
- available weapons
- global upgrade pool

### Weapon

Create one `WeaponData` asset with:

- starting projectile skill
- unique skill pool: projectile, orbiting blades, shockwave

### Skills

Create `SkillData` assets:

1. `Projectile`
   - behavior: `Projectile`
   - parameters:
     - `interval`
     - `range`
     - `projectileSpeed`
     - `projectileLifetime`
     - `damage`

2. `Orbiting Blades`
   - behavior: `OrbitingBlades`
   - parameters:
     - `orbitRadius`
     - `orbitSpeed`
     - `bladeScale`
     - `bladeHeight`
     - `hitCooldown`
     - `bladeCount`
     - `damage`

3. `Shockwave`
   - behavior: `Shockwave`
   - parameters:
     - `interval`
     - `radius`
     - `damage`

### Upgrades

Create `UpgradeData` assets for:

- move speed
- max health
- damage
- cooldown
- pickup radius

### Enemies

Create `EnemyData` assets for:

- basic enemy
- fast enemy
- tank enemy
- boss

Each enemy should define:

- prefab
- category
- spawn cost
- spawn weight
- unlock player level
- kill requirement if needed
- experience reward
- base stats

## Scene Wiring

### Player

Add to player object:

- `CharacterController`
- `CharacterSystem`
- `DataDrivenPlayerController`

Assign the `CharacterData` asset on `CharacterSystem`.

### Spawn

Create a scene object `SpawnSystem` and assign:

- player
- enemy pool
- map bounds
- spawn budget parameters

### Boss

Create a scene object `BossManager` and assign:

- `SpawnSystem`
- boss enemy data list
- kills per boss

## UI Migration

Current UI can stay temporarily.

Next UI refactor should:

- read HP from `CharacterSystem.HealthChanged`
- read XP and level from `ProgressionSystem`
- request level-up choices through `ProgressionSystem.GenerateChoices()`
- apply selected choice via `ProgressionChoice.Apply()`

## Important Note

The new architecture is added alongside the old prototype intentionally.
Do not delete the old scripts first.
Migrate one system at a time and only remove old code after the new path is proven in-scene.
