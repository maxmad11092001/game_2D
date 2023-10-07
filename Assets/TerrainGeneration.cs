using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    /*[Header("Lighting")]
    public Texture2D worldTilesMap;
    public Material lightShader;
    public float lightThreshold;
    public float lightRadius = 7f;
    List<Vector2Int> unlitBlocks = new List<Vector2Int>();*/

    public PlayerController player;
    public CamController camera;
    public GameObject tileDrop;

    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed;

    public BiomeClass[] biomes;
    

    // Start is called before the first frame update

    [Header("Biomes")]
    public float biomeFrequency;
    public Gradient biomeGradient;
    public Texture2D biomeMap;
   

    [Header("Generation Setting")]
    public int chunkSize = 16;
    public int worldSize = 100;
    public int heightAddition = 25;
    public bool generateCaves = true;
    


    [Header("Noise Setting")]
    public Texture2D caveNoiseTexture;
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;

    //[Header("Ore Setting")]
    public OreClass[] ores;
    


    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();
    private List<GameObject> worldTileObjects = new List<GameObject>();
    private List<TileClass> worldTileClasses = new List<TileClass>();
    private TileClass[,] world_BackgroundTiles;
    private TileClass[,] world_ForegroundTiles;

    private Color[] biomeCols;
    private BiomeClass curBiome;

    

    private void Start()
    {
        //initilise light
        //worldTilesMap = new Texture2D(worldSize, worldSize);
        //worldTilesMap.filterMode = FilterMode.Point;
        //lightShader.SetTexture("_ShadowTex", worldTilesMap);

/*        for (int x = 0; x < worldSize; x++)
        {
            for(int y = 0; y < worldSize; y++)
            {
                worldTilesMap.SetPixel(x, y, Color.white);
            }
        }
        worldTilesMap.Apply();*/

        //generate terrian stuff
        seed = Random.Range(-10000, 10000);

        for (int i = 0;i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }

        biomeCols = new Color[biomes.Length];

        for (int i = 0; i < biomes.Length; i++)
        {
            biomeCols[i] = biomes[i].biomeCol;
        }


        DrawTextures();
        DrawBiomeMap();
        DrawCaveAndOres();

        CreateChunks();
        GenerateTerrain();

        /*for (int x =0; x < worldSize; x++)
        {
            for (int y = 0;y < worldSize; y++)
            {
                if (worldTilesMap.GetPixel(x, y) == Color.white)
                    LightBlock(x, y, 1f, 0);
            }
        }
        worldTilesMap.Apply();*/

        camera.Spawn(new Vector3(player.spawnPos.x, player.spawnPos.y, camera.transform.position.z));
        camera.worldSize = worldSize;
        player.Spawn();

        RefreshChunks();
    }

    /*public void Update()
    {
        RefreshChunks();

    }*/

    public void RefreshChunks()
    {
        for (int i = 0; i < worldChunks.Length; i++)  
        { 
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 5f)
                worldChunks[i].SetActive(false);
            else
                worldChunks[i].SetActive(true);
        }
    }

    public void DrawBiomeMap()
    {
        float b;
        Color col;
        biomeMap = new Texture2D(worldSize, worldSize);
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                b = Mathf.PerlinNoise((x + seed) * biomeFrequency, (y + seed) * biomeFrequency);
                col = biomeGradient.Evaluate(b);
                biomeMap.SetPixel(x, y, col);

            }
        }
        biomeMap.Apply();
    }

    public void DrawCaveAndOres()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        float v;
        float o;

        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for(int y = 0; y < caveNoiseTexture.height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                    v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                if (v > curBiome.surfaceValue)
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                else
                    caveNoiseTexture.SetPixel(x, y, Color.black);

                for (int i = 0; i < ores.Length; i++)
                {
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].frequency, (y + seed) * curBiome.ores[i].frequency);
                        if (o > curBiome.ores[i].size)
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);

                        ores[i].spreadTexture.Apply();
                    }
                }
            }
        }
        caveNoiseTexture.Apply();

      
    }

    public void DrawTextures()
    {
            biomeMap = new Texture2D(worldSize, worldSize);

        for (int i = 0; i < biomes.Length; i++)
        {

            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
                GenerateNoiseTextures(biomes[i].ores[o].frequency, biomes[i].ores[o].size, biomes[i].ores[o].spreadTexture);
            }
               
            
                
            }
    }

    private void GenerateNoiseTextures(float frequency, float limit ,Texture2D noiseTexture)
    {
        float v;
       
        for(int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);

                if (v > limit)
                noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }
        noiseTexture.Apply();
    }

    public void CreateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];
        for (int i = 0; i < numChunks; i++)
            {
                GameObject newChunk = new GameObject();
                newChunk.name = i.ToString();
                newChunk.transform.parent = this.transform;
                worldChunks[i] = newChunk;
            }
    }

    public BiomeClass GetCurrentBiome (int x, int y)
    {
       
        if (System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y)) >= 0)
            return biomes[System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y))];


        return curBiome;
    }

    public void GenerateTerrain()
    {
                TileClass tileClass;

        for (int x = 0; x < worldSize; x++)
        {
            float height; 
            for (int y = 0; y < worldSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);

                height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAddition;
                if (x == worldSize / 2)
                    player.spawnPos = new Vector2(x, height + 2);    

                if (y >= height)
                    break;
                {
                    if (y > height + curBiome.dirtLayerHeight)
                    {
                        tileClass = curBiome.tileAtlas.stone;

                        if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                            tileClass = tileAtlas.coal;
                        if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                            tileClass = tileAtlas.iron;
                        if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)
                            tileClass = tileAtlas.gold;
                        if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                            tileClass = tileAtlas.diamond;
                    }
                    else if (y < height - 1)
                    {
                        tileClass = curBiome.tileAtlas.dirt;
                    }
                    else
                    {
                        //top layer of the terrain
                        tileClass = curBiome.tileAtlas.grass;


                    }

                    if (generateCaves)
                    {

                        if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                        {
                            PlaceTile(tileClass, x, y, true);
                        }
                        else if (tileClass.wallVariant != null)
                        {
                            PlaceTile(tileClass.wallVariant, x, y, true);
                        }
                    }
                    else
                    {
                        PlaceTile(tileClass, x, y, true);
                    }
                    if (y >= height - 1)
                    {

                        int t = Random.Range(0, curBiome.treeChance);

                        if (t == 1)
                        {
                            //generate a tree
                            if (worldTiles.Contains(new Vector2(x, y)))
                            {
                                if (curBiome.biomeName == "Desert")
                                {
                                    //generate cacatus
                                    GenerateCactus(curBiome.tileAtlas, Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);

                                }
                                else
                                {
                                     GenerateTree(Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);

                                }
                            }
                        }
                        else
                        {
                            int i = Random.Range(0, curBiome.tallGrassChance);
                            //Generate Grass
                            if (i == 1)
                            {

                                if (worldTiles.Contains(new Vector2(x, y)))
                                {
                                    if (curBiome.tileAtlas.tallGrass != null)
                                        PlaceTile(curBiome.tileAtlas.tallGrass, x, y + 1, true);
                                }
                            }
                        }
                    }
                }
            }
        }
       // worldTilesMap.Apply();
    }



    void GenerateCactus(TileAtlas atlas ,int treeHeigth, int x, int y)
    {
        //define our tree

        for (int i = 0; i < treeHeigth; i++)
        {
            PlaceTile(atlas.log, x, y + i, true);
        }
    }

    void GenerateTree(int treeHeigth, int x, int y)
    {
        //define our tree

        //generate log
        for (int i = 0; i < treeHeigth; i++)
        {
            PlaceTile(tileAtlas.log, x, y + i, true);
        }
        //generate leaves
        PlaceTile(tileAtlas.leaf, x, y + treeHeigth, true);
        PlaceTile(tileAtlas.leaf, x, y + treeHeigth + 1, true);
        PlaceTile(tileAtlas.leaf, x, y + treeHeigth + 2, true);

        PlaceTile(tileAtlas.leaf, x -1, y + treeHeigth, true);
        PlaceTile(tileAtlas.leaf, x -1, y + treeHeigth + 1, true);

        PlaceTile(tileAtlas.leaf, x + 1, y + treeHeigth,true);
        PlaceTile(tileAtlas.leaf, x + 1, y + treeHeigth + 1, true);
    }

    public bool BreakTile(int x, int y, ItemClass item)
    {
        if (worldTiles.Contains(new Vector2Int(x, y)) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            if (worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].toolToBreak == ItemClass.ToolType.none)
            {
                RemoveTile(x, y);
                return true;
            }
            else
            {
                if (item != null)
                if (item.itemType == ItemClass.ItemType.tool)
                {
                    if (worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].toolToBreak == item.toolType)
                    {
                        RemoveTile(x, y);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void RemoveTile(int x, int y)
    {
        //TileClass tile = worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))];

        if (worldTiles.Contains(new Vector2Int(x, y)) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            if (worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].wallVariant != null)
            {
                if(worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].naturallyPlaced)
                PlaceTile(worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].wallVariant, x, y, true);
            }

            Destroy(worldTileObjects[worldTiles.IndexOf(new Vector2(x, y))]);
            //worldTilesMap.SetPixel(x, y, Color.white);
            //LightBlock(x, y, 1f, 0);

            //drop tile
            if (worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].tileDrop)
            {
                GameObject newtileDrop = Instantiate(tileDrop, new Vector2(x, y + 0.5f), Quaternion.identity);
                newtileDrop.GetComponent<SpriteRenderer>().sprite = worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].tileDrop.tileSprites[0];
                ItemClass tileDropItem = new ItemClass(worldTileClasses[worldTiles.IndexOf(new Vector2(x, y))].tileDrop);
                newtileDrop.GetComponent<TileDropController>().item = tileDropItem;
            }

            worldTileObjects.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));
            worldTileClasses.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));
            worldTiles.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));

            //worldTilesMap.Apply();
        }
    }

    public bool CheckTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    {

        if (x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            if (!worldTiles.Contains(new Vector2Int(x, y)))
            {
                //RemoveLightSource(x, y);
                //place the tile regardless
                PlaceTile(tile, x, y, isNaturallyPlaced);
                return true;
            }
            else
            {
                if (worldTileClasses[worldTiles.IndexOf(new Vector2Int(x, y))].inBackground)
                {
                    // overwrite existing tile
                   // RemoveLightSource(x, y);
                    //RemoveTile(x, y);
                    PlaceTile(tile, x, y, isNaturallyPlaced);
                    return true;

                }
                
            }
        }
        return false;
    }

    public void PlaceTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    {
        bool backgroundElement = tile.inBackground;
        if ( x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            
            GameObject newTile = new GameObject();

            int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;

            newTile.transform.parent = worldChunks[chunkCoord].transform;

            newTile.AddComponent<SpriteRenderer>();
            if (!backgroundElement)
            {
                newTile.AddComponent<BoxCollider2D>();
                newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "Ground";
            }

            int spriteIndex = Random.Range(0, tile.tileSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = tile.tileSprites[spriteIndex];
            if (tile.inBackground)
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;
                /*newTile.GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.6f, 0.6f);*/

            else
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;

            if (tile.name.ToUpper().Contains("WALL")) 
            {
                newTile.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
                //worldTilesMap.SetPixel(x, y, Color.black);
            }
            else if (!tile.inBackground)
                //worldTilesMap.SetPixel(x, y, Color.black);




            newTile.name = tile.tileSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            TileClass newTileClass = TileClass.CreateInstance(tile, isNaturallyPlaced);

            worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
            worldTileObjects.Add(newTile);
            worldTileClasses.Add(newTileClass);

        }
    }

   /* void LightBlock(int x, int y, float intensity, int iteration)
    {
        if (iteration < lightRadius)
        {
            worldTilesMap.SetPixel(x, y, Color.white * intensity);

            for (int nx = x -1; nx < x + 2; nx++)
            {
                for (int ny = y - 1; ny < y + 2; ny++)
                {
                    if (nx != x || ny != y)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                        float targetIntensity = Mathf.Pow(0.7f, dist) * intensity;
                        if (worldTilesMap.GetPixel(nx, ny) != null)
                        {
                            if (worldTilesMap.GetPixel(nx, ny).r < targetIntensity)
                            {
                                LightBlock(nx, ny, targetIntensity, iteration + 1);
                            }
                        }
                    }
                }
            }
            worldTilesMap.Apply();
        }

    }

    void RemoveLightSource(int x, int y)
    {
        unlitBlocks.Clear();
        UnLightBlock(x, y, x , y);

        List<Vector2Int> toReLight = new List<Vector2Int>();
        foreach (Vector2Int block in unlitBlocks)
        {
            for (int nx = block.x - 1; nx < block.x + 2; nx++)
            {
                for (int ny = block.y - 1; ny < block.y + 2; ny++)
                {
                    if (worldTilesMap.GetPixel(nx, ny) != null)
                    {
                        if (worldTilesMap.GetPixel(nx, ny).r > worldTilesMap.GetPixel(block.x, block.y).r)
                        {
                            if (!toReLight.Contains(new Vector2Int(nx, ny)))
                                toReLight.Add(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
        }
        foreach (Vector2Int source in toReLight)
        {
            LightBlock(source.x, source.y, worldTilesMap.GetPixel(source.x, source.y).r, 0);
        }
        worldTilesMap.Apply();
    }

    void UnLightBlock(int x, int y, int ix, int iy)
    {
        if (Mathf.Abs(x - ix) >= lightRadius || Mathf.Abs(y - iy) >= lightRadius || unlitBlocks.Contains(new Vector2Int(x, y)))
        return;

        for (int nx =  x - 1; nx < x + 2; nx++)
        {
            for (int ny = y - 1; ny < y + 2; ny++)
            {
                if ( nx != x || ny != y)
                {
                    if (worldTilesMap.GetPixel(nx, ny) != null)
                    {
                        if (worldTilesMap.GetPixel(nx, ny).r < worldTilesMap.GetPixel(x, y).r)
                        {
                            UnLightBlock(nx, ny, ix, iy);
                        }
                    }
                }
            }
        }

        worldTilesMap.SetPixel(x, y, Color.black);
        unlitBlocks.Add(new Vector2Int(x, y));
    }*/
}
