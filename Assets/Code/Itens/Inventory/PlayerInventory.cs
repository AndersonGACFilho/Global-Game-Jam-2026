using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Serializable]
    public class Slot
    {
        public InventoryItemDefinition item;
        public int count;
        public float durability;
        public bool equipped;

        public bool IsEmpty => item == null || count <= 0;

        public void Clear()
        {
            item = null;
            count = 0;
            durability = 0f;
            equipped = false;
        }
    }

    [Header("Slots (Hotbar)")]
    [SerializeField] private int maxSlots = 5;
    [SerializeField] private Slot[] slots;
    
    [Header("Keys (not shown in UI)")]
    [SerializeField]private readonly Dictionary<string, int> _keys = new Dictionary<string, int>();


    [Header("Selection")]
    [SerializeField] private int selectedIndex = 0;

    public int MaxSlots => maxSlots;
    public int SelectedIndex => selectedIndex;
    public Slot[] Slots => slots;

    public event Action OnChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (slots == null || slots.Length != maxSlots)
        {
            slots = new Slot[maxSlots];
            for (int i = 0; i < slots.Length; i++) slots[i] = new Slot();
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Tick de itens equipados (ex: máscara drenando)
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null || s.IsEmpty || !s.equipped) continue;

            s.item.TickEquipped(this, gameObject, s, dt);

            // se quebrou/zerou durante o tick
            if (s.IsEmpty || (s.item != null && s.item.usesDurability && s.durability <= 0f))
                RemoveAt(i);
        }
    }

    public void Select(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, maxSlots - 1);
        OnChanged?.Invoke();
    }

    public bool CanAdd(InventoryItemDefinition def, int amount = 1)
    {
        if (def == null || amount <= 0) return false;
        
        if (def is KeyItemDefinition) return true;

        if (def.stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                if (s.IsEmpty) continue;
                if (s.item == def && s.count < def.maxStack)
                    return true;
            }
        }

        for (int i = 0; i < slots.Length; i++)
            if (slots[i].IsEmpty) return true;

        return false;
    }

    public bool TryAdd(InventoryItemDefinition def, int amount = 1)
    {
        if (def == null || amount <= 0) return false;

        // Keys go to the keyring (no slot cost)
        if (def is KeyItemDefinition keyDef)
        {
            AddKey(keyDef.keyId, amount);
            return true;
        }

        if (!CanAdd(def, amount)) return false;

        if (def.stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty) continue;
                if (slots[i].item != def) continue;
                if (slots[i].count >= def.maxStack) continue;

                int canAdd = def.maxStack - slots[i].count;
                int toAdd = Mathf.Min(canAdd, amount);
                slots[i].count += toAdd;
                amount -= toAdd;

                if (amount <= 0)
                {
                    OnChanged?.Invoke();
                    return true;
                }
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty) continue;

            slots[i].item = def;
            slots[i].count = Mathf.Min(amount, def.stackable ? def.maxStack : 1);
            slots[i].durability = def.usesDurability ? def.maxDurability : 0f;

            amount -= slots[i].count;

            if (amount <= 0)
            {
                OnChanged?.Invoke();
                return true;
            }
        }

        OnChanged?.Invoke();
        return amount <= 0;
    }

    public bool UseSelected()
    {
        return UseSlot(selectedIndex);
    }

    public bool UseSlot(int index)
    {
        if (!IsValid(index)) return false;

        var s = slots[index];
        if (s.IsEmpty) return false;

        if (!s.item.CanUse(this, gameObject, s))
            return false;

        s.item.Use(this, gameObject, s);

        // pós-uso: limpar se acabou
        if (s.item != null && s.item.usesDurability && s.durability <= 0f)
            RemoveAt(index);
        else if (s.count <= 0)
            RemoveAt(index);

        OnChanged?.Invoke();
        return true;
    }

    public bool ToggleEquipSelected()
    {
        return ToggleEquip(selectedIndex);
    }

    public bool ToggleEquip(int index)
    {
        if (!IsValid(index)) return false;

        var s = slots[index];
        if (s.IsEmpty) return false;
        if (!s.item.equippable) return false;

        if (s.equipped)
        {
            Unequip(index);
            return true;
        }

        Equip(index);
        return true;
    }

    public void Equip(int index)
    {
        if (!IsValid(index)) return;
        var s = slots[index];
        if (s.IsEmpty || !s.item.equippable) return;

        // garante 1 item equipado por "equipGroup"
        string group = s.item.equipGroup;
        if (!string.IsNullOrEmpty(group))
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i == index) continue;
                var other = slots[i];
                if (other.IsEmpty || !other.equipped) continue;

                if (other.item != null && other.item.equipGroup == group)
                    Unequip(i);
            }
        }

        s.equipped = true;
        s.item.OnEquip(this, gameObject, s);
        OnChanged?.Invoke();
    }

    public void Unequip(int index)
    {
        if (!IsValid(index)) return;

        var s = slots[index];
        if (s.IsEmpty || !s.equipped) return;

        s.equipped = false;
        s.item.OnUnequip(this, gameObject, s);
        OnChanged?.Invoke();
    }

    public bool HasKey(string keyId)
    {
        if (string.IsNullOrEmpty(keyId)) return true;
        return _keys.TryGetValue(keyId, out var c) && c > 0;
    }

    public int GetKeyCount(string keyId)
    {
        if (string.IsNullOrEmpty(keyId)) return 0;
        return _keys.TryGetValue(keyId, out var c) ? c : 0;
    }

    public void AddKey(string keyId, int amount = 1)
    {
        if (string.IsNullOrEmpty(keyId) || amount <= 0) return;

        if (_keys.ContainsKey(keyId)) _keys[keyId] += amount;
        else _keys[keyId] = amount;

        OnChanged?.Invoke();
    }

    public bool TryConsumeKey(string keyId, int amount = 1)
    {
        if (string.IsNullOrEmpty(keyId) || amount <= 0) return false;

        if (!_keys.TryGetValue(keyId, out var cur) || cur <= 0) return false;

        cur -= amount;
        if (cur <= 0) _keys.Remove(keyId);
        else _keys[keyId] = cur;

        OnChanged?.Invoke();
        return true;
    }

    public void RemoveAt(int index)
    {
        if (!IsValid(index)) return;
        var s = slots[index];
        if (s == null || s.IsEmpty) return;

        if (s.equipped && s.item != null)
            s.item.OnUnequip(this, gameObject, s);

        s.Clear();
        OnChanged?.Invoke();
    }

    public int IndexOf(Slot slot)
    {
        if (slot == null) return -1;
        for (int i = 0; i < slots.Length; i++)
            if (ReferenceEquals(slots[i], slot)) return i;
        return -1;
    }

    private bool IsValid(int index) => index >= 0 && index < slots.Length;
}
