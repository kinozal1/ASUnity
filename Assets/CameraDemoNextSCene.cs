using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraDemoNextSCene : MonoBehaviour
{
    public CameraPathAnimator AnimatorForDemo, AnimatorForNextLevel;
    public CameraPath CameraPathForDemo, CameraPathForNextLevel;
    
    public CameraPathControlPoint Point1,Point2;

    public Vector3 SavedPos;

    public Quaternion SavedQuaternion;

    public GameObject EndPoint;
    public GameObject Camera;

    public GameObject On;
    public GameObject Off;

    public GameObject AnimationScript;



    // Start is called before the first frame update
    void Start()
    {

        SavedPos = Camera.transform.position;
        SavedQuaternion = Camera.transform.rotation;
        AnimatorForDemo.enabled = true;
        AnimatorForDemo.playOnStart = false;
        AnimatorForNextLevel.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void DemoCameraStart()
    {
        On.SetActive(false);
        Off.SetActive(true);
        AnimatorForDemo.Play();     
    }
    public void DemoCameraStop()
    {
        AnimatorForDemo.Stop();
        Camera.transform.position = SavedPos;
        Camera.transform.rotation = SavedQuaternion;
        On.SetActive(true);
        Off.SetActive(false);


    }

    public void NextLevel() 
    {

        AnimatorForDemo.enabled = false;
        AnimatorForNextLevel.enabled = true;
        StartCoroutine(Wait());


       

        //CP.worldPosition = Camera.transform.position;
        //Orientation.rotation = Camera.transform.rotation;
        //CameraPath.InsertPoint(CP,0);
        //CP.worldPosition = EndPoint.transform.position;

    } //ЧТО
    IEnumerator Wait()
    {

        yield return new WaitForSeconds(AnimatorForNextLevel.animationTime-3);
        AnimationScript.GetComponent<OpenAnimation>().StartOpening= false;
        AnimationScript.GetComponent<OpenAnimation>().StartEnding = true;





    }
}
