using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsToGround : MonoBehaviour
{
    public bool ShowPoints = true;
    // Start is called before the first frame update
    void Start()
    {
        RaycastHit hit;
        for (int i = 0; i < transform.childCount; i++)
        {
            for(int j =0; j<transform.GetChild(i).childCount; j++)
            {
                if (((Physics.Raycast(transform.GetChild(i).GetChild(j).position, -transform.GetChild(i).GetChild(j).transform.up, out hit)))) //Проверка, есть ли точка на карте 
                {
                    transform.GetChild(i).GetChild(j).position = hit.point;
                }
                if (!ShowPoints)
                {
                    transform.GetChild(i).GetChild(j).GetComponent<MeshRenderer>().enabled = false;
                }
            }
            


        }
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
}
