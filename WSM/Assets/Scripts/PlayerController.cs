using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int health;
    public float moveSpeed;
    public float invincibilityAfterDamage;
    public WeaponsList weaponsList;
    public GameObject loseScreen;
    public GameObject HUD;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool invincible;
    private float invincibleTimer;
    private HUDHP hpMain;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        invincible = false;
        loseScreen.SetActive(false);
        hpMain = HUD.transform.Find("HPMain").GetComponent<HUDHP>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (invincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                invincible = false;
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon_dropped"))
        {
            GameObject weapon = Instantiate(weaponsList.weapons[other.name[0] - '0'], transform, false);
            weapon.transform.position = transform.position;
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            takeDamage();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            takeDamage();
        }
    }

    private void takeDamage()
    {
        if (!invincible)
        {
            health -= 1;
            hpMain.removeHP();
            if (health <= 0)
            {
                loseGame();
                return;
            }
            invincible = true;
            invincibleTimer = invincibilityAfterDamage;
        }
    }

    private void loseGame()
    {
        loseScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}
