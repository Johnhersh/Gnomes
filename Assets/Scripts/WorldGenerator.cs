using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics; // This is for the Stopwatch in Update()

// [ExecuteInEditMode]
public class WorldGenerator : MonoBehaviour
{
    private Vector2 _roomSizeWorldUnits = new Vector2(150, 150); // This is the size of the map
    private readonly float[,] _noiseMap = new float[150, 150]; // This is where we keep the perlin noise. Used for adding grass
    private const float WorldUnitsInOneGridCell = 1;
    private readonly GridHandler _newGrid = new GridHandler();

    private struct walker
    {
        public Vector2 dir;
        public Vector2 pos;
    }

    private List<walker> _walkers; // This will contain all our active walkers
    private const float ChanceWalkerChangeDir = 0.3f;
    private const float ChanceWalkerSpawn = 0.03f;
    private const float ChanceWalkerDestroy = 0.05f;
    private const int MaxWalkers = 12;
    private const float PercentToFill = 0.2f; //What percentage of the grid should be filled before we move on

    /// <summary>
    /// These are accessible via the editor:
    /// </summary>
    public Tilemap topMap, darkGrassMap, lightGrassMap, botMap;
    public RuleTile topTile, darkGrassTile, lightGrassTile, botTile;
    public RuleTile errTile; //This tile is just used for debugging
    public GameObject Tile1x1;
    public GameObject Tile2x2;
    public GameObject Tile3x3;
    public float noiseScale = 1f;


    // Start is called before the first frame update
    private void Start()
    {
        Stopwatch st = new Stopwatch(); // Start measuring how long it takes to generate a map. Will use later for optimiazations
        st.Start();

        Setup();
        CreateFloors();

        // CreateWalls();
        FillHoles(); // After creating the base floor, I don't want to have empty cells
        FillHoles(); // Catch any newly created gaps

        AddGrass();

        AddBorders();

        SpawnLevelTiles();

        st.Stop();
        UnityEngine.Debug.Log(string.Format("Generating took {0} ms to complete", st.ElapsedMilliseconds));
    }

    private void Setup()
    {
        _newGrid.SetupGrid();

        //make sure our tilemaps are clear. Can be an issue after coming from the editor
        topMap.ClearAllTiles();
        botMap.ClearAllTiles();
        darkGrassMap.ClearAllTiles();
        lightGrassMap.ClearAllTiles();

        //set first walker
        _walkers = new List<walker>(); // init list
        walker newWalker = new walker(); //create a walker
        newWalker.dir = RandomDirection();
        //find center of grid
        Vector2 spawnPos = new Vector2(Mathf.RoundToInt(_newGrid.roomWidth / 2.0f),
                                    Mathf.RoundToInt(_newGrid.roomHeight / 2.0f));
        newWalker.pos = spawnPos;
        //add walker to our list
        _walkers.Add(newWalker);

        //Generate noisemap
        for (int xIndex = 0; xIndex < _newGrid.roomWidth; xIndex++)
        {
            for (int yIndex = 0; yIndex < _newGrid.roomHeight; yIndex++)
            {
                // Mathf.PerlinNoise requires floats as input
                float sampleX = xIndex / noiseScale;
                float sampleY = yIndex / noiseScale;
                _noiseMap[xIndex, yIndex] = Mathf.PerlinNoise(sampleX, sampleY);
            }
        }
    }

    private void CreateFloors()
    {
        int iterations = 0; // Just want to keep track of how many times we've looped so we don't get an infinite loop. This is just in case
        GridHandler.gridSpace floorTile = GridHandler.gridSpace.floor;
        do
        {
            //create floor at position of every walker
            foreach (walker myWalker in _walkers)
            {
                _newGrid.SetTile((int)myWalker.pos.x, (int)myWalker.pos.y, floorTile);
                //Make the path 2 tiles thick:
                //If we're moving up or down, also create the tile to the right of the path
                if ((int)myWalker.dir.y != 0)
                    _newGrid.SetTile((int)myWalker.pos.x + 1, (int)myWalker.pos.y, floorTile);
                //If we're moving left or right, also create the tile to above the path
                if ((int)myWalker.dir.x != 0)
                    _newGrid.SetTile((int)myWalker.pos.x, (int)myWalker.pos.y + 1, floorTile);
            }
            //chance: destroy walker
            int numberChecks = _walkers.Count; //see how many walkers we have
            for (int i = 0; i < numberChecks; i++)
            {
                //only if it's not the only one, and only rarely
                if (Random.value < ChanceWalkerDestroy && _walkers.Count > 1)
                {
                    _walkers.RemoveAt(i);
                    break; //only want to destroy one per iteration
                }
            }
            //chance: walker picks new direction
            for (int i = 0; i < _walkers.Count; i++)
            {
                if (Random.value < ChanceWalkerChangeDir)
                {
                    walker thisWalker = _walkers[i];
                    thisWalker.dir = RandomDirection();
                    _walkers[i] = thisWalker;
                }
            }
            //chance: spawn new walker
            numberChecks = _walkers.Count; //update how many walkers since we may have destroyed one
            for (int i = 0; i < numberChecks; i++)
            {
                //only if we don't have too many walkers, and only rarely
                if (Random.value < ChanceWalkerSpawn && _walkers.Count < MaxWalkers)
                {
                    //create a walker and initialize it
                    walker newWalker = new walker();
                    newWalker.dir = RandomDirection();
                    newWalker.pos = _walkers[i].pos;
                    _walkers.Add(newWalker);
                }
            }
            //move walkers
            for (int i = 0; i < _walkers.Count; i++)
            {
                walker thisWalker = _walkers[i];
                thisWalker.pos += thisWalker.dir;
                _walkers[i] = thisWalker;
            }
            //avoid grid border
            for (int i = 0; i < _walkers.Count; i++)
            {
                walker thisWalker = _walkers[i];
                //clamp x,y to leave a 10 slot space around the edges where we can put other items
                thisWalker.pos.x = Mathf.Clamp(thisWalker.pos.x, 10, _newGrid.roomWidth - 10);
                thisWalker.pos.y = Mathf.Clamp(thisWalker.pos.y, 10, _newGrid.roomHeight - 10);
                _walkers[i] = thisWalker;
            }
            //check if we want to exit the loop
            if ((float)_newGrid.NumberOfFloors() / _newGrid.GetGridLength() > PercentToFill)
            {
                break;
            }
            iterations++;
        } while (iterations < 100000);
    }

    private void FillHoles()
    {
        int fullSlotsCount = 0;

        //loop through every grid space
        for (int x = 1; x < _newGrid.roomWidth - 1; x++)
        {
            for (int y = 1; y < _newGrid.roomHeight - 1; y++)
            {
                //if we find an empty spot, check the spaces around it and count how many have a floor
                if (_newGrid.GetTileType(x, y) == GridHandler.gridSpace.empty)
                {
                    // Top-left
                    if (_newGrid.GetTileType(x - 1, y - 1) == GridHandler.gridSpace.floor)
                        fullSlotsCount++;

                    // Above
                    if (_newGrid.GetTileType(x, y - 1) == GridHandler.gridSpace.floor)
                        fullSlotsCount++;

                    // Top-right
                    if (_newGrid.GetTileType(x + 1, y + 1) == GridHandler.gridSpace.floor)
                        fullSlotsCount++;

                    // Left
                    if (_newGrid.GetTileType(x - 1, y) == GridHandler.gridSpace.floor)
                        fullSlotsCount++;

                    // Right
                    if (_newGrid.GetTileType(x + 1, y) == GridHandler.gridSpace.floor)
                        fullSlotsCount++;

                    // Bottom-left
                    if (_newGrid.GetTileType(x - 1, y - 1) == GridHandler.gridSpace.floor)
                        fullSlotsCount++;

                    // Bottom
                    if (_newGrid.GetTileType(x, y - 1) == GridHandler.gridSpace.floor)
                        fullSlotsCount++;

                    // Bottom-Right
                    if (_newGrid.GetTileType(x + 1, y - 1) == GridHandler.gridSpace.floor)
                        fullSlotsCount++;

                    if (fullSlotsCount > 6)
                        _newGrid.SetTile(x, y, GridHandler.gridSpace.floor);

                    fullSlotsCount = 0;
                }
            }
        }
    }

    /// <summary>
    /// Find places where tiles should be grass
    /// These are walkable tiles
    /// </summary>
    private void AddGrass()
    {
        //loop through every grid space. This is where we're adding the border grass
        for (int x = 0; x < _newGrid.roomWidth - 1; x++)
        {
            for (int y = 0; y < _newGrid.roomHeight - 1; y++)
            {
                //if we find a floor, check the spaces around it
                if (_newGrid.GetTileType(x, y) == GridHandler.gridSpace.floor)
                {
                    if (_noiseMap[x, y] > 0.4f)
                    {
                        _newGrid.SetTile(x, y, GridHandler.gridSpace.lightGrass);
                    }
                    else if (_noiseMap[x, y] > 0.2f)
                    {
                        _newGrid.SetTile(x, y, GridHandler.gridSpace.darkGrass);
                    }

                    //if any surrounding spaces are empty, make grass
                    if (_newGrid.GetTileType(x, y + 1) == GridHandler.gridSpace.empty)
                    {
                        _newGrid.SetTile(x, y, GridHandler.gridSpace.darkGrass);
                        _newGrid.SetTile(x, y + 1, GridHandler.gridSpace.darkGrass);
                    }

                    if (_newGrid.GetTileType(x, y - 1) == GridHandler.gridSpace.empty)
                    {
                        _newGrid.SetTile(x, y, GridHandler.gridSpace.darkGrass);
                        _newGrid.SetTile(x, y - 1, GridHandler.gridSpace.darkGrass);
                    }

                    if (_newGrid.GetTileType(x + 1, y) == GridHandler.gridSpace.empty)
                    {
                        _newGrid.SetTile(x, y, GridHandler.gridSpace.darkGrass);
                        _newGrid.SetTile(x + 1, y, GridHandler.gridSpace.darkGrass);
                    }

                    if (_newGrid.GetTileType(x - 1, y) == GridHandler.gridSpace.empty)
                    {
                        _newGrid.SetTile(x, y, GridHandler.gridSpace.darkGrass);
                        _newGrid.SetTile(x - 1, y, GridHandler.gridSpace.darkGrass);
                    }
                }
            }
        }

        CleanupLightGrass();
    }

    /// <summary>
    /// Light grass shouldn't have single-file tiles
    /// </summary>
    private void CleanupLightGrass()
    {
        for (int x = 0; x < _newGrid.roomWidth - 1; x++)
        {
            for (int y = 0; y < _newGrid.roomHeight - 1; y++)
            {
                if (_newGrid.GetTileType(x, y) == GridHandler.gridSpace.lightGrass)
                {
                    bool isSingleVerticalFile = _newGrid.GetTileType(x + 1, y) == GridHandler.gridSpace.darkGrass &&
                                        _newGrid.GetTileType(x - 1, y) == GridHandler.gridSpace.darkGrass;
                    bool isSingleHorizontalFile = _newGrid.GetTileType(x, y + 1) == GridHandler.gridSpace.darkGrass &&
                                        _newGrid.GetTileType(x, y - 1) == GridHandler.gridSpace.darkGrass;

                    if (isSingleVerticalFile)
                    {
                        _newGrid.SetTile(x + 1, y, GridHandler.gridSpace.lightGrass);
                    }
                    if (isSingleHorizontalFile)
                    {
                        _newGrid.SetTile(x, y + 1, GridHandler.gridSpace.lightGrass);
                    }
                }
            }
        }
    }

    /// <summary>
    /// This is where we fill the bounding area with 1x1, 2x2 or 3x3 assets
    /// These will have collision
    /// </summary>
    private void AddBorders()
    {
        AssetPlacer Placer = new AssetPlacer();

        Placer.grid = _newGrid;

        //loop through every grid space
        for (int x = 0; x < _newGrid.roomWidth - 1; x++)
        {
            for (int y = 0; y < _newGrid.roomHeight - 1; y++)
            {
                //if we find a floor, check the spaces around it
                if (_newGrid.GetTileType(x, y) == GridHandler.gridSpace.darkGrass)
                {
                    //if any surrounding spaces are empty, place a wall there
                    if (_newGrid.GetTileType(x, y + 1) == GridHandler.gridSpace.empty)
                        Placer.FindLargestPossibleTile(_newGrid, x, y, 0, 1);

                    if (_newGrid.GetTileType(x, y - 1) == GridHandler.gridSpace.empty)
                        Placer.FindLargestPossibleTile(_newGrid, x, y, 0, -1);

                    if (_newGrid.GetTileType(x + 1, y) == GridHandler.gridSpace.empty)
                        Placer.FindLargestPossibleTile(_newGrid, x, y, 1, 0);

                    if (_newGrid.GetTileType(x - 1, y) == GridHandler.gridSpace.empty)
                        Placer.FindLargestPossibleTile(_newGrid, x, y, -1, 0);
                }
            }
        }
    }



    /// <summary>
    /// Check every cell, and spawn appropriate tile
    /// </summary>
    private void SpawnLevelTiles()
    {
        for (int x = 0; x < _newGrid.roomWidth; x++)
        {
            for (int y = 0; y < _newGrid.roomHeight; y++)
            {
                switch (_newGrid.GetTileType(x, y))
                {
                    case GridHandler.gridSpace.empty:
                        break;
                    case GridHandler.gridSpace.floor:
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                    case GridHandler.gridSpace.darkGrass:
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                    case GridHandler.gridSpace.lightGrass:
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        lightGrassMap.SetTile(new Vector3Int(x, y, 0), lightGrassTile);
                        break;
                    case GridHandler.gridSpace.wall:
                        topMap.SetTile(new Vector3Int(x, y, 0), topTile);
                        break;
                    case GridHandler.gridSpace.err:
                        topMap.SetTile(new Vector3Int(x, y, 0), errTile);
                        break;
                    case GridHandler.gridSpace.obj3x3:
                        Instantiate(Tile3x3, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                    case GridHandler.gridSpace.used3x3:
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                    case GridHandler.gridSpace.obj2x2:
                        Instantiate(Tile2x2, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                    case GridHandler.gridSpace.used2x2:
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                    case GridHandler.gridSpace.obj1x1:
                        Instantiate(Tile1x1, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                }
            }
        }
    }

    private Vector2 RandomDirection()
    {
        //pick a random int between 0 and 3, representing the 4 directions we can travel
        int choice = Mathf.FloorToInt(Random.value * 3.99f);
        //now let's return a direction
        switch (choice)
        {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            default:
                return Vector2.right;
        }
    }

    void Spawn(float x, float y, GameObject toSpawn)
    {
        //find the position to spawn
        Vector2 offset = _roomSizeWorldUnits / 2.0f;
        Vector2 spawnPos = new Vector2(x, y) * WorldUnitsInOneGridCell - offset;
        //spawn object
        Instantiate(toSpawn, spawnPos, Quaternion.identity);
    }
}
