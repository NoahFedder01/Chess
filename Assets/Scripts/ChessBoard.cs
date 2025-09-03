using UnityEngine;
using System.Collections.Generic;

public class ChessBoard : MonoBehaviour
{
    [Header("Board Settings")]
    public float squareSize = 1f;
    public Vector3 boardCenter = Vector3.zero;
    
    [Header("Visual Feedback")]
    public Color highlightColor = Color.yellow;
    public Color validMoveColor = Color.green;
    public GameObject highlightPrefab; // Optional: assign a prefab for square highlights
    
    // Board state tracking
    private ChessPiece[,] boardPieces = new ChessPiece[8, 8];
    private List<GameObject> highlightObjects = new List<GameObject>();
    
    // Current dragging state
    private ChessPiece currentDraggingPiece;
    private List<Vector2Int> validMoves = new List<Vector2Int>();

    void Awake()
    {
        // Ensure there's only one chess board
        ChessBoard[] boards = FindObjectsByType<ChessBoard>(FindObjectsSortMode.None);
        if (boards.Length > 1)
        {
            Debug.LogWarning("Multiple ChessBoard instances found. This might cause issues.");
        }
    }

    public Vector2Int WorldToBoardPosition(Vector3 worldPos)
    {
        // Convert world position to board coordinates (0-7, 0-7)
        Vector3 relativePos = worldPos - boardCenter;
        
        int x = Mathf.RoundToInt(relativePos.x + 3.5f);
        int y = Mathf.RoundToInt(relativePos.y + 3.5f);
        
        return new Vector2Int(x, y);
    }

    public Vector3 BoardToWorldPosition(Vector2Int boardPos)
    {
        // Convert board coordinates to world position
        float worldX = boardPos.x - 3.5f;
        float worldY = boardPos.y - 3.5f;
        
        return new Vector3(worldX, worldY, -1f) + boardCenter;
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }

    public bool CanPlacePieceAt(Vector2Int pos, ChessPiece movingPiece)
    {
        if (!IsValidPosition(pos)) return false;
        
        ChessPiece existingPiece = boardPieces[pos.y, pos.x];
        
        // Can place if square is empty
        if (existingPiece == null) return true;
        
        // Can capture if it's an opponent's piece
        if (existingPiece.isWhite != movingPiece.isWhite) return true;
        
        // Cannot place on own piece
        return false;
    }

    public void PlacePieceAt(Vector2Int pos, ChessPiece piece)
    {
        if (!IsValidPosition(pos)) return;
        
        // Remove piece from old position if it was on the board
        Vector2Int oldPos = piece.boardPosition;
        if (IsValidPosition(oldPos))
        {
            boardPieces[oldPos.y, oldPos.x] = null;
        }
        
        // Handle capturing
        ChessPiece capturedPiece = boardPieces[pos.y, pos.x];
        if (capturedPiece != null && capturedPiece != piece)
        {
            // Remove captured piece
            Destroy(capturedPiece.gameObject);
        }
        
        // Place the piece
        boardPieces[pos.y, pos.x] = piece;
        piece.boardPosition = pos;
    }

    public void MovePiece(Vector2Int fromPos, Vector2Int toPos, ChessPiece piece)
    {
        if (!IsValidPosition(fromPos) || !IsValidPosition(toPos)) return;
        
        // Clear the old position
        if (IsValidPosition(fromPos))
        {
            boardPieces[fromPos.y, fromPos.x] = null;
        }
        
        // Handle capturing
        ChessPiece capturedPiece = boardPieces[toPos.y, toPos.x];
        if (capturedPiece != null && capturedPiece != piece)
        {
            Destroy(capturedPiece.gameObject);
            Debug.Log($"{piece.pieceType} captured {capturedPiece.pieceType}");
        }
        
        // Place piece at new position
        boardPieces[toPos.y, toPos.x] = piece;
        piece.boardPosition = toPos;
        
        Debug.Log($"Moved {piece.pieceType} from {fromPos} to {toPos}");
    }

    public ChessPiece GetPieceAt(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return null;
        return boardPieces[pos.y, pos.x];
    }

    public void OnPieceStartDrag(ChessPiece piece)
    {
        currentDraggingPiece = piece;
        
        // Calculate valid moves for this piece (basic implementation)
        CalculateValidMoves(piece);
        
        // Show visual feedback for valid moves
        ShowValidMoves();
    }

    public void OnPieceEndDrag(ChessPiece piece)
    {
        currentDraggingPiece = null;
        validMoves.Clear();
        
        // Hide visual feedback
        HideValidMoves();
    }

    private void CalculateValidMoves(ChessPiece piece)
    {
        validMoves.Clear();
        
        // For now, allow moving to any empty square or capturing any opponent piece
        // Later, you can implement proper chess movement rules here
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (CanPlacePieceAt(pos, piece))
                {
                    validMoves.Add(pos);
                }
            }
        }
    }

    private void ShowValidMoves()
    {
        // Simple implementation: you can enhance this with actual highlight GameObjects
        foreach (Vector2Int move in validMoves)
        {
            Vector3 worldPos = BoardToWorldPosition(move);
            Debug.DrawLine(worldPos + Vector3.left * 0.4f + Vector3.down * 0.4f, 
                          worldPos + Vector3.right * 0.4f + Vector3.up * 0.4f, 
                          validMoveColor, 0.5f);
            Debug.DrawLine(worldPos + Vector3.right * 0.4f + Vector3.down * 0.4f, 
                          worldPos + Vector3.left * 0.4f + Vector3.up * 0.4f, 
                          validMoveColor, 0.5f);
        }
    }

    private void HideValidMoves()
    {
        // Clear any highlight objects if you implement them
        foreach (GameObject highlight in highlightObjects)
        {
            if (highlight != null) Destroy(highlight);
        }
        highlightObjects.Clear();
    }

    // Initialize the board with pieces from a FEN string
    public void InitializeFromFEN(string fen)
    {
        // Clear existing pieces
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (boardPieces[y, x] != null)
                {
                    Destroy(boardPieces[y, x].gameObject);
                    boardPieces[y, x] = null;
                }
            }
        }
        
        // Parse FEN string (simplified version)
        string[] fenParts = fen.Split(' ');
        string boardData = fenParts[0];
        
        int rank = 7; // Start from rank 8 (index 7)
        int file = 0; // Start from file a (index 0)
        
        foreach (char c in boardData)
        {
            if (c == '/')
            {
                rank--;
                file = 0;
            }
            else if (char.IsDigit(c))
            {
                file += (c - '0'); // Skip empty squares
            }
            else
            {
                // Create a piece
                bool isWhite = char.IsUpper(c);
                Vector2Int boardPos = new Vector2Int(file, rank);
                Vector3 worldPos = BoardToWorldPosition(boardPos);
                
                CreatePiece(c, isWhite, boardPos, worldPos);
                file++;
            }
        }
    }

    private void CreatePiece(char pieceChar, bool isWhite, Vector2Int boardPos, Vector3 worldPos)
    {
        GameObject pieceObj = new GameObject($"{(isWhite ? "White" : "Black")} {pieceChar}");
        pieceObj.transform.position = worldPos;
        pieceObj.transform.parent = transform;
        
        ChessPiece piece = pieceObj.AddComponent<ChessPiece>();
        piece.SetPieceData(pieceChar, isWhite, boardPos);
        
        // Place in board array
        boardPieces[boardPos.y, boardPos.x] = piece;
    }

    // Debug method to print board state
    public void PrintBoardState()
    {
        Debug.Log("Current Board State:");
        for (int y = 7; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < 8; x++)
            {
                ChessPiece piece = boardPieces[y, x];
                if (piece != null)
                {
                    row += piece.pieceType + " ";
                }
                else
                {
                    row += ". ";
                }
            }
            Debug.Log($"Rank {y + 1}: {row}");
        }
    }
}
