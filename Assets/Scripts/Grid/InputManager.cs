using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public GameObject gridManagerPrefab;

    private GridManager gridManager;
    public GridManager GridManager => gridManager;
    private Camera mainCamera;

    private Block selectedBlock;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("InputManager: Main Camera not found!");
        }

        if (gridManagerPrefab != null)
        {
            GameObject gridGO = Instantiate(gridManagerPrefab);
            gridManager = gridGO.GetComponent<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("InputManager: GridManager component not found on prefab!");
            }
            else
            {
                Debug.Log("InputManager: GridManager instantiated successfully.");
            }
        }
        else
        {
            Debug.LogError("InputManager: gridManagerPrefab is not assigned!");
        }
    }

    void Update()
    {
        if (gridManager == null) return;
        if (!gridManager.IsInputEnabled()) return;

        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // 左键点击，选中块
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            Block hitBlock = GetBlockUnderPosition(worldPos);
            if (hitBlock != null)
            {
                Debug.Log($"Left Click: Selected block at grid pos {gridManager.WorldToGridPosition(hitBlock.transform.position)} with Color {hitBlock.Color} and Shape {hitBlock.Shape}");

                // --- 新增：处理选中状态切换 ---
                if (selectedBlock != null && selectedBlock != hitBlock)
                {
                    selectedBlock.SetSelected(false); // 取消上一个选中块颜色加深
                }

                selectedBlock = hitBlock;
                selectedBlock.SetSelected(true); // 颜色变深
            }
            else
            {
                Debug.Log("Left Click: No block selected.");

                // 点击空白处取消选中
                if (selectedBlock != null)
                {
                    selectedBlock.SetSelected(false);
                    selectedBlock = null;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1)) // 右键点击，交换
        {
            if (selectedBlock == null)
            {
                Debug.Log("Right Click: No block is currently selected.");
                return;
            }

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            Vector2Int dir = GetDirectionFromMouseDelta(selectedBlock.transform.position, worldPos);

            if (dir != Vector2Int.zero)
            {
                Debug.Log($"Right Click: Attempting swap from {gridManager.WorldToGridPosition(selectedBlock.transform.position)} in direction {dir}");
                TrySwapSelectedBlock(dir);

                // 交换后取消选中状态
                if (selectedBlock != null)
                {
                    selectedBlock.SetSelected(false);
                    selectedBlock = null;
                }
            }
            else
            {
                Debug.Log("Right Click: No significant direction detected, cancelling selection.");

                if (selectedBlock != null)
                {
                    selectedBlock.SetSelected(false);
                    selectedBlock = null;
                }
            }
        }
    }

    Block GetBlockUnderPosition(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        return hit.collider?.GetComponent<Block>();
    }

    Vector2Int GetDirectionFromMouseDelta(Vector3 from, Vector3 to)
    {
        Vector2 delta = to - from;

        if (delta.magnitude < 0.1f) return Vector2Int.zero;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return new Vector2Int(delta.x > 0 ? 1 : -1, 0);
        else
            return new Vector2Int(0, delta.y > 0 ? -1 : 1);
    }

    void TrySwapSelectedBlock(Vector2Int direction)
    {
        if (!gridManager.IsInputEnabled() || selectedBlock == null) return;

        Vector2Int selectedPos = gridManager.WorldToGridPosition(selectedBlock.transform.position);
        Vector2Int targetPos = selectedPos + direction;

        Block targetBlock = gridManager.GetBlockAtGridPosition(targetPos);
        if (targetBlock != null)
        {
            Debug.Log($"Trying swap: Selected block at {selectedPos} Color:{selectedBlock.Color}, Target block at {targetPos} Color:{targetBlock.Color}");

            gridManager.SwapBlocks(selectedPos, targetPos);

            List<Block> matchedBlocks = gridManager.GetAllMatches();

            Debug.Log($"Matches found after swap: {matchedBlocks.Count}");
            foreach (var b in matchedBlocks)
            {
                Vector2Int pos = gridManager.GetBlockPosition(b);
                Debug.Log($"Matched block at {pos} Color:{b.Color}");
            }

            if (matchedBlocks.Count > 0)
            {
                Debug.Log($"Swap resulted in {matchedBlocks.Count} matches.");
                gridManager.RemoveBlocks(matchedBlocks);
            }
            else
            {
                Debug.Log("Swap did not result in matches, reverting swap.");
                gridManager.SwapBlocks(targetPos, selectedPos);
            }
        }
        else
        {
            Debug.Log("Swap target block is null or out of bounds.");
        }
    }
}
