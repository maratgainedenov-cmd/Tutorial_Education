# Figma Design Brief — LobbyPanel & SettingsPanel
**Game:** BLOKS vs CHARACTER | **Resolution:** 1920×1080 | **Style:** Dark Sci-Fi / Tech UI

---

## Color Tokens

| Token | Hex | Usage |
|---|---|---|
| bg-dark | `#0D0D17` | Page background, input fields |
| bg-panel | `#12121F` | Main panel background |
| bg-mid | `#1E1E35` | Secondary containers, rows |
| border-default | `#2A2A4A` | Panel borders, dividers |
| accent-p1 | `#4A9EFF` | Primary accent (blue) — buttons, titles, sliders |
| danger | `#FF3A3A` | Back button border |
| text-primary | `#E8E8F0` | Main text |
| text-secondary | `#8888AA` | Labels, placeholders |
| text-disabled | `#444460` | Empty states, disabled |

---

## Typography

| Role | Size | Weight | Color |
|---|---|---|---|
| Panel title | 32px | Bold | `#4A9EFF` |
| Section label | 18px | SemiBold | `#8888AA` |
| Body / input | 20–24px | Regular | `#E8E8F0` |
| Small label | 16–18px | Regular | `#8888AA` |
| Button text | 24px | SemiBold | `#E8E8F0` |

Font: monospace / tech style (e.g. Share Tech Mono, Rajdhani, or similar)

---

## Shared Components

### Panel Card
- Corner radius: `4px`
- Background: `#12121F`
- Border: `1px solid #2A2A4A`
- Top accent bar: `2px solid #4A9EFF` (full width, flush to top edge)
- Padding: `24px` all sides

### Primary Button (Blue)
- Size: `full-width × 56px`
- Background: `#4A9EFF` at `15%` opacity → `#4A9EFF26`
- Border: `1px solid #4A9EFF`
- Corner radius: `4px`
- Text: 24px SemiBold `#E8E8F0`, uppercase

### Danger Button (Back)
- Size: `full-width × 56px`
- Background: `#FF3A3A` at `10%` opacity → `#FF3A3A1A`
- Border: `1px solid #FF3A3A`
- Corner radius: `4px`
- Text: 24px SemiBold `#E8E8F0`, uppercase, text: `НАЗАД`

### Divider
- Height: `1px`
- Width: `100%`
- Color: `#2A2A4A`
- Vertical margin: `12px` top + bottom

### Background Overlay (behind panel)
- Full screen `1920×1080`
- Color: `#0A0A14` at `75%` opacity

---

## Screen 1 — LobbyPanel

**Panel size:** `640 × 640px` centered on screen

### Layout (top → bottom, inside panel)

```
[AccentBar]          2px   #4A9EFF
[padding top]        20px
[Title]              "ЛОББИ"  32px Bold  #4A9EFF  centered
[gap]                16px
[Divider]            1px  #2A2A4A
[gap]                12px
[Label]              "ИМЯ КОМНАТЫ"  18px  #8888AA  left-aligned
[gap]                8px
[InputField]         592×48px
[gap]                8px
[Btn_Create]         592×56px  "СОЗДАТЬ"
[gap]                12px
[Divider]            1px  #2A2A4A
[gap]                12px
[Label]              "ДОСТУПНЫЕ КОМНАТЫ"  18px  #8888AA  left-aligned
[gap]                8px
[ScrollView]         592×220px
[gap]                16px
[Btn_Back]           592×56px  "НАЗАД"
[padding bottom]     24px
```

### InputField spec
- Size: `592 × 48px`
- Background: `#0D0D17`
- Border: `1px solid #2A2A4A`
- Border (focused state): `1px solid #4A9EFF`
- Corner radius: `4px`
- Inner padding: `12px` horizontal
- Text: 24px `#E8E8F0`
- Placeholder: `"Введите имя комнаты..."` color `#444460`

### ScrollView (room list)
- Size: `592 × 220px`
- Background: `#0D0D17`
- Border: `1px solid #2A2A4A`
- Scrollbar: `6px` wide, track `#2A2A4A`, handle `#4A9EFF` 60% opacity
- Show 2 RoomRow items visible + partial 3rd

### RoomRow (repeating item inside scroll)
- Size: `full-width × 48px`
- Background: `#1E1E35`
- Bottom border: `1px solid #2A2A4A`
- Padding: `8px` horizontal
- Layout: horizontal, space-between

| Element | Size | Style |
|---|---|---|
| Room name text | flex-grow | 20px Regular `#E8E8F0` overflow ellipsis |
| Player count | 60px fixed | 18px `#8888AA` centered — e.g. `1 / 2` |
| Btn_Join | 80×32px | bg `#4A9EFF26`, border `#4A9EFF`, text `ВОЙТИ` 16px |

**Empty state** (when list is empty): centered text `"Нет доступных комнат"` 18px `#444460`

---

## Screen 2 — SettingsPanel

**Panel size:** `560 × 660px` centered on screen

### Layout (top → bottom, inside panel)

```
[AccentBar]               2px  #4A9EFF
[padding top]             20px
[Title]                   "НАСТРОЙКИ"  32px Bold  #4A9EFF  centered
[gap]                     16px
[Divider]                 1px
[gap]                     12px
[Section label]           "ЗВУК"  18px SemiBold  #8888AA  uppercase  left
[gap]                     8px
[SliderRow: Мастер]       512×40px
[gap]                     4px
[SliderRow: Музыка]       512×40px
[gap]                     4px
[SliderRow: Эффекты]      512×40px
[gap]                     12px
[Divider]                 1px
[gap]                     12px
[Section label]           "УПРАВЛЕНИЕ"  18px SemiBold  #8888AA  uppercase  left
[gap]                     8px
[ControlRow] × 9          512×32px each
[gap]                     16px
[Btn_Back]                512×56px  "НАЗАД"
[padding bottom]          24px
```

### SliderRow spec
- Size: `512 × 40px`
- Layout: horizontal, vertically centered

| Element | Width | Style |
|---|---|---|
| Label | 120px fixed | 20px Regular `#E8E8F0` left-aligned |
| Slider | flex | track `#0D0D17`, fill `#4A9EFF`, handle `20×20px` white `#E8E8F0` |
| Value | 40px fixed | 18px `#8888AA` right-aligned, format: `100%` |

Default values: Мастер `100%`, Музыка `80%`, Эффекты `100%`

### ControlRow spec (key bindings, read-only)
- Size: `512 × 32px`
- Alternating row bg: odd `#1E1E35`, even `#12121F`
- Padding: `8px` horizontal

| Element | Width | Style |
|---|---|---|
| Action name | flex | 18px Regular `#8888AA` |
| Key bind | 120px fixed | 18px SemiBold `#E8E8F0` right-aligned |

**Key bindings data:**

| Action | Key |
|---|---|
| Персонаж: влево | `A / ←` |
| Персонаж: вправо | `D / →` |
| Персонаж: прыжок | `W / Space` |
| Уничтожить сбоку | `X` |
| Уничтожить вниз | `Z` |
| Толчок | `Numpad 0` |
| Тетрис: выбор колонки | `Мышь` |
| Тетрис: повернуть | `W (drag)` |
| Пауза | `Esc` |

---

## Deliverables expected from Figma

1. **Frame: LobbyPanel** — desktop 1920×1080, panel centered, overlay behind
2. **Frame: SettingsPanel** — desktop 1920×1080, panel centered, overlay behind
3. **Component: RoomRow** — single room list item, with `default` and `full` (disabled Join) variants
4. **Component: SliderRow** — slider row with label + slider + value
5. **Component: ControlRow** — key binding row, odd/even variants
6. Все компоненты в отдельной **Components page** в Figma
