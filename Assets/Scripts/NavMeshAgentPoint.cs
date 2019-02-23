using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentPoint : MonoBehaviour
{
    public Camera cam;
    public float SpeedCont;
    public float Speed;
    public NavMeshAgent agent;
    public GameObject[] Obstacles;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);

            }
            agent.angularSpeed = SpeedCont;
            agent.speed = Speed;


            /*if (agent.isStopped == true)
            {
                for (int i = 0; i < Obstacles.Length; i++)
                {
                    Obstacles[i].SetActive(true);
                }
                agent.SetDestination(hit.point);
                for (int i = 0; i < Obstacles.Length; i++)
                {
                    Obstacles[i].SetActive(false);
                }
                agent.isStopped = false;

            }
            */

        }
    }
}
