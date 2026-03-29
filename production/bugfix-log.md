# Bug Fix Log -- BLOKS vs CHARACTER

Лог подтвержденных исправлений багов. Каждая запись содержит дату, описание проблемы, корневую причину и примененное решение.

---

## 2026-03-29 -- Сетевая синхронизация: замена IsMasterClient на роль игрока

**Сборка:** Build 0.4+
**Область:** Multiplayer, Photon PUN2, роль игрока (Blocs / Character)

### Fix #1: NextPiecePreview -- персонаж мог перетаскивать фигуры

| Поле | Значение |
|------|----------|
| Файл | `Assets/Scripts/NextPiecePreview.cs` |
| Severity | Critical |
| Корневая причина | Проверка `IsMasterClient` вместо роли. Если персонаж был мастер-клиентом, он мог перетаскивать тетромино. |
| Исправление | Заменено на `GameManager.IsTetrisPlayer()` |

### Fix #2: TetrisController -- фигура не управлялась если тетрис-игрок не мастер

| Поле | Значение |
|------|----------|
| Файл | `Assets/Scripts/TetrisController.cs` |
| Severity | Critical |
| Корневая причина | `Update()`, `SpawnAtColumn()`, `SpawnBombNpcAt()` проверяли `IsMasterClient`. Если тетрис-игрок выбрал роль "Blocs" но не был мастером -- не мог управлять. |
| Исправление | Все проверки заменены на `GameManager.IsTetrisPlayer()` |

### Fix #3: Персонаж не видел падающую фигуру

| Поле | Значение |
|------|----------|
| Файлы | `Assets/Scripts/TetrisController.cs`, `Assets/Scripts/GameManager.cs` |
| Severity | Critical |
| Корневая причина | `CreateMirrorBlocks()` вызывался только при `!IsMasterClient`. Персонаж не всегда мастер -- mirror-блоки не создавались. |
| Исправление | Добавлен `InitForViewing()` для персонажа. Добавлены `BroadcastPiecePositions()` + `RpcUpdatePiece` -- позиция фигуры синхронизируется через RPC при спавне и каждом падении. |

### Fix #4: Разные фигуры в NextPiecePreview у двух игроков

| Поле | Значение |
|------|----------|
| Файлы | `Assets/Scripts/TetrominoSpawner.cs`, `Assets/Scripts/TetrisController.cs` |
| Severity | Major |
| Корневая причина | Каждый клиент генерировал `NextType` локально через `PickRandom()`. Синхронизации не было. |
| Исправление | Добавлен `SetNextType()` в TetrominoSpawner. Тетрис-игрок транслирует `NextType` через `RpcSyncNext` при каждой смене и при старте игры. |

### Fix #5: GameManager -- добавлен IsTetrisPlayer()

| Поле | Значение |
|------|----------|
| Файл | `Assets/Scripts/GameManager.cs` |
| Severity | Improvement |
| Корневая причина | Не было централизованной проверки роли. Каждый скрипт использовал `IsMasterClient` как proxy для роли, что ломалось при смене мастера или выборе роли. |
| Исправление | Добавлен статический метод `IsTetrisPlayer()`, читающий роль из room properties Photon. |

---

### Архитектурное решение (ADR)

**Правило:** Для определения роли игрока (Blocs / Character) всегда использовать `GameManager.IsTetrisPlayer()`, а НЕ `PhotonNetwork.IsMasterClient`.

**Причина:** MasterClient -- это сетевая роль Photon (кто управляет комнатой), а не игровая роль. Игрок может выбрать роль "Blocs" не будучи мастером, и наоборот. Смешение этих понятий приводило к 4 критическим багам.
