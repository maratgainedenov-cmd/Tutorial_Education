# Session State

**Last updated:** 2026-03-29

<!-- STATUS -->
Epic: Multiplayer Sync
Feature: Role-based logic fixes
Task: Bug fixes complete, awaiting verification
<!-- /STATUS -->

## Current Task

Сессия 2026-03-29: исправлены 5 критических багов сетевой синхронизации (роль игрока, preview, позиция фигуры).

## Completed

- [x] design/gdd/bloks-vs-character.md -- полный GDD (12 разделов + приложения A и B)
- [x] BugFix: NextPiecePreview -- замена IsMasterClient на GameManager.IsTetrisPlayer()
- [x] BugFix: TetrisController -- замена IsMasterClient на GameManager.IsTetrisPlayer() в Update/SpawnAtColumn/SpawnBombNpcAt
- [x] BugFix: Персонаж не видел падающую фигуру -- добавлен InitForViewing(), BroadcastPiecePositions(), RpcUpdatePiece
- [x] BugFix: Разные фигуры в NextPiecePreview -- добавлен SetNextType() + RpcSyncNext
- [x] BugFix: GameManager.IsTetrisPlayer() -- централизованная проверка роли через room properties

## Key Decisions

- Все числа взяты напрямую из кода (не придуманы)
- Destroy Side и Destroy Down имеют РАЗДЕЛЬНЫЕ кулдауны (подтверждено кодом: _destroySideTimer и _destroyDownTimer)
- BombNPC управляется только MasterClient
- Exit в (0,2) помечен как критический дисбаланс D1 -- требует изменения при плейтесте
- Автономный BombNPCSpawner удален из текущей кодовой базы
- **2026-03-29**: IsMasterClient больше НЕ используется для определения роли игрока. Вместо этого -- GameManager.IsTetrisPlayer(), основанный на room properties Photon.

## Files Modified This Session (2026-03-29)

- Assets/Scripts/NextPiecePreview.cs (IsMasterClient -> IsTetrisPlayer)
- Assets/Scripts/TetrisController.cs (IsMasterClient -> IsTetrisPlayer, добавлены BroadcastPiecePositions/RpcUpdatePiece/InitForViewing)
- Assets/Scripts/GameManager.cs (добавлен IsTetrisPlayer(), InitForViewing integration)
- Assets/Scripts/TetrominoSpawner.cs (добавлен SetNextType(), RpcSyncNext)

## Open Questions

- Push BombNPC работает ли после приземления? (TryPush не проверяет _isLanded явно)
- Как именно умирает P2 от прямого раздавливания блоком? (через KillZone или Physics2D контакт?)
- Финальная позиция Exit -- нужен плейтест для определения оптимальной y
- Планируется ли UI для кулдаун-индикаторов? (код свойства CooldownNormalized уже есть)
- Баги из BUG_REPORT_ANALYSIS.md (дублирование Character, matchmaking, десинхронизация блоков) -- какие из них решены текущими фиксами?
