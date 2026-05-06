# Technical GDD: Soulstone Prototype

Документ составлен по текущему состоянию Unity-проекта `SoulstoneMobilePrototype`.

Дата анализа: 2026-05-03  
Unity: 6000.0.46f1 по контексту проекта  
Проект: 3D action / survivors-like / roguelite prototype

Важно: документ описывает то, что найдено в текущих сценах, скриптах, ScriptableObject, prefab и структуре проекта. Если механика видна по архитектуре, но не завершена, она помечена как частично реализованная или предполагаемая.

# 1. Общее описание игры

## Название проекта

Рабочее название по меню и ассетам: `Soulstone Prototype`.

## Жанр

3D survivors-like / bullet heaven / action roguelite.

## Платформа

Основной целевой вектор по контексту разработки: мобильные устройства.  
Проект также тестируется в Unity Editor / PC.

В проекте присутствуют настройки рендера:

- `Assets/Settings/Mobile_RPAsset.asset`
- `Assets/Settings/Mobile_Renderer.asset`
- `Assets/Settings/PC_RPAsset.asset`
- `Assets/Settings/PC_Renderer.asset`

## Камера и перспектива

В бою используется 3D top-down / angled camera.  
Камера следует за игроком через `CameraFollow`.

Связанные файлы:

- `Assets/Scripts/Core/CameraFollow.cs`
- `Assets/Scenes/main.unity`

В главном меню камера смотрит на 3D-комнату и выбранного героя.

## Краткое описание игрового опыта

Игрок выбирает персонажа, оружие и карту, затем начинает забег на арене. Персонаж управляется вручную, а активные навыки применяются автоматически по кулдауну. Враги массово появляются вокруг игрока, преследуют его, атакуют в ближнем и дальнем бою, умирают и оставляют опыт. При повышении уровня игрок выбирает один из трёх активных навыков или один из трёх апгрейдов.

## Игры-референсы

Очевидные референсы по механикам:

- `Soulstone Survivors`
- `Vampire Survivors`

## Целевая аудитория

Игроки, которым нравятся:

- короткие и насыщенные забеги;
- автоматические атаки;
- построение билда во время боя;
- большое количество врагов на экране;
- средневековое фэнтези;
- roguelite-прогрессия.

# 2. Core Gameplay Loop

## Основной игровой цикл

1. Игрок запускает игру.
2. Загружается сцена `MainMenu`.
3. Игрок выбирает персонажа.
4. Игрок выбирает оружие.
5. Игрок выбирает карту.
6. Выбор сохраняется в профиль.
7. Игрок нажимает кнопку запуска боя.
8. Загружается сцена `main`.
9. `DDRunManager` применяет выбранную карту.
10. Игрок появляется в spawn point карты.
11. `CharacterSystem` инициализирует выбранного героя.
12. `WeaponSystem` экипирует выбранное оружие.
13. `SkillSystem` добавляет стартовый активный навык оружия.
14. `SpawnSystem` начинает спавнить врагов.
15. Игрок двигается по арене.
16. Активные навыки автоматически применяются по кулдауну.
17. Враги получают урон и умирают.
18. Из врагов выпадает опыт.
19. Игрок собирает опыт через радиус подбора.
20. При достижении нового уровня открывается окно выбора.
21. Игра ставится на паузу.
22. Игрок выбирает 1 из 3 навыков или 1 из 3 улучшений.
23. Игра продолжается.
24. Со временем сложность спавна и параметры врагов растут.
25. Появляются элитные враги / мини-боссы / боссы.
26. При смерти игрока показывается Game Over.
27. Игрок может перезапустить забег.
28. Возврат в меню предполагается, но полноценный flow наград и возвращения требует уточнения.

## Связанные системы

- `MainMenuManager`
- `SelectedCharacterStore`
- `SelectedLoadoutStore`
- `GameProfileStore`
- `DDRunManager`
- `CharacterSystem`
- `WeaponSystem`
- `SkillSystem`
- `SpawnSystem`
- `EnemyAgent`
- `ProgressionSystem`
- `DDLevelUpManager`
- `DDHUDManager`

# 3. Игровые режимы

## Найденные режимы

| Режим | Статус | Описание |
|---|---|---|
| Survival Run | Частично реализовано | Основной забег на арене с ростом сложности, спавном врагов, опытом, уровнями и Game Over. |
| Main Menu | Реализовано частично | Главное меню с выбором карты, персонажа, оружия, настройками и прототипом дерева навыков. |
| Meta Progression | Предполагается / частично | Есть экран дерева навыков, но полноценная мета-прогрессия не реализована. |

## Условия старта

- Выбран персонаж.
- Выбрано оружие.
- Выбрана карта.
- Загружена сцена `main`.
- `DDRunManager.beginRunOnStart = true`.

## Условия победы

Требует уточнения.  
В `SpawnTimelineData` есть `runDuration`, но отдельная система победы по завершению времени не найдена.

## Условия поражения

Игрок умирает, если его здоровье достигает 0 и не срабатывает `LifeTotemCount`.

Связанные классы:

- `CharacterSystem`
- `CharacterRuntimeState`
- `DDRunManager`

## Длительность сессии

`SpawnTimelineData` содержит `runDuration = 600f`, что указывает на предполагаемый 10-минутный забег. Требует подтверждения.

# 4. Игрок и управление

## Объект игрока

В сцене `main` объект игрока называется `DD_Player`.

## Основные компоненты игрока

- `CharacterController`
- `CharacterSystem`
- `DataDrivenPlayerController`
- `WeaponSystem`
- `SkillSystem`
- `UpgradeSystem`
- `ProgressionSystem`
- `StatusController`

## Движение

Движение реализовано в `DataDrivenPlayerController`.

Особенности:

- управление через движение персонажа;
- `CharacterController`;
- gravity;
- dash через клавишу `Space`;
- несколько зарядов рывка;
- время восстановления рывка;
- временная неуязвимость во время рывка.

## Камера

Камера следует за игроком через `CameraFollow`.

## Характеристики игрока

Основные `StatType`, связанные с игроком:

- `MaxHealth`
- `MoveSpeed`
- `PickupRadius`
- `Defense`
- `ParryChance`
- `ExperienceMultiplier`
- `DashCharges`
- `HealthRegenPercent`
- `LifeTotemCount`
- `AttackPower`
- `SkillFrequency`
- `AreaMultiplier`
- `DoubleAttackChance`
- `CritChance`
- `CritMultiplier`

## Расчёт итоговых характеристик

Расчёт идёт через `RuntimeStats`.

Формула:

```text
finalStat = (baseValue + additiveModifiers) * multiplierModifiers
```

## Получение урона

`CharacterSystem.TakeDamage`:

1. Проверяет неуязвимость.
2. Проверяет смерть.
3. Проверяет шанс парирования.
4. Рассчитывает входящий урон через защиту.
5. Передаёт урон в `CharacterRuntimeState`.
6. Отправляет событие в `CombatFeedbackEvents`.

## Смерть

Если здоровье достигает 0:

- если доступен `LifeTotemCount`, персонаж восстанавливается;
- иначе вызывается событие `Died`;
- `DDRunManager` переводит игру в `GameOver`.

## Взаимодействие с опытом

`ExperienceGem` проверяет расстояние до игрока. Если игрок входит в радиус `PickupRadius`, гем добавляет опыт через `ProgressionSystem.AddExperience`.

# 5. Персонажи

## Хранение данных

Класс: `CharacterData`.

Поля:

- `characterId`
- `displayName`
- `modelPrefab`
- `baseStats`
- `startingWeapon`
- `availableWeapons`
- `globalUpgradePool`

## Выбор персонажа

Система выбора:

- UI: `MainMenuManager`
- список: `CharacterRoster`
- сохранение: `SelectedCharacterStore`
- профили: `GameProfileStore`

## Персонажи в проекте

| Asset | Статус |
|---|---|
| `Hero_Default` | Есть |
| `Hero_Viking` | Есть |
| `Hero_Assassin` | Есть |
| `Hero_Knight` | Есть |
| `Hero_Necromancer` | Есть |
| `Hero_Paladin` | Есть |
| `Hero_Pyromancer` | Есть |
| `Hero_Ranger` | Есть |
| `Hero_RuneSmith` | Есть |
| `Hero_StormSentinel` | Есть |

## Уникальность персонажей

Сейчас уникальность выражается через:

- разные базовые статы;
- разное стартовое оружие;
- разные доступные оружия;
- разные пулы улучшений;
- стартовый навык оружия.

Отдельная система уникальных пассивок персонажа не выделена явно. Это требует уточнения.

## Разблокировка

Система разблокировки персонажей не найдена. Предполагается как часть будущей мета-прогрессии.

# 6. Оружие

## Хранение данных

Класс: `WeaponData`.

Поля:

- `weaponId`
- `displayName`
- `icon`
- `startingSkill`
- `uniqueSkillPool`

## Связь с персонажем

`CharacterData` содержит:

- `startingWeapon`
- `availableWeapons`

## Стартовый навык

При экипировке оружия `WeaponSystem` регистрирует пул навыков и добавляет `startingSkill` как locked skill.

## Оружие в проекте

| Asset | Статус |
|---|---|
| `Weapon_Starter` | Есть |
| `Weapon_VikingAxe` | Есть |
| `Weapon_AssassinBlades` | Есть |
| `Weapon_KnightSword` | Есть |
| `Weapon_BoneScepter` | Есть |
| `Weapon_FlameStaff` | Есть |
| `Weapon_HolyHammer` | Есть |
| `Weapon_Longbow` | Есть |
| `Weapon_RuneHammer` | Есть |
| `Weapon_StormSigil` | Есть |

## Как оружие влияет на бой

Оружие само не наносит урон напрямую. Оно задаёт:

- стартовый активный навык;
- пул навыков, которые могут выпадать при повышении уровня.

# 7. Навыки и способности

## Хранение навыков

Класс: `SkillData`.

Поля:

- `skillId`
- `displayName`
- `description`
- `icon`
- `hitType`
- `element`
- `runtimeDefinition`
- `visualPrefab`
- `baseDamage`
- `flatDamageBonus`
- `critChance`
- `critMultiplier`
- `areaBonus`
- `cooldown`
- `cooldownCoefficient`
- `cooldownDivider`
- `areaMultiplier`
- `repeatChance`
- `globalSkill`
- `maxRank`
- `tags`
- `appliedStatuses`
- `configurableAttacks`
- `attackDefinitions`
- `parameters`

## Основная система атак

Новая система построена как конструктор:

```text
SkillData
  -> SkillRuntimeDefinition
  -> AttackDefinition[]
      -> AttackTargetingDefinition
      -> AttackDeliveryDefinition
      -> AttackImpactDefinition[]
```

## Типы targeting

- `NearestEnemyTargetingDefinition`
- `RandomEnemyTargetingDefinition`
- `SelfPositionTargetingDefinition`

## Типы delivery

- `ProjectileAttackDeliveryDefinition`
- `ProjectileSpreadAttackDeliveryDefinition`
- `AreaPulseAttackDeliveryDefinition`
- `DelayedAreaAttackDeliveryDefinition`
- `LastingAreaAttackDeliveryDefinition`
- `ChainAttackDeliveryDefinition`
- `BeamAttackDeliveryDefinition`
- `CircularMeleeAttackDeliveryDefinition`
- `CircularSweepAttackDeliveryDefinition`
- `FrontalConeAttackDeliveryDefinition`
- `LineSlashAttackDeliveryDefinition`
- `OrbitalAttackDeliveryDefinition`
- `SummonAttackDeliveryDefinition`

## Типы impact

- `DamageAttackImpactDefinition`
- `StatusAttackImpactDefinition`

## Runtime-объекты атак

- `ModularProjectile`
- `RuntimeCircularSweepHitbox`
- `RuntimeDamageZone`
- `RuntimeDelayedAreaStrike`
- `RuntimeOrbitalAttack`
- `RuntimeSummonedMinion`
- `RuntimeTimedDestroy`

## Активные навыки

Активные навыки автоматически применяются по кулдауну через `ModularAttackSkill`.

## Пассивные навыки

Отдельного класса пассивных навыков нет. Пассивные эффекты представлены как `UpgradeData`.

## Выбор навыков при повышении уровня

`ProgressionChoiceGenerator`:

- генерирует 3 активных навыка;
- или генерирует 3 улучшения;
- если доступны оба типа, случайно выбирает тип набора;
- смешанный набор навык + улучшение одновременно не должен появляться.

## Лимит активных навыков

В `SkillSystem`:

```text
MaxActiveSkills = 6
```

## Что происходит при достижении лимита

Если лимит активных навыков достигнут и не задан replacement skill, новый навык не берётся. Возможность замены предусмотрена кодом, но UX замены требует уточнения.

## Навыки в проекте

| Навык | Статус |
|---|---|
| `Skill_Armageddon` | Реализован через AttackDefinition |
| `Skill_ArrowRain` | Реализован через AttackDefinition |
| `Skill_AssassinKnifeStorm` | Реализован через AttackDefinition |
| `Skill_AxeCleave` | Реализован через AttackDefinition |
| `Skill_BoulderThrow` | Реализован через AttackDefinition |
| `Skill_ChainLightning` | Реализован через AttackDefinition |
| `Skill_DoubleCleave` | Реализован через AttackDefinition |
| `Skill_FireRain` | Реализован через AttackDefinition |
| `Skill_HolyHammer` | Реализован через AttackDefinition |
| `Skill_KnifeFan` | Реализован через AttackDefinition |
| `Skill_KnightSwordArc` | Реализован через AttackDefinition |
| `Skill_LightningSeries` | Реализован через AttackDefinition |
| `Skill_PoisonMire` | Реализован через AttackDefinition |
| `Skill_RaiseDead` | Реализован через AttackDefinition |
| `Skill_RuneQuake` | Реализован через AttackDefinition |
| `Skill_SpearVolley` | Реализован через AttackDefinition |
| `Skill_StoneSpear` | Реализован через AttackDefinition |
| `Skill_StormCall` | Реализован через AttackDefinition |
| `Skill_VikingWhirl` | Реализован через AttackDefinition |
| `Skill_OrbitingBlades` | Legacy / требует миграции |
| `Skill_Projectile` | Legacy / требует миграции |
| `Skill_Shockwave` | Legacy / требует миграции |
| `Skill_ToxicVolley` | Legacy / требует миграции |
| `Skill_StormJavelin` | Частично / требует проверки |
| `Skill_Template_Configurable` | Шаблон / требует уточнения использования |

# 8. Боевая система

## Главный класс формул

`CombatMath`.

## Урон навыка

```text
rankedDamage = baseDamage + max(0, rank - 1) * damagePerRank
totalAttackPower = max(0.05, attackPower * ownerDamageMultiplier)
damage = max(1, rankedDamage * totalAttackPower + flatDamageBonus)
```

## Критический урон

```text
if Random <= critChance:
    damage *= max(1, critMultiplier)

finalDamage = round(damage)
```

## Кулдаун

```text
rankedCooldown = max(0.15, baseCooldown - max(0, rank - 1) * cooldownReduction)
legacyMultiplier = cooldownMultiplier > 0 ? max(0.05, cooldownMultiplier) : 1
frequency = max(0.1, skillFrequency)
cooldown = max(0.15, rankedCooldown * legacyMultiplier / frequency)
```

## Область поражения

```text
radius = max(0.05, (baseRadius + flatAreaBonus) * max(0.1, areaMultiplier))
```

## Броня / защита

```text
if armor >= 100:
    damageMultiplier = 2 / ((armor / 100) + 1)
else:
    damageMultiplier = (200 - armor) / 100
```

Защита игрока:

```text
effectiveArmor = 100 + defense
```

## Повторное применение / multicast

`CombatMath.RollExecutionCount` использует каскадный шанс:

```text
stepChance = chance * 0.4^(executionIndex)
maxExecutions = 8
```

## Статусы

Связанные классы:

- `StatusController`
- `StatusEffectData`
- `StatusEffectType`
- `StatusAttackImpactDefinition`

Формула DoT:

```text
tickDamage = potency * stacks
```

## Враги получают урон

Через:

- `EnemyAgent.TakeDamage`
- `EnemyAgent.TakeDamageFromSummon`

## Игрок получает урон

Через:

- `CharacterSystem.TakeDamage`

## Формулы, которые требуют добавления или уточнения

- формула наград после забега;
- формула победы;
- формула стоимости мета-улучшений;
- формула роста XP после высоких уровней;
- точные правила stack limit статусов;
- правила элементальных синергий;
- resistances врагов.

# 9. Враги

## Система врагов

Главный runtime-класс: `EnemyAgent`.

Связанные классы:

- `EnemyData`
- `EnemyAbilityConfig`
- `EnemyMovementController`
- `EnemyAbilityRunner`
- `EnemyProjectileShooter`
- `EnemyProjectile`
- `EnemyRegistry`
- `EnemyVisualProfile`

## Враги в проекте

| Asset | Тип / роль |
|---|---|
| `Enemy_Basic` | базовый враг |
| `Enemy_Fast` | быстрый враг |
| `Enemy_Tank` | танк |
| `Enemy_Elite` | элитный враг |
| `Enemy_MiniBoss` | мини-босс |
| `Enemy_PoisonSpitter` | дальний ядовитый враг |
| `Enemy_FrostShaman` | дальний frost-враг |

## Характеристики врагов

В `EnemyData`:

- `enemyId`
- `displayName`
- `category`
- `prefab`
- `abilityConfig`
- `baseStats`
- `spawnCost`
- `spawnWeight`
- `unlockPlayerLevel`
- `killRequirement`
- `experienceReward`
- `visualScaleMultiplier`
- `tintColor`

## Появление врагов

Враги появляются через `SpawnSystem` и `PoolManager`.

## Выбор цели

`EnemyAgent` выбирает между игроком и призванными существами.

Правила:

- если игрок недавно нанёс прямой урон, враг фокусится на игроке;
- если summon сильно ближе, враг может атаковать summon;
- если игрок ближе summon, враг атакует игрока.

## Движение

`EnemyMovementController`:

- двигает врага к цели;
- учитывает preferred distance;
- обходит препятствия через probe/avoidance angles;
- использует `CharacterController`.

## Атаки врагов

Через `EnemyAbilityRunner` и `EnemyAbilityConfig`.

Типы:

- `MeleeContact`
- `Projectile`
- `AreaPulse`

## Смерть врага

При смерти:

1. Создаётся experience gem.
2. Увеличивается kill count.
3. Враг возвращается в пул.

# 10. Боссы и элитные враги

## Система боссов

Класс: `BossManager`.

## Условия появления

Поддерживаются:

- появление по количеству убийств;
- появление по normalized time через `TimedBossEvent`.

## Категории врагов

`EnemyCategory`:

- Normal
- Empowered
- Elite
- MiniBoss
- Boss

## Множители категории

HP:

| Категория | Множитель |
|---|---:|
| Empowered | 1.35 |
| Elite | 3.5 |
| MiniBoss | 12 |
| Boss | 35 |

Damage:

| Категория | Множитель |
|---|---:|
| Empowered | 1.15 |
| Elite | 1.35 |
| MiniBoss | 1.75 |
| Boss | 2.5 |

## Награды боссов

Отдельная система наград за боссов не найдена. Сейчас босс может давать опыт как обычный враг через `experienceReward`.

# 11. Спавн врагов и сложность

## Главный класс

`SpawnSystem`.

## Где появляются враги

Враги спавнятся на окружности вокруг игрока через `RingSpawnPositionProvider`.

Учитываются:

- радиус спавна;
- направление движения игрока;
- границы карты;
- отступы от края;
- расстояние между точками спавна;
- расстояние от уже активных врагов.

## Выбор типа врага

`EnemySpawnSelector`:

- фильтрует enemy pool;
- учитывает unlock player level;
- учитывает kill requirement;
- выбирает врага по весу.

## Частота появления

Задаётся через `SpawnTimelineData.GetSpawnRate`.

## Рост сложности

Используются:

- `SpawnTimelineData`
- `MapData` multipliers
- `RunAffixData`
- `CombatMath.ResolveEnemyHealthMultiplier`
- `CombatMath.ResolveEnemyDamageMultiplier`

## Волны

Отдельной wave-системы нет. Есть continuous timeline + burst events.

## Параметры для вынесения в настройки

Уже частично вынесены:

- spawn rate;
- pack size;
- max alive;
- health multiplier;
- damage multiplier;
- enemy weights;
- burst events.

Ещё желательно вынести:

- global spawn defaults;
- boss schedule;
- elite schedule;
- safe spawn rules;
- density limits per device tier.

# 12. Опыт, уровни и прогрессия внутри боя

## Выпадение опыта

`EnemyAgent` при смерти вызывает `SpawnSystem.SpawnExperienceGem`.

## Сбор опыта

`ExperienceGem`:

- получает `CharacterSystem` как collector;
- проверяет `PickupRadius`;
- вызывает `ProgressionSystem.AddExperience`;
- возвращается в пул.

## Формула опыта

```text
requiredXP = baseExperienceToNextLevel + max(0, level - 1) * experienceStepPerLevel
```

Текущие значения:

- `baseExperienceToNextLevel = 5`
- `experienceStepPerLevel = 3`

## Множитель опыта

```text
adjustedXP = ceil(amount * ExperienceMultiplier)
```

## Повышение уровня

При накоплении опыта:

- увеличивается `currentLevel`;
- пересчитывается `experienceToNextLevel`;
- вызывается `LevelChanged`;
- вызывается `ExperienceChanged`.

## Окно выбора

`DDLevelUpManager` подписан на `ProgressionSystem.LevelChanged`.

При level up:

- генерируются choices;
- включается `levelUpPanel`;
- `DDRunManager.EnterLevelUpSelection`;
- `Time.timeScale = 0`.

# 13. Прогрессия вне боя

## Статус

Частично реализована / предполагается.

## Есть сейчас

- выбор профиля;
- сохранение выбранного героя;
- сохранение выбранного оружия;
- сохранение выбранной карты;
- экран дерева навыков как UI-заготовка;
- affix-система забега.

## Нет сейчас

- валюта;
- account upgrades;
- дерево навыков как gameplay system;
- unlock героев;
- unlock оружия;
- unlock карт;
- награды после забега.

## Какие данные должны сохраняться

Текущие:

- active profile;
- selected character;
- selected weapon per character;
- selected map;
- settings.

Будущие:

- unlocked characters;
- unlocked weapons;
- unlocked maps;
- meta currency;
- meta upgrades;
- achievements/statistics;
- completed maps;
- best runs.

# 14. Карты и уровни

## Хранение карт

Класс: `MapData`.

Поля:

- `mapId`
- `displayName`
- `difficulty`
- `biome`
- `enemyDescription`
- `rewardsDescription`
- `mapPrefab`
- `spawnTimeline`
- `enemyPool`
- `enemyHealthMultiplier`
- `enemyDamageMultiplier`
- `spawnRateMultiplier`
- `packSizeBonus`
- `maxAliveBonus`
- `playerSpawnPosition`
- `playerSpawnClearRadius`
- `overrideMapBounds`
- `mapMin`
- `mapMax`

## Карты в проекте

| Asset | Статус |
|---|---|
| `Map_ForgottenPlains` | Есть |
| `Map_AshenQuarry` | Есть |
| `Map_VenomMarsh` | Есть |
| `Map_StormSpire` | Есть |
| `Map_BoneCitadel` | Есть |

## Prefab карт

Путь:

```text
Assets/Prefabs/Maps/
```

Prefab:

- `Map_AshenQuarry.prefab`
- `Map_BoneCitadel.prefab`
- `Map_ForgottenPlains.prefab`
- `Map_StormSpire.prefab`
- `Map_VenomMarsh.prefab`

# 15. UI/UX

## Главное меню

Класс: `MainMenuManager`.

Экраны:

- Home
- Play
- Character
- Weapon
- SkillTree
- Settings

## Сцена меню

`Assets/Scenes/MainMenu.unity`

Объекты:

- `MainMenuRoot`
- `Main Camera`
- `Directional Light`
- `Canvas`
- `EventSystem`
- `MenuButtons`
- `MainMenu_Backdrop`
- `MainMenu_3DRoom`
- `SelectedHeroPreview`

## Боевой HUD

Класс: `DDHUDManager`.

Элементы:

- HP text;
- level text;
- XP text;
- XP slider;
- time text;
- state text;
- active skill cooldown panel.

## Окно выбора навыка

Класс: `DDLevelUpManager`.

Элементы:

- level up panel;
- 3 option buttons;
- icons;
- type/title/description/rank texts;
- swap to upgrades button.

## Экран поражения

Управляется `DDRunManager`:

- `gameOverPanel`
- `gameOverSummaryText`
- restart.

## Не найдено явно

- полноценный экран паузы;
- экран победы;
- экран наград;
- экран результатов забега.

# 16. Экономика и награды

## Реализовано

- опыт внутри боя;
- descriptions наград в `MapData`.

## Не реализовано

- валюта;
- награды после победы;
- награды после смерти;
- магазин;
- стоимость мета-улучшений;
- сундуки;
- предметы;
- ресурсы.

## Требуемые формулы

- награда за время;
- награда за убийства;
- награда за боссов;
- награда за сложность карты;
- награда за affixes;
- множитель наград за победу.

# 17. Сохранения

## Текущая система

Класс: `GameProfileStore`.

Формат: `PlayerPrefs`.

Поддерживается 3 профиля.

## Сохраняемые данные

- active profile index;
- selected character id;
- selected map id;
- selected weapon id per character;
- settings keys.

## Риски

- `PlayerPrefs` подходит для прототипа, но слаб для полноценного прогресса.
- Нет versioning.
- Нет миграции старых сохранений.
- Нет одного JSON save file.
- Нет cloud save.
- Нет защиты от повреждения данных.

# 18. Архитектура проекта

## Основные папки

| Папка | Назначение |
|---|---|
| `Assets/Data` | ScriptableObject-контент |
| `Assets/Scripts/DataDriven` | Основная gameplay-архитектура |
| `Assets/Scripts/UI` | UI и меню |
| `Assets/Prefabs` | Prefab объектов |
| `Assets/Resources` | Глобальные базы |
| `Assets/Editor` | Editor tools |
| `Docs` | Документация |

## Основные сцены

- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/main.unity`

## Основные менеджеры

- `DDRunManager`
- `SpawnSystem`
- `BossManager`
- `MainMenuManager`
- `DDHUDManager`
- `DDLevelUpManager`
- `PoolManager`
- `CombatFeedbackSystem`

## Основные события

- `CharacterSystem.HealthChanged`
- `CharacterSystem.Died`
- `ProgressionSystem.LevelChanged`
- `ProgressionSystem.ExperienceChanged`
- `SkillSystem.ActiveSkillsChanged`
- `DDRunManager.StateChanged`
- `DDRunManager.ElapsedTimeChanged`
- `SpawnSystem.KillCountChanged`
- `CombatFeedbackEvents.DamageTaken`

## Перегруженные ответственностью классы

| Класс | Почему перегружен |
|---|---|
| `MainMenuManager` | UI, 3D-комната, настройки, выборы, scene loading, построение layout |
| `SpawnSystem` | Спавн врагов, сложность, drops, map bounds |
| `EnemyAgent` | HP, target selection, damage, status, movement, death |
| `DDRunManager` | Run state, map loading, spawn point, game over, affixes, scene loading |

# 19. Данные и баланс

## Где хранятся числовые параметры

- `CharacterData.baseStats`
- `SkillData`
- `AttackDefinition`
- `AttackDeliveryDefinition`
- `AttackImpactDefinition`
- `EnemyData.baseStats`
- `EnemyAbilityConfig`
- `MapData`
- `SpawnTimelineData`
- `RunAffixData`
- `CombatMath`

## Захардкожено

- max active skills = 6;
- XP curve;
- rarity weights;
- combat constants;
- UI layout values;
- fallback spawn settings;
- some menu settings keys.

## Нужно вынести в конфиги

- XP curve;
- max active skills;
- rarity weights;
- reward formulas;
- win condition;
- boss schedule;
- enemy density by device tier.

# 20. Аудио и визуальные эффекты

## VFX

Есть:

- floating damage text;
- target flash;
- primitive projectile visuals;
- primitive zones/hitboxes;
- 3D menu room;
- summon circle;
- enemy tint/scale.

## SFX

Боевые SFX не найдены.  
В меню есть настройки громкости, но отдельной аудио-системы не найдено.

## Анимации

Полноценная Animator-система не найдена. Большинство визуалов прототипные.

## Что нужно добавить

- hit VFX;
- cast VFX;
- area warning indicators;
- element colors;
- SFX ударов;
- SFX level up;
- SFX смерти врагов;
- camera shake;
- hit stop.

# 21. Производительность и оптимизация

## Уже есть

- `PoolManager`
- `IPoolable`
- `EnemyRegistry`
- pooling врагов/снарядов/зон/опыта;
- отказ от массового `FindObjects` для targeting.

## Потенциальные проблемы

- много `Update`;
- `RuntimeStats.GetValue` перебирает modifiers каждый вызов;
- UI cooldown refresh каждый кадр;
- orbital visuals создаются/уничтожаются при init;
- physics checks при большом количестве врагов;
- menu room создаёт много primitives/materials при rebuild.

## Приоритет оптимизации

1. RuntimeStats caching.
2. Pool warmup.
3. Reuse orbital visuals.
4. Throttled UI cooldown refresh.
5. Enemy movement LOD.
6. Spatial partition для очень больших толп.

# 22. Текущая степень готовности проекта

| Система | Статус | Где находится | Комментарий | Что доделать |
|---|---|---|---|---|
| Главное меню | Частично | `MainMenuManager` | Работает, но класс перегружен | Разделить на views/controllers |
| Выбор персонажа | Реализовано | `CharacterRoster`, `SelectedCharacterStore` | Есть 10 героев | Добавить unlock |
| Выбор оружия | Реализовано | `WeaponData`, `SelectedLoadoutStore` | Привязано к персонажу | Улучшить UI |
| Выбор карты | Реализовано | `MapDatabase`, `MapData` | Есть 5 карт | Добавить rewards/win |
| Игрок | Реализовано | `CharacterSystem` | HP, dash, regen, totems | Полировка анимаций |
| Камера | Реализовано | `CameraFollow` | Следует за игроком | Настройки камеры по карте |
| Навыки | Частично | `SkillData`, `AttackDefinition` | Есть модульная система | Мигрировать legacy skills |
| Улучшения | Частично | `UpgradeData`, `UpgradeSystem` | Работают | Баланс и UX |
| Враги | Реализовано | `EnemyAgent`, `EnemyData` | Есть melee/ranged/area | Разделить EnemyAgent |
| Спавн | Реализовано | `SpawnSystem` | Ring spawn и scaling | Больше timeline data |
| Боссы | Частично | `BossManager` | Спавн есть | Boss rewards/phases |
| Опыт | Реализовано | `ExperienceGem`, `ProgressionSystem` | Работает | Настроить curve |
| HUD | Частично | `DDHUDManager` | HP/XP/cooldowns | Улучшить visuals |
| Game Over | Реализовано | `DDRunManager` | Есть restart | Rewards summary |
| Победа | Отсутствует | Нет | Требует design | Win condition |
| Мета-прогрессия | Частично | SkillTree screen | UI-заготовка | Реальная система |
| Сохранения | Частично | `GameProfileStore` | PlayerPrefs | JSON save |
| Audio | Отсутствует | Нет | Есть только settings | SFX/music system |
| VFX feedback | Частично | `CombatFeedbackSystem` | Text + flash | Hit/cast/death VFX |

# 23. Проблемы, риски и пробелы

## Неявные механики

- победа;
- награды;
- unlock progression;
- экономика;
- boss phases;
- skill evolution;
- status stack rules.

## Технический долг

- legacy skills рядом с новой modular attack system;
- `MainMenuManager` слишком большой;
- `EnemyAgent` слишком много знает;
- `SpawnSystem` отвечает за несколько разных подсистем;
- `PlayerPrefs` вместо нормального save file.

## Недостаток данных

- нет отдельной таблицы XP curve;
- нет таблицы rewards;
- нет таблицы enemy scaling;
- нет таблицы skill balance;
- нет таблицы meta progression.

# 24. Рекомендации по дальнейшей разработке

## В первую очередь

1. Мигрировать оставшиеся legacy skills на `AttackDefinition`.
2. Определить win condition.
3. Сделать reward system.
4. Сделать meta progression data model.
5. Разделить `MainMenuManager`.
6. Разделить `EnemyAgent`.
7. Создать balance tables.

## Стабилизировать

- Level up choice rules.
- Skill pool rules.
- Weapon restrictions.
- Enemy targeting.
- Boss spawn.
- Status stacking.
- Save profile.

## Дополнительные документы

- Skill Balance Sheet.
- Enemy Balance Sheet.
- Map Difficulty Sheet.
- Reward Economy Sheet.
- Meta Progression Sheet.
- UI Flow Document.

# 25. Итоговая структура идеального проекта

```text
Assets/
  Art/
    Characters/
    Enemies/
    VFX/
    UI/
  Audio/
    Music/
    SFX/
  Data/
    Characters/
    Weapons/
    Skills/
      Definitions/
      Targeting/
      Delivery/
      Impact/
    Enemies/
    EnemyAbilities/
    Maps/
    SpawnTimelines/
    Upgrades/
    Affixes/
    MetaProgression/
    Balance/
  Prefabs/
    Characters/
    Enemies/
    Projectiles/
    Hitboxes/
    Maps/
    UI/
    VFX/
  Scenes/
    Boot.unity
    MainMenu.unity
    Gameplay.unity
    Loading.unity
  Scripts/
    Core/
    Save/
    Data/
    Runtime/
    Combat/
    Skills/
    Enemies/
    Spawning/
    Progression/
    UI/
    Feedback/
    Audio/
    Editor/
  Resources/
    CharacterRoster.asset
    MapDatabase.asset
  Docs/
```

# Вопросы, которые нужно уточнить у владельца проекта

| # | Вопрос | Ответ |
|---:|---|---|
| 1 | Какое финальное условие победы: таймер, босс, волны, цель на карте? |  |
| 2 | Сколько должен длиться стандартный забег: 10, 15, 20 минут? |  |
| 3 | Сколько активных навыков максимум: 5 или 6? |  |
| 4 | Должны ли стартовые навыки оружия занимать слот активного навыка? |  |
| 5 | Можно ли заменять активные навыки после достижения лимита? |  |
| 6 | Какие валюты будут вне боя? |  |
| 7 | Какие награды игрок получает после смерти или победы? |  |
| 8 | Как должны разблокироваться персонажи? |  |
| 9 | Как должны разблокироваться оружия? |  |
| 10 | Как должны разблокироваться карты? |  |
| 11 | Нужна ли полноценная система редкостей навыков или только улучшений? |  |
| 12 | Нужно ли повышать уровень активных навыков отдельно от пассивных улучшений? |  |
| 13 | Как должны стакаться статусы: бесконечно, лимит, refresh duration? |  |
| 14 | Должны ли враги иметь resistances к элементам? |  |
| 15 | Нужны ли разные типы брони у врагов? |  |
| 16 | Должны ли боссы иметь уникальные фазы? |  |
| 17 | Нужны ли elite affixes у врагов? |  |
| 18 | Нужно ли делать карту бесконечной или ограниченной ареной? |  |
| 19 | Должны ли препятствия разрушаться? |  |
| 20 | Какой визуальный стиль финально: low poly, stylized fantasy, realistic, dark fantasy? |  |
| 21 | Нужна ли мобильная оптимизация под конкретный FPS target: 30 или 60? |  |
| 22 | Нужно ли сохранять прогресс локально или в облако? |  |
| 23 | Какой язык будет основным: русский, английский или оба? |  |
| 24 | Нужна ли загрузка дополнительных ресурсов после установки игры? |  |
| 25 | Какие системы важнее делать следующими: мета-прогрессия, VFX/SFX, баланс, новые враги, боссы или карты? |  |
