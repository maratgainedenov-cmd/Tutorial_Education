# Fix Recommendations - I vs Blocks Network Bugs

**Дата**: 29.03.2026
**Версия**: 1.0
**Для**: Programmer + Systems Designer

---

## Executive Summary

5 обнаруженных багов требуют 5 фиксов:
1. **БАГ #1** (CRITICAL): CharacterSpawner + GameManager guard
2. **БАГ #2** (CRITICAL): LobbyPanel callback timing
3. **БАГ #3** (CRITICAL): TetrisController RPC architecture
4. **БАГ #4** (HIGH): UIPanel lifecycle
5. **БАГ #5** (HIGH): GameManager guard

**Приоритет**: #2 (самая простая), затем #5, #4, #1, #3 (самая сложная)

---

## FIX #1: Guard в GameManager.StartGame()

**Файл**: `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/GameManager.cs`

### Проблема
GameManager.StartGame() может быть вызвана дважды, спавня 2 Character'а.

### Решение
Добавить guard переменную.

### Код

**BEFORE**:
```csharp
public class GameManager : MonoBehaviour
{
    private bool _isPaused;
    private bool _isPlaying;

    public void StartGame()
    {
        _isPlaying = true;
        Time.timeScale = 1f;
        // ...
    }
}
```

**AFTER**:
```csharp
public class GameManager : MonoBehaviour
{
    private bool _isPaused;
    private bool _isPlaying;
    private bool _startedOnce = false;  // ← ADD THIS

    public void StartGame()
    {
        if (_startedOnce) return;  // ← ADD THIS GUARD
        _startedOnce = true;

        _isPlaying = true;
        Time.timeScale = 1f;
        _grid?.SetActive(true);
        _exit?.SetActive(true);
        UIManager.Instance?.ShowGameHud();
        GameHUD.Instance?.StartTimer();

        if (LocalDebug)
        {
            _tetrisController?.StartGame();
            _characterSpawner?.StartGame();
            return;
        }

        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            _tetrisController?.StartGame();
        }
        else
        {
            _characterSpawner?.StartGame();
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        _startedOnce = false;  // ← RESET ON RESTART
        if (LocalDebug)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            Photon.Pun.PhotonNetwork.LeaveRoom();
    }
}
```

### Тестирование
- TC-016 (Rapid Role Selection): не должно быть дублирования

### Риск: LOW
- Изменение незначительно
- Не влияет на другие системы

---

## FIX #2: LobbyPanel Button Disable Logic

**Файл**: `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/UI/LobbyPanel.cs`

### Проблема
Кнопка Create Room активна слишком рано, до завершения Photon подключения. JoinRoom вызывается в состоянии "Authenticating".

### Решение
Отключить кнопку до OnConnectedToMaster().

### Код

**BEFORE**:
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

public void OnConnectedToMaster()
{
    PhotonNetwork.JoinLobby();
}
```

**AFTER**:
```csharp
public override void Show()
{
    base.Show();
    SetStatus("Подключение...");
    _createButton.interactable = false;  // ← ВСЕГДА отключена при входе

    if (PhotonNetwork.IsConnectedAndReady)
    {
        OnConnectedToMaster();  // ← CALL DIRECTLY
    }
    else
    {
        PhotonNetwork.ConnectUsingSettings();
    }
}

public void OnConnectedToMaster()
{
    SetStatus("Выберите или создайте комнату.");
    _createButton.interactable = true;  // ← ВКЛЮЧИТЬ КНОПКУ ЗДЕСЬ
    PhotonNetwork.JoinLobby();
}
```

### Альтернативный подход (более надёжный)

Если OnConnectedToMaster() не вызывается по какой-то причине, добавить OnConnected():

```csharp
public void OnConnected()
{
    // Called when connected to any Photon server
    Debug.Log("[LobbyPanel] OnConnected - waiting for Master...");
}

public void OnConnectedToMaster()
{
    SetStatus("Выберите или создайте комнату.");
    _createButton.interactable = true;
    PhotonNetwork.JoinLobby();
}
```

### Тестирование
- TC-001: оба клиента подключаются без ошибок
- TC-002: первая попытка JoinRoom проходит

### Риск: LOW
- Логика потокобезопасна
- Не добавляет новые зависимости

---

## FIX #3: CharacterSpawner Check

**Файл**: `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/CharacterSpawner.cs`

### Проблема
CharacterSpawner.StartGame() спавнит Character без проверки, есть ли уже Character в сцене.

### Решение
Добавить проверку на наличие существующего Character.

### Код

**BEFORE**:
```csharp
public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private string _characterPrefabName = "Character";

    public void StartGame()
    {
        if (GameManager.LocalDebug)
        {
            var prefab = Resources.Load<GameObject>(_characterPrefabName);
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(_characterPrefabName, transform.position, Quaternion.identity);
        }
    }
}
```

**AFTER**:
```csharp
public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private string _characterPrefabName = "Character";
    private Character _spawnedCharacter;  // ← ADD THIS

    public void StartGame()
    {
        // Guard: если Character уже существует, не спавнить ещё
        if (_spawnedCharacter != null) return;  // ← ADD THIS

        if (GameManager.LocalDebug)
        {
            var prefab = Resources.Load<GameObject>(_characterPrefabName);
            _spawnedCharacter = Instantiate(prefab, transform.position, Quaternion.identity)
                .GetComponent<Character>();  // ← CACHE THE REFERENCE
        }
        else
        {
            GameObject obj = PhotonNetwork.Instantiate(_characterPrefabName, transform.position, Quaternion.identity);
            _spawnedCharacter = obj.GetComponent<Character>();  // ← CACHE THE REFERENCE
        }
    }

    // Optional: Add cleanup on room leave
    private void OnDestroy()
    {
        _spawnedCharacter = null;
    }
}
```

### Альтернатива: FindObjectOfType Check

Если нет доступа к кэшированию:

```csharp
public void StartGame()
{
    // Guard: если Character уже существует, не спавнить ещё
    Character existingCharacter = FindObjectOfType<Character>();
    if (existingCharacter != null) return;

    // ... spawn logic
}
```

### Тестирование
- TC-004: ровно 1 Character на доске
- TC-009: Character управляется корректно

### Риск: LOW
- Добавляет минимальный overhead
- Не изменяет логику спауна

---

## FIX #4: UIPanel Hide/Show Lifecycle

**Файл**: `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/UI/UIPanel.cs`

### Проблема
UIPanel.Hide() вызывает gameObject.SetActive(false), отключая OnDisable() и удаляя Photon callback. Если OnRoomPropertiesUpdate прибудет ПОСЛЕ Hide(), он не будет обработан.

### Решение
Отложить RemoveCallbackTarget до явного вызова в Hide().

### Код

**BEFORE** (гипотетический UIPanel):
```csharp
public class UIPanel : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}

// RoleSelectPanel
public class RoleSelectPanel : UIPanel, IInRoomCallbacks
{
    private void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
    private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);  // ← CALLED BY Hide()

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoomStarted))
            GameManager.Instance?.StartGame();  // ← МОЖЕТ НЕ ВЫЗВАТЬСЯ
    }
}
```

**AFTER**:
```csharp
public class UIPanel : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}

// RoleSelectPanel
public class RoleSelectPanel : UIPanel, IInRoomCallbacks
{
    private void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);

    public override void Hide()
    {
        PhotonNetwork.RemoveCallbackTarget(this);  // ← REMOVE EXPLICITLY BEFORE HIDE
        base.Hide();
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoomStarted))
            GameManager.Instance?.StartGame();
    }

    // Optional: Keep callback even when hidden
    // private void OnDisable() { } // ← EMPTY, НЕ УДАЛЯТЬ CALLBACK
}
```

### Альтернатива: Visibility вместо SetActive

Если нужно оставить panel интерактивной в фоне:

```csharp
public override void Hide()
{
    gameObject.SetActive(true);  // ← ОСТАЁТСЯ АКТИВНЫМ
    canvasGroup.alpha = 0;       // ← СПРЯЧЬТЕ ВИЗУАЛЬНО
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;
}

public override void Show()
{
    gameObject.SetActive(true);
    canvasGroup.alpha = 1;
    canvasGroup.interactable = true;
    canvasGroup.blocksRaycasts = true;
}
```

### Тестирование
- TC-003: RoleSelectPanel появляется после WaitingPanel
- TC-005: конфликт ролей разрешается, игра стартует

### Риск: MEDIUM
- Требует проверки всех UIPanel подклассов
- Может повлиять на другие callback'и

---

## FIX #5: TetrisController RPC Architecture (СЛОЖНЫЙ ФИХ)

**Файл**: `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/TetrisController.cs`

**Файл**: `/c/Users/Marat/Documents/Test/Tutorial_Education/Assets/Scripts/Board.cs`

### Проблема
Board.Lock() выполняется только на Master, Non-Master получает информацию только через RPC. Если RPC потеряется или задержится, блоки не синхронизируются.

### Решение
Использовать RpcTarget.AllBuffered вместо RpcTarget.Others.

### Код

**BEFORE**:
```csharp
// TetrisController.cs
private void LockCurrent()
{
    var positions = _current.GetPositions();
    int[] xs = new int[positions.Length];
    int[] ys = new int[positions.Length];
    for (int i = 0; i < positions.Length; i++) { xs[i] = positions[i].x; ys[i] = positions[i].y; }
    int typeInt = (int)_current.Type;

    if (GameManager.LocalDebug)
    {
        RpcLock(xs, ys, typeInt);
    }
    else
    {
        // ПРОБЛЕМА: RPC идёт только Others, Master не получает
        photonView.RPC(nameof(RpcLock), RpcTarget.Others, xs, ys, typeInt);
        SetGhostActive(false);
        _board.Lock(_current.GetPositions(), _current.GetBlocks());
        Destroy(_current.gameObject);
        _current = null;
    }
}
```

**AFTER**:
```csharp
// TetrisController.cs
private void LockCurrent()
{
    var positions = _current.GetPositions();
    int[] xs = new int[positions.Length];
    int[] ys = new int[positions.Length];
    for (int i = 0; i < positions.Length; i++) { xs[i] = positions[i].x; ys[i] = positions[i].y; }
    int typeInt = (int)_current.Type;

    if (GameManager.LocalDebug)
    {
        RpcLock(xs, ys, typeInt);
    }
    else
    {
        // РЕШЕНИЕ: Используйте AllBuffered для новых клиентов
        photonView.RPC(nameof(RpcLock), RpcTarget.AllBuffered, xs, ys, typeInt);
        SetGhostActive(false);
        _board.Lock(_current.GetPositions(), _current.GetBlocks());
        Destroy(_current.gameObject);
        _current = null;
    }
}

[PunRPC]
private void RpcLock(int[] xs, int[] ys, int typeInt)
{
    var positions = new Vector2Int[xs.Length];
    for (int i = 0; i < xs.Length; i++)
        positions[i] = new Vector2Int(xs[i], ys[i]);

    if (GameManager.LocalDebug)
    {
        SetGhostActive(false);
        _board.Lock(_current.GetPositions(), _current.GetBlocks());
        Destroy(_current.gameObject);
        _current = null;
    }
    else
    {
        // ВАЖНО: Non-Master использует LockRemote
        if (!PhotonNetwork.IsMasterClient)
        {
            if (_mirrorBlocks != null)
                foreach (var b in _mirrorBlocks) b.gameObject.SetActive(false);
            _board.LockRemote(positions, (TetrominoType)typeInt);
        }
    }
}
```

### Альтернатива: IPunObservable для Board (более сложно)

Если RpcTarget.AllBuffered недостаточно, реализовать полную синхронизацию:

```csharp
// Board.cs
public class Board : MonoBehaviourPun, IPunObservable
{
    private Block[,] _grid;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Master отправляет состояние доски
            stream.SendNext(_width);
            stream.SendNext(_height);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    bool occupied = _grid[x, y] != null;
                    stream.SendNext(occupied);
                    if (occupied)
                    {
                        // Отправить цвет блока (если нужно)
                        var color = _grid[x, y].GetComponent<SpriteRenderer>().color;
                        stream.SendNext(color.r);
                        stream.SendNext(color.g);
                        stream.SendNext(color.b);
                    }
                }
            }
        }
        else
        {
            // Non-Master получает и восстанавливает состояние
            int width = (int)stream.ReceiveNext();
            int height = (int)stream.ReceiveNext();

            // Clear old grid
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_grid[x, y] != null)
                        Destroy(_grid[x, y].gameObject);
                    _grid[x, y] = null;
                }
            }

            // Reconstruct from stream
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool occupied = (bool)stream.ReceiveNext();
                    if (occupied)
                    {
                        float r = (float)stream.ReceiveNext();
                        float g = (float)stream.ReceiveNext();
                        float b = (float)stream.ReceiveNext();

                        Block b = Instantiate(_blockPrefab, transform);
                        b.transform.localPosition = new Vector3(x, y, 0f);
                        b.SetColor(new Color(r, g, b, 1f));
                        _grid[x, y] = b;
                    }
                }
            }
        }
    }
}
```

### Тестирование
- TC-007: Non-Master видит текущий Tetromino
- TC-008: Non-Master видит заблокированные блоки
- TC-012: синхронизация работает при lag

### Риск: HIGH
- Влияет на основную gameplay механику
- Требует тщательного тестирования

---

## План Внедрения

### Phase 1: LOW-RISK FIXES (День 1)
1. **FIX #5**: Add guard to GameManager.StartGame()
2. **FIX #2**: Fix LobbyPanel button logic
3. **FIX #3**: Add check to CharacterSpawner

**Тестирование**: TC-001, TC-002, TC-004, TC-016

### Phase 2: MEDIUM-RISK FIXES (День 2)
4. **FIX #4**: UIPanel Hide/Show lifecycle

**Тестирование**: TC-003, TC-005

### Phase 3: HIGH-RISK FIXES (День 3-4)
5. **FIX #1**: TetrisController RPC architecture (начать с RpcTarget.AllBuffered)

**Тестирование**: TC-007, TC-008, TC-012, TC-014

---

## Verification Checklist

После каждого фиксинга:
- [ ] Код скомпилирован без ошибок
- [ ] LocalDebug mode протестирован
- [ ] Сетевой режим (2 клиента) протестирован
- [ ] Соответствующие test cases прошли
- [ ] Нет новых ошибок в консоли

---

## Регрессионное Тестирование

После всех фиксов запустить полный регрессионный suite:
- TC-001 до TC-016 (все должны пройти)
- Bonus: TC-012 с lag simulation

