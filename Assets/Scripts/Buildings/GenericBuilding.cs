using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBuilding : MonoBehaviour{
    public enum BUILDING_TYPE { BUS, MEMORY, SCIENCE, ENERGY_STORAGE, ENERGY_NODE, DATA_NODE}

    [SerializeField]
    private BUILDING_TYPE buildingType;

    [SerializeField]
    private string buildingName;

    [SerializeField]
    [Range(-20, 100)]
    private int energy;

    [SerializeField]
    [Range(0, 20)]
    private int capacity;

    [SerializeField]
    private bool isGridConnection = false;

    private int posX;
    private int posY;

    public string BuildingName {
        get {
            return buildingName;
        }

        set {
            buildingName = value;
        }
    }

    public int Energy {
        get {
            return energy;
        }

        set {
            energy = value;
        }
    }

    public int PopulationCapacity {
        get {
            return capacity;
        }

        set {
            capacity = value;
        }
    }

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

    public BUILDING_TYPE BuildingType {
        get {
            return buildingType;
        }

        set {
            buildingType = value;
        }
    }

    public bool IsGridConnection {
        get {
            return isGridConnection;
        }

        set {
            isGridConnection = value;
        }
    }
}
