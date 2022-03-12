using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCam : MonoBehaviour
{
    private Transform camTransform;
    private Transform myTransform;

    private void Awake()
    {
        camTransform = Camera.main.transform;
        myTransform = this.transform;
    }

    private void Update()
    {
        myTransform.LookAt(myTransform.position + camTransform.rotation * Vector3.forward, camTransform.rotation * Vector3.up);
    }
}
