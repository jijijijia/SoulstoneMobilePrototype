# UI Improvement Plan — SoulstoneMobilePrototype

Дата аудита: 2026-05-05  
Статус: Phase 3 в работе

---

## Сводка проблем

**Критические:**
- Нет Safe Area — контент попадает в вырезы/home indicator Android
- WeaponGrid (9 карт × 280px) и MapGrid (6 карт × 220px) в HorizontalLayout без ScrollRect — сломаны на мобильном
- Нет кнопки паузы в HUD

**Высокий приоритет:**
- XP текст красного цвета (цвет урона/опасности) — надо голубой/белый
- RunResultScreen все строки на английском ("Victory!", "Time:", "Kills:", "Skills:", "Affixes:")
- SkillCooldownSlotView: "Ready" на английском
- DDLevelUpManager: Update() polling для подписки
- Нет счётчика убийств в HUD

**Средний приоритет:**
- Смешение legacy Text и TextMeshPro (меню — legacy, HUD — TMP)
- Превью изображения карты (MapData.previewImage) не отображается
- Settings кнопки не показывают текущее значение после перезапуска
- Duplicate end-of-run UI: DDRunManager.gameOverPanel + RunResultScreen оба активны

**Низкий приоритет / Polish:**
- Нет fade-in при переходе между экранами меню
- Нет scale-punch анимации при нажатии кнопок
- SkillTree — заглушка, нет реального контента
- Нет иконок статов в preview персонажа
- UIStyleGuide.md не существует

---

## Phase 1 — Safe technical fixes

**Цель:** исправить технические ошибки без риска сломать логику.

### 1.1 SafeAreaFitter.cs
- Новый компонент `Assets/Scripts/UI/SafeAreaFitter.cs`
- Применяется к RectTransform, вписывает его в `Screen.safeArea`
- Вешается на корневой UI объект в обеих сценах
- **Инструкция:** добавить компонент на `MainMenu_Backdrop` в MainMenu и на `HUDPanel` в main

### 1.2 XP текст — исправить цвет
- Файл: `Assets/Editor/GameplayHudSceneBaker.cs` строка ~101
- Цвет `new Color(0.75f, 0f, 0f, 1f)` → `new Color(0.55f, 0.78f, 1f, 1f)` (холодный голубой)
- После: запустить `Soulstone → Bake Gameplay HUD Scene`

### 1.3 "Ready" → "Готов"
- Файл: `Assets/Scripts/UI/SkillCooldownSlotView.cs`
- Строки 54 и 64: `"Ready"` → `"Готов"`

### 1.4 Перевод RunResultScreen
- Файл: `Assets/Scripts/UI/RunResultScreen.cs`
- "Victory!" → "Победа!"
- "Game Over" → "Поражение"
- "Time: " → "Время: "
- "Level: " → "Уровень: "
- "Kills: " → "Убийств: "
- "Skills: " → "Навыки: "
- "Affixes: " → "Аффиксы: "

### 1.5 DDLevelUpManager — убрать Update() polling
- Файл: `Assets/Scripts/UI/DDLevelUpManager.cs`
- Убрать `Update()` метод
- В `Start()` добавить `Invoke(nameof(RetrySubscription), 0.1f)` если не подписался

**Статус Phase 1:** ✅ Завершена

---

## Phase 2 — Mobile layout fixes

**Цель:** исправить layout-проблемы из-за которых UI ломается на мобильных.

### 2.1 WeaponGrid — горизонтальный ScrollRect
- `MainMenuManager.cs` — метод `EnsureWeaponPanel()`
- Обернуть WeaponGrid в ScrollRect контейнер с горизонтальным скроллом
- Отключить вертикальный скролл, включить горизонтальный

### 2.2 MapGrid — горизонтальный ScrollRect
- `MainMenuManager.cs` — метод `EnsurePlayPanel()`
- То же самое для MapGrid

### 2.3 Pause button в HUD
- `Assets/Editor/GameplayHudSceneBaker.cs` — добавить PauseButton в правый верхний угол
- `Assets/Scripts/UI/DDHUDManager.cs` — добавить SerializeField Button pauseButton + подписку на DDRunManager

### 2.4 Kill counter в HUD
- `Assets/Editor/GameplayHudSceneBaker.cs` — добавить KillsText
- `Assets/Scripts/UI/DDHUDManager.cs` — подписаться на SpawnSystem.KillCountChanged

**Статус Phase 2:** ✅ Завершена

---

## Phase 3 — Gameplay HUD improvements

**Цель:** улучшить информативность и читаемость боевого HUD.

### 3.1 HP Danger state
- При HP < 30% текст здоровья становится красным
- Подписка на HealthChanged в DDHUDManager

### 3.2 HUD визуальная панель
- Добавить полупрозрачный фон-панель под HP/XP/Level элементы
- Добавить полупрозрачный фон под cooldown слоты

### 3.3 Cooldown слоты — размер на мобильном
- Текущий размер 96×72 может быть мелким на телефоне
- Проверить на реальном устройстве, при необходимости увеличить до 110×88

**Статус Phase 3:** ⏳ Ожидает

---

## Phase 4 — Main menu polish

**Цель:** улучшить визуал и информативность главного меню.

### 4.1 Миграция карточек на TMP
- `MenuCardViewBase.cs` — заменить `Text` на `TMP_Text`
- `CharacterCardView.cs`, `WeaponCardView.cs`, `MapCardView.cs` — обновить типы
- `MainMenuManager.cs` — обновить `CreateText()`, `EnsureChildText()`, `CreateSceneButton()`

### 4.2 Превью карты
- `MapInfoPanelView.cs` — добавить Image для MapData.previewImage
- `MainMenuManager.cs` — передавать previewImage в MapDetails панель

### 4.3 Settings — показывать текущие значения
- При `LoadSettings()` обновлять текст QualityButton и LanguageButton текущим значением из PlayerPrefs
- При `CycleSetting()` обновлять текст кнопки

### 4.4 Логотип
- Заменить legacy Text логотипа на TMP с более красивым шрифтом

**Статус Phase 4:** ✅ Завершена

---

## Phase 5 — Animation polish

**Цель:** добавить минимальный визуальный polish без тяжёлых библиотек.

### 5.1 Fade-in панелей в меню
- Добавить CanvasGroup на каждый Page объект
- При `ShowScreen()` корутина: alpha 0 → 1 за 0.12 сек

### 5.2 Button press punch
- В `MenuCardViewBase` добавить корутину scale punch при нажатии
- Scale: 1.0 → 0.93 → 1.0 за 0.1 сек

### 5.3 Level-up карточки highlight
- При hover/selected — лёгкое свечение через Outline или color lerp

**Статус Phase 5:** ✅ Завершена

---

## Phase 6 — Style guide

**Цель:** зафиксировать дизайн-систему для единообразия.

### 6.1 Docs/UIStyleGuide.md
- Цветовая палитра (hex коды)
- Размеры кнопок (mobile min 48dp)
- Размеры шрифтов (иерархия)
- Правила отступов
- Состояния кнопок
- Safe area правила
- Правила для русского текста

### 6.2 UITheme ScriptableObject (опционально)
- Только если проект масштабируется
- Для прототипа — достаточно StyleGuide документа

**Статус Phase 6:** ✅ Завершена

---

## Цветовая палитра (референс)

| Назначение | Hex | RGB (0-1) |
|---|---|---|
| Panel background | #1C1F25 | (0.11, 0.12, 0.145) |
| Card background | #2B2E36 | (0.17, 0.18, 0.21) |
| Accent / кнопка | #DCAE52 | (0.86, 0.68, 0.32) |
| Текст основной | #FFFFFF | (1, 1, 1) |
| Текст вторичный | #B8C0CC | (0.72, 0.75, 0.80) |
| XP / прогресс | #8CC8FF | (0.55, 0.78, 1.0) |
| HP нормальный | #FFFFFF | (1, 1, 1) |
| HP опасность (<30%) | #FF4444 | (1, 0.27, 0.27) |
| Cooldown fill | #F2B838 | (0.95, 0.72, 0.22) |
| Slot background | #0A0B0E | (0.04, 0.045, 0.055) |

---

## Размеры кнопок (mobile)

| Тип | Минимум | Рекомендовано |
|---|---|---|
| Основная кнопка меню | 48px высота | 56-64px высота |
| Кнопка выбора (SELECT) | 54px высота | 60px высота |
| Карточка персонажа | 220×230px | 220×230px |
| Карточка оружия | 260×280px | 260×280px |
| Карточка карты | 200×200px | 210×210px |
| Cooldown слот | 96×72px | 110×88px |
| Кнопка паузы (touch) | 64×64px | 72×72px |
