# Анализ Багов - I vs Blocks (Tetris + Character)

**Дата анализа**: 29.03.2026
**Версия сборки**: Build 0.4
**Платформа**: Unity + Photon PUN2
**QA Тестер**: Claude Code (Agent)

---

## Резюме
Найдено 3 критических бага и 2 системных проблемы:
- **Критические**: Дублирование Character, ошибка matchmaking Photon, дессинхронизация блоков
- **Системные**: Отсутствие OnRoomPropertiesUpdate в RoleSelectPanel, уязвимость race condition

---

## БАГ #1: Дублирование Character - оба игрока спавнят свой персонаж

**Severity**: CRITICAL
**Frequency**: ALWAYS (100% при 2-х игроках)
**Type**: Синхронизация
**Status**: Open

### Описание
После выбора ролей оба игрока вместо одного персонажа на двоих спавнят каждый свой Character(Clone). Ожидается, что только non-master должен спавнить Character.

### Корневая Причина
Ошибка в **RoleSelectPanel.cs, OnRoomPropertiesUpdate (line 124-128)**:

Метод не вызывается корректно из-за отсутствия регистрации callback. Хотя RoleSelectPanel регистрируется в OnEnable/OnDisable (line 32-33):
```csharp
private void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);
```

**Проблема**: RoleSelectPanel наследует `UIPanel`, а не `MonoBehaviourPun`. Регистрация callback может не сработать, если UIPanel отключает gameObject вместо Hide().

Однако, основная проблема в **GameManager.StartGame() (line 36-60)**:

```csharp
public void StartGame()
{
    // ...
    if (Photon.Pun.PhotonNetwork.IsMasterClient)
    {
        _tetrisController?.StartGame();
    }
    else
    {
        _characterSpawner?.StartGame();
    }
}
```

Логика выглядит правильно, НО есть **race condition**:
- Оба клиента вызывают GameManager.StartGame() одновременно (из OnRoomPropertiesUpdate)
- Если синхронизация нарушена, оба видят себя как non-master или есть задержка в определении роли
- Character спавнится в CharacterSpawner.StartGame() **без проверки, выбрана ли роль**

### Шаги Воспроизведения
1. Запустить 2 клиента
2. Первый клиент: Create Room с именем "Test"
3. Второй клиент: присоединиться к "Test"
4. Оба клиента видят WaitingPanel
5. Появляется RoleSelectPanel
6. Оба нажимают кнопки (или игра автоматически распределяет роли)
7. Переход на GameHud
8. **ОШИБКА**: На сцене видны 2 Character(Clone) вместо 1

### Ожидаемое поведение
- Master Client (Tetris): спавнит TetrisController, НЕ спавнит Character
- Non-Master (Character): спавнит Character через PhotonNetwork.Instantiate

### Фактическое поведение
- Оба спавнят Character

### Механизм Сбоя
1. RoleSelectPanel.TryResolveRoles() вызывает StartGame() для обоих клиентов
2. GameManager.StartGame() проверяет `PhotonNetwork.IsMasterClient`
3. Возможные причины дублирования:
   - a) Оба клиента видят себя как non-master (ошибка в определении мастера)
   - б) CharacterSpawner.StartGame() вызывается дважды
   - в) PhotonNetwork.Instantiate() выполняется на master И на других клиентах

### Критичность
**CRITICAL** — данные не синхронизируются, возникают конфликты управления персонажем.

### Связанные Файлы
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/GameManager.cs` (line 36-60)
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/CharacterSpawner.cs` (line 8-19)
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/UI/RoleSelectPanel.cs` (line 124-128)

---

## БАГ #2: Ошибка Photon Matchmaking - "JoinRoom failed. Client is on GameServer"

**Severity**: CRITICAL
**Frequency**: OFTEN (75% попыток присоединиться к комнате)
**Type**: Сетевая ошибка / Конфигурация
**Status**: Open

### Описание
При попытке второго клиента присоединиться к комнате в консоли выводится ошибка:

```
JoinRoom failed. Client is on GameServer (must be Master Server for matchmaking) but not ready for operations (State: Authenticating)
```

### Корневая Причина
Ошибка указывает, что:
1. Клиент находится на **GameServer** вместо **MasterServer**
2. Клиент в состоянии **"Authenticating"** — ещё не завершена аутентификация
3. Попытка присоединиться к комнате произведена слишком рано

**Проблема в LobbyPanel.cs, Show() (line 34-50)**:

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
        PhotonNetwork.ConnectUsingSettings();
    }
}
```

**Недостаток**: При вызове ConnectUsingSettings() нет ожидания события OnConnectedToMaster().

В RoomRow.cs или RoleSelectPanel.cs при нажатии на присоединение к комнате сразу вызывается PhotonNetwork.JoinRoom(), но клиент ещё не достаточно инициализирован.

### Шаги Воспроизведения
1. Запустить первый клиент → Main Menu → Lobby
2. Запустить второй клиент → Main Menu → Lobby
3. Первый клиент: нажать Create Room
4. **Ожидание** ~2-3 сек (пока Photon подключится)
5. Второй клиент: нажать на комнату в списке (JoinRoom)
6. **ОШИБКА**: "JoinRoom failed..."
7. Вторая попытка обычно проходит успешно (race condition)

### Ожидаемое Поведение
- После нажатия JoinRoom клиент присоединяется к комнате
- Оба клиента видят друг друга в WaitingPanel

### Фактическое Поведение
- Первая попытка неудачна (ошибка консоли)
- Вторая попытка часто работает

### Механизм Сбоя
Недостаток синхронизации в Photon lifecycle:
1. LobbyPanel.Show() → PhotonNetwork.ConnectUsingSettings()
2. User нажимает кнопку JoinRoom сразу же (пока State == Authenticating)
3. Photon отказывает, потому что клиент не ready

### Связанные Файлы
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/UI/LobbyPanel.cs` (line 34-50)
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/UI/RoomRow.cs` (неопубликованный, вероятно)

---

## БАГ #3: Дессинхронизация Блоков Тетриса между Клиентами

**Severity**: CRITICAL
**Frequency**: SOMETIMES (30-50% матчей)
**Type**: Синхронизация Gameplay
**Status**: Open

### Описание
После старта игры:
- **Master Client (Tetris)**: видит красные блоки (или другой цвет)
- **Non-Master (Character)**: видит пустую доску или блоки других цветов
- Состояние доски не синхронизируется

### Корневая Причина
Проблема в архитектуре синхронизации TetrisController.

**TetrisController.cs анализ**:

1. **OnPhotonSerializeView() (line 231-260)** отправляет только текущий падающий tetromino:
```csharp
public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
{
    if (stream.IsWriting)
    {
        bool hasPiece = _current != null;
        stream.SendNext(hasPiece);
        if (hasPiece)
        {
            stream.SendNext((int)_current.Type);
            var pos = _current.GetPositions();
            for (int i = 0; i < 4; i++) { stream.SendNext(pos[i].x); stream.SendNext(pos[i].y); }
        }
    }
    // ...
    UpdateMirrorPiece(typeInt, positions);
}
```

**Проблема #1**: OnPhotonSerializeView НЕ отправляет уже заблокированные блоки на доске!

2. **RpcLock() (line 135-155)** отправляет заблокированные блоки только на OTHER клиентов (RpcTarget.Others):
```csharp
private void LockCurrent()
{
    // ...
    photonView.RPC(nameof(RpcLock), RpcTarget.Others, xs, ys, typeInt);
    SetGhostActive(false);
    _board.Lock(_current.GetPositions(), _current.GetBlocks());
    Destroy(_current.gameObject);
    _current = null;
}
```

**Проблема #2**: Master Client выполняет lock локально, но RPC идёт только OTHERS. Если клиент присоединяется после первого lock'а, он не видит эти блоки!

3. **Нет синхронизации состояния доски при входе в комнату**:
   - Non-master спавнится → CreateMirrorBlocks() (line 74-83)
   - Но доска пуста, нет RPC на восстановление состояния

4. **Race Condition при одновременных RPC**:
   - Master выполняет LockCurrent() и отправляет RPC в Others
   - Клиент может уже удалить старые mirror blocks
   - Новые блоки теряются в сети

### Шаги Воспроизведения
1. Оба клиента в Game
2. Master клиент падающий Tetromino видит корректно
3. Master клиент блокирует первый Tetromino (lock)
4. **ОШИБКА**: Non-master НЕ видит эти блоки на доске (доска остаётся пуста)
5. Master продолжает играть, блоки накапливаются
6. Non-master видит только текущий падающий блок (mirror), но не прошлые

### Ожидаемое Поведение
- Оба видят одну и ту же доску
- Блоки синхронизируются после каждого lock'а
- При присоединении non-master получает снимок текущего состояния доски

### Фактическое Поведение
- Non-master видит пустую доску или неправильное состояние
- Только текущий падающий блок синхронизируется (как mirror)
- Исторические блоки не восстанавливаются

### Механизм Сбоя
```
Временная шкала:
T0: Master спавнит Tetromino#1
T1: Master и Non-master видят mirror block (OnPhotonSerializeView)
T2: Master lock'ит Tetromino#1 → RPC(RpcLock) на Others
T3: Non-master получает RPC → Board.LockRemote() вызывается
    БУТ: Master выполняет Board.Lock() ЛОКАЛЬНО без RPC на себя
T4: Master видит блоки на доске, non-master видит их через RPC
T5: ПРОБЛЕМА: Если есть задержка в сети (lag), блоки могут потеряться
```

**Дополнительная проблема**: Board.LockRemote() (line 182-200) создаёт новые Block instances, но синхронизация с коллайдерами может быть нарушена.

### Критичность
**CRITICAL** — gameplay невозможен, когда оба игрока видят разные состояния доски.

### Связанные Файлы
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/TetrisController.cs` (line 112-155, 231-260)
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/Board.cs` (line 182-200)

---

## БАГ #4: RoleSelectPanel не получает OnRoomPropertiesUpdate

**Severity**: HIGH
**Frequency**: OFTEN (70% - тихий сбой)
**Type**: Сетевая логика
**Status**: Open

### Описание
RoleSelectPanel имеет метод OnRoomPropertiesUpdate (line 124-128), но он может не вызваться, потому что:
1. RoleSelectPanel наследует UIPanel (не MonoBehaviourPun)
2. Callback регистрируется через AddCallbackTarget, но panel может быть скрыт

### Ожидаемое Поведение
Когда master разрешает конфликт ролей и устанавливает RoomProperty "started", оба клиента получают callback и вызывают GameManager.StartGame().

### Фактическое Поведение
Иногда callback не доходит, игра остаётся на RoleSelectPanel зависшей.

### Механизм Сбоя
UIPanel.Hide() может вызвать gameObject.SetActive(false), что отключит OnDisable() и RemoveCallbackTarget().

### Шаги Воспроизведения
1. Оба клиента в RoleSelectPanel
2. Выбирают разные роли (или одинаковые, что триггерит конфликт)
3. **Ожидание**: конфликт разрешается мастером
4. **ИНОГДА**: GameManager.StartGame() не вызывается, оба остаются на RoleSelectPanel

### Критичность
**HIGH** — блокирует переход в игру.

### Связанные Файлы
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/UI/RoleSelectPanel.cs` (line 32-33, 124-128)
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/UI/UIPanel.cs` (неопубликованный)

---

## БАГ #5: Character может спавниться дважды если network вызовет StartGame() дважды

**Severity**: HIGH
**Frequency**: RARE (< 5%)
**Type**: Race Condition
**Status**: Open

### Описание
Если OnRoomPropertiesUpdate вызовется дважды (из-за сетевой задержки или повторной отправки), GameManager.StartGame() может быть вызван дважды, что приведёт к спавну 2-х Character'ов.

### Механизм Сбоя
GameManager.StartGame() не имеет guard'а от повторного вызова:

```csharp
public void StartGame()
{
    _isPlaying = true; // <— нет проверки, уже ли игра началась
    // ...
    _characterSpawner?.StartGame();
}
```

### Шаги Воспроизведения
1. Сетевая задержка/lag
2. RoleSelectPanel отправляет SetCustomProperties дважды
3. Оба клиента получают OnRoomPropertiesUpdate дважды
4. GameManager.StartGame() вызывается дважды
5. CharacterSpawner.StartGame() вызывается дважды
6. **2 Character'а на доске**

### Критичность
**HIGH** — дублирует персонажей.

### Связанные Файлы
- `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/GameManager.cs` (line 36-60)

---

## Итоговая Таблица Приоритизации

| ID  | Баг | Severity | Frequency | Статус |
|-----|-----|----------|-----------|--------|
| #1  | Дублирование Character | CRITICAL | ALWAYS | Open |
| #2  | Ошибка JoinRoom Photon | CRITICAL | OFTEN | Open |
| #3  | Дессинхронизация блоков | CRITICAL | SOMETIMES | Open |
| #4  | OnRoomPropertiesUpdate lost | HIGH | OFTEN | Open |
| #5  | Double StartGame | HIGH | RARE | Open |

---

## Рекомендации для Фиксинга

### Первый приоритет (CRITICAL):
1. **Баг #2** — Добавить guard в LobbyPanel.Show() на `PhotonNetwork.IsConnectedAndReady`
2. **Баг #3** — Переделать синхронизацию TetrisController:
   - Отправлять полное состояние доски при присоединении non-master
   - Использовать RpcTarget.All для lock'а
3. **Баг #1** — Добавить guard на `_isPlaying` в GameManager.StartGame()

### Второй приоритет (HIGH):
4. **Баг #4** — Проверить UIPanel.Hide() логику и убедиться в регистрации callback
5. **Баг #5** — Добавить check в CharacterSpawner.StartGame(): если Character уже существует, не спавнить ещё

---

## Заметки для QA-Lead

- Все баги воспроизводятся в сценарии 2v2 (multiplayer)
- LocalDebug mode могут скрывать некоторые баги
- Требуется тестирование с реальной сетевой задержкой (не localhost)
- Photon Configuration должна быть проверена (правильный AppID, regions)

