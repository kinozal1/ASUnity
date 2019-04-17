using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using UnityEngine.EventSystems;


public class GlobalMenu : MonoBehaviour
{
   
    public GameObject MainCamera;
    public GameObject UDPCLIENT;
    public GameObject MainMenu;
    public GameObject Menu;
    public GameObject UDPSettings;
    public GameObject CarSettings;
    public GameObject EnterButton;
    public GameObject ButtonPrefab;
    public GameObject Container;
    public GameObject OpenFileDialog;

    public InputField ListenPortField;
    public InputField SendPortField;
    public InputField IPField;
    public InputField TestStringField;
    public InputField DistanceField;

    public Slider MotorTorque;
    public Slider Angle;
    public Slider Break;
    public Slider FDistance;
    public Slider SDistance;
    public Slider NDistance;

    public Slider ExtremumSlipSlider;
    public Slider ExtremumValueSlider;
    public Slider AsymptoteSlipSlider;
    public Slider AsymptoteValueSlider;
    public Slider StiffnessSlider;

    public Text MotorTorqueText;
    public Text AngleText;
    public Text BreakText;
    public Text FDistanceText;
    public Text SDistanceText;
    public Text NDistanceText;

    public Text ExtremumSlipText;
    public Text ExtremumValueText;
    public Text AsymptoteSlipText;
    public Text AsymptoteValueText;
    public Text StiffnessText;

    public Text DataWriter;
    public Text DataReader;

    public Text TestStringBool;

    public Text ForawrdSideways;



    public bool StartDataWrite = false;
    public bool StartDataRead = false;
    public bool ClearData = false;
    public bool DirectionBool = true;
    public bool ReadCoroutineIsStarted = false;
    public bool WriteCoroutineIsStarted = false;
    public bool UseTestString = false;
    public bool ChangeValues = false;

    public string pathToWrite="LogsForData/Log.txt";
    public string pathToRead;

    public float frequency=4;
    public StreamWriter SW;
    public StreamReader SR;
    Saver saver;
    public string StringForReadAndWrite;


    public bool SideForward=true;
    // Start is called before the first frame update

    private void Awake()
    {
       
    }
    void Start()
    {
      if(!Directory.Exists("LogsForData"))
        {
            Directory.CreateDirectory("LogsForData");
        }
    }


    // Update is called once per frame
    void Update()
    {

        if (StartDataWrite)
        {

            if(!WriteCoroutineIsStarted)
            {
                StartCoroutine(WriteWithTime(StringForReadAndWrite));
            
            }


            //StringForReadAndWrite = UDPCLIENT.GetComponent<UDPClient1>().StringFromGlobalMenu;
            //if (StringForReadAndWrite != null)
            //{
            //    SW.WriteLine(StringForReadAndWrite);
            //}
           
        }
        else if( StartDataRead)
        {
            if (!ReadCoroutineIsStarted)
            {
                StartCoroutine(ReadWithTime());
            }
            
            
        }
        else if(ClearData)
        {
            File.Delete(pathToWrite);
            ClearData = false;
        }
       
    }
    public void EnterToMenu()
    {
        
        EnterButton.SetActive(false);
        MainMenu.SetActive(true);
        Menu.SetActive(true);
        UDPSettings.SetActive(false);
        CarSettings.SetActive(false);
    }
    public void ExitFromMenu()
    {
        EnterButton.SetActive(true);
        MainMenu.SetActive(false);
        Menu.SetActive(false);
        UDPSettings.SetActive(false);
        CarSettings.SetActive(false);
    }
    public void EnterToUDPSettingsMenu()
    {
        

        MainMenu.SetActive(false);
        CarSettings.SetActive(false);
        UDPSettings.SetActive(true);

        //ListenPortField.text = UDPclient.portListen.ToString();
        //SendPortField.text = UDPclient.portSend.ToString();
        //IPField.text = UDPclient.ipSend.ToString();
        //TestStringField.text = UDPclient.TEST.ToString();
        //DistanceField.text = UDPclient.RealCof.ToString();
    }
    public void ExitFromUDPSettingsMenu()
    {
        MainMenu.SetActive(true);
        UDPSettings.SetActive(false);
    }
    public void RefreshScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ExitFromApp()
    {
        UnityEngine.Application.Quit();
    }
    public void EnterToCarSettingsMenu()
    {
        MainMenu.SetActive(false);
        CarSettings.SetActive(true);

        try
        {
       
                MotorTorqueText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().maxAccel.ToString();
                AngleText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().maxSteer.ToString();
                BreakText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().maxBrake.ToString(); 
                FDistanceText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().DistanceForForwardLidars.ToString();
                SDistanceText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().DistanceForSideLidars.ToString();

                ExtremumSlipText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.extremumSlip.ToString();
                ExtremumValueText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.extremumValue.ToString();
                AsymptoteSlipText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.asymptoteSlip.ToString();
                AsymptoteValueText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.asymptoteValue.ToString();
                StiffnessText.text = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.stiffness.ToString();

                MotorTorque.value = (int)(UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().maxAccel / 100);
                Angle.value = (int)(UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().maxSteer);
                Break.value = (int)(UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().maxBrake / 200);
                FDistance.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().DistanceForForwardLidars;
                SDistance.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().DistanceForSideLidars;

                ExtremumSlipSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.extremumSlip;
                ExtremumValueSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.extremumValue;
                AsymptoteSlipSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.asymptoteSlip;
                AsymptoteValueSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.asymptoteValue;
                StiffnessSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.stiffness;
            

        }

        catch (IndexOutOfRangeException)
        {
            MotorTorqueText.text = "НетДанных";
            AngleText.text = "НетДанных";
            BreakText.text = "НетДанных";
            FDistanceText.text = "НетДанных";
            SDistanceText.text = "НетДанных";
            ExtremumSlipText.text = "НетДанных";
            ExtremumValueText.text = "НетДанных";
            AsymptoteSlipText.text = "НетДанных";
            AsymptoteValueText.text = "НетДанных";
            StiffnessText.text = "НетДанных";

        }
        finally
        {
            ChangeValues = true;
        }


    }
    public void ExitFromCarSettingsMenu()
    {
        MainMenu.SetActive(true);
        CarSettings.SetActive(false);
        ChangeValues = false;
    }
    public void ListenPortChange(string s)
    {
        UDPCLIENT.GetComponent<UDPClient1>().portListen = Convert.ToInt32(s);
        
    }
    public void SendPortChange(string s)
    {
        UDPCLIENT.GetComponent<UDPClient1>().portSend = Convert.ToInt32(s);
     
    }
    public void IPChange(string s)
    {
        UDPCLIENT.GetComponent<UDPClient1>().portSend = Convert.ToInt32(s);
        
    }
    public void TestStringChange(string s)
    {
        if (UseTestString)
        {
            UDPCLIENT.GetComponent<UDPClient1>().UseTestString = true;
            UDPCLIENT.GetComponent<UDPClient1>().StringFromGlobalMenu = (s);
        }
        else
        {
            UDPCLIENT.GetComponent<UDPClient1>().UseTestString = false;
        }
       
       
    }
    public void DistanceChange(string s)
    {
        UDPCLIENT.GetComponent<UDPClient1>().RealCof = Convert.ToInt32(s);
      
    }


    public void MotorTorqueChange()
    {
        if (ChangeValues)
        {
            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                Car.GetComponent<WayPointCar>().maxAccel = MotorTorque.value * 100;
            }
            MotorTorqueText.text = (MotorTorque.value * 100).ToString();
        }
    }
    public void AngleChange()
    {
        if (ChangeValues)
        {
            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                Car.GetComponent<WayPointCar>().maxSteer = Angle.value;
            }
            AngleText.text = (Angle.value).ToString();
        }
    }
    public void BreakChange()
    {
        if (ChangeValues)
        {
            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                Car.GetComponent<WayPointCar>().maxBrake = Break.value * 200;
            }
            BreakText.text = (Break.value * 200).ToString();
        }
    }

    public void FDistanceChange()
    {
        if (ChangeValues)
        {
            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                Car.GetComponent<WayPointCar>().maxBrake = FDistance.value;
            }
            FDistanceText.text = (FDistance.value).ToString();
        }
    }
    public void SDistanceChange()
    {
        if (ChangeValues)
        {
            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                Car.GetComponent<WayPointCar>().DistanceForSideLidars = SDistance.value;
            }
            SDistanceText.text = (SDistance.value).ToString();
        }
    }
    public void NDistanceChange(int s)
    {
        if (ChangeValues)
        {
            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                Car.GetComponent<WayPointCar>().DistanceForNormalMove = NDistance.value;
            }
            NDistanceText.text = (NDistance.value).ToString();
        }
    }


    public void ExtremumSlipChange()
    {
        if (ChangeValues)
        {

                foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
                {
                    foreach (WheelCollider w in Car.GetComponent<WayPointCar>().WColForward)
                    {
                        if (SideForward)
                        {
                            WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                            wheelFriction = w.sidewaysFriction;
                            wheelFriction.extremumSlip = ExtremumSlipSlider.value;
                            w.sidewaysFriction = wheelFriction;
                        Debug.Log(wheelFriction.extremumSlip);
                    }
                        else
                        {

                            WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                            wheelFriction = w.forwardFriction;
                            wheelFriction.extremumSlip = ExtremumSlipSlider.value;
                            w.forwardFriction = wheelFriction;
                        Debug.Log(wheelFriction.extremumSlip);
                    }
                    }
                    
                }
                ExtremumSlipText.text = (ExtremumSlipSlider.value).ToString();
        }

    }

    public void ExtremumValueChange()
    {
        if (ChangeValues)
        {

            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                foreach (WheelCollider w in Car.GetComponent<WayPointCar>().WColForward)
                {
                    if (SideForward)
                    {
                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.sidewaysFriction;
                        wheelFriction.extremumValue = ExtremumValueSlider.value;
                        w.sidewaysFriction = wheelFriction;
                    }
                    else
                    {

                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.forwardFriction;
                        wheelFriction.extremumValue = ExtremumValueSlider.value;
                        w.forwardFriction = wheelFriction;
                    }
                }
            }
            ExtremumValueText.text = (ExtremumValueSlider.value).ToString();
        }

    }

    public void StriffnesChanged()
    {
        if (ChangeValues)
        {

            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                foreach (WheelCollider w in Car.GetComponent<WayPointCar>().WColForward)
                {
                    if (SideForward)
                    {
                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.sidewaysFriction;
                        wheelFriction.stiffness = StiffnessSlider.value;
                        w.sidewaysFriction = wheelFriction;
                    }
                    else
                    {

                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.forwardFriction;
                        wheelFriction.stiffness = StiffnessSlider.value;
                        w.forwardFriction = wheelFriction;
                    }
                }
            }
            StiffnessText.text = (StiffnessSlider.value).ToString();
        }

    }

    public void AsymptoteSlipChange()
    {
        if (ChangeValues)
        {

            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                foreach (WheelCollider w in Car.GetComponent<WayPointCar>().WColForward)
                {
                    if (SideForward)
                    {
                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.sidewaysFriction;
                        wheelFriction.asymptoteSlip = AsymptoteSlipSlider.value;
                        w.sidewaysFriction = wheelFriction;
                        
                    }
                    else
                    {

                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.forwardFriction;
                        wheelFriction.asymptoteSlip = AsymptoteSlipSlider.value;
                        w.forwardFriction = wheelFriction;

                    }
                }
               
            }
            AsymptoteSlipText.text = (AsymptoteSlipSlider.value).ToString();
        }

    }

    public void AsymptoteValueChange()
    {
        if (ChangeValues)
        {
            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                foreach (WheelCollider w in Car.GetComponent<WayPointCar>().WColForward)
                {
                    if (SideForward)
                    {
                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.sidewaysFriction;
                        wheelFriction.asymptoteValue = AsymptoteValueSlider.value;
                        w.sidewaysFriction = wheelFriction;
                    }
                    else
                    {

                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.forwardFriction;
                        wheelFriction.asymptoteValue = AsymptoteValueSlider.value;
                        w.forwardFriction = wheelFriction;
                    }
                }
            }
            AsymptoteValueText.text = (AsymptoteValueSlider.value).ToString();
        }

    }

    public void SliderChange() //EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text; 
        //Проверка слайдера
        //Изменение значения текста
        //Привязка текстов к слайдерам через блок канваса
        //
    {

        if(ChangeValues)
        {
            foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList)
            {
                foreach (WheelCollider w in Car.GetComponent<WayPointCar>().WColForward)
                {
                    if (SideForward)
                    {
                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.sidewaysFriction;
                        wheelFriction.asymptoteValue = AsymptoteValueSlider.value;
                        w.sidewaysFriction = wheelFriction;
                    }
                    else
                    {

                        WheelFrictionCurve wheelFriction = new WheelFrictionCurve();
                        wheelFriction = w.forwardFriction;
                        wheelFriction.asymptoteValue = AsymptoteValueSlider.value;
                        w.forwardFriction = wheelFriction;
                    }
                }
            }
            AsymptoteValueText.text = (AsymptoteValueSlider.value).ToString();
        }
    }
    public void ChangeSideAndForward()
    {
        SideForward = !SideForward;
        if (SideForward)
        {
            ChangeValues = false;
            ExtremumSlipSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].sidewaysFriction.extremumSlip;
            ExtremumValueSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].sidewaysFriction.extremumValue;
            AsymptoteSlipSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].sidewaysFriction.asymptoteSlip;
            AsymptoteValueSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].sidewaysFriction.asymptoteValue;
            StiffnessSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].sidewaysFriction.stiffness;

            ExtremumSlipText.text = ExtremumSlipSlider.value.ToString();
            ExtremumValueText.text = ExtremumValueSlider.value.ToString();
            AsymptoteSlipText.text = AsymptoteSlipSlider.value.ToString();
            AsymptoteValueText.text = AsymptoteValueSlider.value.ToString();
            StiffnessText.text = StiffnessSlider.value.ToString();

            ChangeValues = true;
            ForawrdSideways.text = "Боковое сопротивление";
        }
        else
        {
            ChangeValues = false;
            ExtremumSlipSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.extremumSlip;
            ExtremumValueSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.extremumValue;
            AsymptoteSlipSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.asymptoteSlip;
            AsymptoteValueSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.asymptoteValue;
            StiffnessSlider.value = UDPCLIENT.GetComponent<UDPClient1>().CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.stiffness;

            ExtremumSlipText.text = ExtremumSlipSlider.value.ToString();
            ExtremumValueText.text = ExtremumValueSlider.value.ToString();
            AsymptoteSlipText.text = AsymptoteSlipSlider.value.ToString();
            AsymptoteValueText.text = AsymptoteValueSlider.value.ToString();
            StiffnessText.text = StiffnessSlider.value.ToString();

            ChangeValues = true;
            ForawrdSideways.text = "Переднее сопротивление";
        }
    }




        public void ButtonToUseTestString()
    {
        UseTestString = !UseTestString;
        if (UseTestString)
        {
            UDPCLIENT.GetComponent<UDPClient1>().UseTestString = true;
            UDPCLIENT.GetComponent<UDPClient1>().StringFromGlobalMenu = TestStringField.text;
            TestStringBool.text = "ВКЛ";
        }
        else
        {
            UDPCLIENT.GetComponent<UDPClient1>().UseTestString = false;
            TestStringBool.text = "ВЫКЛ";
        }
    }


    public void DataRead()
    {
        StartDataWrite = false;
        DataWriter.text = "Запись остановлена";
        StartDataRead = !StartDataRead;
        if (StartDataRead)
        {
            MainCamera.GetComponent<camera>().enabled = false;
            OpenFileDialog.SetActive(true);
            string[] allfiles = Directory.GetFiles("LogsForData", "*.txt*");
            for (int i = 0; i < Container.transform.childCount; i++)
            {
                Destroy(Container.transform.GetChild(i).gameObject);
            }
            foreach(string filename in allfiles)
            {
                GameObject NewButton = Instantiate(ButtonPrefab, Container.transform);
                NewButton.GetComponentInChildren<Text>().text = filename;
                NewButton.GetComponent<Button>().onClick.AddListener(ChoosePath);
            }
            //if (SW != null)
            //{
            //    SW.Flush();
            //    SW.Close();
            //}
            
        }
        else
        {
            frequency = 0;
            SR.Dispose();
            SR = null;
            UDPCLIENT.GetComponent<UDPClient1>().ClearData();
            UDPCLIENT.GetComponent<UDPClient1>().StartUseTextData = false;
            UDPCLIENT.GetComponent<UDPClient1>().StartWriteTextData = false;
            DataReader.text = "Чтение остановлено";
            StartDataRead = false;
        }
       
    }
    public void DataWrite()
    {

        StartDataRead = false;
        DataReader.text = "Чтение остановлено";
        StartDataWrite = !StartDataWrite;
        if (StartDataWrite)
        {
            pathToWrite ="LogsForData/Log.txt";
            int indexforfile = 0;
           
           while (File.Exists(string.Format("LogsForData/Log{0}.txt", indexforfile)))
            {
                indexforfile++;
            }
            pathToWrite = string.Format("LogsForData/Log{0}.txt", indexforfile);
            if (pathToWrite!=null)
            {
                SW = new StreamWriter(pathToWrite, true);
                UDPCLIENT.GetComponent<UDPClient1>().StartUseTextData = false;
                UDPCLIENT.GetComponent<UDPClient1>().StartWriteTextData = true;
                DataWriter.text = "Запись идет";
                DataReader.text = "Чтение остановлено";
            }
            else
            {
                StartDataWrite = !StartDataWrite;
            }
            
        }
        else
        {
            SW.Flush();
            SW.Close();

            UDPCLIENT.GetComponent<UDPClient1>().StartUseTextData = false;
            UDPCLIENT.GetComponent<UDPClient1>().StartWriteTextData = false;
            DataWriter.text = "Запись остановлена";
            StartDataRead = false;
        }
    }
    public void ClearLogData()
    {
        ClearData = true;
    }

    IEnumerator ReadWithTime()
    {
        if (SR != null)
        {
            ReadCoroutineIsStarted = true;


            StringForReadAndWrite = SR.ReadLine();
            if (StringForReadAndWrite != null)
            {
                string[] s = StringForReadAndWrite.Split('|');
                StringForReadAndWrite = s[0];
                frequency = float.Parse(s[1]);
                yield return new WaitForSeconds(frequency);
                UDPCLIENT.GetComponent<UDPClient1>().StringFromGlobalMenu = StringForReadAndWrite;


            }
            else
            {
                DataRead();
            }


            ReadCoroutineIsStarted = false;
        }


    }

    IEnumerator WriteWithTime(string StringForReadAndWrite)
    {
        StringForReadAndWrite = UDPCLIENT.GetComponent<UDPClient1>().StringFromGlobalMenu;
        WriteCoroutineIsStarted = true;
        if (StringForReadAndWrite != null)
        {
            string[] s = StringForReadAndWrite.Split('|');
            frequency = float.Parse(s[1]);
        }
        yield return new WaitForSeconds(frequency);
        SW.WriteLine(StringForReadAndWrite);
        WriteCoroutineIsStarted = false;


    }

    public void ChoosePath()
    {
        string path = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text;

        SR = new StreamReader(path, true);
        StartDataRead = true;
        UDPCLIENT.GetComponent<UDPClient1>().StartUseTextData = true;
        UDPCLIENT.GetComponent<UDPClient1>().StartWriteTextData = false;
        DataReader.text = "Чтение идет";
        DataWriter.text = "Запись остановлена";
        MainCamera.GetComponent<camera>().enabled = true;
        OpenFileDialog.SetActive(false);
    }

    public void ExitFromOpenFileDialog()
    {
        StartDataRead = false;
        OpenFileDialog.SetActive(false);
    }

    public void ClearObjectsAndData()
    {
        //foreach (GameObject Car in UDPCLIENT.GetComponent<UDPClient1>().CarList )
        //{
        //    Destroy(Car);
        //}
        //UDPCLIENT.GetComponent<UDPClient1>().CarList.Clear();

        //foreach (GameObject Point in UDPCLIENT.GetComponent<UDPClient1>().TargetPointList)
        //{
        //    Destroy(Point);
        //}
        //UDPCLIENT.GetComponent<UDPClient1>().TargetPointList.Clear();

        //foreach (GameObject Agent in UDPCLIENT.GetComponent<UDPClient1>().AgentPointList)
        //{
        //    Destroy(Agent);
        //}


        //UDPCLIENT.GetComponent<UDPClient1>().AgentPointList.Clear();
        //UDPCLIENT.GetComponent<UDPClient1>().AgentPointList.Clear();

        //UDPclient.CarList
        //CarList.Clear();
        //AgentPointList.Clear(); // НавМешАгенты машин
        //TargetPointList.Clear(); // ТочкиСледования для НавМеш
        //CarDataList.Clear(); // Информация о всех машинах 
    }



    ////ExtremumSlipText.text = UDPclient.CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.extremumSlip.ToString();
    ////        ExtremumValueText.text = UDPclient.CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.extremumValue.ToString();
    ////        AsymptoteSlipText.text = UDPclient.CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.asymptoteSlip.ToString();
    ////        AsymptoteValueText.text = UDPclient.CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.asymptoteValue.ToString();
    ////        StiffnessText.text = UDPclient.CarList[0].GetComponent<WayPointCar>().WColForward[0].forwardFriction.stiffness.ToString();
}
