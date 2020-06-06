using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float xMovement;
    [SerializeField] private float yMovement;
    [SerializeField] private List<Sprite> playerSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> projectileSprites = new List<Sprite>();
    [SerializeField] private List<float> projectileSpeeds = new List<float>();
    [SerializeField] private List<float> fireRateFloat = new List<float>();
    [SerializeField] private List<WaitForSeconds> fireRates = new List<WaitForSeconds>();
    [SerializeField] private List<float> abilityCooldown1Float = new List<float>();
    [SerializeField] private List<WaitForSeconds> abilityCooldown1s = new List<WaitForSeconds>();
    [SerializeField] private List<float> abilityCooldown2Float = new List<float>();
    [SerializeField] private List<WaitForSeconds> abilityCooldown2s = new List<WaitForSeconds>();
    [SerializeField] private List<GameObject> abilityPrefabList1 = new List<GameObject>();
    [SerializeField] private List<GameObject> abilityPrefabList2 = new List<GameObject>();
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool canFire;
    [SerializeField] private bool canUse1;
    [SerializeField] private bool canUse2;
    [SerializeField] private int health;
    [SerializeField] private GameObject gui;
    [SerializeField] private GameObject cover0;
    [SerializeField] private GameObject cover1;
    [SerializeField] private GameObject cover2;
    private int spriteIndex = 0;

    private void Start() {
        GetComponent<SpriteRenderer>().sprite = playerSprites[spriteIndex];
        foreach (float convert in fireRateFloat) {
            fireRates.Add(new WaitForSeconds(convert));
        }
        foreach (float convert in abilityCooldown1Float) {
            abilityCooldown1s.Add(new WaitForSeconds(convert));
        }
        StartCoroutine(ToggleFire());
        StartCoroutine(ToggleAbility1());
    }

    private IEnumerator ToggleFire() {
        while (true) {
            if (canFire == false) { canFire = true; }
            yield return fireRates[spriteIndex];
        }
    }

    private IEnumerator ToggleAbility1() {
        while (true) {
            if (canUse1 == false) { canUse1 = true; }
            yield return abilityCooldown1s[spriteIndex];
        }
    }
    private IEnumerator ToggleAbility2() {
        while (true) {
            if (canUse2 == false) { canUse2 = true; }
            yield return abilityCooldown2s[spriteIndex];
        }
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
        // float angleToMouse = Mathf.Sin((transform.position.y - mainCamera.ScreenToWorldPoint(Input.mousePosition).y) / (transform.position.x - mainCamera.ScreenToWorldPoint(Input.mousePosition).x));
        float lookingAngle = (transform.eulerAngles.z + 90f) * Mathf.PI / 180f;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) {
            if (canFire) {
                GameObject projectile = Instantiate(projectilePrefab, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
                projectile.transform.rotation = transform.rotation;
                projectile.GetComponent<SpriteRenderer>().sprite = projectileSprites[spriteIndex];
                projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Cos(lookingAngle) * projectileSpeeds[spriteIndex], Mathf.Sin(lookingAngle) * projectileSpeeds[spriteIndex]);
                canFire = false;
            }
        }
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1)) {
            StartCoroutine(UseAbility1());
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(UseAbility2());
        }
        gui.transform.position = new Vector2(transform.position.x, transform.position.y - 1.8f);
        if (canFire) { cover0.transform.position = new Vector2(transform.position.x - 0.3334f, transform.position.y - 2.15f); }
        else { cover0.transform.position = new Vector2(transform.position.x - 0.3334f, transform.position.y - 1.85f); }
        if (canUse1) { cover1.transform.position = new Vector2(transform.position.x, transform.position.y - 2.15f); }
        else { cover1.transform.position = new Vector2(transform.position.x, transform.position.y - 1.85f); }
        if (canUse2) { cover2.transform.position = new Vector2(transform.position.x + 0.3334f, transform.position.y - 2.15f); }
        else { cover2.transform.position = new Vector2(transform.position.x + 0.3334f, transform.position.y - 1.85f); }
    }
    // +-.33333 or 0, -1.85
    // -2.15 is lower lim

    private IEnumerator UseAbility1() {
        if (spriteIndex == 1) {
            if (canUse1) {
                GameObject ability = Instantiate(abilityPrefabList1[spriteIndex], new Vector2(mainCamera.ScreenToWorldPoint(Input.mousePosition).x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y), Quaternion.identity);
                canUse1 = false;
                yield return new WaitForSeconds(2f);
                Destroy(ability);
            }
        }
    }

    private IEnumerator UseAbility2() {
        if (spriteIndex == 1) {
            if (canUse2) {
                GameObject ability = Instantiate(abilityPrefabList2[spriteIndex], new Vector2(mainCamera.ScreenToWorldPoint(Input.mousePosition).x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y), Quaternion.identity);
                canUse2 = false;
                yield return new WaitForSeconds(5f);
                Destroy(ability);
            }
        }
    }

    private void FixedUpdate() {
        mainCamera.transform.position = new Vector3(transform.position.x + xMovement, transform.position.y + yMovement, -10f);
        transform.position = new Vector2(transform.position.x + xMovement, transform.position.y + yMovement);
    }

    public void UpdateSprite() {
        spriteIndex++;
        if (spriteIndex > 4) {
            spriteIndex = 0;
        }
        GetComponent<SpriteRenderer>().sprite = playerSprites[spriteIndex];
    }
}
