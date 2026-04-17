using UnityEngine;

public class CarMeshDent : MonoBehaviour
{
    [Header("Target mesh path")]
    public Transform targetMeshTransform;   // Drag Car1201/Mesh here if you want
    public string fallbackChildName = "Mesh";

    [Header("Dent settings")]
    public float minImpactForce = 3f;
    public float dentRadius = 0.4f;
    public float dentStrength = 0.08f;
    public float maxDentDistance = 0.35f;

    [Header("Optional")]
    public bool updateMeshCollider = false;

    private MeshFilter targetMeshFilter;
    private MeshCollider targetMeshCollider;
    private Mesh deformedMesh;
    private Vector3[] originalVerts;
    private Vector3[] deformedVerts;

    void Start()
    {
        if (targetMeshTransform == null)
        {
            targetMeshTransform = FindDeepChild(transform, fallbackChildName);
        }

        if (targetMeshTransform == null)
        {
            Debug.LogError("CarMeshDentRoot: Could not find target mesh transform.");
            enabled = false;
            return;
        }

        targetMeshFilter = targetMeshTransform.GetComponent<MeshFilter>();
        if (targetMeshFilter == null)
        {
            Debug.LogError("CarMeshDentRoot: Target mesh has no MeshFilter.");
            enabled = false;
            return;
        }

        targetMeshCollider = targetMeshTransform.GetComponent<MeshCollider>();

        deformedMesh = Instantiate(targetMeshFilter.mesh);
        targetMeshFilter.mesh = deformedMesh;

        originalVerts = deformedMesh.vertices;
        deformedVerts = (Vector3[])deformedMesh.vertices.Clone();

        if (updateMeshCollider && targetMeshCollider != null)
        {
            targetMeshCollider.sharedMesh = deformedMesh;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (impact < minImpactForce) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            DentAt(contact.point, contact.normal, impact);
        }

        ApplyMesh();
    }

    void DentAt(Vector3 worldPoint, Vector3 worldNormal, float impact)
    {
        // Convert collision info from world space into target mesh local space
        Vector3 localPoint = targetMeshTransform.InverseTransformPoint(worldPoint);
        Vector3 localNormal = targetMeshTransform.InverseTransformDirection(worldNormal).normalized;

        float dentAmount = dentStrength * impact;

        for (int i = 0; i < deformedVerts.Length; i++)
        {
            float dist = Vector3.Distance(deformedVerts[i], localPoint);
            if (dist > dentRadius) continue;

            float falloff = 1f - (dist / dentRadius);

            // Push vertices inward relative to the contact normal
            Vector3 offset = -localNormal * dentAmount * falloff;

            Vector3 candidate = deformedVerts[i] + offset;

            // Clamp total deformation from original shape
            Vector3 displacement = candidate - originalVerts[i];
            if (displacement.magnitude > maxDentDistance)
            {
                candidate = originalVerts[i] + displacement.normalized * maxDentDistance;
            }

            deformedVerts[i] = candidate;
        }
    }

    void ApplyMesh()
    {
        deformedMesh.vertices = deformedVerts;
        deformedMesh.RecalculateNormals();
        deformedMesh.RecalculateBounds();

        if (updateMeshCollider && targetMeshCollider != null)
        {
            // Force collider refresh
            targetMeshCollider.sharedMesh = null;
            targetMeshCollider.sharedMesh = deformedMesh;
        }
    }

    Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
}
