using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("关卡文本组件")]
    public TextMeshProUGUI levelText;

    [Header("敌人伤害显示")]
    public TextMeshProUGUI enemyDamageText;  // 敌人血量旁显示伤害数字的文本
    public float damageDisplayDuration = 2f; // 伤害数字显示持续时间

    private int accumulatedDamage = 0;
    private float damageDisplayTimer = 0f;
    private bool isShowingDamage = false;

    public void UpdateLevelDisplay(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
        else
        {
            Debug.LogWarning("UIManager: levelText 未绑定！");
        }
    }

    /// <summary>
    /// 累积并显示敌人伤害数字
    /// </summary>
    /// <param name="damage">本次造成的伤害</param>
    public void ShowDamage(int damage)
    {
        accumulatedDamage += damage;
        UpdateDamageText();

        damageDisplayTimer = damageDisplayDuration;
        isShowingDamage = true;
        enemyDamageText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (isShowingDamage)
        {
            damageDisplayTimer -= Time.deltaTime;
            if (damageDisplayTimer <= 0f)
            {
                ClearDamage();
            }
        }
    }

    private void UpdateDamageText()
    {
        if (enemyDamageText != null)
        {
            enemyDamageText.text = $"-{accumulatedDamage}";
        }
    }

    public void ClearDamage()
    {
        accumulatedDamage = 0;
        isShowingDamage = false;

        if (enemyDamageText != null)
        {
            enemyDamageText.text = "";
            enemyDamageText.gameObject.SetActive(false);
        }
    }
}
