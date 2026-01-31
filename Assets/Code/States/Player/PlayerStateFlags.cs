using UnityEngine;

/// <summary>
/// Lightweight "state bag" for the player (GDD: Disguised / Exposed / Pursued / Hidden)
/// Other systems (mask, hide spot, AI) should ONLY read/write through PlayerStateManager when possible.
/// </summary>
public class PlayerStateFlags : MonoBehaviour
{
    [Header("Abilities")]
    public bool CanMove = true;
    public bool CanInteract = true;

    [Header("GDD States")]
    [Tooltip("True when player is inside a hide spot (should be invisible to FOV).")]
    public bool IsHidden = false;

    [Tooltip("True when a mask is equipped AND not broken.")]
    public bool IsDisguised = false;

    [Tooltip("True when player has no active disguise (mask broken/unequipped).")]
    public bool IsExposed = true;

    [Tooltip("True when at least one vampire is actively chasing/alerted and player hasn't fully escaped yet.")]
    public bool IsPursued = false;

    [Header("Debug")]
    [SerializeField] private int vampiresSeeingCount;
    [SerializeField] private int vampiresChasingCount;

    /// <summary>
    /// "Mask works" rule from GDD:
    /// - If pursued, mask does NOT fool vampires that already identified you.
    /// - While hidden, FOV should not see you.
    /// </summary>
    public bool IsDisguiseEffective => IsDisguised && !IsPursued && !IsHidden;

    /// <summary>
    /// Vampires should start Alert/Chase when player is exposed OR pursued.
    /// </summary>
    public bool CanTriggerDetection => IsExposed || IsPursued;

    internal void SetSeeingCount(int count) => vampiresSeeingCount = count;
    internal void SetChasingCount(int count) => vampiresChasingCount = count;
}