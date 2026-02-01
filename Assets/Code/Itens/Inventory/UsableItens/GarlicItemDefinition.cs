using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Garlic")]
public class GarlicItemDefinition : InventoryItemDefinition
{
    [Header("Garlic Effect")]
    public float radius = 3.5f;
    public float repelDuration = 1.25f;
    public float repelSpeedMultiplier = 1.35f;
    public LayerMask vampireMask = ~0;

    private void OnValidate()
    {
        stackable = true;
        if (maxStack < 1) maxStack = 5;
        usesDurability = false;
        equippable = false;
    }

    public override void Use(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot)
    {
        Vector2 p = user.transform.position;

        var hits = Physics2D.OverlapCircleAll(p, radius, vampireMask);
        foreach (var h in hits)
        {
            var v = h.GetComponentInParent<VampireAffectable>();
            if (v != null)
                v.ApplyRepel(p, repelDuration, repelSpeedMultiplier);
        }

        // destrói a cada uso
        slot.count -= 1;
    }
}