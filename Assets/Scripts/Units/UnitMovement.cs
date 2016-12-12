using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour {

    Vector3[] currentPath;
    int currentWaypoint = 0;
    bool hasPath = false;

    public float movementSpeed;

    public Animator anim;

    void Start() {
        anim.SetBool("IsWalking", true);
    }

    void Update () {
        // followPath
		if (hasPath) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, currentPath[currentWaypoint], Time.deltaTime);// = direction * movementSpeed;
            Vector3 relativePos = currentPath[currentWaypoint] - transform.localPosition;
            Quaternion rotation = Quaternion.LookRotation(relativePos);
            transform.rotation = rotation;
            if (Vector3.Distance(transform.localPosition, currentPath[currentWaypoint]) < 0.5f) {
                currentWaypoint++;
            }

            if (currentWaypoint >= currentPath.Length) {
                Destroy(this.gameObject);
            }
        }

        if (transform.localPosition.y > 2) {
            Destroy(this.gameObject);
        }
	}

    public void SetUnitPath(Vector3[] path) {
        if (path.Length > 3) {
            hasPath = true;
            currentPath = path;
            currentWaypoint = 1;
            transform.localPosition = path[0];
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        } else {
            Destroy(this.gameObject);
        }
        
        
    }
}
