using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {

    public string FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public TMP_Text fenText;   // assign in Inspector
    private string previousFEN = "";
    
    [Header("Chess System")]
    public ChessBoard chessBoard;
    
    void Awake() {
        // Find or create chess board
        if (chessBoard == null) {
            chessBoard = FindFirstObjectByType<ChessBoard>();
        }
        
        if (chessBoard == null) {
            // Create a chess board GameObject if none exists
            GameObject chessBoardObj = new GameObject("ChessBoard");
            chessBoard = chessBoardObj.AddComponent<ChessBoard>();
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
        // Initialize the chess board with pieces from FEN
        if (chessBoard != null) {
            chessBoard.InitializeFromFEN(FEN);
        }
        UpdateFENDisplay();
    }

    // Update is called once per frame
    void Update() {
        if (FEN != previousFEN) {
            // Re-initialize board if FEN changes
            if (chessBoard != null) {
                chessBoard.InitializeFromFEN(FEN);
            }
            UpdateFENDisplay();
        }
    }

    // Method to update FEN when pieces are moved (can be called by ChessBoard)
    public void UpdateFEN(string newFEN) {
        FEN = newFEN;
    }
}
