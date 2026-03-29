# Crisis Communication Plan

**Purpose**: Template for responding to outages, game-breaking bugs, controversial balance changes, and other urgent issues
**Owner**: Community Manager + Producer (joint decision on messaging)
**Priority**: Acknowledge within 30 minutes, update every 30-60 minutes until resolved

---

## Types of Crises & Response Paths

### Tier 1: Game-Breaking Bug (Immediate)

**Definition**: Bug that makes the game unplayable or causes data loss
- Game crashes on startup
- Character falls through floor (respawn impossible)
- Multiplayer desync (client/server positions differ severely)
- Progress lost after session (player levels/unlocks reset)
- Exploit causing infinite wins (breaks matchmaking rating)

**Response Timeline**:
- **T+0 (Detect)**: QA/engineer reports to producer + community manager
- **T+30min**: Acknowledge on itch.io, Discord, TikTok
- **T+60min**: First status update (investigating or estimated fix time)
- **T+120min**: Next update (if still unresolved)
- **T+Resolution**: Announce fix, publish patch, post-mortem blog

**Sample First Message** (itch.io + Discord):

```
🚨 URGENT: [Bug Name]

We're aware that [describe impact in player terms: "games crash on startup" / "progress is being lost"].

Status: INVESTIGATING (current ETA: [time or "unknown"])

What we're doing: [Quick 1-sentence of technical fix direction, if safe to share]

Updates every 30 minutes. Thank you for your patience.

- Community Team
```

**Sample Status Update**:

```
Status Update #2 (2:45 PM UTC+3)

We've isolated the issue to [vague technical detail]. Fix is being tested now.

Expected rollout: [time] (we'll notify immediately when live)

Sorry for the inconvenience. We're moving fast.

- Community Team
```

**Sample Resolution Message**:

```
✓ FIXED: [Bug Name]

The issue has been patched and is live now (Build 0.3.1).

What happened: [brief technical explanation]
Why it happened: [root cause in plain English]
What we're doing to prevent this: [process change, code review, etc.]

Download the latest build. Restart your game if it was running.

Thank you for reporting this and for your patience.

- Community Team
```

---

### Tier 2: Service Outage (Server Down)

**Definition**: Multiplayer matchmaking, lobby, or networking unavailable
- Players cannot join matches
- Lobbies crash when starting
- Frequent disconnects during gameplay
- Matchmaking stuck on "searching"

**Response Timeline**:
- **T+0**: Detect outage (from player reports or monitoring)
- **T+15min**: Acknowledge
- **T+45min+**: Status updates every 30-45 minutes
- **T+Resolution**: "Service Restored" post + optional post-mortem

**Sample First Message**:

```
⚠️ SERVICE OUTAGE: Multiplayer Issues

We're seeing reports that matchmaking is failing / lobbies aren't connecting.

Status: INVESTIGATING our servers now

ETA: ~[time] (will update at [specific time])

Workaround: Single-player vs AI mode still works. Local co-op works.

- Community Team
```

**Sample Status Update** (after 30 min):

```
Status Update: We found the issue. Our [server component] failed at [time].

Deploying fix now. Expected restoration: [20-30 min from now]

Live players: You may see disconnects as we restart. We're working fast.

- Community Team
```

**Sample Resolution**:

```
✓ SERVICE RESTORED

Matchmaking is back online. [specific time] UTC+3.

What happened: [brief explanation of technical failure]
Impact: ~[X] matches affected / [Y] players impacted
Duration: [how long it was down]

We'll post a full post-mortem blog tomorrow explaining:
- Root cause
- Why our failsafes didn't catch it
- Changes to prevent recurrence

Thank you for your patience.

- Community Team
```

---

### Tier 3: Controversial Balance Change (Medium Urgency)

**Definition**: Player outcry over a patch that feels unfair or poorly explained
- Ability nerfed too hard (win rate drops 20%)
- Blocker spawn rate too fast (feels impossible for Dodger)
- Change wasn't in patch notes (surprise nerf)
- Balance change contradicts previous messaging

**Response Timeline**:
- **T+0 (Patch goes live)**: Monitor Discord/itch comments
- **T+2 hours**: Respond to top 3-5 critical comments (be visible)
- **T+12 hours**: Dev blog explaining rationale + data
- **T+24-48 hours**: Announce hotfix ETA if needed (or defend the change with data)

**Sample First Response** (to comment asking "why nerf this?"):

```
Great question! Here's the thinking:

[Brief player-facing explanation of balance philosophy]

The numbers: [ability X had 65% win rate vs 50/50 target, so we reduced Y by Z]

We hear you that it might've been too aggressive. Watching playtesting data closely and can hotfix if needed.

What are you seeing in actual matches? Your feedback shapes the next patch.

- Community Team
```

**Sample Dev Blog**:

```
# Dev Blog: Block Spawn Rate Increase — Here's Why

In Build 0.3, we increased Block Spawn from 0.8s to 0.7s.

If you've played, you've noticed: games feel more chaotic. Dodgers are dying faster.

**Why we did this:**

Our data showed:
- Average match length: 4 min 45 sec (too long, players get bored)
- Blocker win rate: 48% (should be 50%, Blockers felt weak)
- Most matches ended by timeout (player disconnection), not skill-based win

By increasing spawn rate:
- Matches end in 3-4 min (tighter, more exciting)
- Blocker win rate moved to 51% (within acceptable variance)
- More clear winners (skill-based outcomes, not RNG)

**The Trade-off:**

New players feel the difficulty spike. Dodgers with poor reflexes now lose faster.

This is intentional. If you're new, try Survival Mode (practice against easy AI) to learn timing.

**What if it's too hard?**

We're monitoring. If Blocker win rate hits 55%+, we'll tune to 0.75s.

Your feedback matters. Play a few matches and tell us what you think.

- Game Designer & Community Team
```

**Sample Response to "This is broken, revert it now"**:

```
We hear the frustration. Faster spawn rate is a big change.

But reverting would mean Blockers go back to 48% win rate—which isn't fair either.

Here's what we're doing:
1. Monitoring win rates daily (if they spike above 55%, we hotfix)
2. Adding practice modes so new players can adjust
3. Next patch (1 week) includes a separate "Easy Mode" option

Give it 48 hours. If you're still frustrated, let's talk specifics about what felt unfair.

- Community Team
```

---

### Tier 4: Community Drama / Toxicity Incident

**Definition**: Moderation incident, streamer/creator conflict, or toxic player behavior going viral
- Popular streamer harassed a new player (caught on clip)
- Moderator action (ban/mute) perceived as unfair
- Community splitting over rule interpretation
- Hateful content posted by player with large following

**Response Timeline**:
- **T+0**: Assess severity (is it just drama, or affecting game health?)
- **T+1-2 hours**: Private outreach to affected parties
- **T+4-6 hours**: Public statement (if necessary)
- **T+24 hours**: Follow-up post with actions taken

**Sample Private Outreach** (DM to involved parties):

```
Hi [Player],

We saw the [incident]. We want to understand your side of the story.

What happened from your perspective? We're not here to assign blame—just to understand and help.

Respond here or email [community@ypikaeigames.com]

- Community Team
```

**Sample Public Statement** (if incident is public):

```
We're aware of the incident involving [vague reference to what happened].

We take community health seriously. Here's what we're doing:

1. We've reached out to all involved parties
2. We're reviewing our moderation actions to ensure fairness
3. We'll post a follow-up statement with what we found within 24 hours

Our Community Guidelines exist to protect everyone. If you see harassment, please report it.

In the meantime: Remember there's a person on the other side of the screen. Let's keep this community kind.

- Community Team
```

**Sample Follow-Up**:

```
Community Incident Follow-Up

Here's what happened:
[Neutral description of events. No blame-placing. Stick to facts.]

Here's what we did:
- Muted [player] for [days] (Violates Guideline: Harassment)
- Spoke with [other player] about [specific behavior]

Here's what we're changing:
- Moderator review process (added oversight for consistency)
- Clearer rules about [specific issue]

If you disagree with our decision, you can appeal to [email].

Thanks for keeping us honest. This community is stronger when we address issues head-on.

- Community Team
```

---

### Tier 5: Misstep / Accidental Problem (Lower Urgency)

**Definition**: Minor issue that causes embarrassment but isn't critical
- Patch notes have wrong numbers (say 0.7s, actually 0.8s)
- Cosmetic launches with a texture bug (not gameplay-breaking)
- Developer makes tone-deaf comment on stream
- Event announced but implementation delayed

**Response Timeline**:
- **T+1-4 hours**: Acknowledge (no need to rush, but don't ghost)
- **T+24 hours**: Explain + fix

**Sample Response**:

```
Oops, we goofed.

[Brief acknowledgment of mistake]

What we're fixing:
- [Fix 1]: Patch notes corrected (actual values were X, not Y)
- [Fix 2]: Cosmetic texture replaced (new version live in [time])
- [Communication]: We'll be clearer about [thing] next time

Sorry about the confusion. Thanks for catching it!

- Community Team
```

---

## General Crisis Communication Principles

### Do's
- **Acknowledge fast**: Even "we're looking into it" is better than silence
- **Be specific**: "login servers are down" not "we're experiencing issues"
- **Provide ETA**: Give a time, even if uncertain. "~3 PM UTC+3" or "30-60 min" is better than "ASAP"
- **Update regularly**: On schedule. If ETA slips, announce new ETA immediately
- **Take responsibility**: "We messed up" > "unfortunate circumstances"
- **Thank patience**: People are frustrated. Acknowledge that.
- **Explain what you're doing**: Transparency builds trust
- **Post-mortem**: After resolution, explain what happened + prevention steps

### Don'ts
- **Go silent**: Don't disappear for 4 hours even if you don't have a fix
- **Be vague**: "Issues with matchmaking" = bad. "Matchmaking is returning 500 errors" = good
- **Make excuses**: "The servers were old" sounds like blame-shifting
- **Over-promise**: Don't say "fix in 2 hours" if you mean "might be 2-4 hours"
- **Blame players**: Never say "players were exploiting a known bug"
- **Debate on Twitter**: Crisis comms happen on official channels (Discord, itch.io), not social media arguments
- **Hide bad news**: Players will find out. Better you tell them first

---

## Compensation for Crises

If players lose significant progress, time, or in-game currency due to YOUR bug:

**In-Game Compensation**:
- Game-breaking bug: 1 free cosmetic or 500 in-game currency
- Service outage (1+ hour): 200 in-game currency
- Data loss: Full restore + apology message

**Messaging**:

```
We're deeply sorry for [incident]. To apologize:

[Compensation details have been added to your account]

This doesn't fix the frustration, but we hope it shows we care.

What we're doing differently: [prevention steps]

Thank you for sticking with us.
```

---

## Tools & Channels

### Channels to Update Simultaneously

1. **itch.io**: Post in game page comments (pinned)
2. **Discord**: #announcements and #status channels
3. **TikTok**: Pinned video (if visual, like server status)
4. **Email**: If you have an email list
5. **In-game**: UI message (if you can push a hot-fix)

### Monitoring Tools

- **Discord**: Monitor #bug-reports, #general for player reports
- **itch.io Comments**: Check hourly during crises (sort by new)
- **Uptime Monitoring**: Pingdom or StatusPage (tracks uptime publicly)
- **Server Metrics**: Database of response times, error rates
- **Social Media**: TweetDeck or native TikTok analytics (search for game mentions)

---

## Post-Mortem Template

**Always publish after a crisis**, even minor ones.

```markdown
# Post-Mortem: [Crisis Name]

**Date**: [When it happened]
**Duration**: [How long it lasted]
**Impact**: ~[X] players affected, ~[Y] matches lost/affected

## What Happened

[Neutral description of the incident. No blame.]

Timeline:
- 2:15 PM: Players report matchmaking errors
- 2:30 PM: We acknowledge and start investigating
- 2:50 PM: Root cause found (server Y overloaded)
- 3:10 PM: Fix deployed and verified
- 3:15 PM: Service restored

## Root Cause

[Technical explanation in plain English]

We deployed [change], which caused [system] to [fail].

Our monitoring should have caught this, but [why it didn't].

## What We're Doing to Prevent This

1. **Code**: [Code review process change] to catch [issue type]
2. **Process**: [New monitoring alert] for [metric]
3. **Redundancy**: [Failover system] added so [system Y] doesn't take down [system X]
4. **Training**: Team reviewed [procedure] to prevent similar issues

## Lessons Learned

- We need better [thing]
- Players prefer [communication style] during outages
- Our [system] is a single point of failure (addressing in Q2)

## Apology & Compensation

We sincerely apologize. [Compensation added to accounts].

Thank you for your patience and for playing I vs Blocks.

- Community & Engineering Teams
```

---

## Crisis Escalation Flowchart

```
Crisis Detected
    ↓
[Severity Assessment]
    ├─ Game-breaking bug? → TIER 1 (30-min response)
    ├─ Server down? → TIER 2 (30-min response)
    ├─ Balance outcry? → TIER 3 (2-hour response + dev blog)
    ├─ Community drama? → TIER 4 (1-2 hour response + outreach)
    └─ Minor mistake? → TIER 5 (4-hour response)
    ↓
[Notify Producer + Community Manager]
    ↓
[Draft Messaging]
    ├─ Acknowledge statement
    ├─ Status update cadence
    ├─ Post-mortem plan (if applicable)
    └─ Compensation (if applicable)
    ↓
[Publish to All Channels Simultaneously]
    ├─ itch.io
    ├─ Discord
    ├─ TikTok
    ├─ Email (if list exists)
    └─ In-game (if possible)
    ↓
[Update Every 30-60 Minutes Until Resolution]
    ↓
[Post Resolution: Publish Post-Mortem Blog]
    ↓
[Review: What Did We Learn?]
    └─ Add preventative measures to product roadmap
```

---

## Template Email for Outreach

**To affected player(s)**:

```
Hi [Name],

We're aware of the [incident] you experienced.

I'm [Community Manager name], and I want to help.

Here's what happened: [brief explanation]

Here's what we did: [actions taken]

Here's what we want to do: [compensation / fix]

Can you reply with any additional context? We're trying to understand what happened from your perspective.

We really appreciate you reporting this. Players like you help us make I vs Blocks better.

- [Signature]
```

---

## Contact List (Fill in During Setup)

- **Producer**: [email] (approval on messaging)
- **QA Lead**: [email] (confirm bug severity)
- **Tech Lead**: [email] (server/networking issues)
- **Game Designer**: [email] (balance dispute escalations)
- **Community Manager**: [email] (owner of crisis comms)
- **Lawyer/Legal**: [email] (if data loss or compliance issue)
- **Executive**: [email] (if major reputation risk)

---

## Final Principle

**Transparency + Speed + Ownership = Community Trust**

Players don't expect perfection. They expect:
1. You acknowledge problems quickly
2. You're honest about what happened
3. You're working to fix it
4. You explain what you learned

Do those four things, and players forgive almost anything.
