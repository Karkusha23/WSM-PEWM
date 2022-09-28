using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().health += 1;
            other.GetComponent<PlayerController>().HUD.transform.Find("HPMain").GetComponent<HUDHP>().addHP();
            Destroy(gameObject);
        }
    }
}
