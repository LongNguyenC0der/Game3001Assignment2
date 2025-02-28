using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    public enum ETileType : byte
    {
        GROUND = 1,
        WATER = 10,
        WALL = 255
    }

    public const int ROWS = 10;
    public const int COLUMNS = 10;

    [SerializeField] private Tile groundTilePrefab;
    [SerializeField] private Tile waterTilePrefab;
    [SerializeField] private Tile wallTilePrefab;

    // map this to the enum values. The value is basically also the cost of that tile
    private int[,] tiles =
    {
        { 1, 1, 1, 1, 255, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 255, 255, 1, 1, 1, 1, 1 },
        { 1, 255, 255, 1, 255, 1, 1, 1, 255, 255 },
        { 1, 1, 1, 1, 1, 255, 255, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 255, 1, 1 },
        { 1, 1, 255, 255, 255, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 255, 255, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 10, 10, 1, 1, 1 },
        { 1, 1, 1, 1, 10, 10, 10, 1, 1, 1 },
    };

    private List<List<Tile>> tileList = new List<List<Tile>>();

    private Tile start;
    private Tile end;

    private void Start()
    {
        float z = 0.0f;

        for (int row = 0; row < ROWS; row++)
        {
            List<Tile> rowTileList = new List<Tile>();
            float x = 0.0f;

            for (int col = 0; col < COLUMNS; col++)
            {
                // Determine the type of tile here
                ETileType type = (ETileType)tiles[row, col];
                Tile tileToSpawn = null;

                switch(type)
                {
                    case ETileType.GROUND:
                        tileToSpawn = groundTilePrefab;
                        break;
                    case ETileType.WATER:
                        tileToSpawn = waterTilePrefab;
                        break;
                    case ETileType.WALL:
                        tileToSpawn = wallTilePrefab;
                        break;
                    default:
                        tileToSpawn = null;
                        break;
                }

                if (tileToSpawn)
                {
                    Tile tile = Instantiate<Tile>(tileToSpawn, this.transform);
                    tile.transform.position = new Vector3(x, 0, z);
                    tile.Row = row;
                    tile.Col = col;
                    tile.Cost = (int)type;
                    rowTileList.Add(tile);
                    x += 1.0f;
                }
            }

            tileList.Add(rowTileList);
            z -= 1.0f;
        }
    }

    public void ResetAllTiles(bool bIsHardReset)
    {
        if (bIsHardReset)
        {
            start = null;
            end = null;
            RandomizeGridMap();
        }
        
        foreach (List<Tile> rowTileList in tileList)
        {
            foreach (Tile tile in rowTileList)
            {
                tile.ResetTile();
            }
        }
    }

    public void RandomizeGridMap()
    {
        // Clear tileList if any exists
        foreach (List<Tile> rowTileList in tileList)
        {
            foreach(Tile tile in rowTileList)
            {
                Destroy(tile.gameObject);
            }
            rowTileList.Clear();
        }
        tileList.Clear();

        // Randomize tiles
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                float rand = Random.Range(0f, 100f);
                //tiles[row, col] = 
            }
        }


        // Instantiate grid map
        float z = 0.0f;

        for (int row = 0; row < ROWS; row++)
        {
            List<Tile> rowTileList = new List<Tile>();
            float x = 0.0f;

            for (int col = 0; col < COLUMNS; col++)
            {
                // Determine the type of tile here
                ETileType type = (ETileType)tiles[row, col];
                Tile tileToSpawn = null;

                switch (type)
                {
                    case ETileType.GROUND:
                        tileToSpawn = groundTilePrefab;
                        break;
                    case ETileType.WATER:
                        tileToSpawn = waterTilePrefab;
                        break;
                    case ETileType.WALL:
                        tileToSpawn = wallTilePrefab;
                        break;
                    default:
                        tileToSpawn = null;
                        break;
                }

                if (tileToSpawn)
                {
                    Tile tile = Instantiate<Tile>(tileToSpawn, this.transform);
                    tile.transform.position = new Vector3(x, 0, z);
                    tile.Row = row;
                    tile.Col = col;
                    tile.Cost = (int)type;
                    rowTileList.Add(tile);
                    x += 1.0f;
                }
            }

            tileList.Add(rowTileList);
            z -= 1.0f;
        }
    }

    public Tile GetStartTile() { return start; }
    public void SetStartTile(Tile newStart) { start = newStart; }
    public Tile GetEndTile() { return end; }
    public void SetEndTile(Tile newEnd) { end = newEnd; }
    public List<List<Tile>> GetTileList() { return tileList; }
}
