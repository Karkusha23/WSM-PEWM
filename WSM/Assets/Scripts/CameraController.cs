using System.Collections;
using System.IO;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float camDumping;
    public float switchDistance;

    private Vector3 cameraOffset;
    private int camMovingMode;
    private Vector3 target;
    private Transform followingTarget;

    private void Start()
    {
        cameraOffset = new Vector3(0f, 0f, transform.position.z);
        camMovingMode = 0;
    }

    private void Update()
    {
        if (camMovingMode == 1)
        {
            transform.position = Vector3.Lerp(transform.position, target, camDumping * Time.deltaTime);
            if (Vector3.Distance(transform.position, target) <= switchDistance)
            {
                transform.position = target;
                camMovingMode = 0;
                Debug.Log(0);
            }
        }
        else if (camMovingMode == 2)
        {
            target = followingTarget.position + cameraOffset;
            if (Vector3.Distance(transform.position, target) > switchDistance)
            {
                transform.position = Vector3.Lerp(transform.position, target, camDumping * Time.deltaTime);
            }
            else
            {
                transform.position = target;
                camMovingMode = 3;
                Debug.Log(3);
            }
        }    
        else if (camMovingMode == 3)
        {
            transform.position = followingTarget.position + cameraOffset;
        }    
    }

    public void goToPos(Vector3 pos)
    {
        camMovingMode = 1;
        target = pos + cameraOffset;
    }

    public void follow(GameObject what)
    {
        camMovingMode = 2;
        followingTarget = what.transform;
    }
}
