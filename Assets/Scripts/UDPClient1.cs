using UnityEngine;
using UnityEngine.UI;
using System.Collections;
     
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;



public class UDPClient1 : MonoBehaviour
{
    public DateTime Time1, Time2;

    public int Number1, OXint, OYint, OZint, Number2;
    public float OXfloat, OYfloat, OZfloat, Number3float, Number4float;
    public GameObject tear1;
    public GameObject tear2;
    public GameObject tear3;
    public GameObject tear4;
    public Text temp, humid, moving, lidar, compass;
    public GameObject samosval;
    private Double oldLidar;
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

		//Check if the ip address entered is valid. If not, sendMessage will broadcast to all ip addresses 
		IPAddress ip;
		if (IPAddress.TryParse (ipSend, out ip)) {

			remoteEndPoint = new IPEndPoint (ip, portSend);

		} else {

			remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, portSend);

		}

		//Initialize client and thread for receiving

		client = new UdpClient (portListen);

		receiveThread = new Thread (new ThreadStart (ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start ();
		
	}

	void Update ()
	{
	
		//Check if a message has been recibed
		if (received != ""){

          //  Debug.Log("UDPClient: message received \'" + received + "\'");
            string[] splitted = received.Split(new string[] { "," }, StringSplitOptions.None);
            //Notify each object defined in the array with the message received
            //foreach (GameObject g in notifyObjects)
            //{
            //    g.SendMessage(messageToNotify, received, SendMessageOptions.DontRequireReceiver);

            //}
            //Clear message
            Number1 = Int32.Parse(splitted[0]);
            OXint = Int32.Parse(splitted[1]);
            OYint = Int32.Parse(splitted[2]);
            OZint = Int32.Parse(splitted[3]);
            Time1 = DateTime.Parse(splitted[4]);
            Number2 = Int32.Parse(splitted[5]);
            Time2 = DateTime.Parse(splitted[6]);
            OXfloat = float.Parse(splitted[7]);
            OYfloat = float.Parse(splitted[8]);
            OZfloat = float.Parse(splitted[9]);
            Number3float = float.Parse(splitted[10]);
            Number4float = float.Parse(splitted[11]);


            Debug.Log(received);
            Debug.Log(OXint);
            Debug.Log(OYint);
            Debug.Log(OZint);
            Debug.Log(Time1);
            Debug.Log(Number2);
            Debug.Log(Time2);
            Debug.Log(OXfloat);
            Debug.Log(OYfloat);
            Debug.Log(OZfloat);
            Debug.Log(Number3float);
            Debug.Log(Number4float);





            if (oldLidar!= float.Parse(splitted[1])) {
                // tear1.transform.Rotate((tear1.transform.localEulerAngles.x+10)*Time.deltaTime(), 0, 0);
                // tear2.transform.Rotate((tear2.transform. * Time.deltaTime(), 0, 0);
                //  tear3.transform.Rotate(tear3.transform.localEulerAngles.x + 10, 0, 0);
                //  tear4.transform.Rotate(tear4.transform.localEulerAngles.x + 10, 0, 0);
            }
            oldLidar = float.Parse(splitted[1]);
            received = "";
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
		
}