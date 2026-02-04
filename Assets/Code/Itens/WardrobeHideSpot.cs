using UnityEngine;
using Code.States;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class WardrobeHideSpot : MonoBehaviour, IInteractable
{
    [Header("Interactable")]
    [SerializeField] private string displayName = "Armário";
    [SerializeField] private string promptEnter = "Pressione [E] para se esconder";
    [SerializeField] private string promptExit  = "Pressione [E] para sair"; 
    
    [Header("Refs")]
    private PlayerInteractor _occupantInteractor;


    [Header("Snap Points (opcional)")]
    [SerializeField] private Transform hidePoint;
    [SerializeField] private Transform exitPoint;

    [Header("State")]
    [SerializeField] private bool grantDisguiseWhileHidden = true;
    [SerializeField] private bool restorePreviousDisguiseOnExit = true;

    [Header("Visual / Collision (opcional)")]
    [SerializeField] private bool hidePlayerRenderers = true;
    [SerializeField] private bool disablePlayerWorldCollider = true;

    private Collider2D _trigger; // <= collider do armário (IsTrigger)

    public Transform Transform => transform;
    public string DisplayName => displayName;
    public string Prompt => (_occupant != null) ? promptExit : promptEnter;

    private GameObject _occupant;
    private PlayerStateManager _occupantState;

    private bool _prevHidden;
    private bool _prevDisguised;
    private Vector3 _prevPosition;
    private Quaternion _prevRotation;

    private Collider2D _occupantWorldCollider;
    private Renderer[] _occupantRenderers;

    private void Awake()
    {
        _trigger = GetComponent<Collider2D>();
        _trigger.isTrigger = true;

        if (hidePoint == null)
        {
            var t = transform.Find("HidePoint");
            if (t != null) hidePoint = t;
        }

        if (exitPoint == null)
        {
            var t = transform.Find("ExitPoint");
            if (t != null) exitPoint = t;
        }
    }

    public void SetInRange(bool inRange) { }
    public void SetHighlighted(bool highlighted) { }

    public void Interact(GameObject interactor)
    {
        if (interactor == null) return;

        if (_occupant == null)
        {
            Enter(interactor);
            return;
        }

        if (_occupant == interactor)
        {
            Exit();
            return;
        }
    }

    private void Enter(GameObject interactor)
    {
        var psm = interactor.GetComponent<PlayerStateManager>() ??
                  interactor.GetComponentInParent<PlayerStateManager>();

        if (psm == null)
        {
            Debug.LogWarning($"{name}: Interactor não tem PlayerStateManager.");
            return;
        }

        _occupant = interactor;
        _occupantInteractor = interactor.GetComponent<PlayerInteractor>();
        if (_occupantInteractor != null)
            _occupantInteractor.ForceInteractable(this);

        _occupantState = psm;

        // salva estado anterior
        var flags = _occupantState.flags;
        _prevHidden    = (flags != null) && flags.IsHidden;
        _prevDisguised = (flags != null) && flags.IsDisguised;

        _prevPosition = interactor.transform.position;
        _prevRotation = interactor.transform.rotation;

        // ✅ SNAP SEGURO: só teleporta se o HidePoint estiver DENTRO do trigger do armário
        bool canSnap =
            hidePoint != null &&
            _trigger != null &&
            _trigger.OverlapPoint(hidePoint.position);

        if (hidePoint != null && !canSnap)
        {
            Debug.LogWarning(
                $"{name}: HidePoint está fora do trigger do armário. " +
                $"Não vou teleportar o player (senão ele perde o _inRange e não consegue sair).",
                this
            );
        }

        if (canSnap)
        {
            interactor.transform.position = hidePoint.position;
            interactor.transform.rotation = hidePoint.rotation;
        }

        // aplica estados
        _occupantState.SetHidden(true);
        if (grantDisguiseWhileHidden)
            _occupantState.SetDisguised(true);

        // desliga APENAS o WorldCollider (não mexe no InteractionCollider)
        if (disablePlayerWorldCollider)
        {
            var entity = interactor.GetComponent<EntityBehavior>() ??
                         interactor.GetComponentInParent<EntityBehavior>();

            if (entity != null && entity.entityWorldCollider != null)
            {
                _occupantWorldCollider = entity.entityWorldCollider;
                _occupantWorldCollider.enabled = false;
            }
        }

        // some com o player (opcional)
        if (hidePlayerRenderers)
        {
            _occupantRenderers = interactor.GetComponentsInChildren<Renderer>(true);
            foreach (var r in _occupantRenderers)
                if (r != null) r.enabled = false;
        }
    }

    private void Exit()
    {
        if (_occupantInteractor != null)
            _occupantInteractor.ForceInteractable(null);

        _occupantInteractor = null;
        
        if (_occupant == null || _occupantState == null) return;

        // volta posição (exitPoint > posição anterior)
        if (exitPoint != null)
        {
            _occupant.transform.position = exitPoint.position;
            _occupant.transform.rotation = exitPoint.rotation;
        }
        else
        {
            _occupant.transform.position = _prevPosition;
            _occupant.transform.rotation = _prevRotation;
        }

        // restaura estados
        _occupantState.SetHidden(_prevHidden);

        if (restorePreviousDisguiseOnExit)
            _occupantState.SetDisguised(_prevDisguised);
        else if (grantDisguiseWhileHidden)
            _occupantState.SetDisguised(false);

        if (_occupantWorldCollider != null)
            _occupantWorldCollider.enabled = true;
        _occupantWorldCollider = null;

        if (_occupantRenderers != null)
        {
            foreach (var r in _occupantRenderers)
                if (r != null) r.enabled = true;
        }
        _occupantRenderers = null;

        _occupant = null;
        _occupantState = null;
    }
}
