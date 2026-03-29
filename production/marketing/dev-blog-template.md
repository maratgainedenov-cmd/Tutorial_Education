# Dev Blog Template & Editorial Calendar

**Location**: `production/releases/[version]/dev-blog-[date].md`
**Cadence**: Weekly during active development (Monday or Thursday)
**Audience**: Players interested in behind-the-scenes, balance philosophy, upcoming features
**Language**: English primary, Russian translation within 48 hours

---

## Blog Post Structure

### Header
```markdown
# Dev Blog: [Title — make it intriguing]

**Published**: [Date]
**Read Time**: [5 min / 10 min / etc.]
**Author**: [Name, role]
**Topics**: #GameDesign #Development #Balance #Community
```

### Introduction (1 paragraph, 50-80 words)
Hook the reader. Why should they read this?
- Personal: "We almost didn't ship this feature"
- Curious: "Here's a design decision that surprised us"
- Urgent: "This balance change is controversial. Here's why we did it."
- Fun: "We broke our game in the weirdest way"

**Example**:
"Last week, we nerfed Block Spawn Rate and players lost their minds. And honestly? We expected it. Here's what data told us, why we made the call, and what we're watching for in the next patch."

---

### Body Sections (Choose 1-3, keep total 800-1500 words)

#### Option A: Balance Philosophy
**Use when**: Explaining a controversial change or tuning philosophy
**Structure**:
1. The change: "We did X"
2. Why: "Here's the data / feedback that drove it"
3. The tradeoff: "This helps [player type] but hurts [player type]"
4. What we're watching: "If [metric] spikes, we'll hotfix"

**Example**:

```markdown
## Why We Increased Block Spawn Rate

In Build 0.3, we changed Block Spawn from 0.8s → 0.7s.

### The Numbers
- **Before**: 48% Blocker win rate, 4m 45s avg match length, 68% player retention (Day 1 → Day 7)
- **After**: 51% Blocker win rate, 3m 50s avg match length, monitoring retention now

We wanted Blockers at 50% win rate (that's balance). The old 48% meant Dodgers had too many escape tools—armor, speed boost, push—and nothing challenged them until late game.

### The Design Philosophy

In I vs Blocks, Blockers control rhythm. Dodgers control reaction. The spawn rate is the metronome.

A slower metronome (0.8s) = Dodgers have time to breathe, explore the board, set up combos.
A faster metronome (0.7s) = Blockers are aggressive, no room for mistakes.

We're testing: does faster = more exciting?

Early data says yes. Matches end with clear skill winners, not timeouts.

### The Trade-off

**Who loves this**: Expert Blockers (finally feel powerful), speedrunners (tighter pacing)

**Who hates this**: Casual players, new Dodgers (difficulty spike is real)

This is a balance shift, not a bug fix. We're intentionally raising the skill floor for Dodgers.

### What's Next?

We're monitoring these metrics daily:
- Blocker win rate (if it hits 55%+, we hotfix to 0.75s)
- New player retention (if D1 → D7 retention drops below 50%, we add Easy Mode)
- Playstyle diversity (are Blockers using combos, or just spamming?)

We may be wrong. That's OK. Data will tell us.
```

---

#### Option B: Feature Deep-Dive
**Use when**: Explaining new feature, ability, or mechanic
**Structure**:
1. The idea: "We added X because..."
2. How it works: Simple example
3. Why players care: What does it enable?
4. Behind-the-scenes: How hard was it to build?

**Example**:

```markdown
## Inside the Armor Ability: A Deep-Dive

In Build 0.2, we added the Armor ability. Dodgers can now activate it to block 2 hits of damage.

### Why We Built It

Early playtesting showed Dodgers felt helpless. A block falls on you. You die. Repeat.

Armor changes that: now Dodger has agency. "I'm going to tank this hit and survive."

It's a skill-check: do you have enough HP? Can you reach a health pickup after?

### How It Works (Player-Facing)

1. Armor is unlocked after 5 wins
2. Press [F] (or button) to activate
3. You glow blue and take reduced damage for 2 seconds
4. Each hit reduces armor value by 1 (so: 2 hits to break it)
5. Cooldown: 10 seconds

Simple, right?

### Why Designers Care

Armor is a **moment-to-moment decision**. Do you:
- Pop it now (guaranteed survival, risky later)?
- Save it for the next combo (greedy)?
- Let it expire unused (wasted resource)?

Good game design gives players meaningful choices. Armor is one of them.

### Behind-the-Scenes: Why This Was Hard

Animation: Dodger needed a visual "glow" effect that reads instantly (1 frame). We tested 5 shader approaches before settling on a simple color shift (fastest, clearest).

Networking: If Armor activates on Client A but Server disagrees, you get desync. We added validation checks and tested latency up to 300ms.

Balance: 2 seconds is a long time. We tested: 1 sec (too weak), 1.5 sec (better), 2 sec (good). Playtesting settled it.

### What's Coming

- Armor+ ability (3 hits instead of 2, higher cooldown)
- Visual feedback when armor breaks (screenshake, sound cue)
- Armor stacking (controversial, we're prototyping)

We're excited about this system. More survival tools = more creative gameplay.
```

---

#### Option C: Behind-the-Scenes / Postmortem
**Use when**: Sharing team update, bug discovery, creative process
**Structure**:
1. The story: What happened?
2. The challenge: What made it hard?
3. The solution: How did we fix it?
4. What we learned: Broader lesson?

**Example**:

```markdown
## That Time We Broke Multiplayer (And Fixed It)

Last Friday at 4:47 PM, Build 0.2 went live.

By 5:15 PM, we had 12 reports: "Game crashes when joining a match with 3+ players."

Here's what went wrong, and why it matters.

### The Bug

When a 4th player joined a room in Photon, the Client code asked the Server for old block positions. The Server tried to send 2000 physics updates at once.

Result: Network packet too large → deserialization failed → crash.

### Why We Didn't Catch It

Our testing was on LAN (local network, no bandwidth limit). A 4-player match on LAN is instant.

But on public internet, the lag meant all 2000 updates piled up in a single frame. Boom.

### The Fix

**Quick hotfix** (5 PM): Cap historical updates to last 10 seconds (instead of entire game).
Benefit: 90% smaller packets.
Downside: New players miss a few old blocks, might get confused.

**Long-term fix** (in progress): Send updates incrementally (100 updates per frame, spread across 20 frames).
Benefit: Complete data, no cutoff.
Downside: More complex code, took 3 hours to code + test.

We deployed the long-term fix in Build 0.2.1. All reports stopped.

### What We Learned

1. **Test at scale**: Simulation matters. Can't replicate 300ms latency on LAN. We now use Clumsy (latency simulator) in every build test.

2. **Preload vs. Lazy Load**: Our architecture loaded entire history. Better: lazy load what's needed. We're refactoring networking layer.

3. **Player Patience**: Some players waited and reported. Others raged and left. We need to ship a "Loading..." UI to show players what's happening.

### Thanks

Special thanks to players who reported this. You're the QA team we didn't have.

Next build: All new players will see "Syncing game state..." to know something's happening.
```

---

#### Option D: Design Philosophy / Narrative
**Use when**: Explaining the game's identity, themes, or creative vision
**Structure**:
1. The question: "Why did we design the game this way?"
2. The answer: Design philosophy + examples
3. Why it matters: How does it affect players?
4. What's next: Where's this philosophy going?

**Example**:

```markdown
## Dark Comedy as Survival Tool: Our Design Philosophy

I vs Blocks has a dark sense of humor. You might've noticed.

The Blocker's goal is to crush the Dodger. The Dodger's job is to survive impossible odds while sarcastic narrator comments on their failure.

That's not accident. Here's why.

### Why Dark Comedy?

Puzzle games stress players. Losing feels bad. Our job is to make the loss *funny* so the sting softens.

When you fail, you hear: "Well, that was pathetic." (You laugh.)
When you succeed, you hear: "Somehow, you survived." (You feel clever.)

The humor is a release valve. It says: "It's OK to lose. Tomorrow you'll be better."

### Examples in the Game

- **Lore events**: Narrator explains the Dodger's "why" in darkly comedic way ("You stole a sandwich. This is your consequence.")
- **Victory screen**: Both players see funny reactions based on how close the match was
- **Cosmetics**: A few skins are joke items (Dodger in formal wear dying hilariously)

### Why This Matters

In competitive games, toxicity breeds quickly. If losing feels *only* bad, players rage.

But if losing is funny? Players queue up again.

Our data shows: games with humor have 40% higher retention. Not because the game is easier. Because failure is less painful.

### Where We're Going

We're planning a **narrative campaign** (Months 3-4) that leans hard into dark comedy:
- Dodger is a reluctant survivor
- Blocker is an incompetent villain
- Narrator is a sarcastic omniscient observer

The gameplay is serious (skill-based, balanced). The story is silly. That contrast is what makes both better.
```

---

#### Option E: Community Spotlight
**Use when**: Highlighting player achievements, feedback impact, creative contributions
**Structure**:
1. The player/moment: Who did what?
2. Why it's cool: What made it special?
3. Impact: Did it change the game?
4. Call-to-action: Encourage others to contribute

**Example**:

```markdown
## Community Spotlight: How Player Feedback Shaped Block Spawn Rate

This week, we're celebrating the feedback that led directly to balance changes.

### The Moment

During Week 2 of beta testing, player **ShadowNinja42** posted: "Games end by timeout 70% of the time. Something's off."

They didn't just complain. They included:
- Play session data (5 matches, avg 4m 42s each)
- Observations ("I never feel threatened until the last minute")
- A hypothesis ("Block Spawn might be too slow")

### Why This Matters

ShadowNinja42 did our job for us. They identified, quantified, and hypothesized.

We checked our data. They were right. Block Spawn was a bottleneck.

### The Result

In Build 0.3, we tuned Block Spawn from 0.8s → 0.7s.

Impact: Games now end in 3m 50s (60% faster). Blocker win rate moved from 48% → 51%. Player retention improved.

This change came from a community member caring enough to write a thoughtful post.

### A Reminder

If you see something wrong, SAY SOMETHING. You might just shape the next build.

Not all feedback leads to changes (we can't make everyone happy). But thoughtful feedback always gets read and discussed in the dev room.

Thank you, ShadowNinja42. And thank you to every player who writes bug reports and balance suggestions.

You're building this game with us.

### How to Share Feedback

- Discord: #feedback channel
- Itch.io: Comments (we read every thread)
- Email: [feedback@ypikaeigames.com]

We read it all.
```

---

#### Option F: Technical Deep-Dive (Audience: Developers, Nerdy Players)
**Use when**: Sharing technical challenges, architecture decisions, or optimization work
**Structure**:
1. The problem: What was broken or slow?
2. Why it's hard: What made it non-obvious?
3. The solution: What did we build?
4. The result: What improved?

**Example**:

```markdown
## Reducing Network Bandwidth by 60% (Without Breaking Anything)

In Build 0.2, our multiplayer had a bandwidth problem.

Every frame, we sent:
- Block positions (4 blocks × 3 floats = 12 floats)
- Character position & rotation (3 floats)
- Ability cooldowns (2 floats)
- ~100 bytes of metadata

Per frame, at 60 FPS, that's ~72 KB/sec per player.

With 2 players, that's 144 KB/sec. On a 1 Mbps connection, that's already 11% of bandwidth.

Problem: Mobile players on 4G = lag city.

### The Challenge

We couldn't just compress. We needed to be smart about what to send.

Classic networking problem: **send only what changed.**

### The Solution

**Quantization**: Block positions don't need 32-bit floats (sub-millimeter precision). Use 16-bit values instead (cm precision, good enough).

**Dirty Flagging**: Only send block position if it moved. Same for character rotation.

**Prediction**: Use physics to predict next position locally. Only send corrections every 10 frames instead of 60.

**Cull**: Don't send data for offscreen elements (if block is outside camera, skip it).

### The Result

- Bandwidth: 144 KB/sec → 57 KB/sec (60% reduction)
- Latency: Prediction reduced perceived lag (smoother feeling)
- Mobile: 4G players report "playable" instead of "laggy"
- CPU: Less network processing = more frame budget elsewhere

Tradeoff: If prediction is wrong (player dodges unexpectedly), there's a tiny desync. We're OK with that for smooth gameplay.

### Lessons

1. **Premature optimization is evil. Measure first.** We had no idea we were wasteful until we profiled.
2. **Small numbers add up.** 100 bytes × 60 FPS × 2 players = HUGE.
3. **Domain-specific optimization beats generic compression.** Knowing our data (block positions don't need precision) was key.

Next: Mobile optimization (already better, but we want sub-50 KB/sec).
```

---

### Conclusion (1 paragraph, 50-100 words)
Wrap up and call-to-action:
- "Try the new build and tell us what you think"
- "This is version [X]. Next week: [teaser]"
- "Questions? Comment below or join our Discord"
- "Thank you for playing I vs Blocks"

**Example**:
"Build 0.3 is live now. Jump in and tell us how the spawn rate feels. If you hate it, we hear you—send feedback and data, and we'll tune again. That's how we balance together. See you on the board."

---

## Editorial Calendar (Sample)

| Week | Topic | Author | Purpose |
|------|-------|--------|---------|
| 1 | Launch Announcement + Game Overview | Community Manager | Set expectations, explain game |
| 2 | Balance Philosophy (Block Spawn) | Game Designer | Explain controversial change |
| 3 | Community Spotlight (ShadowNinja42) | Community Manager | Celebrate feedback impact |
| 4 | Feature Deep-Dive (Armor Ability) | Designer | Explain new mechanic |
| 5 | Multiplayer Networking Postmortem | Tech Lead | Behind-the-scenes tech story |
| 6 | Roadmap Update + Q2 Plans | Producer | Transparency on upcoming features |
| 7 | Dark Comedy Design Philosophy | Narrative Director | Explain game's identity |
| 8 | Player Speedrun Highlight | Community Manager | Celebrate skilled player |

**Pattern**: 1 balance explanation → 1 community/fun post → 1 technical/creative post → repeat

---

## Publishing Checklist

- [ ] Draft written and spell-checked (EN)
- [ ] Reviewed by producer (tone, messaging accuracy)
- [ ] Reviewed by relevant domain expert (designer for balance, engineer for tech)
- [ ] Examples match actual game state (verify numbers with code)
- [ ] Tone is warm and community-focused (not corporate, not defensive)
- [ ] Conclusion has clear CTA
- [ ] Russian translation complete and reviewed (within 48 hours of publish)
- [ ] Publish simultaneously:
  - [ ] itch.io devlog section
  - [ ] Discord #announcements
  - [ ] TikTok teaser clip (15-30 sec, key point + link)
  - [ ] Email list (if applicable)
- [ ] Pin for 24 hours (so players don't miss it)
- [ ] Monitor comments for 48 hours (respond to questions)

---

## Tone Guidelines

### Do's
- First person ("we noticed", "we tested")
- Honest about uncertainties ("we might be wrong")
- Playful language (match game's dark humor)
- Data-driven explanations (show your work)
- Thank players for feedback
- Admit mistakes when they happen

### Don'ts
- Corporate jargon ("synergize", "optimize", "leverage")
- Defensiveness ("we're right, you're wrong")
- Empty hype ("exciting new feature coming soon!")
- Promises you can't keep ("guaranteed fun")
- Blame players ("you weren't using this correctly")

---

## Length Guidelines

| Type | Word Count | Read Time |
|------|-----------|-----------|
| Balance explanation | 800-1200 | 5-7 min |
| Feature deep-dive | 1000-1500 | 7-10 min |
| Postmortem | 1200-1800 | 8-12 min |
| Community spotlight | 600-800 | 4-5 min |
| Roadmap update | 400-600 | 3-4 min |

**Golden rule**: Every sentence should teach something. Cut fluff.

---

## Examples

See `/production/releases/` for actual published dev blogs from past builds.

---

## FAQ

**Q: What if I don't have a topic?**
A: Emergency options:
- Community spotlight (always works, builds goodwill)
- Upcoming features teaser (gets players excited)
- Stats post ("here's what you played last week")
- Q&A roundup (answer top Discord questions)

**Q: Should every blog have a call-to-action?**
A: Yes, but subtle. "Jump in and tell us what you think" is better than "DOWNLOAD NOW!"

**Q: What if the balance change is unpopular?**
A: Write the blog anyway. Transparency builds trust even (especially) when explaining hard decisions.

**Q: How long should translation take?**
A: English → Russian: ~4-6 hours (includes review). Plan ahead.

**Q: Can we publish a dev blog without a new build?**
A: Yes! Blogs about upcoming features, team updates, or philosophy are great even without patch releases.
