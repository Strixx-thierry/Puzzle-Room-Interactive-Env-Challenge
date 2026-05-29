using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour
{
    [Header("Hinge")]
    [Tooltip("The transform that rotates. Leave empty to rotate this object.")]
    public Transform hinge;

    [Tooltip("Local axis to rotate around (Y for most doors, X for a chest/box lid).")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("How far it swings open, in degrees.")]
    public float openAngle = 90f;

    [Tooltip("Seconds the swing takes.")]
    public float openDuration = 1.2f;

    [Header("Lock")]
    [Tooltip("If true, TryOpen() does nothing until Unlock() is called (door + key case). " +
             "Leave false for a box that opens straight from an event.")]
    public bool startLocked = false;

    [Header("Feedback")]
    [Tooltip("Optional sound played when it opens.")]
    public AudioSource openSound;

    public UnityEvent onOpened;

    bool locked;
    bool opened;
    Quaternion closedRot;

    void Awake()
    {
        if (hinge == null) hinge = transform;
        closedRot = hinge.localRotation;
        locked = startLocked;
    }

    /// <summary>Allow the door to be opened (call from the key pickup).</summary>
    public void Unlock() => locked = false;

    /// <summary>For a door Interactable's onInteract: opens only if unlocked.</summary>
    public void TryOpen()
    {
        if (locked || opened) return;
        Open();
    }

    /// <summary>Force the swing open (use from puzzle-solved events).</summary>
    public void Open()
    {
        if (opened) return;
        opened = true;
        locked = false;
        if (openSound != null) openSound.Play();
        StopAllCoroutines();
        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        Quaternion target = closedRot * Quaternion.AngleAxis(openAngle, rotationAxis.normalized);
        float t = 0f;
        // Use unscaledDeltaTime so the door still swings even if the game is paused.
        while (t < openDuration)
        {
            t += Time.unscaledDeltaTime;
            hinge.localRotation = Quaternion.Slerp(closedRot, target, Mathf.SmoothStep(0f, 1f, t / openDuration));
            yield return null;
        }
        hinge.localRotation = target;
        onOpened?.Invoke();
    }
}
