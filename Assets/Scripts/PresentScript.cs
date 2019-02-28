using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PresentScript : MonoBehaviour
{
    public TextMeshProUGUI TextBar1; // CarID
    public TextMeshProUGUI TextBar2; // OX
    public TextMeshProUGUI TextBar3; // OY
    public TextMeshProUGUI TextBar4;
    public TextMeshProUGUI TextBar5;
    public TextMeshProUGUI TextBar6;
    public TextMeshProUGUI TextBar7;
    public TextMeshProUGUI TextBar8;
    public TextMeshProUGUI TextBar9;


    public GameObject Camera1;
    public GameObject Camera2;

    public float Info1, Info2, Info3, Info4, Info5, X,Y,Z,CarId;
    
    // Start is called before the first frame update
    void Start()
    {
        CarId = 1;
    }

    // Update is called once per frame
    void Update()
    {
        TextBar1.text = ("CarID " + CarId);
        TextBar2.text = ("OX " + X.ToString("f2"));
        TextBar3.text = ("OY " + Y.ToString("f2"));
        TextBar4.text = ("OZ " + Z.ToString("f2"));
        TextBar5.text = ("Sensor 1 " + Info1.ToString("f2"));
        TextBar6.text =("Sensor 2 " + Info2.ToString("f2"));
        TextBar7.text = ("Sensor 3 " + Info3.ToString("f2"));
        TextBar8.text = ("Sensor 4 " + Info4.ToString("f2"));
        TextBar9.text = ("Sensor 5 " + Info5.ToString("f2"));

        if (Input.GetKeyDown(KeyCode.Alpha1))
            {
            Camera2.SetActive(false);
            Camera1.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            
            Camera2.SetActive(true);
            Camera1.SetActive(false);
        }

    }
}
