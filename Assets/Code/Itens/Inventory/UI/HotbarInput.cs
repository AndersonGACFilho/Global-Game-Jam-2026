using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarInput : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private bool disableWhileNoteOpen = true;
    [SerializeField] private bool enableMouseWheel = true;

    private void Awake()
    {
        if (inventory == null) inventory = GetComponent<PlayerInventory>();
        if (inventory == null) inventory = PlayerInventory.Instance;
    }

    private void Update()
    {
        if (inventory == null) return;

        if (disableWhileNoteOpen && NoteUIScreen.Instance != null && NoteUIScreen.Instance.IsOpen)
            return;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.digit1Key.wasPressedThisFrame) inventory.Select(0);
            if (kb.digit2Key.wasPressedThisFrame) inventory.Select(1);
            if (kb.digit3Key.wasPressedThisFrame) inventory.Select(2);
            if (kb.digit4Key.wasPressedThisFrame) inventory.Select(3);
            if (kb.digit5Key.wasPressedThisFrame) inventory.Select(4);

            if (kb.qKey.wasPressedThisFrame) inventory.UseSelected();
            if (kb.rKey.wasPressedThisFrame) inventory.ToggleEquipSelected();
        }

        if (enableMouseWheel && Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                int dir = scroll > 0 ? -1 : 1;
                int next = (inventory.SelectedIndex + dir) % inventory.MaxSlots;
                if (next < 0) next += inventory.MaxSlots;
                inventory.Select(next);
            }
        }
        // Bumper buttons and face buttons
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.leftShoulder.wasPressedThisFrame)
            {
                int next = (inventory.SelectedIndex - 1) % inventory.MaxSlots;
                if (next < 0) next += inventory.MaxSlots;
                inventory.Select(next);
            }
            if (gamepad.rightShoulder.wasPressedThisFrame)
            {
                int next = (inventory.SelectedIndex + 1) % inventory.MaxSlots;
                if (next < 0) next += inventory.MaxSlots;
                inventory.Select(next);
            }
            if (gamepad.buttonSouth.wasPressedThisFrame) inventory.UseSelected();
            if (gamepad.buttonEast.wasPressedThisFrame) inventory.ToggleEquipSelected();
        }
    }
}