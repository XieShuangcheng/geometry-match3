using UnityEngine;

public class Enemy : MonoBehaviour
{
    private const float HP_HIGH = 0.6f;
    private const float HP_MID = 0.3f;

    [Header("属性弱点")]
    public Block.BlockColor WeakColor;
    public Block.BlockShape WeakShape;

    [Header("血量设置")]
    public int MaxHP = 100;
    public int CurrentHP;

    [Header("血量条 UI")]
    public HealthBar healthBar;

    [Header("表情图像（SpriteRenderer）")]
    public SpriteRenderer faceSpriteRenderer;  // ← 替代 Image
    public Sprite[] faceSprites;               // 多个阶段表情（建议3张）

    [Header("下层图像")]
    public SpriteRenderer weaknessRenderer;     // 显示敌人弱点属性（色块）
    public Block.SpriteKey[] spriteMappings;    // 与 Block 共享的颜色+形状映射

    private int currentFaceIndex = -1;

    // 敌人死亡事件，外部可订阅
    public event System.Action OnDeath;

    void Start()
    {
        CurrentHP = MaxHP;

        if (healthBar != null)
        {
            healthBar.SetHP(MaxHP);
            healthBar.UpdateHP(CurrentHP);
        }

        UpdateAppearance();
        SetFace(0); // 初始默认表情
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);

        if (healthBar != null)
        {
            healthBar.UpdateHP(CurrentHP);
        }

        UpdateFaceByHP();

        if (CurrentHP == 0)
        {
            Die();
            OnDeath?.Invoke();
        }
    }

    public float GetBonusMultiplier(Block block)
    {
        float bonus = 1f;
        if (block.Color == WeakColor) bonus *= 1.5f;
        if (block.Shape == WeakShape) bonus *= 1.5f;
        return bonus;
    }

    void UpdateFaceByHP()
    {
        if (faceSprites == null || faceSprites.Length == 0 || faceSpriteRenderer == null) return;

        int faceIndex;
        float hpRatio = (float)CurrentHP / MaxHP;

        if (hpRatio > HP_HIGH) faceIndex = 0;      // 正常
        else if (hpRatio > HP_MID) faceIndex = 1; // 受伤
        else faceIndex = 2;                        // 危险

        if (faceIndex != currentFaceIndex)
        {
            SetFace(faceIndex);
        }
    }

    public void SetFace(int index)
    {
        if (faceSprites == null || faceSprites.Length == 0 || faceSpriteRenderer == null) return;

        index = Mathf.Clamp(index, 0, faceSprites.Length - 1);
        faceSpriteRenderer.sprite = faceSprites[index];
        currentFaceIndex = index;
    }

    /// <summary>
    /// 根据弱点颜色+形状显示下半部分色块图像
    /// </summary>
    public void UpdateAppearance()
    {
        if (weaknessRenderer == null || spriteMappings == null || spriteMappings.Length == 0)
        {
            Debug.LogWarning("Enemy: 缺少 weaknessRenderer 或 spriteMappings，无法更新外观");
            return;
        }

        foreach (var mapping in spriteMappings)
        {
            if (mapping.color == WeakColor && mapping.shape == WeakShape)
            {
                weaknessRenderer.sprite = mapping.sprite;
                return;
            }
        }

        Debug.LogWarning($"Enemy: 未找到匹配图像: {WeakColor} + {WeakShape}");
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 被击败！");
        gameObject.SetActive(false);
        // TODO: 可加入死亡动画或粒子效果
    }
}
