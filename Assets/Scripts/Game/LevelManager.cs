using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int levelNumber;
    public int maxHP;
    public Block.BlockColor weakColor;
    public Block.BlockShape weakShape;
}

public class LevelManager : MonoBehaviour
{
    public LevelData[] levels;

    /// <summary>
    /// 根据关卡号获取对应的关卡数据
    /// 如果关卡号超出范围，返回最后一关的数据
    /// </summary>
    /// <param name="level">关卡号（从1开始）</param>
    /// <returns>对应的 LevelData</returns>
    public LevelData GetLevelData(int level)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogWarning("LevelManager: levels 数组为空！");
            return null;
        }

        if (level <= 0)
        {
            Debug.LogWarning($"LevelManager: 无效关卡号 {level}，应从1开始");
            return levels[0];
        }

        if (level - 1 < levels.Length)
        {
            return levels[level - 1];
        }
        else
        {
            return levels[levels.Length - 1]; // 返回最后一关
        }
    }
}
