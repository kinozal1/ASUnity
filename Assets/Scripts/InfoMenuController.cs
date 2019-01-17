using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoMenuController : MonoBehaviour
{
    public GameObject CurrentCar;
    public string CurrentCarName;
    private float OX, OY, OZ;
    public GameObject CurrentCamera;
    public GameObject NextCamera;

    public Text TextBar1;
    // Start is called before the first frame update
    private void OnEnable()
    {
        
        
        CurrentCar = GameObject.Find(CurrentCarName);
    }
    private void Awake()
    {

        
        NextCamera = GameObject.Find("MainCamera");
        CurrentCamera = GameObject.Find("Camera");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        OX = CurrentCar.transform.position.x;
        TextBar1.text=OX.ToString();
        
    }
  public void WhenClicked()
    {

        NextCamera.SetActive(true);
        CurrentCamera.SetActive(false);
        print("ItJustWorks!");
    }
    public void OnGUI()
    {
        
    }
}
