using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public enum Item { Damage, Tears, Speed };

    public Item item;
    public float damageBoost;
    public float tearsBoost;
    public float speedBoost;

    private void OnTriggerEnter2D(Collider2D other)
    {
        checkItemPickup(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        checkItemPickup(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        checkItemPickup(other);
    }

    private void checkItemPickup(Collider2D other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            other.GetComponent<PlayerController>().GetItem(gameObject);
            Destroy(gameObject);
        }
    }
}
