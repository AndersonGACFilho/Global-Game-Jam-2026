using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Candlestick (Lure)")]
public class CandlestickItemDefinition : ThrowableLureItemDefinition
{
    private void OnValidate()
    {
        stackable = true;
        if (maxStack < 1) maxStack = 3;

        usesDurability = false;
        equippable = false;
    }
}