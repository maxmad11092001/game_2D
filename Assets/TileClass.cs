using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "newtileclass", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
   public string tileName;
   public TileClass wallVariant;

    /*public Sprite tileSprite;*/
    public Sprite[] tileSprites;
    public bool inBackground = false;
    public TileClass tileDrop;
    public ItemClass.ToolType toolToBreak;
    public bool naturallyPlaced = true;
    internal bool isStackable;

    public static TileClass CreateInstance(TileClass tile, bool isNaturallyPlaced)
    {
        var thisTile = ScriptableObject.CreateInstance<TileClass>();
        thisTile.Init(tile, isNaturallyPlaced);
        return thisTile;
    }

    public void Init (TileClass tile, bool isNaturallyPlaced)
    {
        tileName = tile.name;
        wallVariant = tile.wallVariant;
        tileSprites = tile.tileSprites;
        inBackground = tile.inBackground;
        tileDrop = tile.tileDrop;
        toolToBreak = tile.toolToBreak;
        naturallyPlaced |= tile.naturallyPlaced;
    }
}
