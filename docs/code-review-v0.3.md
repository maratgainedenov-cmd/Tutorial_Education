# Code Review: Tetris vs Character — v0.3

**Дата:** 2026-03-23 | **Ревьюер:** Lead Programmer Agent
**Вердикт:** ⛔ CHANGES REQUIRED

---

## Статус соответствия стандартам: 1/6

- Публичные методы и классы без doc-комментариев (`///`) — нигде в проекте
- `FindObjectOfType` как fallback в 3 файлах — скрытые зависимости
- Конфигурационные значения частично захардкожены (без внешних конфигов)
- Интерфейсов нет — все зависимости на конкретные классы

---

## 🔴 Баги и критические проблемы

### 1. BombNPC не синхронизирован по сети
**Файл:** `TetrisController.cs:47`
`Instantiate` вместо `PhotonNetwork.Instantiate`. NPC существует только на мастер-клиенте. Его движение, взрыв и урон игроку не видны и не применяются у Player 2. **Критическая проблема для мультиплеера.**

### 2. BombNPCSpawner спаунит на обоих клиентах
**Файл:** `BombNPCSpawner.cs`
Нет проверки `PhotonNetwork.IsMasterClient` перед спауном. В мультиплеере два независимых набора NPC на каждой машине.

### 3. Анимация смерти не воспроизводится
**Файл:** `Character.cs:254`
`gameObject.SetActive(false)` вызывается сразу после `_animator.SetBool("IsDead", true)`. Объект деактивируется раньше, чем анимация успевает воспроизвестись.

### 4. NullReferenceException при паузе
**Файл:** `GameManager.cs:73`
`_pausePanel.SetActive(_isPaused)` — единственный вызов без `?.`. Все остальные панели используют `?.SetActive(...)`.

### 5. Синглтон без защиты от дублей
**Файл:** `GameManager.cs:25`
`Instance = this` без `if (Instance != null) return`. При двух объектах GameManager в сцене второй молча перезапишет первый.

### 6. Invoke при смерти персонажа
**Файл:** `Character.cs:146`
`Invoke(nameof(StopPushAnim), 0.4f)` не отменяется при смерти. Если персонаж умрёт за 0.4 сек — `StopPushAnim` вызовется на деактивированном аниматоре.

### 7. Хрупкая логика RpcLock
**Файл:** `TetrisController.cs:130–144`
Мастер получает свой же RPC через `RpcTarget.All`, что создаёт двойное выполнение. Логика ветвления `IsMasterClient` спасает, но делает код хрупким.

---

## 🟡 Проблемы архитектуры

### FindObjectOfType как fallback (3 файла)
- `Character.cs:47` — поиск Board
- `TetrisController.cs:25–26` — поиск Board и Spawner
- `BombNPC.cs:53` — поиск Board

O(N) по всем объектам сцены. Скрытые зависимости. Назначать явно через Inspector.

### GameManager нарушает SRP
Управляет UI, состоянием игры и условиями победы одновременно. Сетевая логика ("кто стартует") зашита в `StartGame()`.

### Мёртвый код
**Файл:** `Board.cs:91–111`
Метод `ApplyGravityAnimated()` объявлен, но нигде не вызывается. Используется `ApplyGravity()`.

### Лишний ApplyGravity в Lock()
**Файл:** `Board.cs:37`
Первый `ApplyGravity()` до `ClearLines()` избыточен — блоки только залочились и никуда не падали.

### Нет интерфейса IBoard
`Character` и `BombNPC` зависят от `Board` напрямую. Затрудняет тестирование и замену реализации.

---

## 🟠 Проблемы производительности

### FindWithTag в Update каждый кадр
**Файл:** `BombNPC.cs:105`
`GameObject.FindWithTag("Player")` — O(N) по сцене каждый фрейм пока `_target == null`. Вынести в `OnLanded()` или `Start()`.

### Аллокация массива в Update
**Файл:** `TetrisController.cs:188–189`
`Shifted()` создаёт `new Vector2Int[4]` при каждом вызове в `UpdateGhost()`. GC pressure. Предкэшировать буфер как поле класса.

### Camera.main без кэширования
**Файлы:** `Board.cs:34,47,180`, `Character.cs`
`Camera.main` — это `FindObjectOfType<Camera>` под капотом. Кэшировать в `Awake()`.

### OverlapCircleAll с аллокацией
**Файл:** `Character.cs:150`
`Physics2D.OverlapCircleAll` аллоцирует массив при каждом вызове. Заменить на `OverlapCircleNonAlloc` с переиспользуемым буфером.

### Vector2.Distance в двойном цикле
**Файл:** `Board.cs:164`
`Vector2.Distance` (квадратный корень) для каждой клетки в `Explode()`. Заменить на сравнение `sqrMagnitude`.

### Корутина-опрос на каждый NPC
**Файл:** `BombNPCSpawner.cs:37–40`
`while (npc != null) yield return null` — Unity null-check через C++ каждый кадр. Использовать событие `OnDestroy` в BombNPC.

---

## 🔵 Нарушения паттернов Unity

### DOTween без SetLink
**Файл:** `BombNPC.cs:86–88`
Бесконечный tween (`SetLoops(-1)`). Добавить `.SetLink(gameObject)` чтобы DOTween автоматически убивал tween при уничтожении объекта.

### Некорректная логика Push
**Файл:** `BombNPC.cs:255`
`Push()` возвращает управление если `_isLanded == true`. После приземления игрок не может оттолкнуть NPC. Вероятно условие должно быть только `_exploded`.

### GetComponentInParent избыточный fallback
**Файл:** `BombNPC.cs:292`
`GetComponentInParent<Character>() ?? GetComponent<Character>()` — `GetComponentInParent` уже включает сам объект, второй вызов всегда избыточен.

---

## ✅ Положительные наблюдения

- `_grid ??= new Block[_width, _height]` — корректная защита (Board.cs)
- Ghost piece с чистой очисткой в `OnDisable`
- `DOKill()` перед новыми твинами предотвращает конфликты
- `_isPlaying` флаг защищает от двойного GameOver/Win
- `OnDrawGizmosSelected` в BombNPC — хорошая отладочная практика
- Разделение `TryDestroySide` / `TryDestroyDown` логически чисто

---

## Список исправлений (приоритет)

| Приоритет | Файл | Исправление |
|---|---|---|
| P0 | `TetrisController.cs:47` | Заменить `Instantiate` на `PhotonNetwork.Instantiate` для BombNPC |
| P0 | `BombNPCSpawner.cs` | Добавить `if (!PhotonNetwork.IsMasterClient) return` |
| P1 | `Character.cs:254` | Задержать `SetActive(false)` или не деактивировать — только отключить управление |
| P1 | `GameManager.cs:73` | Добавить `?.` к `_pausePanel.SetActive` |
| P1 | `Character.cs:146` | Добавить `CancelInvoke(nameof(StopPushAnim))` в `RpcDie()` |
| P2 | `BombNPC.cs:105` | Вынести `FindWithTag` из `Update` в `OnLanded()` |
| P2 | `BombNPC.cs:86` | Добавить `.SetLink(gameObject)` к бесконечным твинам |
| P2 | `Board.cs:37` | Убрать первый `ApplyGravity()` из `Lock()` |
| P3 | `Board.cs:164` | Заменить `Vector2.Distance` на `sqrMagnitude` в `Explode()` |
| P3 | `TetrisController.cs:188` | Предкэшировать буфер для `Shifted()` |
| P3 | Все файлы | Кэшировать `Camera.main` в `Awake()` |
| P3 | `Board.cs:91` | Удалить мёртвый `ApplyGravityAnimated()` или начать использовать |
