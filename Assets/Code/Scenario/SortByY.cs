using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class SortByY : MonoBehaviour
{
    [Header("Tuning")]
    public int orderOffset = 0;
    public float unitsToOrder = 100f; 
    SortingGroup _group;
    Renderer _renderer;

    void Awake()
    {
        _group = GetComponent<SortingGroup>();
        _renderer = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        int order = orderOffset + Mathf.RoundToInt(-transform.position.y * unitsToOrder);

        if (_group != null) _group.sortingOrder = order;
        else if (_renderer != null) _renderer.sortingOrder = order;
    }
}