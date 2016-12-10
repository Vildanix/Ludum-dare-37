using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGrid {
    private Mesh gridMesh;
    private Mesh highlightMesh;

    // building structures
    private List<Vector3> gridVertices = new List<Vector3>();
    private List<Vector3> highlightVertices = new List<Vector3>();

    private List<int> gridTriangles = new List<int>();
    private List<int> highlightTriangles = new List<int>();

    private List<Vector2> gridUVs = new List<Vector2>();
    private List<Vector2> hightlightUVs = new List<Vector2>();

    // create new meshes for grid. (Max 150 x 100 grid)
    public void CreateGrid(int cell_x, int cell_y) {

        gridMesh = new Mesh();
        gridMesh.name = "Grid Plane";

        highlightMesh = new Mesh();
        highlightMesh.name = "Highlight Cells";

        // create mesh vertices. Each grid cell have separate quad
        for (int i = 0, y = 0; y <= cell_y; y++) {
            for (int x = 0; x <= cell_x; x++, i++) {
                CreateGridQuad(i, x, y, new Vector2(0, 0));
            }
        }


        gridMesh.vertices = gridVertices.ToArray();
        highlightMesh.vertices = highlightVertices.ToArray();

        gridMesh.triangles = gridTriangles.ToArray();
        highlightMesh.triangles = highlightTriangles.ToArray();

        gridMesh.uv = gridUVs.ToArray();
        highlightMesh.uv = hightlightUVs.ToArray();
    }

    private void CreateGridQuad(int cellIndex, int x, int y, Vector2 textureOffset) {
        // new vertices for quad
        gridVertices.Add(new Vector3(x, 0, y));
        gridVertices.Add(new Vector3(x + 1, 0, y));
        gridVertices.Add(new Vector3(x + 1, 0, y + 1));
        gridVertices.Add(new Vector3(x, 0, y + 1));

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
    }

    // get generated mesh for grid
    public Mesh getGridMesh() {
        return gridMesh;
    }

    // get generated mesh for highlight
    public Mesh getHighlightMesh() {
        return highlightMesh;
    }
    
}
