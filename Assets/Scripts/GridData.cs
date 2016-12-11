using System;
using System.Collections.Generic;
using UnityEngine;

public class GridData {
    public enum GRID_STATE {AVALIBLE, BUILDED, BLOCKED};
    private GRID_STATE gridState;
    private GenericBuilding gridBuilding;
    private Action<GRID_STATE, GridData> notifications;
    private int posX;
    private int posY;
    private RoomGrid parentGrid;

    public int PosX {
        get {
            return posX;
        }

        set {
            posX = value;
        }
    }

    public int PosY {
        get {
            return posY;
        }

        set {
            posY = value;
        }
    }

    public GridData(RoomGrid parentGrid) {
        this.parentGrid = parentGrid;
        gridState = GRID_STATE.AVALIBLE;
    }

    public GridData(int x, int y, RoomGrid parentGrid) {
        this.parentGrid = parentGrid;
        gridState = GRID_STATE.AVALIBLE;
        posX = x;
        posY = y;
    }

    public void AddStateChangeListener (Action<GRID_STATE, GridData> stateChangeCallback) {
        notifications += stateChangeCallback;
    }

    public void RemoveListener(Action<GRID_STATE, GridData> stateChangeCallback) {
        notifications -= stateChangeCallback;
    }

    public bool ConstructBuilding(GenericBuilding newBuilding, bool isGenerated = false) {
        if (gridState != GRID_STATE.AVALIBLE) {
            return false;
        }

        GridData[] neighbors = parentGrid.GetGridNeighbors(posX, posY);

        // check if at least one neighbor have grid connect
        bool isNeigborGrid = false;
        foreach (GridData cell in neighbors) {
            if(cell.IsGridConnection) {
                isNeigborGrid = true;
            }
        }

        // return fail if new construction doesnt neighbor with active grid
        if (!isNeigborGrid && !isGenerated) {
            return false;
        }

        gridBuilding = parentGrid.InstantiateBuildingOnGrid(newBuilding, posX, posY);
        if (isGenerated) {
            gridState = GRID_STATE.BLOCKED;
        } else {
            gridState = GRID_STATE.BUILDED;
        }
            

        if (notifications != null) {
            notifications(gridState, this);
        }

        return true;
    }

    public void DestroyBuilding() {
        GameObject.Destroy(gridBuilding);
        gridState = GRID_STATE.AVALIBLE;
    }

    public GRID_STATE GetGridState() {
        return gridState;
    }

    public GenericBuilding getCurrentBuilding() {
        return gridBuilding;
    }

    public bool IsAvalible {
        get { return gridState == GRID_STATE.AVALIBLE; }
    }

    public bool IsGridConnection {
        get {
            if (gridBuilding != null) {
                return gridBuilding.IsGridConnection;
            }
            return false;
        }
    }
}
