using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float camDumping;
    public float switchDistance;
    public float camMouseMultiplicator;
    public float maxCamMouseDistance;

    [HideInInspector]
    public int camMode;
    // 0 - goes to static pos
    // 1 - goes to dynamic pos
    // 2 - staying in static pos
    // 3 - following dynamic pos
    // 4 - following mouse pos

    [HideInInspector]
    public bool isAllowedToMove;

    private Vector3 cameraOffset;
    private Vector3 target;
    private Transform followingTarget;
    private Action doEveryFrame;
    private Transform player;
    private Vector3 tmp;
    private float sqrMaxCamMouseDistance;

    private void Start()
    {
        isAllowedToMove = true;
        cameraOffset = new Vector3(0f, 0f, transform.position.z);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        doEveryFrame = null;
        sqrMaxCamMouseDistance = maxCamMouseDistance * maxCamMouseDistance;
    }

    private void Update()
    {
        if (doEveryFrame != null && isAllowedToMove)
        {
            doEveryFrame.Invoke();
        }
    }

    public void goToPos(Vector3 pos)
    {
        doEveryFrame = goingToStaticPos;
        target = pos + cameraOffset;
        camMode = 0;
    }

    public void follow(GameObject what)
    {
        doEveryFrame = goingToDynamicPos;
        followingTarget = what.transform;
        camMode = 1;
    }

    public void followMousePos()
    {
        doEveryFrame = goingToMousePos;
        camMode = 1;
    }

    public void checkCamMode()
    {
        if (camMode == 4)
        {
            tmp = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.position;
            if (tmp.sqrMagnitude <= sqrMaxCamMouseDistance)
            {
                tmp = tmp * camMouseMultiplicator + cameraOffset;
            }
            else
            {
                tmp = tmp.normalized * maxCamMouseDistance + cameraOffset;
            }
            if (tmp.magnitude > switchDistance)
            {
                followingMousePos();
            }
        }
    }

    private void goingToStaticPos()
    {
        transform.position = Vector3.Lerp(transform.position, target, camDumping * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) <= switchDistance)
        {
            transform.position = target;
            doEveryFrame = null;
            camMode = 2;
        }
    }

    private void goingToDynamicPos()
    {
        target = followingTarget.position + cameraOffset;
        if (Vector3.Distance(transform.position, target) > switchDistance)
        {
            transform.position = Vector3.Lerp(transform.position, target, camDumping * Time.deltaTime);
        }
        else
        {
            transform.position = target;
            doEveryFrame = following;
            camMode = 3;
        }
    }

    private void following()
    {
        transform.position = followingTarget.position + cameraOffset;
    }

    private void goingToMousePos()
    {
        tmp = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.position;
        if (tmp.sqrMagnitude <= sqrMaxCamMouseDistance)
        {
            target = player.position + tmp * camMouseMultiplicator + cameraOffset;
        }
        else
        {
            target = player.position + tmp.normalized * maxCamMouseDistance + cameraOffset;
        }
        if (Vector3.Distance(transform.position, target) > switchDistance)
        {
            transform.position = Vector3.Lerp(transform.position, target, camDumping * Time.deltaTime);
        }
        else
        {
            transform.position = target;
            doEveryFrame = followingMousePos;
            camMode = 4;
        }
    }

    private void followingMousePos()
    {
        tmp = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.position;
        if (tmp.sqrMagnitude <= sqrMaxCamMouseDistance)
        {
            transform.position = player.position + tmp * camMouseMultiplicator + cameraOffset;
        }
        else
        {
            transform.position = player.position + tmp.normalized * maxCamMouseDistance + cameraOffset;
        }
    }
}
