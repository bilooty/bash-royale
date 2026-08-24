# Bash Royale
 
A real-time multiplayer tower-defence battler, rendered entirely in ASCII, running on a
deterministic lockstep simulation over UDP and built entirely using C#.
 
Two players connect directly, each builds an eight-card deck, and both machines simulate
the *entire* match independently, units, projectiles, pathfinding and damage exchanging
nothing but a few bytes of input per tick. If the two simulations ever disagree in gamestate
the game desyncs. Keeping that from happening drove most of the interesting decisions below.
 
Built in 48 hours at UQCS winning the prize for Best Game.
 
<img width="454" height="634" alt="bashroyale" src="https://github.com/user-attachments/assets/5a6614c5-837e-4b3d-81bc-c0cbc94c2289" />
 

 
## Running it
 
**Requirements:** .NET 8+ SDK
 
```bash
git clone [repo-url]
cd bash-royale
dotnet run
```
 
From the start screen, one player hosts and the other enters the host's IP address.
The game listens on UDP port **9050** on the same LAN that's all you need; across the
internet you'll want to forward that port or run both ends through a tunnel.
 
**Controls**
 
| Input | Action |
| --- | --- |
| `1`–`4` | Select a card from your hand |
| Left click | Deploy the selected card |
| e | Open the emote menu |
| Deck builder | Click cards to add/remove; `Enter` saves, `Esc` cancels |
 

 
## What's actually interesting here
 
### Deterministic lockstep networking

We got the idea to use deterministic lockstep networking from a popular RTS called StarCraft/StarCraft2, 
which uses the exact same model.

There's no authoritative server and no state replication. Each client sends only its
input for a given tick, a card index and two coordinates with both
machines running the identical simulation from the same starting state.
 
This is fast and bandwidth-free, but it's unforgiving: any divergence causes
the two players to begin watching different games aka desync. So the entire simulation is
**integer-only**. No floats anywhere in the sim layer. Sub-cell movement uses fixed-point arithmetic at
1000 units per tile, and square roots go through an integer Newton's-method
implementation rather than `Math.Sqrt`.
 
Anywhere a decision could depend on collection ordering, which of two equidistant
enemies to attack, which building to walk toward, ties break deterministically on unit
ID rather than on list position, since list order shifts as units spawn and die.
 
Inputs run on a **10-tick delay buffer**: at tick *N* each client executes the inputs both
players submitted at tick *N−10*, which absorbs latency without either side ever waiting
on the other mid-frame.
 
### Pathfinding
 
Units navigate with A* over the arena grid, but a few things make it more than textbook:
 
- **Footprint-aware.** Units occupy rectangles, not points. A 2×2 Giant needs its whole
  body to fit through a gap, so passability is tested per-footprint rather than per-cell.
- **Stops at attack range.** The search terminates when the unit's footprint is within
  its attack range of the target's footprint, rather than on reaching the target's
  origin. A melee unit stops adjacent; a Musketeer stops six tiles out. This also cuts a
  lot of wasted search against large buildings.
- **Admissible heuristic under a circular stop radius.** Movement is 4-connected and
  uniform-cost, so true path cost is Manhattan but the stop condition is a Euclidean
  radius. The heuristic subtracts the correct bound so A* stays optimal instead
  of taking visible detours around towers.
  
### Targeting
 
Every unit re-evaluates what to attack each tick, filtered by its own rules:
 
- **Layer** ground and air occupy separate collision layers. Flyers pass over troops
  and can only be hit by units that can shoot upward.
- **Building-only** — Giant and Hog Rider ignore troops entirely and walk for towers.
- **Crown tower locking** the King Tower is untargetable until a Princess Tower falls,
  same as the game this borrows from.
- **Target commitment**  a unit holds its target while it's in range to swing at it, and
  re-evaluates freely while chasing. Without commitment, two equidistant enemies make a
  unit thrash between them and never close on either.
All distances are measured **footprint-to-footprint** rather than centre-to-centre, so a
4×4 castle you're standing beside doesn't read as further away than a skeleton across
the lane.
 
### Composable projectiles
 
Projectiles are built from small behaviours rather than hardcoded per type:
 
```
Arrow        Missile(speed, damage)
WizardBall   Splash(speed): WizardBoom: Linger + InstantDamage(3×3)
Fireball     TowerSummon: travels from your crown tower: 3×3 AoE on impact
Zap          InstantDamage(3×3) + SummonProj(visual effect)
```
 
A behaviour can damage, move, linger, or spawn further projectiles, so new spells are
usually a composition of existing pieces rather than new code. Homing projectiles track
their target's live position each tick and resolve on arrival; area effects test
rectangle intersection against every enemy footprint in the blast.
 
### Cards
 
Three card archetypes share one deployment path:
 
- **Unit**: a single troop or building.
- **Swarm**: N troops in a fixed formation (Skeleton Army's 8-block, Three Musketeers'
  triangle). Formations are offset lists, and each spawn searches outward
  deterministically for free space if its cell is taken, so a swarm dropped against a
  wall spreads rather than stacking.
- **Spell**: spawns a projectile instead of a body, and can be cast on either side of
  the river.
The deploy cursor previews the exact footprint the card will occupy — every member of a
swarm, the full area of a spell, the true size of a large unit — and validates every cell
of it, so you can't drop half a Skeleton Army across the river.
 
### Rendering
 
Everything is drawn in a terminal-style grid via SadConsole. A few tricks make a
dense battle readable in a 28×38 character space:
 
- The client's view is point-reflected, so both players see their own side at the
  bottom. One helper converts screen and world in both directions, since a 180° rotation is
  its own inverse.
- Air units cast shadows on the cell below by darkening the terrain colour at the corresponding grid cell,
  which is what makes a flyer read as flying rather than as another troop.
- Air and ground can occupy the same cell, so the flyer takes the glyph while the troop
  underneath keeps its team-coloured background so both stay visible.
- Health bars sit alongside units and flash when they'd cover something, rather than
  silently hiding a unit behind UI.
- Terrain sprouts are placed by a spatial hash rather than RNG, so the grass texture
  is identical on both machines without costing any state.
---
 
## Project layout
 
```
UnitSim.cs        Targeting, range checks, footprint collision
Movement.cs       Movement behaviours and the attack/chase/neutral behaviour tree
Pathfinder.cs     A* with footprint and stop-distance awareness
ProjectileSim.cs  Projectile behaviours and area effects
CardSim.cs        Card definitions, deployment, swarm formations
GameSim.cs        Tick orchestration, damage resolution, win conditions
UnitInfos.cs      Every unit's stats and behaviours
NetworkManager.cs LiteNetLib transport, deck handshake, input relay
BattleRenderer.cs The arena, HUD and all in-battle drawing
DeckScreen.cs     Deck builder
```
 
## Built with
 
- **C# / .NET** — the whole thing
- **[SadConsole](https://sadconsole.com/)** — ASCII rendering
- **[LiteNetLib](https://github.com/RevenantX/LiteNetLib)** — reliable UDP transport
## Known limitations
 
- No reconnection: a dropped connection ends the match, since lockstep can't recover
  missed input ticks.
- No matchmaking, direct IP only.
- Balance is hackathon-grade some units are definitely overtuned.
  
## Authors:
James Miller 
 
Kasper Hendriks
 
Javed Askary
 
Billy Rule
 
Alan Lu
