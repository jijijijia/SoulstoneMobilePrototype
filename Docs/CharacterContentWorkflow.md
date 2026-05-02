# Быстрый Workflow Создания Персонажа

Этот документ — короткая версия. Подробный гайд лежит здесь: `Docs/ContentCreationGuide.md`.

## Через Wizard

Если нужен быстрый старт:

1. Открой `Soulstone -> Create Character Content Wizard`.
2. Заполни имя героя, оружия и стартового навыка.
3. Нажми `Create Character Pack`.

Wizard создаёт и связывает:

- `CharacterData`
- `WeaponData`
- `SkillData`
- запись в `Assets/Resources/CharacterRoster.asset`

## Ручной Способ

1. Создай или дублируй `CharacterData` в `Assets/Data/Characters`.
2. Создай или дублируй `WeaponData` в `Assets/Data/Weapons`.
3. Создай или дублируй `SkillData` в `Assets/Data/Skills`.
4. Если навык modular:
   - создай `Impact` в `Assets/Data/AttackModules/Impact`;
   - выбери `Targeting`;
   - выбери `Delivery`;
   - создай `AttackDefinition`;
   - привяжи `AttackDefinition` в `SkillData`.
5. Добавь `SkillData` в `WeaponData.Unique Skill Pool`.
6. Добавь `WeaponData` в `CharacterData.Available Weapons`.
7. Добавь `CharacterData` в `Assets/Resources/CharacterRoster.asset`.

## Runtime Flow

```text
MainMenu
-> SelectedCharacterStore
-> SelectedLoadoutStore
-> CharacterSystem
-> WeaponSystem
-> SkillSystem
-> бой
```

## Важные Статы Персонажа

- `MaxHealth`: здоровье.
- `MoveSpeed`: скорость.
- `AttackPower`: множитель урона навыков.
- `CritChance`: шанс критического удара.
- `CritMultiplier`: множитель критического урона.
- `SkillFrequency`: делит кулдаун навыков.
- `Area`: плоский бонус области.
- `AreaMultiplier`: множитель области.
- `DoubleAttackChance`: шанс повторного применения навыка.
- `Defense`: снижает входящий урон через armor-style формулу из `CombatMath`.
- `ParryChance`: шанс полностью заблокировать удар.
- `ExperienceMultiplier`: множитель опыта.
- `PickupRadius`: радиус подбора опыта.
- `DashCharges`: количество рывков.
- `HealthRegenPercent`: процентная регенерация здоровья.
- `LifeTotemCount`: количество воскрешений.
- `Armor`: нейтральная броня врага равна `100`.
- `MulticastChance`: альтернативный стат для повторных применений.

## Уже Есть В Проекте

- `Hero_Default`
- `Hero_FireKnight`
- `Storm Sentinel`
- `Weapon_Starter`
- `Weapon_StormSigil`
- `Skill_KnifeFan`
- `Skill_AxeCleave`
- `Skill_ChainLightning`
- `Skill_PoisonMire`
- `MapDatabase` с пятью картами
