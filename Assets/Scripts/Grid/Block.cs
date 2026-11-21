using UnityEngine;

public class Block : MonoBehaviour
{
    public enum BlockColor { Red, Green, Blue, Yellow, Pink, Purple }
    public enum BlockShape { Circle, Square, Diamond }

    public BlockColor Color;
    public BlockShape Shape;

    private SpriteRenderer spriteRenderer;

    [System.Serializable]
    public struct SpriteKey
    {
        public BlockColor color;
        public BlockShape shape;
        public Sprite sprite;
    }

    public SpriteKey[] spriteMappings;

    private Color originalColor;
    private bool isSelected = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Init(BlockColor color, BlockShape shape)
    {
        Color = color;
        Shape = shape;
        UpdateSprite();
    }

    void UpdateSprite()
    {
        foreach (var mapping in spriteMappings)
        {
            if (mapping.color == Color && mapping.shape == Shape)
            {
                spriteRenderer.sprite = mapping.sprite;
                return;
            }
        }

        Debug.LogWarning($"No sprite found for color {Color} shape {Shape}");
    }

    /// <summary>
    /// 设置选中状态，选中时颜色加深，取消选中恢复原色
    /// </summary>
    /// <param name="selected"></param>
    public void SetSelected(bool selected)
    {
        if (spriteRenderer == null) return;

        if (selected && !isSelected)
        {
            spriteRenderer.color = originalColor * 0.7f; // 颜色变暗70%
            isSelected = true;
        }
        else if (!selected && isSelected)
        {
            spriteRenderer.color = originalColor; // 恢复原色
            isSelected = false;
        }
    }
}
