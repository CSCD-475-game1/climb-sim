using UnityEngine;

public class TerrainGrassManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private string playerTag = "Player";

    [Header("Detail Layer")]
    [SerializeField] private int detailLayerIndex = 0;
    [SerializeField] private int grassDensity = 8;

    [Header("Allowed Terrain Texture Layers")]
    [SerializeField] private int[] allowedLayerIndices = { 5, 6 };
    [SerializeField] private float minAllowedLayerWeight = 0.15f;

    [Header("Radius")]
    [SerializeField] private float paintRadiusWorld = 25f;
    [SerializeField] private float clearRadiusWorld = 35f;

    [Header("Update")]
    [SerializeField] private float updateInterval = 1.0f;
    [SerializeField] private float minPlayerMoveDistance = 4f;

    [Header("Placement")]
    [SerializeField] private float slopeLimit = 35f;
    [SerializeField] private float minHeight = -1000f;
    [SerializeField] private float maxHeight = 10000f;
    [SerializeField] private bool useNoise = true;
    [SerializeField] private float noiseScale = 0.08f;
    [SerializeField] private float noiseThreshold = 0.45f;

    private Transform playerRoot;
    private TerrainData terrainData;

    private int[,] detailCache;
    private Vector3 lastPlayerPos;
    private float timer;

    private int detailWidth;
    private int detailHeight;

    private float[,,] alphamaps;

    private void Start()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogError("TerrainGrassManager: No Terrain assigned or found.");
            enabled = false;
            return;
        }

        terrainData = terrain.terrainData;
        detailWidth = terrainData.detailWidth;
        detailHeight = terrainData.detailHeight;

        if (detailLayerIndex < 0 || detailLayerIndex >= terrainData.detailPrototypes.Length)
        {
            Debug.LogError($"TerrainGrassManager: detailLayerIndex {detailLayerIndex} is out of range.");
            enabled = false;
            return;
        }

        CacheAlphamaps();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            playerRoot = playerObj.transform;

        detailCache = terrainData.GetDetailLayer(0, 0, detailWidth, detailHeight, detailLayerIndex);

        if (playerRoot != null)
        {
            lastPlayerPos = playerRoot.position;
            UpdateGrassAroundPlayer(force: true);
        }
    }

    private void Update()
    {
        if (playerRoot == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                playerRoot = playerObj.transform;
                lastPlayerPos = playerRoot.position;
                UpdateGrassAroundPlayer(force: true);
            }
            return;
        }

        timer += Time.deltaTime;
        if (timer < updateInterval)
            return;

        timer = 0f;

        if (Vector3.Distance(playerRoot.position, lastPlayerPos) < minPlayerMoveDistance)
            return;

        UpdateGrassAroundPlayer(force: false);
        lastPlayerPos = playerRoot.position;
    }

    private void CacheAlphamaps()
    {
        alphamaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
    }

    private void UpdateGrassAroundPlayer(bool force)
    {
        Vector3 playerPos = playerRoot.position;

        WorldToDetail(playerPos, out int playerDX, out int playerDY);

        int paintRadiusX = Mathf.CeilToInt((paintRadiusWorld / terrainData.size.x) * detailWidth);
        int paintRadiusY = Mathf.CeilToInt((paintRadiusWorld / terrainData.size.z) * detailHeight);

        int clearRadiusX = Mathf.CeilToInt((clearRadiusWorld / terrainData.size.x) * detailWidth);
        int clearRadiusY = Mathf.CeilToInt((clearRadiusWorld / terrainData.size.z) * detailHeight);

        int minX = Mathf.Max(0, playerDX - clearRadiusX);
        int maxX = Mathf.Min(detailWidth - 1, playerDX + clearRadiusX);
        int minY = Mathf.Max(0, playerDY - clearRadiusY);
        int maxY = Mathf.Min(detailHeight - 1, playerDY + clearRadiusY);

        bool changed = false;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float distWorld = DetailDistanceToPlayerWorld(x, y, playerPos);

                if (distWorld <= paintRadiusWorld)
                {
                    int newValue = EvaluateGrassValue(x, y);
                    if (detailCache[y, x] != newValue)
                    {
                        detailCache[y, x] = newValue;
                        changed = true;
                    }
                }
                else if (distWorld >= clearRadiusWorld)
                {
                    if (detailCache[y, x] != 0)
                    {
                        detailCache[y, x] = 0;
                        changed = true;
                    }
                }
            }
        }

        if (changed || force)
            terrainData.SetDetailLayer(0, 0, detailLayerIndex, detailCache);
    }

    private int EvaluateGrassValue(int detailX, int detailY)
    {
        Vector3 worldPos = DetailToWorld(detailX, detailY);

        if (worldPos.y < minHeight || worldPos.y > maxHeight)
            return 0;

        float normX = Mathf.Clamp01((worldPos.x - terrain.transform.position.x) / terrainData.size.x);
        float normZ = Mathf.Clamp01((worldPos.z - terrain.transform.position.z) / terrainData.size.z);

        float steepness = terrainData.GetSteepness(normX, normZ);
        if (steepness > slopeLimit)
            return 0;

        float allowedWeight = GetMaxAllowedLayerWeight(worldPos);
        if (allowedWeight < minAllowedLayerWeight)
            return 0;

        if (useNoise)
        {
            float noise = Mathf.PerlinNoise(worldPos.x * noiseScale, worldPos.z * noiseScale);
            if (noise < noiseThreshold)
                return 0;
        }

        // Scale density by terrain layer weight.
        float normalizedWeight = Mathf.InverseLerp(minAllowedLayerWeight, 1f, allowedWeight);
        int weightedDensity = Mathf.RoundToInt(normalizedWeight * grassDensity);

        return Mathf.Max(0, weightedDensity);
    }

    private float GetMaxAllowedLayerWeight(Vector3 worldPos)
    {
        if (alphamaps == null)
            return 0f;

        Vector3 terrainPos = terrain.transform.position;

        float normX = Mathf.Clamp01((worldPos.x - terrainPos.x) / terrainData.size.x);
        float normZ = Mathf.Clamp01((worldPos.z - terrainPos.z) / terrainData.size.z);

        int mapX = Mathf.Clamp(
            Mathf.RoundToInt(normX * (terrainData.alphamapWidth - 1)),
            0,
            terrainData.alphamapWidth - 1
        );

        int mapZ = Mathf.Clamp(
            Mathf.RoundToInt(normZ * (terrainData.alphamapHeight - 1)),
            0,
            terrainData.alphamapHeight - 1
        );

        float bestWeight = 0f;

        foreach (int layerIndex in allowedLayerIndices)
        {
            if (layerIndex >= 0 && layerIndex < terrainData.alphamapLayers)
            {
                float weight = alphamaps[mapZ, mapX, layerIndex];
                if (weight > bestWeight)
                    bestWeight = weight;
            }
        }

        return bestWeight;
    }

    private void WorldToDetail(Vector3 worldPos, out int dx, out int dy)
    {
        Vector3 local = worldPos - terrain.transform.position;

        float nx = Mathf.Clamp01(local.x / terrainData.size.x);
        float ny = Mathf.Clamp01(local.z / terrainData.size.z);

        dx = Mathf.RoundToInt(nx * (detailWidth - 1));
        dy = Mathf.RoundToInt(ny * (detailHeight - 1));
    }

    private Vector3 DetailToWorld(int dx, int dy)
    {
        float nx = dx / (float)(detailWidth - 1);
        float nz = dy / (float)(detailHeight - 1);

        float worldX = terrain.transform.position.x + nx * terrainData.size.x;
        float worldZ = terrain.transform.position.z + nz * terrainData.size.z;
        float worldY = terrain.SampleHeight(new Vector3(worldX, 0f, worldZ)) + terrain.transform.position.y;

        return new Vector3(worldX, worldY, worldZ);
    }

    private float DetailDistanceToPlayerWorld(int dx, int dy, Vector3 playerPos)
    {
        Vector3 worldPos = DetailToWorld(dx, dy);
        Vector2 a = new Vector2(worldPos.x, worldPos.z);
        Vector2 b = new Vector2(playerPos.x, playerPos.z);
        return Vector2.Distance(a, b);
    }
}
