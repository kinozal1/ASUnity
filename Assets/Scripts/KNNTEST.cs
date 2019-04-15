using UnityEngine;

public class KNNTEST : MonoBehaviour
{
    public GameObject Points;
    public Vector3 Pos;
    private Vector3 SavesPos;
    public bool ChangePostition;
    public float distance = 10000;

    // Start is called before the first frame update
    private void Start()
    {
        SavesPos = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (ChangePostition)
        {
            for (int i = 0; i < Points.transform.childCount; i++)
            {
                for (int j = 0; j < Points.transform.GetChild(i).childCount; j++)
                {
                    Vector3 OZCoords, OZPoint;
                    OZCoords = new Vector3(transform.position.x, 0, transform.position.z);
                    OZPoint = new Vector3(Points.transform.GetChild(i).GetChild(j).position.x, 0, Points.transform.GetChild(i).GetChild(j).position.z);
                    if (((OZPoint - OZCoords).magnitude < distance)) //Проверка, есть ли точка на карте 
                    {
                        distance = (OZPoint - OZCoords).magnitude;
                        SavesPos = Points.transform.GetChild(i).GetChild(j).position;
                    }
                }



            }
            transform.position = SavesPos;
            distance = 10000;

        }
        else
        {

        }

    }
}
