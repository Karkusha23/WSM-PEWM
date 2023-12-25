using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Items { Damage, Tears, Speed };

    public Items item;
    public float damageBoost;
    public float tearsBoost;
    public float speedBoost;

    private bool isPicked;

    private void Start()
    {
        isPicked = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        checkItemPickup(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        checkItemPickup(other);
    }

    private void checkItemPickup(Collider2D other)
    {
        if (!isPicked && other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            isPicked = true;
            other.GetComponent<Player>().getItem(gameObject);
            Destroy(gameObject);
        }
    }
}
