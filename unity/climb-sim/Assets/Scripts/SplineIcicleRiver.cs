using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineIcicleRiver : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float width = 8f;
    public int resolution = 100;
    public float surfaceOffset = 0.02f;
    public LayerMask groundMask = ~0;
    public bool autoUpdate = true;
    public float uvTiling = 6f;

    private MeshFilter meshFilter;

    void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (autoUpdate) Generate();
    }

    void OnValidate()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (autoUpdate) Generate();
    }
    public AnimationCurve widthProfile = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(1f, 1f)
    );

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying && autoUpdate)
        {
            Generate();
        }
    }
#endif

    [ContextMenu("Generate River")]
    public void Generate()
    {
        if (splineContainer == null || splineContainer.Spline == null || resolution < 2)
            return;

        var spline = splineContainer.Spline;
        var splineTransform = splineContainer.transform;

        Vector3[] vertices = new Vector3[resolution * 2];
        Vector2[] uvs = new Vector2[resolution * 2];
        int[] triangles = new int[(resolution - 1) * 6];

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);

            // Spline values are local to the spline object
            Vector3 localPos = (Vector3)spline.EvaluatePosition(t);
            Vector3 worldPos = splineTransform.TransformPoint(localPos);

            Vector3 localTangent = (Vector3)spline.EvaluateTangent(t);
            Vector3 worldForward = splineTransform.TransformDirection(localTangent).normalized;
            if (worldForward.sqrMagnitude < 0.0001f)
                worldForward = Vector3.forward;

            // Snap centerline to terrain / colliders
            Ray ray = new Ray(worldPos + Vector3.up * 1000f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, groundMask))
            {
                worldPos.y = hit.point.y + surfaceOffset;
            }

            Vector3 right = Vector3.Cross(Vector3.up, worldForward).normalized;

            float localWidth = width * widthProfile.Evaluate(t);
            Vector3 leftWorld = worldPos - right * (localWidth * 0.5f);
            Vector3 rightWorld = worldPos + right * (localWidth * 0.5f);

            // Optional extra edge-snapping so both banks hug terrain better
            Ray leftRay = new Ray(leftWorld + Vector3.up * 1000f, Vector3.down);
            if (Physics.Raycast(leftRay, out RaycastHit leftHit, 5000f, groundMask))
                leftWorld.y = leftHit.point.y + surfaceOffset;

            Ray rightRay = new Ray(rightWorld + Vector3.up * 1000f, Vector3.down);
            if (Physics.Raycast(rightRay, out RaycastHit rightHit, 5000f, groundMask))
                rightWorld.y = rightHit.point.y + surfaceOffset;

            vertices[i * 2] = transform.InverseTransformPoint(leftWorld);
            vertices[i * 2 + 1] = transform.InverseTransformPoint(rightWorld);

            float v = i / (float)(resolution - 1) * uvTiling;
            uvs[i * 2] = new Vector2(0f, v);
            uvs[i * 2 + 1] = new Vector2(1f, v);

            if (i < resolution - 1)
            {
                int tri = i * 6;
                int vi = i * 2;

                triangles[tri] = vi;
                triangles[tri + 1] = vi + 2;
                triangles[tri + 2] = vi + 1;

                triangles[tri + 3] = vi + 1;
                triangles[tri + 4] = vi + 2;
                triangles[tri + 5] = vi + 3;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "SplineRiverMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

#if UNITY_EDITOR
        if (!Application.isPlaying)
            meshFilter.sharedMesh = mesh;
        else
            meshFilter.mesh = mesh;
#else
        meshFilter.mesh = mesh;
#endif
    }
}
