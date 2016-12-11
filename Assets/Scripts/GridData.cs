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

    public GridData() {
        gridState = GRID_STATE.AVALIBLE;
    }

    public GridData(int x, int y) {
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

    public void ConstructBuilding(GenericBuilding newBuilding) {
        gridBuilding = newBuilding;
        notifications(gridState, this);
    }

    public GRID_STATE GetGridState() {
        return gridState;
    }

    public GenericBuilding getCurrentBuilding() {
        return gridBuilding;
    }


}
