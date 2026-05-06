# Stage 6 Polish Guide

Этот документ описывает слой финальной полировки, который добавлен поверх текущей data-driven архитектуры проекта.

## Combat Feedback

Боевой отклик теперь вынесен в отдельный слой:

- `CombatFeedbackEvent` описывает событие урона.
- `CombatFeedbackEvents` является простой шиной событий.
- `CombatFeedbackSystem` автоматически создается при загрузке сцены и показывает floating damage text.
- `FloatingCombatText` отвечает только за визуал текста урона.

Игровые системы не создают визуалы напрямую. `CharacterSystem` и `EnemyAgent` только отправляют событие урона, а feedback-слой сам решает, как это показать. Это важно, потому что позже можно заменить текст на частицы, вспышки, звук или экранную тряску без переписывания боевой логики.

## Save Profiles

Выбор персонажа, карты и оружия теперь сохраняется через профиль:

- `GameProfileStore.ActiveProfileIndex` выбирает один из трех профилей.
- `SelectedCharacterStore` хранит выбранного героя внутри активного профиля.
- `SelectedLoadoutStore` хранит выбранную карту и оружие внутри активного профиля.

В Unity Editor появились команды:

- `Soulstone/Profile/Use Profile 1`
- `Soulstone/Profile/Use Profile 2`
- `Soulstone/Profile/Use Profile 3`
- `Soulstone/Profile/Clear Active Profile`
- `Soulstone/Profile/Print Active Profile`

Это позволит тестировать разные сохранения без ручной чистки `PlayerPrefs`.

## Content Validation

Команда `Soulstone/Validate Content` теперь проверяет не только дубли ID, но и базовую целостность контента:

- у персонажей есть `MaxHealth`, `MoveSpeed`, `PickupRadius`;
- стартовое оружие персонажа входит в список доступного оружия;
- стартовый навык оружия входит в пул навыков оружия;
- у врагов есть здоровье, скорость и контактный урон;
- у врагов есть ability config;
- карты имеют prefab, timeline, enemy pool, корректные bounds и безопасную точку спавна.

Запускай эту проверку после создания новых героев, оружия, врагов, карт и навыков. Если валидатор ругается, лучше исправить asset до запуска игры.

## What To Test In Unity

После изменений стоит проверить:

- урон по врагам показывает цифры;
- урон по игроку показывает красные цифры;
- при выборе другого профиля в меню выбор героя/оружия/карты становится независимым;
- `Clear Active Profile` сбрасывает выборы только текущего профиля;
- `Soulstone/Validate Content` не показывает критичных предупреждений для рабочего контента;
- рестарт после смерти не оставляет зависшие floating texts.

## Combat Sound System

Звуки ударов добавлены через тот же `CombatFeedbackEvents`:

- `CombatSoundSystem` — авто-создаётся при запуске, подписывается на `CombatFeedbackEvents.DamageTaken`.
- Три слота AudioClip: `playerHitClip`, `enemyHitClip`, `criticalHitClip`.
- Лёгкая рандомизация pitch для разнообразия.
- Назначь клипы на объекте `CombatSoundSystem` в инспекторе во время Play Mode или через prefab.

## Camera Shake

`CameraFollow` теперь поддерживает camera shake:

- `CameraFollow.Shake(intensity)` — статический вызов из любого места.
- Shake затухает автоматически через `shakeDecay` (по умолчанию 8 ед./сек).
- `CombatFeedbackSystem` автоматически вызывает shake при ударе по игроку.
  - Обычный удар: `0.22f` интенсивность.
  - Критический удар: `0.42f` интенсивность.
- Все три поля настраиваемы в инспекторе (`shakeOnPlayerHit`, `playerHitShakeIntensity`, `criticalHitShakeIntensity`).

## Cooldown Slot — Ready Pulse

`SkillCooldownSlotView` теперь показывает pulse-анимацию когда навык становится готов:

- При переходе `IsReady: false → true` проигрывается лёгкое увеличение слота (по умолчанию x1.18 за 0.22 сек).
- Параметры настраиваемы: `pulseScale`, `pulseDuration`.
- Анимация работает на `filledStateRoot` — его scale.

## Enemy Ability Config Wizard

Новый editor wizard: `Soulstone/Create Enemy Ability Config Wizard`.

- Форма для всех полей `EnemyAbilityConfig`: ID, тип доставки, дистанция, кулдаун, урон.
- Секция Projectile показывается только при выборе `Projectile` delivery type.
- Секция Area Pulse — только при `AreaPulse`.
- Создаёт `.asset` файл в `Assets/Data/EnemyAbilities/` (папка настраиваема).

## Meta-Progression

Слой мета-прогрессии добавлен как глобальное хранилище (не привязан к профилям):

- `MetaProgressionStore` — статический класс:
  - `TotalSouls` — валюта через все забеги.
  - `AddSouls(int)` / `SpendSouls(int)` — управление валютой.
  - `IsCharacterUnlocked(id)` / `UnlockCharacter(id)` — анлоки героев.
  - `IsWeaponUnlocked(id)` / `UnlockWeapon(id)` — анлоки оружия.
  - `GetMetaUpgradeRank(id)` / `PurchaseMetaUpgrade(id, cost, maxRank)` — дерево мета-апгрейдов.

- `SoulsRewardSystem` — авто-создаётся при запуске, начисляет души по итогам забега:
  - 5 душ за каждого убитого врага.
  - 10 душ за каждую выживленную минуту.
  - +50 душ за победу.
  - Множитель x1.25 за каждый активный Affix (сложнее = больше награда).

## What To Test In Unity

После изменений стоит проверить:

- урон по врагам показывает цифры;
- урон по игроку показывает красные цифры;
- camera shake срабатывает при получении урона игроком;
- camera shake не срабатывает при уроне по врагам;
- когда навык готов — слот делает лёгкий pulse;
- `Soulstone/Create Enemy Ability Config Wizard` создаёт asset без ошибок;
- после завершения забега (победа или поражение) в Console видно `[SoulsRewardSystem] Run ended ...`;
- при выборе другого профиля в меню выбор героя/оружия/карты становится независимым;
- `Clear Active Profile` сбрасывает выборы только текущего профиля;
- `Soulstone/Validate Content` не показывает критичных предупреждений для рабочего контента;
- рестарт после смерти не оставляет зависшие floating texts.

## Next Steps

- Добавить UI для мета-прогрессии (экран между забегами: показать души, кнопки анлока).
- Добавить `MetaUpgradeData` ScriptableObject и UI дерева навыков.
- Добавить hit-stop (brief `Time.timeScale` dip) для особо сильных ударов.
- Улучшить cooldown HUD prefab визуально (иконки, фреймы, цвета).
