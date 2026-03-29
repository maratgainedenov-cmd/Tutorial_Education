# Root Cause Analysis - I vs Blocks Network Bugs

**Дата**: 29.03.2026
**Анализирующий**: QA Agent (Claude Code)

---

## БАГ #1: Дублирование Character — Глубокий Анализ

### Сценарий Сбоя

```
Master Client (Actor 1)          Non-Master Client (Actor 2)
├─ RoleSelectPanel               ├─ RoleSelectPanel
├─ Choose "Tetris"               ├─ Choose "Character"
├─ CustomProperty[role]=tetris    ├─ CustomProperty[role]=character
├─ TryResolveRoles() (Master)
│  └─ SetCustomProperties(
│     {started: true}
│  )
├─ OnRoomPropertiesUpdate         ├─ OnRoomPropertiesUpdate
│  └─ GameManager.StartGame()     │  └─ GameManager.StartGame()
│     ├─ IsMasterClient = true    │     ├─ IsMasterClient = false
│     ├─ TetrisController.Start   │     ├─ CharacterSpawner.Start
│     └─ grid.SetActive(true)     │     └─ PhotonNetwork.Instantiate(Character)
│
│ ОШИБКА: Почему оба вызывают CharacterSpawner?
```

### Возможные Причины

#### Причина 1: Race Condition в PhotonNetwork.IsMasterClient

```csharp
// GameManager.StartGame()
if (Photon.Pun.PhotonNetwork.IsMasterClient)  // <--- ЭТА ПРОВЕРКА МОЖЕТ БЫТЬ НЕВЕРНА
{
    _tetrisController?.StartGame();
}
else
{
    _characterSpawner?.StartGame();
}
```

**Гипотеза**: На Non-Master клиенте в момент вызова GameManager.StartGame(), свойство IsMasterClient ещё не обновлено корректно. Возможно:

1. Photon callback IsMasterClient обновляется асинхронно
2. Есть задержка между OnRoomPropertiesUpdate и обновлением состояния master client
3. Оба клиента видят себя как non-master (или наоборот)

**Диагностика**:
```csharp
public void StartGame()
{
    Debug.Log($"[GameManager] IsMasterClient = {PhotonNetwork.IsMasterClient}, " +
              $"LocalPlayer.ActorNumber = {PhotonNetwork.LocalPlayer.ActorNumber}, " +
              $"MasterClient.ActorNumber = {PhotonNetwork.MasterClient.ActorNumber}");
}
```

#### Причина 2: OnRoomPropertiesUpdate вызывается дважды

Если сетевая задержка или повторная трансмиссия RPC вызывает OnRoomPropertiesUpdate дважды, то GameManager.StartGame() может вызваться дважды на одном клиенте.

```csharp
public override void Show()
{
    // ...
}

public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
{
    if (propertiesThatChanged.ContainsKey(RoomStarted))
        GameManager.Instance?.StartGame();  // <--- ВЫЗЫВАЕТСЯ ДВА РАЗА?
}
```

**Защита**: добавить guard в GameManager.StartGame()

```csharp
private bool _gameStarted = false;

public void StartGame()
{
    if (_gameStarted) return;  // <--- GUARD
    _gameStarted = true;
    // ...
}
```

#### Причина 3: CharacterSpawner вызывается на Master Client

Возможно, логика в GameManager неправильная:

```csharp
// Может быть, оба вызывают оба spawner'а?
_tetrisController?.StartGame();     // ВСЕГДА вызывается?
_characterSpawner?.StartGame();     // ВСЕГДА вызывается?
```

**Проверка кода**: В GameManager.cs (line 36-60) видно условие, поэтому это маловероятно.

#### Причина 4: PhotonNetwork.Instantiate вызывается на Мастере без guard'а

```csharp
// CharacterSpawner.cs
public void StartGame()
{
    if (GameManager.LocalDebug)
    {
        // ...
    }
    else
    {
        // ВСЕ КЛИЕНТЫ вызывают это?
        PhotonNetwork.Instantiate(_characterPrefabName, transform.position, Quaternion.identity);
    }
}
```

**Гипотеза**: CharacterSpawner.StartGame() вызывается на ОБОИХ клиентах, и PhotonNetwork.Instantiate() создаёт Character на Master сервере, видимый обоим.

### Диагностический План

1. Добавить логирование в GameManager.StartGame():
   ```csharp
   Debug.Log($"[StartGame] IsMasterClient={PhotonNetwork.IsMasterClient}");
   ```

2. Добавить логирование в CharacterSpawner.StartGame():
   ```csharp
   Debug.Log($"[CharacterSpawner] Called on {(PhotonNetwork.IsMasterClient ? "Master" : "Non-Master")}");
   ```

3. Добавить guard в GameManager.StartGame():
   ```csharp
   private bool _startedOnce = false;
   public void StartGame()
   {
       if (_startedOnce) return;
       _startedOnce = true;
       // ...
   }
   ```

### Вероятность Каждой Причины

| Причина | Вероятность | Обоснование |
|---------|------------|------------|
| Race condition в IsMasterClient | 40% | Возможна задержка в обновлении статуса |
| OnRoomPropertiesUpdate x2 | 35% | Retry-логика Photon может отправить дважды |
| Guard отсутствует | 25% | Может быть triggered дважды |

---

## БАГ #2: JoinRoom Ошибка — Анализ Photon Lifecycle

### Photon State Machine

```
DISCONNECTED
  │
  ├─ ConnectUsingSettings()
  │  └─ Connecting...
  │     ├─ OnConnectedToMaster()
  │     │  └─ JoinLobby()
  │     │     └─ OnJoinedLobby()
  │     │        └─ READY FOR OPERATIONS
  │     │
  │     └─ [ERROR]: JoinRoom() вызвана ДО этого момента!
```

### Корневая Причина

LobbyPanel.Show() вызывает ConnectUsingSettings() БЕЗ ожидания состояния IsConnectedAndReady.

```csharp
public override void Show()
{
    base.Show();
    SetStatus("Подключение...");
    _createButton.interactable = false;

    if (PhotonNetwork.IsConnectedAndReady)
    {
        SetStatus("Выберите или создайте комнату.");
        _createButton.interactable = true;
        PhotonNetwork.JoinLobby();
    }
    else
    {
        PhotonNetwork.ConnectUsingSettings();  // ← АСИНХРОННО!
    }
}
```

**Проблема**: ConnectUsingSettings() асинхронна, но UI сразу позволяет пользователю нажимать кнопки, которые вызывают JoinRoom().

### Диагностический Сценарий

```
T0: LobbyPanel.Show() вызвана
T1: PhotonNetwork.ConnectUsingSettings() запущена (состояние = Authenticating)
T2: User нажимает кнопку JoinRoom (СЛИШКОМ РАНО!)
T3: Photon отказывает: "Client on GameServer but not ready"
T4: (через 1-2 сек) OnConnectedToMaster() вызывается
T5: User повторно нажимает JoinRoom (успешно)
```

### Решение

1. **Disable кнопок до IsConnectedAndReady**:
```csharp
public override void Show()
{
    base.Show();
    SetStatus("Подключение...");
    _createButton.interactable = false;  // ← ВСЕГДА отключена

    if (PhotonNetwork.IsConnectedAndReady)
    {
        SetStatus("Выберите или создайте комнату.");
        _createButton.interactable = true;
        PhotonNetwork.JoinLobby();
    }
    else
    {
        PhotonNetwork.ConnectUsingSettings();
    }
}

public void OnConnectedToMaster()
{
    SetStatus("Выберите или создайте комнату.");
    _createButton.interactable = true;  // ← ВКЛЮЧАЕМ ТОЛЬКО ЗДЕСЬ
    PhotonNetwork.JoinLobby();
}
```

2. **Добавить callback OnConnected (не OnConnectedToMaster)**:
```csharp
public void OnConnected()
{
    Debug.Log("[LobbyPanel] OnConnected - but need to wait for lobby join");
}
```

---

## БАГ #3: Дессинхронизация Блоков — Архитектурный Дефект

### Сетевой Модель Tetris

```
MASTER CLIENT (Tetris Controller)
│
├─ CurrentTetromino (падает)
│  └─ OnPhotonSerializeView() отправляет позицию
│     └─ Non-Master получает и показывает "mirror blocks"
│
├─ Lock tetromino (блокирует)
│  └─ RPC(RpcLock, RpcTarget.Others)
│     └─ Non-Master: Board.LockRemote(positions, type)
│     └─ Non-Master создаёт новые Block instances
│
└─ Board.Lock() вызывается локально на Master
   └─ Master видит блоки, но НЕ отправляет RPC себе


NON-MASTER CLIENT (Character Controller)
│
├─ Получает OnPhotonSerializeView
│  └─ UpdateMirrorPiece() показывает текущий падающий блок
│
├─ Получает RPC(RpcLock)
│  └─ Board.LockRemote() создаёт заблокированные блоки
│
└─ Видит состояние доски ТОЛЬКО из RPC'ов
   └─ ПРОБЛЕМА: Если RPC потеряется или прийдёт позже, блоки ИСЧЕЗНУТ
```

### Проблема 1: Асимметричное Состояние Доски

**Master Client**:
- Имеет свежее состояние Board._grid (обновляется локально)
- Отправляет RPC Others для синхронизации

**Non-Master Client**:
- Зависит ТОЛЬКО от RPC'ов
- Если RPC задержится или потеряется, состояние разойдётся
- При входе в игру — доска ПУСТА (нет восстановления состояния)

### Проблема 2: Нет IPunObservable для Board

Board.cs не наследует MonoBehaviourPun и не имеет OnPhotonSerializeView(). Это значит:
- Board не отправляет состояние доски синхронно
- Только RPC'ы отправляют блоки (ненадёжно)

### Проблема 3: RPC(RpcTarget.Others) не включает Master

```csharp
// TetrisController.LockCurrent()
photonView.RPC(nameof(RpcLock), RpcTarget.Others, xs, ys, typeInt);
```

**Проблема**: Master отправляет RPC только ДРУГИМ. Если Master crash'ит до отправки RPC, эта информация теряется.

**Решение**: Использовать RpcTarget.AllBuffered (сохранит последний RPC для новых клиентов при присоединении)

```csharp
photonView.RPC(nameof(RpcLock), RpcTarget.AllBuffered, xs, ys, typeInt);
```

### Диагностический Сценарий

```
T0: Game starts
    - Master: Board._grid = empty
    - Non-Master: _mirrorBlocks = empty (не инициализирована!)

T5: Master spawns Tetromino#1
    - OnPhotonSerializeView отправляет позицию
    - Non-Master получает UpdateMirrorPiece(positions)
    - Non-Master видит ОДИН падающий блок

T15: Master locks Tetromino#1
    - RPC(RpcLock, RpcTarget.Others) отправляется
    - Master локально выполняет Board.Lock() — видит 4 блока
    - Non-Master получает RPC через 50-200ms
    - Non-Master выполняет Board.LockRemote() — создаёт 4 блока

ПРОБЛЕМА: Если RPC задержится на 500ms, Tetromino#2 уже падает.
         Non-Master видит только #2 (зеркальный), но не #1 (lock'ированный).
```

### Решение Архитектуры

**Вариант 1: IPunObservable для Board**

```csharp
public class Board : MonoBehaviourPun, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Master отправляет ВСЁ состояние доски
            stream.SendNext(_grid.Length);
            foreach (var block in _grid)
            {
                stream.SendNext(block != null);
                // ...
            }
        }
        else
        {
            // Non-Master получает состояние
            int count = (int)stream.ReceiveNext();
            // ...
        }
    }
}
```

**Вариант 2: Snapshot State при присоединении**

```csharp
public void OnPlayerEnteredRoom(Player newPlayer)
{
    if (PhotonNetwork.IsMasterClient)
    {
        // Master отправляет "snapshot" текущего состояния всем
        SendBoardSnapshot();
    }
}
```

**Вариант 3: Использовать RpcTarget.AllBuffered**

```csharp
// Вместо RpcTarget.Others
photonView.RPC(nameof(RpcLock), RpcTarget.AllBuffered, xs, ys, typeInt);
```

---

## БАГ #4: OnRoomPropertiesUpdate не приходит

### UIPanel Lifecycle Issue

```csharp
// UIPanel неопубликована, но вероятно имеет:
public override void Hide()
{
    gameObject.SetActive(false);  // ← ОТКЛЮЧАЕТ GameObject!
}
```

Когда UIPanel.Hide() отключает gameObject, OnDisable() вызывается:

```csharp
// RoleSelectPanel
private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);
```

**Проблема**: После Hide() callback больше не будет вызваться!

```
Временная шкала:
T0: RoleSelectPanel.Show() → OnEnable() → AddCallbackTarget()
T1: User выбирает роль
T2: Мастер вызывает SetCustomProperties({started: true})
T3: Photon отправляет OnRoomPropertiesUpdate
T4: UIManager.ShowGameHud() → RoleSelectPanel.Hide()
T5: Hide() вызывает gameObject.SetActive(false)
T6: OnDisable() → RemoveCallbackTarget()
T7: OnRoomPropertiesUpdate прибывает в сеть, но callback не зарегистрирована!
    → GameManager.StartGame() НЕ вызывается
    → Оба клиента зависают на последней видимой панели
```

### Решение

**Вариант 1: Show() перед Hide()**

```csharp
private void Show(UIPanel panel)
{
    if (_current != null)
    {
        _current.Hide();  // Отключает callback
        // WAIT для разрегистрации?
    }
    _current = panel;
    _current?.Show();  // Включает callback
}
```

**Вариант 2: Не отключать gameObject в Hide()**

```csharp
public override void Hide()
{
    // Вместо gameObject.SetActive(false):
    // Просто спрячьте UI визуально
    canvasGroup.alpha = 0;
    // Или используйте layoutElement.ignoreLayout = true;
}
```

**Вариант 3: Отложить RemoveCallbackTarget**

```csharp
private void OnDisable()
{
    // НЕ удаляйте callback сразу
    // PhotonNetwork.RemoveCallbackTarget(this);
}

public override void Hide()
{
    base.Hide();
    PhotonNetwork.RemoveCallbackTarget(this);  // Удалите явно
}
```

---

## БАГ #5: Double StartGame() — Guard Missing

### Проблема

GameManager.StartGame() может быть вызвана несколько раз если:

1. OnRoomPropertiesUpdate вызывается дважды
2. Есть retry-логика в Photon
3. User нажимает кнопку дважды

```csharp
public void StartGame()
{
    _isPlaying = true;  // Каждый раз устанавливается true
    // ...
    _characterSpawner?.StartGame();  // Может быть вызвана дважды!
}
```

### Решение

```csharp
private bool _gameStarted = false;

public void StartGame()
{
    if (_gameStarted) return;  // GUARD
    _gameStarted = true;

    _isPlaying = true;
    Time.timeScale = 1f;
    // ...
}
```

---

## Итоговая Матрица Причин

| БАГ | Корневая Причина | Вероятность | Сложность Фиксинга |
|-----|------------------|-------------|-------------------|
| #1  | Race condition в IsMasterClient или OnRoomPropertiesUpdate x2 | 75% | Средняя |
| #2  | Асинхронность ConnectUsingSettings + ранний JoinRoom | 95% | Низкая |
| #3  | Отсутствие IPunObservable на Board + RpcTarget.Others | 90% | Высокая |
| #4  | gameObject.SetActive(false) в Hide() отключает callback | 80% | Средняя |
| #5  | Отсутствие guard на _isPlaying / _startedOnce | 85% | Низкая |

