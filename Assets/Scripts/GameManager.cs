using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {

    public char[,] boardState = new char[8, 8];
    public string FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public Sprite[] chessPieces;

    public TMP_Text fenText;   // assign in Inspector
    private string previousFEN = "";

    void InitializeBoardState() {
        int FENIndex = 0, boardStateIndex = 0;
        char curr = FEN[0];
        while (curr != ' ') {
            if (curr == '/') {
                FENIndex++;
                curr = FEN[FENIndex];
                continue;
            }
            if (curr > 47 && curr < 58) {
                int n = curr - 48;
                while (n > 0) {
                    boardState[7 - boardStateIndex / 8, boardStateIndex % 8] = ' ';
                    boardStateIndex++;
                    n--;
                }
                FENIndex++;
                curr = FEN[FENIndex];
                continue;
            }
            boardState[7 - boardStateIndex / 8, boardStateIndex % 8] = curr;
            FENIndex++;
            curr = FEN[FENIndex];
            boardStateIndex++;
        }
    }

    void GeneratePiece(string piece, Vector3 position) {
        Sprite myPiece = Resources.Load<Sprite>(piece);
        Vector3 scale = new Vector3(1f, 1f, 0f);

        if (AdvancedSpriteRenderer.Instance == null) {
            Debug.Log("Instance not found");
        }

        AdvancedSpriteRenderer.Instance.AddSprite(
            myPiece,
            position,
            Quaternion.identity,
            scale
        );
    }

    string GivePNGFile(char pType) {
        // Debug.Log("GivePNGFile function recieved char = '" + pType + "'");
        switch (pType) {
            case 'p':
                return "black-pawn";
            case 'n':
                return "black-knight";
            case 'b':
                return "black-bishop";
            case 'r':
                return "black-rook";
            case 'q':
                return "black-queen";
            case 'k':
                return "black-king";
            case 'P':
                return "white-pawn";
            case 'N':
                return "white-knight";
            case 'B':
                return "white-bishop";
            case 'R':
                return "white-rook";
            case 'Q':
                return "white-queen";
            default:
                return "white-king";
        }
    }

    void RenderPieces() {
        for (int row = 0; row < 8; row++) {
            for (int col = 0; col < 8; col++) {
                char pType = boardState[row, col];
                // Debug.Log("Board state for (" + (row) + ", " + (col) + ") = " + "'" + boardState[row, col] + "'");
                if (pType == ' ') continue;
                string pngFile = GivePNGFile(pType);
                Vector3 location = new Vector3(col - 3.5f, row - 3.5f, -1f);
                // Debug.Log("Loading piece: " + pngFile + " derived from pType = '" + pType + "' at location (" + (col + 1) + ", " + (row + 1) + ")");
                GeneratePiece(pngFile, location);
            }
        }
    }

    public void UpdateFENDisplay() {
        if (fenText == null) return;

        fenText.text = "FEN: " + FEN;
        previousFEN = FEN;
        fenText.overflowMode = TextOverflowModes.Overflow;
        fenText.textWrappingMode = TextWrappingModes.NoWrap;
        fenText.alignment = TextAlignmentOptions.Center;
        // Keep X centered, adjust Y
        RectTransform rt = fenText.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, 480f);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        InitializeBoardState();
        RenderPieces();
        UpdateFENDisplay();
    }

    // Update is called once per frame
    void Update() {
        if (FEN != previousFEN) {
            UpdateFENDisplay();
        }
    }
}
