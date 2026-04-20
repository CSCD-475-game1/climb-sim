using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class VegetationChunkManager : MonoBehaviour
{
    public Transform player;
    public OffRoadWheelCarController playerCar;
    private Transform playerPosTransform;

    public GameObject grassPrefab1;
    public GameObject grassPrefab2;
    public GameObject rocksPrefab1;
    public GameObject rocksPrefab2;

    // list of tree prefabs, with logs included
    public List<GameObject> treePrefabs; // assign in inspector, include logs here with appropriate naming or tagging
    public GameObject logPrefab1; // assign in inspector

    public GameObject treePrefab1;
    public GameObject treePrefab2;

    public int chunkSize = 20;
    public int loadRadius = 1;
    public int objectsPerChunk = 40;
    public LayerMask groundMask;
    public float yOffset = 0.02f;

    private Dictionary<Vector2Int, List<GameObject>> loadedChunks = new();
    private Vector2Int currentPlayerChunk;

    [SerializeField] private SplineContainer roadSpline; 
    [SerializeField] private int roadSampleCount = 130;
    [SerializeField] private float minLogDistanceFromRoad = 12f;

    private readonly List<Vector3> roadSamplePoints = new();

    void Start()
    {
        CacheRoadSamplePoints();
        UpdateChunks(force: true);
    }

    void Update()
    {
        UpdateChunks();
    }

    private void CacheRoadSamplePoints()
    {
        roadSamplePoints.Clear();

        if (roadSpline == null || roadSpline.Splines.Count == 0)
            return;

        var spline = roadSpline.Splines[0];

        for (int i = 0; i < roadSampleCount; i++)
        {
            float t = roadSampleCount == 1 ? 0f : i / (float)(roadSampleCount - 1);
            Vector3 localPoint = spline.EvaluatePosition(t);
            Vector3 worldPoint = roadSpline.transform.TransformPoint(localPoint);
            roadSamplePoints.Add(worldPoint);
        }
    }

    void UpdateChunks(bool force = false)
    {
        if (player == null || 
                grassPrefab1 == null || 
                grassPrefab2 == null ||
                rocksPrefab1 == null ||
                rocksPrefab2 == null ||
                treePrefab1 == null ||
                treePrefab2 == null 
            ) return;

        if (playerCar != null && playerCar.canDrive) {
            playerPosTransform = playerCar.transform;
        } else {
            playerPosTransform = player;
        }

        Vector2Int newChunk = WorldToChunk(playerPosTransform.position);

        if (!force && newChunk == currentPlayerChunk)
            return;

        currentPlayerChunk = newChunk;

        HashSet<Vector2Int> needed = new();

        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int z = -loadRadius; z <= loadRadius; z++)
            {
                Vector2Int coord = new Vector2Int(
                    currentPlayerChunk.x + x,
                    currentPlayerChunk.y + z
                );

                needed.Add(coord);

                if (!loadedChunks.ContainsKey(coord))
                {
                    LoadChunk(coord);
                }
            }
        }

        List<Vector2Int> toRemove = new();

        foreach (var kvp in loadedChunks)
        {
            if (!needed.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (Vector2Int coord in toRemove)
        {
            UnloadChunk(coord);
        }
    }

    Vector2Int WorldToChunk(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / chunkSize);
        int z = Mathf.FloorToInt(worldPos.z / chunkSize);
        return new Vector2Int(x, z);
    }

    void LoadChunk(Vector2Int coord)
    {
        List<GameObject> envObjects = new();

        Vector3 chunkOrigin = new Vector3(
            coord.x * chunkSize,
            0f,
            coord.y * chunkSize
        );

        Random.InitState(coord.x * 73856093 ^ coord.y * 19349663);

        for (int i = 0; i < objectsPerChunk; i++)
        {
            float localX = Random.Range(0f, chunkSize);
            float localZ = Random.Range(0f, chunkSize);

            //Vector3 rayStart = new Vector3(
                //chunkOrigin.x + localX,
                //raycastHeight,
                //chunkOrigin.z + localZ
            //);
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null) return;

            Vector3 samplePos = new Vector3(
                chunkOrigin.x + localX,
                0f,
                chunkOrigin.z + localZ
            );

            float terrainY = terrain.SampleHeight(samplePos) + terrain.transform.position.y;
            float raycastHeight = terrainY + 10f; // start raycast above the terrain

            Vector3 rayStart = new Vector3(
                samplePos.x,
                 raycastHeight,
                samplePos.z
            );


            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask))
            {
                //Terrain terrain = Terrain.activeTerrain;
                if (terrain == null) return;

                int trailLayerIndex = 0; // example only

                if (!IsForestPosition(terrain, hit.point, trailLayerIndex, 0.4f))
                {
                    continue; // skip grass on trail
                }

                // random choice of grass or rock
                float rand = Random.value;
                if (rand < 0.6f)
                {
                    if (Random.value < 0.7f)
                    {
                        GameObject grass = Instantiate(
                            grassPrefab1,
                            hit.point + Vector3.up * yOffset,
                            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                            transform
                        );
                        envObjects.Add(grass);
                    } else {
                        GameObject grass = Instantiate(
                            grassPrefab2,
                            hit.point + Vector3.up * yOffset,
                            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                            transform
                        );
                        envObjects.Add(grass);
                    }
                } else if (rand < 0.95f) {

                    if (Random.value < 0.7f)
                    {
                        GameObject rock = Instantiate(
                            rocksPrefab1,
                            hit.point + Vector3.up * yOffset,
                            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                            transform
                        );
                        envObjects.Add(rock);
                    } else {
                        GameObject rock = Instantiate(
                            rocksPrefab2,
                            hit.point + Vector3.up * yOffset,
                            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                            transform
                        );
                        envObjects.Add(rock);
                    }
                }
                else
                {
                    // trees
                    //
                    float roll = Random.value;

                    Vector3 spawnPos = hit.point + Vector3.up * yOffset;
                    Quaternion rotation;
                    Vector3 scale;

                    float distanceToRoad = GetDistanceToRoad(hit.point);
                    bool allowLog = distanceToRoad >= minLogDistanceFromRoad;
                    //Debug.Log($"Distance to road: {distanceToRoad:F1} (allow log: {allowLog})");

                    // choose prefab from list
                    int randIndex = Random.Range(0, treePrefabs.Count);
                    GameObject prefabToSpawn = treePrefabs[randIndex];

                    GameObject logPrefab = logPrefab1; // default log prefab, can be overridden by treePrefabs if they include logs with specific naming or tagging

                    if (roll > 0.8 && allowLog)
                    {
                        prefabToSpawn = logPrefab; 
                    } 

                    // random yaw always
                    float yaw = Random.Range(0f, 360f);

                    // slight leaning for standing trees
                    float leanX = 0f;
                    float leanZ = 0f;

                    // logs lie on ground
                    if (prefabToSpawn == logPrefab && allowLog)
                    {
                        rotation = Quaternion.Euler(
                            Random.Range(80f, 100f),
                            yaw,
                            Random.Range(-8f, 8f)
                        );

                        float logScale = Random.Range(0.15f, 0.5f);
                        scale = new Vector3(logScale, logScale, logScale);

                        // sink into ground a bit
                        spawnPos.y -= 0.3f;
                    }
                    else
                    {
                        // small lean
                        leanX = Random.Range(-6f, 6f);
                        leanZ = Random.Range(-6f, 6f);

                        rotation = Quaternion.Euler(leanX, yaw, leanZ);

                        // mostly normal trees, sometimes bigger
                        float uniformScale = Random.value < 0.05f
                            ? Random.Range(1.2f, 1.7f)   // occasional large tree
                            : Random.Range(0.15f, 0.25f); // normal variation

                        scale = new Vector3(uniformScale, uniformScale, uniformScale);
                    }

                    GameObject tree = Instantiate(
                        prefabToSpawn,
                        spawnPos,
                        rotation,
                        transform
                    );

                    tree.transform.localScale = scale;
                    envObjects.Add(tree);
                }
            }
        }

        if (loadedChunks.ContainsKey(coord))
            return;

        loadedChunks.Add(coord, envObjects);

    }

    void UnloadChunk(Vector2Int coord)
    {
        if (!loadedChunks.TryGetValue(coord, out List<GameObject> grassObjects))
            return;

        foreach (GameObject grass in grassObjects)
        {
            if (grass != null)
                Destroy(grass);
        }

        loadedChunks.Remove(coord);
    }

    bool IsForestPosition(Terrain terrain, Vector3 worldPos, int trailLayerIndex, float threshold = 0.5f)
    {
        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        float normX = Mathf.Clamp01((worldPos.x - terrainPos.x) / data.size.x);
        float normZ = Mathf.Clamp01((worldPos.z - terrainPos.z) / data.size.z);

        int mapX = Mathf.Clamp(
            Mathf.RoundToInt(normX * (data.alphamapWidth - 1)),
            0,
            data.alphamapWidth - 1
        );

        int mapZ = Mathf.Clamp(
            Mathf.RoundToInt(normZ * (data.alphamapHeight - 1)),
            0,
            data.alphamapHeight - 1
        );

        float[,,] splat = data.GetAlphamaps(mapX, mapZ, 1, 1);

        float trailWeight = splat[0, 0, trailLayerIndex];
        return trailWeight >= threshold;
    }

    private float GetDistanceToRoad(Vector3 worldPos)
    {
        float bestSqr = float.MaxValue;

        foreach (Vector3 p in roadSamplePoints)
        {
            float sqr = (worldPos - p).sqrMagnitude;
            if (sqr < bestSqr)
                bestSqr = sqr;
        }

        return Mathf.Sqrt(bestSqr);
    }
}
