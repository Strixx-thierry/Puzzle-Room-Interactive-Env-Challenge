using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Memory / "Simon" puzzle. The panel of numbered pads flashes a sequence; the player
/// repeats it by clicking the pads in the same order.
///
///   • Open()  — opens the panel, freezes the player, plays the sequence, then listens.
///   • PadPressed(int)  — wire each pad button's OnClick to this with its index.
///   • Correct full sequence  -> onSolved (wire to ChapterClue.Reveal — the reward).
///   • Wrong step  -> onWrong, the sequence replays so the player can try again.
///
/// Self-contained: the answer is shown by the flashes, so there is no external (text) clue.
/// Put this on the always-active Canvas object; <see cref="canvasRoot"/> is the child panel
/// that gets toggled (so coroutines keep running while the panel is hidden).
/// </summary>
public class SequencePuzzle : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Child panel toggled on/off. Leave THIS script on the always-active Canvas.")]
    public GameObject canvasRoot;

    [Tooltip("Heading, e.g. 'REPEAT THE PATTERN'.")]
    public Text titleText;

    [Tooltip("Status line ('Watch…', 'Now repeat', 'Wrong', 'Unlocked!').")]
    public Text statusText;

    [Tooltip("The clickable pads, in index order (pad 0, pad 1, ...).")]
    public Image[] pads;

    [Header("Pad colours")]
    [Tooltip("Fallback colour if a pad has none. Each pad keeps its OWN colour; flashing just " +
             "brightens it (that's what the player memorises).")]
    public Color padIdle = new Color(0.18f, 0.18f, 0.22f);

    [Header("Puzzle")]
    [Tooltip("How many steps in the pattern.")]
    public int sequenceLength = 4;

    [Tooltip("If on, a fresh random pattern each time. If off, uses Fixed Sequence below.")]
    public bool randomize = true;

    [Tooltip("Used when Randomize is off: the pad indices to flash, in order (e.g. 0,2,3,1).")]
    public int[] fixedSequence;

    [Tooltip("Seconds each pad stays lit while showing the pattern.")]
    public float litTime = 0.45f;

    [Tooltip("Dark gap between flashes.")]
    public float gapTime = 0.20f;

    [Header("Player control")]
    [Tooltip("Frozen while open (FirstPersonLook, FirstPersonMovement, PlayerInteractor).")]
    public Behaviour[] disableWhileOpen;

    [Tooltip("Key that backs out of the puzzle.")]
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Order gate (optional)")]
    [Tooltip("If set, this puzzle only opens once the previous chapter is read.")]
    public PuzzleManager puzzleManager;

    [Tooltip("This puzzle's index (0-based). Used only for the order gate.")]
    public int puzzleIndex = 0;

    [Tooltip("If on, won't open until puzzle (index - 1) is solved. Index 0 is never gated.")]
    public bool requirePrevious = false;

    [Header("Events")]
    public UnityEvent onSolved;
    public UnityEvent onWrong;

    readonly List<int> sequence = new List<int>();
    Color[] baseColors;
    int inputIndex;
    bool solved;
    bool isOpen;
    bool acceptingInput;

    void Awake()
    {
        // Remember each pad's own colour so flashing can just brighten it.
        if (pads != null)
        {
            baseColors = new Color[pads.Length];
            for (int i = 0; i < pads.Length; i++)
                baseColors[i] = pads[i] != null ? pads[i].color : padIdle;
        }
        if (canvasRoot != null) canvasRoot.SetActive(false);
        ResetPads();
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(closeKey)) Close();
    }

    /// <summary>Wire the prop's Interactable.onInteract to this.</summary>
    public void Open()
    {
        if (solved) return;

        // Order gate: refuse to open until the previous chapter is done.
        if (requirePrevious && puzzleManager != null && puzzleIndex > 0 &&
            !puzzleManager.IsSolved(puzzleIndex - 1))
            return;

        if (canvasRoot != null) canvasRoot.SetActive(true);
        isOpen = true;
        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        BuildSequence();
        StopAllCoroutines();
        StartCoroutine(PlayThenListen());
    }

    public void Close()
    {
        StopAllCoroutines();
        acceptingInput = false;
        if (canvasRoot != null) canvasRoot.SetActive(false);
        isOpen = false;
        SetPlayerControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ResetPads();
    }

    void BuildSequence()
    {
        if (pads == null || pads.Length == 0) return;
        sequence.Clear();

        if (!randomize && fixedSequence != null && fixedSequence.Length > 0)
        {
            foreach (int i in fixedSequence)
                sequence.Add(Mathf.Clamp(i, 0, pads.Length - 1));
        }
        else
        {
            int n = Mathf.Max(1, sequenceLength);
            for (int k = 0; k < n; k++)
                sequence.Add(Random.Range(0, pads.Length));
        }
    }

    IEnumerator PlayThenListen()
    {
        acceptingInput = false;
        ResetPads();
        if (statusText != null) statusText.text = "Watch the pattern…";
        yield return new WaitForSecondsRealtime(0.6f);

        foreach (int idx in sequence)
        {
            Flash(idx, true);
            yield return new WaitForSecondsRealtime(litTime);
            Flash(idx, false);
            yield return new WaitForSecondsRealtime(gapTime);
        }

        inputIndex = 0;
        acceptingInput = true;
        if (statusText != null) statusText.text = "Now repeat it.";
    }

    /// <summary>Wire each pad button's OnClick to this with that pad's index.</summary>
    public void PadPressed(int index)
    {
        if (!acceptingInput || solved) return;
        if (index < 0 || index >= pads.Length) return;

        StartCoroutine(Blink(index));

        if (index == sequence[inputIndex])
        {
            inputIndex++;
            if (inputIndex >= sequence.Count)
            {
                acceptingInput = false;
                solved = true;
                if (statusText != null) statusText.text = "Unlocked!";
                StartCoroutine(CloseAfter(0.6f));
            }
        }
        else
        {
            acceptingInput = false;
            if (statusText != null) statusText.text = "Wrong — watch again.";
            onWrong?.Invoke();
            StartCoroutine(Replay());
        }
    }

    IEnumerator Replay()
    {
        yield return new WaitForSecondsRealtime(0.9f);
        StartCoroutine(PlayThenListen());
    }

    IEnumerator CloseAfter(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        Close();                 // give control back first...
        onSolved?.Invoke();      // ...then the chapter overlay takes it (cursor free to read/close)
    }

    IEnumerator Blink(int index)
    {
        Flash(index, true);
        yield return new WaitForSecondsRealtime(0.18f);
        if (!solved) Flash(index, false);
    }

    void Flash(int index, bool on)
    {
        if (pads == null || index < 0 || index >= pads.Length || pads[index] == null) return;
        Color b = (baseColors != null && index < baseColors.Length) ? baseColors[index] : padIdle;
        pads[index].color = on ? Color.Lerp(b, Color.white, 0.65f) : b;
    }

    void ResetPads()
    {
        if (pads == null) return;
        for (int i = 0; i < pads.Length; i++)
            if (pads[i] != null)
                pads[i].color = (baseColors != null && i < baseColors.Length) ? baseColors[i] : padIdle;
    }

    void SetPlayerControl(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b != null) b.enabled = enabled;
    }
}
