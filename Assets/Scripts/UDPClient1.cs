using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;



public class UDPClient1 : MonoBehaviour
{
    public GameObject TargetPointPrefab, CarPrefab,AgentPrefab; //Префабы для ТочкиСледования, Машины
    public float unityCof,RealCof;

    public GameObject CurrentCamera;
    public GameObject NextCamera;

    public GameObject NavMeshSurf;
    public GameObject PresentScript;

    public struct CarData 
    {


        public DateTime Time1, Time2;
        public int CarID, Number1;

        public float Info1, Info2, Info3, Info4, Info5, OX, OY, OZ; 

        public CarData(string[] splitted)
        {
            CarID = int.Parse(splitted[0])-1;
            OX = float.Parse(splitted[1]);
            OY = float.Parse(splitted[2]);
            OZ = float.Parse(splitted[3]);
            Time1 = DateTime.Parse(splitted[4]);
            Number1 = int.Parse(splitted[5]);
            Time2 = DateTime.Parse(splitted[6]);
            Info1 = float.Parse(splitted[7]);
            Info2 = float.Parse(splitted[8]);
            Info3 = float.Parse(splitted[9]);
            Info4 = float.Parse(splitted[10]);
            Info5 = float.Parse(splitted[11]);
        }
    } // Вся информация о машине

   
    public List<GameObject> CarList = new List<GameObject>(0); // Машины на карте
    public List<GameObject> AgentPointList = new List<GameObject>(0); // НавМешАгенты машин
    public List<GameObject> TargetPointList = new List<GameObject>(0); // ТочкиСледования для НавМеш
    public List<CarData> CarDataList = new List<CarData>(0);  // Информация о всех машинах 


    public string TestString;

    public DateTime Time1, Time2;

    public int CarID, Number1;

    public float Info1, Info2, Info3, Info4, Info5,OX, OY, OZ;

	public int portListen = 8051;
	public string ipSend = "";
	public int portSend = 5555;

	public GameObject[]  notifyObjects;
	public string messageToNotify;

	private string received = "";
	
	private UdpClient client;
	private Thread receiveThread;
	private IPEndPoint remoteEndPoint;
	private IPAddress ipAddressSend;
    void Start()
    {
        Debug.Log("starting script");
    }

	public void Awake ()
	{

		IPAddress ip;
		if (IPAddress.TryParse (ipSend, out ip)) {

			remoteEndPoint = new IPEndPoint (ip, portSend);

		} else {

			remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, portSend);

		}


		client = new UdpClient (portListen);

		receiveThread = new Thread (new ThreadStart (ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start ();
		
	}

	void Update ()
	{
        received = TestString;

        if ((received != "")){

            string[] splitted = received.Split(new string[] { "," }, StringSplitOptions.None);
            CarID = Int32.Parse(splitted[0]);
            
            if (CarID>CarDataList.Count)
                {
                CarID--;
                RaycastHit hit;
                CarData CurrentCarData = new CarData(splitted);
                
                RealCoords(unityCof, RealCof, ref CurrentCarData.OX, ref CurrentCarData.OZ); //Подстройка координат 
                CarDataList.Add(CurrentCarData);

                TargetPointList.Add(Instantiate(TargetPointPrefab, new Vector3(CurrentCarData.OX, 250, CurrentCarData.OZ), Quaternion.Euler(0, 0, 0))); // Добавление ТочкиСледования
                if (((Physics.Raycast(TargetPointList[CarID].transform.position, -TargetPointList[CarID].transform.up, out hit)))) //Проверка, есть ли точка на карте 
                {

                    AgentPointList.Add(Instantiate(AgentPrefab, hit.point, Quaternion.Euler(hit.normal)));

                    AgentPointList[CarID].GetComponent<NavMeshAgent>().Warp(hit.point);
                    Debug.Log(hit.point);
                    TargetPointList[CarID].transform.position = hit.point; // Перемещние точки в координату, находящуюся на карте


                    // Добавление агента на карту в координаты точки
                    CarList.Add(Instantiate(CarPrefab, new Vector3(AgentPointList[CarID].transform.position.x, AgentPointList[CarID].transform.position.y + 5, AgentPointList[CarID].transform.position.z), AgentPointList[CarID].transform.rotation)); //Добавление машины на карту

                    CarList[CarID].GetComponent<WayPointCar>().CurrentWaypoint = AgentPointList[CarID].gameObject; // Привязка агента к машине
                    CarList[CarID].GetComponent<WayPointCar>().CurrentCamera = CurrentCamera;
                    CarList[CarID].GetComponent<WayPointCar>().NextCamera = NextCamera;


                    PresentScript.GetComponent<PresentScript>().Camera1 = CarList[CarID].transform.Find("CarCamera").gameObject;




                } // Добавление новых машин, их агентов, точек
            }
            else
            {
                CarID--;
                CarData CurrentCarData = new CarData(splitted);
                RealCoords(unityCof, RealCof, ref CurrentCarData.OX, ref CurrentCarData.OZ);
              

                CarDataList[CarID] = CurrentCarData;
                RaycastHit hit;
                TargetPointList[CarID].transform.position = new Vector3(CurrentCarData.OX, 200, CurrentCarData.OZ);
                if (Physics.Raycast(TargetPointList[CarID].transform.position, -TargetPointList[CarID].transform.up, out hit))  //Проверка, есть ли точка на карте 
                {
                   
                    TargetPointList[CarID].transform.position = hit.point; // Перемещние точки в координату, находящуюся на карте
                   // AgentPointList[CarID].GetComponent<NavMeshAgent>().SetDestination(TargetPointList[CarID].transform.position); // Переключение пути агента к точке
                }

                PresentScript.GetComponent<PresentScript>().X = CarList[CarID].transform.position.x;
                PresentScript.GetComponent<PresentScript>().Y = CarList[CarID].transform.position.y;
                PresentScript.GetComponent<PresentScript>().Z = CarList[CarID].transform.position.z;
                PresentScript.GetComponent<PresentScript>().Info1 = CarList[CarID].GetComponent<WayPointCar>().LFLidarDistance;
                PresentScript.GetComponent<PresentScript>().Info2 = CarList[CarID].GetComponent<WayPointCar>().LeftLidarDistance;
                PresentScript.GetComponent<PresentScript>().Info3 = CarList[CarID].GetComponent<WayPointCar>().RFLidarDistance;
                PresentScript.GetComponent<PresentScript>().Info4 = CarList[CarID].GetComponent<WayPointCar>().RFLidarDistance;
                PresentScript.GetComponent<PresentScript>().Info5 = CarList[CarID].GetComponent<WayPointCar>().one;

                AgentPointList[CarID].GetComponent<NavMeshAgentPoint>().cam = CurrentCamera.GetComponent<Camera>();






            } // Обновление данных о машинах и их точек следования
            







           
		}
	}

	//Call this method to send a message from this app to ipSend using portSend
	public void SendValue (string valueToSend)
	{
		try {
			if (valueToSend != "") {

				//Get bytes from string
				byte[] data = Encoding.UTF8.GetBytes (valueToSend);

				// Send bytes to remote client
				client.Send (data, data.Length, remoteEndPoint);
				Debug.Log ("UDPClient: send \'" + valueToSend + "\'");
				//Clear message
				valueToSend = "";
	
			}
		} catch (Exception err) {
			Debug.LogError ("Error udp send : " + err.Message);
		}
	}

	//This method checks if the app receives any message
	public void ReceiveData ()
	{
 
		while (true) {

			try
            {
				// Bytes received
				IPEndPoint anyIP = new IPEndPoint (IPAddress.Any, 0);
				byte[] data = client.Receive (ref anyIP);

				// Bytes into text
				string text = "";
				text = Encoding.UTF8.GetString (data);
	
                received = text;		
       
			}
            catch (Exception err)
            {
				Debug.Log ("Error:" + err.ToString ());
			}
		}
	}
		
	//Exit UDP client
	public void OnDisable ()
	{
		if (receiveThread != null) {
				receiveThread.Abort ();
				receiveThread = null;
		}
		client.Close ();
		Debug.Log ("UDPClient: exit");
	}

    public void RealCoords(float unityCof,float RealCofFor,ref float RealOX,ref float RealOZ)
    {
        RealOX = (RealOX / (unityCof)) + 40;
        RealOZ = (RealOZ / (unityCof)) + 40;
    }

    public void DebugString(ref string[] splitted,string StringForDebug)
    {
        splitted = StringForDebug.Split(' ');
    }
		
}