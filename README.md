# Library Escape

## What the game is
You're locked in an old library and a **5-minute timer** is ticking. Poke around the room,
solve the five puzzles hidden in the objects, crack the safe to get the door key, and get
out before the clock hits `0:00`. If the timer runs out first, you lose and restart.

One room, one locked door, a countdown, and five things to figure out. There's a progress
counter on screen (**Puzzle Progress: X / 5**) so you can see how many are left, and the
door only opens once you've escaped with the key.

## The five puzzle tasks
You have to do all five, and none of them are explained in words — it's all lights, counting,
symbols and sound. The only text on screen is the timer and the progress counter.

1. **Turn on the desk lamp** — the lamp lights up a shelf so you can **count the glowing books** (that's the first number).
2. **Use the magnifier** — look through it at a mark to reveal **faint symbols you count** (the second number).
3. **Search the furniture** — check the chair / table to uncover a **hidden set of marks** (the third number).
4. **Crack the safe** — punch the three counted numbers into the safe's keypad. It swings open and there's the **door key** inside. Wrong code just resets, so you can't brute-force it.
5. **Escape** — grab the key, walk to the door, and it unlocks and swings open. You made it out → **YOU ESCAPED!**

Nothing ever tells you the code in text — every digit is *counted from the environment*.

## Assets I used
- **Library (the room)** — Sketchfab: https://sketchfab.com/3d-models/library-7fc61f0d65ee49d0b2904e85d1fa520e
- **Western safe (the lock box)** — Sketchfab: https://sketchfab.com/3d-models/western-safe-fd9c2492eb7a4aabb19597af10269f5a
- **Desk lamp** — Sketchfab: https://sketchfab.com/3d-models/library-desk-lamp-fe755dad093b4c27b508574b607dce8d
- **Books with magnifier** — Sketchfab: https://sketchfab.com/3d-models/books-with-magnifier-cc153ce6258b4e538d9fc7da5b5d0fe4
- **Breen chair** — Sketchfab: https://sketchfab.com/3d-models/breenchair-e696f8cb3c894137aaced90c3abf3d17
- **Table** — Sketchfab: https://sketchfab.com/3d-models/table-42fbbc6bc5964fb69a7154df79044a8b
- **Flower pot w/ table** — Sketchfab: https://sketchfab.com/3d-models/flower-pot-with-wooden-table-c9f0e8ca4c7d4eda86590eb2aaa91633
- **Bomb (timer prop)** — Free3D: https://free3d.com/3d-model/bomp-maya-compleet-files-858034.html
- **First-person player controller** — Mini First Person Controller (Unity Asset Store): https://assetstore.unity.com/packages/tools/input-management/mini-first-person-controller-174710

Most of the Sketchfab models are **CC-BY** (credit the author). Custom C# I wrote: `GameManager`
(timer + win/lose), `PuzzleManager` (the X / 5 tracking), `Interactable` + `PlayerInteractor`
(look-at glow + `F` to interact), and `DoorController` (swings the safe + door open).

## Controls
| Key | Action |
|-----|--------|
| `W A S D` | Move around |
| Mouse | Look around |
| `F` | Interact — turn on the lamp, use the magnifier, open the safe, grab the key, open the door |
| Mouse Left Click | Press the keypad buttons on the safe |
| `Esc` | Quit |

That's it — walk up to something, look at it (it glows), and press `F`.
