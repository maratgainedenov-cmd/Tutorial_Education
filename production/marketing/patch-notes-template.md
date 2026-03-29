# Patch Notes Template

**Format**: Use for every release. Publish simultaneously in EN and RU.
**Location**: `production/releases/[version]/patch-notes.md`
**Audience**: Players (not developers) — explain what changed and why it matters to them

---

## Template Structure

```markdown
# Patch Notes: Build [X.X]

**Release Date**: [Date]
**Build Version**: [Steam/itch.io build number]
**Estimated Playtime**: [if applicable, e.g., "1-5 min per round"]

---

## Headline: [Most Exciting or Important Change]

[1-2 sentence pitch. Why should players care? What changed fundamentally?]

Example: "Destroy Side now applies 1-second slowdown to the Dodger — giving Blockers more control over rhythm and Dodgers a chance to react."

---

## New Content

- **[Feature Name]**: [Description in player language]
  - Example: "Character Armor ability now grants 2 seconds of invulnerability + visual aura (instead of 30% damage reduction)"

- **[New Map / Mode / Cosmetic]**: [Why it's cool]

---

## Gameplay Changes

### Blocker Changes
- **Block Spawn Rate**: 0.8s → 0.7s (increases difficulty mid-game)
  - Why: Dodgers were surviving too long; Blockers needed more block volume
  - What you'll notice: Games feel more chaotic, but Dodger abilities now matter more

- **Destroy Side Cooldown**: 1.5s → 1.0s
  - Why: Destroy Side felt clunky; faster feedback loop = more fun
  - What you'll notice: You can chain destruction effects more often

### Dodger Changes
- **Dash Ability**: Now resets if you land on a platform
  - Why: Gave Dodgers more mobility options for high-skill plays
  - What you'll notice: Expert players can chain dashes; new players have better escape options

- **Character Speed**: 5.0 → 5.5 units/sec
  - Why: Dodgers felt slow relative to faster block spawn rate
  - What you'll notice: Closer matches, less frustration

---

## Bug Fixes

### Gameplay
- Fixed: Destroy Down damage not applying if block landed on character simultaneously ✓
- Fixed: Push ability knocking blocks outside playfield (now stops at boundary) ✓
- Fixed: Character falling through floor on game start (rare edge case) ✓

### UI
- Fixed: Cooldown timer UI not updating for Destroy Side ✓
- Fixed: Ability icons not highlighting on selection ✓
- Fixed: Spectator mode showing wrong player names ✓

### Networking
- Fixed: Rare desync where block position differed between clients ✓
- Fixed: Occasional lag spike when joining a match (reduced network message volume) ✓

### Localization
- Fixed: Russian text overflow in ability tooltips ✓
- Fixed: "Destroy Side" translation inconsistency (now "Разрушение Сбоку" across UI) ✓

---

## Known Issues (Tracked, Working On)

- **Character can get stuck briefly after landing on block** (rare, no impact on gameplay)
  - Workaround: Press jump twice to unstick
  - Fix ETA: Next patch (code refactor in progress)

- **Colorblind mode missing** (planned feature, not yet released)
  - We hear you! High on our roadmap for Build [X.X+1]

- **Russian subtitles lag in lore events** (only affects first 30 seconds)
  - Cause: Subtitle sync issue with video playback
  - Fix ETA: Patch [X.X+1]

---

## Balance Rationale (Developer Commentary)

**Block Spawn Rate Increase**

We noticed in playtesting that matches were going 4-5 minutes without clear winners. Blockers felt like they lacked agency—landing blocks felt good, but destruction didn't create urgency for Dodgers.

By increasing spawn rate from 0.8s to 0.7s, we've tightened the skill gap:
- Expert Blockers can now execute combos faster
- Dodgers with low reactivity now lose, as intended
- Ability timing becomes more critical (dash windows are smaller)

If this feels too hard, we'll tune to 0.75s in the next patch. Your feedback determines the future balance.

**Why Destroy Side Cooldown Decreased**

Destroyer Side felt like it was on "cooldown jail"—you'd land blocks, wait 1.5 seconds, and then destroy. The mechanic felt disconnected.

Lowering to 1.0s means:
- You get 3 destructions per 3 seconds (satisfying rhythm)
- Dodgers have tighter windows to escape (more skill expression)
- Experienced Blockers can chain effects (combo potential)

This is a **pure fun buff**. We're watching win rates carefully.

---

## Statistics (Optional, Share with Community)

- **Match Completion Rate**: 92% (up from 88% last patch)
  - Players are staying longer → gameplay is more engaging

- **Average Match Length**: 4m 12s (up from 3m 45s)
  - Slightly longer matches = more exciting comebacks

- **Ability Usage**: Armor used in 68% of matches (up from 52%)
  - Players are discovering the ability, which is good

- **Win Distribution**: Blocker wins 53%, Dodger wins 47%
  - Target is 50/50; slight Blocker advantage is acceptable, monitoring

---

## What's Coming Next

**In Build [X.X+1] (2 weeks)**
- [ ] Colorblind mode (high priority based on community feedback)
- [ ] New character skin "Survivor" (cosmetic)
- [ ] Performance optimization (reduced memory footprint)

**In Build [X.X+2] (4-6 weeks)**
- [ ] New map "Factory" (with moving hazards)
- [ ] Ranked matchmaking system
- [ ] Better spectator UI (watch pro matches)

**Longer Term (Months 2-3)**
- [ ] Seasonal cosmetics + battle pass
- [ ] Tournament mode (custom lobbies)
- [ ] Lore campaign (narrative progression)

---

## How to Report Issues

- **Discord**: [link]
- **Itch.io Comments**: Reply directly (we read every comment)
- **GitHub Issues**: [link, if applicable]

Your feedback shapes every patch. Thank you for playing.

---

## Credits

**Lead Designer**: [Name]
**Balance & QA**: [Name]
**Community Manager**: [Name]

---

## Patch History

| Build | Release Date | Major Change |
|-------|--------------|--------------|
| 0.3 | [Date] | Block spawn rate increase, Destroy Side cooldown decrease |
| 0.2 | [Date] | Character abilities system, multiplayer lobby |
| 0.1 | [Date] | Initial release, core gameplay |
```

---

## Publishing Checklist

Before publishing any patch notes:

- [ ] Numbers match the actual code (verify with developer)
- [ ] Balance rationale makes sense to non-developers
- [ ] Known issues are honest (don't hide problems)
- [ ] Tone is warm and community-focused
- [ ] Both EN and RU versions are complete and reviewed
- [ ] Publish simultaneously across all channels:
  - [ ] itch.io post
  - [ ] Discord announcement
  - [ ] TikTok teaser (short clip of new feature)
  - [ ] Dev blog (if significant patch)
- [ ] Pin the post for 24 hours (so players don't miss it)

---

## Example Patch Note (Concrete)

See `/examples/patch-notes-v0.3.md` for a real example based on actual game state.
