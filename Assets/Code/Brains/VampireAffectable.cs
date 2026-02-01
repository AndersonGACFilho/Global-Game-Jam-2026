using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class VampireAffectable : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private VampireFSM fsm;
    [SerializeField] private MovementBehavior movement;
    [SerializeField] private EntityBehavior entity;

    private Coroutine _routine;

    private void Awake()
    {
        if (fsm == null) fsm = GetComponent<VampireFSM>();
        if (movement == null) movement = GetComponent<MovementBehavior>();
        if (entity == null) entity = GetComponent<EntityBehavior>();
    }

    public void ApplyStun(float seconds)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(CoStun(seconds));
    }

    public void ApplyRepel(Vector2 from, float seconds, float speedMultiplier = 1.25f)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(CoRepel(from, seconds, speedMultiplier));
    }

    private IEnumerator CoStun(float seconds)
    {
        bool prevFsm = fsm != null && fsm.enabled;
        if (fsm != null) fsm.enabled = false;

        if (movement != null) movement.Stop();

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            if (movement != null) movement.Stop();
            yield return null;
        }

        if (fsm != null) fsm.enabled = prevFsm;
        _routine = null;
    }

    private IEnumerator CoRepel(Vector2 from, float seconds, float speedMultiplier)
    {
        bool prevFsm = fsm != null && fsm.enabled;
        if (fsm != null) fsm.enabled = false;

        float prevMaxV = (entity != null) ? entity.maxVelocity : 0f;
        if (entity != null) entity.maxVelocity = prevMaxV * speedMultiplier;

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;

            if (movement != null)
            {
                Vector2 dir = ((Vector2)transform.position - from);
                if (dir.sqrMagnitude < 0.0001f) dir = Random.insideUnitCircle;
                movement.SetMoveIntent(dir.normalized);
            }

            yield return null;
        }

        if (entity != null) entity.maxVelocity = prevMaxV;
        if (movement != null) movement.SetMoveIntent(Vector2.zero);

        if (fsm != null) fsm.enabled = prevFsm;
        _routine = null;
    }
}
