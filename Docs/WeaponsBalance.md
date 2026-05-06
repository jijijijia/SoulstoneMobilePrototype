# Баланс Оружия

Этот документ описывает все оружия проекта: роли, стартовые навыки, пулы навыков и синергии.

---

## Принципы Оружия

- Оружие не наносит урон напрямую. Оно задаёт стартовый навык и пул навыков.
- Каждый персонаж должен иметь минимум 2–3 оружия с разными стилями.
- Оружие определяет архетип билда: АоЕ-воин, дальник, призыватель, элементалист.
- Пул навыков оружия должен быть тематически связан с его визуальным образом.

---

## Warrior — Viking

### Weapon_VikingAxe

| Поле | Значение |
| --- | --- |
| ID | weapon_viking_axe |
| Display Name | Топор Викинга |
| Визуальный тип | Топор |
| Стартовый навык | Skill_VikingWhirl |
| Боевая роль | АоЕ-воин ближнего боя |

**Пул навыков:** VikingWhirl, AxeCleave, DoubleCleave, RuneQuake, HolyHammer, BoulderThrow  
**Хорошие пассивки:** AreaMastery, EchoStrike, AllSkillDamage, Bulwark  
**Стиль:** Вращение в центре толпы, постоянный АоЕ урон.

---

### Weapon_RuneHammer (для Viking — второй вариант)

| Поле | Значение |
| --- | --- |
| ID | weapon_rune_hammer |
| Display Name | Рунный Молот |
| Визуальный тип | Молот |
| Стартовый навык | Skill_RuneQuake |
| Боевая роль | Земляная АоЕ-атака |

**Пул навыков:** RuneQuake, BoulderThrow, AxeCleave, DoubleCleave, StoneSpear, HolyHammer  
**Хорошие пассивки:** AreaMastery, AllSkillDamage, Precision

---

## Assassin — Ассасин

### Weapon_AssassinBlades

| Поле | Значение |
| --- | --- |
| ID | weapon_assassin_blades |
| Display Name | Клинки Ассасина |
| Визуальный тип | Двойные кинжалы |
| Стартовый навык | Skill_KnifeFan |
| Боевая роль | Быстрые снаряды, высокий крит |

**Пул навыков:** KnifeFan, AssassinKnifeStorm, StoneSpear, ChainLightning, LightningSeries, KnightSwordArc  
**Хорошие пассивки:** Precision, EchoStrike, ProjectileMastery, Overcharge  
**Стиль:** Веер снарядов, быстрые кулдауны, крит на каждом попадании.

---

### Weapon_GhostWand (для Assassin — второй вариант)

| Поле | Значение |
| --- | --- |
| ID | weapon_ghost_wand |
| Display Name | Жезл Призрака |
| Визуальный тип | Посох теней |
| Стартовый навык | Skill_ChainLightning |
| Боевая роль | Цепные теневые удары |

**Пул навыков:** ChainLightning, LightningSeries, KnifeFan, AssassinKnifeStorm, StormCall, PoisonMire  
**Хорошие пассивки:** Precision, Overcharge, EchoStrike

---

## Knight — Рыцарь

### Weapon_KnightSword

| Поле | Значение |
| --- | --- |
| ID | weapon_knight_sword |
| Display Name | Меч Рыцаря |
| Визуальный тип | Одноручный меч |
| Стартовый навык | Skill_KnightSwordArc |
| Боевая роль | Фронтальная атака, высокий урон |

**Пул навыков:** KnightSwordArc, DoubleCleave, AxeCleave, HolyHammer, BoulderThrow, VikingWhirl  
**Хорошие пассивки:** AllSkillDamage, Bulwark, Renewal, EchoStrike  
**Стиль:** Стоять фронтом к врагам, поглощать урон и наносить мощные удары.

---

### Weapon_HolyHammer

| Поле | Значение |
| --- | --- |
| ID | weapon_holy_hammer |
| Display Name | Святой Молот |
| Визуальный тип | Молот |
| Стартовый навык | Skill_HolyHammer |
| Боевая роль | Защитный АоЕ, лёгкое исцеление |

**Пул навыков:** HolyHammer, RuneQuake, VikingWhirl, BoulderThrow, ArrowRain, Armageddon  
**Хорошие пассивки:** Bulwark, Renewal, AreaMastery, SoulTotem

---

## Pyromancer — Пиромант

### Weapon_FlameStaff

| Поле | Значение |
| --- | --- |
| ID | weapon_flame_staff |
| Display Name | Жезл Пламени |
| Визуальный тип | Посох с огнём |
| Стартовый навык | Skill_FireRain |
| Боевая роль | Огненный дождь, покрытие площади |

**Пул навыков:** FireRain, Armageddon, StormCall, RuneQuake, HolyHammer, BoulderThrow  
**Хорошие пассивки:** AreaMastery, AllSkillDamage, Precision, EchoStrike  
**Стиль:** Постоянно покрывать поле пожарами и взрывами. Мощный в толпе, слабее против одиночек.

---

### Weapon_SoulScythe (для Pyromancer — второй вариант)

| Поле | Значение |
| --- | --- |
| ID | weapon_soul_scythe |
| Display Name | Коса Душ |
| Визуальный тип | Коса |
| Стартовый навык | Skill_Armageddon |
| Боевая роль | Высокий урон с долгим КД |

**Пул навыков:** Armageddon, FireRain, StormCall, BoulderThrow, PoisonMire, VikingWhirl  
**Хорошие пассивки:** AllSkillDamage, Precision, EchoStrike

---

## Ranger — Лучница

### Weapon_Longbow

| Поле | Значение |
| --- | --- |
| ID | weapon_longbow |
| Display Name | Длинный Лук |
| Визуальный тип | Лук |
| Стартовый навык | Skill_ArrowRain |
| Боевая роль | Дальние атаки, покрытие области |

**Пул навыков:** ArrowRain, SpearVolley, StoneSpear, KnifeFan, ChainLightning, Armageddon  
**Хорошие пассивки:** ProjectileMastery, Precision, PickupRadius, EchoStrike  
**Стиль:** Держаться на расстоянии, держать врагов под стрелами, быстро собирать уровни.

---

### Weapon_StormBow (для Ranger — второй вариант)

| Поле | Значение |
| --- | --- |
| ID | weapon_storm_bow |
| Display Name | Грозовой Лук |
| Визуальный тип | Лук с молнией |
| Стартовый навык | Skill_LightningSeries |
| Боевая роль | Быстрые молниеносные выстрелы |

**Пул навыков:** LightningSeries, StormCall, ChainLightning, ArrowRain, StormJavelin, SpearVolley  
**Хорошие пассивки:** Overcharge, ProjectileMastery, Precision

---

## Necromancer — Некромант

### Weapon_BoneScepter

| Поле | Значение |
| --- | --- |
| ID | weapon_bone_scepter |
| Display Name | Посох Костей |
| Визуальный тип | Посох черепа |
| Стартовый навык | Skill_RaiseDead |
| Боевая роль | Призыв скелетов |

**Пул навыков:** RaiseDead, PoisonMire, ChainLightning, Armageddon, StormCall, VikingWhirl  
**Хорошие пассивки:** AllSkillDamage, EchoStrike, Renewal, SoulTotem

---

### Weapon_ThunderSpear (для Necromancer — второй вариант)

| Поле | Значение |
| --- | --- |
| ID | weapon_thunder_spear |
| Display Name | Копьё Грома |
| Визуальный тип | Копьё с молнией |
| Стартовый навык | Skill_StormJavelin |
| Боевая роль | Пробивающий электрический снаряд |

**Пул навыков:** StormJavelin, ChainLightning, LightningSeries, StormCall, SpearVolley, StoneSpear  
**Хорошие пассивки:** ProjectileMastery, Precision, Overcharge

---

## Сводная Таблица Оружий

| Оружие | Персонаж | Стартовый навык | Роль |
| --- | --- | --- | --- |
| Weapon_VikingAxe | Viking | VikingWhirl | АоЕ ближний бой |
| Weapon_RuneHammer | RuneSmith / Viking | RuneQuake | Земляные удары |
| Weapon_AssassinBlades | Assassin | KnifeFan | Быстрые снаряды |
| Weapon_GhostWand | Assassin | ChainLightning | Цепные атаки |
| Weapon_KnightSword | Knight | KnightSwordArc | Фронтальный удар |
| Weapon_HolyHammer | Knight / Paladin | HolyHammer | Защитный АоЕ |
| Weapon_FlameStaff | Pyromancer | FireRain | Огневой дождь |
| Weapon_SoulScythe | Pyromancer | Armageddon | Сильный нюк |
| Weapon_Longbow | Ranger | ArrowRain | Дальние АоЕ |
| Weapon_StormBow | Ranger | LightningSeries | Молниеносные выстрелы |
| Weapon_BoneScepter | Necromancer | RaiseDead | Призыватель |
| Weapon_ThunderSpear | Necromancer / StormSentinel | StormJavelin | Пробивающий |
| Weapon_StormSigil | StormSentinel | StormJavelin | Молнии |
| Weapon_Ragnar_GreatAxe | Ragnar | AxeCleave | Мощный взмах |

---

## Рекомендации по Созданию Нового Оружия

1. Стартовый навык оружия должен быть доступен немедленно — не делай его слишком сильным.
2. Пул навыков должен содержать 5–8 навыков разных типов.
3. Добавляй хотя бы 1 навык из другой стихии — это поощряет гибридные билды.
4. Одно и то же оружие может быть доступно нескольким персонажам.
5. Если два оружия у одного персонажа имеют похожие пулы — это плохо. Разводи их.
