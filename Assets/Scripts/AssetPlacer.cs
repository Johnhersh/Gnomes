using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetPlacer
{
    public WorldGenerator.gridSpace[,] grid;

    public WorldGenerator.gridSpace[,] FindLargestPossibleTile(int CheckX, int CheckY, int OffsetX, int OffsetY)
    {
        // Search for a 3x3 first in a grid around the tile

        // If no Y, then we need to search left/right of the tile
        if (OffsetY == 0)
        {
            // Go two tiles to the left/right
            if (CanPlace3x3(CheckX + (OffsetX * 2), CheckY))
                return grid;

            // Try one up
            if (CanPlace3x3(CheckX + (OffsetX * 2), CheckY + 1))
                return grid;

            // Try one down
            if (CanPlace3x3(CheckX + (OffsetX * 2), CheckY - 1))
                return grid;
        }

        // If no X, we need to search up/down of the tile
        if (OffsetX == 0)
        {
            // Go two tiles to the left/right
            if (CanPlace3x3(CheckX, CheckY + (OffsetY * 2)))
                return grid;

            // Try one to the left
            if (CanPlace3x3(CheckX + 1, CheckY + (OffsetY * 2)))
                return grid;

            // Try one to the right
            if (CanPlace3x3(CheckX - 1, CheckY + (OffsetY * 2)))
                return grid;
        }

        // If we're here, then no 3x3 was found. Search for a 2x2
        // If no Y, then we need to search left/right of the tile
        if (OffsetY == 0)
        {
            // Go two tiles to the left/right
            if (CanPlace2x2(CheckX + (OffsetX * 2), CheckY))
                return grid;

            // Try one down
            if (CanPlace2x2(CheckX + (OffsetX * 2), CheckY + 1))
                return grid;
        }
        // If no X, we need to search up/down of the tile
        if (OffsetX == 0)
        {
            // Go to the left/right
            if (CanPlace2x2(CheckX, CheckY + (OffsetY)))
                return grid;

            // Try one to the left, since a 2x2 can only extend to the right and we're trying to find a 2x2 that may include the original tile
            if (CanPlace2x2(CheckX - 1, CheckY + (OffsetY)))
                return grid;
        }

        // If we're here, we can't spawn a 3x3 or a 2x2 but we know the slot is empty. So spawn a 1x1
        grid[CheckX + OffsetX, CheckY + OffsetY] = WorldGenerator.gridSpace.obj1x1;
        return grid;
    }

    // Given an (X,Y) check for a 3x3 assuming the tile is in the middle
    bool CanPlace3x3(int CheckX, int CheckY)
    {
        int Counter = 0;
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (grid[CheckX + x, CheckY + y] == WorldGenerator.gridSpace.empty)
                {
                    Counter++;
                }
            }
        }

        if (Counter == 9)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    grid[CheckX + x, CheckY + y] = WorldGenerator.gridSpace.used3x3;
                }
            }
            grid[CheckX, CheckY] = WorldGenerator.gridSpace.obj3x3;
            return true;
        }

        return false;
    }

    // Given an (X,Y) check for a 2x2 assuming the tile is in bottom-left
    bool CanPlace2x2(int CheckX, int CheckY)
    {
        int Counter = 0;

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                if (grid[CheckX + x, CheckY + y] == WorldGenerator.gridSpace.empty)
                {
                    Counter++;
                }
            }
        }

        if (Counter == 4)
        {
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    grid[CheckX + x, CheckY + y] = WorldGenerator.gridSpace.used2x2;
                }
            }
            grid[CheckX, CheckY] = WorldGenerator.gridSpace.obj2x2;
            return true;
        }

        return false;
    }
}
