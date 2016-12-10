using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour {
    enum ROOM_SIDES {FLOOR, WALL_N, WALL_E, WALL_S, WALL_W, CEILING};
    Dictionary<ROOM_SIDES, RoomGrid> roomSides;
    RoomGrid highlight;

    // presets
    public GameObject gridPrefab;
    public Material highlightMat;
    public Material gridBaseMat;

    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cubeSize = 5.0f;

    // inicialize map qube
    void Awake () {
        if (!gridPrefab) {
            Debug.LogError("RoomController missing grid prefab");
        }

        highlight = InstantiateGridSide(new Vector3(-cubeSize / 2, 0.01f, -cubeSize / 2), new Vector3(0, 0, 0), 0.15f, "Highlight grid", highlightMat);

        // initialize room
        roomSides = new Dictionary<ROOM_SIDES, RoomGrid>();

        // floor
        RoomGrid currentGrid = InstantiateGridSide(new Vector3(-cubeSize / 2, 0f, -cubeSize / 2), new Vector3(0, 0, 0), 0.0f, "Floor grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.FLOOR, currentGrid);

        // north side
        currentGrid = InstantiateGridSide(new Vector3(-cubeSize / 2, cubeSize, -cubeSize / 2), new Vector3(90, 0, 0), 0.0f, "Wall N grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.WALL_N, currentGrid);

        // south side
        currentGrid = InstantiateGridSide(new Vector3(-cubeSize / 2, 0, cubeSize / 2), new Vector3(-90, 0, 0), 0.0f, "Wall S grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.WALL_S, currentGrid);

        // west side
        currentGrid = InstantiateGridSide(new Vector3(cubeSize / 2, 0, -cubeSize / 2), new Vector3(0, 0, 90), 0.0f, "Wall W grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.WALL_W, currentGrid);

        // east side
        currentGrid = InstantiateGridSide(new Vector3(-cubeSize / 2, cubeSize, -cubeSize / 2), new Vector3(0, 0, -90), 0.0f, "Wall E grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.WALL_E, currentGrid);

        // top side
        currentGrid = InstantiateGridSide(new Vector3(cubeSize / 2, cubeSize, -cubeSize / 2), new Vector3(0, 0, 180), 0.0f, "Ceiling grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.CEILING, currentGrid);
    }

    // instantiate grid on given position and return RoomGrid component on new grid
    private RoomGrid InstantiateGridSide(Vector3 position, Vector3 rotation, float spacing, string name, Material gridMat) {
        GameObject gridObj = Instantiate(gridPrefab);
        gridObj.transform.parent = this.transform;
        gridObj.transform.localScale = new Vector3(cubeSize / (gridWidth + 1), 1, cubeSize / (gridWidth + 1));
        gridObj.transform.localPosition = position;
        gridObj.transform.Rotate(rotation);
        gridObj.name = name;
        gridObj.GetComponent<MeshRenderer>().material = gridMat;

        RoomGrid grid = gridObj.GetComponent<RoomGrid>();
        grid.CreateGrid(gridWidth, gridHeight, spacing);

        return grid;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
