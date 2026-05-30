using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// A reusable code-entry puzzle on a Canvas. The flow:
///   1. The player walks up to the lock box / keypad object and presses F.
///      That object's Interactable.onInteract should call <see cref="Open"/>.
///   2. The keypad canvas appears, the cursor unlocks, and player look/move is frozen.
///   3. The player clicks digit buttons (each button's OnClick calls <see cref="PressDigit"/>),
///      then Enter (<see cref="Submit"/>). Clear (<see cref="Clear"/>) resets entry.
///   4. Correct code -> <see cref="onSolved"/> fires (wire to PuzzleManager.Solve and the
///      lock-box/door opening) and the canvas closes. Wrong code -> brief shake/feedback.
///
/// The 4-digit code is set in the Inspector. In the README design the digits are *derived*
/// from the framed-picture counts; here we just store the answer.
/// </summary>
public class KeypadController : MonoBehaviour
{
    [Header("Code")]
    [Tooltip("The correct code the player must enter.")]
    public string correctCode = "1234";

    [Tooltip("Maximum number of digits the player can type.")]
    public int maxLength = 4;

    [Tooltip("Wrong-guess limit (e.g. the bomb = 3). 0 = unlimited.")]
    public int maxAttempts = 0;

    [Header("UI")]
    [Tooltip("Root of the keypad canvas (toggled on/off).")]
    public GameObject canvasRoot;

    [Tooltip("Text that shows what the player has typed so far.")]
    public Text displayText;

    [Tooltip("Optional message line for 'Correct!' / 'Wrong code'.")]
    public Text feedbackText;

    [Header("Player control")]
    [Tooltip("Behaviours disabled while the keypad is open (drag in FirstPersonLook, " +
             "FirstPersonMovement, and the PlayerInteractor).")]
    public Behaviour[] disableWhileOpen;

    [Header("Events")]
    public UnityEvent onSolved;
    public UnityEvent onWrong;

    [Tooltip("Fires when the player runs out of attempts (e.g. the bomb explodes -> lose).")]
    public UnityEvent onOutOfAttempts;

    string entry = "";
    bool solved;
    int attemptsUsed;
    bool outOfAttempts;

    void Awake()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    /// <summary>Show the keypad and hand control to the cursor.</summary>
    public void Open()
    {
        if (solved) return;
        entry = "";
        UpdateDisplay();
        if (feedbackText != null) feedbackText.text = "";
        if (canvasRoot != null) canvasRoot.SetActive(true);

        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Hide the keypad and return control to the player.</summary>
    public void Close()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
        SetPlayerControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Hook each number button's OnClick to this with the digit as the argument.</summary>
    public void PressDigit(int digit)
    {
        if (entry.Length >= maxLength) return;
        entry += digit.ToString();
        UpdateDisplay();
    }

    /// <summary>Some Button OnClick setups pass a string more easily than an int.</summary>
    public void PressDigitString(string digit)
    {
        if (entry.Length >= maxLength) return;
        entry += digit;
        UpdateDisplay();
    }

    public void Clear()
    {
        entry = "";
        if (feedbackText != null) feedbackText.text = "";
        UpdateDisplay();
    }

    public void Backspace()
    {
        if (entry.Length > 0) entry = entry.Substring(0, entry.Length - 1);
        UpdateDisplay();
    }

    /// <summary>Wire the Enter button here. Validates the entered code.</summary>
    public void Submit()
    {
        if (outOfAttempts) return;

        if (entry == correctCode)
        {
            solved = true;
            if (feedbackText != null) feedbackText.text = "Correct!";
            onSolved?.Invoke();
            Close();
            return;
        }

        // Wrong guess.
        attemptsUsed++;
        entry = "";
        UpdateDisplay();
        onWrong?.Invoke();

        if (maxAttempts > 0 && attemptsUsed >= maxAttempts)
        {
            outOfAttempts = true;
            if (feedbackText != null) feedbackText.text = "OUT OF ATTEMPTS!";
            onOutOfAttempts?.Invoke();   // e.g. bomb explodes -> Lose
            // Hide the keypad so the Lose screen behind it is visible.
            if (canvasRoot != null) canvasRoot.SetActive(false);
        }
        else if (feedbackText != null)
        {
            feedbackText.text = maxAttempts > 0
                ? "Wrong — " + (maxAttempts - attemptsUsed) + " tries left"
                : "Wrong code";
        }
    }

    void UpdateDisplay()
    {
        if (displayText == null) return;
        // Show typed digits, padded with underscores up to maxLength.
        displayText.text = entry.PadRight(maxLength, '_');
    }

    void SetPlayerControl(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b != null) b.enabled = enabled;
    }
}
