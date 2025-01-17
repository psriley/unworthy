using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour
{
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    [SerializeField] private Mesh PreMesh;

    public int Dimensions = 10;
    public float UVScale;
    public Transform playerTransform;
    public SphereCollider playerArea;
    public Octave[] Octaves;

    // Start is called before the first frame update
    void Start()
    {
        // Mesh Setup
        Mesh = new Mesh();
        Mesh.name = gameObject.name;

        Mesh.vertices = GenerateVerts();
        Mesh.triangles = GenerateTries();
        Mesh.uv = GenerateUVs();
        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();

        MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.mesh = Mesh;
    }

    public float GetHeight(Vector3 position)
    {
        // scale factor and position in local space
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        // get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        // clamp if the position is outside of the plane (error handling)
        p1.x = Mathf.Clamp(p1.x, 0, Dimensions);
        p1.z = Mathf.Clamp(p1.z, 0, Dimensions);
        p2.x = Mathf.Clamp(p2.x, 0, Dimensions);
        p2.z = Mathf.Clamp(p2.z, 0, Dimensions);
        p3.x = Mathf.Clamp(p3.x, 0, Dimensions);
        p3.z = Mathf.Clamp(p3.z, 0, Dimensions);
        p4.x = Mathf.Clamp(p4.x, 0, Dimensions);
        p4.z = Mathf.Clamp(p4.z, 0, Dimensions);

        // get the max distance to one of the edges and take that to compute max - dist
        var max = Mathf.Max(Vector3.Distance(p1, localPos), Vector3.Distance(p2, localPos), Vector3.Distance(p3, localPos), Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        var dist = (max - Vector3.Distance(p1, localPos))
                 + (max - Vector3.Distance(p2, localPos))
                 + (max - Vector3.Distance(p3, localPos))
                 + (max - Vector3.Distance(p4, localPos)) + Mathf.Epsilon;

        // weighted sum
        var height = Mesh.vertices[index((int)p1.x, (int)p1.z)].y * (max - Vector3.Distance(p1, localPos))
                   + Mesh.vertices[index((int)p2.x, (int)p2.z)].y * (max - Vector3.Distance(p2, localPos))
                   + Mesh.vertices[index((int)p3.x, (int)p3.z)].y * (max - Vector3.Distance(p3, localPos))
                   + Mesh.vertices[index((int)p4.x, (int)p4.z)].y * (max - Vector3.Distance(p4, localPos));

        // scale
        return height * transform.lossyScale.y / dist;
    }

    public Vector4 GetSpeed()
    {
        float maxXSpeed = Octaves[0].speed.x;
        float maxYSpeed = Octaves[0].speed.y;
        float xDir = 1;
        float yDir = 1;

        foreach (Octave o in Octaves)
        {
            if (Mathf.Abs(o.speed.x) > maxXSpeed) { maxXSpeed = o.speed.x; xDir = Mathf.Sign(o.speed.x); }
        }
        foreach (Octave o in Octaves)
        {
            if (Mathf.Abs(o.speed.y) > maxYSpeed) { maxYSpeed = o.speed.y; yDir = Mathf.Sign(o.speed.y); }
        }

        return new Vector4(xDir, maxXSpeed, yDir, maxYSpeed);
    }

    private int[] GenerateTries()
    {
        var tries = new int[Mesh.vertices.Length * 6];

        // two tirangles are one tile
        for (int x = 0; x < Dimensions; x++)
        {
            for (int z = 0; z < Dimensions; z++)
            {
                tries[index(x, z) * 6 + 0] = index(x, z);
                tries[index(x, z) * 6 + 1] = index(x + 1, z + 1);
                tries[index(x, z) * 6 + 2] = index(x + 1, z);
                tries[index(x, z) * 6 + 3] = index(x, z);
                tries[index(x, z) * 6 + 4] = index(x, z + 1);
                tries[index(x, z) * 6 + 5] = index(x + 1, z + 1);
            }
        }

        return tries;
    }

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[(Dimensions + 1) * (Dimensions + 1)];

        // equally distributed verts
        for (int x = 0; x <= Dimensions; x++)
        {
            for (int z = 0; z <= Dimensions; z++)
            {
                verts[index(x, z)] = new Vector3(x, 0, z);
            }
        }

        return verts;
    }

    private int index(int x, int z)
    {
        // x = 0 & z = 0 -> index = 0...x = 0 & z = 9 -> index = 9...x = 1 & z = 0 -> index = 12
        return x * (Dimensions + 1) + z;
    }

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[Mesh.vertices.Length];

        // always set one uv over n tiles then flip the uv and set it again
        for (int x = 0; x <= Dimensions; x++)
        {
            for (int z = 0; z <= Dimensions; z++)
            {
                var vec = new Vector2((x / UVScale) % 2, (z / UVScale) % 2);
                uvs[index(x, z)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }

        return uvs;
    }

    // Update is called once per frame
    void Update()
    {
        float heightMultiplier = 1;

        var playerAreaCenter = playerArea.bounds.center;
        var playerAreaSize = playerArea.bounds.size;

        var verts = Mesh.vertices;
        for (int x = 0; x <= Dimensions; x++)
        {
            for (int z = 0; z <= Dimensions; z++)
            {
                // Calculate the vertex position
                var vertexPosition = verts[index(x, z)];

                // Check if the vertex is within the player area
                if (IsWithinPlayerArea(vertexPosition, playerAreaCenter, playerAreaSize))
                {
                    heightMultiplier = 0.25f;
                }
                else
                {
                    heightMultiplier = 1;
                }

                // Apply height multiplier
                var y = CalculateVertexHeight(x, z, heightMultiplier);

                verts[index(x, z)] = new Vector3(x, y, z);
            }
        }

        Mesh.vertices = verts;
        Mesh.RecalculateNormals();
    }

    // Function to check if a vertex is within the player area
    private bool IsWithinPlayerArea(Vector3 vertexPosition, Vector3 playerAreaCenter, Vector3 playerAreaSize)
    {
        // Adjust the position based on the player area center
        var adjustedPosition = vertexPosition - playerAreaCenter;

        // Check if the adjusted position is within the player area size
        return Mathf.Abs(adjustedPosition.x) <= playerAreaSize.x &&
               Mathf.Abs(adjustedPosition.y) <= playerAreaSize.y &&
               Mathf.Abs(adjustedPosition.z) <= playerAreaSize.z;
    }

    // Function to calculate the vertex height with the height multiplier
    private float CalculateVertexHeight(int x, int z, float heightMultiplier)
    {
        var y = 0f;
        for (int o = 0; o < Octaves.Length; o++)
        {
            if (Octaves[o].alternate)
            {
                var perl = Mathf.PerlinNoise((x * Octaves[o].scale.x) / Dimensions, (z * Octaves[o].scale.y) / Dimensions) * Mathf.PI * 2f;
                y += Mathf.Cos(perl + Octaves[o].speed.magnitude * Time.time) * (Octaves[o].height * heightMultiplier);
            }
            else
            {
                var perl = Mathf.PerlinNoise((x * Octaves[o].scale.x + Time.time * Octaves[o].speed.x) / Dimensions, (z * Octaves[o].scale.y + Time.time * Octaves[o].speed.y) / Dimensions) - 0.5f;
                y += perl * (Octaves[o].height * heightMultiplier);
            }
        }

        return y;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireMesh(PreMesh, new Vector3(Dimensions * transform.localScale.x, 0, Dimensions * transform.localScale.z), Quaternion.identity, new Vector3(50, 0, 50));
    }

    [System.Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }
}
