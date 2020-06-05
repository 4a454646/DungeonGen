using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float xMovement;
    [SerializeField] private float yMovement;
    [SerializeField] private List<Sprite> spriteList = new List<Sprite>();
    [SerializeField] private int spriteIndex = 0;
    [SerializeField] private Camera mainCamera;
    private void Start() {
        GetComponent<SpriteRenderer>().sprite = spriteList[spriteIndex];
    }

    private void Update() {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 perpendicular = transform.position - mousePos;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, -perpendicular);
        if (Input.GetKey(KeyCode.W)) { yMovement = moveSpeed; }
        if (Input.GetKeyUp(KeyCode.W)) { yMovement = 0; }
        if (Input.GetKey(KeyCode.A)) { xMovement = -moveSpeed; }
        if (Input.GetKeyUp(KeyCode.A)) { xMovement = 0; }
        if (Input.GetKey(KeyCode.S)) { yMovement = -moveSpeed; }
        if (Input.GetKeyUp(KeyCode.S)) { yMovement = 0; }
        if (Input.GetKey(KeyCode.D)) { xMovement = moveSpeed; }
        if (Input.GetKeyUp(KeyCode.D)) { xMovement = 0; }
    }

    private void FixedUpdate() {
        mainCamera.transform.position = new Vector3(transform.position.x + xMovement, transform.position.y + yMovement, -10f);
        transform.position = new Vector2(transform.position.x + xMovement, transform.position.y + yMovement);
    }
}
