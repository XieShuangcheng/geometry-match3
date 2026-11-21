using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 6;
    public int columns = 10;
    public float spacing = 1.1f;

    [Header("Prefabs and Assets")]
    public GameObject blockPrefab;
    public Block.SpriteKey[] spriteMappings;
    public ParticleSystem destroyEffect;
    public AudioClip destroySound;
    public AudioClip swapSound;

    [Header("Object Pool")]
    public int initialPoolSize = 100;
    public bool useObjectPool = true;

    [Header("Performance")]
    public float blockMoveDuration = 0.15f;
    public float blockSpawnDelay = 0.02f;
    public float matchCheckDelay = 0.1f;

    [Header("普通容器（非持久化物体，存放生成的色块）")]
    public Transform blocksContainer;

    private Block[,] grid;
    private Queue<Block> blockPool = new Queue<Block>();
    private AudioSource audioSource;
    private bool isProcessingMatches = false;
    private bool isInputEnabled = true;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (useObjectPool)
            InitializeBlockPool();
    }

    void Start()
    {
        Debug.Log("GridManager Start(), isActiveAndEnabled = " + isActiveAndEnabled);
        GenerateGrid();
    }

    public bool IsInputEnabled() => isInputEnabled && !isProcessingMatches;
    public void SetInputEnabled(bool enabled) => isInputEnabled = enabled;

    public void ResetGrid()
    {
        Debug.Log("ResetGrid() called, isActiveAndEnabled = " + isActiveAndEnabled);
        if (grid != null)
        {
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                {
                    if (grid[y, x] != null)
                    {
                        if (useObjectPool)
                            ReturnBlockToPool(grid[y, x]);
                        else
                            Destroy(grid[y, x].gameObject);
                        grid[y, x] = null;
                    }
                }
        }
        GenerateGrid();
    }

    void InitializeBlockPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
            CreatePooledBlock();
    }

    void CreatePooledBlock()
    {
        GameObject go = Instantiate(blockPrefab, Vector3.zero, Quaternion.identity, blocksContainer);
        EnsureCollider(go);
        go.SetActive(false);
        Block block = go.GetComponent<Block>();
        block.spriteMappings = spriteMappings;
        blockPool.Enqueue(block);
    }

    public Block GetPooledBlock(Block.BlockColor color, Block.BlockShape shape)
    {
        Block block;

        if (blockPool.Count > 0)
        {
            block = blockPool.Dequeue();
        }
        else
        {
            GameObject go = Instantiate(blockPrefab, Vector3.zero, Quaternion.identity, blocksContainer);
            EnsureCollider(go);
            block = go.GetComponent<Block>();
        }

        block.spriteMappings = spriteMappings;
        block.Init(color, shape);
        block.gameObject.SetActive(true);
        return block;
    }


    void EnsureCollider(GameObject go)
    {
        if (go.GetComponent<BoxCollider2D>() == null)
        {
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);
            collider.offset = Vector2.zero;
        }
    }

    public void ReturnBlockToPool(Block block)
    {
        block.gameObject.SetActive(false);
        blockPool.Enqueue(block);
    }

    void GenerateGrid()
    {
        grid = new Block[rows, columns];
        Vector3 startPos = CalculateGridStartPosition();

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("GenerateGrid called while GridManager is inactive, aborting.");
            return;
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector3 spawnPos = startPos + new Vector3(x * spacing, -y * spacing, 0);

                Block newBlock = GetPooledBlock(RandomColor(), RandomShape());
                newBlock.transform.position = spawnPos;
                grid[y, x] = newBlock;
            }
        }

        StartCoroutine(CheckInitialMatches());
    }

    IEnumerator CheckInitialMatches()
    {
        yield return new WaitForEndOfFrame();
        List<Block> matches = FindMatches();
        if (matches.Count > 0)
            yield return StartCoroutine(DestroyMatchedBlocks(matches));

        yield return StartCoroutine(CheckAndHandleNoMoreMoves());
    }

    Vector3 CalculateGridStartPosition()
    {
        float width = (columns - 1) * spacing;
        float height = (rows - 1) * spacing;
        return transform.position - new Vector3(width / 2, -height / 2, 0);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        Vector3 startPos = CalculateGridStartPosition();
        return startPos + new Vector3(x * spacing, -y * spacing, 0);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3 start = CalculateGridStartPosition();
        int x = Mathf.RoundToInt((worldPos.x - start.x) / spacing);
        int y = Mathf.RoundToInt((start.y - worldPos.y) / spacing);
        return new Vector2Int(x, y);
    }

    public Block GetBlockAtGridPosition(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= columns || pos.y < 0 || pos.y >= rows)
            return null;
        return grid[pos.y, pos.x];
    }

    public void SwapBlocks(Vector2Int a, Vector2Int b)
    {
        Block temp = grid[a.y, a.x];
        grid[a.y, a.x] = grid[b.y, b.x];
        grid[b.y, b.x] = temp;

        Vector3 tempPos = grid[a.y, a.x].transform.position;
        grid[a.y, a.x].transform.position = grid[b.y, b.x].transform.position;
        grid[b.y, b.x].transform.position = tempPos;
    }

    public List<Block> GetAllMatches() => FindMatches();

    public void RemoveBlocks(List<Block> blocks) => StartCoroutine(DestroyMatchedBlocks(blocks));

    public Vector2Int GetBlockPosition(Block block)
    {
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
                if (grid[y, x] == block)
                    return new Vector2Int(x, y);
        return new Vector2Int(-1, -1);
    }

    Block.BlockColor RandomColor() => (Block.BlockColor)Random.Range(0, System.Enum.GetValues(typeof(Block.BlockColor)).Length);
    Block.BlockShape RandomShape() => (Block.BlockShape)Random.Range(0, System.Enum.GetValues(typeof(Block.BlockShape)).Length);

    List<Block> FindMatches()
    {
        List<Block> matches = new List<Block>();
        HashSet<Block> matchedSet = new HashSet<Block>();

        for (int y = 0; y < rows; y++)
        {
            int matchCount = 1;
            for (int x = 1; x < columns; x++)
            {
                if (IsBlockMatch(grid[y, x], grid[y, x - 1]))
                    matchCount++;
                else
                {
                    if (matchCount >= 3)
                        for (int i = 0; i < matchCount; i++)
                            AddMatch(grid[y, x - 1 - i]);
                    matchCount = 1;
                }
            }
            if (matchCount >= 3)
                for (int i = 0; i < matchCount; i++)
                    AddMatch(grid[y, columns - 1 - i]);
        }

        for (int x = 0; x < columns; x++)
        {
            int matchCount = 1;
            for (int y = 1; y < rows; y++)
            {
                if (IsBlockMatch(grid[y, x], grid[y - 1, x]))
                    matchCount++;
                else
                {
                    if (matchCount >= 3)
                        for (int i = 0; i < matchCount; i++)
                            AddMatch(grid[y - 1 - i, x]);
                    matchCount = 1;
                }
            }
            if (matchCount >= 3)
                for (int i = 0; i < matchCount; i++)
                    AddMatch(grid[rows - 1 - i, x]);
        }

        void AddMatch(Block b)
        {
            if (b != null && matchedSet.Add(b))
                matches.Add(b);
        }

        return matches;
    }

    bool IsBlockMatch(Block a, Block b)
    {
        if (a == null || b == null) return false;
        return a.Color == b.Color && a.Shape == b.Shape;
    }

    IEnumerator DestroyMatchedBlocks(List<Block> matched)
    {
        isProcessingMatches = true;
        SetInputEnabled(false);

        GameManager.Instance?.OnBlocksMatched(matched);

        foreach (Block block in matched)
        {
            if (destroyEffect != null)
            {
                ParticleSystem effect = Instantiate(destroyEffect, block.transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            if (destroySound != null && audioSource != null)
                audioSource.PlayOneShot(destroySound);

            Vector2Int pos = GetBlockPosition(block);
            if (useObjectPool)
                ReturnBlockToPool(block);
            else
                Destroy(block.gameObject);

            if (pos.x >= 0 && pos.y >= 0)
                grid[pos.y, pos.x] = null;
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(CollapseAndFillGrid());

        List<Block> newMatches = FindMatches();
        if (newMatches.Count > 0)
            yield return StartCoroutine(DestroyMatchedBlocks(newMatches));
        else
        {
            isProcessingMatches = false;
            SetInputEnabled(true);
            yield return StartCoroutine(CheckAndHandleNoMoreMoves());
        }
    }

    IEnumerator CollapseAndFillGrid()
    {
        for (int x = 0; x < columns; x++)
        {
            int emptySpaces = 0;
            for (int y = rows - 1; y >= 0; y--)
            {
                if (grid[y, x] == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    grid[y + emptySpaces, x] = grid[y, x];
                    grid[y, x] = null;
                    Vector3 targetPos = GridToWorld(x, y + emptySpaces);
                    StartCoroutine(MoveBlockToPosition(grid[y + emptySpaces, x], targetPos));
                }
            }
            for (int i = 0; i < emptySpaces; i++)
            {
                Block newBlock = GetPooledBlock(RandomColor(), RandomShape());
                Vector3 spawnPos = GridToWorld(x, i - emptySpaces);
                newBlock.transform.position = spawnPos;
                grid[i, x] = newBlock;

                Vector3 targetPos = GridToWorld(x, i);
                StartCoroutine(MoveBlockToPosition(newBlock, targetPos));
            }
        }

        yield return new WaitForSeconds(blockMoveDuration + 0.1f);
    }

    IEnumerator MoveBlockToPosition(Block block, Vector3 targetPos)
    {
        Vector3 startPos = block.transform.position;
        float elapsed = 0f;
        while (elapsed < blockMoveDuration)
        {
            block.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / blockMoveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        block.transform.position = targetPos;
    }

    IEnumerator CheckAndHandleNoMoreMoves()
    {
        yield return new WaitForSeconds(0.1f);
        if (!HasPossibleMatch())
        {
            Debug.Log("无更多可交换匹配，重新生成网格");
            yield return new WaitForSeconds(0.3f);
            ResetGrid();
        }
    }

    bool HasPossibleMatch()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Block current = grid[y, x];
                if (current == null) continue;

                if (x < columns - 1)
                {
                    SwapSimulate(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                    if (FindMatches().Count > 0)
                    {
                        SwapSimulate(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                        return true;
                    }
                    SwapSimulate(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                }

                if (y < rows - 1)
                {
                    SwapSimulate(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                    if (FindMatches().Count > 0)
                    {
                        SwapSimulate(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                        return true;
                    }
                    SwapSimulate(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }
            }
        }
        return false;
    }

    void SwapSimulate(Vector2Int a, Vector2Int b)
    {
        Block temp = grid[a.y, a.x];
        grid[a.y, a.x] = grid[b.y, b.x];
        grid[b.y, b.x] = temp;
    }
}
