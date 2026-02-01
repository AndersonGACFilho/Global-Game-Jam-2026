using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Wine Bottle (Lure)")]
public class WineBottleItemDefinition : ThrowableLureItemDefinition
{
    private void OnValidate()
    {
        stackable = true;
        if (maxStack < 1) maxStack = 5;

        usesDurability = false;
        equippable = false;
    }
}