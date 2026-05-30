# Library Escape

## What the game is
You're locked in an old library and a **5-minute timer** is ticking. Scattered around the
room are five glowing objects. Walk up to one, press **F**, and a little **puzzle** pops up.
Solve it and you're rewarded with a **chapter** of a story — a professor's confession that
he hid in this room. Read all four chapters and you'll notice each one quietly hides a
**number**. Those four digits are the code to **defuse the bomb** and unlock your way out.

One room, one countdown, five puzzles, a four-digit secret. There's a progress counter on
screen (**Puzzle Progress: X / 5**) so you always know how many are left, and the exit only
opens once all five are done.

## The five puzzles
Each object holds a different kind of puzzle, and solving it unlocks the next chapter of the
story. You have to do them **in order**.

1. **The Safe — Colour Memory.** Four coloured pads flash a pattern. Watch it, then click the
   pads back in the same order. Get it right and the safe gives up **Chapter 1**.
2. **The Lamp — Truth or Lie.** Answer a few true/false statements. One wrong answer restarts
   the round, so read carefully. Clear them all to earn **Chapter 2**.
3. **The Magnifier — Word Arrangement.** A sentence is scrambled into word tiles. Click the
   words in the right order to rebuild it (Undo / Backspace fixes mistakes) for **Chapter 3**.
4. **The Chair — Find the Way Out.** A little maze appears. Guide the token from the start to
   the green exit with WASD / arrows (or the on-screen arrows) to unlock **Chapter 4**.
5. **The Bomb — Disarm It.** Each chapter hid one digit. Punch the four numbers into the bomb's
   keypad. You get **3 attempts** — fail all three and it goes off (you lose). Get it right and
   all five puzzles are done.

When the last puzzle is solved the **exit door glows green** so you know where to go. Walk
through it → **YOU ESCAPED!**

## Win & lose
- **Win** — solve all five puzzles, then walk through the now-green exit door.
- **Lose** — the 5-minute timer hits `0:00`, **or** you enter the wrong bomb code three times.
  Either way a Game Over screen shows with a **Restart** button.

## Assets I used
- **Library (the room)** — Sketchfab: https://sketchfab.com/3d-models/library-7fc61f0d65ee49d0b2904e85d1fa520e
- **Western safe (the lock box)** — Sketchfab: https://sketchfab.com/3d-models/western-safe-fd9c2492eb7a4aabb19597af10269f5a
- **Desk lamp** — Sketchfab: https://sketchfab.com/3d-models/library-desk-lamp-fe755dad093b4c27b508574b607dce8d
- **Books with magnifier** — Sketchfab: https://sketchfab.com/3d-models/books-with-magnifier-cc153ce6258b4e538d9fc7da5b5d0fe4
- **Breen chair** — Sketchfab: https://sketchfab.com/3d-models/breenchair-e696f8cb3c894137aaced90c3abf3d17
- **Table** — Sketchfab: https://sketchfab.com/3d-models/table-42fbbc6bc5964fb69a7154df79044a8b
- **Flower pot w/ table** — Sketchfab: https://sketchfab.com/3d-models/flower-pot-with-wooden-table-c9f0e8ca4c7d4eda86590eb2aaa91633
- **Bomb (the finale prop)** — Free3D: https://free3d.com/3d-model/bomp-maya-compleet-files-858034.html
- **First-person player controller** — Mini First Person Controller (Unity Asset Store): https://assetstore.unity.com/packages/tools/input-management/mini-first-person-controller-174710

Most of the Sketchfab models are **CC-BY** (credit the author). The C# I wrote myself:
`CountdownTimer` (the clock + lose screen), `PuzzleManager` (the X / 5 tracking),
`Interactable` + `PlayerInteractor` (the glow + `F` to interact), `ChapterClue` + `InfoPanel`
(the chapter overlays), the four puzzle scripts `SequencePuzzle` / `QuizPuzzle` / `WordPuzzle`
/ `MazePuzzle`, `KeypadController` (the bomb), `ExitTrigger` + `ExitBeacon` (the green exit and
the win), and `DoorController` (swinging the door open).

## Controls
| Key | Action |
|-----|--------|
| `W A S D` | Move around (and move the token in the maze puzzle) |
| Mouse | Look around |
| `F` | Interact — open the puzzle on the object you're standing next to |
| Mouse Left Click | Press buttons inside a puzzle (pads, answers, words, keypad) |
| `Esc` | Back out of a puzzle without solving it |

That's it — walk up to a glowing object, press `F`, solve the puzzle, read your chapter.
