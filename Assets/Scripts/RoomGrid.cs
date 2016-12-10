using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class RoomGrid : MonoBehaviour{
    private Mesh gridMesh;

    // building structures
    private List<Vector3> gridVertices;
    private List<int> gridTriangles;
    private List<Vector2> gridUVs;
    private List<Vector4> gridTangets;

    MeshFilter meshFilter;

    public void Awake() {
        gridVertices = new List<Vector3>();
        gridTriangles = new List<int>();
        gridUVs = new List<Vector2>();
        gridTangets = new List<Vector4>();
        
        meshFilter = GetComponent<MeshFilter>();
    }

    // create new meshes for grid. (Max 150 x 100 grid)
    public void CreateGrid(int cell_x, int cell_y, float spacing = 0.0f) {
        gridMesh = new Mesh();
        gridMesh.name = "Room Grid plane";

        // create mesh vertices. Each grid cell have separate quad
        for (int i = 0, y = 0; y < cell_y; y++) {
            for (int x = 0; x < cell_x; x++, i++) {
                CreateGridQuad(i, x, y, new Vector2(0, 0), spacing);
            }
        }

        gridMesh.vertices = gridVertices.ToArray();

        gridMesh.triangles = gridTriangles.ToArray();

        gridMesh.uv = gridUVs.ToArray();

        gridMesh.RecalculateNormals();

        gridMesh.tangents = gridTangets.ToArray();

        meshFilter.mesh = gridMesh;
    }

    private void CreateGridQuad(int cellIndex, int x, int y, Vector2 textureOffset, float spacing) {
        // new vertices for quad
        gridVertices.Add(new Vector3(x + spacing, 0, y + spacing));
        gridVertices.Add(new Vector3(x - spacing + 1, 0, y + spacing));
        gridVertices.Add(new Vector3(x - spacing + 1, 0, y - spacing + 1));
        gridVertices.Add(new Vector3(x + spacing, 0, y - spacing + 1));

        // two triangles for new vertex quad
        gridTriangles.Add(cellIndex * 4);
        gridTriangles.Add((cellIndex * 4) + 2);
        gridTriangles.Add((cellIndex * 4) + 1);
        gridTriangles.Add((cellIndex * 4) + 0);
        gridTriangles.Add((cellIndex * 4) + 3);
        gridTriangles.Add((cellIndex * 4) + 2);

        // expect only 4 textures for cell
        gridUVs.Add(new Vector2(textureOffset.x, textureOffset.y));
        gridUVs.Add(new Vector2(textureOffset.x + 0.5f, textureOffset.y));
        gridUVs.Add(new Vector2(textureOffset.x + 0.5f, textureOffset.y + 0.5f));
        gridUVs.Add(new Vector2(textureOffset.x, textureOffset.y + 0.5f));

        gridTangets.Add(new Vector4(1f, 0f, 0f, -1f));
        gridTangets.Add(new Vector4(1f, 0f, 0f, -1f));
        gridTangets.Add(new Vector4(1f, 0f, 0f, -1f));
        gridTangets.Add(new Vector4(1f, 0f, 0f, -1f));
    }

    // get generated mesh for grid
    public Mesh getGridMesh() {
        return gridMesh;
    }
    
}
