# Карта агентов — кто с кем работает

---

## Иерархия

### Креатив
- **creative-director** — арбитр всех дизайн-конфликтов
  - **art-director** → technical-artist
  - **game-designer** → systems-designer, level-designer, economy-designer, ux-designer
  - **narrative-director** → writer, world-builder

### Техника
- **technical-director** — арбитр всех технических конфликтов
  - **lead-programmer** → gameplay-programmer, engine-programmer, ui-programmer, network-programmer, ai-programmer, tools-programmer
  - **unity-specialist** — консультирует всех программистов

### Продакшн
- **producer** — координирует всех, следит за сроками
  - **qa-lead** → qa-tester
  - **release-manager** → devops-engineer
  - **community-manager** → (контент и коммуникация)
  - **analytics-engineer** — данные и метрики

---

## Горизонтальные связи

| Кто придумывает | → | Кто реализует |
|---|---|---|
| game-designer | → | gameplay-programmer |
| systems-designer | → | economy-designer |
| ux-designer | → | ui-programmer |
| art-director | → | technical-artist |
| audio-director | → | sound-designer |
| performance-analyst | → | engine-programmer |
| network-programmer | → | lead-programmer |
| qa-lead | → | qa-tester |
| community-manager | → | producer |

---

## Кто активен по фазам

| Фаза | Агенты |
|---|---|
| **Pre-production** | creative-director, game-designer, systems-designer, art-director, technical-director, lead-programmer, producer, prototyper |
| **Production** | gameplay-programmer, ui-programmer, network-programmer, technical-artist, sound-designer, qa-tester, unity-specialist |
| **Polish** | performance-analyst, engine-programmer, technical-artist, qa-lead, qa-tester, accessibility-specialist, localization-lead |
| **Release** | producer, qa-lead, release-manager, devops-engineer, community-manager |
| **Post-launch** | community-manager, live-ops-designer, analytics-engineer, producer |

---

## Кто решает конфликт

| Конфликт | Кто решает |
|---|---|
| Дизайн vs Дизайн | creative-director |
| Техника vs Техника | technical-director |
| Скоуп vs Время | producer |
| Арт vs Техника | art-director + technical-director |
| Дизайн vs Техника | creative-director + technical-director |

---

## Связки для частых задач

| Задача | Связка агентов |
|---|---|
| Новая механика | game-designer → systems-designer → gameplay-programmer → qa-tester |
| Баг | lead-programmer → qa-tester |
| UI экран | ux-designer → art-director → ui-programmer |
| Мультиплеер | network-programmer → lead-programmer → qa-tester |
| Оптимизация | performance-analyst → engine-programmer |
| Маркетинг | community-manager → producer |
| Релиз | producer → qa-lead → release-manager → devops-engineer → community-manager |
| Нарратив | narrative-director → writer → world-builder |
