using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.States
{
    /// <summary>
    /// Source of truth for player state transitions (GDD).
    ///
    /// - Maintains "vampiresSeeingPlayer" HashSet for mask drain multiplier.
    /// - Maintains "vampiresChasingPlayer" HashSet for Pursued state.
    /// - Implements escape rule: after 3s with NO vampire in Chase AND not being seen -> clears Pursued.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerStateFlags))]
    public class PlayerStateManager : MonoBehaviour
    {
        [Header("Refs")]
        public PlayerStateFlags flags;

        [Header("Pursuit Rule (GDD)")]
        [Tooltip("Seconds with NO vampire chasing and NOT being seen to clear Pursued.")]
        public float clearPursuedAfterSeconds = 3f;

        private readonly HashSet<int> _vampiresSeeing = new HashSet<int>();
        private readonly HashSet<int> _vampiresChasing = new HashSet<int>();

        private float _lastSeenTime = -999f;

        public int VampiresSeeingCount => _vampiresSeeing.Count;
        public int VampiresChasingCount => _vampiresChasing.Count;

        public event Action<int> OnSeeingCountChanged;
        public event Action<int> OnChasingCountChanged;
        public event Action OnFlagsChanged;

        private void Awake()
        {
            flags = GetComponent<PlayerStateFlags>();

            // Keep initial state consistent
            if (flags.IsDisguised && flags.IsExposed) flags.IsExposed = false;
            if (!flags.IsDisguised && !flags.IsExposed) flags.IsExposed = true;

            SyncDebugCounts();
        }

        private void Update()
        {
            if (_vampiresSeeing.Count > 0)
                _lastSeenTime = Time.time;

            TickClearPursued();
        }

        public void SetHidden(bool hidden)
        {
            if (flags.IsHidden == hidden) return;

            flags.IsHidden = hidden;

            // Typical: can't move while hidden, but can still interact to exit
            flags.CanMove = !hidden;

            OnFlagsChanged?.Invoke();
        }

        public void SetDisguised(bool disguised)
        {
            if (flags.IsDisguised == disguised) return;

            flags.IsDisguised = disguised;
            flags.IsExposed = !disguised;

            OnFlagsChanged?.Invoke();
        }

        public void ForceExposed()
        {
            if (flags.IsExposed && !flags.IsDisguised) return;

            flags.IsDisguised = false;
            flags.IsExposed = true;

            OnFlagsChanged?.Invoke();
        }

        public void RegisterVampireSeeing(int vampireId)
        {
            if (_vampiresSeeing.Add(vampireId))
            {
                _lastSeenTime = Time.time;
                SyncDebugCounts();
                OnSeeingCountChanged?.Invoke(_vampiresSeeing.Count);
            }
        }

        public void UnregisterVampireSeeing(int vampireId)
        {
            if (_vampiresSeeing.Remove(vampireId))
            {
                SyncDebugCounts();
                OnSeeingCountChanged?.Invoke(_vampiresSeeing.Count);
            }
        }

        public void RegisterChaser(int vampireId)
        {
            if (_vampiresChasing.Add(vampireId))
            {
                flags.IsPursued = true;
                SyncDebugCounts();
                OnChasingCountChanged?.Invoke(_vampiresChasing.Count);
                OnFlagsChanged?.Invoke();
            }
        }

        public void UnregisterChaser(int vampireId)
        {
            if (_vampiresChasing.Remove(vampireId))
            {
                SyncDebugCounts();
                OnChasingCountChanged?.Invoke(_vampiresChasing.Count);
                // Pursued clears only by rule (3s without chase AND not seen)
            }
        }

        private void TickClearPursued()
        {
            if (!flags.IsPursued) return;

            if (_vampiresChasing.Count > 0) return;
            if (_vampiresSeeing.Count > 0) return;

            if (Time.time - _lastSeenTime >= clearPursuedAfterSeconds)
            {
                flags.IsPursued = false;
                OnFlagsChanged?.Invoke();
            }
        }

        private void SyncDebugCounts()
        {
            flags.SetSeeingCount(_vampiresSeeing.Count);
            flags.SetChasingCount(_vampiresChasing.Count);
        }

        private void OnDisable()
        {
            _vampiresSeeing.Clear();
            _vampiresChasing.Clear();
            SyncDebugCounts();
        }
    }
}
