# Tetris vs Hero — асимметричная игра

## Концепция

Два игрока с разными ролями:
- **Тетрис-игрок** — роняет фигуры сверху, цель: задавить персонажа
- **Персонаж** — Celeste-стиль (прыжок, wall-jump, удар), цель: разбить X блоков

### Победные условия

| Кто | Условие победы |
|-----|----------------|
| Тетрис | Блок падает на персонажа (crush) |
| Персонаж | Суммарно разбито X блоков (удары + очищенные линии) |

### Конфликт интересов (ключевая механика)

- Тетрис заполняет доску → риск crush → **хорошо**
- Тетрис очищает линии → помогает персонажу → **плохо** для Тетриса
- Персонаж бьёт слабые блоки → прогресс к победе → риск быть задавленным

---

## Типы блоков

| Тип | PieceType | Описание |
|-----|-----------|----------|
| Сильный | 1–7 | Обычный тетромино, персонаж не может разбить |
| Слабый | 8+ | Персонаж может разбить ударом, имеет HP |

---

## Архитектура MVC — 12 файлов в `Assets/Scripts/`

### MODEL (чистый C#)

| Файл | Назначение |
|------|-----------|
| `CellData.cs` | struct: PieceType, IsWeak, HP |
| `TetrominoData.cs` | Static: формы и цвета 7 фигур |
| `ActivePiece.cs` | class: текущая падающая фигура |
| `TetrisBoard.cs` | Grid2D\<CellData\>, коллизии, очистка линий, события |
| `VictoryModel.cs` | Счётчик разбитых блоков, условия победы |

### VIEW (MonoBehaviour)

| Файл | Назначение |
|------|-----------|
| `BoardRenderer.cs` | Рендер ячеек доски (SpriteRenderer pool) |
| `CharacterView.cs` | Визуал персонажа (flip, анимации) |
| `GameUI.cs` | Счётчики, game over, таймер (TextMeshPro) |

### CONTROLLER (MonoBehaviour)

| Файл | Назначение |
|------|-----------|
| `TetrisController.cs` | Ввод Тетриса, падение, спавн, state machine |
| `CharacterController2D.cs` | Celeste-физика: прыжок, wall-jump, удар |
| `BlockInteraction.cs` | Удар по блоку → Grid2D → VictoryModel |
| `GameManager.cs` | Связывает всё, следит за победой/поражением |

---

## CharacterController2D — Celeste-механики

- Кастомная физика (не Rigidbody2D) — AABB vs Grid2D\<CellData\>
- Горизонталь: ускорение + замедление (не мгновенная скорость)
- **Coyote Time** (~0.12с) — прыжок сразу после края платформы
- **Jump Buffer** (~0.12с) — прыжок чуть до приземления
- **Variable Jump Height** — отпустил кнопку раньше → ниже прыгнул
- **Wall-Slide** — у стены гравитация × 0.3
- **Wall-Jump** — отталкивание от стены

### Ввод персонажа

| Действие | Клавиша |
|----------|---------|
| Движение | A / D |
| Прыжок | W |
| Удар влево | J |
| Удар вправо | L |

### Ввод Тетриса

| Действие | Клавиша |
|----------|---------|
| Движение ← → | ← → |
| Поворот по часовой | ↑ |
| Поворот против | Z |
| Soft drop | ↓ |
| Hard drop | Space |

---

## TetrominoData — формы

7 фигур, каждая с 4 поворотами × 4 клетки (Vector2Int смещения от Pos).
Хранятся в статическом трёхмерном массиве `Shapes[тип][поворот][клетка]`.

---

## TetrisBoard — ключевые методы

```csharp
IsValidPosition(piece)  // проверка коллизий
PlacePiece(piece)       // зафиксировать фигуру
ClearLines()            // → кол-во очищенных
DamageCell(x, y)        // урон слабому блоку → true если разбит
CanSpawn(piece)         // есть ли место для спавна
```

---

## VictoryModel — события

```csharp
OnBlocksDestroyedChanged  // (int count)
OnCharacterWin            // достигнут порог блоков
OnTetrisWin               // персонаж раздавлен
```

---

## Геймплейный нюанс

Тетрис-игрок может очистить линию, пока персонаж скользит по ней → **персонаж падает**!

---

## Статус реализации

- [x] MODEL: CellData, TetrominoData, ActivePiece, TetrisBoard, VictoryModel
- [x] VIEW: BoardRenderer, CharacterView, GameUI
- [x] CONTROLLER: TetrisController, CharacterController2D, BlockInteraction, GameManager
- [ ] Сцена Unity: расставить GameObjects, привязать ссылки в Inspector
- [ ] Добавить спрайты для BoardRenderer (cellSprite)
- [ ] Настроить Animator для персонажа
- [ ] Playtest и балансировка (blocksToWin, weakPieceChance, fallInterval)
