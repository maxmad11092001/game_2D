using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName ="ToolClass", menuName ="Tool Class")]
public class ToolClass : ScriptableObject
{
    public string name;
    public Sprite sprite;
    public ItemClass.ToolType tooltype;
}
