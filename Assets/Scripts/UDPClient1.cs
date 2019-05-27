using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.Burst;
using UnityEngine;
using UnityEngine.AI;



public class UDPClient1 : MonoBehaviour
{



  

    public string StringFromGlobalMenu;
    public bool StartUseTextData = false;
    public bool StartWriteTextData = false;
    public bool UseTestString = false;
    public float frequency =2;
    private float time1, time2;
    public bool ChangeCameraBool = false;
    private CarData_Manager carDataManager;






    public DateTime Time1, Time2;

    public int CarID, Number1;


    public int portListen = 7777;
    public string ipSend = "";
    public int portSend = 5555;


    private string received = "";
    public string TestData;
    private UdpClient client;
    private Thread receiveThread;
    private IPEndPoint remoteEndPoint;
    private IPAddress ipAddressSend;


    public bool ReceiveByUDP;
    private void Start()
    {
        Debug.Log("starting script");
        carDataManager = GameObject.Find("CarData_Manager").GetComponent<CarData_Manager>();
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
            received = StringFromGlobalMenu;
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
            carDataManager.DataProcessing(splitted);

            










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
        Debug.Log("UDPClient: exit");
        if(client != null)
        {
            client.Close();
        }
        
        
    }

    

    public void DebugString(ref string[] splitted, string StringForDebug)
    {
        splitted = StringForDebug.Split(' ');
    }

    public void ChangeCamera()
    {
        ChangeCameraBool=! ChangeCameraBool;
        CarData_Manager.ChangeCamera(ChangeCameraBool);
        
        
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

    public void ClearData()
    {
        received = "";
        StringFromGlobalMenu = "";
        carDataManager.ClearData();
        


    }
}