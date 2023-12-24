using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int health;
    public float moveSpeed;
    public float damage;
    public float reloadTime;
    public float invincibilityAfterDamage;
    public float dodgerollActivePhaseTime;
    public float dodgerollPassivePhaseTime;
    public float dodgerollSpeedBoost;
    public WeaponsList weaponsList;
    public GameObject HUD;
    public GameObject loseScreen;

    public Dictionary<ItemController.Item, int> itemCounts;

    [HideInInspector]
    public bool hasWeapon;
    [HideInInspector]
    public int weaponID;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool invincible;
    private float invincibleTimer;
    private HUDHP hpMain;
    private Minimap minimap;
    private ItemPanel itemPanel;
    private Animator anim;
    private float dropKeyTime;
    private float dropWeaponTimer;
    private CameraController camcon;
    private int invincible_dodgeroll;
    private float dodgeroolTimer;
    private GameObject weapon;

    private void Awake()
    {
        health = PlayerData.health;
        hasWeapon = PlayerData.hasWeapon;
        if (hasWeapon)
        {
            weapon = Instantiate(weaponsList.weapons[PlayerData.weaponID], transform.position, Quaternion.identity, transform);
            weaponID = PlayerData.weaponID;
        }
        damage = PlayerData.damage;
        reloadTime = PlayerData.reloadTime;
        moveSpeed = PlayerData.moveSpeed;
        itemCounts = new Dictionary<ItemController.Item, int>();
        itemCounts.Add(ItemController.Item.Damage, PlayerData.damageItemCount);
        itemCounts.Add(ItemController.Item.Tears, PlayerData.tearsItemCount);
        itemCounts.Add(ItemController.Item.Speed, PlayerData.speedItemCount);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        invincible = false;
        invincibleTimer = 0f;
        loseScreen.SetActive(false);
        hpMain = HUD.transform.Find("HPMain").GetComponent<HUDHP>();
        minimap = HUD.transform.Find("Minimap").GetComponent<Minimap>();
        itemPanel = HUD.transform.Find("ItemPanel").GetComponent<ItemPanel>();
        anim = GetComponent<Animator>();
        anim.SetFloat("InvincibilityTime", invincibilityAfterDamage);
        anim.SetBool("Invincible", false);
        anim.SetBool("IsRolling", false);
        dropKeyTime = 1f;
        camcon = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        invincible_dodgeroll = 0;
        dodgeroolTimer = 0f;
    }

    private void Update()
    {
        if (invincible_dodgeroll == 0)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }

        dodgeRollCheck();

        if (invincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                invincible = false;
                anim.SetBool("Invincible", false);
            }
        }

        if (hasWeapon && Input.GetKey(KeyCode.F))
        {
            dropWeaponTimer += Time.deltaTime;
            if (dropWeaponTimer >= dropKeyTime)
            {
                dropWeapon();
            }
        }
        else
        {
            dropWeaponTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        checkTakeWeapon(other);
        if (other.CompareTag("SmallRoom") || other.CompareTag("BigRoom"))
        {
            minimap.checkRoom(other.transform.position);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        checkTakeWeapon(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        checkTakeDamage(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        checkTakeDamage(collision);
    }

    public void setInvincible(float time)
    {
        invincible = true;
        invincibleTimer = time;
    }

    public void lockWeapon()
    {
        if (hasWeapon)
        {
            weapon.GetComponent<PlayerWeapon>().canPlayerControlWeapon = false;
        }
    }

    public void unlockWeapon()
    {
        if (hasWeapon)
        {
            weapon.GetComponent<PlayerWeapon>().canPlayerControlWeapon = true;
        }
    }

    private void checkTakeDamage(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("EnemyBullet"))
        {
            takeDamage();
        }
    }

    private void takeDamage()
    {
        if (!(invincible || invincible_dodgeroll == 1))
        {
            health -= 1;
            hpMain.removeHP();
            if (health <= 0)
            {
                loseGame();
                return;
            }
            invincible = true;
            anim.SetBool("Invincible", true);
            invincibleTimer = invincibilityAfterDamage;
        }
    }

    private void loseGame()
    {
        loseScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    private void checkTakeWeapon(Collider2D other)
    {
        if (other.CompareTag("Weapon_dropped") && Input.GetKey(KeyCode.E) && Vector3.Distance(transform.position, other.transform.position) <= 2f)
        {
            dropWeapon();
            weapon = Instantiate(weaponsList.weapons[other.name[0] - '0'], transform.position, Quaternion.identity, transform);
            hasWeapon = true;
            Destroy(other.gameObject);
            if (camcon.camMode == 1 || camcon.camMode == 3)
            {
                camcon.followMousePos();
            }
        }
    }

    private void dropWeapon()
    {
        if (hasWeapon)
        {
            Instantiate(weaponsList.weapons_dropped[weapon.name[0] - '0'], transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
            Destroy(weapon);
            hasWeapon = false;
            weapon = null;
            if (camcon.camMode == 1 || camcon.camMode == 3)
            {
                camcon.follow(gameObject);
            }
        }
    }

    private void dodgeRollCheck()
    {
        if (Input.GetMouseButtonDown(1) && invincible_dodgeroll == 0)
        {
            if (hasWeapon)
            {
                weapon.SetActive(false);
            }
            dodgeroolTimer = dodgerollActivePhaseTime;
            invincible_dodgeroll = 1;
            anim.SetBool("IsRolling", true);
            moveSpeed *= dodgerollSpeedBoost;
            movement = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        }

        if (invincible_dodgeroll == 1)
        {
            dodgeroolTimer -= Time.deltaTime;
            if (dodgeroolTimer <= 0f)
            {
                invincible_dodgeroll = 2;
                dodgeroolTimer = dodgerollPassivePhaseTime;
                anim.SetBool("IsRolling", false);
                moveSpeed /= dodgerollSpeedBoost;
            }
        }

        if (invincible_dodgeroll == 2)
        {
            dodgeroolTimer -= Time.deltaTime;
            if (dodgeroolTimer <= 0f)
            {
                if (hasWeapon)
                {
                    weapon.SetActive(true);
                }
                invincible_dodgeroll = 0;
            }
        }
    }

    public void getItem(GameObject item)
    {
        ItemController itemCont = item.GetComponent<ItemController>();
        itemPanel.updateCountValue(itemCont.item, ++itemCounts[itemCont.item]);
        Debug.Log(itemCounts[itemCont.item]);
        moveSpeed += itemCont.speedBoost;
        damage += itemCont.damageBoost;
        reloadTime -= itemCont.tearsBoost;
        if (reloadTime < 0.05f)
        {
            reloadTime = 0.05f;
        }
    }
}
