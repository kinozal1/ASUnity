using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;



public class UDPClient1 : MonoBehaviour
{
    public GameObject TargetPointPrefab, CarPrefab, AgentPrefab; //Префабы для ТочкиСледования, Машины
    public float unityCof, RealCof;

    public GameObject GlobalMenu;

    public GameObject CurrentCamera;
    public GameObject NextCamera;

    public GameObject NavMeshSurf;

    public GameObject Points;

    public string StringFromGlobalMenu;
    public bool StartUseTextData = false;
    public bool StartWriteTextData = false;
    public bool UseTestString = false;
    public float frequency =2;
    private float time1, time2;
    public bool ChangeCameraBool = false;
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


    public List<GameObject> CarList = new List<GameObject>(0); // Машины на карте
    public List<GameObject> AgentPointList = new List<GameObject>(0); // НавМешАгенты машин
    public List<GameObject> TargetPointList = new List<GameObject>(0); // ТочкиСледования для НавМеш
    public List<CarData> CarDataList = new List<CarData>(0);  // Информация о всех машинах 



    public DateTime Time1, Time2;

    public int CarID, Number1;

    public float Info1, Info2, Info3, Info4, Info5, OX, OY, OZ;

    public int portListen = 7777;
    public string ipSend = "";
    public int portSend = 5555;

    public GameObject[] notifyObjects;
    public string messageToNotify;

    private string received = "";
    public string TestData;
    private UdpClient client;
    private Thread receiveThread;
    private IPEndPoint remoteEndPoint;
    private IPAddress ipAddressSend;
    WorkWithPoints workWithPoints;

    public bool ReceiveByUDP;
    private void Start()
    {
        workWithPoints = gameObject.GetComponent<WorkWithPoints>();
        Debug.Log("starting script");
    }

    public void Awake()
    {

        //IPAddress ip;
        //if (IPAddress.TryParse(ipSend, out ip))
        //{

        //    remoteEndPoint = new IPEndPoint(ip, portSend);

        //}
        //else
        //{

        //    remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, portSend);

        //}


        //client = new UdpClient(portListen);
        //time1 = Time.time * 1000;
        //time2= Time.time * 1000;
        //receiveThread = new Thread(new ThreadStart(ReceiveData));
        
        //receiveThread.IsBackground = true;
        //receiveThread.Start(frequency);

    }

    private void Update()
    {
        
        if(UseTestString)
        {
            received = StringFromGlobalMenu;
        }
        if (StartUseTextData) // Прием данных из файла
        {
            if(StringFromGlobalMenu != null || StringFromGlobalMenu!="")
            {
                received = StringFromGlobalMenu;
            }
           
        }
        else if (StartWriteTextData) // Запись данных в файл
        {
            //string s = StringFromGlobalMenu;
            StringFromGlobalMenu = (received + "|" + frequency.ToString());
            //StringFromGlobalMenu = s;



        }
        if (received != "")
        {
            
            string[] splitted = received.Split(new string[] { "," }, StringSplitOptions.None);
            
                CarID = Int32.Parse(splitted[0]);

                if (CarID > CarDataList.Count)
                {
                    CarID--;

                    CarData CurrentCarData = new CarData(splitted);

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
                    CarData CurrentCarData = new CarData(splitted);
                    RealCoords(unityCof, RealCof, ref CurrentCarData.OZ, ref CurrentCarData.OX, ref CurrentCarData.OY);
                    Vector3 CurrentPoint = new Vector3(CurrentCarData.OX, 0, CurrentCarData.OZ);
                   
                    workWithPoints.KNN(ref CurrentPoint);
                    CarDataList[CarID] = CurrentCarData;
                    TargetPointList[CarID].transform.position = CurrentPoint;
                    AgentPointList[CarID].GetComponent<NavMeshAgentPoint>().cam = CurrentCamera.GetComponent<Camera>();
                } // Обновление данных о машинах и их точек следования
            









        }
    }

    //Call this method to send a message from this app to ipSend using portSend
    public void SendValue(string valueToSend)
    {
        try
        {
            if (valueToSend != "")
            {

                //Get bytes from string
                byte[] data = Encoding.UTF8.GetBytes(valueToSend);

                // Send bytes to remote client
                client.Send(data, data.Length, remoteEndPoint);
                Debug.Log("UDPClient: send \'" + valueToSend + "\'");
                //Clear message
                valueToSend = "";

            }
        }
        catch (Exception err)
        {
            Debug.LogError("Error udp send : " + err.Message);
        }
    }

    //This method checks if the app receives any message
    public void ReceiveData()
    {

        while (true)
        {

            try
            {
                time1 = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                // Bytes received
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // Bytes into text
                string text = "";
                text = Encoding.UTF8.GetString(data);

                received = text;
                Debug.Log(received);
                time2 = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

                if (time2>time1)
                {
                    frequency = (time2 - time1) / 1000;
                }
                else
                {
                    time2 += 60000; 
                    frequency = (time2 - time1) / 1000;
                }


            }
            catch (Exception err)
            {
                Debug.Log("Error:" + err.ToString());
            }
            finally
            {
                
            }
        }
        
    }

    //Exit UDP client
    public void OnDisable()
    {
        if (receiveThread != null)
        {
            receiveThread.Abort();
            receiveThread = null;
        }
        client.Close();
        Debug.Log("UDPClient: exit");
    }

    public void RealCoords(float unityCof, float RealCofFor, ref float RealOX, ref float RealOY, ref float RealOZ)
    {

        RealOX = (RealOX / (3600)) * 160 + 45;
        RealOY = (RealOY / (3600)) * 160 + 45;
        RealOZ = (RealOZ / (3600)) * 160 + 45;

      
    }

    public void DebugString(ref string[] splitted, string StringForDebug)
    {
        splitted = StringForDebug.Split(' ');
    }

    public void ChangeCamera()
    {
        ChangeCameraBool=! ChangeCameraBool;
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

    public void TurnOnUDP()
    {
        ReceiveByUDP = !ReceiveByUDP;
        if (ReceiveByUDP)
        {

            IPAddress ip;
            if (IPAddress.TryParse(ipSend, out ip))
            {

                remoteEndPoint = new IPEndPoint(ip, portSend);

            }
            else
            {

                remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, portSend);

            }

           if ( client == null)
            {
                client = new UdpClient(portListen);
            }
            
            time1 = Time.time * 1000;
            time2 = Time.time * 1000;
            receiveThread = new Thread(new ThreadStart(ReceiveData));

            receiveThread.IsBackground = true;
            receiveThread.Start(frequency);
            ReceiveByUDP = !ReceiveByUDP;
        }
        else
        {
            
           
            receiveThread.Abort();
            ClearData();
           
        }
    }

    void ClearData()
    {
        CarList.Clear();
        AgentPointList.Clear(); // НавМешАгенты машин
        TargetPointList.Clear(); // ТочкиСледования для НавМеш
        CarDataList.Clear(); // Информация о всех машинах 
    }
}