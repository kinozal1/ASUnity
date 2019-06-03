using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WayPointCar : MonoBehaviour
{
    public int Car_ID;
    public float[] Data = new float[8];
    public string Name;

    public GameObject Anchor;
    public GameObject CurrentWaypoint;
    public int exceptionAction = 0;
    public NavMeshAgent agent;

    public GameObject CurrentCamera;
    public GameObject NextCamera;

    public bool MovingBack;
    public bool MovingByUser;

    public GameObject CarCamera,Shaft;

    public float LFLidarDistance, DebugScum;
    public float RFLidarDistance;
    public float LeftLidarDistance;
    public float RightLidarDistance;

    public float MaxspeedValue = 20;
    Rigidbody rb;

    public float Lerper1, Lerper2;

    public bool WayByOnlyPoints;

    public float DistanceForForwardLidars = 3;
    public float DistanceForSideLidars = 3;
    public float DistanceForNormalMove = 4;
    public WheelCollider[] WColForward;
    public WheelCollider[] WColBack;

    public GameObject LFLidar, RFLidar, LeftLidar, RightLidar;
    public Transform[] wheelsF; //1
    public Transform[] wheelsB; //1
    public GameObject CDR,CDL;
    public float wheelOffset = 0.1f; //2
    public float wheelRadius = 0.13f; //2

    public float maxSteer = 30;
    public float maxAccel = 25;
    public float maxBrake = 50;

    public float one = 50;

    public float Lenght;

    public float Immitsteer = 0;
    public float MiddleAngle = 0;

    public Vector3 OY;

    public Vector3 Axis = new Vector3(0, 1, 0);

    public Vector3 directionToTarget;

    public Transform COM;

    public float MinimalRadius;

    public GameObject Pointer;
    public GameObject Points;
    public float Distance;

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
        if (WayByOnlyPoints)
        {
            CarCamera.SetActive(false);
        }
        CDR.GetComponent<CircleDrawer>().radius = (WColForward[0].transform.position - WColBack[0].transform.position).magnitude/Mathf.Tan(maxSteer);
        CDL.GetComponent<CircleDrawer>().radius = -(WColForward[0].transform.position - WColBack[0].transform.position).magnitude / Mathf.Tan(maxSteer);
        OY = (Anchor.transform.position - Shaft.transform.position);
        rb = transform.GetComponent<Rigidbody>();
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
        if (WayByOnlyPoints)
        {
            MovingByPoints();
        }
        accel = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");
        CheckAngle(CurrentWaypoint, Shaft, Axis, ref MiddleAngle, ref Lenght);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (MovingByUser)
            {
                MovingByUser = false;
            }
            else
            {
                MovingByUser = true;
            }
        }
        CheckLidar();
        if (!WayByOnlyPoints)
        {
            ActionsWithAgent(directionToTarget);
        }
        if (exceptionAction == 0)
        {
             CarMoveWithNav(accel, steer);
        }

        else { Exceptions(exceptionAction, DebugScum); }
       
        UpdateWheels();
       
        

    }

    private void UpdateWheels()
    {
        float delta = Time.fixedDeltaTime;
        foreach (WheelData w in wheels)
        {

            w.rotation = Mathf.Repeat(w.rotation + delta * w.col.rpm * 360.0f / 60.0f, 360.0f); //20

            //if (w.col.steerAngle<25 && w.col.steerAngle > -25)

            // {

            //    w.wheelTransform.localRotation = Quaternion.Euler(w.rotation, 0, 0.0f); //21

            //}

                w.wheelTransform.localRotation = Quaternion.Euler(w.rotation, w.col.steerAngle, 0.0f); //21

            Event_Manager.Instance.CurrentCar.Data[0] = w.col.rpm;
            
        }

    }

    private void CarMoveWithNav(float accel, float steer)
    {

        if (Lenght > 2.5)
        {
            accel = 1;

        }
        else
        {
            accel = 0;
        }
        if (MiddleAngle >= 90 || MiddleAngle <=-90)
        {
            MovingBack = true;
        }
        else 
        {
            MovingBack = false;
        }



        if (MiddleAngle >= maxSteer)
        {
            MiddleAngle = maxSteer;
        }
        else if (MiddleAngle <= -maxSteer)
        {
            MiddleAngle = -maxSteer;
        }

       
   
        foreach (WheelCollider col in WColForward)
        {
            if (MovingBack)
            {

                col.steerAngle = Mathf.MoveTowardsAngle(col.steerAngle, 0, 1f);
                accel = -1;
                Debug.Log(col.steerAngle);
            }
            else
            {

                col.steerAngle = Mathf.MoveTowardsAngle(col.steerAngle, MiddleAngle, 1f);
            }

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
                    col.brakeTorque = maxBrake * 50;
                else
                {
                    col.brakeTorque = 0;
                    col.motorTorque = accel * maxAccel;
                }

            }

        }
     



    } //Движение машины за агентом



    private void OnMouseUpAsButton()
    {
        NextCamera.SetActive(true);
        CurrentCamera.SetActive(false);

        Event_Manager.Instance.CurrentCar.Position = transform;
        Event_Manager.Instance.CurrentCar.Car_id = Car_ID;
        Event_Manager.Instance.CurrentCar.Data = Data;
        Event_Manager.Instance.CurrentCar.Name = Name;

        //Нужно выбросить :(
       // GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCar = gameObject;
       // GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCarName = gameObject.name;




    } //Нажатие на машину

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




    } //Движение машины по кнопкам

    private void CheckLidar()
    {
        RaycastHit hit1, hit2, hit3, hit4;
        if (Physics.Raycast(LFLidar.transform.position, LFLidar.transform.forward, out hit1))
        {
            LFLidarDistance = Vector3.Distance(LFLidar.transform.position, hit1.point);
        }
        if (Physics.Raycast(RFLidar.transform.position, RFLidar.transform.forward, out hit2))
        {
            RFLidarDistance = Vector3.Distance(RFLidar.transform.position, hit2.point);
        }
        if (Physics.Raycast(LeftLidar.transform.position, LeftLidar.transform.forward, out hit3))
        {
            LeftLidarDistance = Vector3.Distance(LeftLidar.transform.position, hit3.point);
        }
        if (Physics.Raycast(RightLidar.transform.position, RightLidar.transform.forward, out hit4))
        {
            RightLidarDistance = Vector3.Distance(RightLidar.transform.position, hit4.point);
        }
        
        
       
       





        float[] Distances = new float[] { LeftLidarDistance, LFLidarDistance, RFLidarDistance, RightLidarDistance };

        float[] ReactionDistances = new float[] { DistanceForSideLidars, DistanceForForwardLidars, DistanceForForwardLidars, DistanceForSideLidars };

        bool[] LidarReaction = { false, false, false, false };

        int count = 0;
        foreach (float Dist in Distances)
        {
            if (Dist <= ReactionDistances[count])
            {
                LidarReaction[count] = true;
            }
            else
            {
                LidarReaction[count] = false;
            }
            count++;
        }
        // Debug.Log("Dis1 " + Distance1);
        //Debug.Log("Dis2 " + Distance2);

        if (!LidarReaction[0] && !LidarReaction[1] && !LidarReaction[2] && !LidarReaction[3])
        {
            exceptionAction = 0;
        }  // Свободная дорога

        else if ((LidarReaction[0] && LidarReaction[1] && LidarReaction[2] && LidarReaction[3]) || (!LidarReaction[0] && LidarReaction[1] && LidarReaction[2] && !LidarReaction[3]))
        {
            exceptionAction = 1;

        } // Врезание в "параболу" 

        else if (LidarReaction[0] && LidarReaction[1] && LidarReaction[2] && !LidarReaction[3])

        {
            exceptionAction = 2;

        } // Сильно Неудачный поворот вправо 

        else if (LidarReaction[0] && LidarReaction[1] && !LidarReaction[2] && !LidarReaction[3])

        {
            exceptionAction = 3;

        } // Неудачный поворот вправо 

        else if (!LidarReaction[0] && LidarReaction[1] && LidarReaction[2] && LidarReaction[3])

        {
            exceptionAction = 4;

        } // Сильно Неудачный поворот влево 

        else if (!LidarReaction[0] && !LidarReaction[1] && LidarReaction[2] && LidarReaction[3])

        {
            exceptionAction = 5;

        } //  Неудачный поворот влево 

        /*Debug.Log(exceptionAction);
        Debug.Log("Левый лидар" + LidarReaction[0]);
        Debug.Log("Левый передний лидар" + LidarReaction[1]);
        Debug.Log("Правый передний" + LidarReaction[2]);
        Debug.Log("Правый лидар" + LidarReaction[3]);*/


    } //Проверка расстояния для исключений

    private void Exceptions(int exceptionAction,float DebugScum)
    {

        if (exceptionAction == 1)
        {
            foreach (WheelCollider col in WColForward)
            {
                    col.steerAngle = Mathf.MoveTowardsAngle(col.steerAngle, 0, 1f);
            }
            foreach (WheelCollider col in WColBack)
            {
                col.brakeTorque = 0;
                col.motorTorque = -maxAccel/2;
            }

        }//Врезание в параболу = отъезд назад

        else if (exceptionAction == 2)
        {
            foreach (WheelCollider col in WColForward)
            {
                col.steerAngle = Mathf.MoveTowardsAngle(col.steerAngle, -maxSteer, 1f);
            }
            foreach (WheelCollider col in WColBack)
            {
                col.brakeTorque = 0;
                col.motorTorque = -maxAccel/2;
            }

        } // Сильно неудачный поворот вправо = Машина едет назад с сильным поворотом колес влево

        else if (exceptionAction == 3)
        {

            foreach (WheelCollider col in WColForward)
            {
                col.steerAngle = Mathf.MoveTowardsAngle(col.steerAngle, -maxSteer/2, 1f);
            }
            foreach (WheelCollider col in WColBack)
            {
                col.brakeTorque = 0;
                col.motorTorque = -maxAccel/2;
            }

        } // неудачный поворот вправо = Машина едет назад с поворотом колес влево

        else if (exceptionAction == 4)
        {
            foreach (WheelCollider col in WColForward)
            {
                col.steerAngle = Mathf.MoveTowardsAngle(col.steerAngle, maxSteer, 1f);
            }
            foreach (WheelCollider col in WColBack)
            {
                col.brakeTorque = 0;
                col.motorTorque = -maxAccel/2;
            }
        } // Сильно неудачный поворот влево = Машина едет назад с сильным поворотом колес вправо

        else if (exceptionAction == 5)
        {
            foreach (WheelCollider col in WColForward)
            {
                col.steerAngle = Mathf.MoveTowardsAngle(col.steerAngle, maxSteer/2, 1f);
            }
            foreach (WheelCollider col in WColBack)
            {
                col.brakeTorque = 0;
                col.motorTorque = -maxAccel/2;
            }
        } //  неудачный поворот влево = Машина едет назад с  поворотом колес вправо

    } // Исключения

    private void ActionsWithAgent(Vector3 directionToTarget)
    {
        if ((directionToTarget.magnitude > 6f))
        {
            CurrentWaypoint.GetComponent<NavMeshAgent>().isStopped = true;
            CurrentWaypoint.GetComponent<NavMeshAgentPoint>().Repath();

        }
        else
        {
            CurrentWaypoint.GetComponent<NavMeshAgent>().isStopped = false;
            CurrentWaypoint.GetComponent<NavMeshAgentPoint>().Path();
        }
    } //Остановка агента

    private void MaxSpeed(Rigidbody rb)
    {
        if (rb.velocity.sqrMagnitude > MaxspeedValue)
        {
            foreach (WheelCollider col in WColBack)
            {

                col.motorTorque = 0;
            }

        }

    }

    private void CheckAngle(GameObject Target, GameObject Shaft, Vector3 Axis,ref float MiddleAngle, ref float Length)
    {
        OY = (Anchor.transform.position - Shaft.transform.position);
        directionToTarget = (Target.transform.position - Shaft.transform.position);  
        Lenght = directionToTarget.magnitude;
        directionToTarget.y = 0;
        OY.y = 0;
        MiddleAngle = Vector3.SignedAngle(OY, directionToTarget, Axis);
    }
    
    private void MovingByPoints()
    {
        for (int i = 0; i < Points.transform.childCount; i++)
        {
            for (int j = 0; j < Points.transform.GetChild(i).childCount; j++)
            {
                Vector3 PointserVector, PointVector, CarVector;
                CarVector = new Vector3(transform.position.x, 0, transform.position.z);
                PointserVector = new Vector3(Pointer.transform.position.x, 0, Pointer.transform.position.z);
                PointVector = new Vector3(Points.transform.GetChild(i).GetChild(j).position.x, 0, Points.transform.GetChild(i).GetChild(j).position.z);
                if ((PointVector - PointserVector).magnitude < Distance && (CarVector - PointVector).magnitude > (CarVector - PointserVector).magnitude)
                {
                    CurrentWaypoint = Points.transform.GetChild(i).GetChild(j).gameObject;
                }

            }
        }
    }
    





}

