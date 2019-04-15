using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoMenuController : MonoBehaviour
{
    public GameObject TruckWithLift;
    public GameObject JustTruck;

    public GameObject CurrentCar;
    public GameObject Point;
    public GameObject Menu;


    public int TypeOfCar;
    public GameObject[] Types;


    public string CurrentCarName;
    public float OX, OY, OZ;
    public float RPM,AngleOfLifter;

    public GameObject CurrentCamera;
    public GameObject NextCamera;

    public bool Kostil = true;
    public Text TextBar1;
    public Text TextBar2;
    public Text TextBar3;
    public Text TextBar4;
    public Text TextBar5;
    public Text TextBar6;
    public Text TextBar7;
    public Text TextBar8;
    public Text TextBar9;
    // Start is called before the first frame update
    public void OnEnable()
    {
        Kostil = true;
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
       if (Kostil)
        {
            for (int i=0; i<Types.Length;i++)
            {
                Types[i].SetActive(false);
            }
            Types[TypeOfCar].SetActive(true);
            Kostil = false;
        }

        Random rd = new Random();
        OX = CurrentCar.transform.position.x;
        OY = CurrentCar.transform.position.y;
        OZ = CurrentCar.transform.position.z;
        TextBar1.text = "Тестовая машина";
        TextBar2.text=OX.ToString("f2");
        TextBar3.text = OY.ToString("f2");
        TextBar4.text = OZ.ToString("f2");
        TextBar5.text = AngleOfLifter.ToString("f2");
        TextBar6.text = RPM.ToString("f2");
       

    }
  public void WhenClicked()
    {
        NextCamera.SetActive(true);
        CurrentCamera.SetActive(false);
    }
}
