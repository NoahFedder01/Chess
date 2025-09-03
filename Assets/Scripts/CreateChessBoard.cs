using UnityEngine;

public class CreateChessBoard : MonoBehaviour
{
    public Color32 darkColor = new Color32(159, 83, 18, 255),
        lightColor = new Color32(255, 209, 126, 255);

    [Header("Board Settings")]
    public float squareSize = 1f;
    public int boardLayer = 0; // Layer for board squares (behind pieces)

    void CreateSquare(Color32 color, Vector2 position) {
        // Create a 1x1 texture with the given color
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        // Create a sprite from the texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Create a GameObject for this square
        GameObject square = new GameObject($"Square_{position.x}_{position.y}");
        square.transform.parent = transform;
        square.transform.position = new Vector3(position.x, position.y, 0f);
        
        // Add SpriteRenderer
        SpriteRenderer sr = square.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = boardLayer;
        
        // Scale the square
        square.transform.localScale = Vector3.one * squareSize;
    }

    void RenderChessBoard() {
        for (int row = 0; row < 8; row++) {
            for (int col = 0; col < 8; col++) {
                bool isLightSquare = (row + col) % 2 == 1;
                Color32 color = isLightSquare ? lightColor : darkColor;
                Vector2 myVec = new Vector2(col - 3.5f, row - 3.5f);
                CreateSquare(color, myVec);
            }
        }
    }

    void Start() {
        RenderChessBoard();
    }
}