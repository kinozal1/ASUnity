// Camera Path 3
// Available on the Unity Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com/camera-path/
// For support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Xml;
#endif

[ExecuteInEditMode]
public class CameraPathSpeedList : CameraPathPointList 
{
    public enum Interpolation
    {
        None,
        Linear,
        SmoothStep
    }

    public Interpolation interpolation = Interpolation.SmoothStep;

    [SerializeField]
    private bool _enabled = true;


    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public new CameraPathSpeed this[int index] 
    {
        get { return ((CameraPathSpeed)(base[index])); }
    }

    public override void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;
        pointTypeName = "Speed";
        base.Init(_cameraPath);
    }
    
    public bool listEnabled
    {
        get {return _enabled&&realNumberOfPoints>0;} 
        set {_enabled = value;}
    }

    public void AddSpeedPoint(CameraPathControlPoint atPoint)
    {
        CameraPathSpeed point = gameObject.AddComponent<CameraPathSpeed>();//CreateInstance<CameraPathSpeed>();
        point.hideFlags = HideFlags.HideInInspector;
        AddPoint(point,atPoint);
        RecalculatePoints();
    }

    public CameraPathSpeed AddSpeedPoint(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage)
    {
        CameraPathSpeed point = gameObject.AddComponent<CameraPathSpeed>();//CreateInstance<CameraPathSpeed>();
        point.hideFlags = HideFlags.HideInInspector;
        AddPoint(point, curvePointA, curvePointB, Mathf.Clamp01(curvePercetage));
        RecalculatePoints();
        return point;
    }

    public float GetLowesetSpeed()
    {
        float output = Mathf.Infinity;
        int numberOfSpeedPoints = numberOfPoints;
        for(int i = 0; i < numberOfSpeedPoints; i++)
        {
            if(this[i].speed < output)
                output = this[i].speed;
        }
        return output;
    }

    public float GetSpeed(float percentage)
    {
        if (realNumberOfPoints < 2)
        {
            if (realNumberOfPoints == 1)
                return (this[0]).speed;
            Debug.Log("Not enough points to define a speed");
            return 0;
        }

        if (percentage >= 1)
            return ((CameraPathSpeed)GetPoint(realNumberOfPoints - 1)).speed;

        percentage = Mathf.Clamp(percentage, 0.0f, 0.999f);

        switch(interpolation)
        {
            case Interpolation.SmoothStep:
                return SmoothStepInterpolation(percentage);

            case Interpolation.Linear:
                return LinearInterpolation(percentage);

            case Interpolation.None:
                CameraPathSpeed point = (CameraPathSpeed)GetPoint(GetNextPointIndex(percentage));
                return point.speed;

            default:
                return LinearInterpolation(percentage);
        }
    }

    private float LinearInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathSpeed pointP = (CameraPathSpeed)GetPoint(index);
        CameraPathSpeed pointQ = (CameraPathSpeed)GetPoint(index + 1);

        if (percentage < pointP.percent)
            return pointP.speed;
        if (percentage > pointQ.percent)
            return pointQ.speed;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        float output = Mathf.Lerp(pointP.speed, pointQ.speed, ct);
        return output;
    }

    private float SmoothStepInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathSpeed pointP = (CameraPathSpeed)GetPoint(index);
        CameraPathSpeed pointQ = (CameraPathSpeed)GetPoint(index + 1);

        if (percentage < pointP.percent)
            return pointP.speed;
        if (percentage > pointQ.percent)
            return pointQ.speed;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        return Mathf.Lerp(pointP.speed, pointQ.speed, CPMath.SmoothStep(ct));
    }
    
#if UNITY_EDITOR
    public override void FromXML(XmlNodeList nodes)
    {
        Clear();
        foreach (XmlNode node in nodes)
        {
            CameraPathSpeed newCameraPathPoint = gameObject.AddComponent<CameraPathSpeed>();//CreateInstance<CameraPathSpeed>();
            newCameraPathPoint.hideFlags = HideFlags.HideInInspector;
            CameraPathPoint.PositionModes positionModes = (CameraPathPoint.PositionModes)Enum.Parse(typeof(CameraPathPoint.PositionModes), node["positionModes"].FirstChild.Value);
            switch (positionModes)
            {
                case CameraPathPoint.PositionModes.Free:
                    CameraPathControlPoint cPointA = cameraPath[int.Parse(node["cpointA"].FirstChild.Value)];
                    CameraPathControlPoint cPointB = cameraPath[int.Parse(node["cpointB"].FirstChild.Value)];
                    float curvePercentage = float.Parse(node["curvePercentage"].FirstChild.Value);
                    AddPoint(newCameraPathPoint, cPointA, cPointB, curvePercentage);
                    break;

                case CameraPathPoint.PositionModes.FixedToPoint:
                    CameraPathControlPoint point = cameraPath[int.Parse(node["point"].FirstChild.Value)];
                    AddPoint(newCameraPathPoint, point);
                    break;
            }
            newCameraPathPoint.FromXML(node, cameraPath);
        }
    }
#endif
}