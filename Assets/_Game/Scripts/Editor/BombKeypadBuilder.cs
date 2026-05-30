#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;

/// <summary>
/// One-click builder for the bomb keypad. Run: Puzzle Room -> Build Bomb Keypad.
/// Creates a canvas with a display, a feedback line, and buttons 1-9 / Clear / 0 / Enter,
/// adds a KeypadController (code 4729, 3 attempts), and wires every button.
/// You then: point the bomb's Interactable.onInteract at KeypadController.Open(),
/// and wire onSolved / onOutOfAttempts to your win / lose.
/// </summary>
public static class BombKeypadBuilder
{
    [MenuItem("Puzzle Room/Build Bomb Keypad")]
    public static void Build()
    {
        DeleteExisting("Bomb Keypad Canvas");

        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf");
        if (font == null) font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        // Canvas
        var canvasGO = new GameObject("Bomb Keypad Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(KeypadController));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 210;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Dark full-screen panel (the root the controller toggles)
        var panel = NewUI("Keypad Panel", canvasGO.transform);
        Stretch(panel.GetComponent<RectTransform>());
        panel.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.03f, 0.92f);

        // Center box
        var box = NewUI("Box", panel.transform);
        var boxRT = box.GetComponent<RectTransform>();
        boxRT.anchorMin = boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.sizeDelta = new Vector2(560, 760);
        box.AddComponent<Image>().color = new Color(0.10f, 0.10f, 0.12f, 1f);

        // Title
        var title = NewText("Title", box.transform, font, "ENTER DISARM CODE", 30, FontStyle.Bold,
            new Color(1f, 0.4f, 0.3f), TextAnchor.MiddleCenter);
        Place(title.rectTransform, 0, 320, 520, 50);

        // Display
        var display = NewText("Display", box.transform, font, "____", 60, FontStyle.Bold,
            Color.white, TextAnchor.MiddleCenter);
        Place(display.rectTransform, 0, 250, 520, 80);

        // Feedback
        var feedback = NewText("Feedback", box.transform, font, "", 28, FontStyle.Normal,
            new Color(1f, 0.8f, 0.3f), TextAnchor.MiddleCenter);
        Place(feedback.rectTransform, 0, 190, 520, 40);

        // Controller
        var kp = canvasGO.GetComponent<KeypadController>();
        kp.canvasRoot = panel;
        kp.displayText = display;
        kp.feedbackText = feedback;
        kp.correctCode = "4729";
        kp.maxLength = 4;
        kp.maxAttempts = 3;
        kp.disableWhileOpen = FindPlayerBehaviours();

        // Button grid: 1-9, Clear, 0, Enter
        string[] labels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "Clear", "0", "Enter" };
        float bw = 150, bh = 110, gapX = 20, gapY = 18;
        float startX = -(bw + gapX);           // 3 columns centred
        float startY = 60;
        for (int i = 0; i < labels.Length; i++)
        {
            int col = i % 3, row = i / 3;
            float x = startX + col * (bw + gapX);
            float y = startY - row * (bh + gapY);
            MakeButton(box.transform, font, labels[i], x, y, bw, bh, kp);
        }

        Undo.RegisterCreatedObjectUndo(canvasGO, "Build Bomb Keypad");
        Selection.activeGameObject = canvasGO;
        Debug.Log("[BombKeypadBuilder] Built 'Bomb Keypad Canvas' (code 4729, 3 attempts). " +
                  "Wire the bomb's Interactable.onInteract -> KeypadController.Open(), and " +
                  "KeypadController.onSolved -> your win, onOutOfAttempts -> your lose.");
    }

    static void MakeButton(Transform parent, Font font, string label, float x, float y,
        float w, float h, KeypadController kp)
    {
        var go = NewUI("Btn " + label, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);

        bool isEnter = label == "Enter";
        bool isClear = label == "Clear";
        Color c = isEnter ? new Color(0.3f, 0.7f, 0.4f)
                : isClear ? new Color(0.7f, 0.4f, 0.4f)
                : new Color(0.25f, 0.25f, 0.30f);
        go.AddComponent<Image>().color = c;
        var btn = go.AddComponent<Button>();

        var t = NewText("Text", go.transform, font, label, isEnter || isClear ? 30 : 44,
            FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        Stretch(t.rectTransform);

        if (isEnter)
            UnityEventTools.AddPersistentListener(btn.onClick, kp.Submit);
        else if (isClear)
            UnityEventTools.AddPersistentListener(btn.onClick, kp.Clear);
        else
            UnityEventTools.AddIntPersistentListener(btn.onClick, kp.PressDigit, int.Parse(label));
    }

    static void DeleteExisting(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Undo.DestroyObjectImmediate(go);
    }

    static Behaviour[] FindPlayerBehaviours()
    {
        var list = new List<Behaviour>();
        var look = Object.FindFirstObjectByType<FirstPersonLook>();
        var move = Object.FindFirstObjectByType<FirstPersonMovement>();
        var inter = Object.FindFirstObjectByType<PlayerInteractor>();
        if (look != null) list.Add(look);
        if (move != null) list.Add(move);
        if (inter != null) list.Add(inter);
        return list.ToArray();
    }

    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static Text NewText(string name, Transform parent, Font font, string content, int size,
        FontStyle style, Color color, TextAnchor align)
    {
        var go = NewUI(name, parent);
        var t = go.AddComponent<Text>();
        t.font = font; t.text = content; t.fontSize = size; t.fontStyle = style;
        t.color = color; t.alignment = align;
        return t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);
    }
}
#endif
