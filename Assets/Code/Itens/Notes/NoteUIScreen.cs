using TMPro;
using UnityEngine;

public class NoteUIScreen : MonoBehaviour
{
    public static NoteUIScreen Instance { get; private set; }

    [Header("UI Refs")] 
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;

    private float _prevTimeScale = 1f;

    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (root != null)
            root.SetActive(false);
    }

    public void Open(string title, string body)
    {
        if (root == null) return;

        titleText.text = title ?? "";
        bodyText.text  = body ?? "";

        root.SetActive(true);
        
        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void Close()
    {
        if (root == null) return;

        root.SetActive(false);
        Time.timeScale = _prevTimeScale <= 0f ? 1f : _prevTimeScale;
    }
}