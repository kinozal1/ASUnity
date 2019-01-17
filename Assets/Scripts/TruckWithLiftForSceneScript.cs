using UnityEngine;
using System.Collections;

public class TruckWithLiftForSceneScript : MonoBehaviour
{
    private GameObject CurrentCamera;
    private GameObject NextCamera;

    public WheelCollider[] WColForward;
    public WheelCollider[] WColBack;

    public Transform[] wheelsF; //1
    public Transform[] wheelsB; //1

    public float wheelOffset = 0.1f; //2
    public float wheelRadius = 0.13f; //2

    public float maxSteer = 30;
    public float maxAccel = 25;
    public float maxBrake = 50;

    public Transform COM;

    public class WheelData
    { //3
        public Transform wheelTransform; //4
        public WheelCollider col; //5
        public Vector3 wheelStartPos; //6 
        public float rotation = 0.0f;
    }

    protected WheelData[] wheels; //8

    // Use this for initialization
    void Awake()
    {
        CurrentCamera = GameObject.Find("Camera");
        NextCamera = GameObject.Find("MainCamera");
    }

    void Start()
    {


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

        float accel = 0;
        float steer = 0;

        accel = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");

        CarMove(accel, steer);
       // UpdateWheels(); 
    }


    private void UpdateWheels()
    {
        float delta = Time.fixedDeltaTime;



        foreach (WheelData w in wheels)
        {
            w.rotation = Mathf.Repeat(w.rotation + delta * w.col.rpm * 360.0f / 60.0f, 360.0f); //20
            w.wheelTransform.localRotation = Quaternion.Euler(w.rotation, w.col.steerAngle, 0.0f); //21
        }

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
                col.motorTorque = accel * maxAccel;
            }

        }



    }
    private void OnMouseUpAsButton()
    {
        NextCamera.SetActive(true);
        CurrentCamera.SetActive(false);

    }

}