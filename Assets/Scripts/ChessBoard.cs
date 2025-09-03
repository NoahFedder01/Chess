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
    
    [Header("Game State")]
    public bool whiteToMove = true; // Track whose turn it is
    public bool whiteCanCastleKingside = true;
    public bool whiteCanCastleQueenside = true;
    public bool blackCanCastleKingside = true;
    public bool blackCanCastleQueenside = true;
    public Vector2Int enPassantTarget = new Vector2Int(-1, -1); // -1,-1 means no en passant
    public int halfmoveClock = 0; // Moves since last capture or pawn move
    public int fullmoveNumber = 1; // Increments after black's move
    
    // Board state tracking
    private ChessPiece[,] boardPieces = new ChessPiece[8, 8];
    private List<GameObject> highlightObjects = new List<GameObject>();
    
    // Current dragging state
    private ChessPiece currentDraggingPiece;
    private List<Vector2Int> validMoves = new List<Vector2Int>();
    
    // Reference to GameManager for FEN updates
    private GameManager gameManager;

    void Awake()
    {
        // Ensure there's only one chess board
        ChessBoard[] boards = FindObjectsByType<ChessBoard>(FindObjectsSortMode.None);
        if (boards.Length > 1)
        {
            Debug.LogWarning("Multiple ChessBoard instances found. This might cause issues.");
        }
        
        // Find the GameManager
        gameManager = FindFirstObjectByType<GameManager>();
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
        
        bool isCapture = false;
        
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
            isCapture = true;
        }
        
        // Place piece at new position
        boardPieces[toPos.y, toPos.x] = piece;
        piece.boardPosition = toPos;
        
        // Update game state
        UpdateGameStateAfterMove(piece, fromPos, toPos, isCapture);
        
        // Generate and update FEN
        string newFEN = GenerateFEN();
        if (gameManager != null)
        {
            gameManager.UpdateFEN(newFEN);
        }
        
        Debug.Log($"Moved {piece.pieceType} from {fromPos} to {toPos}");
        Debug.Log($"New FEN: {newFEN}");
    }

    public ChessPiece GetPieceAt(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return null;
        return boardPieces[pos.y, pos.x];
    }

    public void SetGameManager(GameManager gm)
    {
        gameManager = gm;
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
        
        // Parse FEN string
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
        
        // Parse game state from FEN
        ParseFENGameState(fen);
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

    // Update game state after a move
    private void UpdateGameStateAfterMove(ChessPiece piece, Vector2Int fromPos, Vector2Int toPos, bool isCapture)
    {
        // Reset en passant target
        enPassantTarget = new Vector2Int(-1, -1);
        
        // Handle pawn moves (check for double move for en passant)
        if (char.ToLower(piece.pieceType) == 'p')
        {
            halfmoveClock = 0; // Reset on pawn move
            
            // Check for double pawn move (en passant target)
            int moveDistance = Mathf.Abs(toPos.y - fromPos.y);
            if (moveDistance == 2)
            {
                // Set en passant target square
                int targetRank = piece.isWhite ? fromPos.y + 1 : fromPos.y - 1;
                enPassantTarget = new Vector2Int(fromPos.x, targetRank);
            }
        }
        else if (isCapture)
        {
            halfmoveClock = 0; // Reset on capture
        }
        else
        {
            halfmoveClock++; // Increment for non-pawn, non-capture moves
        }
        
        // Handle castling rights (simplified - just disable if king or rook moves)
        if (piece.pieceType == 'K') // White king
        {
            whiteCanCastleKingside = false;
            whiteCanCastleQueenside = false;
        }
        else if (piece.pieceType == 'k') // Black king
        {
            blackCanCastleKingside = false;
            blackCanCastleQueenside = false;
        }
        else if (piece.pieceType == 'R') // White rook
        {
            if (fromPos.x == 0) whiteCanCastleQueenside = false; // Queenside rook
            if (fromPos.x == 7) whiteCanCastleKingside = false; // Kingside rook
        }
        else if (piece.pieceType == 'r') // Black rook
        {
            if (fromPos.x == 0) blackCanCastleQueenside = false; // Queenside rook
            if (fromPos.x == 7) blackCanCastleKingside = false; // Kingside rook
        }
        
        // Switch turns
        whiteToMove = !whiteToMove;
        
        // Increment fullmove counter after black's move
        if (whiteToMove)
        {
            fullmoveNumber++;
        }
    }

    // Generate FEN string from current board state
    public string GenerateFEN()
    {
        string fen = "";
        
        // 1. Piece placement (ranks 8-1)
        for (int rank = 7; rank >= 0; rank--)
        {
            int emptySquares = 0;
            for (int file = 0; file < 8; file++)
            {
                ChessPiece piece = boardPieces[rank, file];
                if (piece == null)
                {
                    emptySquares++;
                }
                else
                {
                    if (emptySquares > 0)
                    {
                        fen += emptySquares.ToString();
                        emptySquares = 0;
                    }
                    fen += piece.pieceType;
                }
            }
            if (emptySquares > 0)
            {
                fen += emptySquares.ToString();
            }
            if (rank > 0) fen += "/";
        }
        
        // 2. Active color
        fen += whiteToMove ? " w " : " b ";
        
        // 3. Castling availability
        string castling = "";
        if (whiteCanCastleKingside) castling += "K";
        if (whiteCanCastleQueenside) castling += "Q";
        if (blackCanCastleKingside) castling += "k";
        if (blackCanCastleQueenside) castling += "q";
        if (castling == "") castling = "-";
        fen += castling + " ";
        
        // 4. En passant target
        if (enPassantTarget.x >= 0 && enPassantTarget.y >= 0)
        {
            char file = (char)('a' + enPassantTarget.x);
            int rank = enPassantTarget.y + 1;
            fen += file.ToString() + rank.ToString();
        }
        else
        {
            fen += "-";
        }
        
        // 5. Halfmove clock
        fen += " " + halfmoveClock.ToString();
        
        // 6. Fullmove number
        fen += " " + fullmoveNumber.ToString();
        
        return fen;
    }

    // Parse FEN to set game state
    public void ParseFENGameState(string fen)
    {
        string[] parts = fen.Split(' ');
        if (parts.Length >= 6)
        {
            // Active color
            whiteToMove = parts[1] == "w";
            
            // Castling rights
            string castling = parts[2];
            whiteCanCastleKingside = castling.Contains("K");
            whiteCanCastleQueenside = castling.Contains("Q");
            blackCanCastleKingside = castling.Contains("k");
            blackCanCastleQueenside = castling.Contains("q");
            
            // En passant target
            if (parts[3] != "-")
            {
                char file = parts[3][0];
                int rank = int.Parse(parts[3][1].ToString());
                enPassantTarget = new Vector2Int(file - 'a', rank - 1);
            }
            else
            {
                enPassantTarget = new Vector2Int(-1, -1);
            }
            
            // Halfmove clock
            halfmoveClock = int.Parse(parts[4]);
            
            // Fullmove number
            fullmoveNumber = int.Parse(parts[5]);
        }
    }
}
