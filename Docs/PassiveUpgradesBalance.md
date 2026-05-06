# Баланс Пассивных Улучшений

Этот документ содержит данные всех пассивных улучшений игры с параметрами баланса.

---

## Принципы Улучшений

- Обычные (Common): небольшой приятный прирост. Вес 10.
- Необычные (Uncommon): умеренное улучшение. Вес 6.
- Редкие (Rare): значительное влияние на билд. Вес 3.
- Эпические (Epic): изменяют правила игры. Вес 1.
- Не должно быть одного очевидно лучшего улучшения для всех ситуаций.
- Рискованные улучшения дают больше, но требуют чего-то взамен.

---

## StatType Для Апгрейдов

Подтверждённые значения из анализа .asset файлов:

| StatType | Параметр | Пример | Подтверждение |
| --- | --- | --- | --- |
| 0 | MaxHealth | +20 HP | Upgrade_MaxHealth |
| 1 | MoveSpeed | +0.5 | Upgrade_MoveSpeed |
| 2 | FlatDamageBonus | +10 к навыкам | Upgrade_Damage |
| 3 | CritChance | +0.06 = +6% | Upgrade_Precision |
| 4 | CritMultiplier | +0.25 | Upgrade_Precision |
| 5 | AreaBonus | +0.8 (flat) | Upgrade_AreaMastery |
| 7 | PickupRadius | +0.8 | Upgrade_PickupRadius |
| 14 | AttackPower | +0.18 = +18% | Upgrade_Overcharge |
| 15 | SkillFrequency | +0.15 = +15% | Upgrade_Overclock |
| 17 | DoubleAttackChance | +0.08 = +8% | Upgrade_EchoStrike |
| 18 | Defense | +20 | Upgrade_Bulwark |
| 19 | ParryChance | +0.04 = +4% | Upgrade_Bulwark |
| 20 | ExperienceMultiplier | +0.10 = +10% | Upgrade_Scholar (новый) |
| 21 | DashCharges | +1 | Upgrade_Momentum (новый) |
| 22 | HealthRegenPercent | +0.004 = +0.4%/сек | Upgrade_Renewal |
| 23 | LifeTotemCount | +1 воскрешение | Upgrade_SoulTotem |

---

## Улучшения Урона

### Upgrade_Damage (Heavy Shot)
| Поле | Значение |
| --- | --- |
| Цель (Scope) | All Skills |
| Стат | FlatDamageBonus |
| Значение за уровень | +10 |
| Макс уровень | 5 |
| Редкость | Common |
| Вес | 10 |
| Синергии | Все навыки с коротким КД |

Итог: +50 плоского урона на 5-м уровне.

---

### Upgrade_AllSkillDamage (Power Surge)
| Поле | Значение |
| --- | --- |
| Цель | All Skills |
| Стат | AttackPower (multiplier) |
| Значение за уровень | +0.08 (8%) |
| Макс уровень | 5 |
| Редкость | Uncommon |
| Вес | 6 |
| Синергии | Все навыки с высоким базовым уроном |

Итог: +40% к AttackPower на 5-м уровне (стакается мультипликативно с базой).

---

### Upgrade_ArmageddonDamage (Harbinger)
| Поле | Значение |
| --- | --- |
| Цель | Specific: Skill_Armageddon |
| Стат | FlatDamageBonus |
| Значение за уровень | +50 |
| Макс уровень | 3 |
| Редкость | Rare |
| Вес | 3 |
| Синергии | Тяжёлые АоЕ билды |

---

### Upgrade_VikingWhirlDamage (Whirlwind Mastery)
| Поле | Значение |
| --- | --- |
| Цель | Specific: Skill_VikingWhirl |
| Стат | FlatDamageBonus |
| Значение за уровень | +25 |
| Макс уровень | 3 |
| Редкость | Uncommon |
| Вес | 4 |

---

### Upgrade_AxeCleaveDamage (Cleave Mastery)
*(аналогично, ссылка на Skill_AxeCleave)*

### Upgrade_DoubleCleaveDamage
*(ссылка на Skill_DoubleCleave)*

### Upgrade_KnightSwordArcDamage
### Upgrade_ArrowRainDamage
### Upgrade_FireRainDamage
### Upgrade_RuneQuakeDamage
### Upgrade_BoulderThrowDamage
### Upgrade_SpearVolleyDamage
### Upgrade_HolyHammerDamage
### Upgrade_LightningSeriesDamage
### Upgrade_AssassinKnifeStormDamage
### Upgrade_RaiseDeadDamage

Все skill-specific апгрейды:
| Параметр | Значение |
| --- | --- |
| Стат | FlatDamageBonus |
| Значение за уровень | +20–50 (в зависимости от базового урона навыка) |
| Макс уровень | 3 |
| Редкость | Uncommon |
| Вес | 4 |

---

## Улучшения Атаки и Частоты

### Upgrade_Overcharge (Rapid Fire)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | SkillFrequency |
| Значение за уровень | +0.20 (20%) |
| Макс уровень | 4 |
| Редкость | Uncommon |
| Вес | 6 |
| Синергии | KnifeFan, LightningSeries, DoubleCleave |
| Ограничение | Итоговый КД не ниже 0.15 сек |

Итог: SkillFrequency 1.0 → 1.8 на 4-м уровне (навыки работают на 80% чаще).

---

### Upgrade_Overclock (Cooldown Crunch)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | Cooldown Divider |
| Значение за уровень | +0.15 |
| Макс уровень | 3 |
| Редкость | Rare |
| Вес | 3 |
| Синергии | Мощные навыки с долгим КД (Armageddon, StormCall) |

---

### Upgrade_EchoStrike (Echo)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | DoubleAttackChance |
| Значение за уровень | +0.10 (10%) |
| Макс уровень | 3 |
| Редкость | Rare |
| Вес | 3 |
| Синергии | Projectile навыки, KnifeFan |

Итог: +30% шанс двойной атаки.

---

### Upgrade_ProjectileMastery (Extra Barrage)
| Поле | Значение |
| --- | --- |
| Цель | All Skills (projectile тег) |
| Стат | Количество снарядов +1 |
| Значение за уровень | +1 снаряд |
| Макс уровень | 3 |
| Редкость | Uncommon |
| Вес | 5 |
| Синергии | KnifeFan, SpearVolley, AssassinKnifeStorm |

---

## Улучшения Критического Удара

### Upgrade_Precision (Sharp Edge)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | CritChance |
| Значение за уровень | +0.05 (5%) |
| Макс уровень | 4 |
| Редкость | Uncommon |
| Вес | 5 |
| Синергии | Assassin, StormJavelin |

Итог: +20% шанс крита на 4-м уровне.

---

### Upgrade_Overcharge_Crit (Critical Power)

*Новый апгрейд — требует создания .asset файла вручную*

| Поле | Значение |
| --- | --- |
| ID | critical_power |
| Название | Critical Power |
| Цель | Character |
| Стат | CritMultiplier |
| Значение за уровень | +0.30 |
| Макс уровень | 3 |
| Редкость | Rare |
| Вес | 3 |
| Синергии | Assassin + Precision |

---

## Улучшения Области

### Upgrade_AreaMastery (Wide Reach)
| Поле | Значение |
| --- | --- |
| Цель | All Skills |
| Стат | AreaMultiplier |
| Значение за уровень | +0.15 (15%) |
| Макс уровень | 4 |
| Редкость | Uncommon |
| Вес | 6 |
| Синергии | VikingWhirl, AreaPulse навыки, PoisonMire |

Итог: AreaMultiplier +0.6 (×1.6 к базе на 4-м уровне).

---

## Улучшения Выживаемости

### Upgrade_MaxHealth (Vitality)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | MaxHealth |
| Значение за уровень | +20 |
| Макс уровень | 5 |
| Редкость | Common |
| Вес | 9 |
| Синергии | Knight, Paladin |

---

### Upgrade_Bulwark (Iron Guard)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | Defense |
| Значение за уровень | +10 |
| Макс уровень | 4 |
| Редкость | Uncommon |
| Вес | 6 |
| Синергии | Knight, Paladin |

Итог: +40 к Defense. Итоговая броня Knight = 140, снижение урона ≈ 59%.

---

### Upgrade_Renewal (Life Flow)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | HealthRegenPercent |
| Значение за уровень | +0.005 (0.5%/сек) |
| Макс уровень | 4 |
| Редкость | Uncommon |
| Вес | 5 |
| Синергии | Paladin, Knight, высокий MaxHealth |

---

### Upgrade_SoulTotem (Death Ward)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | LifeTotemCount |
| Значение за уровень | +1 |
| Макс уровень | 2 |
| Редкость | Epic |
| Вес | 1 |
| Описание | При смертельном уроне восстанавливает 50% HP. |

---

### Upgrade_Parry (Iron Reflexes)

*Новый апгрейд*

| Поле | Значение |
| --- | --- |
| ID | iron_reflexes |
| Название | Iron Reflexes |
| Цель | Character |
| Стат | ParryChance |
| Значение за уровень | +0.06 (6%) |
| Макс уровень | 3 |
| Редкость | Rare |
| Вес | 3 |
| Синергии | Knight, Paladin |

---

## Улучшения Прогрессии

### Upgrade_PickupRadius (Magnet)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | PickupRadius |
| Значение за уровень | +0.8 |
| Макс уровень | 4 |
| Редкость | Common |
| Вес | 7 |
| Синергии | Ranger |

---

### Upgrade_MoveSpeed (Swift)
| Поле | Значение |
| --- | --- |
| Цель | Character |
| Стат | MoveSpeed |
| Значение за уровень | +0.5 |
| Макс уровень | 4 |
| Редкость | Common |
| Вес | 8 |
| Синергии | Assassin |

---

### Upgrade_ExperienceGain (Scholar)

*Новый апгрейд*

| Поле | Значение |
| --- | --- |
| ID | scholar |
| Название | Scholar |
| Цель | Character |
| Стат | ExperienceMultiplier |
| Значение за уровень | +0.10 (10%) |
| Макс уровень | 4 |
| Редкость | Common |
| Вес | 7 |
| Синергии | Ranger (базовый множитель 1.3) |

---

### Upgrade_DashCharges (Momentum)

*Новый апгрейд*

| Поле | Значение |
| --- | --- |
| ID | momentum |
| Название | Momentum |
| Цель | Character |
| Стат | DashCharges |
| Значение за уровень | +1 |
| Макс уровень | 2 |
| Редкость | Rare |
| Вес | 3 |
| Синергии | Assassin |

---

## Рискованные Улучшения

### Upgrade_BloodPact (Blood Pact)

*Новый эпик апгрейд — требует кодовой поддержки или custom logic*

| Поле | Значение |
| --- | --- |
| ID | blood_pact |
| Название | Blood Pact |
| Цель | Character |
| Эффект | AttackPower multiplier +0.35, MaxHealth additive −30 |
| Макс уровень | 1 |
| Редкость | Epic |
| Вес | 1 |
| Описание | Мощь за счёт жизни. +35% урон, −30 HP. |
| Ограничение | Нельзя брать если MaxHealth < 90 |

Примечание: требует проверки в коде — UpgradeData поддерживает несколько statModifiers.

---

### Upgrade_StormJavelinFocus (Javelin Master)
| Поле | Значение |
| --- | --- |
| Цель | Specific: Skill_StormJavelin |
| Стат | FlatDamageBonus |
| Значение за уровень | +30 |
| Макс уровень | 3 |
| Редкость | Uncommon |
| Вес | 4 |

---

## Сводная Таблица Улучшений

| ID | Название | Редкость | Цель | Вес |
| --- | --- | --- | --- | ---: |
| damage_up | Heavy Shot | Common | All Skills | 10 |
| power_surge | Power Surge | Uncommon | All Skills | 6 |
| max_health | Vitality | Common | Character | 9 |
| move_speed | Swift | Common | Character | 8 |
| pickup_radius | Magnet | Common | Character | 7 |
| scholar | Scholar | Common | Character | 7 |
| area_mastery | Wide Reach | Uncommon | All Skills | 6 |
| overcharge | Rapid Fire | Uncommon | Character | 6 |
| bulwark | Iron Guard | Uncommon | Character | 6 |
| renewal | Life Flow | Uncommon | Character | 5 |
| precision | Sharp Edge | Uncommon | Character | 5 |
| projectile_mastery | Extra Barrage | Uncommon | Projectiles | 5 |
| overclock | Cooldown Crunch | Rare | Character | 3 |
| echo_strike | Echo | Rare | Character | 3 |
| critical_power | Critical Power | Rare | Character | 3 |
| iron_reflexes | Iron Reflexes | Rare | Character | 3 |
| momentum | Momentum | Rare | Character | 3 |
| soul_totem | Death Ward | Epic | Character | 1 |
| blood_pact | Blood Pact | Epic | Character | 1 |
| skill_damage_* | Skill-specific | Uncommon | Specific | 4 |

---

## Требования Создания В Unity Editor

Следующие улучшения нужно создать вручную в `Assets/Data/Upgrades/`:

1. **Upgrade_CriticalPower** — statType 4, additive 0.30, mult 0, max 3
2. **Upgrade_Scholar** — statType 20, additive 0, multiplier 0.10, max 4
3. **Upgrade_IronReflexes** — statType 8, additive 0.06, mult 0, max 3
4. **Upgrade_Momentum** — statType 21, additive 1, mult 0, max 2
5. **Upgrade_BloodPact** — два statModifier: statType 14 mult +0.35 + statType 0 additive −30, max 1

Остальные уже существуют в проекте.
