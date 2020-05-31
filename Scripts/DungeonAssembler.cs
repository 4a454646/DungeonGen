using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonAssembler : MonoBehaviour {
    [SerializeField] private List<GameObject> colliderList = new List<GameObject>();
    // list of colliders to attach to a room
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite northEntranceClosing;
    [SerializeField] private Sprite eastEntranceClosing;
    [SerializeField] private Sprite southEntranceClosing;
    [SerializeField] private Sprite westEntranceClosing;
    [SerializeField] private List<Sprite> northSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> eastSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> southSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> westSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> allSprites = new List<Sprite>();
    // lists of sprites for rooms to use
    [SerializeField] private GameObject roomPrefab;
    // gameobject to instantiate
    [SerializeField] private float roomOffset;
    // how much to offset each room by
    [SerializeField] private int curRooms;
    // # of rooms that currently exist
    [SerializeField] private int desiredRooms;
    // the # of rooms to create
    [SerializeField] private List<Vector2> roomPositions = new List<Vector2>();
    // list of positions of chunks
    [SerializeField] private List<GameObject> createdRooms = new List<GameObject>();
    // list of created chunks
    private Dictionary<string, string> oppositeOf = new Dictionary<string, string>() {
        {"n", "s"},
        {"e", "w"},
        {"s", "n"},
        {"w", "e"}
    };
    private WaitForSeconds quickDelay = new WaitForSeconds(0.05f);

    private void Start() {
        curRooms = 0;
        StartCoroutine(CreateRoom(0, 0, "starter"));
        StartCoroutine(FixRooms());
    }

    /// <summary>
    /// Create a room at the designated location, and use recursion to create all nearby rooms.
    /// </summary>
    /// <param name="xPos">The x position to create the room at.</param>
    /// <param name="yPos">The y position to create the room at.</param>
    /// <param name="roomNeedsEntranceAt">Which cardinal direction the room must connect to.</param>
    private IEnumerator CreateRoom(int xPos, int yPos, string roomNeedsEntranceAt) {
        yield return quickDelay;
        if (curRooms >= desiredRooms) { yield break; }
        // instantly break out if we have exceeded the max #
        // test the position.
        Vector2 curPos = new Vector2(xPos, yPos);
        if (roomPositions.Contains(curPos)) { yield break; }
        // if the position is already created, break out
        GameObject created = Instantiate(roomPrefab, new Vector3(xPos * roomOffset, yPos * roomOffset, 0), Quaternion.identity);
        // create a gameobject from the prefab at the designated location.
        int rand = Random.Range(0, 9);
        // create a random number, 0-6
        // based on the needed room, assign the sprite
        GenerateSprite(created, roomNeedsEntranceAt, rand, curRooms, desiredRooms);
        string spriteName = created.GetComponent<SpriteRenderer>().sprite.name;
        // get the name of the sprite, e.g. 'ew' for east and west entrances
        if (spriteName.Contains("n")) {
            ParentColliderTo("north path collider", created);
            StartCoroutine(CreateRoom(xPos, yPos + 1, "s"));
        }
        else { ParentColliderTo("north wall collider", created); }
        if (spriteName.Contains("e")) {
            ParentColliderTo("east path collider", created);
            StartCoroutine(CreateRoom(xPos + 1, yPos, "w"));
        }
        else { ParentColliderTo("east wall collider", created); }
        if (spriteName.Contains("s")) {
            ParentColliderTo("south path collider", created);
            StartCoroutine(CreateRoom(xPos, yPos - 1, "n"));
        }
        else { ParentColliderTo("south wall collider", created); }
        if (spriteName.Contains("w")) {
            ParentColliderTo("west path collider", created);
            StartCoroutine(CreateRoom(xPos - 1, yPos, "e"));
        }
        else { ParentColliderTo("west wall collider", created); }
        // based on the sprite name, create the needed rooms to connect to it
        yield return quickDelay;
        roomPositions.Add(curPos);
        createdRooms.Add(created);
        curRooms++;
    }

    private IEnumerator FixRooms() {
        while (curRooms < desiredRooms - 5) {
            yield return quickDelay;
        }
        yield return quickDelay;
        yield return quickDelay;
        yield return quickDelay;
        yield return quickDelay;
        yield return quickDelay;
        yield return quickDelay;
        yield return quickDelay;
        yield return quickDelay;
        yield return quickDelay;
        for (int i = 0; i < createdRooms.Count; i++) {
            string unwantedEntrances = "";
            GameObject curRoom = createdRooms[i];
            Vector2 checkPosition;
            string spriteName = curRoom.GetComponent<SpriteRenderer>().sprite.name;
            if (spriteName.Contains("n")) {
                checkPosition = new Vector2(curRoom.transform.position.x, curRoom.transform.position.y + 1);
                if (roomPositions.Contains(checkPosition)) {
                    if(!createdRooms[roomPositions.IndexOf(checkPosition)].GetComponent<SpriteRenderer>().sprite.name.Contains("s")) {
                        // no south entrance to match the north one
                        unwantedEntrances += "n";
                    }
                }
                else {
                    unwantedEntrances += "n";
                }
            }
            if (spriteName.Contains("e")) {
                checkPosition = new Vector2(curRoom.transform.position.x + 1, curRoom.transform.position.y);
                if (roomPositions.Contains(checkPosition)) {
                    if(!createdRooms[roomPositions.IndexOf(checkPosition)].GetComponent<SpriteRenderer>().sprite.name.Contains("w")) {
                        unwantedEntrances += "e";
                    }
                }
                else {
                    unwantedEntrances += "e";
                }
            }
            if (spriteName.Contains("s")) {
                checkPosition = new Vector2(curRoom.transform.position.x, curRoom.transform.position.y - 1);
                if (roomPositions.Contains(checkPosition)) {
                    if(!createdRooms[roomPositions.IndexOf(checkPosition)].GetComponent<SpriteRenderer>().sprite.name.Contains("n")) {
                        unwantedEntrances += "s";
                    }
                }
                else {
                    unwantedEntrances += "s";
                }
            }
            if (spriteName.Contains("w")) {
                checkPosition = new Vector2(curRoom.transform.position.x - 1, curRoom.transform.position.y);
                if (roomPositions.Contains(checkPosition)) {
                    if(!createdRooms[roomPositions.IndexOf(checkPosition)].GetComponent<SpriteRenderer>().sprite.name.Contains("e")) {
                        unwantedEntrances += "w";
                    }
                }
                else {
                    unwantedEntrances += "w";
                }
            }
            string wantedEntrances = spriteName;
            for (int k = 0; k < unwantedEntrances.Length; k++) {
               wantedEntrances = string.Join("", wantedEntrances.Split(unwantedEntrances[k]));
            }
            print($"wantedentrances: {wantedEntrances}");
            print($"is containedin allsprites? {(from sprite in allSprites select sprite.name).ToList().Contains(wantedEntrances)}");
            if (wantedEntrances == "") {
                Debug.LogError($"big error at {curRoom.transform.position.x}, {curRoom.transform.position.y}");
            }
            else {
                curRoom.GetComponent<SpriteRenderer>().sprite = allSprites[(from sprite in allSprites select sprite.name).ToList().IndexOf(wantedEntrances)];
            }
        }
    }

    /// <summary>
    /// Child a collider with a given name to the given gameobject.
    /// </summary>
    /// <param name="colliderName">The name of the collider to match to.</param>
    /// <param name="parentTo">The gameobject to chil the collider to.</param>
    private void ParentColliderTo(string colliderName, GameObject parentTo) {
        GameObject collider = Instantiate(colliderList[(from col in colliderList select col.name).ToList().IndexOf(colliderName)], new Vector3(0, 0, 0), Quaternion.identity);
        // get the collider based on its name
        collider.transform.parent = parentTo.transform;
        // child the collider to the gameobject
        collider.transform.localPosition = new Vector2(0, 0);
        // set the collider's localposition to be normal
    }

    private void GenerateSprite(GameObject created, string roomNeedsEntranceAt, int rand, int curRooms, int desiredRooms) {
        if (roomNeedsEntranceAt == "starter") {
            // if creating a starting room
            created.GetComponent<SpriteRenderer>().sprite = startSprite;
            created.GetComponent<SpriteRenderer>().color = Color.green;
            created.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // set the sprite, make it green, make it go forwards
        }
        else if (roomNeedsEntranceAt == "n") { 
            if (curRooms + Mathf.RoundToInt(desiredRooms / 10) + 1 < desiredRooms) {
                created.GetComponent<SpriteRenderer>().sprite = northSprites[rand]; 
            }
            else {
                created.GetComponent<SpriteRenderer>().sprite = northEntranceClosing;
            }
        }
        else if (roomNeedsEntranceAt == "e") { 
            if (curRooms + Mathf.RoundToInt(desiredRooms / 10) + 1 < desiredRooms) {
                created.GetComponent<SpriteRenderer>().sprite = eastSprites[rand]; 
            }
            else {
                created.GetComponent<SpriteRenderer>().sprite = eastEntranceClosing;
            }
        }
        else if (roomNeedsEntranceAt == "s") { 
            if (curRooms + Mathf.RoundToInt(desiredRooms / 10) + 1 < desiredRooms) {
                created.GetComponent<SpriteRenderer>().sprite = southSprites[rand]; 
            }
            else {
                created.GetComponent<SpriteRenderer>().sprite = southEntranceClosing;
            }
        }
        else if (roomNeedsEntranceAt == "w") { 
            if (curRooms + Mathf.RoundToInt(desiredRooms / 10) + 1 < desiredRooms) {
                created.GetComponent<SpriteRenderer>().sprite = westSprites[rand]; 
            }
            else {
                created.GetComponent<SpriteRenderer>().sprite = westEntranceClosing;
            }
        }
        else { print("big error"); }
    }
}
