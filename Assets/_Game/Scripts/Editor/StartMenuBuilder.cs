using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

// Editor utility uses some object-finding APIs Unity has since renamed; they still
// work correctly here, so silence the obsolete (CS0618) warnings for this file.
#pragma warning disable 0618

/// <summary>
/// Editor utility that builds the gameplay UI in the active scene and wires it to
/// <see cref="MainMenuController"/> and <see cref="GameManager"/>. Run it from:
///   Puzzle Room  ->  Build Start Menu
///
/// It creates one overlay canvas containing:
///   - a top-center countdown timer (driven by GameManager),
///   - a start menu (title image + Start button),
///   - a lose screen (shown when the timer runs out, with a Restart button).
/// Re-running deletes the previously built canvas and rebuilds it cleanly.
/// </summary>
public static class StartMenuBuilder
{
    private const string CanvasName = "Menu Canvas";
    private const string TitleImagePath = "Assets/Scenes/defuse the room title.png";

    [MenuItem("Puzzle Room/Build Start Menu")]
    public static void Build()
    {
        var res = MakeResources();

        // Remove any previously built canvas so re-runs stay clean.
        var old = GameObject.Find(CanvasName);
        if (old != null) Object.DestroyImmediate(old);

        EnsureEventSystem();

        // --- Root canvas (screen-space overlay, scales with resolution) ---
        var canvasGo = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // draw on top of everything else
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // --- Timer HUD (created first so the menu/lose panels draw over it) ---
        var timerText = MakeTimerText("Timer Text", canvasGo.transform);

        // --- Main panel: title image + Start ---
        var mainPanel = MakePanel("Main Panel", canvasGo.transform, res, new Color(0.04f, 0.04f, 0.06f, 0.94f));
        var titleSprite = LoadTitleSprite();
        if (titleSprite != null)
            MakeImage("Title", titleSprite, mainPanel.transform, new Vector2(0, 120), new Vector2(900, 225));
        else
            MakeTitle("Title", "THE DEFUSE ROOM", mainPanel.transform, new Vector2(0, 120), 64, Color.white);
        var startBtn = MakeButton("Start Button", "Start", mainPanel.transform, res, new Vector2(0, -120));

        // --- Lose panel: message + Restart (hidden until the timer runs out) ---
        var losePanel = MakePanel("Lose Panel", canvasGo.transform, res, new Color(0.06f, 0.0f, 0.0f, 0.96f));
        MakeTitle("Lose Title", "TIME'S UP", losePanel.transform, new Vector2(0, 140), 72, new Color(1f, 0.25f, 0.2f));
        MakeTitle("Lose Subtitle", "The bomb went off.", losePanel.transform, new Vector2(0, 60), 30, Color.white);
        var restartBtn = MakeButton("Restart Button", "Restart", losePanel.transform, res, new Vector2(0, -60));

        // --- Win panel: message + Play Again (hidden until the player escapes) ---
        var winPanel = MakePanel("Win Panel", canvasGo.transform, res, new Color(0.0f, 0.06f, 0.03f, 0.96f));
        MakeTitle("Win Title", "YOU ESCAPED!", winPanel.transform, new Vector2(0, 140), 72, new Color(0.4f, 1f, 0.5f));
        MakeTitle("Win Subtitle", "Bomb defused. You made it out.", winPanel.transform, new Vector2(0, 60), 30, Color.white);
        var playAgainBtn = MakeButton("Play Again Button", "Play Again", winPanel.transform, res, new Vector2(0, -60));

        // --- Find the player scripts so the menu/lose screen can freeze them ---
        var playerLook = Object.FindObjectOfType<FirstPersonLook>();
        var playerMovement = Object.FindObjectOfType<FirstPersonMovement>();

        // --- Controllers + field wiring ---
        var menu = canvasGo.AddComponent<MainMenuController>();
        var menuSo = new SerializedObject(menu);
        menuSo.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        menuSo.FindProperty("playerLook").objectReferenceValue = playerLook;
        menuSo.FindProperty("playerMovement").objectReferenceValue = playerMovement;
        menuSo.ApplyModifiedProperties();

        var gm = canvasGo.AddComponent<GameManager>();
        var gmSo = new SerializedObject(gm);
        gmSo.FindProperty("timerText").objectReferenceValue = timerText;
        gmSo.FindProperty("losePanel").objectReferenceValue = losePanel;
        gmSo.FindProperty("winPanel").objectReferenceValue = winPanel;
        gmSo.FindProperty("playerLook").objectReferenceValue = playerLook;
        gmSo.ApplyModifiedProperties();

        if (playerLook == null)
            Debug.LogWarning("[StartMenuBuilder] No FirstPersonLook found in the scene. " +
                             "The menu can't freeze the camera/cursor — make sure your player prefab is in the scene.");

        // --- Button click events (persisted into the scene) ---
        UnityEventTools.AddPersistentListener(startBtn.onClick, menu.StartGame);
        UnityEventTools.AddPersistentListener(restartBtn.onClick, gm.Restart);
        UnityEventTools.AddPersistentListener(playAgainBtn.onClick, gm.Restart);

        // Hidden by default; the controllers turn them on when needed.
        losePanel.SetActive(false);
        winPanel.SetActive(false);

        EnsureSceneInBuildSettings(canvasGo.scene);

        Selection.activeGameObject = canvasGo;
        EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        Debug.Log("[StartMenuBuilder] Menu + timer + lose screen built. Press Play to test, then save the scene.");
    }

    // ----------------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------------

    private static void EnsureEventSystem()
    {
        var es = Object.FindObjectOfType<EventSystem>();
        if (es == null)
            es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

        // This project's Active Input Handling is "Both", so the legacy
        // StandaloneInputModule drives UI clicks reliably. The Input System UI module
        // needs its actions asset wired up, which does NOT persist when added from a
        // script, leaving buttons silently dead. So we use the standalone module and
        // remove the Input System one if a previous build added it.
#if ENABLE_INPUT_SYSTEM
        var ism = es.GetComponent<InputSystemUIInputModule>();
        if (ism != null) Object.DestroyImmediate(ism);
#endif
        if (es.GetComponent<StandaloneInputModule>() == null)
            es.gameObject.AddComponent<StandaloneInputModule>();
    }

    private static void EnsureSceneInBuildSettings(UnityEngine.SceneManagement.Scene scene)
    {
        if (string.IsNullOrEmpty(scene.path)) return; // unsaved scene

        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == scene.path)) return;

        scenes.Add(new EditorBuildSettingsScene(scene.path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[StartMenuBuilder] Added '" + scene.path + "' to Build Settings so Restart can reload it.");
    }

    private static Sprite LoadTitleSprite()
    {
        var importer = AssetImporter.GetAtPath(TitleImagePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("[StartMenuBuilder] Title image not found at '" + TitleImagePath + "'. Falling back to text title.");
            return null;
        }

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(TitleImagePath);
    }

    private static DefaultControls.Resources MakeResources()
    {
        return new DefaultControls.Resources
        {
            standard   = Builtin("UI/Skin/UISprite.psd"),
            background = Builtin("UI/Skin/Background.psd"),
            inputField = Builtin("UI/Skin/InputFieldBackground.psd"),
            knob       = Builtin("UI/Skin/Knob.psd"),
            checkmark  = Builtin("UI/Skin/Checkmark.psd"),
            dropdown   = Builtin("UI/Skin/DropdownArrow.psd"),
            mask       = Builtin("UI/Skin/UIMask.psd"),
        };
    }

    private static Sprite Builtin(string path) =>
        AssetDatabase.GetBuiltinExtraResource<Sprite>(path);

    private static GameObject MakePanel(string name, Transform parent, DefaultControls.Resources res, Color color)
    {
        var go = DefaultControls.CreatePanel(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static void MakeImage(string name, Sprite sprite, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    private static Text MakeTimerText(string name, Transform parent)
    {
        var go = DefaultControls.CreateText(new DefaultControls.Resources());
        go.name = name;
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        // Anchor to top-center of the screen.
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(300, 90);
        rt.anchoredPosition = new Vector2(0, -50);
        var t = go.GetComponent<Text>();
        t.text = "0:10";
        t.fontSize = 60;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }

    private static Text MakeTitle(string name, string text, Transform parent, Vector2 pos, int fontSize, Color color)
    {
        var go = DefaultControls.CreateText(new DefaultControls.Resources());
        go.name = name;
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900, fontSize + 24);
        rt.anchoredPosition = pos;
        var t = go.GetComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    private static Button MakeButton(string name, string label, Transform parent, DefaultControls.Resources res, Vector2 pos)
    {
        var go = DefaultControls.CreateButton(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(320, 64);
        rt.anchoredPosition = pos;
        var t = go.GetComponentInChildren<Text>();
        t.text = label;
        t.fontSize = 28;
        return go.GetComponent<Button>();
    }
}
