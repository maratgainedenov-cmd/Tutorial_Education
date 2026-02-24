# Мультиплеер — Архитектура MVP

## Стек

- **Netcode for GameObjects (NGO)** — сетевой слой
- **Unity Relay** — обход NAT, игра через интернет без IP
- **Unity Lobby** — создание/поиск комнаты по коду (V2, не MVP)
- **Transport: UDP** (Unity Transport)

---

## Роли в сессии

| Роль | Кто | Примечание |
|------|-----|------------|
| **Host** | Тетрис-игрок | Запускает сервер + играет сам |
| **Client** | Персонаж-игрок | Подключается по JoinCode |

Host = Tetris фиксировано для MVP. Так проще: Host владеет доской, запускает всю логику.

---

## Authority модель

```
┌─────────────────── HOST (Tetris) ──────────────────────┐
│  TetrisBoard — source of truth                         │
│  VictoryModel — счёт, победа/поражение                 │
│  CrushDetection — проверка каждый тик                  │
│  Валидация всех ServerRpc                              │
└───────────┬────────────────────────┬───────────────────┘
    ServerRpc ↑          ClientRpc ↓  │  ClientRpc ↓
            │                         │
    ┌───────┴──────┐         ┌────────┴──────────────┐
    │ Tetris Input │         │  Character Client      │
    │ (на Host,    │         │  движение локально     │
    │  без RPC)    │         │  NetworkTransform sync │
    │              │         │  удар → ServerRpc      │
    └──────────────┘         └───────────────────────┘
```

### Что работает где

| Система | Где живёт | Обоснование |
|---------|-----------|-------------|
| `TetrisBoard` | Только сервер | Source of truth для доски |
| `VictoryModel` | Только сервер | Счёт нельзя трогать с клиента |
| `TetrisController` input | Host (локально) | Host = Tetris игрок, RPC не нужен |
| `CharacterController2D` physics | Character client | Client Authority для game feel |
| `BlockInteraction` | Только сервер | Валидация урона на сервере |
| `GameManager` crush check | Только сервер | Победа определяется сервером |
| `BoardRenderer` | Все клиенты | Оба игрока видят доску |
| `CharacterView` | Все клиенты | Оба видят персонажа |
| `GameUI` | Все клиенты | Счёт синхронизируется |

---

## Что нужно изменить в коде

### Новые зависимости (добавить `NetworkBehaviour`)

| Файл | Было | Станет |
|------|------|--------|
| `GameManager.cs` | `MonoBehaviour` | `NetworkBehaviour` |
| `TetrisController.cs` | `MonoBehaviour` | `NetworkBehaviour` |
| `CharacterController2D.cs` | `MonoBehaviour` | `NetworkBehaviour` |
| `BlockInteraction.cs` | `MonoBehaviour` | `NetworkBehaviour` |
| `BoardRenderer.cs` | `MonoBehaviour` | `MonoBehaviour` (без изменений) |

### Файлы без изменений

`TetrisBoard`, `VictoryModel`, `CellData`, `TetrominoData`, `ActivePiece`, `Grid2D` —
чистый C#, работают как есть на сервере.

---

## Синхронизация состояния

### NetworkVariable (сервер пишет, все читают)

```csharp
// GameManager / NetworkGameManager
NetworkVariable<GameState> State;          // Waiting/Playing/GameOver
NetworkVariable<int>       BlocksDestroyed; // счёт персонажа

// TetrisController
NetworkVariable<Vector2Int> ActivePiecePos;
NetworkVariable<int>        ActivePieceRotation;
NetworkVariable<int>        ActivePieceType;
NetworkVariable<bool>       ActivePieceIsWeak;
```

Клиенты подписываются на `OnValueChanged` → `BoardRenderer` обновляет ghost и активную фигуру.

### ClientRpc (сервер → все клиенты, события)

```csharp
[ClientRpc] void PiecePlacedClientRpc(CellUpdateData[] changes);
// передаём только изменившиеся ячейки, не всю доску

[ClientRpc] void LinesClearedClientRpc(int[] clearedRows);
// клиент сдвигает визуал вниз

[ClientRpc] void BlockDamagedClientRpc(int x, int y, int newHP);
// обновить цвет ячейки на клиенте

[ClientRpc] void GameOverClientRpc(Winner winner);
// показать экран победы/поражения
```

### ServerRpc (клиент → сервер, запросы)

```csharp
// Tetris (от Host — не нужны, он на сервере)
// Если Client будет Tetris — понадобятся:
[ServerRpc] void MoveServerRpc(int dx);
[ServerRpc] void RotateServerRpc(int dir);
[ServerRpc] void HardDropServerRpc();

// Character (от Character Client)
[ServerRpc] void AttackServerRpc(int gridX, int gridY, int dir);
// Сервер валидирует: IsWeak? → DamageCell → VictoryModel
```

---

## Движение персонажа (Client Authority)

Character Client запускает `CharacterController2D.Update()` локально.
`NetworkTransform` синхронизирует `transform.position` на сервер и другие клиенты.

```
Character Client:
  ┌─ Update() → physics → transform.position изменился
  └─ NetworkTransform → репликация позиции на сервер

Server (Host):
  ┌─ видит позицию персонажа с небольшой задержкой (RTT/2)
  └─ CheckActivePieceCrush() работает по этой позиции
```

**Trade-off:** Character Client может теоретически подделать позицию.
Для игры между друзьями — приемлемо. Для V2 → server-side prediction.

---

## Синхронизация доски

### Стратегия: событийная (не полная репликация)

Передавать 200 ячеек целиком — избыточно. Вместо этого:

1. При `PlacePiece` → `PiecePlacedClientRpc` с массивом изменённых ячеек (≤4)
2. При `ClearLines` → `LinesClearedClientRpc` с номерами строк
3. При `DamageCell` → `BlockDamagedClientRpc(x, y, newHP)`
4. При реконнекте → отправить полный снапшот доски новому клиенту

### Клиентская доска

Каждый клиент держит свою копию `TetrisBoard` (только для рендера).
Сервер — единственный, кто модифицирует данные. Клиенты применяют полученные обновления.

---

## Что нужно добавить (новые файлы/компоненты)

```
Assets/Scripts/Network/
  NetworkGameManager.cs   — NetworkBehaviour обёртка GameManager
  NetworkTetrisSync.cs    — NetworkVariable для активной фигуры + ClientRpc событий
  NetworkCharacterSync.cs — IsOwner guard + AttackServerRpc
```

Или: модифицировать существующие файлы напрямую (проще для MVP).

---

## Crush Detection в сети

Сейчас: `GameManager.Update()` → `CheckActivePieceCrush()` — работает на клиенте.

**Нужно:** только сервер проверяет crush.

```csharp
// GameManager (на сервере)
private void Update()
{
    if (!IsServer || _gameEnded) return;
    CheckActivePieceCrush();   // используем серверную позицию персонажа
    // Character Client пишет через NetworkTransform → сервер читает
}
```

`CheckCrush()` (при PlacePiece) — уже корректен, вызывается на сервере.

---

## Сессионная модель MVP

```
1. Host нажимает "Create Game"
   └─ NetworkManager.StartHost()
   └─ Unity Relay → получает JoinCode

2. Host показывает JoinCode другу

3. Client вводит JoinCode → "Join Game"
   └─ NetworkManager.StartClient()
   └─ Relay соединяет через сервер Unity

4. Оба на сцене → GameManager.OnNetworkSpawn() → StartGame()

5. При выходе одного из игроков → GameOver + уведомление оставшемуся
```

---

## MVP vs V2

| Фича | MVP | V2 |
|------|-----|----|
| Основная механика по сети | ✅ | |
| Client Authority для персонажа | ✅ | |
| Unity Relay (интернет) | ✅ | |
| Lobby (список комнат) | | ✅ |
| Server-side prediction | | ✅ |
| Reconnect / Grace Period | | ✅ |
| Спектатор-режим | | ✅ |

---

## TODO перед реализацией

- [ ] Разделить `CharacterController2D.Update()` на IsOwner-зависимую часть (ввод) и общую (визуал)
- [ ] Добавить `NetworkObject` на префаб персонажа
- [ ] Добавить `NetworkTransform` на персонажа
- [ ] Создать `NetworkTetrisSync` — NetworkVariable для активной фигуры
- [ ] Переделать события `TetrisBoard` → ClientRpc в GameManager
- [ ] Изолировать `CheckActivePieceCrush()` → только `if (IsServer)`
- [ ] Установить пакеты: `com.unity.netcode.gameobjects`, `com.unity.services.relay`

---

← [[Tetris vs Hero]] · [[Фаза 2 — Мультиплеер MVP]] · → [[Фаза 3 — Полировка]]
