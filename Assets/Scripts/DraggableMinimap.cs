using UnityEngine;
using System.Collections.Generic;

public class DraggableMinimap : MonoBehaviour {
    public Camera minimapCamera;
    // the camera used for viewing the minimap
    Vector3 downPoint;
    Vector3 curPoint;
    // points used to determine how much the player dragged their mouse
    public bool showMinimap = false;
    public GameObject minimap;
    public GameObject roomParent;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            showMinimap = !showMinimap;
        }
        // Vector2 playerCoords = new Vector2(player.transform.position.x, player.transform.position.y);
        // blackScreen.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -1);
        // minimap.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -1);
        // playerIndicator.transform.position = new Vector3(player.transform.position.x + player.transform.position.x / 50f, player.transform.position.y + player.transform.position.y / 50f, -1);
    }

    private void OnMouseDown() {
        if (minimapCamera.enabled) { 
            // if the minimap is on
            downPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            // when the mouse is pressed down, get the current position
        }
    }

    private void OnMouseDrag() {
        if (minimapCamera.enabled) { 
            // if the minimap is on
            curPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            // get the current position of the mouse
            minimapCamera.transform.position = new Vector3((minimapCamera.transform.position.x + (downPoint.x - curPoint.x) / ((1f / minimapCamera.orthographicSize) * 500f)), (minimapCamera.transform.position.y + (downPoint.y - curPoint.y) / ((1f / minimapCamera.orthographicSize) * 500f)), -10f);
            // move the minimap based on player's mouse drag distance relative to the camera's orthographic size
            downPoint = curPoint;
            // reassign the downpoint
        }
    }

    public void Generate() {
        minimap = Instantiate(roomParent, new Vector3(0, 0, -5), Quaternion.identity);
        minimap.name = "minimap";
        // minimap.SetActive(false);
        minimap.transform.localScale = new Vector2(0.02f, 0.02f);
        for (int i = 0; i < minimap.transform.childCount; i++) { 
            Transform room = minimap.transform.GetChild(i);
            room.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
            for (int j = 0; j < room.transform.childCount; j++) {
                Destroy(room.transform.GetChild(j).gameObject);
            }
        }
    }
}
