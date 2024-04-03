using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] [Range(0,20)]private int cptMaxBox;
    [SerializeField] private Transform camTransforme;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Box boxPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private GameManager gameManager;

    private Dictionary<Vector2, GameObject> board;
    private int cptBox, height, width;

    public TextAsset csvFile;

    void Start()
    {
        GenerateGrid();
    }

    static public string[,] SplitCsvGrid(string csvText)
    {
        string[] lines = csvText.Split("\n"[0]);

        // finds the max width of row
        int width = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string[] row = SplitCsvLine(lines[i]);
            width = Mathf.Max(width, row.Length);
        }

        // creates new 2D string grid to output to
        string[,] outputGrid = new string[width + 1, lines.Length + 1];
        for (int y = 0; y < lines.Length; y++)
        {
            string[] row = SplitCsvLine(lines[y]);
            for (int x = 0; x < row.Length; x++)
            {
                outputGrid[x, y] = row[x];

                // This line was to replace "" with " in my output. 
                // Include or edit it as you wish.
                outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
            }
        }

        return outputGrid;
    }

    static public string[] SplitCsvLine(string line)
    {
        return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
        @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
        System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                select m.Groups[1].Value).ToArray();
    }

    void GenerateGrid()
    {
        string[,] grid = SplitCsvGrid(csvFile.text);
        height = grid.GetUpperBound(1);
        width = grid.GetUpperBound(0) - 1;
        board = new Dictionary<Vector2, GameObject>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var spawnedObject = creatObject(grid[x,y],x,y);
                spawnedObject.transform.parent = transform;
                board[new Vector2(x, y)] = spawnedObject;
            }
        }

        camTransforme.position = new Vector3((float)(grid.GetUpperBound(0)-1) / 2 - 0.5f, (float)grid.GetUpperBound(1) / 2 - 0.5f, -10);
    }

    GameObject creatObject(string s,int x,int y)
    {
        switch (s)
        {
            case "#":
                var spawnedWall = Instantiate(wallPrefab,new Vector3(x, y), Quaternion.identity);
                spawnedWall.name = $"Wall {x} {y}";
                return spawnedWall;
            default:
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
                switch (s)
                {
                    case "O":
                        gameManager.AddPlayer(Instantiate(playerPrefab, spawnedTile.transform));
                        break;
                    default:
                        if (cptBox < cptMaxBox)
                        {
                            Box box;
                            var rnd = new System.Random();
                            int r = rnd.Next(20);
                            if (r == 5 || r == 10 || r == 15)
                            {
                                Instantiate(boxPrefab, spawnedTile.transform);
                                cptBox++;
                            }
                            else if (r == 2)
                            {
                                box = Instantiate(boxPrefab, spawnedTile.transform);
                                box.cherrie = true;
                                cptBox++;
                            }
                        }
                        break;
                }
                return spawnedTile.gameObject;
        }
    }



    public GameObject GetTileAtPosition(Vector2 pos)
    {
        if (board.TryGetValue(pos, out var go)) return go;
        return null;
    }
}