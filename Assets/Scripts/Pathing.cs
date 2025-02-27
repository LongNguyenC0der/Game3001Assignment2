using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Node
{
    public Tile currentTile;
    public Tile previousTile;
    public float cost;
}

public static class Pathing
{
    
    public static List<Tile> Dijkstra(Tile start, Tile end, List<List<Tile>> tileList, int iterations, GridMap gridMap, out float totalPathCost, out float heuristic)
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
        nodes[start.Row, start.Col].cost = 0.0f;

        bool found = false;
        HashSet<Tile> debugTiles = new HashSet<Tile>();

        totalPathCost = 0.0f;
        heuristic = 0.0f;

        for (int i = 0; i < iterations; i++)
        {
            // Examine the tile with the lowest cost
            Tile front = open.OrderBy((key) => key.Value).First().Key;
            open.Remove(front);

            // Stop searching if we've reached our goal
            if (front.Equals(end))
            {
                totalPathCost = nodes[front.Row, front.Col].cost;

                //F,G,H for end tile
                nodes[front.Row, front.Col].currentTile.G = nodes[front.Row, front.Col].cost;

                found = true;
                break;
            }

            // If we want to see the visual
            debugTiles.Add(front);

            // Update tile cost and add it to open list if the new cost is cheaper than the old cost
            foreach (Tile adj in Adjacent(front, gridMap.GetTileList(), GridMap.ROWS, GridMap.COLUMNS))
            {
                float previousCost = nodes[adj.Row, adj.Col].cost;
                float currentCost = nodes[front.Row, front.Col].cost + adj.Cost;

                //F,G,H for each tile
                nodes[front.Row, front.Col].currentTile.G = nodes[front.Row, front.Col].cost;


                // Manhattan Distance (absolute sum of differences)
                float h = Mathf.Abs(end.Row - adj.Row) + Mathf.Abs(end.Col - adj.Col);
                heuristic = h;
                // f = g + h (Estimated Total Cost)
                float f = currentCost + h;

                // Note to self: pretty funky to wrap my head around
                // F only determine which tile to explore first
                // G is the actual path, so we only compare G
                if (currentCost < previousCost)
                {
                    if (open.ContainsKey(adj)) open[adj] = f;
                    else open.Add(adj, f);

                    nodes[adj.Row, adj.Col].cost = currentCost;
                    nodes[adj.Row, adj.Col].previousTile = front;
                }
            }
        }

        if (!found)
        {
            foreach (Tile tile in debugTiles)
            {
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
        Tile previous = nodes[current.Row, current.Col].previousTile;

        // Search until nothing came before the previous tile, meaning we've reached start!
        while(previous)
        {
            // Add current to path
            path.Add(current);
            // Set current equal to previous
            current = previous;
            // Set previous equal to the tile that came before current
            previous = nodes[current.Row, current.Col].previousTile;
        }

        return path;
    }

    public static List<Tile> Adjacent(Tile tile, List<List<Tile>> tileList, int rows, int cols)
    {
        List<Tile> tiles = new List<Tile>();

        if (tile.Col - 1 >= 0)
        {
            Tile left = tileList[tile.Row][tile.Col - 1];
            tiles.Add(left);
        }
        if (tile.Col + 1 < cols)
        {
            Tile right = tileList[tile.Row][tile.Col + 1];
            tiles.Add(right);
        }
        if (tile.Row - 1 >= 0)
        {
            Tile up = tileList[tile.Row - 1][tile.Col];
            tiles.Add(up);
        }
        if (tile.Row + 1 < rows)
        {
            Tile down = tileList[tile.Row + 1][tile.Col];
            tiles.Add(down);
        }
        
        return tiles;
    }
}
