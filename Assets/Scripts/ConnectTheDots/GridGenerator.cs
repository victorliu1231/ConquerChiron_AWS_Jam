using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A typical GridGenerator generates and displays the pathfinder grid
/// </summary>
public class GridGenerator : MonoBehaviour
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField] Transform instantiationLocation;
    [SerializeField] Camera gameCamera;
    public int width = 6;
    public int height = 6;
    public bool hasRandomSeed = false;
    public int randomSeed = 42;

    System.Random random;

    internal static float vertMouseMoveMultiplier = 0;
    internal static float horizMouseMoveMultiplier = 0;

    List<List<Vector2Int>> walks = new List<List<Vector2Int>>();
    List<Color> availableColors = new List<Color>();
    Color[] colors = new Color[]
    {
        new Color(0.95f, 0.76f, 0),
        new Color(0.53f, 0.34f, 0.57f),
        new Color(0.95f, 0.52f, 0f),
        new Color(0.63f, 0.79f, 0.95f),
        new Color(0.75f, 0f, 0.20f),
        new Color(0.76f, 0.70f, 0.50f),
        new Color(0.52f, 0.52f, 0.52f),
        new Color(0.0f, 0.53f, 0.34f),
        new Color(0.90f, 0.56f, 0.67f),
        new Color(0f, 0.40f, 0.65f),
        new Color(0f, 0f, 1f),
        new Color(1f, 0f, 0f),
        new Color(0f, 1f, 0f),
        new Color(1f, 1f, 0f),
        new Color(0f, 1f, 1f),
        new Color(1f, 0f, 1f),
        new Color(0.98f, 0.58f, 0.47f),
        new Color(0.38f, 0.31f, 0.59f),
        new Color(0.96f, 0.65f, 0f),
        new Color(0.70f, 0.27f, 0.42f),
        new Color(0.86f, 0.83f, 0f),
        new Color(0.53f, 0.18f, 0.09f),
        new Color(0.55f, 0.71f, 0f),
        new Color(0.40f, 0.27f, 0.13f),
        new Color(0.89f, 0.35f, 0.13f),
        new Color(0.17f, 0.24f, 0.15f),
        new Color(0.50f, 0.70f, 0.70f)
    };

    byte[,] grid;

    /// <summary>
    /// Initializes the grid
    /// </summary>
    private void Start()
    {
        availableColors.AddRange(colors);

        // Find a better way to set the proper size for the cells
        float cameraSize = (1.2f * ((width > height) ? width : height) + 0.08f) *1.01f;
        //gameCamera.orthographicSize = cameraSize;

        vertMouseMoveMultiplier = ((0.0005f * (cameraSize * cameraSize)) + (0.405f * cameraSize) + 0.01f) * 1.2f;
        horizMouseMoveMultiplier = ((0.0005f * (cameraSize * cameraSize)) + (0.405f * cameraSize) + 0.01f) * 1.2f;

        if (hasRandomSeed)
        {
            random = new System.Random(randomSeed);
        }
        else
        {
            random = new System.Random();

            randomSeed = random.Next();

            random = new System.Random(randomSeed);
        }

        GenerateLocations();
        PlaceImages();
    }

    /// <summary>
    /// For debugging purposes only
    /// </summary>
    private void Update()
    {
        if (GameManager.Instance.isDebugging && Input.GetKeyDown(KeyCode.P))
        {
            PrintGrid();
            PrintWalks();
        }
    }

    /// <summary>
    /// Generates locations 
    /// </summary>
    public void GenerateLocations()
    {
        #region initialize the grid
        grid = new byte[width, height];

        //set all values to 0
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                grid[i, j] = 0;
            }
        }
        #endregion

        //how far does our crawler walk
        int walkTimes = width > height ? width : height;

        int walkIndex = 0;
        while (GetRandomStartPos().HasValue)
        {
            List<Vector2Int> walk = new List<Vector2Int>();
            Vector2Int? startPos = walkIndex == 0 ? new Vector2Int(random.Next(0, width), random.Next(0, height)) : GetRandomStartPos();

            //we found a cell by itself with no neighbors, time to panic
            if(CountEmptySquareNeighbors(startPos.Value.x, startPos.Value.y) < 1)
            {
                //gather the list of indexes around this location
                List<int> indexes = new List<int>();

                if (startPos.Value.x > 0 && !indexes.Contains(grid[startPos.Value.x - 1, startPos.Value.y])) indexes.Add(grid[startPos.Value.x - 1, startPos.Value.y]);
                if (startPos.Value.x < width-1 && !indexes.Contains(grid[startPos.Value.x + 1, startPos.Value.y])) indexes.Add(grid[startPos.Value.x + 1, startPos.Value.y]);
                if (startPos.Value.y > 0 && !indexes.Contains(grid[startPos.Value.x, startPos.Value.y-1])) indexes.Add(grid[startPos.Value.x, startPos.Value.y-1]);
                if (startPos.Value.y < height-1 && !indexes.Contains(grid[startPos.Value.x, startPos.Value.y+1])) indexes.Add(grid[startPos.Value.x, startPos.Value.y+1]);

                foreach(int ind in indexes)
                {
                    foreach(Vector2Int vector2Int in walks[ind - 1])
                    {
                        grid[vector2Int.x, vector2Int.y] = 0;
                    }

                    List<Vector2Int> walk0 = new List<Vector2Int>();
                    Vector2Int? secondPos = GetRandomStartPos();
                    if (secondPos.HasValue) RecursiveWalk(walk0, secondPos.Value, walkTimes % 2 == 0 ? walkTimes - 1 : walkTimes, ind);
                    walks[ind - 1] = walk0;
                }

                walkIndex--;
            }
            //our cell has neighbors, have it walk
            else
            {
                RecursiveWalk(walk, startPos.Value, walkTimes % 2 == 0 ? walkTimes - 1 : walkTimes, walkIndex+1);

                Vector2Int? randStartPos = GetRandomStartPos();

                //we found a cell by itself with no neighbors, time to panic
                if (randStartPos.HasValue && CountEmptySquareNeighbors(randStartPos.Value.x, randStartPos.Value.y) < 1)
                {
                    //reset
                    if (hasRandomSeed)
                    {
                        randomSeed++;
                    }

                    walks.Clear();
                    GenerateLocations();
                    return;
                }
                //everything is all hunkydory
                else
                {
                    walks.Add(walk);
                }
            }

            walkIndex++;
        }

        //Crawls randomly across our grid
        List<Vector2Int> RecursiveWalk(List<Vector2Int> walk, Vector2Int startPos, int steps, int index)
        {
            walk.Add(new Vector2Int(startPos.x, startPos.y));

            grid[startPos.x, startPos.y] = (byte)index;

            Vector3Int? nextDir = NextDirection(startPos.x, startPos.y);

            if(nextDir != null && (nextDir.Value.z == -1 || steps > 0))
            {
                return RecursiveWalk(walk, new Vector2Int(nextDir.Value.x, nextDir.Value.y), --steps, index);
            }

            return walk;
        }

        //gets the next random direction
        Vector3Int? NextDirection(int x, int y)
        {
            List<Vector3Int> possiblePositions = new List<Vector3Int>();

            //look for empty neighbors
            if(x>0 && grid[x-1, y] == 0)
            {
                if (CountEmptySquareNeighbors(x - 1, y) == 0) return new Vector3Int(x - 1, y, -1);

                possiblePositions.Add(new Vector3Int(x - 1, y, 0));
            }
            if(x < width-1 && grid[x+1, y] == 0)
            {
                if (CountEmptySquareNeighbors(x + 1, y) == 0) return new Vector3Int(x + 1, y, -1);

                possiblePositions.Add(new Vector3Int(x + 1, y, 0));
            }
            if (y > 0 && grid[x , y-1] == 0)
            {
                if (CountEmptySquareNeighbors(x, y - 1) == 0) return new Vector3Int(x, y - 1, -1);

                possiblePositions.Add(new Vector3Int(x, y - 1, 0));
            }
            if (y < height - 1 && grid[x, y + 1] == 0)
            {
                if (CountEmptySquareNeighbors(x, y + 1) == 0) return new Vector3Int(x, y + 1, -1);

                possiblePositions.Add(new Vector3Int(x, y + 1, 0));
            }

            if (possiblePositions.Count == 0) return null;

            return possiblePositions[random.Next(0, possiblePositions.Count)];
        }

        //choose a random start position from the empty cells
        Vector2Int? GetRandomStartPos()
        {
            int minNeighborCount = 5;

            List<Vector2Int> startPositions = new List<Vector2Int>();

            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    if (grid[i, j] > 0) continue;

                    int neighborCount = CountEmptySquareNeighbors(i, j);

                    if(neighborCount < minNeighborCount)
                    {
                        startPositions.Clear();
                        minNeighborCount = neighborCount;
                        startPositions.Add(new Vector2Int(i, j));
                    }
                    else if ( neighborCount == minNeighborCount)
                    {
                        startPositions.Add(new Vector2Int(i, j));
                    }
                }
            }

            return (startPositions.Count == 0) ? null : startPositions[random.Next(0, startPositions.Count)];
        }

        //Counts the unassigned neighbors
        int CountEmptySquareNeighbors(int x, int y)
        {
            int count = 0;

            if (x > 0 && grid[x - 1, y] == 0) count++;
            if (x < width-1 && grid[x + 1, y] == 0) count++;
            if (y > 0 && grid[x, y - 1] == 0) count++;
            if (y < height-1 && grid[x, y+1] == 0) count++;

            return count;
        }
    }

    /// <summary>
    /// Creates and places images on the screen
    /// </summary>
    public void PlaceImages()
    {
        Cell[,] newCells = new Cell[width, height];

        // Make the float cellSize dynamic based on the camera size
        float camHeight = 2f * Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;
        //float cellSize = 2.4f; // Make this dynamic based on camera size
        float cellSize = camHeight / height < camWidth / width ? (camHeight / height) * 0.9f : (camWidth / width) * 0.9f;
        float halfCellSize = cellSize / 2;

        for(int j = 0; j< height; j++)
        {
            for(int i = 0; i < width; i++)
            {
                int index = i - (width / 2);

                //is it to the left of 0?
                if(i < (width / 2))
                {
                    GetY((cellSize * ++index) - halfCellSize - (width % 2 == 0 ? 0 : halfCellSize));
                }
                else
                {
                    GetY((cellSize * index) + halfCellSize - (width % 2 == 0 ? 0 : halfCellSize));
                }

                void GetY(float x)
                {
                    Cell cell = Instantiate(cellPrefab, instantiationLocation, false).GetComponent<Cell>();
                    cell.transform.localScale = new Vector3(cellSize / (1.8f / 0.7f), cellSize / (1.8f / 0.7f), 1);
                    newCells[i, j] = cell;

                    int index = -(j - (height / 2));

                    //cellObj 1_2
                    cell.gameObject.name = "cellObj " + i + "_" + j;

                    ConnectDotsGameController.instance.cells.Add(cell);
                    cell.Initialize(i, j);

                    if (j < (height / 2))
                    {
                        cell.transform.localPosition = new Vector3(x, (cellSize * --index) + halfCellSize + (height % 2 == 0 ? 0 : halfCellSize));
                    }
                    else
                    {
                        cell.transform.localPosition = new Vector3(x, (cellSize * index) - halfCellSize + (height % 2 == 0 ? 0 : halfCellSize));
                    }
                }
            }
        }

        int walkIndex = 1;
        foreach(List<Vector2Int> walk in walks)
        {
            //pick a color
            int index = random.Next(0, availableColors.Count);
            Color color = availableColors[index];
            availableColors.RemoveAt(index);

            //fill the colors again if we run out
            if (availableColors.Count < 1) availableColors.AddRange(colors);

            Line line = new Line();
            line.Initialize(
                newCells[walk[0].x, walk[0].y], 
                newCells[walk[walk.Count - 1].x, walk[walk.Count - 1].y], 
                color, 
                (byte)walkIndex++);

            ConnectDotsGameController.instance.lines.Add(line);
        }

        ConnectDotsGameController.isGameActive = true;
    }
    
    /// <summary>
    /// for debugging only, shows the base values after walking
    /// </summary>
    void PrintGrid()
    {
        string s = "";

        for(int i = 0; i < height; i++)
        {
            if (i > 0) s += new string('-', height * 2) + "\n";
            for(int j = 0; j < width; j++)
            {
                if (j > 0) s += "|";
                s += grid[j, i];
            }
            s += "\n";
        }

        print(s);
    }

    void PrintWalks()
    {
        for (int i = 0; i < walks.Count; i++)
        {
            string s = "Walks: " + i + "\n";
            for (int j = 0; j < walks[i].Count; j++)
            {
                s += walks[i][j].ToString() + ", ";
            }
            print(s);
        }
    }
}
