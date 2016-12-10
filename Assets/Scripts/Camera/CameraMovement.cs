using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    private Vector3 lowerLeftBoundary;
    private Vector3 upperRightBoundary;
    private float cubeSize;
    private Resolution screenResolution;

    public float cameraSpeed = 2.5f;
    public float boundaryMarginHorizontal = 3.0f;
    public float boundaryMarginVertical = 2.0f;

    public RoomController roomController;

    // Use this for initialization
    void Start() {
        // get viewpoer boundaries to prevent movement camera outside cube (camera is looking down on Y axis and is positioned in center of the cube)
        cubeSize = roomController.getCubeSize();
        CalculateBound();
    }

    // Update is called once per frame
    void Update() {
        // detect windows resize
        if (Screen.currentResolution.width != screenResolution.width || Screen.currentResolution.height != screenResolution.height) {
            CalculateBound();
        }

        Vector3 newPosition = transform.position;
        newPosition.x += Input.GetAxis("Horizontal") * cameraSpeed * Time.deltaTime;
        newPosition.z += Input.GetAxis("Vertical") * cameraSpeed * Time.deltaTime;

        // clamp camera movement inside cube
        // allow movement in x-axis if camera don't see all tiles on the left and right side
        if (upperRightBoundary.x - lowerLeftBoundary.x < cubeSize + boundaryMarginHorizontal) {
            newPosition.x = Mathf.Clamp(newPosition.x,
                                        -(cubeSize / 2) - lowerLeftBoundary.x - boundaryMarginHorizontal / 2,
                                        (cubeSize / 2) - upperRightBoundary.x + boundaryMarginHorizontal / 2);
        } else {
            newPosition.x = 0; // lock horizontal movement
        }

        // allow movement in z-axis if camera don't see all tiles on top or bottom
        if (upperRightBoundary.z - lowerLeftBoundary.z < cubeSize + boundaryMarginVertical) {
            newPosition.z = Mathf.Clamp(newPosition.z,
                                        -(cubeSize / 2) - lowerLeftBoundary.z - boundaryMarginVertical / 2,
                                        (cubeSize / 2) - upperRightBoundary.z + boundaryMarginVertical / 2);
        } else {
            newPosition.z = 0; // lock vertical movement
        }

        transform.position = newPosition;
    }

    private void CalculateBound() {
        lowerLeftBoundary = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, cubeSize / 2));
        upperRightBoundary = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, cubeSize / 2));

        screenResolution = Screen.currentResolution;
    }
}
