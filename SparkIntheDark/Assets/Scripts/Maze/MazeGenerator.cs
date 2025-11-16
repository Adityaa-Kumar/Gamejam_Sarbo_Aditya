using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Dimensions")]
    [Tooltip("Width of the maze (number of cells)")]
    [Range(5, 50)]
    public int mazeWidth = 10;

    [Tooltip("Height of the maze (number of cells)")]
    [Range(5, 50)]
    public int mazeHeight = 10;

    [Header("Cell Settings")]
    [Tooltip("Size of each cell in Unity units")]
    [Range(1f, 10f)]
    public float cellSize = 2f;

    [Tooltip("Width/thickness of corridor walls")]
    [Range(0.1f, 1f)]
    public float wallThickness = 0.2f;

    [Tooltip("Height of the walls")]
    [Range(1f, 5f)]
    public float wallHeight = 3f;

    [Header("Prefabs")]
    [Tooltip("Wall prefab (will be created if null)")]
    public GameObject wallPrefab;

    [Tooltip("Floor prefab (will be created if null)")]
    public GameObject floorPrefab;

    [Header("Generation Settings")]
    [Tooltip("Visualize generation step by step")]
    public bool animateGeneration = false;

    [Tooltip("Delay between generation steps (seconds)")]
    [Range(0f, 0.5f)]
    public float generationDelay = 0.05f;

    [Header("Materials")]
    public Material wallMaterial;
    public Material floorMaterial;

    private bool[,] visitedCells;
    private bool[,] horizontalWalls;
    private bool[,] verticalWalls;

    void Start()
    {
        GenerateMaze();
    }

    [ContextMenu("Generate New Maze")]
    public void GenerateMaze()
    {
        ClearMaze();

        if (animateGeneration)
        {
            StartCoroutine(GenerateMazeCoroutine());
        }
        else
        {
            GenerateMazeInstant();
            BuildMazeVisuals();
        }
    }

    void ClearMaze()
    {
        // Clear existing maze
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void GenerateMazeInstant()
    {
        InitializeArrays();

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(0, 0);
        visitedCells[current.x, current.y] = true;

        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(current);

            if (unvisitedNeighbors.Count > 0)
            {
                stack.Push(current);

                Vector2Int chosen = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                RemoveWallBetween(current, chosen);

                visitedCells[chosen.x, chosen.y] = true;
                stack.Push(chosen);
            }
        }
    }

    IEnumerator GenerateMazeCoroutine()
    {
        InitializeArrays();
        BuildMazeVisuals();

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(0, 0);
        visitedCells[current.x, current.y] = true;

        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(current);

            if (unvisitedNeighbors.Count > 0)
            {
                stack.Push(current);

                Vector2Int chosen = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                RemoveWallBetween(current, chosen);

                visitedCells[chosen.x, chosen.y] = true;
                stack.Push(chosen);

                yield return new WaitForSeconds(generationDelay);
                UpdateMazeVisuals();
            }
        }
    }

    void InitializeArrays()
    {
        visitedCells = new bool[mazeWidth, mazeHeight];
        horizontalWalls = new bool[mazeWidth, mazeHeight + 1];
        verticalWalls = new bool[mazeWidth + 1, mazeHeight];

        // Initialize all walls as present
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y <= mazeHeight; y++)
            {
                horizontalWalls[x, y] = true;
            }
        }

        for (int x = 0; x <= mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                verticalWalls[x, y] = true;
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // North
        if (cell.y < mazeHeight - 1 && !visitedCells[cell.x, cell.y + 1])
            neighbors.Add(new Vector2Int(cell.x, cell.y + 1));

        // South
        if (cell.y > 0 && !visitedCells[cell.x, cell.y - 1])
            neighbors.Add(new Vector2Int(cell.x, cell.y - 1));

        // East
        if (cell.x < mazeWidth - 1 && !visitedCells[cell.x + 1, cell.y])
            neighbors.Add(new Vector2Int(cell.x + 1, cell.y));

        // West
        if (cell.x > 0 && !visitedCells[cell.x - 1, cell.y])
            neighbors.Add(new Vector2Int(cell.x - 1, cell.y));

        return neighbors;
    }

    void RemoveWallBetween(Vector2Int current, Vector2Int neighbor)
    {
        int xDiff = neighbor.x - current.x;
        int yDiff = neighbor.y - current.y;

        if (xDiff == 1) // East
        {
            verticalWalls[neighbor.x, current.y] = false;
        }
        else if (xDiff == -1) // West
        {
            verticalWalls[current.x, current.y] = false;
        }
        else if (yDiff == 1) // North
        {
            horizontalWalls[current.x, neighbor.y] = false;
        }
        else if (yDiff == -1) // South
        {
            horizontalWalls[current.x, current.y] = false;
        }
    }

    void BuildMazeVisuals()
    {
        ClearMaze();

        // Create floor
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                CreateFloor(x, y);
            }
        }

        UpdateMazeVisuals();
    }

    void UpdateMazeVisuals()
    {
        // Remove existing walls
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Wall"))
            {
                Destroy(child.gameObject);
            }
        }

        // Create horizontal walls
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y <= mazeHeight; y++)
            {
                if (horizontalWalls[x, y])
                {
                    CreateHorizontalWall(x, y);
                }
            }
        }

        // Create vertical walls
        for (int x = 0; x <= mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (verticalWalls[x, y])
                {
                    CreateVerticalWall(x, y);
                }
            }
        }
    }

    void CreateFloor(int x, int y)
    {
        GameObject floor;

        if (floorPrefab != null)
        {
            floor = Instantiate(floorPrefab, transform);
        }
        else
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.parent = transform;
            floor.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);

            if (floorMaterial != null)
            {
                floor.GetComponent<Renderer>().material = floorMaterial;
            }
        }

        float posX = (x - mazeWidth / 2f) * cellSize + cellSize / 2f;
        float posZ = (y - mazeHeight / 2f) * cellSize + cellSize / 2f;
        floor.transform.localPosition = new Vector3(posX, 0, posZ);
        floor.name = $"Floor_{x}_{y}";
    }

    void CreateHorizontalWall(int x, int y)
    {
        GameObject wall;

        if (wallPrefab != null)
        {
            wall = Instantiate(wallPrefab, transform);
        }
        else
        {
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.parent = transform;

            if (wallMaterial != null)
            {
                wall.GetComponent<Renderer>().material = wallMaterial;
            }
        }

        wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);

        float posX = (x - mazeWidth / 2f) * cellSize + cellSize / 2f;
        float posZ = (y - mazeHeight / 2f) * cellSize;
        wall.transform.localPosition = new Vector3(posX, wallHeight / 2f, posZ);
        wall.name = $"Wall_H_{x}_{y}";
    }

    void CreateVerticalWall(int x, int y)
    {
        GameObject wall;

        if (wallPrefab != null)
        {
            wall = Instantiate(wallPrefab, transform);
        }
        else
        {
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.parent = transform;

            if (wallMaterial != null)
            {
                wall.GetComponent<Renderer>().material = wallMaterial;
            }
        }

        wall.transform.localScale = new Vector3(wallThickness, wallHeight, cellSize);

        float posX = (x - mazeWidth / 2f) * cellSize;
        float posZ = (y - mazeHeight / 2f) * cellSize + cellSize / 2f;
        wall.transform.localPosition = new Vector3(posX, wallHeight / 2f, posZ);
        wall.name = $"Wall_V_{x}_{y}";
    }
}
