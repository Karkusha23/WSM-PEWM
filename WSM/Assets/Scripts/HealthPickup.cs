using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().health += 1;
            other.GetComponent<Player>().HUD.transform.Find("HPMain").GetComponent<HUDHP>().addHP();
            Destroy(gameObject);
        }
    }
}
