using System.Collections.Generic;
using UnityEngine;

public class TreesRocks : MonoBehaviour
{
    [Header("References")]
    public Terrain terrain;
    public Transform parentRoot;

    [Header("Area in local/world XZ")]
    public Vector2 areaMin = new Vector2(0, 0);
    public Vector2 areaMax = new Vector2(200, 200);

    [Header("Prefabs")]
    public GameObject[] treePrefabs;
    public GameObject[] rockPrefabs;

    [Header("Counts")]
    public int treeCount = 50;
    public int rockCount = 30;

    [Header("Terrain Constraints")]
    public float minSlope = 0f;
    public float maxSlope = 35f;
    public float minHeight = -9999f;
    public float maxHeight = 9999f;

    [Header("Trees")]
    public Vector2 treeScaleRange = new Vector2(0.8f, 1.3f);
    public float treeYOffset = 0f;
    public bool alignTreesToSlope = false;

    [Header("Rocks")]
    public Vector2 rockScaleRange = new Vector2(0.7f, 2.0f);
    public float rockYOffset = -0.15f;
    public bool alignRocksToSlope = true;

    [Header("Spacing")]
    public float minTreeSpacing = 4f;
    public float minRockSpacing = 2f;
    public float minTreeRockSpacing = 2f;

    [Header("Random")]
    public int seed = 12345;

    private readonly List<Vector3> placedTrees = new();
    private readonly List<Vector3> placedRocks = new();

    [ContextMenu("Scatter")]
    public void Scatter()
    {
        if (terrain == null)
        {
            Debug.LogError("AreaScatter: Terrain is missing.");
            return;
        }

        ClearChildren();

        if (parentRoot == null)
        {
            parentRoot = transform;
        }

        placedTrees.Clear();
        placedRocks.Clear();

        Random.InitState(seed);

        ScatterGroup(
            prefabs: treePrefabs,
            count: treeCount,
            scaleRange: treeScaleRange,
            yOffset: treeYOffset,
            alignToSlope: alignTreesToSlope,
            minSpacingSameType: minTreeSpacing,
            minSpacingOtherType: minTreeRockSpacing,
            outputPositions: placedTrees,
            otherPositions: placedRocks,
            label: "Tree"
        );

        ScatterGroup(
            prefabs: rockPrefabs,
            count: rockCount,
            scaleRange: rockScaleRange,
            yOffset: rockYOffset,
            alignToSlope: alignRocksToSlope,
            minSpacingSameType: minRockSpacing,
            minSpacingOtherType: minTreeRockSpacing,
            outputPositions: placedRocks,
            otherPositions: placedTrees,
            label: "Rock"
        );
    }

    [ContextMenu("Clear Children")]
    public void ClearChildren()
    {
        List<GameObject> toDelete = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            toDelete.Add(transform.GetChild(i).gameObject);
        }

        foreach (GameObject go in toDelete)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.Undo.DestroyObjectImmediate(go);
            else
                Destroy(go);
#else
            Destroy(go);
#endif
        }
    }

    private void ScatterGroup(
        GameObject[] prefabs,
        int count,
        Vector2 scaleRange,
        float yOffset,
        bool alignToSlope,
        float minSpacingSameType,
        float minSpacingOtherType,
        List<Vector3> outputPositions,
        List<Vector3> otherPositions,
        string label
    )
    {
        if (prefabs == null || prefabs.Length == 0 || count <= 0)
            return;

        int maxAttempts = count * 40;
        int placed = 0;
        int attempts = 0;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(areaMin.x, areaMax.x);
            float z = Random.Range(areaMin.y, areaMax.y);

            Vector3 world = new Vector3(x, 0f, z);

            float y = terrain.SampleHeight(world) + terrain.transform.position.y;
            world.y = y;

            if (!IsValidTerrainPoint(world))
                continue;

            if (!HasSpacing(world, outputPositions, minSpacingSameType))
                continue;

            if (!HasSpacing(world, otherPositions, minSpacingOtherType))
                continue;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject instance = Instantiate(prefab, parentRoot);

            instance.name = $"{label}_{placed:D3}";
            instance.transform.position = world + Vector3.up * yOffset;

            Quaternion yaw = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            if (alignToSlope)
            {
                Vector3 normal = GetTerrainNormal(world);
                Quaternion slopeRot = Quaternion.FromToRotation(Vector3.up, normal);
                instance.transform.rotation = slopeRot * yaw;
            }
            else
            {
                instance.transform.rotation = yaw;
            }

            float s = Random.Range(scaleRange.x, scaleRange.y);
            instance.transform.localScale *= s;

            outputPositions.Add(world);
            placed++;
        }

        Debug.Log($"{label}s placed: {placed}/{count}");
    }

    private bool IsValidTerrainPoint(Vector3 world)
    {
        Vector3 tPos = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;

        if (world.x < tPos.x || world.z < tPos.z || world.x > tPos.x + size.x || world.z > tPos.z + size.z)
            return false;

        if (world.y < minHeight || world.y > maxHeight)
            return false;

        Vector3 normal = GetTerrainNormal(world);
        float slope = Vector3.Angle(normal, Vector3.up);

        return slope >= minSlope && slope <= maxSlope;
    }

    private Vector3 GetTerrainNormal(Vector3 world)
    {
        Vector3 tPos = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;

        float nx = Mathf.InverseLerp(tPos.x, tPos.x + size.x, world.x);
        float nz = Mathf.InverseLerp(tPos.z, tPos.z + size.z, world.z);

        return terrain.terrainData.GetInterpolatedNormal(nx, nz);
    }

    private bool HasSpacing(Vector3 candidate, List<Vector3> existing, float minSpacing)
    {
        float minSq = minSpacing * minSpacing;
        foreach (Vector3 p in existing)
        {
            Vector2 a = new Vector2(candidate.x, candidate.z);
            Vector2 b = new Vector2(p.x, p.z);
            if ((a - b).sqrMagnitude < minSq)
                return false;
        }
        return true;
    }
}
