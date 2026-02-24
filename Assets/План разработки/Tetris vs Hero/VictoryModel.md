# VictoryModel

`VictoryModel.cs` — чистый C#, не MonoBehaviour.

## Состояние

```csharp
int BlocksDestroyed   // текущий счёт персонажа
int BlocksToWin       // порог победы (задаётся в GameManager._blocksToWin = 30)
```

## Методы

```csharp
void AddDestroyedBlocks(int count)
// Добавить блоки → OnBlocksDestroyedChanged → если ≥ BlocksToWin → OnCharacterWin

void TriggerTetrisWin()
// Вызвать победу Тетриса
```

## События

```csharp
event Action<int> OnBlocksDestroyedChanged   // обновить UI
event Action      OnCharacterWin             // персонаж победил
event Action      OnTetrisWin                // тетрис победил
```

## Кто вызывает TriggerTetrisWin

- `GameManager.CheckCrush()` — блок зафиксирован на персонаже
- `GameManager.OnTetrisStateChanged()` — доска переполнена (GameOver-фоллбэк)

> ⚠️ Путаница: комментарий в GameManager говорит «Тетрис проиграл»,
> но вызывает `TriggerTetrisWin()`. Логика: GameOver — фоллбэк на случай,
> если CheckCrush не поймал crush. Комментарий вводит в заблуждение.

---

← [[Tetris vs Hero]] · [[Архитектура MVC]] · → [[Взаимодействия блоков и персонажа]]
