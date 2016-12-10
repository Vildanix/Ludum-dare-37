using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RoomController : MonoBehaviour {
    enum ROOM_SIDES {FLOOR, WALL_N, WALL_E, WALL_S, WALL_W, CEILING};
    Dictionary<ROOM_SIDES, RoomGrid> roomSides;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    void Awake () {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        // initialize room
        roomSides = new Dictionary<ROOM_SIDES, RoomGrid>();

        RoomGrid currentGrid = new RoomGrid();
        currentGrid.CreateGrid(20, 20);
        meshFilter.mesh = currentGrid.getGridMesh();
        roomSides.Add(ROOM_SIDES.FLOOR, currentGrid);
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
