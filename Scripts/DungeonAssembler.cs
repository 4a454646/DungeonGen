using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonAssembler : MonoBehaviour {
    [SerializeField] private List<GameObject> colliderList = new List<GameObject>();
    // list of colliders to attach to a room
    [SerializeField] private Sprite startSprite;
    // nesw sprite
    [SerializeField] private List<Sprite> northSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> eastSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> southSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> westSprites = new List<Sprite>();
    // lists of sprites for rooms to use
    [SerializeField] private GameObject roomPrefab;
    // gameobject to instantiate
    [SerializeField] private float roomOffset;
    // how much to offset each room by
    [SerializeField] private int curRooms;
    // # of rooms that currently exist
    [SerializeField] private int maxRooms;
    // the # of rooms to create
    [SerializeField] private List<Vector2> roomPositions = new List<Vector2>();
    // list of positions of chunks
    [SerializeField] private List<GameObject> createdRooms = new List<GameObject>();
    // list of created chunks
    [SerializeField] private bool fillToMax;
    [SerializeField] private bool fixHallways;
    private Dictionary<string, string> oppositeOf = new Dictionary<string, string>() {
        {"n", "s"},
        {"e", "w"},
        {"s", "n"},
        {"w", "e"}
    };
    private WaitForSeconds quickDelay = new WaitForSeconds(0.05f);

    void Start() {
        curRooms = 0;
        StartCoroutine(CreateRoom(0, 0, "starter"));
        StartCoroutine(FixRoomsCoroutine());
    }

    /// <summary>
    /// Create a room at the designated location, and use recursion to create all nearby rooms.
    /// </summary>
    /// <param name="xPos">The x position to create the room at.</param>
    /// <param name="yPos">The y position to create the room at.</param>
    /// <param name="roomNeedsEntranceAt">Which cardinal direction the room must connect to.</param>
    private IEnumerator CreateRoom(int xPos, int yPos, string roomNeedsEntranceAt) {
        yield return quickDelay;
        if (curRooms + 1 > maxRooms) { yield break; }
        // instantly break out if we have exceeded the max #
        // test the position.
        Vector2 curPos = new Vector2(xPos, yPos);
        if (roomPositions.Contains(curPos)) { yield break; }
        // if the position is already created, break out
        GameObject created = Instantiate(roomPrefab, new Vector3(xPos * roomOffset, yPos * roomOffset, 0), Quaternion.identity);
        // create a gameobject from the prefab at the designated location.
        int rand = Random.Range(0, 7);
        // create a random number, 0-6
        // based on the needed room, assign the sprite
        GenerateSprite(created, roomNeedsEntranceAt, rand);
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
        roomPositions.Add(curPos);
        createdRooms.Add(created);
        
        curRooms++;
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
    private IEnumerator FixRoomsCoroutine() {
        yield return new WaitForSeconds(1f);
        if (fillToMax) {
            FillToMax();
        }
        if (fixHallways) {
            FixHallways();
        }
    }

    private void FillToMax() {
        for (int i = 0; i < 100; i+=5) {
            GameObject focusRoom = createdRooms[Mathf.Clamp(i, 0, createdRooms.Count)];
            focusRoom.GetComponent<SpriteRenderer>().sprite = startSprite;
            string spriteName = focusRoom.GetComponent<SpriteRenderer>().sprite.name;
            StartCoroutine(CreateRoom((int)(focusRoom.transform.position.x / roomOffset), (int)(focusRoom.transform.position.y / roomOffset + 1), "s"));
            StartCoroutine(CreateRoom((int)(focusRoom.transform.position.x / roomOffset + 1), (int)(focusRoom.transform.position.y / roomOffset), "w"));
            StartCoroutine(CreateRoom((int)(focusRoom.transform.position.x / roomOffset), (int)(focusRoom.transform.position.y / roomOffset - 1), "n"));
            StartCoroutine(CreateRoom((int)(focusRoom.transform.position.x / roomOffset - 1), (int)(focusRoom.transform.position.y / roomOffset), "e"));
            for (int k = 0; k < 4; k++) {
                Destroy(focusRoom.transform.GetChild(0).gameObject);
            }
            ParentColliderTo("north path collider", focusRoom);
            ParentColliderTo("east path collider", focusRoom);
            ParentColliderTo("south path collider", focusRoom);
            ParentColliderTo("west path collider", focusRoom);
        }
    }

    private void FixHallways() {
        for (int i = 0; i < createdRooms.Count; i++) {
            string undesiredRooms = "";
            GameObject curRoom = createdRooms[i];
            string curSpriteName = curRoom.GetComponent<SpriteRenderer>().sprite.name;
            if (curSpriteName.Contains("n")) {
                undesiredRooms += CheckHallway(
                    curRoom, 
                    Mathf.RoundToInt(curRoom.transform.position.x / roomOffset), 
                    Mathf.RoundToInt(curRoom.transform.position.y / roomOffset + 1), 
                    "s"
                );
            }
            if (curSpriteName.Contains("e")) {
                undesiredRooms += CheckHallway(
                    curRoom, 
                    Mathf.RoundToInt(curRoom.transform.position.x / roomOffset + 1), 
                    Mathf.RoundToInt(curRoom.transform.position.y / roomOffset), 
                    "w"
                );
            }
            if (curSpriteName.Contains("s")) {
                undesiredRooms += CheckHallway(
                    curRoom, 
                    Mathf.RoundToInt(curRoom.transform.position.x / roomOffset), 
                    Mathf.RoundToInt(curRoom.transform.position.y / roomOffset - 1), 
                    "s"
                );
            }
            if (curSpriteName.Contains("w")) {
                undesiredRooms += CheckHallway(
                    curRoom, 
                    Mathf.RoundToInt(curRoom.transform.position.x / roomOffset - 1), 
                    Mathf.RoundToInt(curRoom.transform.position.y / roomOffset), 
                    "e"
                );
            }
            string desiredRooms = "";
            // if (!undesiredRooms.Contains("n")) { desiredRooms += "n"; }
            // if (!undesiredRooms.Contains("e")) { desiredRooms += "e"; }
            // if (!undesiredRooms.Contains("s")) { desiredRooms += "s"; }
            // if (!undesiredRooms.Contains("w")) { desiredRooms += "w"; }
            print(undesiredRooms);
        }
    }

    private string CheckHallway(GameObject curRoom, int x, int y, string checkFor) {
        Vector2 checkingVector = new Vector2(x, y);
        // get a vector2 for the position we want to check at
        if (roomPositions.Contains(checkingVector)) {
            // if there is a room leading from the pathway
            if (!createdRooms[roomPositions.IndexOf(checkingVector)].GetComponent<SpriteRenderer>().name.Contains(oppositeOf[checkFor])) {
                // if the corresponding room has doesn't connect
                return checkFor;
                // return a pathway we DONT want 
            }
            else {
                return "";
                // room does connect up, so leave the entrance alone
            }
        }
        else {
            // no room, so we don't want a pathway there
            return checkFor;
        }
    }

    private void GenerateSprite(GameObject created, string roomNeedsEntranceAt, int rand) {
        if (roomNeedsEntranceAt == "starter") {
            // if creating a starting room
            created.GetComponent<SpriteRenderer>().sprite = startSprite;
            created.GetComponent<SpriteRenderer>().color = Color.green;
            created.GetComponent<SpriteRenderer>().sortingOrder = 1;
            // set the sprite, make it green, make it go forwards
        }
        else if (roomNeedsEntranceAt == "n") { created.GetComponent<SpriteRenderer>().sprite = northSprites[rand]; }
        else if (roomNeedsEntranceAt == "e") { created.GetComponent<SpriteRenderer>().sprite = eastSprites[rand]; }
        else if (roomNeedsEntranceAt == "s") { created.GetComponent<SpriteRenderer>().sprite = southSprites[rand]; }
        else if (roomNeedsEntranceAt == "w") { created.GetComponent<SpriteRenderer>().sprite = westSprites[rand]; }
        else { print("big error"); }
    }
}
