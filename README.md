# Puzzle Room: The Defuse Room

A single-room, first-person escape puzzle built in Unity (URP).
The player is locked in a room with an **armed bomb on a 5:00 countdown**. They must
solve a chain of five interconnected environmental puzzles to derive the bomb's
disarm code, defuse it, recover the door key, and escape before the timer hits zero.

> **Status:** In development. Sections marked `TODO` are filled in as the build progresses.

---

## 1. Game Concept Summary

- **Setting:** One enclosed room (magician's study / library blockout).
- **Goal:** Escape the locked room before the bomb timer reaches `0:00`.
- **Core loop:** Explore → read non-text environmental clues → solve a puzzle →
  the solved puzzle reveals the next clue → repeat → derive the bomb code →
  defuse the bomb → grab the door key → walk to the door → escape.
- **Win condition:** Bomb defused and door key used on the door → door opens → player exits.
- **Lose condition:** The 5:00 bomb timer reaches `0:00` → explosion → lose screen → restart.
- **Clue philosophy:** All clues are **non-text / diegetic** (lights, symbols, object
  alignment, counting, patterns). No clue is ever a written instruction or a written-out
  code. The 4-digit bomb code is *derived from the environment*, never displayed as digits.

---

## 2. Puzzle Structure — 5 Sequential Layers

The puzzles are **strictly sequential**: each solved layer physically reveals or unlocks
the next. Progress is shown on-screen as `Puzzle Progress: X / 5` and only increments on
genuine task completion.

| # | Layer | Player action (interaction) | Non-text clue used | What it reveals / unlocks |
|---|-------|------------------------------|--------------------|----------------------------|
| 1 | **Light the room** | Flip 3 desk/wall switches into the correct on/off state | A flickering lamp + a UV/blacklight cue | A hidden symbol becomes visible on the wall |
| 2 | **Symbol match** | Open the drawer/cabinet marked with the matching symbol | The wall symbol from Layer 1 | A colored key piece + reveals a rotation clue |
| 3 | **Rotation align** | Rotate 3 objects (clock hands, a dial, a painting) until their marks align | Shapes/notches that must point the same way | A locked box pops open, exposing the picture wall |
| 4 | **Count & order** | Read 4 framed pictures, each showing N objects, in the order given by a cue | Object counts + a light/color sequence giving the read order | The 4-digit bomb disarm code (derived, never written) |
| 5 | **Defuse & key** | Enter the 4-digit code on the bomb keypad | Code derived in Layer 4 | Bomb disarms; a compartment opens revealing the **door key** |

**Win step:** Pick up the door key → approach the door → door unlocks and opens → exit trigger fires win.

> Each layer is interactable, contributes to the final solution, and is tracked
> programmatically by the `PuzzleManager`.

---

## 3. Clue System (non-text)

| Clue type | Where it appears |
|-----------|------------------|
| Light / color change | Layer 1 lamp + blacklight reveal |
| Symbols / icons | Wall symbol → drawer match (Layer 2) |
| Object positioning / rotation | Dial/clock/painting alignment (Layer 3) |
| Visual pattern + counting | Framed pictures + light sequence for read order (Layer 4) |
| Audio / visual feedback | Disarm beep, explosion, door unlock sound |

Text on screen is limited to **non-solving** UI only: the progress counter, the timer,
and interaction prompts (e.g. "Press E"). No text reveals an answer.

---

## 4. Controls

| Input | Action |
|-------|--------|
| `W A S D` | Move |
| Mouse | Look |
| `Left Shift` | Run |
| `Space` | Jump |
| `E` / Left Click | Interact (pick up, flip switch, open, enter code) |
| `Esc` | Pause / settings |

*(Movement provided by the Mini First Person Controller; interaction by custom scripts.)*

---

## 5. Win & Lose Conditions

- **Win:** All 5 layers complete → bomb defused → door key collected → door opened → exit.
  Feedback: door-open animation, win sound, win screen.
- **Lose:** Timer reaches `0:00`. Feedback: explosion VFX/SFX, lose screen with a **Restart** button.
- The lose state prevents an instant win: the code cannot be entered before Layer 4 is solved,
  and the door cannot open without the key from Layer 5.

---

## 6. Scenes & Flow

```
MainMenu  ──[Start]──►  GameRoom  ──[escape]──►  Win screen
   │                        │
[Settings]              [timer = 0] ──► Lose screen ──[Restart]──► GameRoom
```

- **MainMenu** — Start button (loads GameRoom), Settings button (volume / sensitivity panel).
- **GameRoom** — the playable room, bomb, puzzles, HUD (timer + progress).
- Win / Lose are panels (or scenes) reachable from GameRoom.

---

## 7. Custom C# Scripts (≥ 3 required)

| Script | Responsibility |
|--------|----------------|
| `MainMenuController` | Start / Settings / Quit buttons, scene loading |
| `GameManager` | Bomb timer countdown, win/lose state, restart |
| `PuzzleManager` | Tracks completed layers, drives `X / 5` progress, unlocks final step |
| `Interactable` (+ `PlayerInteractor`) | Raycast interaction, "Press E" prompts |
| `KeypadController` | Bomb code entry & validation |
| `DoorController` | Key check + open animation + exit trigger |

> Demonstrates: colliders/triggers, conditional logic (`if`, `bool`, counters), and
> event-based interaction (UnityEvents / C# events between puzzles and the manager).

---

## 8. Project Structure

```
Assets/
├── _Game/                 # our custom content (keep separate from imported assets)
│   ├── Scripts/
│   ├── Scenes/            # MainMenu.unity, GameRoom.unity
│   ├── Prefabs/
│   ├── Materials/
│   └── Audio/
├── Mini First Person Controller/   # imported (movement)
├── Scenes/                # room blockout / library models
└── Settings/              # URP assets
```

---

## 9. Asset Sources

| Asset | Source | Link | License |
|-------|--------|------|---------|
| Mini First Person Controller | Unity Asset Store | TODO | TODO |
| Room / library model | TODO (Sketchfab?) | TODO | TODO |
| Bomb model | TODO | TODO | TODO |
| Furniture / props | TODO | TODO | TODO |
| SFX (beep, explosion, click) | TODO | TODO | TODO |

---

## 10. Submission Checklist

- [ ] Unity project repo
- [ ] Gameplay video recording — TODO (link)
- [x] README (this file)
- [ ] 5 puzzle layers implemented & tracked
- [ ] Win + lose conditions with clear feedback
- [ ] All clues non-text
- [ ] ≥ 3 custom scripts
- [ ] Assets sourced, licensed, and organized
