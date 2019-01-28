using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMov : MonoBehaviour
{
    public float OX, OY, OZ;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OX = GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().OX;
        OY = GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().OY;
        OZ = GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().OZ;
        transform.Translate(OX, OY, OZ);
    }
}
