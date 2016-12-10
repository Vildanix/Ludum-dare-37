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

    void Awake () {
        if (!gridPrefab) {
            Debug.LogError("RoomController missing grid prefab");
        }

        highlight = InstantiateGridSide(new Vector3(-cubeSize / 2, 0.01f, -cubeSize / 2), new Vector3(0, 0, 0), 0.15f, highlightMat);

        // initialize room
        roomSides = new Dictionary<ROOM_SIDES, RoomGrid>();

        // floor
        RoomGrid currentGrid = InstantiateGridSide(new Vector3(-cubeSize / 2, 0f, -cubeSize / 2), new Vector3(0, 0, 0), 0.0f, gridBaseMat);
        roomSides.Add(ROOM_SIDES.FLOOR, currentGrid);

        currentGrid = InstantiateGridSide(new Vector3(-cubeSize / 2, 0f, -cubeSize / 2), new Vector3(0, 0, 0), 0.0f, gridBaseMat);
        roomSides.Add(ROOM_SIDES.FLOOR, currentGrid);

    }

    // instantiate grid on given position and return RoomGrid component on new grid
    private RoomGrid InstantiateGridSide(Vector3 position, Vector3 rotation, float spacing, Material gridMat) {
        GameObject gridObj = Instantiate(gridPrefab);
        gridObj.transform.localScale = new Vector3(cubeSize / gridWidth, 1, cubeSize / gridHeight);
        gridObj.transform.localPosition = position;
        gridObj.transform.Rotate(rotation);
        gridObj.GetComponent<MeshRenderer>().material = gridMat;

        RoomGrid grid = gridObj.GetComponent<RoomGrid>();
        grid.CreateGrid(gridWidth, gridHeight, spacing);

        return grid;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
