using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lives on the player camera. Every frame it casts a short ray straight ahead; if it
/// hits an <see cref="Interactable"/> within range, that object lights up and the HUD
/// shows a "[F] {message}" prompt. Pressing the interact key triggers the object.
///
/// Attach this to the camera (or anything that points where the player looks). If you
/// leave the camera field empty it grabs Camera.main automatically.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [Tooltip("How close the player must be looking to interact (metres).")]
    public float range = 3f;

    [Tooltip("Camera the ray is cast from. Leave empty to use Camera.main.")]
    public Camera viewCamera;

    [Tooltip("Which layers count as interactable. Default = Everything.")]
    public LayerMask interactMask = ~0;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.F;

    [Header("UI")]
    [Tooltip("Root object of the prompt (enabled only while looking at something).")]
    public GameObject promptRoot;

    [Tooltip("Text label that displays the prompt message.")]
    public Text promptText;

    Interactable current;

    void Awake()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (promptRoot != null) promptRoot.SetActive(false);
        Debug.Log("[PlayerInteractor] active on '" + name + "'. Camera = " +
                  (viewCamera != null ? viewCamera.name : "NULL (no MainCamera!)"));
    }

    void Update()
    {
        Interactable found = FindInteractable();

        // Focus changed: unfocus the old one, focus the new one, update the prompt.
        if (found != current)
        {
            if (current != null) current.OnUnfocus();
            current = found;
            if (current != null) current.OnFocus();
            RefreshPrompt();
            if (current != null)
                Debug.Log("[PlayerInteractor] looking at: " + current.name + " (press " + interactKey + ")");
        }

        if (current != null && Input.GetKeyDown(interactKey))
        {
            current.Interact();
            // The interaction may have disabled the object (one-shot); refresh prompt.
            if (!current.CanInteract)
            {
                current.OnUnfocus();
                current = null;
                RefreshPrompt();
            }
        }
    }

    Interactable FindInteractable()
    {
        if (viewCamera == null) return null;
        Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, interactMask, QueryTriggerInteraction.Ignore))
        {
            Interactable it = hit.collider.GetComponentInParent<Interactable>();
            if (it != null && it.CanInteract) return it;
        }
        return null;
    }

    void RefreshPrompt()
    {
        if (promptRoot != null) promptRoot.SetActive(current != null);
        if (promptText != null && current != null)
            promptText.text = "[" + interactKey + "] " + current.promptMessage;
    }
}
