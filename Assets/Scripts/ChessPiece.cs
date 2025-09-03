using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    [Header("Piece Data")]
    public char pieceType;
    public bool isWhite;
    public Vector2Int boardPosition;
    
    [Header("Dragging")]
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Camera mainCamera;
    private Vector3 originalPosition;
    private int originalSortingOrder;
    
    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    
    [Header("Visual Feedback")]
    public float dragScale = 1.1f;
    public float snapDistance = 0.5f;
    
    private ChessBoard chessBoard;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        mainCamera = Camera.main;
        
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // Find the chess board in the scene
        chessBoard = FindFirstObjectByType<ChessBoard>();
    }

    void Start()
    {
        originalSortingOrder = spriteRenderer.sortingOrder;
        
        // Set up the collider to match the sprite
        if (spriteRenderer.sprite != null)
        {
            boxCollider.size = spriteRenderer.sprite.bounds.size;
        }
    }

    void OnMouseDown()
    {
        if (mainCamera == null) return;
        
        isDragging = true;
        originalPosition = transform.position;
        
        // Calculate offset between mouse and piece center
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        dragOffset = transform.position - mouseWorldPos;
        
        // Visual feedback - bring to front and scale up
        spriteRenderer.sortingOrder = 100;
        transform.localScale = Vector3.one * dragScale;
        
        // Notify chess board that a piece is being dragged
        if (chessBoard != null)
        {
            chessBoard.OnPieceStartDrag(this);
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging || mainCamera == null) return;
        
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        transform.position = mouseWorldPos + dragOffset;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        // Reset visual feedback
        spriteRenderer.sortingOrder = originalSortingOrder;
        transform.localScale = Vector3.one;
        
        // Try to snap to the nearest square
        if (chessBoard != null)
        {
            Vector2Int newBoardPos = chessBoard.WorldToBoardPosition(transform.position);
            
            if (chessBoard.IsValidPosition(newBoardPos) && 
                chessBoard.CanPlacePieceAt(newBoardPos, this))
            {
                // Move to the new position
                Vector3 newWorldPos = chessBoard.BoardToWorldPosition(newBoardPos);
                transform.position = newWorldPos;
                
                // Update board state
                chessBoard.MovePiece(boardPosition, newBoardPos, this);
                boardPosition = newBoardPos;
            }
            else
            {
                // Invalid move - return to original position
                transform.position = originalPosition;
            }
            
            chessBoard.OnPieceEndDrag(this);
        }
        else
        {
            // No chess board found - return to original position
            transform.position = originalPosition;
        }
    }

    public void SetPieceData(char type, bool white, Vector2Int position)
    {
        pieceType = type;
        isWhite = white;
        boardPosition = position;
        
        // Set the sprite based on piece type
        string spriteName = GetSpriteNameFromType(type, white);
        Sprite pieceSprite = Resources.Load<Sprite>(spriteName);
        
        if (pieceSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = pieceSprite;
            
            // Update collider size
            if (boxCollider != null)
            {
                boxCollider.size = pieceSprite.bounds.size;
            }
        }
        else
        {
            Debug.LogError($"Could not load sprite: {spriteName}");
        }
    }

    private string GetSpriteNameFromType(char type, bool white)
    {
        string colorPrefix = white ? "white" : "black";
        string pieceName = "";
        
        switch (char.ToLower(type))
        {
            case 'p': pieceName = "pawn"; break;
            case 'r': pieceName = "rook"; break;
            case 'n': pieceName = "knight"; break;
            case 'b': pieceName = "bishop"; break;
            case 'q': pieceName = "queen"; break;
            case 'k': pieceName = "king"; break;
            default: pieceName = "pawn"; break;
        }
        
        return $"{colorPrefix}-{pieceName}";
    }

    public void SetPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        originalPosition = worldPosition;
    }

    // Helper method to check if this piece belongs to the current player
    public bool IsCurrentPlayerPiece()
    {
        // For now, we'll allow moving any piece
        // Later you can add turn-based logic here
        return true;
    }
}
