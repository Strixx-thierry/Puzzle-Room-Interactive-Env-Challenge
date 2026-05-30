using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Simple grid-maze puzzle. A token starts at 'S' and the player guides it to 'E' using
/// WASD / arrow keys or the four on-screen arrow buttons. Walls ('#') block movement.
///
///   • Open()                         — shows the panel, resets the token, freezes the player.
///   • MoveUp/Down/Left/Right()       — wire the four arrow buttons (keys also work).
///   • Reaching 'E' -> onSolved (wire to ChapterClue.Reveal — the reward).
///
/// The maze is drawn from <see cref="layout"/> (one string per row): '#' wall, '.' floor,
/// 'S' start, 'E' exit. Self-contained, non-text. Put this on the always-active Canvas;
/// <see cref="canvasRoot"/> is the child panel toggled, <see cref="gridRoot"/> holds the cells.
/// </summary>
public class MazePuzzle : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Child panel toggled on/off. Leave THIS script on the always-active Canvas.")]
    public GameObject canvasRoot;

    public Text titleText;
    public Text statusText;

    [Tooltip("Empty RectTransform that the maze cells + token are drawn into (centred).")]
    public RectTransform gridRoot;

    [Header("Maze")]
    [Tooltip("One string per row. '#'=wall, '.'=floor, 'S'=start, 'E'=exit. Rows same length.")]
    public string[] layout =
    {
        "S.....#",
        "#####.#",
        "#.....#",
        "#.#####",
        "#......",
        "######.",
        "......E",
    };

    public float cellSize = 64f;
    public Color wallColor = new Color(0.08f, 0.08f, 0.10f);
    public Color floorColor = new Color(0.30f, 0.30f, 0.36f);
    public Color goalColor = new Color(0.30f, 0.75f, 0.40f);
    public Color tokenColor = new Color(0.35f, 0.65f, 1f);

    [Header("Player control")]
    public Behaviour[] disableWhileOpen;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Order gate (optional)")]
    public PuzzleManager puzzleManager;
    public int puzzleIndex = 3;
    public bool requirePrevious = false;

    [Header("Events")]
    public UnityEvent onSolved;
    public UnityEvent onWrong;

    RectTransform token;
    int rows, cols;
    int startCol, startRow, goalCol, goalRow;
    int col, row;
    bool built;
    bool solved;
    bool isOpen;

    void Awake()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
        EnsureBuilt();
    }

    void Update()
    {
        if (!isOpen || solved) return;
        if (Input.GetKeyDown(closeKey)) { Close(); return; }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) TryMove(0, -1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) TryMove(0, 1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) TryMove(-1, 0);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) TryMove(1, 0);
    }

    /// <summary>Wire the prop's Interactable.onInteract to this.</summary>
    public void Open()
    {
        if (solved) return;
        if (requirePrevious && puzzleManager != null && puzzleIndex > 0 &&
            !puzzleManager.IsSolved(puzzleIndex - 1)) return;

        EnsureBuilt();
        if (canvasRoot != null) canvasRoot.SetActive(true);
        isOpen = true;
        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        col = startCol; row = startRow;
        PlaceToken();
        if (statusText != null) statusText.text = "Reach the green exit. (WASD / arrows)";
    }

    public void Close()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
        isOpen = false;
        SetPlayerControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void MoveUp() { TryMove(0, -1); }
    public void MoveDown() { TryMove(0, 1); }
    public void MoveLeft() { TryMove(-1, 0); }
    public void MoveRight() { TryMove(1, 0); }

    void TryMove(int dx, int dy)
    {
        if (!isOpen || solved) return;
        int nc = col + dx, nr = row + dy;
        if (nr < 0 || nr >= rows || nc < 0 || nc >= cols) return;
        if (CellAt(nr, nc) == '#') return;

        col = nc; row = nr;
        PlaceToken();

        if (row == goalRow && col == goalCol)
        {
            solved = true;
            if (statusText != null) statusText.text = "Unlocked!";
            Close();                 // restore control first...
            onSolved?.Invoke();      // ...then the chapter overlay grabs the cursor
        }
    }

    void EnsureBuilt()
    {
        if (built || gridRoot == null || layout == null || layout.Length == 0) return;
        built = true;

        rows = layout.Length;
        cols = 0;
        foreach (var r in layout) cols = Mathf.Max(cols, r.Length);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                char ch = CellAt(r, c);
                var cell = new GameObject("cell_" + r + "_" + c, typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(gridRoot, false);
                var rt = cell.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cellSize - 4, cellSize - 4);
                rt.anchoredPosition = CellPos(c, r);
                cell.GetComponent<Image>().color =
                    ch == '#' ? wallColor : (ch == 'E' ? goalColor : floorColor);

                if (ch == 'S') { startCol = c; startRow = r; }
                if (ch == 'E') { goalCol = c; goalRow = r; }
            }
        }

        // token on top
        var t = new GameObject("Token", typeof(RectTransform), typeof(Image));
        t.transform.SetParent(gridRoot, false);
        token = t.GetComponent<RectTransform>();
        token.anchorMin = token.anchorMax = new Vector2(0.5f, 0.5f);
        token.sizeDelta = new Vector2(cellSize * 0.6f, cellSize * 0.6f);
        t.GetComponent<Image>().color = tokenColor;
        t.transform.SetAsLastSibling();

        col = startCol; row = startRow;
        PlaceToken();
    }

    char CellAt(int r, int c)
    {
        if (r < 0 || r >= layout.Length) return '#';
        string line = layout[r];
        if (c < 0 || c >= line.Length) return '#';
        return line[c];
    }

    Vector2 CellPos(int c, int r)
    {
        float x = (c - (cols - 1) / 2f) * cellSize;
        float y = ((rows - 1) / 2f - r) * cellSize;
        return new Vector2(x, y);
    }

    void PlaceToken()
    {
        if (token != null) token.anchoredPosition = CellPos(col, row);
    }

    void SetPlayerControl(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b != null) b.enabled = enabled;
    }
}
