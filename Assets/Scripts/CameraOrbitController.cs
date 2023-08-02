using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbitController : MonoBehaviour
{
    public Transform _target;
    public SOFloat _cameraRotateSpeed;
    bool _mouseClicked;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) { _mouseClicked = true; }
        if (Input.GetMouseButtonUp(1)) { _mouseClicked = false; }

        if (_mouseClicked) 
        { 
            transform.RotateAround(_target.position, Vector3.up, Input.GetAxis("Mouse X") * _cameraRotateSpeed.Value);
        }
    }
}