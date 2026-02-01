using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Key")]
public class KeyItemDefinition : InventoryItemDefinition
{
    [Header("Key")]
    public string keyId = "door_key_a";

    private void OnValidate()
    {
        stackable = false;
        maxStack = 1;
        usesDurability = false;
        equippable = false;
    }
}