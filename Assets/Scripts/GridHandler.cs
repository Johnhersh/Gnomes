using UnityEngine;

public class GridHandler
{
    public enum gridSpace { empty, floor, wall, darkGrass, lightGrass, err, obj3x3, obj2x2, obj1x1, used3x3, used2x2 };
    private gridSpace[,] _grid;
    public int roomHeight, roomWidth;
    Vector2 _roomSizeWorldUnits = new Vector2(150, 150); // This is the size of the map
    const float WorldUnitsInOneGridCell = 1;

    public void SetupGrid()
    {
        //find grid size
        roomHeight = Mathf.RoundToInt(_roomSizeWorldUnits.x / WorldUnitsInOneGridCell);
        roomWidth = Mathf.RoundToInt(_roomSizeWorldUnits.y / WorldUnitsInOneGridCell);

        //create grid
        _grid = new gridSpace[roomWidth, roomHeight];

        //set grid's default state
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                //make every cell "empty"
                _grid[x, y] = gridSpace.empty;
            }
        }
    }

    public void SetTile(int x, int y, gridSpace type)
    {
        _grid[x, y] = type;
    }

    public gridSpace GetTileType(int x, int y)
    {
        return _grid[x, y];
    }

    public int NumberOfFloors()
    {
        int count = 0;
        foreach (gridSpace space in _grid)
        {
            if (space == gridSpace.floor)
                count++;
        }
        return count;
    }

    public float GetGridLength()
    {
        return (float)_grid.Length;
    }
}
