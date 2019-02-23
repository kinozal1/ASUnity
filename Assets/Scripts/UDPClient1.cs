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
    public GameObject TargetPointPrefab, CarPrefab; //Префабы для ТочкиСледования, Машины
    public float unityCof,RealCof;




    public struct CarData 
    {


        public DateTime Time1, Time2;
        public int CarID, Number1;

        public float Info1, Info2, Info3, Info4, Info5, OX, OY, OZ; 

        public CarData(string[] splitted)
        {
            CarID = int.Parse(splitted[0]);
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
    List<CarData> CarDataList; // Информация о всех машинах 
    List<GameObject> CarList; // Машины на карте
    List<NavMeshAgent> AgentPointList; // НавМешАгенты машин
    List<GameObject> TargetPointList; // ТочкиСледования для НавМеш


    


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

		if (received != ""){
            string[] splitted = received.Split(new string[] { "," }, StringSplitOptions.None);
            CarID = Int32.Parse(splitted[0]);

            if (CarID<CarDataList.Count)
                {
                RaycastHit hit;
                CarData CurrentCarData = new CarData(splitted);
                RealCoords(unityCof, RealCof, ref CurrentCarData.OX, ref CurrentCarData.OZ); //Подстройка координат
                CarDataList.Add(CurrentCarData);

                TargetPointList.Add(Instantiate(TargetPointPrefab)); // Добавление ТочкиСледования
                TargetPointList[CarID].transform.position = new Vector3(CurrentCarData.OX, 200, CurrentCarData.OZ); // Перемещение точки в координату из строки
                if (Physics.Raycast(TargetPointList[CarID].transform.position, -TargetPointList[CarID].transform.up, out hit)) //Проверка, есть ли точка на карте 
                {
                    TargetPointList[CarID].transform.position = hit.point; // Перемещние точки в координату, находящуюся на карте
                    NavMeshAgent Agent = new NavMeshAgent();
                    AgentPointList.Add(Agent); // Добавление агента в лист 

                    Instantiate(Agent, TargetPointList[CarID].transform.position, TargetPointList[CarID].transform.rotation); // Добавление агента на карту в координаты точки
                    CarList[CarID].GetComponent<WayPointCar>().CurrentWaypoint = Agent.gameObject; // Привязка агента к машине
                    AgentPointList[CarID].SetDestination(TargetPointList[CarID].transform.position); // Привязка агента к точке

                    CarList.Add(Instantiate(CarPrefab, TargetPointList[CarID].transform.position, TargetPointList[CarID].transform.rotation)); //Добавление машины на карту
                } // Добавление новых машин, их агентов, точек
            }
            else
            {
                
                CarData CurrentCarData = new CarData(splitted);
                CarDataList[CarID] = CurrentCarData;
                RaycastHit hit;
                if (Physics.Raycast(TargetPointList[CarID].transform.position, -TargetPointList[CarID].transform.up, out hit))  //Проверка, есть ли точка на карте 
                {
                    TargetPointList[CarID].transform.position = hit.point; // Перемещние точки в координату, находящуюся на карте
                    AgentPointList[CarID].SetDestination(TargetPointList[CarID].transform.position); // Переключение пути агента к точке
                }

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
        RealOX = (RealCofFor * (unityCof)) + 40;
        RealOZ = (RealCofFor * (unityCof)) + 40;
    }
		
}