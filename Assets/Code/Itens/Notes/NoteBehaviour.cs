using UnityEngine;

public class NoteBehaviour : ItemBehavior
{
    [Header("Note Content")]
    [SerializeField] private string noteTitle = "Note";
    [TextArea(6, 20)]
    [SerializeField] private string noteBody = "Write text here...";

    [Header("Note Settings")]
    [SerializeField] private bool destroyAfterRead = false;

    // Override what PlayerInteractor displays
    public override string DisplayName => noteTitle;
    public override string Prompt => "Press [E] to read";

    public override void Interact(GameObject interactor)
    {
        if (NoteUIScreen.Instance == null)
        {
            Debug.LogWarning("No NoteUIScreen in scene.");
            return;
        }
        NoteUIScreen.Instance.Open(noteTitle, noteBody);
 
        if (destroyAfterRead)
            Destroy(gameObject);
    }
}