using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBuilding {
    private string buildingName;
    private int energy;
    private int requiredPopulation;
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

    public int RequiredPopulation {
        get {
            return requiredPopulation;
        }

        set {
            requiredPopulation = value;
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
}
