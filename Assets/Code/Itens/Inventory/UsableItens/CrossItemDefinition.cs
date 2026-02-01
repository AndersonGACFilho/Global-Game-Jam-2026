using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Cross")]
public class CrossItemDefinition : InventoryItemDefinition
{
    [Header("Cross Effect")]
    public float radius = 2.5f;
    public float stunDuration = 1.5f;
    public LayerMask vampireMask = ~0;

    [Header("Durability per use")]
    public float durabilityLossPerUse = 1f;

    private void OnValidate()
    {
        stackable = false;
        maxStack = 1;

        usesDurability = true;
        if (maxDurability < 1f) maxDurability = 3f;

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
                v.ApplyStun(stunDuration);
        }

        slot.durability -= durabilityLossPerUse;
        if (slot.durability <= 0f)
        {
            slot.count = 0; // força remover
        }
    }
}