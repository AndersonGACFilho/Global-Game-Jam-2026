using UnityEngine;

public class ItemPickup : ItemBehavior
{
    [Header("Inventory Item")]
    [SerializeField] private InventoryItemDefinition itemDefinition;
    [SerializeField] private int amount = 1;

    public override string DisplayName => itemDefinition != null ? itemDefinition.displayName : base.DisplayName;

    public override string Prompt
    {
        get
        {
            if (itemDefinition == null) return base.Prompt;

            var inv = PlayerInventory.Instance;
            if (inv == null) return base.Prompt;

            return inv.CanAdd(itemDefinition, amount)
                ? "Press [E] to pick up"
                : "Inventory full";
        }
    }

    public override void Interact(GameObject interactor)
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning($"ItemPickup: no itemDefinition on '{name}'");
            return;
        }

        var inv = interactor.GetComponentInParent<PlayerInventory>();
        if (inv == null) inv = PlayerInventory.Instance;

        if (inv == null)
        {
            Debug.LogWarning($"ItemPickup: no PlayerInventory found (interactor='{interactor.name}')");
            return;
        }

        if (!inv.TryAdd(itemDefinition, amount))
        {
            Debug.Log($"Inventory full, can't pick up '{itemDefinition.displayName}'");
            return;
        }

        Debug.Log($"Picked up: {itemDefinition.displayName} x{amount}");
        Destroy(gameObject);
    }

}