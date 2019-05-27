using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.AI;

public class CarData_Manager : MonoBehaviour
{
    public GameObject TargetPointPrefab, CarPrefab, AgentPrefab, Points; //Префабы для ТочкиСледования, Машины
    public static float unityCof, RealCof;
    public WorkWithPoints workWithPoints;


    public GameObject CurrentCamera;
    public GameObject NextCamera;
    public struct CarData
    {


        //public DateTime Time1, Time2;
        // public int  Number1;

        public float Temp, Moisture, Compass, Forward, Backward, OX, OY, OZ;

        public CarData(string[] splitted)
        {
            //CarID = int.Parse(splitted[0])-1;
            OZ = Mathf.Abs(float.Parse(splitted[1])); //OZ
            OX = Mathf.Abs(float.Parse(splitted[2]));  //OX
            OY = float.Parse(splitted[3]); // OY
            //Time1 = DateTime.Parse(splitted[4]);
            //Number1 = int.Parse(splitted[5]);
            //Time2 = DateTime.Parse(splitted[6]);
            Temp = float.Parse(splitted[4]);
            Moisture = float.Parse(splitted[5]);
            Compass = float.Parse(splitted[6]);
            Forward = float.Parse(splitted[7]);
            Backward = float.Parse(splitted[8]);
        }
    } // Вся информация о машине
    public static List<GameObject> CarList = new List<GameObject>(0); // Машины на карте
    public static List<GameObject> AgentPointList = new List<GameObject>(0); // НавМешАгенты машин
    public static List<GameObject> TargetPointList = new List<GameObject>(0); // ТочкиСледования для НавМеш
    public static List<CarData> CarDataList = new List<CarData>(0);  // Информация о всех машинах 

    public GameObject TruckPrefab;

    private static List<GameObject> carsGameObjects = new List<GameObject>();

    public static List<CarInfo> CarsData = new List<CarInfo>();

    public class CarInfo
    {
        public string Name;
        public Transform Position;
        public int Car_id { get; set; }
        public float[] Data;
        //OX, OY, OZ, Temp, Moisture, Compass, Forward, Backward;
        //Данные о текущей машине для меню 
    }

    void Update()
    {
        workWithPoints = gameObject.GetComponent<WorkWithPoints>();
    }
    

    public void DataProcessing(string[] receviedData)
    {
       int CarID = int.Parse(receviedData[0]);

        if (CarID > CarDataList.Count)
        {
            CarID--;

            CarData CurrentCarData = new CarData(receviedData);

            RealCoords(unityCof, RealCof, ref CurrentCarData.OZ, ref CurrentCarData.OX, ref CurrentCarData.OY); //Подстройка координат 
            CarDataList.Add(CurrentCarData);
            Vector3 CurrentPoint = new Vector3(CurrentCarData.OX, 0, CurrentCarData.OZ);
            workWithPoints.KNN(ref CurrentPoint);
            TargetPointList.Add(Instantiate(TargetPointPrefab, CurrentPoint, Quaternion.Euler(0, 0, 0))); // Добавление ТочкиСледования


            AgentPointList.Add(Instantiate(AgentPrefab, CurrentPoint, Quaternion.Euler(0, 0, 0)));

            AgentPointList[CarID].GetComponent<NavMeshAgent>().Warp(CurrentPoint);
            AgentPointList[CarID].GetComponent<NavMeshAgentPoint>().PointForSearch = TargetPointList[CarID];




            // Добавление агента на карту в координаты точки
            CarList.Add(Instantiate(CarPrefab, new Vector3(AgentPointList[CarID].transform.position.x, AgentPointList[CarID].transform.position.y + 3, AgentPointList[CarID].transform.position.z), Quaternion.Euler(0, CurrentCarData.Compass, 0))); //Добавление машины на карту
            CarList[CarID].GetComponent<WayPointCar>().CurrentWaypoint = AgentPointList[CarID].gameObject; // Привязка агента к машине
            CarList[CarID].GetComponent<WayPointCar>().CurrentCamera = CurrentCamera;
            CarList[CarID].GetComponent<WayPointCar>().NextCamera = NextCamera;
        }
        else
        {
            CarID--;
            CarData CurrentCarData = new CarData(receviedData);
            RealCoords(unityCof, RealCof, ref CurrentCarData.OZ, ref CurrentCarData.OX, ref CurrentCarData.OY);
            Vector3 CurrentPoint = new Vector3(CurrentCarData.OX, 0, CurrentCarData.OZ);

            workWithPoints.KNN(ref CurrentPoint);
            CarDataList[CarID] = CurrentCarData;
            TargetPointList[CarID].transform.position = CurrentPoint;
            AgentPointList[CarID].GetComponent<NavMeshAgentPoint>().cam = CurrentCamera.GetComponent<Camera>();
        } 
    }

    public  void ClearData()
    {
        foreach (GameObject Car in CarList)
        {
            Destroy(Car);
        }
        CarList.Clear();

        foreach (GameObject Point in TargetPointList)
        {
            Destroy(Point);
        }
        TargetPointList.Clear();

        foreach (GameObject Agent in AgentPointList)
        {
            Destroy(Agent);

        }
        AgentPointList.Clear();
        CarDataList.Clear();
        workWithPoints.ClearData();

    }

    public static void ChangeCamera(bool ChangeCameraBool)
    {
        if (ChangeCameraBool)
        {
            foreach (GameObject Car in CarList)
            {
                Car.GetComponent<WayPointCar>().CarCamera.SetActive(false);
                Car.GetComponent<WayPointCar>().CurrentCamera.SetActive(true);

            }
        }
        else
        {
            foreach (GameObject Car in CarList)
            {
                Car.GetComponent<WayPointCar>().CarCamera.SetActive(true);
                Car.GetComponent<WayPointCar>().CurrentCamera.SetActive(false);
            }
        }
    }

    public static void RealCoords(float unityCof, float RealCofFor, ref float RealOX, ref float RealOY, ref float RealOZ)
    {

        RealOX = (RealOX / (3600)) * 160 + 45;
        RealOY = (RealOY / (3600)) * 160 + 45;
        RealOZ = (RealOZ / (3600)) * 160 + 45;


    }



}
