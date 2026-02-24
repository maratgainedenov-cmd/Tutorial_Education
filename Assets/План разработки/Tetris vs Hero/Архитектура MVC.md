# 🏗 Архитектура MVC

Все файлы в `Assets/Scripts/`. Чистое разделение на три слоя.

---

## Model — чистый C#

> [!abstract] Без зависимостей от Unity Engine
> Можно тестировать без запуска редактора.

| Файл | Назначение |
|------|-----------|
| `CellData.cs` | `struct`: PieceType, IsWeak, HP, MaxHP |
| `TetrominoData.cs` | Статические формы и цвета 7 фигур |
| `ActivePiece.cs` | Текущая падающая фигура (позиция, поворот) |
| `TetrisBoard.cs` | `Grid2D<CellData>` — коллизии, очистка линий, урон блокам |
| `VictoryModel.cs` | Счётчик разбитых блоков, условия победы |

---

## View — MonoBehaviour

> [!abstract] Только визуал, никакой логики

| Файл | Назначение |
|------|-----------|
| `BoardRenderer.cs` | Пул SpriteRenderer'ов, рендер ячеек и ghost |
| `CharacterView.cs` | Flip спрайта, анимации (опционально) |
| `GameUI.cs` | TextMeshPro счётчики, экран GameOver |

---

## Controller — MonoBehaviour

> [!abstract] Ввод → логика → Model → View

| Файл | Назначение |
|------|-----------|
| `TetrisController.cs` | Ввод тетриса, автопадение, спавн, state machine |
| `CharacterController2D.cs` | Celeste-физика: прыжок, wall-jump, AABB-коллизии |
| `BlockInteraction.cs` | Удар персонажа → DamageCell → VictoryModel |
| `GameManager.cs` | Связывает все системы, crush detection |

---

## Граф инициализации

```
GameManager.Awake()
  │
  ├─ TetrisController.Init(BoardRenderer)
  │    └─ создаёт TetrisBoard
  │    └─ BoardRenderer.Init(Board)
  │
  ├─ CharacterController2D.Init(Board)
  │
  ├─ BlockInteraction.Init(Board, VictoryModel, Character)
  │    └─ character.OnAttack      → HandleAttack
  │    └─ board.OnLinesCleared    → HandleLinesCleared
  │
  └─ GameUI.Init(VictoryModel)
       └─ victory.OnBlocksDestroyedChanged → обновить UI
```

---

## Ключевые события

| Событие | Источник | Подписчик |
|---------|----------|-----------|
| `OnAttack(x, y, dir)` | CharacterController2D | BlockInteraction |
| `OnLinesCleared(count)` | TetrisBoard | BlockInteraction |
| `OnPiecePlaced` | TetrisBoard | GameManager (CheckCrush) |
| `OnBlocksDestroyedChanged(n)` | VictoryModel | GameUI |
| `OnCharacterWin` | VictoryModel | GameManager |
| `OnTetrisWin` | VictoryModel | GameManager |
| `OnStateChanged` | TetrisController | GameManager |
| `OnValueChanged(x,y,data)` | Grid2D | BoardRenderer |

---

← [[Tetris vs Hero]]
