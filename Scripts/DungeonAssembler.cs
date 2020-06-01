using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RoomData {
    public Vector2 pos;
    public string needsEntranceAt;
    public RoomData(Vector2 pos, string needsEntranceAt) {
        this.pos = pos;
        this.needsEntranceAt = needsEntranceAt;
    }
}

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
    [SerializeField] private int desiredRooms;
    // the # of rooms to create
    [SerializeField] private List<Vector2> roomPositions = new List<Vector2>();
    // list of positions of chunks
    [SerializeField] private List<GameObject> createdRooms = new List<GameObject>();
    // list of created chunks
    [SerializeField] private List<RoomData> roomsToCreate = new List<RoomData>();
    private WaitForSeconds quickDelay = new WaitForSeconds(0.01f);

    private void Start() {
        StartCoroutine(CreateRoomCoro(new Vector2(0, 0), "starter"));
        StartCoroutine(CreateRoomCoro(new Vector2(0, 1), "s"));
        StartCoroutine(CreateRoomCoro(new Vector2(1, 0), "w"));
        StartCoroutine(CreateRoomCoro(new Vector2(0, -1), "n"));
        StartCoroutine(CreateRoomCoro(new Vector2(-1, 0), "e"));
        StartCoroutine(CreateAllRoomsCoro());
        // StartCoroutine(FixRooms());
    }

    /// <summary>
    /// Create a room at the designated location, and use recursion to create all nearby rooms.
    /// </summary>
    /// <param name="xPos">The x position to create the room at.</param>
    /// <param name="yPos">The y position to create the room at.</param>
    /// <param name="roomNeedsEntranceAt">Which cardinal direction the room must connect to.</param>
    private IEnumerator CreateRoomCoro(Vector2 pos, string roomNeedsEntranceAt) {
        if (createdRooms.Count >= desiredRooms) { yield break; }
        // instantly break out if we have exceeded the max #
        if (roomPositions.Contains(pos)) { yield break; }
        roomPositions.Add(pos);
        // if the position is already created, break out, REDUNDANT BUT NEEDED FOR SOME REASON
        GameObject created = Instantiate(roomPrefab, new Vector3(pos.x * roomOffset, pos.y * roomOffset, 0), Quaternion.identity);
        createdRooms.Add(created);
        // create a gameobject from the prefab at the designated location.
        int rand = Random.Range(0, 9);
        // create a random number, 0-6
        // based on the needed room, assign the sprite
        GenerateSprite(created, roomNeedsEntranceAt, rand, desiredRooms);
        string spriteName = created.GetComponent<SpriteRenderer>().sprite.name;
        // get the name of the sprite, e.g. 'ew' for east and west entrances
        if (spriteName.Contains("n")) {
            ParentColliderTo("north path collider", created);
            roomsToCreate.Add(new RoomData(new Vector2(pos.x, pos.y + 1), "s"));
        }
        else { ParentColliderTo("north wall collider", created); }
        if (spriteName.Contains("e")) {
            ParentColliderTo("east path collider", created);
            roomsToCreate.Add(new RoomData(new Vector2(pos.x + 1, pos.y), "w"));
        }
        else { ParentColliderTo("east wall collider", created); }
        if (spriteName.Contains("s")) {
            ParentColliderTo("south path collider", created);
            roomsToCreate.Add(new RoomData(new Vector2(pos.x, pos.y - 1), "n"));
        }
        else { ParentColliderTo("south wall collider", created); }
        if (spriteName.Contains("w")) {
            ParentColliderTo("west path collider", created);
            roomsToCreate.Add(new RoomData(new Vector2(pos.x - 1, pos.y), "e"));
        }
        else { ParentColliderTo("west wall collider", created); }
        // based on the sprite name, create the needed rooms to connect to it
    }

    private IEnumerator CreateAllRoomsCoro() {
        while (createdRooms.Count < desiredRooms) {
            int rand = Random.Range(0, roomsToCreate.Count);
            RoomData roomData = roomsToCreate[rand];
            roomsToCreate.RemoveAt(rand);
            if (!(roomPositions.Contains(roomData.pos))) {
                yield return quickDelay;
                StartCoroutine(CreateRoomCoro(roomData.pos, roomData.needsEntranceAt));
            }
        }
        CheckRoomPositions();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(FixRooms());
        }
    }

    private void CheckRoomPositions() {
        roomPositions.Clear();
        for (int i = 0; i < createdRooms.Count; i++) {
            Vector2 rounded = new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x), Mathf.RoundToInt(createdRooms[i].transform.position.y));
            createdRooms[i].transform.position = rounded;
            roomPositions.Add(rounded);
        }
    }

    private IEnumerator FixRooms() {
        for (int i = 0; i < createdRooms.Count; i++) {
            GameObject room = createdRooms[i];
            string spriteName = room.GetComponent<SpriteRenderer>().sprite.name;
            string wantedEntrances = room.GetComponent<SpriteRenderer>().sprite.name;
            string unwantedEntrances = "";
            Vector2 northCheck = new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x), Mathf.RoundToInt(createdRooms[i].transform.position.y + 1));
            if (spriteName.Contains("n") && !roomPositions.Contains(northCheck)) {
                unwantedEntrances += "n";
            }
            else if (spriteName.Contains("n") && roomPositions.Contains(northCheck) && !createdRooms[roomPositions.IndexOf(northCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("s")) {
                unwantedEntrances += "n";
            }
            if (spriteName.Contains("e") && !roomPositions.Contains(new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x) + 1, Mathf.RoundToInt(createdRooms[i].transform.position.y)))) {
                unwantedEntrances += "e";
            }
            if (spriteName.Contains("s") && !roomPositions.Contains(new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x), Mathf.RoundToInt(createdRooms[i].transform.position.y - 1)))) {
                unwantedEntrances += "s";
            }
            if (spriteName.Contains("w") && !roomPositions.Contains(new Vector2(Mathf.RoundToInt(createdRooms[i].transform.position.x - 1), Mathf.RoundToInt(createdRooms[i].transform.position.y)))) {
                unwantedEntrances += "w";
            }
            for (int k = 0; k < unwantedEntrances.Length; k++) {
                wantedEntrances = string.Join("", wantedEntrances.Split(unwantedEntrances[k]));
            }
            room.GetComponent<SpriteRenderer>().sprite = allSprites[(from sprite in allSprites select sprite.name).ToList().IndexOf(wantedEntrances)];
            yield return quickDelay;
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

    private void GenerateSprite(GameObject created, string roomNeedsEntranceAt, int rand, int desiredRooms) {
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
        // if (createdRooms.Count == Mathf.RoundToInt(desiredRooms / 3)) {
        //     created.GetComponent<SpriteRenderer>().color = Color.yellow;
        // }
        // else if (createdRooms.Count == Mathf.RoundToInt(2 * desiredRooms / 3)) {
        //     created.GetComponent<SpriteRenderer>().color = Color.yellow;
        // }
        // else if (createdRooms.Count == desiredRooms) {
        //     created.GetComponent<SpriteRenderer>().color = Color.red;
        // }
    }
}
