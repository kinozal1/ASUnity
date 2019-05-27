using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_Manager : MonoBehaviour
{
    private static Event_Manager _instance;

    public static Event_Manager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject Manager = new GameObject("Event_Manager");
                
                Manager.AddComponent<Event_Manager>();
            }

            return _instance;
        }
    }
    
    


    public CarInfo CurrentCar = new CarInfo();

    public class CarInfo
    {
        public string Name;
        public Transform Position;
        public int Car_id { get; set; }
        public float[] Data = new float[8];
        //OX, OY, OZ, Temp, Moisture, Compass, Forward, Backward;
        //Данные о текущей машине для меню 
    }



    void Awake()
    {
        _instance = this;
        
    }

   

}
