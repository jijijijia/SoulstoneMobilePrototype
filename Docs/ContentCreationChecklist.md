# Чеклист Создания Нового Контента

Этот документ — пошаговый гайд для добавления нового контента в проект через Unity Editor.

---

## Создание Нового Персонажа

### Через Wizard (быстрый способ)
1. Открой `Soulstone -> Create Character Content Wizard`.
2. Заполни Hero Name, Weapon Name, Starting Skill Name.
3. Нажми `Create Character Pack`.

### Вручную
1. В `Assets/Data/Characters` дублируй `Hero_Default.asset`.
2. Переименуй в `Hero_ИмяГероя`.
3. Заполни:
   - `characterId`: уникальный нижним регистром через подчёркивания
   - `displayName`: имя для UI
   - `baseStats`: значения по таблице из CharactersBalance.md
   - `startingWeapon`: ссылка на WeaponData
   - `availableWeapons`: список доступных WeaponData
   - `globalUpgradePool`: список UpgradeData
4. Добавь запись в `Assets/Resources/CharacterRoster.asset`.

### Статы для baseStats (StatType → Значение):
Смотри таблицу StatType Mapping в CharactersBalance.md.

### Чеклист завершения:
- [ ] characterId уникален
- [ ] Есть startingWeapon
- [ ] availableWeapons содержит хотя бы 1 оружие
- [ ] globalUpgradePool содержит хотя бы 5 улучшений
- [ ] Запись добавлена в CharacterRoster

---

## Создание Нового Оружия

1. В `Assets/Data/Weapons` дублируй любой `Weapon_*.asset`.
2. Переименуй в `Weapon_ИмяОружия`.
3. Заполни:
   - `weaponId`: уникальный id
   - `displayName`: имя для UI
   - `startingSkill`: ссылка на SkillData
   - `uniqueSkillPool`: список 5–8 SkillData
4. Добавь оружие в `CharacterData.availableWeapons` нужного персонажа.

### Чеклист завершения:
- [ ] weaponId уникален
- [ ] Есть startingSkill
- [ ] uniqueSkillPool содержит 5–8 навыков
- [ ] Оружие привязано к персонажу
- [ ] startingSkill тоже входит в uniqueSkillPool

---

## Создание Нового Навыка

### Шаг 1: Определить механику
Выбери из PlayerSkillsBalance.md:
- Тип урона, кулдаун, радиус, количество снарядов
- Targeting: NearestEnemy, RandomEnemies или SelfPosition
- Delivery: Projectile, Spread, AreaPulse, Delayed, Lasting, Chain, Melee, Frontal, Orbital, Summon
- Impact: Damage, Status (или оба)

### Шаг 2: Создать AttackDefinition
1. В `Assets/Data/AttackModules/Definitions` создай `Attack_ИмяНавыка.asset`.
2. Укажи Targeting, Delivery, Impacts.

### Шаг 3: Создать/выбрать Targeting
- `Assets/Data/AttackModules/Targeting`
- Три варианта: NearestEnemy, RandomEnemies, SelfPosition

### Шаг 4: Создать/выбрать Delivery
- `Assets/Data/AttackModules/Delivery`
- Дублируй похожий существующий Delivery

### Шаг 5: Создать Impact
- `Assets/Data/AttackModules/Impact`
- Для урона: `Impact_Damage_ИмяНавыка.asset`
- Для статуса: `Impact_Status_ИмяНавыка.asset`
- Можно указать оба

### Шаг 6: Создать SkillData
1. В `Assets/Data/Skills` дублируй схожий `Skill_*.asset`.
2. Заполни:
   - `skillId`: уникальный id
   - `displayName`: имя для UI
   - `description`: описание
   - `baseDamage`: базовый урон по PlayerSkillsBalance.md
   - `cooldown`: базовый кулдаун
   - `areaMultiplier`: 1.0 по умолчанию
   - `runtimeDefinition`: ModularAttackRuntimeDefinition
   - `attackDefinitions`: ссылка на созданный AttackDefinition
   - `maxRank`: 5

### Шаг 7: Добавить в пул оружия
1. Открой нужный `WeaponData.asset`.
2. Добавь навык в `uniqueSkillPool`.

### Чеклист завершения:
- [ ] skillId уникален
- [ ] Есть attackDefinitions
- [ ] runtimeDefinition = ModularAttackRuntimeDefinition
- [ ] baseDamage и cooldown заполнены
- [ ] Навык добавлен в uniqueSkillPool хотя бы одного оружия
- [ ] Проверил, что visualPrefab назначен (или null → placeholder куб)

---

## Создание Улучшения

1. В `Assets/Data/Upgrades` дублируй `Upgrade_MaxHealth.asset`.
2. Заполни:
   - `upgradeId`: уникальный id
   - `displayName`: имя
   - `description`: описание бонуса
   - `scope`: Character = 0, AllSkills = 1, SpecificSkill = 2
   - `repeatable`: true если можно брать несколько раз
   - `targetSkill`: ссылка на SkillData (только при scope = SpecificSkill)
   - `statModifiers`: список изменений статов

### Для statModifiers:
```text
statType: (см. StatType Mapping)
additive: плоское значение изменения
multiplier: процентное значение изменения (0.15 = +15%)
```

### Чеклист завершения:
- [ ] upgradeId уникален
- [ ] scope и statType соответствуют нужному стату
- [ ] Если scope = SpecificSkill — targetSkill заполнен
- [ ] Улучшение добавлено в globalUpgradePool нужных персонажей

---

## Создание Врага

1. В `Assets/Data/Enemies` дублируй `Enemy_Basic.asset`.
2. Переименуй в `Enemy_ИмяВрага`.
3. Заполни:
   - `enemyId`: уникальный id
   - `displayName`: имя
   - `category`: 0 = Normal, 1 = Empowered, 2 = Elite, 3 = MiniBoss, 4 = Boss
   - `prefab`: ссылка на Prefab (назначь вручную)
   - `abilityConfig`: ссылка на EnemyAbilityConfig (можно дублировать Basic)
   - `baseStats`: Health, MoveSpeed, Damage, Armor по EnemiesBalance.md
   - `spawnCost`, `spawnWeight`, `experienceReward`
   - `preferredDistance`, `attackInterval`
4. Добавь врага в `MapData.enemyPool` нужной карты.
5. Добавь трек веса в `SpawnTimelineData.enemyWeightTracks`.

### Чеклист завершения:
- [ ] enemyId уникален
- [ ] category соответствует роли
- [ ] baseStats заполнены
- [ ] Враг добавлен в enemyPool карты
- [ ] Добавлен enemyWeightTrack в SpawnTimeline

---

## Обновление SpawnTimeline для 30-минутного Забега

Открой `Assets/Data/SpawnTimeline_Default.asset` и обнови:

1. `runDuration: 1800` (было 600)
2. Обнови keyframes для healthMultiplier, damageMultiplier, spawnRate, packSize, maxAlive.

Значения keyframes смотри в DifficultyScaling.md — раздел "SpawnTimeline_Default: Обновлённые Значения".

---

## Добавление Нового BurstEvent

В `SpawnTimelineData.burstEvents`:

```text
eventId: уникальный_id
t: нормализованное время (минута / 30)
enemy: ссылка на EnemyData
count: количество
healthMultiplier: 1.0–1.5
damageMultiplier: 1.0–1.3
```

Пример для появления 3 Elite в минуту 15:
```text
eventId: elite_pressure_15min
t: 0.50
enemy: Enemy_Elite
count: 3
healthMultiplier: 1.0
damageMultiplier: 1.0
```

---

## Добавление Нового Аффикса Забега

1. В `Assets/Data/Affixes` создай новый `RunAffix_ИмяАффикса.asset`.
2. Заполни эффект аффикса.
3. Аффикс появится в списке выбора аффиксов в UI (если UI его поддерживает).

---

## Важные Правила Именования

| Тип | Паттерн | Пример |
| --- | --- | --- |
| CharacterData | Hero_ИмяКэмелКейс | Hero_Viking |
| WeaponData | Weapon_ИмяКэмелКейс | Weapon_VikingAxe |
| SkillData | Skill_ИмяКэмелКейс | Skill_VikingWhirl |
| UpgradeData | Upgrade_ИмяКэмелКейс | Upgrade_MaxHealth |
| EnemyData | Enemy_ИмяКэмелКейс | Enemy_Basic |
| AttackDefinition | Attack_ИмяКэмелКейс | Attack_VikingWhirl |
| Delivery | Delivery_Тип | Delivery_Projectile |
| Impact | Impact_Тип_ИмяНавыка | Impact_Damage_Spear |

---

## Частые Ошибки

| Ошибка | Следствие | Как исправить |
| --- | --- | --- |
| ID не уникален | Конфликт данных в рантайме | Проверь все .asset файлы |
| startingSkill не в uniqueSkillPool | Стартовый навык недоступен для прокачки | Добавь навык в пул |
| Навык не в пуле ни одного оружия | Никогда не выпадает | Добавь в uniqueSkillPool |
| Враг не в enemyPool карты | Не спавнится | Добавь в MapData |
| Нет enemyWeightTrack | Вес = 0, не спавнится | Добавь трек в Timeline |
| maxRank = 0 | Навык нельзя прокачивать | Установи maxRank = 5 |
| scope = SpecificSkill, targetSkill пустой | Крэш или ошибка | Заполни targetSkill |
