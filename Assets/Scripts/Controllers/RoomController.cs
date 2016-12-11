using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour {
    public enum ROOM_SIDES {FLOOR, WALL_N, WALL_E, WALL_S, WALL_W, CEILING};
    Dictionary<ROOM_SIDES, RoomGrid> roomSides;
    public HightlightController highlightController;

    // presets
    public GameObject gridPrefab;
    public Material gridBaseMat;

    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cubeSize = 5.0f;

    private Vector3 roomTargetRotation;
    public float roomRotationSpeed = 2.0f;
    private ROOM_SIDES currentSide = ROOM_SIDES.FLOOR;
    private bool isRotating = false;

    private bool isDragSelect = false;
    private Vector3 startDrag;
    private RoomGrid activeGrid;
    public LayerMask interactionMask;

    // inicialize map qube
    void Awake () {
        if (!gridPrefab) {
            Debug.LogError("RoomController missing grid prefab");
        }

        float halfRoomSize = cubeSize / 2;

        RoomGrid highlightGrid = highlightController.GetComponent<RoomGrid>();
        highlightGrid.transform.localScale = new Vector3(cubeSize / gridWidth, 1, cubeSize / gridWidth);
        highlightGrid.transform.localPosition = new Vector3(-halfRoomSize, -halfRoomSize + 0.01f, -halfRoomSize);
        highlightGrid.CreateGrid(gridWidth, gridHeight, 0.1f, true);

        // initialize room
        roomSides = new Dictionary<ROOM_SIDES, RoomGrid>();

        // bottom side
        RoomGrid currentGrid = InstantiateGridSide(new Vector3(-halfRoomSize, -halfRoomSize, -halfRoomSize), new Vector3(0, 0, 0), 0.0f, "Floor grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.FLOOR, currentGrid);
        activeGrid = currentGrid;

        // north side
        currentGrid = InstantiateGridSide(new Vector3(-halfRoomSize, halfRoomSize, -halfRoomSize), new Vector3(90, 0, 0), 0.0f, "Wall N grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.WALL_N, currentGrid);

        // south side
        currentGrid = InstantiateGridSide(new Vector3(-halfRoomSize, -halfRoomSize, halfRoomSize), new Vector3(-90, 0, 0), 0.0f, "Wall S grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.WALL_S, currentGrid);

        // west side
        currentGrid = InstantiateGridSide(new Vector3(halfRoomSize, -halfRoomSize, -halfRoomSize), new Vector3(0, 0, 90), 0.0f, "Wall W grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.WALL_W, currentGrid);

        // east side
        currentGrid = InstantiateGridSide(new Vector3(-halfRoomSize, halfRoomSize, -halfRoomSize), new Vector3(0, 0, -90), 0.0f, "Wall E grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.WALL_E, currentGrid);

        // top side
        currentGrid = InstantiateGridSide(new Vector3(halfRoomSize, halfRoomSize, -halfRoomSize), new Vector3(0, 0, 180), 0.0f, "Ceiling grid", gridBaseMat);
        roomSides.Add(ROOM_SIDES.CEILING, currentGrid);

        currentSide = ROOM_SIDES.FLOOR;
    }

    // instantiate grid on given position and return RoomGrid component on new grid
    private RoomGrid InstantiateGridSide(Vector3 position, Vector3 rotation, float spacing, string name, Material gridMat) {
        GameObject gridObj = Instantiate(gridPrefab);
        gridObj.transform.parent = this.transform;
        gridObj.transform.localScale = new Vector3(cubeSize / gridWidth, 1, cubeSize / gridWidth);
        gridObj.transform.localPosition = position;
        gridObj.transform.Rotate(rotation);
        gridObj.name = name;
        gridObj.GetComponent<MeshRenderer>().material = gridMat;

        RoomGrid grid = gridObj.GetComponent<RoomGrid>();
        grid.CreateGrid(gridWidth, gridHeight, spacing);

        return grid;
    }

    public float getCubeSize() {
        return cubeSize;
    }
	
	// Update is called once per frame
	void Update () {
		// testing rotation
        if (Input.GetKeyDown(KeyCode.A)) {
            RotateRoom(currentSide++);
            if (currentSide == ROOM_SIDES.CEILING) {
                currentSide = ROOM_SIDES.FLOOR;
            } 
        }

        if (isRotating) {
            Quaternion targetRotation = Quaternion.Euler(roomTargetRotation);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * roomRotationSpeed);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5) {
                transform.rotation = targetRotation;
                isRotating = false;
                highlightController.EnableInteraction();
            }
        }

        HandleDragSelect();

    }

    private void HandleDragSelect() {
        // start draging event on mouse down
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 50.0f, interactionMask)) {  // 8 = interaction layer
                startDrag = hitInfo.point;
            }
        }

        // update draging on mouse button
        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 50.0f, interactionMask)) {  // 8 = interaction layer
                GridData[] highlightedCells = activeGrid.GetGridCellsBetweenPoints(startDrag, hitInfo.point);
                highlightController.SetActiveCells(highlightedCells);
            }
        }

        // end draging event and proccess
        if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 50.0f, interactionMask)) {  // 8 = interaction layer
                GridData[] highlightedCells = activeGrid.GetGridCellsBetweenPoints(startDrag, hitInfo.point);
                highlightController.SetActiveCells(null);// clear highlight

                // do selected action on selected cells
            }
        }
    }

    public void RotateRoom(ROOM_SIDES targetSide) {
        activeGrid = roomSides[targetSide];
        roomTargetRotation = activeGrid.transform.localRotation.eulerAngles;
        isRotating = true;
        highlightController.DisableInteraction();
        highlightController.SetActiveGrid(activeGrid);
    }
}
