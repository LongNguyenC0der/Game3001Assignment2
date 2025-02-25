using System.Collections.Generic;
using System.Linq;

public struct Node
{
    public Tile currentTile;
    public Tile previousTile;
    public float cost;
}

public static class Pathing
{
    
    public static List<Tile> Dijkstra(Tile start, Tile end, List<List<Tile>> tileList, int iterations, GridMap gridMap)
    {
        Node[,] nodes = new Node[GridMap.ROWS, GridMap.COLUMNS];
        for (int row = 0; row < GridMap.ROWS; row++)
        {
            for (int col = 0; col < GridMap.COLUMNS; col++)
            {
                nodes[row, col].currentTile = tileList[row][col];
                nodes[row, col].previousTile = null;
                nodes[row, col].cost = float.MaxValue;
            }
        }

        Dictionary<Tile, float> open = new Dictionary<Tile, float>();
        open.Add(start, 0.0f);
        nodes[start.row, start.col].cost = 0.0f;

        bool found = false;
        HashSet<Tile> debugTiles = new HashSet<Tile>();

        for (int i = 0; i < iterations; i++)
        {
            Tile front = null;

            // Examine the tile with the lowest cost
            foreach (KeyValuePair<Tile, float> tile in open.OrderBy((key) => key.Value))
            {
                if (open.ContainsKey(tile.Key))
                {
                    front = tile.Key;
                    open.Remove(tile.Key);
                    break;
                }
            }

            // Stop searching if we've reached our goal
            if (front.Equals(end))
            {
                found = true;
                break;
            }

            // If DebugDraw
            debugTiles.Add(front);

            // Update tile cost and add it to open list if the new cost is cheaper than the old cost
            foreach (Tile adj in Adjacent(front, gridMap.GetTileList(), GridMap.ROWS, GridMap.COLUMNS))
            {
                float previousCost = nodes[adj.row, adj.col].cost;
                float currentCost = nodes[front.row, front.col].cost + adj.GetCost();
                if (currentCost < previousCost)
                {
                    open.Add(adj, currentCost);
                    nodes[adj.row, adj.col].cost = currentCost;
                    nodes[adj.row, adj.col].previousTile = front;
                }
            }
        }

        if (!found)
        {
            foreach (Tile tile in debugTiles)
            {
                // tile.change color or something
                tile.BeingExplored();
            }
        }

        // If we've found the end, retrace our steps. Otherwise, there's no solution so return an empty list.
        List<Tile> result = found ? Retrace(nodes, start, end) : new List<Tile>();
        return result;
    }

    private static List<Tile> Retrace(Node[,] nodes, Tile start, Tile end)
    {
        List<Tile> path = new List<Tile>();

        // Start at the end, and work backwards until we reach the start!
        Tile current = end;

        // Previous is the tile that came before the current tile
        Tile previous = nodes[current.row, current.col].previousTile;

        // Search until nothing came before the previous tile, meaning we've reached start!
        while(previous)
        {
            // Add current to path
            path.Add(current);
            // Set current equal to previous
            current = previous;
            // Set previous equal to the tile that came before current
            previous = nodes[current.row, current.col].previousTile;
        }

        return path;
    }

    public static List<Tile> Adjacent(Tile tile, List<List<Tile>> tileList, int rows, int cols)
    {
        List<Tile> tiles = new List<Tile>();

        if (tile.col - 1 >= 0)
        {
            Tile left = tileList[tile.row][tile.col - 1];
            tiles.Add(left);
        }
        if (tile.col + 1 < cols)
        {
            Tile right = tileList[tile.row][tile.col + 1];
            tiles.Add(right);
        }
        if (tile.row - 1 >= 0)
        {
            Tile up = tileList[tile.row - 1][tile.col];
            tiles.Add(up);
        }
        if (tile.row + 1 < rows)
        {
            Tile down = tileList[tile.row + 1][tile.col];
            tiles.Add(down);
        }
        
        return tiles;
    }
}
