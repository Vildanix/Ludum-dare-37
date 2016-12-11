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
    private List<Color> gridColors;

    private GridData[] gridInformation;
    private int gridWidth;
    private int gridHeight;

    MeshFilter meshFilter;
    private bool useColors = false;

    public void Awake() {
        gridVertices = new List<Vector3>();
        gridTriangles = new List<int>();
        gridUVs = new List<Vector2>();
        gridTangets = new List<Vector4>();
        gridColors = new List<Color>();
        
        meshFilter = GetComponent<MeshFilter>();
    }

    // create new meshes for grid. (Max 150 x 100 grid)
    public void CreateGrid(int cell_x, int cell_y, float spacing = 0.0f, bool isHighlight = false) {
        // set current grid size
        gridWidth = cell_x;
        gridHeight = cell_y;

        gridMesh = new Mesh();
        gridMesh.name = "Room Grid plane";

        if (!isHighlight) {
            gridInformation = new GridData[cell_x * cell_y];
            for (int i = 0; i < cell_x * cell_y; i++) {
                gridInformation[i] = new GridData();
                gridInformation[i].PosX = i % gridWidth;
                gridInformation[i].PosY = i / gridWidth;
            }
        }

        // create mesh vertices. Each grid cell have separate quad
        for (int i = 0, y = 0; y < cell_y; y++) {
            for (int x = 0; x < cell_x; x++, i++) {
                CreateGridQuad(i, x, y, new Vector2(0, 0), spacing, isHighlight);
            }
        }

        // set constructed mesh data
        useColors = true;
        AsignMeshValues();

        meshFilter.mesh = gridMesh;
    }

    private void AsignMeshValues() {
        gridMesh.vertices = gridVertices.ToArray();
        gridMesh.triangles = gridTriangles.ToArray();
        gridMesh.uv = gridUVs.ToArray();
        gridMesh.tangents = gridTangets.ToArray();
        if (useColors) {
            gridMesh.colors = gridColors.ToArray();
        }
        gridMesh.RecalculateNormals();
    }

    private void CreateGridQuad(int cellIndex, int x, int y, Vector2 textureOffset, float spacing, bool useColor = false) {
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

        if (useColor) {
            gridColors.Add(Color.black);
            gridColors.Add(Color.black);
            gridColors.Add(Color.black);
            gridColors.Add(Color.black);
        }
    }

    // get generated mesh for grid
    public Mesh GetGridMesh() {
        return gridMesh;
    }

    // set cell color for texture offset. Red value shift texture to right, green shift up
    public void SetCellColor(int x, int y, Color color) {
        x = x * 4;
        y = y * 4;
        if (x + y * gridWidth < gridColors.Count) {
            gridColors[x + y * gridWidth] = color;
            gridColors[x + y * gridWidth + 1] = color;
            gridColors[x + y * gridWidth + 2] = color;
            gridColors[x + y * gridWidth + 3] = color;

            AsignMeshValues();
        }
    }
    
    public GridData GetGridDataCell(int x, int y) {
        if (x + y * gridWidth < gridInformation.Length) {
            return gridInformation[x + y * gridWidth];
        }

        Debug.LogError("Calling grid information on non data storage grid?");
        return null;
    }

    public GridData[] GetGridData() {
        return gridInformation;
    }
    
    // Get array of cells between two point in world space on grid that are currently on bottom
    public GridData[] GetGridCellsBetweenPoints(Vector3 startPoint, Vector3 endPoints) {
        float worldWidth = gridWidth * transform.localScale.x;
        float worldHeight = gridHeight * transform.localScale.z;

        int startX = Mathf.Clamp(Mathf.FloorToInt((worldWidth / 2 + startPoint.x) * gridWidth / worldWidth), 0, gridWidth - 1);
        int endX = Mathf.Clamp(Mathf.FloorToInt((worldWidth / 2 + endPoints.x) * gridWidth / worldWidth), 0, gridWidth - 1);

        int startY = Mathf.Clamp(Mathf.FloorToInt((worldHeight / 2 + startPoint.z) * gridHeight / worldWidth), 0, gridHeight - 1);
        int endY = Mathf.Clamp(Mathf.FloorToInt((worldHeight / 2 + endPoints.z) * gridHeight / worldWidth), 0, gridHeight - 1);

        int temp;
        // select row or column from DataGrid. Direction depends on longer selection in both direction
        // select horizontaly
        if (Mathf.Abs(endX - startX) >= Mathf.Abs(endY - startY)) {
            if (startX > endX) {
                temp = startX;
                startX = endX;
                endX = temp;
            }

            GridData[] subgrid = new GridData[endX - startX + 1];
            for(int x = startX; x <= endX; x++) {
                subgrid[x - startX] = gridInformation[x + startY * gridWidth];
            }

            return subgrid;

        // select verticaly
        } else {
            if (startY > endY) {
                temp = startY;
                startY = endY;
                endY = temp;
            }

            GridData[] subgrid = new GridData[endY - startY + 1];
            for (int y = startY; y <= endY; y++) {
                subgrid[y - startY] = gridInformation[startX + y * gridWidth];
            }

            return subgrid;
        }
    }



}
