using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDrawer : MonoBehaviour
{
    public float ThetaScale = 0.01f;
    public float radius = 3f;
    public int Size;
    private LineRenderer LineDrawer;
    private float Theta = 0f;

    void Start()
    {
        LineDrawer = GetComponent<LineRenderer>();
        LineDrawer.useWorldSpace = false;
    }

    void Update()
    {

        Theta = 0f;
        Size = (int)((1f / ThetaScale) + 1f);
        LineDrawer.positionCount=Size;
        for (int i = 0; i < Size; i++)
        {
            Theta += (2.0f * Mathf.PI * ThetaScale);
            float x = radius * Mathf.Cos(Theta);
            float y = radius * Mathf.Sin(Theta);
            LineDrawer.SetPosition(i, new Vector3(x+radius, 0, y ));
        }
    }
}
