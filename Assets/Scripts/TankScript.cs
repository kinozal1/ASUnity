using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 
public class TankScript : MonoBehaviour
{
        public GameObject Anchor;
        public GameObject CurrentWaypoint;
        public int exceptionAction = 0;
        public NavMeshAgent agent;
        public GameObject CurrentCamera;
        public GameObject NextCamera;
        public bool Back7;
        public bool TriggerForOptionToMove;
     Rigidbody rb;
        public float DistanceForForwardLidars = 3;
        public float DistanceForSideLidars = 3;
        public float DistanceForNormalMove = 4;


        public WheelCollider[] WColLeft;
        public WheelCollider[] WColRight;

        public GameObject LFLidar, RFLidar, LeftLidar, RightLidar;

        public Transform[] wheelsL; //1
        public Transform[] wheelsR; //1

        public float wheelOffset = 0.1f; //2
        public float wheelRadius = 0.13f; //2
        public float maxSpeed = 1f;
        public float maxSteer = 30;
        public float maxAccel = 25;
        public float maxBrake = 50;

        public float one = 50;

        public float Lenght;
    public bool easttest;
        public float Immitsteer = 0;
        public float ImmiAngle = 0;
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
        rb = transform.GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().centerOfMass = COM.localPosition;

        wheels = new WheelData[WColLeft.Length + WColRight.Length];

        for (int i = 0; i < WColLeft.Length; i++)
        {
            wheels[i] = SetupWheels(wheelsL[i], WColLeft[i]);
        }

        for (int i = 0; i < WColRight.Length; i++)
        {
            wheels[i + WColLeft.Length] = SetupWheels(wheelsR[i], WColRight[i]);
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

            accel = Input.GetAxis("Vertical");
            steer = Input.GetAxis("Horizontal");
        CarMove(accel, steer);
        UpdateWheels();
            ActionsWithAgent(directionToTarget);
            
        }

        private void UpdateWheels()
        {
            float delta = Time.fixedDeltaTime;
            foreach (WheelData w in wheels)
            {

                w.rotation = Mathf.Repeat(w.rotation + delta * w.col.rpm * 360.0f / 60.0f, 360.0f); //20
                w.wheelTransform.localRotation = Quaternion.Euler(w.rotation, 0, 0.0f); //21
                if (GameObject.Find("InfoMenu") != null)
                {
                    GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().RPM = w.col.rpm;
                }
            }

        }

        

        private void OnMouseUpAsButton()
        {

            NextCamera.SetActive(true);
            CurrentCamera.SetActive(false);


            GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCar = gameObject;
            GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCarName = gameObject.name;




        } //Нажатие на машину

        private void CarMove(float accel, float steer)
        {
        sbyte Left, Right;
        float speed = rb.velocity.sqrMagnitude;
        Left = 0;
        Right = 0;
        TankDirections(ref Left, ref Right, accel, steer);
        if (accel == 0 && steer == 0)
        {

            foreach (WheelCollider col in WColRight)
            {
                col.brakeTorque = maxBrake * 1000000;
            }
            foreach (WheelCollider col in WColLeft)
            {
                col.brakeTorque = maxBrake * 1000000;
            }
        }

        else 
            {
            if ((speed <= maxSpeed))
            {
                foreach (WheelCollider col in WColRight)
                {
                    col.brakeTorque = 0;
                    col.motorTorque = maxAccel * Right;
                }
                foreach (WheelCollider col in WColLeft)
                {
                    col.brakeTorque = 0;
                    col.motorTorque = maxAccel * Left;
                }
                Debug.Log("ОК");
            }
            else
            {
                foreach (WheelCollider col in WColRight)
                {
                    col.motorTorque = 0;
                }
                foreach (WheelCollider col in WColLeft)
                {
                    col.motorTorque = 0;
                }
                Debug.Log("Превысил");
                Debug.Log(speed);
            }


        }
        } 

      

       

        private void ActionsWithAgent(Vector3 directionToTarget)
        {
            if (directionToTarget.magnitude > 6f)
            {
                CurrentWaypoint.GetComponent<NavMeshAgent>().isStopped = true;

            }
            else
            {
                CurrentWaypoint.GetComponent<NavMeshAgent>().isStopped = false;
            }
        } //Остановка агента
    private void TankDirections(ref sbyte Left, ref sbyte Right,float accel,float steer)
    {
        if (accel == 0 && steer == 0)
        {
            Left = 0;
            Right = 0;
        }
        else if (accel < 0 && steer == 0)
        {
            Left = -1;
            Right = -1;
        }
        else if (accel > 0 && steer == 0)
        {
            Left = 1;
            Right = 1;
        }
        else if (accel== 0 && steer > 0)
        {
            Left = 10;
            Right = -10;
        }
        else if (accel == 0 && steer < 0)
        {
            Left = -10;
            Right = 10;
        }
        else
        {
            Left = 1;
            Right = 1;
        }
    }

}
