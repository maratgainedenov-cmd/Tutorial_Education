# Session State

**Last updated:** 2026-03-25

<!-- STATUS -->
Epic: Game Design Document
Feature: BLOKS vs CHARACTER GDD
Task: Complete
<!-- /STATUS -->

## Current Task

GDD "BLOKS vs CHARACTER" — написан и записан в файл.

## Completed

- [x] design/gdd/bloks-vs-character.md — полный GDD (12 разделов + приложения A и B)

## Key Decisions

- Все числа взяты напрямую из кода (не придуманы)
- Destroy Side и Destroy Down имеют РАЗДЕЛЬНЫЕ кулдауны (подтверждено кодом: _destroySideTimer и _destroyDownTimer)
- BombNPC управляется только MasterClient
- Exit в (0,2) помечен как критический дисбаланс D1 — требует изменения при плейтесте
- Автономный BombNPCSpawner удалён из текущей кодовой базы

## Files Modified This Session

- design/gdd/bloks-vs-character.md (создан)
- production/session-state/active.md (обновлён)

## Open Questions

- Push BombNPC работает ли после приземления? (TryPush не проверяет _isLanded явно)
- Как именно умирает P2 от прямого раздавливания блоком? (через KillZone или Physics2D контакт?)
- Финальная позиция Exit — нужен плейтест для определения оптимальной y
- Планируется ли UI для кулдаун-индикаторов? (код свойства CooldownNormalized уже есть)
