using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HightlightController : MonoBehaviour {
    MeshRenderer meshRenderer;
    RoomGrid highlightGrid;
    public float overlayRestoreTime = 0.5f;
    private float overlayTimer = 0;

    //private RoomGrid activeGrid;
    private GridData[] highlightedCells;

    private bool isInteractionEnabled = true;

	void Awake () {
        // find mesh renderer with material to update with current cursor position
        meshRenderer = GetComponent<MeshRenderer>();
        highlightGrid = GetComponent<RoomGrid>();
	}

    void Update() {
        // update overlay timer
        if (isInteractionEnabled && overlayTimer < overlayRestoreTime) {
            overlayTimer += Time.deltaTime;
        }

        // find cursor position on grid (camera is expected to look down on the y-axis)
        Vector3 mousePosOnGrid = Camera.main.ViewportToWorldPoint(new Vector3(Input.mousePosition.x / Screen.width,
                                                                              Input.mousePosition.y / Screen.height,
                                                                              Mathf.Abs(transform.position.y - Camera.main.transform.position.y)));
        meshRenderer.material.SetVector("_CursorPos", new Vector4(mousePosOnGrid.x, mousePosOnGrid.y, mousePosOnGrid.z, 0));
        meshRenderer.material.SetFloat("_Overlay", overlayTimer / overlayRestoreTime);
    }

    public void EnableInteraction() {
        isInteractionEnabled = true;
        meshRenderer.enabled = true;
        overlayTimer = 0;
    }

    public void DisableInteraction() {
        isInteractionEnabled = false;
        meshRenderer.enabled = false;
        overlayTimer = 0;
    }

    // set highlighter reference to new active grid
    public void SetActiveGrid(RoomGrid grid) {
        //activeGrid = grid;

        // rebuild highlight for new grid
        GridData[] gridData = grid.GetGridData();

        foreach (GridData gridCell in gridData) {
           
        }
    }

    public void SetActiveCells(GridData[] cells) {

        // restore default highlight for old cells
        if (highlightedCells != null) {
            foreach (GridData gridCell in highlightedCells) {
                SetCellColorFromData(gridCell);
            }
        }


        // create new highlight
        if (cells != null) {
            foreach (GridData gridCell in cells) {
                if (gridCell.GetGridState() == GridData.GRID_STATE.AVALIBLE) {
                    highlightGrid.SetCellColor(gridCell.PosX, gridCell.PosY, Color.green);
                } else if (gridCell.GetGridState() == GridData.GRID_STATE.BLOCKED) {
                    highlightGrid.SetCellColor(gridCell.PosX, gridCell.PosY, Color.yellow);
                }
            }
        }

        // save active cells for cleanup later
        highlightedCells = cells;
    }

    // set default color state for grid cell with given coordinates containes
    private void SetCellColorFromData(GridData cell) {
        switch (cell.GetGridState()) {
            case GridData.GRID_STATE.AVALIBLE:
                highlightGrid.SetCellColor(cell.PosX, cell.PosY, Color.black);
                break;
            case GridData.GRID_STATE.BLOCKED:
                highlightGrid.SetCellColor(cell.PosX, cell.PosY, Color.yellow);
                break;
            case GridData.GRID_STATE.BUILDED:
                highlightGrid.SetCellColor(cell.PosX, cell.PosY, Color.red);
                break;
        }
    }
}
