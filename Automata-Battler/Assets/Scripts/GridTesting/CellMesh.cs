using UnityEngine;

//[ExecuteAlways]
public class BoardCellMesh : MonoBehaviour {
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    [SerializeField] public Color currentColor = Color.white; 

    // Reference to your mesh, which you rebuild as needed.
    private Mesh mesh;

    void OnEnable(){
        // Ensure we have the required components.
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if(meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if(mesh == null) {
            mesh = new Mesh();
            mesh.name = "HexMesh";
            meshFilter.sharedMesh = mesh;
        }
        // Optionally, ensure a material is assigned.
    }

    //no, don't do this Unity will complain
    // void OnValidate() {
    //     if(meshFilter == null)
    //         meshFilter = GetComponent<MeshFilter>();

    //     if(mesh == null) {
    //         mesh = new Mesh();
    //         mesh.name = "HexMesh";
    //         meshFilter.sharedMesh = mesh;
    //     }
    //     GenerateMesh();
    // }

    void Awake()
    {
        if(meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if(mesh == null) {
            mesh = new Mesh();
            mesh.name = "HexMesh";
            meshFilter.sharedMesh = mesh;
        }
        meshCollider = gameObject.AddComponent<MeshCollider>(); //dunno why the tutorial adds it manually
        GenerateMesh();
    }

    public void GenerateMesh() {
        
        mesh.Clear();

        // For a hexagon, we create a center + 6 outer vertices.
        Vector3[] vertices = new Vector3[7];
        vertices[0] = Vector3.zero; // center
        // Assume HexMetrics.corners is your static array of 6 corner positions.
        for (int i = 0; i < 6; i++) {
            vertices[i + 1] = HexMetrics.corners[i];
        }
        mesh.vertices = vertices;

        // Build triangles for 6 segments.
        int[] triangles = new int[6 * 3];
        for (int i = 0; i < 6; i++) {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i == 5 ? 1 : i + 2;
        }
        mesh.triangles = triangles;

        // Create a colors array that is the same length as vertices.
        Color[] colors = new Color[mesh.vertices.Length];
        for (int i = 0; i < colors.Length; i++) {
            colors[i] = currentColor;  // default white color
        }
        mesh.colors = colors;

        mesh.RecalculateNormals();

        // IMPORTANT: If any methods like SendMessage are triggered by mesh changes,
        // consider scheduling further updates via EditorApplication.delayCall.
        meshCollider.sharedMesh = mesh;
    }

    //dunno if this even works lol
    public void SetColors()
    {
        // Create a colors array that is the same length as vertices.
        Color[] colors = new Color[mesh.vertices.Length];
        for (int i = 0; i < colors.Length; i++) {
            colors[i] = currentColor;  // default white color
        }
        mesh.colors = colors;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }
}