using UnityEngine;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("引用")]
    public Transform enemyTransform;      // 敌人在场景中的Transform
    public RectTransform healthBarRect;   // HealthBar的RectTransform (UI)
    public Canvas canvas;                 // HealthBar所属的Canvas（Screen Space类型）

    [Header("位置偏移")]
    public Vector2 offset = new Vector2(0, -50); // 偏移量，控制血条相对于敌人的位置

    void LateUpdate()
    {
        if (enemyTransform == null || healthBarRect == null || canvas == null)
            return;

        // 敌人世界坐标转换为屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemyTransform.position);

        // 判断是否在摄像机前方
        if (screenPos.z < 0)
        {
            // 敌人在摄像机后面，隐藏血条
            healthBarRect.gameObject.SetActive(false);
            return;
        }
        else
        {
            healthBarRect.gameObject.SetActive(true);
        }

        // 将屏幕坐标转为Canvas本地坐标
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.worldCamera,
            out localPoint);

        // 赋值加偏移
        healthBarRect.localPosition = localPoint + offset;
    }
}
