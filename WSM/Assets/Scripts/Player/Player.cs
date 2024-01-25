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

    public GameObject weapon;
    public GameObject HUD;
    public GameObject loseScreen;

    public Dictionary<Item.Items, int> itemCounts;

    [HideInInspector]
    public bool hasWeapon;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float curMoveSpeed;
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

    private void Awake()
    {
        health = PlayerData.health;
        hasWeapon = PlayerData.weaponSample != null;
        if (hasWeapon)
        {
            weapon = Instantiate(PlayerData.weaponSample, transform.position, Quaternion.identity, transform);
        }
        damage = PlayerData.damage;
        reloadTime = PlayerData.reloadTime;
        moveSpeed = PlayerData.moveSpeed;
        itemCounts = PlayerData.itemCounts;
        if (itemCounts == null)
        {
            itemCounts = new Dictionary<Item.Items, int>();
            itemCounts.Add(Item.Items.Damage, 0);
            itemCounts.Add(Item.Items.Tears, 0);
            itemCounts.Add(Item.Items.Speed, 0);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        curMoveSpeed = moveSpeed;
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

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            minimap.toggleFullMap();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * curMoveSpeed * Time.fixedDeltaTime);
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
        if (other.CompareTag("WeaponDropped") && Input.GetKey(KeyCode.E) && Vector3.Distance(transform.position, other.transform.position) <= 2f)
        {
            dropWeapon();
            weapon = Instantiate(other.gameObject.GetComponent<WeaponDropped>().weaponPrefab, transform.position, Quaternion.identity, transform);
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
            Instantiate(weapon.GetComponent<PlayerWeapon>().weaponDroppedPrefab, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
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
            curMoveSpeed = moveSpeed * dodgerollSpeedBoost;
            movement = Vector2.Distance(movement, Vector2.zero) < 0.001f ? Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position : movement;
        }

        if (invincible_dodgeroll == 1)
        {
            dodgeroolTimer -= Time.deltaTime;
            if (dodgeroolTimer <= 0f)
            {
                invincible_dodgeroll = 2;
                dodgeroolTimer = dodgerollPassivePhaseTime;
                anim.SetBool("IsRolling", false);
                curMoveSpeed = moveSpeed;
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
        Item itemCont = item.GetComponent<Item>();
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
