using UnityEngine;

public class WeaponDropped : MonoBehaviour
{
    // Prefab of this weapon as part of player
    public GameObject weaponPrefab;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("PlayerIsNear", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("PlayerIsNear", false);
        }
    }
}
