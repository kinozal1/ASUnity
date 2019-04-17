using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpenAnimation : MonoBehaviour
{
    public float FadeRate = 0.005f;
    public float ReFadeRate = 0.005f;
    public RawImage image;
    public Image[] Logos;
    public float targetAlpha;
    public float TimeToWait;
    public bool StartOpening = false;
    public bool StartEnding = false;

    public float alphaDiff;
    public Color currentcolor;
    // Use this for initialization
    void Start()
    {
        if (image.transform.gameObject.activeInHierarchy == false)
        {
            image.transform.gameObject.SetActive(true);
        }
        this.targetAlpha =0;
        StartCoroutine(Wait());
    }

    // Update is called once per frame
    void Update()
    {
        if (StartOpening)
        {
            currentcolor = this.image.color;
            alphaDiff = Mathf.Abs(currentcolor.a - this.targetAlpha);

            if (alphaDiff > 0.0001f)
            {

                currentcolor.a = currentcolor.a - FadeRate; 
                image.color = currentcolor;
                for (int i = 0; i < Logos.Length; i++)
                {
                    Logos[i].color = currentcolor;
                }
            }
            if(currentcolor.a<0)

            {
                StartOpening = false;
            }
        }
        if (StartEnding)
        {
            currentcolor = this.image.color;
            alphaDiff = Mathf.Abs(currentcolor.a - 1);

            if (alphaDiff > 0.0001f)
            {

                currentcolor.a = currentcolor.a + ReFadeRate;
                image.color = currentcolor;
                for (int i = 0; i < Logos.Length; i++)
                {
                    Logos[i].color = currentcolor;
                }
                
            }
            if(currentcolor.a>0.99)
            {
                Debug.Log("New");
                StartEnding = false;
                SceneManager.LoadScene("MapWithGraph");
               
            }
        }
    }

    public void FadeOut()
    {
        this.targetAlpha = 0.0f;
    }

    public void FadeIn()
    {
        //this.targetAlpha = 1.0f;
    }

    IEnumerator Wait()
    {
       
        yield return new WaitForSeconds(TimeToWait);
        StartOpening = true;



    }

    public void Exit()
    {
        UnityEngine.Application.Quit();
    }
}
