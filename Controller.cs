using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{

    public WheelCollider[] WColForward;
    public WheelCollider[] WColBack;

    public Transform[] wheelsF; 
    public Transform[] wheelsB; 

    public float wheelOffset = 0.1f; 
    public float wheelRadius = 0.13f; 

    public float maxSteer = 30;
    public float maxAccel = 25;
    public float maxBrake = 50;

    public Transform COM;

    public class WheelData
    { 
        public Transform wheelTransform; 
        public WheelCollider col; 
        public Vector3 wheelStartPos; 
    }

    protected WheelData[] wheels; 




    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = COM.localPosition;

       

      

    }


  

    void FixedUpdate()
    {

        float accel = 0;
        float steer = 0;

        accel = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");

        CarMove(accel, steer);
     
    }


   

    private void CarMove(float accel, float steer)
    {

        foreach (WheelCollider col in WColForward)
        {
            col.steerAngle = steer * maxSteer;
        }

        if (accel == 0)
        {
            foreach (WheelCollider col in WColBack)
            {
                col.brakeTorque = maxBrake;
            }

        }
        else
        {

            foreach (WheelCollider col in WColBack)
            {
                col.brakeTorque = 0;
                
                
            }
            foreach (WheelCollider col in WColForward)
            {
                col.motorTorque = accel * maxAccel;
            }
            

        }



    }

}