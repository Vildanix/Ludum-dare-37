using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomController : MonoBehaviour {
    public enum ROOM_SIDES {FLOOR, WALL_N, WALL_E, WALL_S, WALL_W, CEILING};
    Dictionary<ROOM_SIDES, RoomGrid> roomSides;
    public HightlightController highlightController;
    public AudioController audioController;

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

    // game logic values
    private bool isGameRunning = false;
    private int currentWords = 0;
    private int wordCapacity = 0;
    private int energyCount = 10;
    private int buildingMaterialPoints = 10;
    private int researchPoints = 0;

    // event timers
    public float tickInterval = 1.0f;       // 1 second tick interval
    public float worldTimerInterval = 15.0f;       // 20 second world event interval
    private float worldEventTimer = 30.0f;
    private float updateTickTimer = 1.0f;
    private int materialTicks = 0;

    // building statistics
    private int gridBuildingsCount = 0;
    private int storageBuildingsCount = 0;
    private int energyBuildingsCount = 0;
    private int scienceBuildingsCount = 0;
    private int energyNodesConnected = 0;

    [Header("BuildingObjects")]
    // building for map setup
    public GenericBuilding dataSource;
    public GenericBuilding energySource;

    // building for player
    public GenericBuilding bus;
    public GenericBuilding storage;
    public GenericBuilding energyStorage;
    public GenericBuilding scienceCenter;

    [Header("MessageWindows")]
    public GameObject beginWindow;
    public GameObject firstResearchWindow;
    public GameObject secondResearchWindow;
    public GameObject victoryWindow;
    public GameObject failedMemoryWindow;
    public GameObject negativeEnergyWindow;

    [Header("Statitics")]
    public Text storageText;
    public Text energyText;
    public Text materialText;
    public Text researchText;
    public Text eventText;
    public Text eventTimerText;

    [Header("GridTabs")]
    public GameObject tab0;
    public GameObject tab1;
    public GameObject tab2;
    public GameObject tab3;
    public GameObject tab4;
    public GameObject tab5;

    public enum CONSTRUCTION_MODE {NONE, BUS, STORAGE, ENERGY_BANK, DESTROY, SCIENCE};
    private CONSTRUCTION_MODE constructionMode = CONSTRUCTION_MODE.NONE;

    private Action nextEvent = null;
    private int researchComplete = 0;

    // inicialize map qube
    void Start () {
        if (!gridPrefab) {
            Debug.LogError("RoomController missing grid prefab");
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
        currentSide = ROOM_SIDES.FLOOR;

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


        // set default state texts
        UpdateWordCapacityStatus();
        UpdateEnergyStatus();
        UpdateBuildingMaterialStatus();
        UpdateResearchProgress();

        // display intro window
        beginWindow.SetActive(true);
        PauseGame();
        tab0.SetActive(false);
        tab1.SetActive(false);
        tab2.SetActive(false);
        tab3.SetActive(false);
        tab4.SetActive(false);
        tab5.SetActive(false);
        negativeEnergyWindow.SetActive(false);

        // plan next event to be word arival
        nextEvent += WordAriveTwo;
        eventText.text = "Two new words arives";
        eventTimerText.text = String.Format("{0:0.} s", worldEventTimer);
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

        // place random starting position for energy node and data input
        grid.PlaceBuildingRandom(dataSource);
        //grid.PlaceBuildingRandom(energySource);

        return grid;
    }

    public float getCubeSize() {
        return cubeSize;
    }

    public void ResumeGame() {
        isGameRunning = true;
    }

    public void RestartGame() {
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void PauseGame() {
        isGameRunning = false;
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

        if (!isGameRunning) {
            return;
        }

        HandleDragSelect();
        HandleConstructionCancel();
        HandleGameTicks();
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
                GridData[] highlightedCells = activeGrid.GetGridCellsBetweenPoints(startDrag, hitInfo.point, true);
                highlightedCells = TrimConstructionArrayWithAvalibleMaterial(highlightedCells);
                highlightController.SetActiveCells(highlightedCells);
            }
        }
        // end draging event and proccess
        if ((Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0)) && isDragSelect) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 50.0f, interactionMask)) {  // 8 = interaction layer
                GridData[] highlightedCells = activeGrid.GetGridCellsBetweenPoints(startDrag, hitInfo.point, true);
                highlightedCells = TrimConstructionArrayWithAvalibleMaterial(highlightedCells);
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

    private void HandleGameTicks() {
        updateTickTimer -= Time.deltaTime;
        worldEventTimer -= Time.deltaTime;

        eventTimerText.text = String.Format("{0:0.} s", worldEventTimer);

        float multiplier = 1.0f;
        if (energyCount < 0) {
            multiplier = 2.0f;
        }
        if (energyCount < -10) {
            multiplier = 4.0f;
        }

        // reset tick timer
        if (updateTickTimer < 0) {
            updateTickTimer += tickInterval * multiplier;
            ProccessBuildings();
        }

        // reset event timer
        if (worldEventTimer < 0) {
            worldEventTimer += worldTimerInterval;
            DispatchCurrentEvent();
            GenerateNewEvent();
        }
    }

    public void RotateRoom(ROOM_SIDES targetSide) {
        if (!isGameRunning) {
            return;
        }
        if (targetSide == currentSide)
            return;

        currentSide = targetSide;
        activeGrid = roomSides[targetSide];
        roomTargetRotation = -activeGrid.transform.localRotation.eulerAngles;
        isRotating = true;
        highlightController.DisableInteraction();
        highlightController.SetActiveGrid(activeGrid);

        audioController.PlayRotateSound();
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
        if (!isGameRunning) {
            return;
        }
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
        bool success = false;
        int successfullPlacements = 0;
        foreach (GridData cell in gridCells) {
            switch (constructionMode) {
                case CONSTRUCTION_MODE.BUS:
                    if (cell.ConstructBuilding(bus)) {
                        successfullPlacements++;
                        gridBuildingsCount++;
                        AddWordCapacity(bus.PopulationCapacity);
                        AddEnergy(bus.Energy);
                        success = true;
                    }
                    break;
                case CONSTRUCTION_MODE.STORAGE:
                    if (cell.ConstructBuilding(storage)) {
                        successfullPlacements++;
                        storageBuildingsCount++;
                        AddWordCapacity(storage.PopulationCapacity);
                        AddEnergy(storage.Energy);
                        success = true;
                    }
                    break;
                case CONSTRUCTION_MODE.ENERGY_BANK:
                    if (cell.ConstructBuilding(energyStorage)) {
                        successfullPlacements++;
                        energyBuildingsCount++;
                        AddWordCapacity(energyStorage.PopulationCapacity);
                        AddEnergy(energyStorage.Energy);
                        success = true;
                    }
                    break;
                case CONSTRUCTION_MODE.SCIENCE:
                    if (cell.ConstructBuilding(scienceCenter)) {
                        successfullPlacements++;
                        scienceBuildingsCount++;
                        AddWordCapacity(scienceCenter.PopulationCapacity);
                        AddEnergy(scienceCenter.Energy);
                        success = true;
                    }
                    break;
            }
        }

        if (success) {
            audioController.PlayPlaceSound();

            // consume building material for each builded cell
            ConsumeBuildingMaterial(successfullPlacements);
        }
    }

    private GridData[] TrimConstructionArrayWithAvalibleMaterial(GridData[] gridCells) {
        if (gridCells.Length > buildingMaterialPoints) {
            GridData[] newGrid = new GridData[buildingMaterialPoints];
            // copy avalible cells to shorter array
            for (int i = 0; i < buildingMaterialPoints; i++) {
                newGrid[i] = gridCells[i];
            }

            return newGrid;
        }

        return gridCells;
    }

    //////////////////////////////////////////////////
    // events and intervals
    //////////////////////////////////////////////////
    private void DispatchCurrentEvent() {
        nextEvent();

        // remove action
        foreach (Delegate d in nextEvent.GetInvocationList()) {
            nextEvent -= (Action)d;
        }
    }

    private void GenerateNewEvent() {
        int eventNum = Mathf.FloorToInt(UnityEngine.Random.value * 10);

        switch (eventNum) {
            case 0:
                nextEvent += WordAriveOne;
                eventText.text = "One new word arive";
                break;
            case 1:
                nextEvent += WordAriveTwo;
                eventText.text = "Two new words arives";
                break;
            case 2:
                nextEvent += WordAriveThree;
                eventText.text = "Three new words arives";
                break;
            case 3:
                nextEvent += EnergyDischarge;
                eventText.text = "Energy loss";
                break;
            case 4:
                nextEvent += EnergyBoost;
                eventText.text = "Energy boost";
                break;
            case 5:
                nextEvent += AdditionalMaterial;
                eventText.text = "Additional material";
                break;
            case 6:
                nextEvent += WordAriveFour;
                eventText.text = "Four new words arives";
                break;
            case 7:
                nextEvent += WordAriveFour;
                eventText.text = "Four new words arives";
                break;
            case 8:
                nextEvent += EnergyDischarge;
                eventText.text = "Energy loss";
                break;
            case 9:
                nextEvent += WordAriveFour;
                eventText.text = "Four new words arives";
                break;
            default:
                nextEvent += WordAriveTwo;
                eventText.text = "One new word arive";
                break;
        }
    }

    private void ProccessBuildings() {
        materialTicks++;
        Debug.Log("Tick");

        if (materialTicks >= 3) {
            // add material proportionly to free words
            int avalibleWords = Mathf.Max(currentWords - scienceBuildingsCount, 0);
            AddBuildingMaterial(Mathf.CeilToInt(avalibleWords / 2) + 1);     // each free word give half material unit each cycle
            materialTicks = 0;
        }

        int scienceGrow = Mathf.CeilToInt(Mathf.Min(scienceBuildingsCount, currentWords) / (2 + researchComplete * 2));
        AddResearchPoints(scienceGrow);
    }

    private void WordAriveOne() {
        AddWords(1);
    }

    private void WordAriveTwo() {
        AddWords(2);
    }

    private void WordAriveThree() {
        AddWords(3);
    }

    private void WordAriveFour() {
        AddWords(4);
    }

    private void EnergyDischarge() {
        AddEnergy(-6);
    }

    private void EnergyBoost() {
        AddEnergy(3);
    }

    private void AdditionalMaterial() {
        AddBuildingMaterial(3);
    }




    //////////////////////////////////////////////////
    // message status texts
    //////////////////////////////////////////////////

    private void UpdateWordCapacityStatus() {
        storageText.text = currentWords + " / " + wordCapacity + " Words";
    }

    private void UpdateEnergyStatus() {
        energyText.text = energyCount + " Units";

        if (energyCount < 0) {
            negativeEnergyWindow.SetActive(true);
        } else {
            negativeEnergyWindow.SetActive(false);
        }
    }

    private void UpdateBuildingMaterialStatus() {
        materialText.text = buildingMaterialPoints + " Units";
    }

    private void UpdateResearchProgress() {
        researchText.text = researchPoints + " %";
    }

    //////////////////////////////////////////////////
    // message window triggers
    //////////////////////////////////////////////////

    private void TriggerMemoryCorruptionEnd() {
        PauseGame();
        failedMemoryWindow.SetActive(true);
    }

    private void TriggerAIVictoryEnd() {
        PauseGame();
        victoryWindow.SetActive(true);
    }

    private void TriggerFirstResearch() {
        PauseGame();
        firstResearchWindow.SetActive(true);
        tab0.SetActive(true);
        tab1.SetActive(true);
        tab2.SetActive(true);
    }

    private void TriggerSeconfResearch() {
        PauseGame();
        secondResearchWindow.SetActive(true);
        tab3.SetActive(true);
        tab4.SetActive(true);
        tab5.SetActive(true);
    }

    ///////////////////////////////////////////////////
    // Game logic methods
    ///////////////////////////////////////////////////

    /// <summary>
    /// Add new avalible capacity for words to store
    /// </summary>
    /// <param name="capacity">New capacity</param>
    public void AddWordCapacity(int capacity) {
        wordCapacity += capacity;
        UpdateWordCapacityStatus();
    }

    public void AddWords(int wordsCount) {
        currentWords += wordsCount;

        // end game / memory corruption
        if (currentWords > wordCapacity) {
            TriggerMemoryCorruptionEnd();
        }

        UpdateWordCapacityStatus();
    }

    public void AddEnergy(int newEnergy) {
        energyCount += newEnergy;

        UpdateEnergyStatus();
    }

    public void ConsumeEnergy(int energy) {
        energyCount -= energy;

        UpdateEnergyStatus();
    }

    public void AddBuildingMaterial(int newMaterial) {
        buildingMaterialPoints += newMaterial;

        UpdateBuildingMaterialStatus();
    }

    public void ConsumeBuildingMaterial(int materialCost) {
        buildingMaterialPoints -= materialCost;
        if (buildingMaterialPoints < 0) {
            buildingMaterialPoints = 0;
        }

        UpdateBuildingMaterialStatus();
    }

    public void AddResearchPoints(int points) {
        researchPoints += points;

        if (researchPoints >= 100) {
            researchComplete++;
            worldTimerInterval -= 1;
            if (worldTimerInterval < 3) {
                worldTimerInterval = 3.0f;
            }

            switch (researchComplete) {
                case 1:
                    TriggerFirstResearch();
                    break;
                case 2:
                    TriggerSeconfResearch();
                    break;
            }
            researchPoints = 0;
        }
        UpdateResearchProgress();
    }

    public void RemoveResearchPoints(int points) {
        researchPoints -= points;
        if (researchPoints < 0) {
            researchPoints = 0;
        }
        UpdateResearchProgress();
    }
}
