using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MovementBehavior))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerBehavior : EntityBehavior
{
    [Header("Fail State")]
    [SerializeField] private float restartDelay = 0.5f;

    private bool _isDead;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleCaught(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleCaught(collision.collider);
    }

    private void TryHandleCaught(Collider2D other)
    {
        if (_isDead) return;
        if (other == null) return;

        if (other.GetComponentInParent<VampireFSM>() == null) return;

        _isDead = true;
        if (GameWinController.Instance != null)
        {
            GameWinController.Instance.CancelInvoke();
        }
        Invoke(nameof(ReloadScene), restartDelay);
    }

    private void ReloadScene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
    }
}
