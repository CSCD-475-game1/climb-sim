using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineRoad : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float width = 4f;
    public int resolution = 100;
    public float terrainOffset = 0.05f;
    public LayerMask groundMask = ~0;

    private MeshFilter meshFilter;

    void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        Generate();
    }

    void OnValidate()
    {
        meshFilter = GetComponent<MeshFilter>();
        Generate();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Generate();
        }
#endif
    }

    public void Generate()
    {
        if (splineContainer == null || splineContainer.Spline == null || resolution < 2)
        {
            return;
        }

        var spline = splineContainer.Spline;
        var splineTransform = splineContainer.transform;

        Vector3[] vertices = new Vector3[resolution * 2];
        Vector2[] uvs = new Vector2[resolution * 2];
        int[] triangles = new int[(resolution - 1) * 6];

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);

            // Spline evaluates in spline-container local space, so convert to world.
            Vector3 localPos = (Vector3)spline.EvaluatePosition(t);
            Vector3 worldPos = splineTransform.TransformPoint(localPos);

            Vector3 localTangent = (Vector3)spline.EvaluateTangent(t);
            Vector3 worldForward = splineTransform.TransformDirection(localTangent).normalized;

            if (worldForward.sqrMagnitude < 0.0001f)
            {
                worldForward = Vector3.forward;
            }

            // Snap centerline down to terrain/collider.
            Ray ray = new Ray(worldPos + Vector3.up * 1000f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, groundMask))
            {
                worldPos.y = hit.point.y + terrainOffset;
            }

            Vector3 right = Vector3.Cross(Vector3.up, worldForward).normalized;

            Vector3 leftWorld = worldPos - right * (width * 0.5f);
            Vector3 rightWorld = worldPos + right * (width * 0.5f);

            // Store mesh vertices in THIS object's local space.
            vertices[i * 2] = transform.InverseTransformPoint(leftWorld);
            vertices[i * 2 + 1] = transform.InverseTransformPoint(rightWorld);

            float v = i / (float)(resolution - 1);
            uvs[i * 2] = new Vector2(0, v * 10f);
            uvs[i * 2 + 1] = new Vector2(1, v * 10f);

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
        mesh.name = "SplineRoadMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            meshFilter.mesh = mesh;
        }
#else
        meshFilter.mesh = mesh;
#endif
    }
}
