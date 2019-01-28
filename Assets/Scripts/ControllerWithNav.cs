using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ControllerWithNav : MonoBehaviour
{
    public GameObject CurrentCamera;
    public GameObject NextCamera;
    public GameObject AgentTarget;
    public GameObject Lifter;
    public BoxCollider BoxCollider;
    public GameObject Anchor;

    public float LifterRotarion;

    public WheelCollider[] WColForward;
    public WheelCollider[] WColBack;

    public Transform[] wheelsF; //1
    public Transform[] wheelsB; //1

    public float wheelOffset = 0.1f; //2
    public float wheelRadius = 0.13f; //2

    public float maxSteer = 30;
    public float maxAccel = 25;
    public float maxBrake = 50;

    public float X;
    public float Y;
    public float Z;
    public float Lenght;


    public float MiddleAngle = 0;

    public Vector3 OY;

    public Vector3 Axis = new Vector3(0,1,0);

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
        LifterRotarion = 0.0f;


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
        Vector3 directionToTarget = (AgentTarget.transform.position - transform.position);
        X = directionToTarget.x;
        Lenght = directionToTarget.magnitude;
        Y = directionToTarget.z;
        Z = directionToTarget.y;
        float accel = 0;
        float steer = 0;
        CarMoveWithNav(accel, steer, directionToTarget,OY);
        UpdateWheels(); //11
        LifterButtons();


        LifterAnimation(LifterRotarion);
    }


    private void UpdateWheels()
    {
        float delta = Time.fixedDeltaTime;
        foreach (WheelData w in wheels)
        {

            w.rotation = Mathf.Repeat(w.rotation + delta * w.col.rpm * 360.0f / 60.0f, 360.0f); //20
            w.wheelTransform.localRotation = Quaternion.Euler(w.rotation, w.col.steerAngle, 0.0f); //21
            if (GameObject.Find("InfoMenu") != null)
            {
                GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().RPM = w.col.rpm;
            }
        }

    }
   
    private void CarMoveWithNav(float accel, float steer,Vector3 Direction,Vector3 OY)
    {
        if (Direction.magnitude > 13) { accel = 1; }
        else { accel = 0; }
        MiddleAngle = Vector3.SignedAngle(Direction, OY, Axis);
        MiddleAngle = -MiddleAngle;

        
        
        if ((MiddleAngle>=90) || (MiddleAngle < -90))
        {
            accel = -accel;
        }

        if (MiddleAngle >= maxSteer)
        {
            MiddleAngle = maxSteer;
        }
        if (MiddleAngle < -maxSteer)
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
                col.brakeTorque = 0;
                col.motorTorque = accel * maxAccel;
            }

        }



    }

    private void OnMouseUpAsButton()
    {


        CurrentCamera.SetActive(false);
        NextCamera.SetActive(true);

        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCar = gameObject;
        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCarName = gameObject.name;




    }

    private void LifterAnimation(float Rotation)
    {

        if ((Rotation <= 5.0f) && (Rotation >= -40.0f))
        {
            Lifter.transform.localRotation = Quaternion.Euler(Rotation, 0.0f, 0.0f);
            if (GameObject.Find("InfoMenu") != null)
            {
                GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().AngleOfLifter = -LifterRotarion;
            }
        }
        else
        {
            if (Rotation > 5.0f)
            {
                LifterRotarion = 5.0f;
            }
            else
            {
                LifterRotarion = -40.0f;
            }
        }

    }

    private void LifterButtons()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            LifterRotarion += 1.0f;
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            LifterRotarion -= 1.0f;
        }
    }


    
}
