using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image durabilityFill;     
    [SerializeField] private GameObject durabilityRoot;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private GameObject equippedMark;

    public void Set(PlayerInventory inv, int index, TextMeshProUGUI currentItemText)
    {
        if (inv == null || inv.Slots == null || index < 0 || index >= inv.Slots.Length)
        {
            SetEmpty(false);
            return;
        }

        var slot = inv.Slots[index];
        bool selected = index == inv.SelectedIndex;

        selectedFrame?.SetActive(selected);

        if (slot == null || slot.IsEmpty || slot.item == null)
        {
            SetEmpty(selected);
            currentItemText?.SetText("");
            return;
        }

        // Icon
        if (iconImage != null)
        {
            iconImage.enabled = slot.item.icon != null;
            iconImage.sprite = slot.item.icon;
            iconImage.color = Color.white;
        }

        // Count
        if (countText != null)
        {
            bool showCount = slot.item.stackable && slot.count > 1;
            countText.gameObject.SetActive(showCount);
            if (showCount) countText.text = slot.count.ToString();
        }

        // Durability
        bool usesDur = slot.item.usesDurability && slot.item.maxDurability > 0f;
        if (durabilityRoot != null) durabilityRoot.SetActive(usesDur);

        if (usesDur && durabilityFill != null)
        {
            durabilityFill.fillAmount = Mathf.Clamp01(slot.durability / slot.item.maxDurability);
        }

        // Equipped marker
        if (equippedMark != null)
            equippedMark.SetActive(slot.equipped);
        
        if (currentItemText != null && selected)
        {
            currentItemText.text = slot.item.displayName;
        }
        
    }

    private void SetEmpty(bool selected)
    {
        selectedFrame?.SetActive(selected);

        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }

        if (countText != null)
            countText.gameObject.SetActive(false);

        if (durabilityRoot != null)
            durabilityRoot.SetActive(false);

        if (equippedMark != null)
            equippedMark.SetActive(false);
    }
}
