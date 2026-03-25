# UI Art Direction — Tetris vs Character

**Версия:** 1.0 | **Дата:** 2026-03-24 | **Движок:** Unity + Photon PUN2

---

## Содержание

1. [Visual Identity & Pillars](#1-visual-identity--pillars)
2. [Color System](#2-color-system)
3. [Typography & Localization](#3-typography--localization)
4. [Layout & Visual Hierarchy](#4-layout--visual-hierarchy)
5. [HUD — Player 1 (Tetris)](#5-hud--player-1-tetris)
6. [HUD — Player 2 (Character)](#6-hud--player-2-character)
7. [Panel & Container Design](#7-panel--container-design)
8. [Animation & Feedback](#8-animation--feedback)
9. [Cross-Platform Scaling](#9-cross-platform-scaling)
10. [Canvas Setup — Screen Space Camera](#10-canvas-setup--screen-space-camera)
11. [Menu Canvas Setup](#11-menu-canvas-setup)
12. [Asset Specifications & Naming](#12-asset-specifications--naming)
13. [Accessibility Baseline](#13-accessibility-baseline)

---

## 1. Visual Identity & Pillars

### Core Concept

"Tetris vs Character" — асимметричная игра с двумя принципиально разными
игровыми ролями. Визуальный язык должен отражать эту дуальность: **архитектурный
контроль** Player 1 против **выживания под давлением** Player 2.

### Three Visual Pillars

**Pillar 1 — Читаемость прежде всего.**
В любой момент матча оба игрока должны мгновенно читать критическую информацию
(позиция персонажа, следующая фигура, cooldown). Ни один декоративный элемент не
должен конкурировать с игровой информацией.

**Pillar 2 — Контраст ролей.**
UI Player 1 (тетрис) — холодный, структурный, геометрический. UI Player 2
(персонаж) — теплый, органичный, более динамичный. Это не просто эстетика: игрок
за секунду должен понять, чей это элемент.

**Pillar 3 — Честная обратная связь.**
Каждое игровое состояние (cooldown готов, exit заблокирован, фитиль горит) имеет
однозначный визуальный сигнал. Нет состояния "угадай сам".

---

## 2. Color System

### Primary Palette

| Токен | Hex | Применение |
|---|---|---|
| `color-board-bg` | `#0A0A14` | Фон игрового поля |
| `color-board-grid` | `#1A1A2E` | Линии сетки (opacity 40%) |
| `color-ui-bg-dark` | `#12121F` | Фон панелей, overlay |
| `color-ui-bg-mid` | `#1E1E35` | Фон вторичных контейнеров |
| `color-ui-border` | `#2A2A4A` | Обводки панелей |
| `color-text-primary` | `#E8E8F0` | Основной текст |
| `color-text-secondary` | `#8888AA` | Подписи, неактивное |
| `color-text-disabled` | `#444460` | Недоступные элементы |

### Player Identity Colors

| Токен | Hex | Применение |
|---|---|---|
| `color-p1-primary` | `#4A9EFF` | Player 1 акцент (холодный синий) |
| `color-p1-glow` | `#4A9EFF40` | Свечение P1 элементов |
| `color-p2-primary` | `#FF7A3D` | Player 2 акцент (теплый оранжевый) |
| `color-p2-glow` | `#FF7A3D40` | Свечение P2 элементов |

### Tetromino Colors (Tetris Guideline)

| Фигура | Hex | Примечание |
|---|---|---|
| I | `#00F0F0` | Cyan |
| O | `#F0F000` | Yellow |
| T | `#A000F0` | Purple |
| S | `#00F000` | Green |
| Z | `#F00000` | Red |
| J | `#0000F0` | Blue |
| L | `#F0A000` | Orange |
| Ghost piece | Базовый цвет, alpha `0.6`, brightness `75%` | |
| BombNPC ghost | `#FF6600` | Оранжевый, alpha `0.7` |

### Semantic / State Colors

| Токен | Hex | Применение |
|---|---|---|
| `color-state-success` | `#40D080` | Exit открыт, победа P2 |
| `color-state-danger` | `#FF3A3A` | Exit заблокирован, угроза смерти |
| `color-state-warning` | `#FFB020` | Фитиль BombNPC, предупреждение |
| `color-state-cooldown` | `#6060AA` | Способность на перезарядке |
| `color-state-ready` | `#80FFB0` | Способность доступна |

### Запрещенные комбинации

- Никогда не использовать `color-p1-primary` для P2 UI элементов и наоборот.
- Белый текст (#FFFFFF) на `color-board-bg` — допустимо только для score
  (максимальный контраст). Везде остальное — `color-text-primary`.
- Красный и зеленый никогда не должны быть единственным индикатором состояния
  (учитывается дальтонизм — добавляется форма или иконка).

---

## 3. Typography & Localization

### Требования к шрифту

Игра поддерживает кириллицу и потенциально другие языки. Это исключает
большинство стандартных игровых шрифтов (Pixel, Press Start 2P), которые не
покрывают кириллицу.

### Рекомендуемые шрифты

**Основной выбор: Rajdhani (Google Fonts)**
- Покрытие: Latin Extended. **Для кириллицы не подходит** — исключить.

**Рекомендуемый выбор: Exo 2 (Google Fonts)**
- Покрытие: Latin + Cyrillic (полный набор)
- Стиль: геометрический, технологичный, хорошо читается в малых размерах
- Лицензия: SIL Open Font License — свободно в коммерческих проектах
- Начертания для игры: Regular (400), SemiBold (600), Bold (700)
- Скачать: https://fonts.google.com/specimen/Exo+2

**Альтернатива: Rubik (Google Fonts)**
- Покрытие: Latin + Cyrillic + Hebrew
- Стиль: мягче, округлее — меньше подходит архитектурной эстетике P1
- Лицензия: SIL Open Font License

**Альтернатива для акцентных заголовков: Orbitron (Google Fonts)**
- Покрытие: Latin Only — **НЕ использовать для кириллического текста**
- Допустимо только для числовых значений (счёт, таймер), которые не локализуются

### Установка в Unity (TextMeshPro)

1. Скачать TTF файлы Exo 2 (Regular, SemiBold, Bold).
2. В Unity: `Window → TextMeshPro → Font Asset Creator`.
3. Character Set: **Custom Range** — включить диапазоны:
   - Basic Latin: U+0020–U+007E
   - Latin Extended-A: U+0100–U+017F
   - Cyrillic: U+0400–U+04FF
   - Cyrillic Supplement: U+0500–U+052F (опционально)
4. Atlas Resolution: 2048×2048 для полного покрытия
5. Render Mode: `SDF32` (для масштабирования без артефактов на разных экранах)
6. Создать три Font Asset: `font_exo2_regular`, `font_exo2_semibold`, `font_exo2_bold`
7. Поместить в `Assets/Resources/Fonts/`

### Размеры текста (TMP Font Size)

Все размеры в единицах Reference Resolution (1920×1080). Для Scale Mode
`Scale With Screen Size` TMP масштабирует автоматически.

| Роль | Font Size | Начертание | Применение |
|---|---|---|---|
| `text-xl` | 72 | Bold | Результат матча (победа/поражение) |
| `text-lg` | 48 | Bold | Счёт игрока, крупные числа |
| `text-md` | 32 | SemiBold | Заголовки панелей, кнопки меню |
| `text-sm` | 24 | Regular | Подписи, статус cooldown |
| `text-xs` | 18 | Regular | Вспомогательные подписи, версия |

Минимальный допустимый размер для локализованного текста: **18px** (text-xs).
Текст меньше 18px никогда не локализуется — только иконки или числа.

### Локализация: практические правила

**Расширение текста.** Русский текст в среднем на 30–40% длиннее английского.
Все текстовые контейнеры проектируются с запасом `×1.4` от английской версии.

**Метод:** использовать Unity Localization Package (`com.unity.localization`).
- String Tables для всех UI строк
- Никаких хардкодированных строк в Prefab и коде
- Ключи вида: `UI_MENU_PLAY`, `UI_HUD_COOLDOWN_READY`, `UI_WIN_P1`

**Шрифт по языку.** В Unity Localization можно переключать Font Asset по Locale.
Для языков с нелатинским письмом (арабский, японский) — отдельный Font Asset
с соответствующим покрытием Unicode.

---

## 4. Layout & Visual Hierarchy

### Информационная иерархия (в порядке приоритета)

1. **Игровое поле** — занимает центр экрана, максимальный размер
2. **Позиция персонажа внутри поля** — всегда видна, не перекрывается HUD
3. **Текущая/следующая фигура** — P1 панель, правая сторона
4. **Exit статус** — цветовой индикатор всегда в поле зрения P2
5. **Cooldown индикаторы** — P2 HUD, нижний край
6. **Счёт / таймер** — верхний край, минимальный footprint
7. **Сервисная информация** (пинг, режим) — угол, минимальный размер

### Принцип зонирования

```
┌─────────────────────────────────────────────────┐
│  [P1 Score]          [Timer]          [P2 Score] │  <- Header zone (8% высоты)
├──────────┬──────────────────────────┬────────────┤
│          │                          │            │
│  P1 HUD  │      GAME BOARD          │  P1 HUD    │
│ (Preview)│      10×20 grid          │ (Controls) │
│          │                          │            │
│          │   [CHARACTER inside]     │            │
│          │                          │            │
└──────────┴──────────────────────────┴────────────┤
│           [P2 HUD: Cooldowns / Abilities]         │  <- Footer zone (10% высоты)
└─────────────────────────────────────────────────┘
```

Игровое поле занимает **центральные 60%** ширины. P1-панели — левые и правые
**20%** каждая. Это сохраняется на всех разрешениях через Anchors.

---

## 5. HUD — Player 1 (Tetris)

### Next Piece Preview Panel

**Расположение:** правая панель, верхняя треть.

**Визуал:**
- Контейнер `ui_panel_preview`: `color-ui-bg-dark`, border 1px `color-p1-primary`
- Внутри: сетка 4×4 клетки, каждая клетка 32×32px (Reference Resolution)
- Подпись "NEXT" / локализованный ключ `UI_P1_NEXT`: `text-xs`, `color-text-secondary`
- Фигура рендерится теми же цветами, что и игровые блоки

**Drag-and-drop взаимодействие:**
- В состоянии Idle: фигура в preview слегка пульсирует (scale 1.0→1.03, 1.2 сек loop)
- При начале drag: preview подсвечивается `color-p1-glow`
- Ghost piece на поле появляется немедленно при drag, синхронно с позицией курсора

### Счёт Player 1

**Расположение:** header zone, левая треть.

**Визуал:**
- Label "P1" или имя игрока: `text-sm`, `color-p1-primary`
- Числовое значение: `text-lg`, `color-text-primary`, font Exo 2 Bold
- При увеличении счёта: число "прыгает" scale 1.0→1.2→1.0 за 0.3 сек

### BombNPC Control (если P1 управляет спавном)

**Расположение:** правая панель, под Preview.

**Визуал:**
- Иконка бомбы (спрайт `ui_icon_bomb_idle`) с подписью `UI_P1_BOMB`
- Состояние Ready: иконка `color-state-ready`, пульсация
- Состояние Cooldown: иконка `color-state-cooldown`, radial fill overlay показывает
  прогресс перезарядки

---

## 6. HUD — Player 2 (Character)

Этот раздел решает критический пробел из GDD (Balance Note #6: нет UI cooldown).

### Cooldown Bar

**Расположение:** footer zone, по центру.

**Структура:**
```
[DESTROY SIDE icon] [radial/bar fill] | [DESTROY DOWN icon] [radial/bar fill]
```

**Спрайты иконок:**
- `ui_icon_destroy_side_ready` / `ui_icon_destroy_side_cooldown`
- `ui_icon_destroy_down_ready` / `ui_icon_destroy_down_cooldown`

**Поведение:**
- Состояние Ready (cooldown = 0): иконка `color-state-ready`, легкое свечение
- Состояние Cooldown (timer > 0): иконка `color-state-cooldown`, radial fill
  убывает от 1 до 0 за 5 сек. Цвет fill `color-p2-primary`
- За 0.5 сек до окончания cooldown: иконка flash (alpha 0→1→0, 3 раза) — сигнал
  "почти готово"
- Момент готовности: scale bump 1.0→1.3→1.0 за 0.25 сек + звуковой сигнал

**Важно:** X и Z используют общий таймер `_destroyCooldownTimer`. Оба индикатора
должны обновляться от одного значения — это честно отражает игровой дизайн.

### Exit Status Indicator

**Расположение:** footer zone, правый край.

**Визуал:**
- Иконка двери + цветная точка статуса
- Open: `color-state-success` + текст `UI_P2_EXIT_OPEN`
- Blocked: `color-state-danger` + текст `UI_P2_EXIT_BLOCKED`
- Иконка анимируется: Open — пульсация scale, Blocked — shake по горизонтали

При изменении статуса (открылся/закрылся): brief flash overlay поверх индикатора.

### Push Indicator

**Расположение:** footer zone, левый край.

**Визуал:**
- Иконка `ui_icon_push` (стрелка толчка)
- Если BombNPC в воздухе в радиусе действия: иконка `color-state-ready` + pulse
- Иначе: `color-text-disabled`
- Текст подсказки управления (только на PC): `Keypad0/Ins`

### Счёт / Статус Player 2

**Расположение:** header zone, правая треть.

**Визуал:** зеркалит P1, но с `color-p2-primary` вместо `color-p1-primary`.

---

## 7. Panel & Container Design

### Panel Style (общий)

Все панели используют единую визуальную систему:

- **Фон:** `color-ui-bg-dark` с opacity `0.92` — частично прозрачный, игра
  просвечивает за пустыми зонами
- **Border:** 1px `color-ui-border`. Угловые пиксели скруглены radius `4px`
- **Акцентная полоса:** 2px горизонтальная линия у верхнего края панели.
  P1 панели: `color-p1-primary`. P2 панели: `color-p2-primary`
- **Внутренние отступы (Padding):** 12px со всех сторон

### Panel Variants

| Вариант | Применение | Отличие |
|---|---|---|
| `panel-primary` | Preview, Score, Cooldown HUD | Описан выше |
| `panel-overlay` | Victory/Defeat screen | opacity `0.96`, border `color-state-success` или `color-state-danger` |
| `panel-menu` | Lobby, Room screen | Фон `color-ui-bg-mid`, border `color-p1-primary` |
| `panel-tooltip` | Hover подсказки (PC only) | Мини-версия, max-width 200px |

### Victory / Defeat Overlay

Показывается поверх всего. `Time.timeScale = 0` уже установлен движком.

**Состав:**
1. Полупрозрачный overlay на весь экран: `color-board-bg` opacity `0.7`
2. Центральная карточка `panel-overlay`: ширина 480px, высота auto
3. Заголовок победителя: `text-xl`, Bold. P1 win: `color-p1-primary`. P2 win:
   `color-p2-primary`
4. Подзаголовок (локализованная строка): `text-sm`, `color-text-secondary`
5. Кнопка Restart: `ui_btn_primary`
6. Появление карточки: slide-in снизу + fade, duration 0.4 сек, ease OutBack

---

## 8. Animation & Feedback

### Принцип Juice

Каждое значимое игровое событие имеет визуальный отклик. Без анимации нет
ощущения веса и отдачи.

### Таблица анимаций UI

| Событие | Элемент | Анимация | Duration |
|---|---|---|---|
| Фигура зафиксирована (Lock) | Счёт P1 | Scale bump 1.0→1.15→1.0 | 0.2 сек |
| Линия очищена | Весь board overlay | Flash white opacity 0→0.3→0 | 0.15 сек |
| Персонаж умер | P2 HUD | Shake horizontal ±4px, 3 раза | 0.3 сек |
| Cooldown завершён | Иконка способности | Scale bump 1.0→1.3→1.0 | 0.25 сек |
| Exit открылся | Exit indicator | Scale 1.0→1.2→1.0 + color flash | 0.4 сек |
| Exit закрылся | Exit indicator | Shake ±3px + color flash red | 0.3 сек |
| BombNPC фитиль | Иконка bomb | Flash rate ускоряется с 1 Гц до 4 Гц | 2 сек |
| Victory/Defeat | Overlay panel | Slide-in + fade | 0.4 сек |
| Кнопка hover (PC) | Кнопка | Scale 1.0→1.05, color → lighter | 0.1 сек |
| Кнопка press | Кнопка | Scale 1.0→0.95→1.0 | 0.1 сек |

### Правила анимации

- Все анимации UI через **DOTween** (уже используется в проекте для camera shake).
- Анимации не должны блокировать ввод. Cooldown icon анимируется поверх
  рабочего cooldown timer.
- При `Time.timeScale = 0` (victory screen) анимации overlay продолжают работать
  через `DOTween.SetUpdate(true)` (ignoreTimeScale = true).
- Нет анимаций длиннее 0.5 сек для UI элементов реального времени — это
  затуманивает обратную связь.

---

## 9. Cross-Platform Scaling

### Canvas Scaler Settings

Оба игровых Canvas (P1 и P2) и меню Canvas используют одинаковые настройки
Canvas Scaler:

```
UI Scale Mode:          Scale With Screen Size
Reference Resolution:   1920 × 1080
Screen Match Mode:      Match Width Or Height
Match:                  0.5  (50/50 width and height)
```

`Match = 0.5` — компромисс. Чисто ширина (0.0) ломает вертикальные устройства.
Чисто высота (1.0) ломает горизонтальные ультраширокие. 0.5 адаптируется к обоим.

**Для игры с вертикально ориентированным полем (10×20)** — рассмотреть `Match = 0.7`
(ближе к высоте), чтобы поле занимало максимум вертикального пространства на
мобильных устройствах.

### Safe Zone (Mobile Notch / Rounded Corners)

На iOS и Android с вырезами (notch, punch-hole) системные элементы перекрывают
углы экрана. Решение в Unity:

1. Добавить скрипт `SafeAreaPanel.cs` на корневой RectTransform каждого Canvas.
2. Скрипт применяет `Screen.safeArea` к RectTransform `offsetMin`/`offsetMax`.
3. Весь интерфейс внутри SafeAreaPanel автоматически прижимается к безопасной зоне.

**Буфер:** не размещать критические UI элементы ближе 20px (reference) к краям
экрана даже в safe zone — это выглядит тесно.

### Touch Targets

На мобильных устройствах минимальный размер касаемого элемента — **48×48 dp**.
В единицах Reference Resolution (1920×1080, при PPI ~160): **48px**.

| Элемент | Минимум | Рекомендовано |
|---|---|---|
| Кнопка меню | 48×48px | 64×48px |
| Иконка cooldown (tap-to-use, если реализовано) | 48×48px | 64×64px |
| Drag-and-drop фигуры | Hit zone 48×48px вокруг центра | Вся фигура |
| Кнопка Restart | 48×48px | 200×64px |

На PC размеры кнопок могут быть меньше (32px высота), если touch mode недоступен.
Рекомендуется единый размер 48px для совместимости.

### Ориентация экрана

Игровое поле 10×20 — вертикальное. Рекомендуется:
- **PC:** landscape only (поле + боковые панели вписываются)
- **Mobile:** landscape preferred. Если portrait — поле сжимается, боковые
  панели перемещаются вниз/вверх (требует отдельный layout вариант)

Unity Player Settings: `Player → Resolution and Presentation → Orientation → Landscape`.

### Breakpoints (Reference)

| Разрешение | Сценарий | Проверить |
|---|---|---|
| 1920×1080 | PC Reference | Базовый layout |
| 1280×720 | Старые PC / ноутбуки | Текст не обрезается |
| 2560×1440 | Современные PC | Нет пикселизации шрифтов |
| 390×844 | iPhone 14 (portrait) | Safe area, touch targets |
| 844×390 | iPhone 14 (landscape) | Поле вписывается |
| 1668×2388 | iPad (portrait) | Layout не разваливается |

---

## 10. Canvas Setup — Screen Space Camera

### Структура: два отдельных Canvas

Во время матча — два Canvas, каждый привязан к своей камере. Это позволяет
независимо управлять depth, post-processing и видимостью.

```
Scene Hierarchy:
├── Camera_P1_Game          (Camera компонент, Depth: 0)
├── Camera_P2_Game          (Camera компонент, Depth: 1)
├── Canvas_P1_HUD           (Canvas, Render Mode: Screen Space - Camera)
│   ├── Panel_Preview       (Next Piece + BombNPC control)
│   ├── Text_P1_Score
│   └── ...
└── Canvas_P2_HUD           (Canvas, Render Mode: Screen Space - Camera)
    ├── Panel_Cooldowns     (Destroy Side + Destroy Down indicators)
    ├── Indicator_Exit
    ├── Indicator_Push
    ├── Text_P2_Score
    └── ...
```

### Canvas_P1_HUD — Inspector Settings

```
Canvas:
  Render Mode:        Screen Space - Camera
  Render Camera:      Camera_P1_Game
  Plane Distance:     100
  Sorting Layer:      UI
  Order in Layer:     0

Canvas Scaler:
  UI Scale Mode:      Scale With Screen Size
  Reference Res:      1920 × 1080
  Screen Match Mode:  Match Width Or Height
  Match:              0.5

Graphic Raycaster:
  Blocking Mask:      Everything  (для drag-and-drop P1)
```

### Canvas_P2_HUD — Inspector Settings

```
Canvas:
  Render Mode:        Screen Space - Camera
  Render Camera:      Camera_P2_Game
  Plane Distance:     100
  Sorting Layer:      UI
  Order in Layer:     0

Canvas Scaler:
  UI Scale Mode:      Scale With Screen Size
  Reference Res:      1920 × 1080
  Screen Match Mode:  Match Width Or Height
  Match:              0.5

Graphic Raycaster:
  Blocking Mask:      Everything
```

### Зачем два Canvas, а не один

- **Независимый culling.** Если P1 Camera отрендерена — её HUD рендерится
  автоматически. Нет риска случайно показать P1 HUD на P2 экране в split-screen.
- **Depth control.** Каждый Canvas живет в пространстве своей камеры. При
  добавлении post-processing effects на одну камеру это не затрагивает другой HUD.
- **Photon sync.** В Photon PUN2 можно включать/отключать Canvas целиком
  в зависимости от `PhotonNetwork.IsMasterClient`.

### Важно: Plane Distance

`Plane Distance = 100` — UI рисуется на расстоянии 100 единиц от камеры.
Camera Near Clip Plane должен быть меньше 100 (стандарт Unity: 0.3 — ОК).
Camera Far Clip Plane должен быть больше 100 (стандарт: 1000 — ОК).

### Определение видимости по роли (Photon)

```
// В методе Start() на компонентах Canvas:
void Start()
{
    bool isMasterClient = PhotonNetwork.IsMasterClient;
    Canvas_P1_HUD.SetActive(isMasterClient);
    Canvas_P2_HUD.SetActive(!isMasterClient);
}
```

В LocalDebug режиме — оба Canvas активны одновременно.

---

## 11. Menu Canvas Setup

### Структура

```
Canvas_Menu:
  Render Mode:    Screen Space - Overlay
  Sort Order:     10  (выше игровых Canvas)

Содержимое:
  Panel_Lobby     (список комнат)
  Panel_Room      (ожидание игроков, выбор роли)
  Panel_Settings  (громкость, язык)
  Overlay_Loading (spinner при подключении)
```

### Почему Overlay для меню

В меню нет 3D сцены — Screen Space Overlay проще и надежнее. В игровых сценах
переход к игре: `Canvas_Menu.SetActive(false)`, игровые Canvas активируются.

### Меню — визуальный стиль

- Фон меню: полноэкранный background sprite (концептуальное изображение поля
  тетриса с персонажем внутри) + `color-board-bg` overlay opacity `0.7`
- Логотип игры: по центру сверху, `text-xl` или спрайт
- Кнопки: `ui_btn_primary`, ширина 240px, высота 64px, выровнены по центру
- Разделитель между блоками кнопок: 1px линия `color-ui-border`

### Кнопка (Button Style)

```
ui_btn_primary:
  Background:       color-p1-primary, opacity 0.15
  Border:           1px color-p1-primary
  Border Radius:    4px
  Text:             text-md, SemiBold, color-text-primary
  Padding:          12px 24px
  State Hover:      background opacity → 0.3, border brighter (+20%)
  State Press:      scale 0.95, background opacity → 0.5
  State Disabled:   all opacity 0.4, no interaction

ui_btn_danger (Quit, Leave Room):
  То же, но border и accent: color-state-danger
```

---

## 12. Asset Specifications & Naming

### Конвенция именования

Формат: `[category]_[name]_[variant]_[size].[ext]`

### UI Asset Specs

| Категория | Prefix | Формат | Цветовой профиль |
|---|---|---|---|
| UI спрайты | `ui_` | PNG, прозрачность | sRGB |
| UI иконки | `ui_icon_` | PNG, 64×64px baseline | sRGB |
| Кнопки | `ui_btn_` | PNG или 9-slice | sRGB |
| Шрифты (TMP Asset) | `font_` | TMP Font Asset | — |
| Overlay текстуры | `ui_overlay_` | PNG | sRGB |

### Список обязательных спрайтов

```
ui_icon_destroy_side_ready.png      (64×64)
ui_icon_destroy_side_cooldown.png   (64×64)
ui_icon_destroy_down_ready.png      (64×64)
ui_icon_destroy_down_cooldown.png   (64×64)
ui_icon_push_ready.png              (64×64)
ui_icon_push_inactive.png           (64×64)
ui_icon_bomb_idle.png               (64×64)
ui_icon_bomb_active.png             (64×64)
ui_icon_exit_open.png               (64×64)
ui_icon_exit_blocked.png            (64×64)
ui_btn_primary_normal.png           (9-slice, 240×64)
ui_btn_primary_hover.png            (9-slice, 240×64)
ui_btn_primary_pressed.png          (9-slice, 240×64)
ui_btn_danger_normal.png            (9-slice, 240×64)
ui_panel_preview_bg.png             (9-slice, 160×200)
```

### Размещение в проекте

```
Assets/
└── UI/
    ├── Icons/          (ui_icon_*)
    ├── Buttons/        (ui_btn_*)
    ├── Panels/         (ui_panel_*)
    ├── Overlays/       (ui_overlay_*)
    └── Fonts/          (font_*)
```

### Texture Import Settings (Unity)

```
Texture Type:     Sprite (2D and UI)
Filter Mode:      Bilinear
Compression:      RGBA Compressed ASTC 4x4 (mobile) / RGBA Compressed DXT5 (PC)
Max Size:         512 для иконок, 1024 для панелей
Generate Mip Maps: OFF (UI не использует mip maps)
```

---

## 13. Accessibility Baseline

### Контрастность (WCAG AA минимум)

Все игровые тексты должны иметь контраст не менее **4.5:1** на фоне панели.

| Текст | Фон | Контраст (approx) | Статус |
|---|---|---|---|
| `color-text-primary` (#E8E8F0) на `color-ui-bg-dark` (#12121F) | 12.5:1 | PASS |
| `color-p1-primary` (#4A9EFF) на `color-ui-bg-dark` (#12121F) | 6.2:1 | PASS |
| `color-p2-primary` (#FF7A3D) на `color-ui-bg-dark` (#12121F) | 5.8:1 | PASS |
| `color-text-secondary` (#8888AA) на `color-ui-bg-dark` (#12121F) | 4.6:1 | PASS |
| `color-text-disabled` (#444460) на `color-ui-bg-dark` (#12121F) | 2.1:1 | FAIL — намеренно (disabled) |

### Colorblind Support

Все статусные индикаторы используют **форму + цвет**, а не только цвет:
- Exit Open: зеленый цвет + иконка открытой двери
- Exit Blocked: красный цвет + иконка закрытой двери с замком
- Cooldown Ready: зеленый цвет + иконка без overlay
- Cooldown Active: серо-синий цвет + radial fill overlay поверх иконки

### Размер touch targets

Минимум 48×48px (Reference Resolution) для всех интерактивных элементов на
мобильных устройствах. Визуальный размер кнопки может быть меньше — hit zone
расширяется через `CanvasGroup` или padding на RectTransform.

---

*Art Direction документ поддерживается Art Director агентом.*
*При изменении игровых механик (GDD) — обновить разделы 5, 6, 8 соответственно.*
*При добавлении нового языка — обновить раздел 3 (шрифты и Unicode диапазоны).*
