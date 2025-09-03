using UnityEngine;
using System.Collections.Generic;

// Attach this to an empty GameObject in your scene.
public class AdvancedSpriteRenderer : MonoBehaviour
{
    public static AdvancedSpriteRenderer Instance { get; private set; }

    [Tooltip("Material using 'Sprites/Default' shader.")]
    public Material spriteMaterial;

    public struct SpriteData
    {
        public Sprite sprite;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public override bool Equals(object obj)
        {
            if (!(obj is SpriteData)) return false;
            SpriteData other = (SpriteData)obj;
            return sprite == other.sprite &&
                   position == other.position &&
                   rotation == other.rotation &&
                   scale == other.scale;
        }

        public override int GetHashCode()
        {
            return (sprite, position, rotation, scale).GetHashCode();
        }
    }

    private List<SpriteData> spritesToRender = new List<SpriteData>();
    private List<(Mesh mesh, MaterialPropertyBlock mpb, Matrix4x4 matrix)> cachedRenderData =
        new List<(Mesh, MaterialPropertyBlock, Matrix4x4)>();

    private Mesh quadMesh;
    private bool isDirty = true; // flag to only rebuild when data changes

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (spriteMaterial == null)
            Debug.LogWarning("Sprite Material not assigned. Assign a 'Sprites/Default' material.");
    }

    public void AddSprite(SpriteData data)
    {
        if (data.sprite == null) return;
        spritesToRender.Add(data);
        isDirty = true;
    }

    public void AddSprite(Sprite sprite, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        SpriteData data = new SpriteData
        {
            sprite = sprite,
            position = position,
            rotation = rotation,
            scale = scale
        };
        AddSprite(data);
    }

    public void ClearSprites()
    {
        spritesToRender.Clear();
        cachedRenderData.Clear();
        isDirty = true;
    }

    private void LateUpdate()
    {
        if (spriteMaterial == null || spritesToRender.Count == 0)
            return;

        if (quadMesh == null)
            quadMesh = CreateQuadMesh();

        // Rebuild only if data changed
        if (isDirty)
        {
            cachedRenderData.Clear();

            foreach (var data in spritesToRender)
            {
                if (data.sprite == null) continue;

                float width = data.sprite.rect.width / data.sprite.pixelsPerUnit;
                float height = data.sprite.rect.height / data.sprite.pixelsPerUnit;
                Vector3 finalScale = Vector3.Scale(new Vector3(width, height, 1), data.scale);

                Matrix4x4 matrix = Matrix4x4.TRS(data.position, data.rotation, finalScale);

                Mesh mesh = Object.Instantiate(quadMesh);
                mesh.uv = GetSpriteUVs(data.sprite);

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetTexture("_MainTex", data.sprite.texture);

                cachedRenderData.Add((mesh, mpb, matrix));
            }

            isDirty = false;
        }

        // Just draw cached data every frame
        foreach (var (mesh, mpb, matrix) in cachedRenderData)
        {
            Graphics.DrawMesh(mesh, matrix, spriteMaterial, 0, null, 0, mpb);
        }
    }

    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Quad";

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0)
        };

        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0)
        };

        mesh.RecalculateNormals();
        return mesh;
    }

    private Vector2[] GetSpriteUVs(Sprite sprite)
    {
        Rect rect = sprite.textureRect;
        Texture tex = sprite.texture;

        float xMin = rect.x / tex.width;
        float yMin = rect.y / tex.height;
        float xMax = (rect.x + rect.width) / tex.width;
        float yMax = (rect.y + rect.height) / tex.height;

        return new Vector2[]
        {
            new Vector2(xMin, yMin),
            new Vector2(xMin, yMax),
            new Vector2(xMax, yMax),
            new Vector2(xMax, yMin)
        };
    }
}
