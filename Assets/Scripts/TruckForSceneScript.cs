using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckForSceneScript : MonoBehaviour
{
    private GameObject Car;
    private GameObject CurrentCarLifter;

    public GameObject Lifter;

    public GameObject[] CurrentCarWheels = new GameObject[] { null, null, null, null };
    public GameObject[] Wheels;

    private string[] WheelsNames = new string[] { "BR", "BL", "FL", "FR" };

    public bool Kostil = true;
    // Use this for initialization
    private void OnEnable()
    {

        Kostil = true;

    }

    void Start()
    {



    }




    void FixedUpdate()
    {
        if (Kostil)
        {
            int i = 0;
            Car = GameObject.Find("InfoMenu").GetComponent<InfoMenuController>().CurrentCar;

            CurrentCarLifter = GetChildWithName(Car, "Lifter");
            foreach (string name in WheelsNames)
            {
                CurrentCarWheels[i] = GetGrandChildWithName(Car, "WheelsMesh", name);
                i++;
            }
            i = 0;
            Kostil = false;
        }
        for (int i = 0; i < 4; i++)
        {
            Wheels[i].transform.localRotation = CurrentCarWheels[i].transform.localRotation;
            Wheels[i].transform.Rotate(15, 0, 0);
        }
        Lifter.transform.localRotation = CurrentCarLifter.transform.localRotation;
    }

    GameObject GetChildWithName(GameObject obj, string name)
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null)
        {
            return childTrans.gameObject;
        }
        else
        {
            return null;
        }
    }

    GameObject GetGrandChildWithName(GameObject obj, string child, string childname)
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(child).Find(childname);
        if (childTrans != null)
        {
            return childTrans.gameObject;
        }
        else
        {
            return null;
        }
    }
}
