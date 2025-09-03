using UnityEngine;

public class CreateChessBoard : MonoBehaviour
{

    public Color32 darkColor = new Color32(159, 83, 18, 255),
        lightColor = new Color32(255, 209, 126, 255);

    

    public SpriteRenderer mySpriteRenderer;

    

    void CreateSquare(Color32 color, Vector2 position) {
        // Create a 1x1 texture with the given color
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        // Create a sprite from the texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Prepare sprite data for the AdvancedSpriteRenderer
        Vector2 scale = new Vector2(100f, 100f);

        // Add it to the renderer (no new GameObject needed)
        AdvancedSpriteRenderer.Instance.AddSprite(sprite, position, Quaternion.identity, scale);
    }


    
    void RenderChessBoard() {
        for (int row = 0; row < 8; row++) {
            for (int col = 0; col < 8; col++) {
                bool isLightSquare = (row + col) % 2 == 1;
                Color32 color = isLightSquare ? lightColor : darkColor;
                Vector2 myVec = new Vector2(row - 3.5f, col - 3.5f);
                CreateSquare(color, myVec);
            }
        }
    }
    void Start() {
        RenderChessBoard();
    }

    // Update is called once per frame
    void Update() {

    }
}