using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCubeMoving : MonoBehaviour {

    public float speed=0.5f, tilt;
   // public GameObject go;
  //  Rigidbody rb;

    void Start()
    {
       // rb = go.GetComponent<Rigidbody>();
        
    }

    void FixedUpdate()
    {
          transform.GetComponent<Rigidbody>().AddForce(transform.forward*speed, ForceMode.Impulse);
     

    }
}
