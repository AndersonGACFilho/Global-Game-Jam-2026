using UnityEngine;
using Code.States;

[CreateAssetMenu(menuName = "Game/Items/Mask")]
public class MaskItemDefinition : InventoryItemDefinition
{
    [Header("Mask Drain")]
    public float drainPerSecondPerVampire = 0.25f;

    private void OnValidate()
    {
        stackable = false;
        maxStack = 1;

        usesDurability = true;
        if (maxDurability < 1f) maxDurability = 10f;

        equippable = true;
        equipGroup = "mask";
    }

    public override void OnEquip(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot)
    {
        var sm = user.GetComponent<PlayerStateManager>();
        if (sm != null) sm.SetDisguised(true);
    }

    public override void OnUnequip(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot)
    {
        var sm = user.GetComponent<PlayerStateManager>();
        if (sm != null) sm.ForceExposed();
    }

    public override void TickEquipped(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot, float dt)
    {
        var sm = user.GetComponent<PlayerStateManager>();
        if (sm == null) return;

        int seeing = sm.VampiresSeeingCount;
        if (seeing <= 0) return;

        slot.durability -= drainPerSecondPerVampire * seeing * dt;

        if (slot.durability <= 0f)
        {
            // quebrou: desequipa e remove
            int idx = inv.IndexOf(slot);
            if (idx >= 0) inv.RemoveAt(idx);
        }
    }
}