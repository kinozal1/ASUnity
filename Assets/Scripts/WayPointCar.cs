using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WayPointCar : MonoBehaviour
{

    public GameObject[] Waypoints;
    public GameObject Anchor;
    public GameObject CurrentWaypoint;
    public int IndexOfWaypoint=0;
    public NavMeshAgent agent;
    public GameObject CurrentCamera;
    public GameObject NextCamera; 

    public WheelCollider[] WColForward;
    public WheelCollider[] WColBack;

    public Transform[] wheelsF; //1
    public Transform[] wheelsB; //1

    public float wheelOffset = 0.1f; //2
    public float wheelRadius = 0.13f; //2

    public float maxSteer = 30;
    public float maxAccel = 25;
    public float maxBrake = 50;

    public float one = 50;
    public float two = 50;
    public float three = 50;
    public float four = 50;

    public float Lenght;


    public float MiddleAngle = 0;

    public Vector3 OY;

    public Vector3 Axis = new Vector3(0, 1, 0);

    public Vector3 directionToTarget;

    public Transform COM;

    private void Awake()
    {


    }


    public class WheelData
    { //3
        public Transform wheelTransform; //4
        public WheelCollider col; //5
        public Vector3 wheelStartPos; //6 
        public float rotation = 0.0f;
    }

    protected WheelData[] wheels; //8

    // Use this for initialization
    void Start()
    {
        CurrentWaypoint = Waypoints[0];



        GetComponent<Rigidbody>().centerOfMass = COM.localPosition;

        wheels = new WheelData[WColForward.Length + WColBack.Length];

        for (int i = 0; i < WColForward.Length; i++)
        {
            wheels[i] = SetupWheels(wheelsF[i], WColForward[i]);
        }

        for (int i = 0; i < WColBack.Length; i++)
        {
            wheels[i + WColForward.Length] = SetupWheels(wheelsB[i], WColBack[i]);
        }

    }



    private WheelData SetupWheels(Transform wheel, WheelCollider col)
    {
        WheelData result = new WheelData();

        result.wheelTransform = wheel;
        result.col = col;
        result.wheelStartPos = wheel.transform.localPosition;

        return result;

    }

    void FixedUpdate()
    {
        
        Vector3 OY = (Anchor.transform.position - transform.position).normalized;
        Vector3 directionToTarget = (CurrentWaypoint.transform.position - transform.position);
        Lenght = directionToTarget.magnitude;

        float accel = 0;
        float steer = 0;
        CarMoveWithNav(accel, steer, directionToTarget, OY);
        UpdateWheels(); //11
       if (directionToTarget.magnitude>4.5f)
        {
            CurrentWaypoint.GetComponent<NavMeshAgent>().isStopped=true;

        }
        else
        {
            CurrentWaypoint.GetComponent<NavMeshAgent>().isStopped = false;
        }
        


    }


    private void UpdateWheels()
    {
        float delta = Time.fixedDeltaTime;
        foreach (WheelData w in wheels)
        {
            
            w.rotation = Mathf.Repeat(w.rotation + delta * w.col.rpm * 360.0f / 60.0f, 360.0f); //20
            if (w.col.steerAngle<8 && w.col.steerAngle > -8)
            {
                w.wheelTransform.localRotation = Quaternion.Euler(w.rotation, 0, 0.0f); //21
            }
            else
            {
                w.wheelTransform.localRotation = Quaternion.Euler(w.rotation, w.col.steerAngle, 0.0f); //21
            }
            
            if (GameObject.Find("InfoMenu") != null)
            {
                GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().RPM = w.col.rpm;
            }
        }

    }

    private void CarMoveWithNav(float accel, float steer, Vector3 Direction, Vector3 OY)
    {
        if (Direction.magnitude > 4)
        {
            accel = 1;
            
        }
        else
        {   accel = 0;
            IndexOfWaypoint++;
            if (IndexOfWaypoint >= Waypoints.Length)
            {
                IndexOfWaypoint = 0;
                CurrentWaypoint = Waypoints[IndexOfWaypoint];
            }
            CurrentWaypoint = Waypoints[IndexOfWaypoint];
            
        }
        MiddleAngle = Vector3.SignedAngle(Direction, OY, Axis);
        MiddleAngle = -MiddleAngle;



        if ((MiddleAngle >= 90) || (MiddleAngle <= -90))
        {
            if (MiddleAngle>165 && MiddleAngle<-165)
            {
                MiddleAngle = 0;
            }
            accel = -accel;
        }

        if (MiddleAngle >= maxSteer)
        {
            MiddleAngle = maxSteer;
        }
        else if (MiddleAngle < -maxSteer)
        {
            MiddleAngle = -maxSteer;
        }



        foreach (WheelCollider col in WColForward)
        {

            col.steerAngle = MiddleAngle;

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
                one = col.rpm;
                if (col.rpm > 50 || col.rpm < -50)
                    col.brakeTorque = maxBrake*50;
                else
                {
                    col.brakeTorque = 0;
                    col.motorTorque = accel * maxAccel;
                }
                
            }

        }



    }
    private void OnMouseUpAsButton()
    {

        NextCamera.SetActive(true);
        CurrentCamera.SetActive(false);
        

        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCar = gameObject;
        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCarName = gameObject.name;




    }
}
