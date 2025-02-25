using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    public const int ROWS = 10;
    public const int COLUMNS = 10;

    [SerializeField] private Tile tilePrefab;

    private int[,] tiles =
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    };

    private List<List<Tile>> tileList = new List<List<Tile>>();

    public Tile start;
    public Tile end;

    private void Start()
    {
        float z = 0.0f;

        for (int row = 0; row < ROWS; row++)
        {
            List<Tile> rowTileList = new List<Tile>();
            float x = 0.0f;

            for (int col = 0; col < COLUMNS; col++)
            {
                Tile tile = Instantiate<Tile>(tilePrefab, this.transform);
                tile.transform.position = new Vector3(x, 0, z);
                tile.row = row;
                tile.col = col;
                rowTileList.Add(tile);
                x += 1.0f;
            }

            tileList.Add(rowTileList);
            z -= 1.0f;
        }
    }

    public void ResetAllTiles()
    {
        start = null;
        end = null;
        foreach (List<Tile> rowTileList in tileList)
        {
            foreach (Tile tile in rowTileList)
            {
                tile.ResetTile();
            }
        }
    }

    public List<List<Tile>> GetTileList() { return tileList; }
}
