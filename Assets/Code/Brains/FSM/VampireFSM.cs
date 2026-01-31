using System.Collections;
using UnityEngine;
using Code.States;

[DisallowMultipleComponent]
[RequireComponent(typeof(MovementBehavior))]
[RequireComponent(typeof(VampireVisionSensor))]
public class VampireFSM : MonoBehaviour
{
    public enum State { Patrol, Alert, Chase, Search, ChaseHard }

    [Header("Refs")]
    public MovementBehavior movement;
    public EntityBehavior entity;
    public VampireVisionSensor vision;
    public PlayerStateManager player;
    public ObstacleAvoidance obstacleAvoidance;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float arriveDistance = 0.25f;

    [Header("Timing")]
    public float alertDuration = 0.35f;
    public float searchDuration = 4f;
    public float loseSightGrace = 0.1f;

    [Header("Speed")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public float searchSpeed = 3.0f;

    [Header("LKP Vulto (optional)")]
    public GameObject lkpMarkerPrefab;
    public float lkpMarkerLifetime = 3f;

    [Header("Audio (optional)")]
    public AudioSource audioSource;
    public AudioClip alertClip;

    [Header("Debug")]
    public State currentState = State.Patrol;

    private int _patrolIndex = 0;
    private Vector2 _lastKnownPos;
    private float _lostSightTime = -999f;
    private Coroutine _alertRoutine;
    private Coroutine _searchRoutine;

    private bool _wasSeeingPlayer = false;
    private bool _isRegisteredAsChaser = false;

    private int VampireId => GetInstanceID();

    private void Awake()
    {
        if (entity == null) entity = GetComponent<EntityBehavior>();
        if (movement == null) movement = GetComponent<MovementBehavior>();
        if (vision == null) vision = GetComponent<VampireVisionSensor>();
        if (obstacleAvoidance == null) obstacleAvoidance = GetComponent<ObstacleAvoidance>();

        if (player == null) player = vision.playerStateManager;
        if (player == null) 
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.GetComponent<PlayerStateManager>();
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable() => EnterState(State.Patrol);

    private void OnDisable()
    {
        if (player != null)
        {
            player.UnregisterVampireSeeing(VampireId);
            player.UnregisterChaser(VampireId);
        }
        _wasSeeingPlayer = false;
        _isRegisteredAsChaser = false;
    }

    private void Update()
    {
        if (player == null) return;

        vision.ManualTick();
        UpdateSeeingRegistry(vision.IsSeeingPlayer);

        switch (currentState)
        {
            case State.Patrol:
                TickPatrol();
                if (ShouldTriggerAlert()) EnterState(State.Alert);
                break;

            case State.Alert:
                break;

            case State.Chase:
                TickChase(hard: false);
                break;

            case State.Search:
                TickSearchMoveToLkp();
                if (ShouldTriggerChaseFromSearch()) EnterState(State.Chase);
                break;

            case State.ChaseHard:
                TickChase(hard: true);
                break;
        }
    }

    public void SetHardChase(bool enabled) => EnterState(enabled ? State.ChaseHard : State.Patrol);

    private void EnterState(State next)
    {
        if (currentState == next) return;

        ExitState(currentState);
        currentState = next;

        switch (currentState)
        {
            case State.Patrol:
                entity.maxVelocity = patrolSpeed;
                movement.SetMoveIntent(Vector2.zero);
                break;

            case State.Alert:
                movement.SetMoveIntent(Vector2.zero);
                PlayAlertSfx();
                if (_alertRoutine != null) StopCoroutine(_alertRoutine);
                _alertRoutine = StartCoroutine(AlertRoutine());
                break;

            case State.Chase:
                entity.maxVelocity = chaseSpeed;
                RegisterAsChaser(true);
                break;

            case State.Search:
                entity.maxVelocity = searchSpeed;
                RegisterAsChaser(false);
                StartSearch();
                break;

            case State.ChaseHard:
                entity.maxVelocity = chaseSpeed;
                RegisterAsChaser(true);
                break;
        }
    }

    private void ExitState(State state)
    {
        switch (state)
        {
            case State.Alert:
                if (_alertRoutine != null) StopCoroutine(_alertRoutine);
                _alertRoutine = null;
                break;

            case State.Search:
                if (_searchRoutine != null) StopCoroutine(_searchRoutine);
                _searchRoutine = null;
                break;

            case State.Chase:
            case State.ChaseHard:
                RegisterAsChaser(false);
                break;
        }
    }

    private void TickPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            movement.SetMoveIntent(Vector2.zero);
            return;
        }

        Transform target = patrolPoints[_patrolIndex];
        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        
        if (to.magnitude <= arriveDistance)
        {
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            target = patrolPoints[_patrolIndex];
            to = (Vector2)target.position - (Vector2)transform.position;
        }

        Vector2 moveDirection = to.normalized;
        
        // Apply obstacle avoidance if available
        if (obstacleAvoidance != null)
        {
            moveDirection = obstacleAvoidance.GetAvoidanceAdjustedDirection(moveDirection);
        }

        // Set rotation
        if (moveDirection.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            entity.SetTargetRotation(angle);
        }

        movement.SetMoveIntent(moveDirection);
    }

    private bool ShouldTriggerAlert()
    {
        if (!vision.IsSeeingPlayer) return false;
        var pf = player.flags;
        if (pf == null) return false;
        return pf.CanTriggerDetection;
    }

    private IEnumerator AlertRoutine()
    {
        float t = 0f;
        while (t < alertDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (vision.IsSeeingPlayer && player.flags != null && player.flags.CanTriggerDetection)
            EnterState(State.Chase);
        else
            EnterState(State.Patrol);
    }

    private void PlayAlertSfx()
    {
        if (audioSource == null || alertClip == null) return;
        audioSource.PlayOneShot(alertClip);
    }

    private void TickChase(bool hard)
    {
        Vector2 to = vision.LastSeenPlayerPosition - (Vector2)transform.position;
        Vector2 moveDirection = to.normalized;
        
        if (obstacleAvoidance != null)
        {
            moveDirection = obstacleAvoidance.GetAvoidanceAdjustedDirection(moveDirection);
        }

        movement.SetMoveIntent(moveDirection);
        
        if (to.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - 90f;
            entity.SetTargetRotation(angle);
        }

        if (hard) return;

        if (vision.IsSeeingPlayer)
        {
            _lastKnownPos = vision.LastSeenPlayerPosition;
            _lostSightTime = -999f;
            return;
        }

        if (_lostSightTime < 0f) _lostSightTime = Time.time;

        if (Time.time - _lostSightTime >= loseSightGrace)
        {
            _lastKnownPos = _lastKnownPos == Vector2.zero ? (Vector2)player.transform.position : _lastKnownPos;
            EnterState(State.Search);
        }
    }

    private void RegisterAsChaser(bool chasing)
    {
        if (player == null) return;

        if (chasing && !_isRegisteredAsChaser)
        {
            player.RegisterChaser(VampireId);
            _isRegisteredAsChaser = true;
        }
        else if (!chasing && _isRegisteredAsChaser)
        {
            player.UnregisterChaser(VampireId);
            _isRegisteredAsChaser = false;
        }
    }

    private void StartSearch()
    {
        _lastKnownPos = vision.LastSeenPlayerPosition;
        SpawnLkpMarker(_lastKnownPos);

        if (_searchRoutine != null) StopCoroutine(_searchRoutine);
        _searchRoutine = StartCoroutine(SearchRoutine());
    }

    private void TickSearchMoveToLkp()
    {
        Vector2 to = _lastKnownPos - (Vector2)transform.position;
        if (to.magnitude <= arriveDistance)
        {
            movement.SetMoveIntent(Vector2.zero);
            return;
        }

        Vector2 moveDirection = to.normalized;
        
        if (obstacleAvoidance != null)
        {
            moveDirection = obstacleAvoidance.GetAvoidanceAdjustedDirection(moveDirection);
        }

        movement.SetMoveIntent(moveDirection);
    }

    private IEnumerator SearchRoutine()
    {
        float t = 0f;
        // look around at LKP for searchDuration
        Quaternion initialRotation = transform.rotation;
        bool toRight = true;
        float lookInterval = 0.5f;
        float lookTimer = 0f;
        while (t < searchDuration)
        {
            lookTimer += Time.deltaTime;
            if (lookTimer >= lookInterval)
            {
                lookTimer = 0f;
                float angleOffset = toRight ? 45f : -45f;
                entity.SetTargetRotation(initialRotation.eulerAngles.z + angleOffset);
                toRight = !toRight;
            }
            t += Time.deltaTime;
            yield return null;
        }

        EnterState(State.Patrol);
    }

    private bool ShouldTriggerChaseFromSearch()
    {
        if (!vision.IsSeeingPlayer) return false;
        if (player.flags == null) return false;
        return player.flags.CanTriggerDetection;
    }

    private void SpawnLkpMarker(Vector2 pos)
    {
        if (lkpMarkerPrefab == null) return;

        var go = Instantiate(lkpMarkerPrefab, pos, Quaternion.identity);
        Destroy(go, Mathf.Max(0.1f, lkpMarkerLifetime));
    }

    private void UpdateSeeingRegistry(bool isSeeingNow)
    {
        if (player == null) return;

        if (isSeeingNow && !_wasSeeingPlayer)
        {
            player.RegisterVampireSeeing(VampireId);
            _wasSeeingPlayer = true;
        }
        else if (!isSeeingNow && _wasSeeingPlayer)
        {
            player.UnregisterVampireSeeing(VampireId);
            _wasSeeingPlayer = false;
        }
    }
}