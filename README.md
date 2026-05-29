# Puzzle Room: Library Escape

A single-room, first-person escape puzzle built in Unity (URP).
The player is locked in a **library** with a **5:00 countdown** running. They must solve a
chain of five interconnected environmental puzzles to derive the **safe code** by counting
non-text clues, crack the safe, recover the door key, and escape before the timer hits zero.

## 1. Game Concept Summary

- **Setting:** One enclosed room — a library (imported `library.glb`).
- **Goal:** Crack the safe, get the door key, and escape the library before the countdown reaches `0:00`.
- **Core loop:** Explore → solve a puzzle → it physically reveals a *countable*
  non-text clue → repeat across the room → derive the 3-digit safe code by counting →
  open the safe → take the door key → use it on the locked door → escape.
- **Win condition:** Safe opened and door key used on the door → door swings open →
  player walks through the exit trigger → win screen.
- **Lose condition:** The 5:00 timer reaches `0:00` → lose screen → restart. (Entering a
  wrong code on the safe also resets the entry, so the player cannot brute-force instantly.)
- **Clue philosophy:** All clues are **non-text / diegetic** — the player *counts glowing
  objects* (books, symbols, marks) revealed by each puzzle, and a **colour/position cue**
  gives the order. No clue is ever a written instruction or a written-out code. The 3-digit
  safe code is *derived from the environment*, never displayed as digits.

---

## 2. Puzzle Structure — 5 Sequential Layers

The layers are **interconnected**: lighting the room makes later clues visible, and every
clue feeds the single safe code. Progress is shown on-screen as `Puzzle Progress: X / 5`
and only increments on genuine task completion.

| # | Layer | Player action (interaction) | Non-text clue revealed | What it reveals / unlocks |
|---|-------|------------------------------|--------------------|----------------------------|
| 1 | **Light the room** | Press `F` on the desk lamp to switch it on | The light reveals **N glowing books** on a shelf — *count them* = 1st digit. It also lights a dark corner so the magnifier becomes usable. | Digit ① + unlocks Layer 2 |
| 2 | **Magnify the mark** | Press `F` to use the magnifier on a framed picture | **N faint symbols** become visible under the lens — *count them* = 2nd digit | Digit ② |
| 3 | **Search the furniture** | Press `F` on the chair / table to search it | A hidden cluster of **N tally marks** is revealed — *count them* = 3rd digit | Digit ③ |
| 4 | **Crack the safe** | Press `F` on the safe → keypad canvas → enter the 3 digits in the order shown by the **colour cue** | The three digits from Layers 1–3 | Safe swings open, exposing the **door key** |
| 5 | **Escape** | Pick up the key (`F`), then press `F` on the door | The key unlocks the door | Door swings open → exit trigger → **win** |

**Win step:** Open the safe → pick up the door key → approach the door → door unlocks and opens → exit trigger fires win.

> Each layer is interactable, contributes to the final solution, and is tracked
> programmatically by the `PuzzleManager`.

---

## 3. Clue System (non-text)

| Clue type | Where it appears |
|-----------|------------------|
| Light / color change | Layer 1 lamp switches on and illuminates the countable books |
| Counting / visual quantity | Books (L1), symbols under the magnifier (L2), tally marks (L3) → each is a digit |
| Object positioning / colour order | A small set of coloured markers by the safe gives the order to enter the 3 digits |
| Audio / visual feedback | Lamp click, safe-unlock clunk, door-open creak, win/lose stingers |

Text on screen is limited to **non-solving** UI only: the progress counter, the timer,
interaction prompts ("[F] …"), and *non-solving* flavour ("a light flicked on somewhere").
**No on-screen text ever states a digit or the code** — every number is derived by counting.

---

## 4. Controls

| Input | Action |
|-------|--------|
| `W A S D` | Move |
| Mouse | Look |
| `Left Shift` | Run |
| `Space` | Jump |
| `F` | Interact (turn on lamp, use magnifier, search, open safe, pick up key, open door) |
| Mouse Left Click | Press keypad buttons on the safe canvas |

*(Movement provided by the Mini First Person Controller; interaction by custom scripts.)*

---

## 5. Win & Lose Conditions

- **Win:** All 5 layers complete → safe opened → door key collected → door swings open → player
  walks through the exit trigger. Feedback: door-open swing animation + **Win panel** ("YOU ESCAPED!").
- **Lose:** The countdown reaches `0:00`. Feedback: **Lose panel** ("TIME'S UP") with a **Restart** button.
- The lose state prevents an instant win: the safe code is *derived by counting* the puzzle clues,
  and the door stays locked until the key from the safe (Layer 5) unlocks it.

---

## 6. Scenes & Flow

The whole game runs in **one scene** (`Assets/Scenes/SampleScene.unity`). The menu, gameplay,
and win/lose are **UI panels** layered over that single scene — no scene loading needed except
the restart.

```
[Start menu overlay] ──[Start]──► gameplay (time runs) ──[escape door]──► Win panel
  (timeScale = 0)                      │
                                  [timer = 0:00] ──► Lose panel ──[Restart]──► reload scene
```

- **Start menu** — an overlay panel (`Menu Canvas` / `Main Panel`) shown on load with the game
  paused (`Time.timeScale = 0`); the **Start** button un-pauses and locks the cursor for FPS play.
- **Gameplay** — the library, the safe, the puzzle props, and the HUD (timer + `X / 5` progress).
- **Win / Lose** — panels in the same scene, shown by `GameManager`; Restart reloads the scene.

---

## 7. Custom C# Scripts (≥ 3 required)

| Script | Responsibility |
|--------|----------------|
| `MainMenuController` | Start / Quit buttons, scene loading |
| `GameManager` | 5:00 timer countdown, win/lose state, restart |
| `PuzzleManager` | Tracks completed layers, drives `X / 5` progress + indicator dots, fires `onAllSolved` |
| `Interactable` | Per-object glow highlight + `onInteract` UnityEvent, one-shot support |
| `PlayerInteractor` | Camera raycast, focus highlight, `[F]` prompt, fires the focused Interactable |
| `KeypadController` | Safe code entry canvas, validation, freezes player while open |
| `DoorController` | Reusable hinge swing (door **and** safe lid); locked until `Unlock()`; `onOpened` |
| `ClueHint` | Shows *non-solving* flavour hints only (never a digit) |
| `IntroController` | Story/rules screen on load; freezes the timer until the player clicks Begin |

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

| Asset (role in game) | Source | Link | License |
|----------------------|--------|------|---------|
| Library (room) | Sketchfab | https://sketchfab.com/3d-models/library-7fc61f0d65ee49d0b2904e85d1fa520e | CC-BY (verify on page) |
| Bomb (timer theme) | Free3D | https://free3d.com/3d-model/bomp-maya-compleet-files-858034.html | per Free3D page |
| Desk lamp (Puzzle 1) | Sketchfab | https://sketchfab.com/3d-models/library-desk-lamp-fe755dad093b4c27b508574b607dce8d | CC-BY (verify on page) |
| Western safe (Puzzle 4 / lock box) | Sketchfab | https://sketchfab.com/3d-models/western-safe-fd9c2492eb7a4aabb19597af10269f5a | CC-BY (verify on page) |
| Table | Sketchfab | https://sketchfab.com/3d-models/table-42fbbc6bc5964fb69a7154df79044a8b | CC-BY (verify on page) |
| Books with magnifier (Puzzle 2) | Sketchfab | https://sketchfab.com/3d-models/books-with-magnifier-cc153ce6258b4e538d9fc7da5b5d0fe4 | CC-BY (verify on page) |
| Breen chair (Puzzle 3) | Sketchfab | https://sketchfab.com/3d-models/breenchair-e696f8cb3c894137aaced90c3abf3d17 | CC-BY (verify on page) |
| Flower pot w/ table | Sketchfab | https://sketchfab.com/3d-models/flower-pot-with-wooden-table-c9f0e8ca4c7d4eda86590eb2aaa91633 | CC-BY (verify on page) |
| Mini First Person Controller (movement) | Unity Asset Store | (add link) | Asset Store EULA |

> **License note:** most of these Sketchfab models are **CC-BY** — that means you must
> credit the author. Open each model's page, copy the exact license + author name shown,
> and replace "verify on page" above. Keeping this table accurate is part of the grade.

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

