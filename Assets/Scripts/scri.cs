using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scri : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (((Physics.Raycast(transform.position, -transform.up, out hit)))) //Проверка, есть ли точка на карте 
        {
            transform.position = hit.point;
        } 

    }
}
