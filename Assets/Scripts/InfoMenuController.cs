using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoMenuController : MonoBehaviour
{
    public GameObject TruckWithLift;
    public GameObject JustTruck;

    public GameObject CurrentCar;
    public GameObject Menu;


    public int TypeOfCar;
    public GameObject[] Types;



    public float OX, OY, OZ;


    public GameObject CurrentCamera;
    public GameObject NextCamera;

    public bool TypesChanger = true;
    public Text TextBar1;
    public Text TextBar2;
    public Text TextBar3;
    public Text TextBar4;
    public Text TextBar5;
    public Text TextBar6;

    // Start is called before the first frame update
    public void OnEnable()
    {
        TypesChanger = true;
        Menu.SetActive(false);
    }
    public void OnDisable()
    {
        Menu.SetActive(true);
    }

    private void Awake()
    {


    }
    void Start()
    {
        CurrentCamera.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       if (TypesChanger)
        {
            for (int i=0; i<Types.Length;i++)
            {
                Types[i].SetActive(false);
            }
            Types[TypeOfCar].SetActive(true);
            TypesChanger = false;
        }


        Random rd = new Random();
        OX = Event_Manager.Instance.CurrentCar.Position.position.x;
        OY = Event_Manager.Instance.CurrentCar.Position.position.y;
        OZ = Event_Manager.Instance.CurrentCar.Position.position.z;
        TextBar1.text = "Тестовая машина";
        TextBar2.text= Event_Manager.Instance.CurrentCar.Data[0].ToString("f2");
        TextBar3.text = Event_Manager.Instance.CurrentCar.Data[1].ToString("f2");
        TextBar4.text = Event_Manager.Instance.CurrentCar.Data[2].ToString("f2");
        TextBar5.text = Event_Manager.Instance.CurrentCar.Data[3].ToString("f2");
        TextBar6.text = Event_Manager.Instance.CurrentCar.Data[4].ToString("f2");


    }
  public void WhenClicked()
    {
        NextCamera.SetActive(true);
        CurrentCamera.SetActive(false);
    }
}
