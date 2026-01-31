using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VampireFOVRenderer : MonoBehaviour
{
    [Header("Settings")]
    public VampireVisionSensor visionSensor;
    public VampireFSM vampireFSM; // will auto-find in parent if null
    public int rayCount = 50;

    [Header("Colors")]
    public Color patrolColor = new Color(0, 1, 0, 0.2f);        // Green
    public Color chaseColor  = new Color(1, 0, 0, 0.4f);        // Red
    public Color searchColor = new Color(1, 0.92f, 0.016f, 0.3f); // Yellow

    [Header("Debug")]
    public bool logWarnings = true;
    public bool drawDebugRays = false;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propBlock;

    private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProp     = Shader.PropertyToID("_Color");

    private void Awake()
    {
        _mesh = new Mesh { name = "FOV_Mesh" };
        _mesh.MarkDynamic();

        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter.mesh = _mesh;

        _propBlock = new MaterialPropertyBlock();

        // Auto-wire refs (common prefab setup: renderer on child, sensor/FSM on parent)
        if (visionSensor == null) visionSensor = GetComponentInParent<VampireVisionSensor>();
        if (vampireFSM == null) vampireFSM = GetComponentInParent<VampireFSM>();

        if (logWarnings)
        {
            if (visionSensor == null)
                Debug.LogWarning($"[{gameObject.name}] VampireFOVRenderer: visionSensor is NULL. Assign it or place renderer under the enemy root.", this);

            if (_meshRenderer.sharedMaterial == null)
                Debug.LogWarning($"[{gameObject.name}] VampireFOVRenderer: MeshRenderer has NO material. Assign a transparent material (URP 2D/Sprite-Unlit-Default works well).", this);
        }
    }

    private void LateUpdate()
    {
        if (visionSensor == null) return;

        UpdateColorByState();
        DrawFOV();
    }

    private void UpdateColorByState()
    {
        // If FSM is missing, default to patrol color (but still render)
        Color targetColor = patrolColor;

        if (vampireFSM != null)
        {
            switch (vampireFSM.currentState)
            {
                case VampireFSM.State.Chase:
                case VampireFSM.State.ChaseHard:
                    targetColor = chaseColor;
                    break;
                case VampireFSM.State.Alert:
                case VampireFSM.State.Search:
                    targetColor = searchColor;
                    break;
                default:
                    targetColor = patrolColor;
                    break;
            }
        }
        else if (logWarnings)
        {
            // Only warn once-ish: avoid log spam
            logWarnings = false;
            Debug.LogWarning($"[{gameObject.name}] VampireFOVRenderer: vampireFSM is NULL, so state-based coloring won't change. Auto-find failed; assign it in inspector.", this);
        }

        ApplyColorToMaterial(targetColor);
    }

    private void ApplyColorToMaterial(Color c)
    {
        // Some shaders use _BaseColor (URP Lit/Unlit), Sprite shaders use _Color.
        // We'll set whichever exists. If neither exists, fallback to material.color if available.
        var mat = _meshRenderer.sharedMaterial;
        if (mat == null) return;

        _meshRenderer.GetPropertyBlock(_propBlock);

        bool didSet = false;

        if (mat.HasProperty(BaseColorProp))
        {
            _propBlock.SetColor(BaseColorProp, c);
            didSet = true;
        }

        if (mat.HasProperty(ColorProp))
        {
            _propBlock.SetColor(ColorProp, c);
            didSet = true;
        }

        _meshRenderer.SetPropertyBlock(_propBlock);

        if (!didSet)
        {
            // Fallback (will instantiate material at runtime)
            try
            {
                _meshRenderer.material.color = c;
            }
            catch
            {
                // ignore
            }
        }
    }

    private void DrawFOV()
    {
        float viewAngle = visionSensor.viewAngle;
        float viewDistance = visionSensor.viewDistance;
        LayerMask obstacleMask = visionSensor.obstacleMask;
        Transform fovOrigin = visionSensor.fovOrigin != null ? visionSensor.fovOrigin : visionSensor.transform;

        int rc = Mathf.Max(3, rayCount);
        float currentAngle = viewAngle / 2f;
        float angleStep = viewAngle / rc;

        Vector3[] vertices = new Vector3[rc + 2];
        int[] triangles = new int[rc * 3];

        // Center vertex MUST be set; otherwise mesh might be culled / malformed if renderer isn't at (0,0,0)
        vertices[0] = transform.InverseTransformPoint(fovOrigin.position);

        Vector3 forwardDir = (visionSensor.fovForward != null ? visionSensor.fovForward : visionSensor.transform).up;

        for (int i = 0; i <= rc; i++)
        {
            Vector3 dir = Quaternion.Euler(0, 0, currentAngle) * forwardDir;

            RaycastHit2D hit = Physics2D.Raycast(fovOrigin.position, dir, viewDistance, obstacleMask);

            Vector3 worldEndPoint = (hit.collider == null)
                ? (Vector3)fovOrigin.position + (dir * viewDistance)
                : (Vector3)hit.point;

            vertices[i + 1] = transform.InverseTransformPoint(worldEndPoint);

            if (i < rc)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            if (drawDebugRays)
                Debug.DrawRay(fovOrigin.position, dir * viewDistance, hit.collider == null ? Color.green : Color.red, 0f, false);

            currentAngle -= angleStep;
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds(); // important for culling
    }
}
