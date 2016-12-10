using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HightlightControll : MonoBehaviour {
    MeshRenderer meshRenderer;

    private bool isRenderingEnabled = false;

	void Start () {
        // find mesh renderer with material to update with current cursor position
        meshRenderer = GetComponent<MeshRenderer>();
	}
	
	void Update () {
        // find cursor position on grid (camera is expected to look down on the y-axis)
        Vector3 mousePosOnGrid = Camera.main.ViewportToWorldPoint(new Vector3(Input.mousePosition.x / Screen.width,
                                                                              Input.mousePosition.y / Screen.height,
                                                                              Mathf.Abs(transform.position.y - Camera.main.transform.position.y)));
        meshRenderer.material.SetVector("_CursorPos", new Vector4(mousePosOnGrid.x, mousePosOnGrid.y, mousePosOnGrid.z, 0));
    }

    public void EnableRendering() {
        isRenderingEnabled = true;
    }

    public void DisableRendering() {
        isRenderingEnabled = false;
    }
}
