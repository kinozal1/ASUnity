﻿using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
    private GameObject CurrentCamera;
    public GameObject NextCamera;

    public GameObject Lifter;

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

    public Transform COM;

    private void Awake()
    {
        CurrentCamera = GameObject.Find("MainCamera");
        NextCamera = GameObject.Find("Camera");

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
        CurrentCamera = GameObject.Find("MainCamera");
      
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


        CurrentCamera.SetActive(false);
        NextCamera.SetActive(true);

        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCar = gameObject;
        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCarName = gameObject.name;
        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().TypeOfCar = 0;



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
            if (Rotation>5.0f)
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