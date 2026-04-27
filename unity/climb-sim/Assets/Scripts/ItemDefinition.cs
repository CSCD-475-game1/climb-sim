using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType;
}
