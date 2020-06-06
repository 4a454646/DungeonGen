using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour {
    public Vector2 truePos;
    public Vector2 pos01;
    public string needsEntranceAt;
    public bool isExplored;
    [SerializeField] private List<GameObject> wallColliders = new List<GameObject>();
    [SerializeField] private List<GameObject> extraColliders = new List<GameObject>();
    [SerializeField] private bool lockIn = false;
    private Vector2 zeroZero = new Vector2(0, 0);
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.name == "Player") {
            if (lockIn) {
                foreach (GameObject collider in wallColliders) {
                    GameObject created = Instantiate(collider, zeroZero, Quaternion.identity);
                    created.transform.parent = transform;
                    created.transform.position = zeroZero;
                    extraColliders.Add(created);
                }
            }
            if (!isExplored) {
                FindObjectOfType<Player>().UpdateSprite();
            }
            isExplored = true;
        }
    }
}
