using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WayPointCar : MonoBehaviour
{
    public GameObject Anchor;
    public GameObject CurrentWaypoint;
    public int exceptionAction = 0;
    public NavMeshAgent agent;
    public GameObject CurrentCamera;
    public GameObject NextCamera;
    public bool Back7;
    public bool TriggerForOptionToMove;

    public GameObject CarCamera;
    
    public float LFLidarDistance;
    public float RFLidarDistance;
    public float LeftLidarDistance;
    public float RightLidarDistance;

    public float MaxspeedValue=20;
    Rigidbody rb;




    public float DistanceForForwardLidars = 3;
    public float DistanceForSideLidars = 3;
    public float DistanceForNormalMove = 4;
    public WheelCollider[] WColForward;
    public WheelCollider[] WColBack;

    public GameObject LFLidar, RFLidar, LeftLidar, RightLidar;
    public Transform[] wheelsF; //1
    public Transform[] wheelsB; //1

    public float wheelOffset = 0.1f; //2
    public float wheelRadius = 0.13f; //2

    public float maxSteer = 30;
    public float maxAccel = 25;
    public float maxBrake = 50;

    public float one = 50;

    public float Lenght;

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

        accel = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");


        CheckLidar();
        if (exceptionAction == 0)
        {

            if (steer == 0 && Immitsteer > 0)
            {
                Immitsteer -= 0.01f;
                steer = Immitsteer;
                Debug.Log(steer);
            }
            else if ((steer == 0 && Immitsteer < 0))
            {
                Immitsteer += 0.01f;
                steer = Immitsteer;
                Debug.Log(steer);
            }
            else if((steer == 0 && Immitsteer == 0))
            {
                steer = Immitsteer;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (TriggerForOptionToMove)
                {
                    TriggerForOptionToMove = false;
                }
                else
                {
                    TriggerForOptionToMove = true;
                }
            }
            else if(Input.GetKeyUp(KeyCode.Space))
            {
                TriggerForOptionToMove = false;
            }

            if (TriggerForOptionToMove)
            {
                CarMove(accel, steer);
            }
            else
            {
                CarMoveWithNav(accel, steer, directionToTarget, OY);
            }
        }

        else { Exceptions(exceptionAction); }
        UpdateWheels();
        ActionsWithAgent(directionToTarget);
        Debug.Log("Скорость");
        Debug.Log(rb.velocity.sqrMagnitude);
        Debug.Log("Коллайдер");
        foreach (WheelCollider col in WColBack)
        {

            Debug.Log(col.motorTorque);
        }
        Debug.Log("Коллайдер");
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
            if (Back7)
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

    private void CarMoveWithNav(float accel,float steer, Vector3 Direction, Vector3 OY)
    {

        if (Direction.magnitude > 2)
        {
            accel = 1;

        }
        else
        {
            accel = 0;


        }

        MiddleAngle = Vector3.SignedAngle(Direction, OY, Axis);
        MiddleAngle = -MiddleAngle;
        

        if ((MiddleAngle >= 90) || (MiddleAngle <= -90))
        {
            Back7 = true;
            accel = -accel;
        }
        else
        {
            Back7 = false;
        }

        if (MiddleAngle >= maxSteer)
        {
            MiddleAngle = maxSteer;
        }
        else if (MiddleAngle <= -maxSteer)
        {
            MiddleAngle = -maxSteer;
        }

        if ((ImmiAngle != MiddleAngle) && (Mathf.Abs(MiddleAngle- ImmiAngle) >=0))
        {
           if ((MiddleAngle - ImmiAngle) > 0)
            {
                ImmiAngle += 0.3f;
            }
            else
            {
                ImmiAngle -= 0.3f;
            }
        }

        foreach (WheelCollider col in WColForward)
        {
            if (Back7)
            {
                col.steerAngle = 0;
            }
            else
            {
                col.steerAngle = ImmiAngle;
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


        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCar = gameObject;
        GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCarName = gameObject.name;




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
        RaycastHit hit1,hit2,hit3,hit4;
        if (Physics.Raycast(LFLidar.transform.position, LFLidar.transform.forward, out hit1))
        {

        }
        if (Physics.Raycast(RFLidar.transform.position, RFLidar.transform.forward, out hit2))
        {

        }
        if (Physics.Raycast(LeftLidar.transform.position, LeftLidar.transform.forward, out hit3))
        {

        }
        if (Physics.Raycast(RightLidar.transform.position, RightLidar.transform.forward, out hit4))
        {

        }
        LFLidarDistance = Vector3.Distance(LFLidar.transform.position, hit1.point);
        RFLidarDistance = Vector3.Distance(RFLidar.transform.position, hit2.point);
        LeftLidarDistance = Vector3.Distance(LeftLidar.transform.position, hit3.point);
        RightLidarDistance = Vector3.Distance(RightLidar.transform.position, hit4.point);





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

    private void Exceptions(int exceptionAction)
    {
        ImmiAngle = 0;
        if (exceptionAction == 1)
        {
            if (Immitsteer > 0)
            {
                Immitsteer -= 0.01f;
            }
            else if (Immitsteer < 0)
            {
                Immitsteer += 0.01f;
            }
            CarMove(-1, Immitsteer);
        }//Врезание в параболу = отъезд назад

        else if (exceptionAction == 2)
        {
            CarMove(-1, Immitsteer);
            Immitsteer -= 0.02f;
            if (Immitsteer < -1)
            {
                Immitsteer = -1;
            }
        } // Сильно неудачный поворот вправо = Машина едет назад с сильным поворотом колес влево

        else if (exceptionAction == 3)
        {
            CarMove(-1, Immitsteer);
            Immitsteer += 0.01f;
            if (Immitsteer > 1)
            {
                Immitsteer = 1;
            }
        } // неудачный поворот вправо = Машина едет назад с поворотом колес влево

        else if (exceptionAction == 4)
        {
            CarMove(-1, Immitsteer);
            Immitsteer += 0.02f;
            if (Immitsteer > 1)
            {
                Immitsteer = 1;
            }
        } // Сильно неудачный поворот влево = Машина едет назад с сильным поворотом колес вправо

        else if (exceptionAction == 5)
        {
            CarMove(-1, Immitsteer);
            Immitsteer += 0.01f;
            if (Immitsteer > 1)
            {
                Immitsteer = 1;
            }
        } //  неудачный поворот влево = Машина едет назад с  поворотом колес вправо

    } // Исключения

    private void ActionsWithAgent(Vector3 directionToTarget)
    { 
        MiddleAngle = Vector3.SignedAngle(directionToTarget, OY, Axis);
        MiddleAngle = -MiddleAngle;
        if ((directionToTarget.magnitude > 4f) || (MiddleAngle>maxSteer|| MiddleAngle<-maxSteer))
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



}

