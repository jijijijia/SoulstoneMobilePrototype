# Баланс Персонажей

Этот документ описывает базовые характеристики всех персонажей, их архетипы, сильные и слабые стороны.

---

## Принципы Создания Персонажей

- Каждый персонаж должен иметь чёткую роль и стиль игры.
- Разброс MaxHealth: 80–200. Разброс MoveSpeed: 4.5–7.5.
- Ни один персонаж не должен быть безусловно лучше других.
- Уникальность достигается через разные базовые статы и пул оружия.

---

## StatType Mapping

Для создания/редактирования CharacterData.baseStats.
Подтверждено из анализа .asset файлов:

| StatType | Параметр | Примечание |
| ---: | --- | --- |
| 0 | MaxHealth | base 120 |
| 1 | MoveSpeed | base 6.0 |
| 2 | FlatDamageBonus | +плоский урон к навыкам |
| 3 | CritChance | 0.05 = 5%, подтверждено из Precision |
| 4 | CritMultiplier | 2.0 = ×2, подтверждено из Precision |
| 5 | AreaBonus | плоский бонус области, из AreaMastery |
| 7 | PickupRadius | base 2.5 |
| 14 | AttackPower | 1.0 = 100%, подтверждено из Overcharge |
| 15 | SkillFrequency | 1.0 = норм, подтверждено из Overclock |
| 16 | AreaMultiplier | 1.0 = норм |
| 17 | DoubleAttackChance | 0.10 = 10%, подтверждено из EchoStrike |
| 18 | Defense | +20 = бронирование, подтверждено из Bulwark |
| 19 | ParryChance | 0.04 = 4%, подтверждено из Bulwark |
| 20 | ExperienceMultiplier | 1.0 = норм (предположительно) |
| 21 | DashCharges | 2 = 2 рывка (предположительно) |
| 22 | HealthRegenPercent | 0.004 = 0.4%/сек, подтверждено из Renewal |
| 23 | LifeTotemCount | +1 воскрешение, подтверждено из SoulTotem |

Примечание: statType 6, 8, 10, 11 присутствуют в Hero_Default.asset, но их назначение требует проверки через код CharacterData.

---

## 1. Воин (Viking)

**Архетип:** Сбалансированный воин — простой и надёжный выбор.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 150 |
| MoveSpeed | 5.5 |
| AttackPower | 1.05 |
| CritChance | 5% |
| CritMultiplier | 2.0 |
| Defense | 12 |
| PickupRadius | 3.0 |
| ExperienceMultiplier | 1.0 |
| DoubleAttackChance | 0% |
| DashCharges | 2 |
| HealthRegenPercent | 0.5% |

**Сильные стороны:**
- Хорошее здоровье и базовая защита.
- Умеренный AttackPower позволяет хорошо масштабироваться.
- Подходит для любого типа билда.

**Слабые стороны:**
- Нет особых бонусов. Средние статы везде.
- Не имеет доминирующего архетипа.

**Стартовое оружие:** Weapon_VikingAxe (Skill_VikingWhirl)  
**Стиль игры:** Кружить среди врагов, применять AoE навыки, собирать максимум опыта. Хорошо работает как кнопка "начни здесь".

---

## 2. Ассасин (Assassin)

**Архетип:** Быстрый рискованный боец с высоким критом.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 82 |
| MoveSpeed | 7.5 |
| AttackPower | 1.10 |
| CritChance | 12% |
| CritMultiplier | 2.3 |
| Defense | 0 |
| ParryChance | 8% |
| PickupRadius | 3.5 |
| ExperienceMultiplier | 1.0 |
| DoubleAttackChance | 0% |
| DashCharges | 3 |
| HealthRegenPercent | 0% |

**Сильные стороны:**
- Максимальная скорость передвижения.
- Высокий базовый крит.
- 3 заряда рывка.

**Слабые стороны:**
- Очень мало здоровья.
- Совсем нет защиты.
- Требует постоянного движения.

**Стартовое оружие:** Weapon_AssassinBlades (Skill_KnifeFan)  
**Стиль игры:** Движение = выживание. Постоянно в движении, строит билды вокруг крита и двойной атаки. Высокий потолок, высокое требование к навыку.

---

## 3. Рыцарь (Knight)

**Архетип:** Танк — медленный, но очень живучий.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 200 |
| MoveSpeed | 4.5 |
| AttackPower | 0.90 |
| CritChance | 3% |
| CritMultiplier | 2.0 |
| Defense | 30 |
| ParryChance | 15% |
| PickupRadius | 2.5 |
| ExperienceMultiplier | 1.0 |
| DoubleAttackChance | 0% |
| DashCharges | 1 |
| HealthRegenPercent | 2.0% |
| LifeTotemCount | 1 |

**Сильные стороны:**
- Максимальное здоровье.
- Самая высокая защита.
- 15% парирование = хорошая пассивная выживаемость.
- Воскрешение.

**Слабые стороны:**
- Самый низкий AttackPower.
- Медленный, трудно убегать.
- Только 1 заряд рывка.

**Стартовое оружие:** Weapon_KnightSword (Skill_KnightSwordArc)  
**Стиль игры:** Стоять на месте и получать урон. Строит оборонительные билды, выживает долго. Наказывает за неосторожные подходы.

---

## 4. Пиромант (Pyromancer)

**Архетип:** Маг с высоким уроном навыков и AoE.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 90 |
| MoveSpeed | 5.5 |
| AttackPower | 1.30 |
| CritChance | 8% |
| CritMultiplier | 2.2 |
| Defense | 0 |
| PickupRadius | 3.0 |
| ExperienceMultiplier | 1.0 |
| AreaMultiplier | 1.10 |
| DoubleAttackChance | 0% |
| DashCharges | 2 |
| HealthRegenPercent | 0% |

**Сильные стороны:**
- Самый высокий AttackPower — навыки наносят максимальный урон.
- Увеличенная площадь всех навыков.
- Хороший крит.

**Слабые стороны:**
- Мало здоровья.
- Нет защиты.
- Зависит от хорошего набора навыков.

**Стартовое оружие:** Weapon_FlameStaff (Skill_FireRain)  
**Стиль игры:** AoE-нуклер. Стоит в центре толпы и испепеляет всё вокруг. Очень слаб против боссов и элиток в соло.

---

## 5. Лучница (Ranger)

**Архетип:** Персонаж с бонусом к опыту и прогрессии.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 100 |
| MoveSpeed | 6.0 |
| AttackPower | 1.00 |
| CritChance | 7% |
| CritMultiplier | 2.0 |
| Defense | 5 |
| PickupRadius | 5.0 |
| ExperienceMultiplier | 1.30 |
| DoubleAttackChance | 5% |
| DashCharges | 2 |
| HealthRegenPercent | 0% |

**Сильные стороны:**
- Максимальный радиус подбора опыта.
- +30% XP = быстрее уровни = больше выборов.
- Хорошая скорость.

**Слабые стороны:**
- Средний урон.
- Среднее здоровье.
- Сила раскрывается через накопленные уровни, а не сразу.

**Стартовое оружие:** Weapon_Longbow (Skill_ArrowRain)  
**Стиль игры:** Держаться вдали, собирать как можно больше уровней, строить разнообразные билды. Раскрывается в поздней игре.

---

## 6. Некромант (Necromancer)

**Архетип:** Призыватель армии миньонов.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 95 |
| MoveSpeed | 5.0 |
| AttackPower | 1.00 |
| CritChance | 4% |
| CritMultiplier | 2.0 |
| Defense | 8 |
| PickupRadius | 3.0 |
| ExperienceMultiplier | 1.0 |
| DoubleAttackChance | 0% |
| DashCharges | 2 |
| HealthRegenPercent | 0.5% |

**Сильные стороны:**
- Миньоны поглощают удары и отвлекают врагов.
- Хорошо работает в паре с Summon-навыками.

**Слабые стороны:**
- Слабее всего без миньонов.
- Средние статы во всём.

**Стартовое оружие:** Weapon_BoneScepter (Skill_RaiseDead)  
**Стиль игры:** Строит армию, прячется за миньонами, добивает ослабленных врагов.

---

## 7. Паладин (Paladin)

**Архетип:** Защитный герой с регенерацией и AoE.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 160 |
| MoveSpeed | 5.0 |
| AttackPower | 0.95 |
| CritChance | 4% |
| CritMultiplier | 2.0 |
| Defense | 18 |
| ParryChance | 8% |
| PickupRadius | 3.0 |
| ExperienceMultiplier | 1.0 |
| DoubleAttackChance | 0% |
| DashCharges | 2 |
| HealthRegenPercent | 1.5% |

**Стартовое оружие:** Weapon_HolyHammer (Skill_HolyHammer)  
**Стиль игры:** Выживание через регенерацию и защиту. Хорошо работает в затяжных боях.

---

## 8. Рунный Кузнец (RuneSmith)

**Архетип:** Тяжёлый герой с рунной магией.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 140 |
| MoveSpeed | 4.8 |
| AttackPower | 1.10 |
| CritChance | 5% |
| CritMultiplier | 2.0 |
| Defense | 20 |
| PickupRadius | 2.8 |
| ExperienceMultiplier | 1.0 |
| DoubleAttackChance | 0% |
| DashCharges | 2 |
| HealthRegenPercent | 0.5% |

**Стартовое оружие:** Weapon_RuneHammer (Skill_RuneQuake)  
**Стиль игры:** Мощные земляные атаки, хороший для элитных врагов.

---

## 9. Вождь (Ragnar / RagnarIronfang)

**Архетип:** Агрессивный воин с двуручным оружием.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 130 |
| MoveSpeed | 5.8 |
| AttackPower | 1.15 |
| CritChance | 6% |
| CritMultiplier | 2.1 |
| Defense | 8 |
| PickupRadius | 2.5 |
| ExperienceMultiplier | 1.0 |
| DoubleAttackChance | 5% |
| DashCharges | 2 |
| HealthRegenPercent | 0% |

**Стартовое оружие:** Weapon_Ragnar_GreatAxe (Skill_AxeCleave)

---

## 10. Страж Бурь (StormSentinel)

**Архетип:** Молниеносный маг-воин.

| Параметр | Значение |
| --- | ---: |
| MaxHealth | 105 |
| MoveSpeed | 6.2 |
| AttackPower | 1.15 |
| CritChance | 6% |
| CritMultiplier | 2.0 |
| Defense | 5 |
| PickupRadius | 3.2 |
| ExperienceMultiplier | 1.0 |
| DoubleAttackChance | 3% |
| DashCharges | 2 |
| HealthRegenPercent | 0% |

**Стартовое оружие:** Weapon_StormSigil (Skill_StormJavelin)

---

## Сравнительная Таблица

| Персонаж | HP | Скорость | AttackPower | Защита | Крит | Архетип |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Viking | 150 | 5.5 | 1.05 | Защита 12 | 5% | Баланс |
| Assassin | 82 | 7.5 | 1.10 | — | 12% | Скорость/Крит |
| Knight | 200 | 4.5 | 0.90 | Защита 30, Пари 15% | 3% | Танк |
| Pyromancer | 90 | 5.5 | 1.30 | — | 8% | Нуклер |
| Ranger | 100 | 6.0 | 1.00 | Защита 5 | 7% | Прогрессия |
| Necromancer | 95 | 5.0 | 1.00 | Защита 8 | 4% | Призыватель |
| Paladin | 160 | 5.0 | 0.95 | Защита 18, Пари 8% | 4% | Выживание |
| RuneSmith | 140 | 4.8 | 1.10 | Защита 20 | 5% | Земляные удары |
| Ragnar | 130 | 5.8 | 1.15 | Защита 8 | 6% | Агрессия |
| StormSentinel | 105 | 6.2 | 1.15 | Защита 5 | 6% | Молнии |

---

## Проверка Баланса

При проверке персонажа убедись:

1. Самый хилый персонаж должен выжить хотя бы 3 минуты без улучшений.
2. Танк должен быть жив к 15-й минуте без единого навыка выживания.
3. Пиромант должен убивать быстрее Vikings в чистом DPS.
4. Ассасин должен иметь преимущество в мобильности, а не просто быть хуже рыцаря.
5. Лучница должна достигать уровня на 2–3 выше других персонажей при тех же условиях.
