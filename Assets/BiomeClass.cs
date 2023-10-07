using System.Collections;
using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    public string biomeName;
    public Color biomeCol;
    public TileAtlas tileAtlas;

    [Header("Noise Setting")]
    public Texture2D caveNoiseTexture;

    [Header("Generation Setting")]
    public bool generateCaves = true;
    public int dirtLayerHeight = 5;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 4f;

    [Header("Trees")]
    public int treeChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Addons")]
    public int tallGrassChance = 10;

    [Header("Ore Setting")]
    public OreClass[] ores;
}
