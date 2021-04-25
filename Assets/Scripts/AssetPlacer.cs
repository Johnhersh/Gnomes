public class AssetPlacer
{
    public GridHandler grid;

    public void PlaceLargestPossibleTile(int CheckX, int CheckY, int OffsetX, int OffsetY)
    {
        if (isOutOfBounds(CheckX + OffsetX, CheckY + OffsetY)) return;

        // Search for a 3x3 first in a grid around the tile

        // If no Y, then we need to search left/right of the tile
        if (OffsetY == 0)
        {
            // Go two tiles to the left/right
            if (CanPlace3x3(CheckX + (OffsetX * 2), CheckY))
                return;

            // Try one up
            if (CanPlace3x3(CheckX + (OffsetX * 2), CheckY + 1))
                return;

            // Try one down
            if (CanPlace3x3(CheckX + (OffsetX * 2), CheckY - 1))
                return;
        }

        // If no X, we need to search up/down of the tile
        if (OffsetX == 0)
        {
            // Go two tiles to the left/right
            if (CanPlace3x3(CheckX, CheckY + (OffsetY * 2)))
                return;

            // Try one to the left
            if (CanPlace3x3(CheckX + 1, CheckY + (OffsetY * 2)))
                return;

            // Try one to the right
            if (CanPlace3x3(CheckX - 1, CheckY + (OffsetY * 2)))
                return;
        }

        // If we're here, then no 3x3 was found. Search for a 2x2
        // If no Y, then we need to search left/right of the tile
        if (OffsetY == 0)
        {
            // Go two tiles to the left/right
            if (CanPlace2x2(CheckX + (OffsetX * 2), CheckY))
                return;

            // Try one down
            if (CanPlace2x2(CheckX + (OffsetX * 2), CheckY + 1))
                return;
        }
        // If no X, we need to search up/down of the tile
        if (OffsetX == 0)
        {
            // Go to the left/right
            if (CanPlace2x2(CheckX, CheckY + (OffsetY)))
                return;

            // Try one to the left, since a 2x2 can only extend to the right and we're trying to find a 2x2 that may include the original tile
            if (CanPlace2x2(CheckX - 1, CheckY + (OffsetY)))
                return;
        }

        // If we're here, we can't spawn a 3x3 or a 2x2 but we know the slot is empty. So spawn a 1x1
        grid.SetTile(CheckX + OffsetX, CheckY + OffsetY, GridHandler.gridSpace.obj1x1);
        return;
    }

    /// <summary>
    /// Check if a given coordinate is out of bounds of the grid
    /// </summary>
    private bool isOutOfBounds(int CheckX, int CheckY)
    {
        bool outOfBounds = CheckX >= grid.roomWidth - 1 || CheckY >= grid.roomHeight - 1 ||
                            CheckX < 1 || CheckY < 1;

        return outOfBounds;
    }

    // Given an (X,Y) check for a 3x3 assuming the tile is in the middle
    private bool CanPlace3x3(int CheckX, int CheckY)
    {
        if (isOutOfBounds(CheckX, CheckY)) return false;

        int Counter = 0;
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (grid.GetTileType(CheckX + x, CheckY + y) == GridHandler.gridSpace.empty)
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
                    grid.SetTile(CheckX + x, CheckY + y, GridHandler.gridSpace.used3x3);
                }
            }
            grid.SetTile(CheckX, CheckY, GridHandler.gridSpace.obj3x3);
            return true;
        }

        return false;
    }

    // Given an (X,Y) check for a 2x2 assuming the tile is in bottom-left
    private bool CanPlace2x2(int CheckX, int CheckY)
    {
        if (isOutOfBounds(CheckX, CheckY)) return false;

        int Counter = 0;

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                if (grid.GetTileType(CheckX + x, CheckY + y) == GridHandler.gridSpace.empty)
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
                    grid.SetTile(CheckX + x, CheckY + y, GridHandler.gridSpace.used2x2);
                }
            }
            grid.SetTile(CheckX, CheckY, GridHandler.gridSpace.obj2x2);
            return true;
        }

        return false;
    }
}
