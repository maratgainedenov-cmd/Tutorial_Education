# 🌐 Фаза 2 — Мультиплеер MVP

> [!warning] Предусловие
> Фаза 1 завершена и проверена в single-player.

---

## Архитектура (коротко)

> [!info] Роли и Authority
> **Host = Тетрис-игрок** — запускает сервер, владеет доской, без лага на ввод.
> **Client = Персонаж** — подключается по JoinCode, двигается локально.
>
> | Что | Где живёт |
> |-----|-----------|
> | TetrisBoard, VictoryModel | Только сервер |
> | Crush detection | Только сервер (`if (!IsServer) return`) |
> | Физика персонажа | Client Authority + NetworkTransform |
> | Результат игры | `GameOverClientRpc` → оба игрока |

→ [[Мультиплеер — Архитектура MVP|Подробная архитектура]]

---

## Шаг 1 — Пакеты

### Package Manager → Add by name

```
com.unity.netcode.gameobjects
com.unity.services.relay
```

### Unity Gaming Services

```
Edit → Project Settings → Services → Create / Link project
```

---

## Шаг 2 — NetworkManager

- [ ] Создать пустой GameObject `NetworkManager`
- [ ] Добавить компонент `NetworkManager`
- [ ] Добавить компонент `UnityTransport` → Protocol Type: **UDP**
- [ ] Зарегистрировать в **Network Prefabs**:
  - Префаб `Character` (с компонентом `NetworkObject`)

---

## Шаг 3 — Рефакторинг скриптов

### `GameManager.cs`

```diff
- public class GameManager : MonoBehaviour
+ public class GameManager : NetworkBehaviour

+ NetworkVariable<int>  _netScore     = new(...WritePermission.Server);
+ NetworkVariable<byte> _netGameState = new(...WritePermission.Server);

  private void Update()
  {
+     if (!IsServer || _gameEnded) return;
      CheckActivePieceCrush();
  }

+ [ClientRpc]
+ void GameOverClientRpc(bool tetrisWins) { /* показать экран */ }
```

- [ ] `MonoBehaviour` → `NetworkBehaviour`
- [ ] Добавить NetworkVariable для счёта и состояния
- [ ] Guard `if (!IsServer)` в `Update`, `CheckCrush`, `CheckActivePieceCrush`
- [ ] Добавить `GameOverClientRpc`

---

### `TetrisController.cs`

```diff
- public class TetrisController : MonoBehaviour
+ public class TetrisController : NetworkBehaviour

+ NetworkVariable<Vector2Int> _netPiecePos      = new(...);
+ NetworkVariable<int>        _netPieceRotation = new(...);
+ NetworkVariable<int>        _netPieceType     = new(...);
+ NetworkVariable<int>        _netPieceHP       = new(...);

  private void Update()
  {
+     if (!IsServer) return;  // Host = Tetris игрок
      ...
  }

+ void SyncActivePiece()    // вызывать после каждого Move/Rotate/Spawn
+ {
+     _netPiecePos.Value      = Current.Pos;
+     _netPieceRotation.Value = Current.Rotation;
+     _netPieceType.Value     = Current.Type;
+     _netPieceHP.Value       = Current.HP;
+ }
```

Клиент подписывается на `OnValueChanged` → обновляет локальный `Current` → `BoardRenderer`.

- [ ] `MonoBehaviour` → `NetworkBehaviour`
- [ ] Добавить 4 NetworkVariables для активной фигуры
- [ ] `Update()` → `if (!IsServer) return`
- [ ] Вызывать `SyncActivePiece()` после Move / Rotate / Spawn / Lock

---

### `CharacterController2D.cs`

```diff
- public class CharacterController2D : MonoBehaviour
+ public class CharacterController2D : NetworkBehaviour

  private void Update()
  {
+     if (IsOwner)
+     {
          HandleAttack();
          HandleHorizontal(dt);
          HandleJumpInput();
          ApplyGravity(dt);
          MoveAndCollide(dt);
+     }
      _view.UpdateAnimations(_velocity.x, _grounded, _wallSliding); // всем
  }

  private void HandleAttack()
  {
      ...
-     OnAttack?.Invoke(cx + dir, cy, dir);
+     AttackServerRpc(cx + dir, cy, dir);
  }

+ [ServerRpc]
+ void AttackServerRpc(int x, int y, int dir)
+ {
+     _blockInteraction.HandleAttackFromServer(x, y, dir);
+ }
```

На GameObject `Character` добавить:

| Компонент | Зачем |
|-----------|-------|
| `NetworkObject` | Обязателен для любого сетевого объекта |
| `NetworkTransform` | Синхронизация позиции на все клиенты |
| `NetworkAnimator` | Синхронизация триггеров аниматора |

- [ ] `MonoBehaviour` → `NetworkBehaviour`
- [ ] Ввод и физика — только `if (IsOwner)`
- [ ] Удар → `AttackServerRpc` вместо `OnAttack.Invoke`
- [ ] Добавить `NetworkObject`, `NetworkTransform`, `NetworkAnimator` в Unity

---

### `BlockInteraction.cs`

```diff
- private void HandleAttack(int x, int y, int dir)   // подписка на событие
+ public void HandleAttackFromServer(int x, int y, int dir)  // вызов с сервера
```

- [ ] Убрать подписку на `OnAttack` событие
- [ ] Переименовать метод в `HandleAttackFromServer` (вызывается из `AttackServerRpc`)

---

### `BoardRenderer.cs`

Клиент не имеет прямого доступа к `TetrisBoard` — получает изменения через ClientRpc.

```csharp
[ClientRpc]
public void SyncCellClientRpc(int x, int y, CellData data)
{
    _board.Grid.SetValue(x, y, data); // → OnValueChanged → RefreshCell
}

[ClientRpc]
public void SyncClearLineClientRpc(int[] clearedRows)
{
    // применить DropLinesAbove на клиентской копии доски
}
```

- [ ] Клиентская копия `TetrisBoard` создаётся при входе (размер приходит с сервера)
- [ ] Подписаться на NetworkVariables активной фигуры → `RenderActivePiece`

---

## Шаг 4 — Сессионный UI

### Макет экрана лобби

```
┌──────────────────────────────┐
│        TETRIS VS HERO        │
│                              │
│       [ HOST GAME ]          │
│       [ JOIN GAME ]          │
│                              │
│  Код игры:  █ █ █ █ - █ █   │
│  Введите:  [____________]    │
└──────────────────────────────┘
```

### Unity Relay — Host

```csharp
var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections: 1);
var joinCode   = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
// Показать joinCode игроку
NetworkManager.Singleton.StartHost();
```

### Unity Relay — Client

```csharp
var join = await RelayService.Instance.JoinAllocationAsync(joinCode);
// Передать данные в UnityTransport
NetworkManager.Singleton.StartClient();
```

- [ ] Создать `LobbyManager.cs` с методами `HostGame()` и `JoinGame(string code)`
- [ ] Показывать JoinCode после создания игры
- [ ] Кнопка Join активируется только если код введён

---

## Шаг 5 — Отключение игрока

```csharp
// GameManager.OnNetworkSpawn():
NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

void OnClientDisconnected(ulong clientId)
{
    if (!IsServer) return;
    GameOverClientRpc(tetrisWins: false); // соперник вышел
}
```

- [ ] Подписаться на `OnClientDisconnectCallback`
- [ ] Вывести сообщение "Opponent disconnected" на оставшемся клиенте

---

## Шаг 6 — Тестирование

### На одной машине — ParrelSync

```
1. Установить: github.com/VeriorPies/ParrelSync
2. ParrelSync → Clones Manager → Add Clone
3. Запустить Clone — Host в одном окне, Join в другом
```

- [ ] Фигуры тетриса видны на клиенте Персонажа
- [ ] Движение персонажа видно на клиенте Тетриса
- [ ] Crush → оба видят экран GameOver
- [ ] Победа персонажа → оба видят экран GameOver

### Через Relay (два компьютера)

- [ ] Host создаёт игру → отправляет JoinCode другу
- [ ] Client вводит код → подключается
- [ ] Полный игровой цикл через интернет

---

← [[Фаза 1 — Single Player]] · [[Роадмап]] · → [[Фаза 3 — Полировка]]
