using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomController : MonoBehaviour {
    public enum ROOM_SIDES {FLOOR, WALL_N, WALL_E, WALL_S, WALL_W, CEILING};
    Dictionary<ROOM_SIDES, RoomGrid> roomSides;
    public HightlightController highlightController;
    public GridController gridController;

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

    [Header("BuildingObjects")]
    // building for map setup
    public GenericBuilding dataSource;
    public GenericBuilding energySource;

    // building for player
    public GenericBuilding bus;
    public GenericBuilding storage;
    public GenericBuilding energyStorage;
    public GenericBuilding scienceCenter;

    public enum CONSTRUCTION_MODE {NONE, BUS, STORAGE, ENERGY_BANK, DESTROY, SCIENCE};
    private CONSTRUCTION_MODE constructionMode = CONSTRUCTION_MODE.NONE;

    // inicialize map qube
    void Start () {
        if (!gridPrefab) {
            Debug.LogError("RoomController missing grid prefab");
        }

        if (!gridController) {
            gridController = GetComponent<GridController>();
        }

        float halfRoomSize = cubeSize / 2;

        RoomGrid highlightGrid = highlightController.GetComponent<RoomGrid>();
        highlightGrid.transform.localScale = new Vector3(cubeSize / gridWidth, cubeSize / gridWidth, cubeSize / gridWidth);
        highlightGrid.transform.localPosition = new Vector3(-halfRoomSize, -halfRoomSize + 0.01f, -halfRoomSize);
        highlightGrid.CreateGrid(gridWidth, gridHeight, 0.1f, true);
        highlightController.DisableInteraction();

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
        gridObj.transform.localScale = new Vector3(cubeSize / gridWidth, cubeSize / gridWidth, cubeSize / gridWidth);
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

        if (isRotating) {
            Quaternion targetRotation = Quaternion.Euler(roomTargetRotation);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * roomRotationSpeed);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5) {
                transform.rotation = targetRotation;
                isRotating = false;
                EnableConstructionHighlight();
            }
        }

        HandleDragSelect();
        HandleConstructionCancel();
    }

    private void HandleDragSelect() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            isDragSelect = false;
            highlightController.SetActiveCells(null);// clear highlight
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject()) {
            highlightController.SetActiveCells(null);// clear highlight
            return;
        }


        // start draging event on mouse down
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 50.0f, interactionMask)) {  // 8 = interaction layer
                startDrag = hitInfo.point;
                isDragSelect = true;
            }
        }

        // update draging on mouse button
        if (Input.GetMouseButton(0) && isDragSelect) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 50.0f, interactionMask)) {  // 8 = interaction layer
                GridData[] highlightedCells = activeGrid.GetGridCellsBetweenPoints(startDrag, hitInfo.point);
                highlightController.SetActiveCells(highlightedCells);
            }
        }
        // end draging event and proccess
        if ((Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0)) && isDragSelect) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 50.0f, interactionMask)) {  // 8 = interaction layer
                GridData[] highlightedCells = activeGrid.GetGridCellsBetweenPoints(startDrag, hitInfo.point);
                highlightController.SetActiveCells(null);// clear highlight
                startDrag = Vector3.zero;
                isDragSelect = false;
                // do selected action on selected cells
                ConstructOnGridCells(highlightedCells);
            }
        }
    }

    private void HandleConstructionCancel() {
        // cancel construction when escape or right mouse button is pressed
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) {
            constructionMode = CONSTRUCTION_MODE.NONE;
            highlightController.DisableInteraction();
        }   
    }

    public void RotateRoom(ROOM_SIDES targetSide) {
        activeGrid = roomSides[targetSide];
        roomTargetRotation = -activeGrid.transform.localRotation.eulerAngles;
        isRotating = true;
        highlightController.DisableInteraction();
        highlightController.SetActiveGrid(activeGrid);
    }

    public void RotateRoomByNum(int gridNum) {
        switch (gridNum) {
            case 0:
                RotateRoom(ROOM_SIDES.FLOOR);
                break;
            case 1:
                RotateRoom(ROOM_SIDES.WALL_N);
                break;
            case 2:
                RotateRoom(ROOM_SIDES.WALL_E);
                break;
            case 3:
                RotateRoom(ROOM_SIDES.WALL_S);
                break;
            case 4:
                RotateRoom(ROOM_SIDES.WALL_W);
                break;
            case 5:
                RotateRoom(ROOM_SIDES.CEILING);
                break;
        }
    }

    public void SetConstructionMode(CONSTRUCTION_MODE mode) {
        constructionMode = mode;

        EnableConstructionHighlight();
    }

    public void SetConstructionModeByNum(int modeNum) {
        switch (modeNum) {
            case 0:
                SetConstructionMode(CONSTRUCTION_MODE.NONE);
                break;
            case 1:
                SetConstructionMode(CONSTRUCTION_MODE.BUS);
                break;
            case 2:
                SetConstructionMode(CONSTRUCTION_MODE.STORAGE);
                break;
            case 3:
                SetConstructionMode(CONSTRUCTION_MODE.ENERGY_BANK);
                break;
            case 4:
                SetConstructionMode(CONSTRUCTION_MODE.DESTROY);
                break;
            case 5:
                SetConstructionMode(CONSTRUCTION_MODE.SCIENCE);
                break;
        }
    }

    private void EnableConstructionHighlight() {
        if (constructionMode != CONSTRUCTION_MODE.NONE) {
            highlightController.EnableInteraction();
        }
    }

    private void ConstructOnGridCells(GridData[] gridCells) {
        foreach (GridData cell in gridCells) {
            switch (constructionMode) {
                case CONSTRUCTION_MODE.BUS:
                    cell.ConstructBuilding(bus);
                    break;
                case CONSTRUCTION_MODE.STORAGE:
                    cell.ConstructBuilding(storage);
                    break;
                case CONSTRUCTION_MODE.ENERGY_BANK:
                    cell.ConstructBuilding(energyStorage);
                    break;
                case CONSTRUCTION_MODE.SCIENCE:
                    cell.ConstructBuilding(scienceCenter);
                    break;
            }
        } 
    }
}
