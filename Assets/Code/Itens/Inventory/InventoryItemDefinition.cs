using UnityEngine;

public abstract class InventoryItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public string itemId = "item_id";
    public string displayName = "Item";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Stack")]
    public bool stackable = false;
    public int maxStack = 1;

    [Header("Durability")]
    public bool usesDurability = false;
    public float maxDurability = 1f;

    [Header("Equip")]
    public bool equippable = false;

    [Tooltip("Itens com mesmo grupo se excluem (ex: só 1 máscara equipada).")]
    public string equipGroup = "";

    public virtual bool CanUse(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot) => true;

    public virtual void Use(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot) { }

    public virtual void OnEquip(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot) { }

    public virtual void OnUnequip(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot) { }

    public virtual void TickEquipped(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot, float dt) { }
}