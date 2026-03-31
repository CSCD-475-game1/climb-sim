using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[ExecuteAlways]
public class TrailPreview : MonoBehaviour
{
    [Header("Input")]
    public TextAsset csvFile;
    public Terrain terrain;

    [Header("Preview")]
    public bool spawnSpheres = true;
    public float sphereSize = 10.0f;
    public float heightOffset = 0.1f;
    public Material lineMaterial;
    public Material dotMaterial;
    public float lineWidth = 0.75f;
    public bool reset;

    private readonly List<Vector3> _points = new();


    private void Start()
    {
        foreach (Transform child in transform)
        {
            if (Application.isEditor)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
        }

        if (csvFile == null)
        {
            Debug.LogError("TrailPreview: CSV file is missing.");
            return;
        }

        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogError("TrailPreview: No terrain assigned and no active terrain found.");
                return;
            }
        }

        ParseCsv();
        DrawPreview();

        
    }

    void Update() {
        if (reset) {
            foreach (Transform child in transform)
            {
                if (Application.isEditor)
                    DestroyImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
            }
            ParseCsv();
            DrawPreview();
            PaintTrail(_points);
            reset = false;
        }
    }

    private void ParseCsv()
    {
        _points.Clear();

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            Debug.LogError("TrailPreview: CSV is empty.");
            return;
        }

        string[] header = SplitCsvLine(lines[0]);
        int xIndex = Array.FindIndex(header, h => h.Trim().Equals("u_x", StringComparison.OrdinalIgnoreCase));
        int zIndex = Array.FindIndex(header, h => h.Trim().Equals("u_z", StringComparison.OrdinalIgnoreCase));

        if (xIndex < 0 || zIndex < 0)
        {
            Debug.LogError("TrailPreview: CSV must contain unity_x and unity_z columns.");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = SplitCsvLine(lines[i]);
            if (cols.Length <= Mathf.Max(xIndex, zIndex))
                continue;

            if (!float.TryParse(cols[xIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                continue;

            if (!float.TryParse(cols[zIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                continue;

            x = 4062f - x;
            z = z - 240f;
            float y = terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y + heightOffset;
            _points.Add(new Vector3(x, y, z));
        }

        Debug.Log($"TrailPreview: Loaded {_points.Count} trail points.");
    }

    private void DrawPreview()
    {
        if (_points.Count == 0)
        {
            Debug.LogWarning("TrailPreview: No valid points to draw.");
            return;
        }

        if (spawnSpheres)
        {
            for (int i = 0; i < _points.Count; i+= 10)
            {
                GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dot.name = $"TrailPoint_{i:D4}";
                // set material
                if (dotMaterial != null)
                {
                    Renderer renderer = dot.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = dotMaterial;
                    }
                }
                dot.transform.SetParent(transform, true);
                dot.transform.position = _points[i];
                dot.transform.localScale = Vector3.one * sphereSize;
            }
        }

        LineRenderer lr = gameObject.GetComponent<LineRenderer>();
        if (lr == null)
            lr = gameObject.AddComponent<LineRenderer>();

        lr.useWorldSpace = true;
        lr.positionCount = _points.Count;
        lr.SetPositions(_points.ToArray());
        lr.widthMultiplier = lineWidth;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 2;
        lr.alignment = LineAlignment.View;
        lr.material = lineMaterial != null ? lineMaterial : DefaultLineMaterial();
    }

    private static string[] SplitCsvLine(string line)
    {
        return line.Split(',');
    }

    private static Material DefaultLineMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        return new Material(shader);
    }

    public int terrainLayerIndex = 1; // your trail/sand layer
    public float brushRadius = 3f;

    void PaintTrail(List<Vector3> points)
    {
        TerrainData td = terrain.terrainData;

        int mapRes = td.alphamapResolution;
        float terrainWidth = td.size.x;
        float terrainLength = td.size.z;

        float[,,] alphamaps = td.GetAlphamaps(0, 0, mapRes, mapRes);

        foreach (var p in points)
        {
            int mapX = (int)((p.x / terrainWidth) * mapRes);
            int mapZ = (int)((p.z / terrainLength) * mapRes);

            int radius = Mathf.RoundToInt((brushRadius / terrainWidth) * mapRes);

            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    int nx = mapX + x;
                    int nz = mapZ + z;

                    if (nx < 0 || nz < 0 || nx >= mapRes || nz >= mapRes)
                        continue;

                    float dist = Mathf.Sqrt(x * x + z * z) / radius;
                    float strength = Mathf.Clamp01(1 - dist);

                    // clear others slightly
                    for (int l = 0; l < td.alphamapLayers; l++)
                        alphamaps[nz, nx, l] *= (1 - strength);

                    // apply trail layer
                    alphamaps[nz, nx, terrainLayerIndex] += strength;
                }
            }
        }

        td.SetAlphamaps(0, 0, alphamaps);
    }
}
