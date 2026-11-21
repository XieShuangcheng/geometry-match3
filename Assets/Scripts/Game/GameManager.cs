using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏配置")]
    public int baseDamagePerBlock = 5;
    public float colorMatchBonus = 0.5f; // 50%加成
    public float shapeMatchBonus = 0.5f; // 50%加成

    [Header("引用")]
    public InputManager inputManager;    // 新增对 InputManager 的引用
    public Enemy currentEnemy;
    public UIManager uiManager;            // 只引用 UIManager
    public LevelManager levelManager;

    private int currentLevel = 1;
    private int totalScore = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;
    }

    void Start()
    {
        StartNewLevel(currentLevel);

        if (uiManager != null)
        {
            uiManager.UpdateLevelDisplay(currentLevel);
        }
    }

    public void StartNewLevel()
    {
        StartNewLevel(currentLevel);
    }

    public void StartNewLevel(int level)
    {
        currentLevel = level;

        currentEnemy = GenerateNewEnemy(level);

        GridManager gridManager = GetGridManager();
        if (gridManager != null)
        {
            StartCoroutine(EnsureGridReady(gridManager));
        }
        else
        {
            Debug.LogWarning("GameManager: 无法获取 GridManager 实例（请检查 InputManager 是否正确赋值）");
        }

        if (uiManager != null)
        {
            uiManager.UpdateLevelDisplay(currentLevel);
        }
    }

    private IEnumerator EnsureGridReady(GridManager gridManager)
    {
        gridManager.gameObject.SetActive(true);
        yield return new WaitUntil(() => gridManager.isActiveAndEnabled);
        gridManager.ResetGrid();
    }

    public void OnBlocksMatched(List<Block> matchedBlocks)
    {
        if (currentEnemy == null)
        {
            Debug.LogWarning("当前敌人不存在，无法造成伤害");
            return;
        }

        int totalDamage = 0;

        foreach (var block in matchedBlocks)
        {
            float multiplier = currentEnemy.GetBonusMultiplier(block);
            int damage = Mathf.RoundToInt(baseDamagePerBlock * multiplier);
            totalDamage += damage;
        }

        currentEnemy.TakeDamage(totalDamage);

        // 改成调用 UIManager 的 ShowDamage(int) 方法（显示总伤害）
        if (uiManager != null)
        {
            uiManager.ShowDamage(totalDamage);
        }

        if (currentEnemy.CurrentHP <= 0)
        {
            LevelCompleted();
        }
    }


    void LevelCompleted()
    {
        totalScore += currentLevel * 100;
        currentLevel++;

        Invoke(nameof(StartNewLevel), 2f);
    }

    private Enemy GenerateNewEnemy(int level)
    {
        if (levelManager == null)
        {
            Debug.LogError("LevelManager 未赋值，无法生成敌人");
            return null;
        }

        LevelData levelData = levelManager.GetLevelData(level);
        if (levelData == null)
        {
            Debug.LogError("无法获取关卡数据，生成默认敌人");
            return null;
        }

        GameObject enemyObj = Instantiate(Resources.Load<GameObject>("Prefabs/Enemy"));
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        enemy.MaxHP = levelData.maxHP;
        enemy.CurrentHP = levelData.maxHP;
        enemy.WeakColor = levelData.weakColor;
        enemy.WeakShape = levelData.weakShape;
        enemy.UpdateAppearance();

        return enemy;
    }

    private GridManager GetGridManager()
    {
        if (inputManager == null)
        {
            Debug.LogWarning("GameManager: InputManager 未赋值");
            return null;
        }

        if (inputManager.GridManager == null)
        {
            Debug.LogWarning("GameManager: InputManager 中的 GridManager 未实例化或赋值");
            return null;
        }

        return inputManager.GridManager;
    }

}
