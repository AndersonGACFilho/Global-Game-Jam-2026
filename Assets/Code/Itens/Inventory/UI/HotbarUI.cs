using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private HotbarSlotUI slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private TextMeshProUGUI currentItemText;

    private readonly List<HotbarSlotUI> _slots = new();

    private void Awake()
    {
        if (slotsParent == null) slotsParent = transform;
    }

    private void OnEnable()
    {
        if (inventory == null) inventory = PlayerInventory.Instance;
        if (inventory != null) inventory.OnChanged += Refresh;

        BuildIfNeeded();
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    private void BuildIfNeeded()
    {
        if (inventory == null || slotPrefab == null) return;

        int needed = inventory.MaxSlots;

        // If already built correctly, keep it
        if (_slots.Count == needed) return;

        // Rebuild
        for (int i = slotsParent.childCount - 1; i >= 0; i--)
            Destroy(slotsParent.GetChild(i).gameObject);

        _slots.Clear();

        for (int i = 0; i < needed; i++)
        {
            var ui = Instantiate<HotbarSlotUI>(slotPrefab, slotsParent);
            ui.gameObject.name = $"Slot_{i}";
            _slots.Add(ui);
        }
    }

    public void Refresh()
    {
        if (inventory == null) inventory = PlayerInventory.Instance;
        if (inventory == null) return;
        BuildIfNeeded();

        for (int i = 0; i < _slots.Count; i++)
            _slots[i].Set(inventory, i, currentItemText);
    }
}