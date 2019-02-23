using UnityEngine;
using UnityEngine.AI;

public class AgentPoint : MonoBehaviour
{

    public Vector3 Vec;
    public float OX, OY, OZ;
    void Update()
    {
        //GameObject.Find("Point").GetComponent<UDPClient1>().Info1 = OX;
        //GameObject.Find("Point").GetComponent<UDPClient1>().Info2 = OY;
        //GameObject.Find("Point").GetComponent<UDPClient1>().Info3 = OZ;

        Ray ray = new Ray(transform.position, -transform.up);
            
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                OX = hit.point.x;
                OY = hit.point.y+0.2f;

                OZ = hit.point.z;
            Vec.x = OX;
            Vec.y = OY;
            Vec.z = OZ;
            }
        transform.position = Vec;
        
    }
}