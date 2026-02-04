using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameWinController : MonoBehaviour
{
    public static GameWinController Instance { get; private set; }

    [Header("UI (opcional)")]
    [SerializeField] private GameObject winRoot;          // um Panel no Canvas
    [SerializeField] private TextMeshProUGUI winText;     // texto "Você venceu!"

    [Header("Flow")]
    [SerializeField] private bool pauseTimeScale = true;

    [Header("Load Win Scene (opcional)")]
    [SerializeField] private bool loadWinScene = false;
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private float loadDelayRealtime = 0.25f;

    private bool _ended;
    private float _prevTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (winRoot != null) winRoot.SetActive(false);
    }

    public void Win()
    {
        if (_ended) return;
        _ended = true;

        // trava player
        var flags = FindFirstObjectByType<PlayerStateFlags>();
        if (flags != null)
        {
            flags.CanMove = false;
            flags.CanInteract = false;
        }

        // mostra UI (opcional)
        if (winText != null) winText.text = "VOCÊ VENCEU!";
        if (winRoot != null) winRoot.SetActive(true);

        // pausa jogo
        if (pauseTimeScale)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        // opcional: carregar cena de vitória
        if (loadWinScene)
        {
            // em WebGL/timeScale=0, use delay em "realtime"
            Invoke(nameof(LoadWinScene), loadDelayRealtime);
        }
    }

    private void LoadWinScene()
    {
        // garante que timeScale não atrapalhe transição
        Time.timeScale = 1f;
        SceneManager.LoadScene(winSceneName);
    }
}