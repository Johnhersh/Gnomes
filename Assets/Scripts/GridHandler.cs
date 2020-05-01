using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHandler
{
    public enum gridSpace { empty, floor, wall, darkGrass, err, obj3x3, obj2x2, obj1x1, used3x3, used2x2 };
    private gridSpace[,] grid;
    public int roomHeight, roomWidth;
    Vector2 roomSizeWorldUnits = new Vector2(150, 150); // This is the size of the map
    float worldUnitsInOneGridCell = 1;

    public void SetupGrid()
    {
        //find grid size
        roomHeight = Mathf.RoundToInt(roomSizeWorldUnits.x / worldUnitsInOneGridCell);
        roomWidth = Mathf.RoundToInt(roomSizeWorldUnits.y / worldUnitsInOneGridCell);

        //create grid
        grid = new gridSpace[roomWidth, roomHeight];

        //set grid's default state
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                //make every cell "empty"
                grid[x, y] = gridSpace.empty;
            }
        }
    }

    public void SetTile(int x, int y, gridSpace type)
    {
        grid[x, y] = type;
    }

    public gridSpace GetTileType(int x, int y)
    {
        return grid[x, y];
    }

    public int NumberOfFloors()
    {
        int count = 0;
        foreach (gridSpace space in grid)
        {
            if (space == gridSpace.floor)
                count++;
        }
        return count;
    }

    public float GetGridLength()
    {
        return (float)grid.Length;
    }
}
