using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkWithPoints : MonoBehaviour
{
    public GameObject Points;
    public Vector3 Pos;
    private Vector3 SavesPos;
    public bool ChangePostition;
    public float distance = 10000;
    public bool ShowPoints = true;
    public bool ShowCurrentEdge = true;
    public bool CorrectToPoints = true;
    public Material MaterialForCurrentEdge;
    public Material MaterialForEdge;
    public int NumberOfEdge;
    public int NumberOfCurrentEdge;

    // Start is called before the first frame update
    void Start()
    {
        RaycastHit hit;
        for (int i = 0; i < Points.transform.childCount; i++)
        {
            for (int j = 0; j < Points.transform.GetChild(i).childCount; j++)
            {
                if (((Physics.Raycast(Points.transform.GetChild(i).GetChild(j).position, -Points.transform.GetChild(i).GetChild(j).transform.up, out hit)))) //Проверка, есть ли точка на карте 
                {
                    Points.transform.GetChild(i).GetChild(j).position = hit.point;
                }
                if (!ShowPoints)
                {
                    Points.transform.GetChild(i).GetChild(j).GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void KNN(ref Vector3 CurrentCoordPoint)
    {
        if (CorrectToPoints)
        {

            for (int i = 0; i < Points.transform.childCount; i++)
            {
                for (int j = 0; j < Points.transform.GetChild(i).childCount; j++)
                {
                    Vector3 OZCoords, OZPoint;
                    OZCoords = new Vector3(CurrentCoordPoint.x, 0, CurrentCoordPoint.z);
                    OZPoint = new Vector3(Points.transform.GetChild(i).GetChild(j).position.x, 0, Points.transform.GetChild(i).GetChild(j).position.z);
                    if (((OZPoint - OZCoords).magnitude < distance))
                    {
                        distance = (OZPoint - OZCoords).magnitude;
                        SavesPos = Points.transform.GetChild(i).GetChild(j).position;
                        NumberOfEdge = i;
                    }

                }
            }

            if (ShowCurrentEdge && NumberOfCurrentEdge != NumberOfEdge)
            {
                for (int i = 0; i < Points.transform.GetChild(NumberOfCurrentEdge).childCount; i++)
                {
                    Points.transform.GetChild(NumberOfCurrentEdge).GetChild(i).GetComponent<MeshRenderer>().material = MaterialForEdge;
                }
                NumberOfCurrentEdge = NumberOfEdge;
                for (int i = 0; i < Points.transform.GetChild(NumberOfEdge).childCount; i++)
                {
                    if (Points.transform.GetChild(NumberOfCurrentEdge).GetChild(i).name != "ChoicePoints")
                    {
                        Points.transform.GetChild(NumberOfEdge).GetChild(i).GetComponent<MeshRenderer>().material = MaterialForCurrentEdge;
                    }

                }
            }


            CurrentCoordPoint = SavesPos;
            distance = 10000;
        }
        else
        {

        }


    }

    public void ClearData()
    {

            for (int i = 0; i < Points.transform.GetChild(NumberOfCurrentEdge).childCount; i++)
            {
                Points.transform.GetChild(NumberOfCurrentEdge).GetChild(i).GetComponent<MeshRenderer>().material = MaterialForEdge;
            }
        NumberOfCurrentEdge = 0;



    }

    public void ShowPointsMethod(bool ShowPoints)
    {
        RaycastHit hit;
        for (int i = 0; i < Points.transform.childCount; i++)
        {
            for (int j = 0; j < Points.transform.GetChild(i).childCount; j++)
            {
                if (!ShowPoints)
                {
                    Points.transform.GetChild(i).GetChild(j).GetComponent<MeshRenderer>().enabled = false;
                }
                else
                {
                    Points.transform.GetChild(i).GetChild(j).GetComponent<MeshRenderer>().enabled = true; ;
                }
            }
        }
    }
}
