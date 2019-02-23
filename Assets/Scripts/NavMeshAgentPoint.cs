using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentPoint : MonoBehaviour
{
    public Camera cam;
    public float SpeedCont;
    public float Speed;
    public float Angle;

    public NavMeshAgent agent;
    public GameObject[] Obstacle;
    public GameObject PointForSearch;
    public GameObject Anchor;

    public bool TestTest=false;

    // Update is called once per frame
    void Update()
    {

        if (TestTest)
        {
            RaycastHit hit;

            
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    agent.SetDestination(hit.point);


                    CheckAngle(hit.point);
                }

            }
  
            CheckAngle(agent.pathEndPosition);
        }
        else
        {
            RaycastHit hit;
            PointForSearch.transform.position = new Vector3(PointForSearch.transform.position.x, 200, PointForSearch.transform.position.z);
            if (Physics.Raycast(PointForSearch.transform.position, -PointForSearch.transform.up, out hit)) //Проверка, есть ли точка на карте 
            {
                PointForSearch.transform.position = hit.point;

                agent.SetDestination(PointForSearch.transform.position);
            }
            CheckAngle(PointForSearch.transform.position);


        }
    }
    void CheckAngle(Vector3 Point)
    {
        Vector3 Axis = new Vector3(0, 1, 0);
        Vector3 OY = (Anchor.transform.position - transform.position).normalized;
        Vector3 directionToTarget = (Point - transform.position);
        Angle = Vector3.SignedAngle(directionToTarget, OY, Axis);
        if (Angle > 140 || Angle < -140)
        {
            agent.transform.Rotate(0, 180, 0);

        }
       
    }
}
