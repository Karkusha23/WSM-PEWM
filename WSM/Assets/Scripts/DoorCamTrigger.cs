using UnityEngine;

public class DoorCamTrigger : MonoBehaviour
{
    private CameraController camcon;

    private void Start()
    {
        camcon = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            camcon.goToPos(transform.position);
        }
    }

    /*private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>().hasWeapon)
            {
                camcon.followMousePos();
            }
            else
            {
                camcon.follow(other.gameObject);
            }
        }
    }*/
}
