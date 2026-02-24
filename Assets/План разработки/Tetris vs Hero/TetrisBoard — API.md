# TetrisBoard — API

`TetrisBoard.cs` — чистый C#, не MonoBehaviour.

## Публичные методы

```csharp
bool IsValidPosition(ActivePiece piece)
// Все клетки фигуры в допустимых позициях (нет коллизий, в границах)

bool CanSpawn(ActivePiece piece)
// Псевдоним IsValidPosition — проверка при спавне

void PlacePiece(ActivePiece piece)
// Зафиксировать фигуру на доске → OnPiecePlaced

int ClearLines()
// Очистить заполненные линии → кол-во очищенных → OnLinesCleared(count)

bool DamageCell(int x, int y, int damage = 1)
// Урон любому непустому блоку → HP--; true если разрушен (HP ≤ 0)

bool IsCellOccupied(int x, int y)
// Проверить зафиксированную ячейку (активная фигура НЕ учитывается!)
```

## События

```csharp
event Action<int> OnLinesCleared   // (кол-во линий)
event Action      OnPiecePlaced    // после фиксации фигуры
```

## Важно

`IsCellOccupied` видит только **зафиксированные** блоки.
Активная падающая фигура (`TetrisController.Current`) — не в гриде.

`PlacePiece` записывает каждую ячейку как `CellData.Block(type, piece.HP)` —
HP берётся из `ActivePiece`, который получает его при спавне (`_blockHP = 3`).

---

← [[Tetris vs Hero]] · [[Архитектура MVC]] · → [[Взаимодействия блоков и персонажа]] · [[Типы блоков]]
