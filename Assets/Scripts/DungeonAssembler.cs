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
    [Header("Lists")]
    [SerializeField] private List<GameObject> colliderList = new List<GameObject>();
    // list of colliders to attach to a room
    [SerializeField] private List<Sprite> northSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> eastSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> southSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> westSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> allSprites = new List<Sprite>();
    // lists of sprites for rooms to use
    [SerializeField] private List<Vector2> roomPositions = new List<Vector2>();
    // list of positions of chunks
    [SerializeField] private List<GameObject> createdRooms = new List<GameObject>();
    // list of created chunks
    [SerializeField] private List<RoomData> roomsToCreate = new List<RoomData>();
    // list of roomDatas which we take from
    [Header("Creation Variables")]
    [SerializeField] private bool showAnimation;
    [SerializeField] public int seed = 0;
    [SerializeField] private int desiredRooms;
    // the # of rooms to create
    [SerializeField] private float roomOffset;
    // how much to offset each room by
    [SerializeField] private Sprite startSprite;
    // the sprite for starting out with
    [SerializeField] private GameObject roomPrefab;
    // gameobject to instantiate
    [SerializeField] private GameObject roomParent;
    [SerializeField] private Camera mainCamera;
    private WaitForSeconds shortDelay = new WaitForSeconds(0f);
    // just to make it animated/cool
    private WaitForSeconds longDelay = new WaitForSeconds(0.5f);
    private Vector2 zeroZero = new Vector2(0, 0);
    private DraggableMinimap minimap;
    private System.Random prng;

    private void Start() {
        prng = new System.Random(seed);
        CreateRoom(new Vector2(0, 0), "starter");
        CreateRoom(new Vector2(0, 1), "s");
        CreateRoom(new Vector2(1, 0), "w");
        CreateRoom(new Vector2(0, -1), "n");
        CreateRoom(new Vector2(-1, 0), "e");
        // create the starter rooms
        StartCoroutine(CreateAllRoomsCoro());
        // create all the rooms with the coro
        minimap = FindObjectOfType<DraggableMinimap>();
    }

    /// <summary>
    /// A coroutine to create all the rooms required, as well as fixing bad entrances.
    /// </summary>
    private IEnumerator CreateAllRoomsCoro() {
        while (createdRooms.Count < desiredRooms) {
            // while there are less rooms than wanted
            // can't use a for loop, so just stick with the while loop
            int rand = prng.Next(0, roomsToCreate.Count);
            // get a random number to pick with
            RoomData roomData = roomsToCreate[rand];
            // get the data from that location
            roomsToCreate.RemoveAt(rand);
            // and then remove it
            if (!(roomPositions.Contains(new Vector2(roomData.pos.x * roomOffset, roomData.pos.y * roomOffset)))) {
                // as long as the position isn't already taken
                if (showAnimation) { yield return shortDelay; }
                // quick delay (only use for visual effects)
                CreateRoom(roomData.pos, roomData.needsEntranceAt);
                // make a room based on the data we grabbed
            }
        }
        StartCoroutine(FixRoomsCoro());
        // after all the rooms are created, fix em up
    }

    /// <summary>
    /// Create a room at the designated location and add the necessary rooms to the roomsToCreate list.
    /// </summary>
    /// <param name="pos">The x position vector2 to create the room at.</param>
    /// <param name="roomNeedsEntranceAt">Which cardinal direction the room must connect to.</param>
    private void CreateRoom(Vector2 pos, string roomNeedsEntranceAt) {
        if (createdRooms.Count >= desiredRooms) { return; }
        // instantly break out if we have exceeded the max #
        Vector2 offsetRoom = new Vector2(pos.x * roomOffset, pos.y * roomOffset);
        if (roomPositions.Contains(offsetRoom)) { return; }
        roomPositions.Add(offsetRoom);
        // if the position is already created, break out, REDUNDANT BUT NEEDED FOR SOME REASON
        GameObject created = Instantiate(roomPrefab, offsetRoom, Quaternion.identity);
        createdRooms.Add(created);
        created.GetComponent<Room>().needsEntranceAt = roomNeedsEntranceAt;
        created.GetComponent<Room>().truePos = offsetRoom;
        created.GetComponent<Room>().pos01 = pos;
        // create a gameobject from the prefab at the designated location, and add it to the list
        int rand = prng.Next(0, 9);
        // create a random number, 0-9
        GenerateSprite(created, roomNeedsEntranceAt, rand, desiredRooms);
        // based on the needed room, assign the sprite
        string spriteName = created.GetComponent<SpriteRenderer>().sprite.name;
        // get the name of the sprite, e.g. 'ew' for east and west entrances
        created.transform.parent = roomParent.transform;
        if (spriteName.Contains("n")) {
            // if the sprite has an entrance pointing north
            roomsToCreate.Add(new RoomData(new Vector2(pos.x, pos.y + 1), "s"));
            // add a roomData object to the list, specifying it's position and required entrance
        }
        // no entrance pointing north, so use the correct collider
        // same process for all other directions
        if (spriteName.Contains("e")) { roomsToCreate.Add(new RoomData(new Vector2(pos.x + 1, pos.y), "w")); }
        if (spriteName.Contains("s")) { roomsToCreate.Add(new RoomData(new Vector2(pos.x, pos.y - 1), "n")); }
        if (spriteName.Contains("w")) { roomsToCreate.Add(new RoomData(new Vector2(pos.x - 1, pos.y), "e")); }
    }

    /// <summary>
    /// A coroutine to fix all the bad entrances of a room, called in CreateAllRoomsCoro.
    /// </summary>
    private IEnumerator FixRoomsCoro() {
        for (int i = 0; i < createdRooms.Count; i++) {
            // for every room created
            GameObject room = createdRooms[i];
            // get the room (faster than using foreach)
            string spriteName = room.GetComponent<SpriteRenderer>().sprite.name;
            string wantedEntrances = room.GetComponent<SpriteRenderer>().sprite.name;
            string unwantedEntrances = "";
            Vector2 northCheck = new Vector2(
                Mathf.RoundToInt(createdRooms[i].transform.position.x), 
                Mathf.RoundToInt(createdRooms[i].transform.position.y + roomOffset));
            Vector2 eastCheck = new Vector2(
                Mathf.RoundToInt(createdRooms[i].transform.position.x + roomOffset), 
                Mathf.RoundToInt(createdRooms[i].transform.position.y));
            Vector2 southCheck = new Vector2(
                Mathf.RoundToInt(createdRooms[i].transform.position.x), 
                Mathf.RoundToInt(createdRooms[i].transform.position.y - roomOffset));
            Vector2 westCheck = new Vector2(
                Mathf.RoundToInt(createdRooms[i].transform.position.x - roomOffset), 
                Mathf.RoundToInt(createdRooms[i].transform.position.y));
            // assign necessary starting variables
            if (spriteName.Contains("n") && !roomPositions.Contains(northCheck) || spriteName.Contains("n") && roomPositions.Contains(northCheck) && 
                !createdRooms[roomPositions.IndexOf(northCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("s")) {
                // if there is a north entrance
                // if there is no room at that position OR the room at that position doesn't have the right entrance
                unwantedEntrances += "n";
                // add it to the string of entrances we dont want
            }
            // repeat for all other cardinal directions
            if (spriteName.Contains("e") && !roomPositions.Contains(eastCheck) || spriteName.Contains("e") && roomPositions.Contains(eastCheck) && 
                !createdRooms[roomPositions.IndexOf(eastCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("w")) {
                unwantedEntrances += "e";
            }
            if (spriteName.Contains("s") && !roomPositions.Contains(southCheck) || spriteName.Contains("s") && roomPositions.Contains(southCheck) && 
                !createdRooms[roomPositions.IndexOf(southCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("n")) {
                unwantedEntrances += "s";
            }
            if (spriteName.Contains("w") && !roomPositions.Contains(westCheck) || spriteName.Contains("w") && roomPositions.Contains(westCheck) && 
                !createdRooms[roomPositions.IndexOf(westCheck)].GetComponent<SpriteRenderer>().sprite.name.Contains("e")) {
                unwantedEntrances += "w";
            }
            for (int k = 0; k < unwantedEntrances.Length; k++) {
                // for every character
                wantedEntrances = string.Join("", wantedEntrances.Split(unwantedEntrances[k]));
                // attempt to remove it, getting a string with only the entrances we want
            }
            room.GetComponent<SpriteRenderer>().sprite = allSprites[(from sprite in allSprites select sprite.name).ToList().IndexOf(wantedEntrances)];
            // locate the sprite by name
            spriteName = room.GetComponent<SpriteRenderer>().sprite.name;
            room.name = spriteName;
            if (spriteName.Contains("n")) { ParentColliderTo("north path collider", room); }
            else { ParentColliderTo("north wall collider", room); }
            if (spriteName.Contains("e")) { ParentColliderTo("east path collider", room); }
            else { ParentColliderTo("east wall collider", room); }
            if (spriteName.Contains("s")) { ParentColliderTo("south path collider", room); }
            else { ParentColliderTo("south wall collider", room); }
            if (spriteName.Contains("w")) { ParentColliderTo("west path collider", room); }
            else { ParentColliderTo("west wall collider", room); }
            if (showAnimation) { yield return shortDelay; }
            // quick delay (aesthetics)
        }
        StartCoroutine(GiveControl());
    }

    /// <summary>
    /// Child a collider with a given name to the given gameobject.
    /// </summary>
    /// <param name="colliderName">The name of the collider to match to.</param>
    /// <param name="parentTo">The gameobject to chil the collider to.</param>
    private void ParentColliderTo(string colliderName, GameObject parentTo) {
        GameObject collider = Instantiate(colliderList[(from col in colliderList select col.name).ToList().IndexOf(colliderName)], zeroZero, Quaternion.identity);
        // get the collider based on its name
        collider.transform.parent = parentTo.transform;
        // child the collider to the gameobject
        collider.transform.localPosition = zeroZero;
        // set the collider's localposition to be normal
    }

    /// <summary>
    /// Assign a sprite fitting given requirements to the specified gameobject.
    /// </summary>
    /// <param name="created">The gameobject to assign the sprite to.</param>
    /// <param name="roomNeedsEntranceAt">Which entrance the sprite MUST have.</param>
    /// <param name="rand">The </param>
    /// <param name="desiredRooms"></param>
    private void GenerateSprite(GameObject created, string roomNeedsEntranceAt, int rand, int desiredRooms) {
        if (roomNeedsEntranceAt == "starter") {
            // if creating a starting room
            created.GetComponent<SpriteRenderer>().sprite = startSprite;
            created.GetComponent<SpriteRenderer>().color = Color.green;
            // set the sprite, make it green, set its sorting order forwards
        }
        else if (roomNeedsEntranceAt == "n") { created.GetComponent<SpriteRenderer>().sprite = northSprites[rand]; }
        else if (roomNeedsEntranceAt == "e") { created.GetComponent<SpriteRenderer>().sprite = eastSprites[rand]; }
        else if (roomNeedsEntranceAt == "s") { created.GetComponent<SpriteRenderer>().sprite = southSprites[rand]; }
        else if (roomNeedsEntranceAt == "w") { created.GetComponent<SpriteRenderer>().sprite = westSprites[rand]; }
        else { print("big error"); }
        created.GetComponent<SpriteRenderer>().sortingOrder = -1;
        // assign a sprite based on the requirement
        if (createdRooms.Count == Mathf.RoundToInt(desiredRooms / 2)) {
            created.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else if (createdRooms.Count == desiredRooms) {
            created.GetComponent<SpriteRenderer>().color = Color.red;
        }
        // assign a treasure room half and boss room as the final (in terms of room creation)
    }

    private IEnumerator GiveControl() {
        yield return longDelay;
        // for (int i = 0; i < 98; i++) {
        //     mainCamera.orthographicSize -= 1.0f;
        //     if (showAnimation) { yield return shortDelay; }
        // }
        minimap.Generate();
    }
}
