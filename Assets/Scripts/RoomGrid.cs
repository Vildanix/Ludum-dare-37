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
        // set current grid size (-2 is for grid cell border)
        gridWidth = cell_x - 2;
        gridHeight = cell_y - 2;

        gridMesh = new Mesh();
        gridMesh.name = "Room Grid plane";

        if (!isHighlight) {
            // leave space
            gridInformation = new GridData[gridWidth * gridHeight];
            for (int i = 0; i < gridWidth * gridHeight; i++) {
                gridInformation[i] = new GridData(i % gridWidth, i / gridWidth, this);
            }
        }

        // create mesh vertices. Each grid cell have separate quad
        if (isHighlight) {
            for (int i = 0, y = 0; y < gridHeight; y++) {
                for (int x = 0; x < gridWidth; x++, i++) {
                    CreateGridQuad(i, x + 1, y + 1, new Vector2(0, 0), spacing, isHighlight);
                }
            }
        } else {
            for (int i = 0, y = 0; y < cell_y; y++) {
                for (int x = 0; x < cell_x; x++, i++) {
                    CreateGridQuad(i, x, y, new Vector2(0, 0), spacing, isHighlight);
                }
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

    private void CreateGridQuad(int cellIndex, int x, int y, Vector2 textureOffset, float spacing, bool isHighlight = false) {
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

        if (isHighlight) {
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

    public void ResetGridColors() {
        for (int i = 0; i < gridColors.Count; i++) {
            gridColors[i] = Color.black;
        }

        AsignMeshValues();
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

    public GridData[] GetGridNeighbors(int x, int y) {
        List<GridData> neighbors = new List<GridData>();

        if (IsValidGridCoord(x-1, y)) {
            neighbors.Add(gridInformation[x - 1 + y * gridWidth]);
        }

        if (IsValidGridCoord(x + 1, y)) {
            neighbors.Add(gridInformation[x + 1 + y * gridWidth]);
        }

        if (IsValidGridCoord(x, y - 1)) {
            neighbors.Add(gridInformation[x + (y - 1) * gridWidth]);
        }

        if (IsValidGridCoord(x, y + 1)) {
            neighbors.Add(gridInformation[x + (y + 1) * gridWidth]);
        }

        return neighbors.ToArray();
    }

    private bool IsValidGridCoord(int x, int y) {
        if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight) {
            return true;
        }

        return false;
    }

    // Get array of cells between two point in world space on grid that are currently on bottom
    public GridData[] GetGridCellsBetweenPoints(Vector3 startPoint, Vector3 endPoints, bool onlyAvalible = false) {
        float worldWidth = gridWidth * transform.localScale.x;
        float worldHeight = gridHeight * transform.localScale.z;

        int startX = Mathf.Clamp(Mathf.FloorToInt((worldWidth / 2 + startPoint.x) * gridWidth / worldWidth), 0, gridWidth - 1);
        int endX = Mathf.Clamp(Mathf.FloorToInt((worldWidth / 2 + endPoints.x) * gridWidth / worldWidth), 0, gridWidth - 1);

        int startY = Mathf.Clamp(Mathf.FloorToInt((worldHeight / 2 + startPoint.z) * gridHeight / worldWidth), 0, gridHeight - 1);
        int endY = Mathf.Clamp(Mathf.FloorToInt((worldHeight / 2 + endPoints.z) * gridHeight / worldWidth), 0, gridHeight - 1);

        // select row or column from DataGrid. Direction depends on longer selection in both direction
        // select horizontaly
        List<GridData> subgrid = new List<GridData>();
        if (Mathf.Abs(endX - startX) >= Mathf.Abs(endY - startY)) {
            // keep order from start vector
            if (startX > endX) {
                for (int x = startX; x >= endX; x--) {
                    if (onlyAvalible && !gridInformation[x + startY * gridWidth].IsAvalible) {
                        continue;
                    }
                    subgrid.Add(gridInformation[x + startY * gridWidth]);
                }
            } else {
                for (int x = startX; x <= endX; x++) {
                    if (onlyAvalible && !gridInformation[x + startY * gridWidth].IsAvalible) {
                        continue;
                    }
                    subgrid.Add(gridInformation[x + startY * gridWidth]);
                }
            }

            

            return subgrid.ToArray();

        // select verticaly
        } else {
            if (startY > endY) {
                for (int y = startY; y >= endY; y--) {
                    if (onlyAvalible && !gridInformation[startX + y * gridWidth].IsAvalible) {
                        continue;
                    }
                    subgrid.Add(gridInformation[startX + y * gridWidth]);
                }
            } else {
                for (int y = startY; y <= endY; y++) {
                    if (onlyAvalible && !gridInformation[startX + y * gridWidth].IsAvalible) {
                        continue;
                    }
                    subgrid.Add(gridInformation[startX + y * gridWidth]);
                }
            }

            

            return subgrid.ToArray();
        }
    }

    public GenericBuilding InstantiateBuildingOnGrid(GenericBuilding prototype, int x, int y) {
        GenericBuilding instantiatedBuilding = Instantiate(prototype, transform);

        instantiatedBuilding.transform.localPosition = getConstructionPosition(x, y);
        instantiatedBuilding.transform.localScale = Vector3.one;
        instantiatedBuilding.transform.localRotation = Quaternion.identity;

        return instantiatedBuilding;
    }

    public Vector3 getConstructionPosition(int x, int y) {
        // offset by 1 + 0.5 to compensate empty grid border and center of cell
        return new Vector3(x + 1.5f, 0, y + 1.5f);
    }

    public void PlaceBuildingRandom(GenericBuilding building) {
        int x, y;
        x = (int)(Random.value * gridWidth);
        y = (int)(Random.value * gridWidth);

        while (!GetGridDataCell(x, y).IsAvalible) {
            x = (int)(Random.value * gridWidth);
            y = (int)(Random.value * gridWidth);
        }

        GetGridDataCell(x, y).ConstructBuilding(building, true);
    }

    public Vector3[] GetRandomPath(int tileLength) {
        List<GridData> buildedCells = new List<GridData>();
        List<Vector3> path = new List<Vector3>();
        foreach (GridData cell in gridInformation) {
            if (cell.getCurrentBuilding() != null) {
                buildedCells.Add(cell);
            }
        }

        int startBuildingIndex = Mathf.FloorToInt(Random.value * buildedCells.Count);
        GridData currentCell = buildedCells[startBuildingIndex];
        path.Add(currentCell.getCurrentBuilding().transform.localPosition);

        int pathIndex = 1;
        bool foundNextGrid = true;
        while(pathIndex <= tileLength && foundNextGrid) {
            GridData[] neighbors = GetGridNeighbors(currentCell.PosX, currentCell.PosY);

            //foundNextGrid = false;
            // on last path index find ending building 
            GenericBuilding backupBuilding = null;
            if (pathIndex == tileLength) {
                foreach(GridData cell in neighbors) {
                    GenericBuilding buildingOnCell = cell.getCurrentBuilding();
                    if (buildingOnCell && !buildingOnCell.IsGridConnection) {
                        path.Add(buildingOnCell.transform.localPosition);
                        break;
                    }
                }

            // find next grid tile that wasnt this
            } else {
                foreach (GridData cell in neighbors) {
                    GenericBuilding buildingOnCell = cell.getCurrentBuilding();
                    if (buildingOnCell && buildingOnCell.IsGridConnection && cell != currentCell) {
                        path.Add(buildingOnCell.transform.localPosition);
                        currentCell = cell;
                        break;
                    }
                }
            }

            pathIndex++;
        }
        

        return path.ToArray();
    }

}
