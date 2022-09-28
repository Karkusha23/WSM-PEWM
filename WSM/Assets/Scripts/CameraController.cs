using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float camDumping;
    public float switchDistance;
    public float camMouseMultiplicator;
    public float maxCamMouseDistance;

    private Vector3 cameraOffset;
    private Vector3 target;
    private Transform followingTarget;
    private Action doEveryFrame;
    private Transform player;
    private Vector3 tmp;
    private float sqrMaxCamMouseDistance;

    private void Start()
    {
        cameraOffset = new Vector3(0f, 0f, transform.position.z);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        doEveryFrame = null;
        sqrMaxCamMouseDistance = maxCamMouseDistance * maxCamMouseDistance;
    }

    private void Update()
    {
        if (doEveryFrame != null)
        {
            doEveryFrame.Invoke();
        }
    }

    public void goToPos(Vector3 pos)
    {
        doEveryFrame = goingToStaticPos;
        target = pos + cameraOffset;
    }

    public void follow(GameObject what)
    {
        doEveryFrame = goingToDynamicPos;
        followingTarget = what.transform;
    }

    public void followMousePos()
    {
        doEveryFrame = goingToMousePos;
    }

    private void goingToStaticPos()
    {
        transform.position = Vector3.Lerp(transform.position, target, camDumping * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) <= switchDistance)
        {
            transform.position = target;
            doEveryFrame = null;
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
