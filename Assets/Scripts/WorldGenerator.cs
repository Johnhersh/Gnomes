using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Diagnostics; // This is for the Stopwatch in Update()

[ExecuteInEditMode]
public class WorldGenerator : MonoBehaviour
{
    public bool Generate; // Hacky. This is just to be able to preview things in the editor
    public bool Clear;
    private List<GameObject> Allprefabs = null;

    void Update()
    {
        if (Generate)
        {
            Stopwatch st = new Stopwatch(); // Start measuring how long it takes to generate a map. Will use later for optimiazations
            Generate = false;
            st.Start();
            Start();
            st.Stop();
            UnityEngine.Debug.Log(string.Format("Generating took {0} ms to complete", st.ElapsedMilliseconds));
        }

        if (Clear)
        {
            topMap.ClearAllTiles();
            botMap.ClearAllTiles();
            darkGrassMap.ClearAllTiles();
            lightGrassMap.ClearAllTiles();
            foreach (GameObject obj in Allprefabs)
            {
                DestroyImmediate(obj);
            }
            Clear = false;
        }
    }

    public enum gridSpace { empty, floor, wall, darkGrass, err, obj3x3, obj2x2, obj1x1, used3x3, used2x2 };
    public gridSpace[,] grid;
    int roomHeight, roomWidth;
    Vector2 roomSizeWorldUnits = new Vector2(150, 150); // This is the size of the map
    float[,] noiseMap = new float[150, 150]; // This is where we keep the perlin noise. Used for adding grass
    float worldUnitsInOneGridCell = 1;

    struct walker
    {
        public Vector2 dir;
        public Vector2 pos;
    }
    List<walker> walkers; // This will contain all our active walkers
    float chanceWalkerChangeDir = 0.3f;
    float chanceWalkerSpawn = 0.03f;
    float chanceWalkerDestroy = 0.05f;
    int maxWalkers = 12;
    float percentToFill = 0.2f; //What percentage of the grid should be filled before we move on

    ///////////////////////
    // These are accessible via the editor:
    ///////////////////////
    public Tilemap topMap, darkGrassMap, lightGrassMap, botMap;
    public RuleTile topTile, darkGrassTile, lightGrassTile, botTile;
    public RuleTile errTile; //This tile is just used for debugging
    public GameObject Tile1x1;
    public GameObject Tile2x2;
    public GameObject Tile3x3;
    public float noiseScale = 1f;


    // Start is called before the first frame update
    void Start()
    {
        Setup();
        CreateFloors();

        // CreateWalls();
        FillHoles(); // After creating the base floor, I don't want to have empty cells
        FillHoles(); // Catch any newly created gaps

        AddFirstGrassLayer();

        AddBorders();

        SpawnLevel();
        // SpawnBorderTrees();
    }

    void Setup()
    {
        //find grid size
        roomHeight = Mathf.RoundToInt(roomSizeWorldUnits.x / worldUnitsInOneGridCell);
        roomWidth = Mathf.RoundToInt(roomSizeWorldUnits.y / worldUnitsInOneGridCell);

        // UnityEngine.Debug.Log("Room Height during setup: " + roomHeight);
        //create grid
        grid = new gridSpace[roomWidth, roomHeight];
        //make sure our tilemaps are clear. Can be an issue after coming from the editor
        topMap.ClearAllTiles();
        botMap.ClearAllTiles();
        darkGrassMap.ClearAllTiles();
        lightGrassMap.ClearAllTiles();
        //set grid's default state
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                //make every cell "empty"
                grid[x, y] = gridSpace.empty;
            }
        }

        //set first walker
        walkers = new List<walker>(); // init list
        walker newWalker = new walker(); //create a walker
        newWalker.dir = RandomDirection();
        //find center of grid
        Vector2 spawnPos = new Vector2(Mathf.RoundToInt(roomWidth / 2.0f),
                                    Mathf.RoundToInt(roomHeight / 2.0f));
        newWalker.pos = spawnPos;
        //add walker to our list
        walkers.Add(newWalker);

        //Generate noisemap
        for (int xIndex = 0; xIndex < roomWidth; xIndex++)
        {
            for (int yIndex = 0; yIndex < roomHeight; yIndex++)
            {
                // Mathf.PerlinNoise requires floats as input
                float sampleX = xIndex / noiseScale;
                float sampleY = yIndex / noiseScale;
                noiseMap[xIndex, yIndex] = Mathf.PerlinNoise(sampleX, sampleY);
            }
        }
    }

    void CreateFloors()
    {
        int iterations = 0; // Just want to keep track of how many times we've looped so we don't get an infinite loop. This is just in case
        do
        {
            //create floor at position of every walker
            foreach (walker myWalker in walkers)
            {
                grid[(int)myWalker.pos.x, (int)myWalker.pos.y] = gridSpace.floor;
                //Make the path 2 tiles thick:
                //If we're moving up or down, also create the tile to the right of the path
                if ((int)myWalker.dir.y != 0)
                    grid[(int)myWalker.pos.x + 1, (int)myWalker.pos.y] = gridSpace.floor;
                //If we're moving left or right, also create the tile to above the path
                if ((int)myWalker.dir.x != 0)
                    grid[(int)myWalker.pos.x, (int)myWalker.pos.y + 1] = gridSpace.floor;
            }
            //chance: destroy walker
            int numberChecks = walkers.Count; //see how many walkers we have
            for (int i = 0; i < numberChecks; i++)
            {
                //only if it's not the only one, and only rarely
                if (Random.value < chanceWalkerDestroy && walkers.Count > 1)
                {
                    walkers.RemoveAt(i);
                    break; //only want to destroy one per iteration
                }
            }
            //chance: walker picks new direction
            for (int i = 0; i < walkers.Count; i++)
            {
                if (Random.value < chanceWalkerChangeDir)
                {
                    walker thisWalker = walkers[i];
                    thisWalker.dir = RandomDirection();
                    walkers[i] = thisWalker;
                }
            }
            //chance: spawn new walker
            numberChecks = walkers.Count; //update how many walkers since we may have destroyed one
            for (int i = 0; i < numberChecks; i++)
            {
                //only if we don't have too many walkers, and only rarely
                if (Random.value < chanceWalkerSpawn && walkers.Count < maxWalkers)
                {
                    //create a walker and initialize it
                    walker newWalker = new walker();
                    newWalker.dir = RandomDirection();
                    newWalker.pos = walkers[i].pos;
                    walkers.Add(newWalker);
                }
            }
            //move walkers
            for (int i = 0; i < walkers.Count; i++)
            {
                walker thisWalker = walkers[i];
                thisWalker.pos += thisWalker.dir;
                walkers[i] = thisWalker;
            }
            //avoid grid border
            for (int i = 0; i < walkers.Count; i++)
            {
                walker thisWalker = walkers[i];
                //clamp x,y to leave a 10 slot space around the edges where we can put other items
                thisWalker.pos.x = Mathf.Clamp(thisWalker.pos.x, 10, roomWidth - 10);
                thisWalker.pos.y = Mathf.Clamp(thisWalker.pos.y, 10, roomHeight - 10);
                walkers[i] = thisWalker;
            }
            //check if we want to exit the loop
            if ((float)NumberOfFloors() / (float)grid.Length > percentToFill)
            {
                break;
            }
            iterations++;
        } while (iterations < 100000);
    }

    void FillHoles()
    {
        int fullSlotsCount = 0;

        //loop through every grid space
        for (int x = 1; x < roomWidth - 1; x++)
        {
            for (int y = 1; y < roomHeight - 1; y++)
            {
                //if we find an empty spot, check the spaces around it and count how many have a floor
                if (grid[x, y] == gridSpace.empty)
                {
                    // Top-left
                    if (grid[x - 1, y - 1] == gridSpace.floor)
                        fullSlotsCount++;

                    // Above
                    if (grid[x, y - 1] == gridSpace.floor)
                        fullSlotsCount++;

                    // Top-right
                    if (grid[x + 1, y + 1] == gridSpace.floor)
                        fullSlotsCount++;

                    // Left
                    if (grid[x - 1, y] == gridSpace.floor)
                        fullSlotsCount++;

                    // Right
                    if (grid[x + 1, y] == gridSpace.floor)
                        fullSlotsCount++;

                    // Bottom-left
                    if (grid[x - 1, y - 1] == gridSpace.floor)
                        fullSlotsCount++;

                    // Bottom
                    if (grid[x, y - 1] == gridSpace.floor)
                        fullSlotsCount++;

                    // Bottom-Right
                    if (grid[x + 1, y - 1] == gridSpace.floor)
                        fullSlotsCount++;

                    if (fullSlotsCount > 6)
                        grid[x, y] = gridSpace.floor;

                    fullSlotsCount = 0;
                }
            }
        }
    }

    void AddFirstGrassLayer()
    {
        //loop through every grid space. This is where we're adding the border grass
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                //if we find a floor, check the spaces around it
                if (grid[x, y] == gridSpace.floor)
                {
                    //if any surrounding spaces are empty, make grass
                    if (grid[x, y + 1] == gridSpace.empty)
                    {
                        grid[x, y] = gridSpace.darkGrass;
                        grid[x, y + 1] = gridSpace.darkGrass;
                    }

                    if (grid[x, y - 1] == gridSpace.empty)
                    {
                        grid[x, y] = gridSpace.darkGrass;
                        grid[x, y - 1] = gridSpace.darkGrass;
                    }

                    if (grid[x + 1, y] == gridSpace.empty)
                    {
                        grid[x, y] = gridSpace.darkGrass;
                        grid[x + 1, y] = gridSpace.darkGrass;
                    }

                    if (grid[x - 1, y] == gridSpace.empty)
                    {
                        grid[x, y] = gridSpace.darkGrass;
                        grid[x - 1, y] = gridSpace.darkGrass;
                    }
                }

                // Add noise-based grass
                if (noiseMap[x, y] > 0.2f)
                {
                    if (grid[x, y] == gridSpace.floor)
                    { // I don't want to draw grass outside of the level area
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                    }
                    // UnityEngine.Debug.Log("Found grass tile at: " + x + "," + y);
                }
                if (noiseMap[x, y] > 0.4f)
                {
                    if (grid[x, y] == gridSpace.floor)
                    { // I don't want to draw grass outside of the level area
                        lightGrassMap.SetTile(new Vector3Int(x, y, 0), lightGrassTile);
                    }
                }
            }
        }
    }

    void CreateWalls()
    {
        //loop through every grid space
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                //if we find a floor, check the spaces around it
                if (grid[x, y] == gridSpace.floor)
                {
                    //if any surrounding spaces are empty, place a wall there
                    if (grid[x, y + 1] == gridSpace.empty)
                        grid[x, y + 1] = gridSpace.wall;

                    if (grid[x, y - 1] == gridSpace.empty)
                        grid[x, y - 1] = gridSpace.wall;

                    if (grid[x + 1, y] == gridSpace.empty)
                        grid[x + 1, y] = gridSpace.wall;

                    if (grid[x - 1, y] == gridSpace.empty)
                        grid[x - 1, y] = gridSpace.wall;
                }
            }
        }
    }

    void AddBorders() // This is where we fill the bounding area with 1x1, 2x2 or 3x3 assets
    {
        AssetPlacer Placer = new AssetPlacer();

        Placer.grid = grid;

        //loop through every grid space
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                //if we find a floor, check the spaces around it
                if (grid[x, y] == gridSpace.darkGrass)
                {
                    //if any surrounding spaces are empty, place a wall there
                    if (grid[x, y + 1] == gridSpace.empty)
                        grid = Placer.FindLargestPossibleTile(x, y, 0, 1);

                    if (grid[x, y - 1] == gridSpace.empty)
                        grid = Placer.FindLargestPossibleTile(x, y, 0, -1);

                    if (grid[x + 1, y] == gridSpace.empty)
                        grid = Placer.FindLargestPossibleTile(x, y, 1, 0);

                    if (grid[x - 1, y] == gridSpace.empty)
                        grid = Placer.FindLargestPossibleTile(x, y, -1, 0);
                }
            }
        }
    }



    void SpawnLevel()
    {
        //Check every cell, and spawn appropriate tile
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                switch (grid[x, y])
                {
                    case gridSpace.empty:
                        break;
                    case gridSpace.floor:
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                    case gridSpace.darkGrass:
                        darkGrassMap.SetTile(new Vector3Int(x, y, 0), darkGrassTile);
                        botMap.SetTile(new Vector3Int(x, y, 0), botTile);
                        break;
                    case gridSpace.wall:
                        topMap.SetTile(new Vector3Int(x, y, 0), topTile);
                        break;
                    case gridSpace.err:
                        topMap.SetTile(new Vector3Int(x, y, 0), errTile);
                        break;
                    case gridSpace.obj3x3:
                        Allprefabs.Add(Instantiate(Tile3x3, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity));
                        // topMap.SetTile(new Vector3Int(x, y, 0), topTile);
                        break;
                    case gridSpace.obj2x2:
                        Allprefabs.Add(Instantiate(Tile2x2, new Vector3(x + 0.5f, y - 0.5f, 0), Quaternion.identity));
                        break;
                    case gridSpace.obj1x1:
                        Allprefabs.Add(Instantiate(Tile1x1, new Vector3(x + 0.5f, y - 0.5f, 0), Quaternion.identity));
                        break;
                }
            }
        }
    }

    Vector2 RandomDirection()
    {
        //pick a rando int between 0 and 3, representing the 4 directions we can travel
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

    int NumberOfFloors()
    {
        int count = 0;
        foreach (gridSpace space in grid)
        {
            if (space == gridSpace.floor)
                count++;
        }
        return count;
    }

    void Spawn(float x, float y, GameObject toSpawn)
    {
        //find the position to spawn
        Vector2 offset = roomSizeWorldUnits / 2.0f;
        Vector2 spawnPos = new Vector2(x, y) * worldUnitsInOneGridCell - offset;
        //spawn object
        Instantiate(toSpawn, spawnPos, Quaternion.identity);
    }
}
