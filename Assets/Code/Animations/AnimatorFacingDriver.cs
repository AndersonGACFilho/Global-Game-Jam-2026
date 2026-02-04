using UnityEngine;

public class AnimatorFacingDriver : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float deadZone = 0.01f;

    static readonly int FacingParam = Animator.StringToHash("Facing");

    void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void ApplyFacing(Vector2 move)
    {
        if (move.sqrMagnitude < deadZone * deadZone) return;

        move.Normalize();

        if (Mathf.Abs(move.y) > Mathf.Abs(move.x))
        {
            anim.SetInteger(FacingParam, move.y > 0 ? 1 : 0);
            sr.flipX = false;
        }
        else
        {
            anim.SetInteger(FacingParam, 2);
            sr.flipX = (move.x < 0);
        }
    }
}