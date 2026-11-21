using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private const int HP_UNIT = 25;

    [Header("血量格预制体")]
    public GameObject fullUnitPrefab;   // 实心格子
    public GameObject emptyUnitPrefab;  // 空心格子

    private int maxHP;
    private int currentHP;

    private List<GameObject> units = new List<GameObject>();
    private List<Image> unitImages = new List<Image>();

    public void SetHP(int maxHP)
    {
        ClearBar();

        if (fullUnitPrefab == null || emptyUnitPrefab == null)
        {
            Debug.LogError("[HealthBar] fullUnitPrefab 或 emptyUnitPrefab 未设置！");
            return;
        }

        this.maxHP = maxHP;
        this.currentHP = maxHP;

        int count = Mathf.CeilToInt((float)maxHP / HP_UNIT);
        Debug.Log($"[HealthBar] 准备生成血量格数量：{count}");

        for (int i = 0; i < count; i++)
        {
            GameObject unit = Instantiate(fullUnitPrefab, transform);
            RectTransform rt = unit.GetComponent<RectTransform>();
            if (rt != null)
            {
                Debug.Log($"[HealthBar] 血条格 {i} 实例化成功，尺寸: {rt.sizeDelta}, 位置: {rt.anchoredPosition}");
            }
            units.Add(unit);

            Image img = unit.GetComponent<Image>();
            if (img != null)
            {
                unitImages.Add(img);
            }
            else
            {
                Debug.LogWarning($"[HealthBar] 血条格 {i} 缺少 Image 组件！");
                unitImages.Add(null);
            }
        }

        UpdateHP(currentHP);
    }

    public void UpdateHP(int currentHP)
    {
        this.currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        int fullCount = 0;

        if (this.currentHP == 0)
        {
            fullCount = 0; // 血量为0，全空格
        }
        else if (this.currentHP > 75)
        {
            fullCount = 4;
        }
        else if (this.currentHP > 50)
        {
            fullCount = 3;
        }
        else if (this.currentHP > 25)
        {
            fullCount = 2;
        }
        else // currentHP > 0
        {
            fullCount = 1;
        }

        for (int i = 0; i < unitImages.Count; i++)
        {
            if (unitImages[i] == null) continue;

            if (i < fullCount)
            {
                unitImages[i].sprite = fullUnitPrefab.GetComponent<Image>().sprite;
            }
            else
            {
                unitImages[i].sprite = emptyUnitPrefab.GetComponent<Image>().sprite;
            }
        }
    }

    private void ClearBar()
    {
        foreach (var unit in units)
        {
            if (unit != null) Destroy(unit);
        }

        units.Clear();
        unitImages.Clear();
    }
}
