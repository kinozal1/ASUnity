// Camera Path 3
// Available on the Unity Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com/camera-path/
// For support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using System;
#if UNITY_EDITOR
using System.Xml;
#endif

[ExecuteInEditMode]
public class CameraPathOrientationList : CameraPathPointList
{
    public enum Interpolation
    {
        None,
        Linear,
        SmoothStep,
        Hermite,
        Cubic
    }
    
    public Interpolation interpolation = Interpolation.Cubic;


    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public override void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;

        pointTypeName = "Orientation";
        base.Init(_cameraPath);
        cameraPath.PathPointAddedEvent += AddOrientation;
        initialised = true;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        cameraPath.PathPointAddedEvent -= AddOrientation;
        initialised = false;
    }

    public new CameraPathOrientation this[int index] 
    {
        get {return ((CameraPathOrientation)(base[index]));}
    }

    public void AddOrientation(CameraPathControlPoint atPoint)
    {
        CameraPathOrientation orientation = gameObject.AddComponent<CameraPathOrientation>();//CreateInstance<CameraPathOrientation>();
        if (atPoint.forwardControlPoint != Vector3.zero)
            orientation.rotation = Quaternion.LookRotation(atPoint.forwardControlPoint);
        else
            orientation.rotation = Quaternion.LookRotation(cameraPath.GetPathDirection(atPoint.percentage));
        orientation.hideFlags = HideFlags.HideInInspector;
        AddPoint(orientation, atPoint);
        RecalculatePoints();
        //return orientation;
    }

    public CameraPathOrientation AddOrientation(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage, Quaternion rotation)
    {
        CameraPathOrientation orientation = gameObject.AddComponent<CameraPathOrientation>();//CreateInstance<CameraPathOrientation>();
        orientation.rotation = rotation;
        orientation.hideFlags = HideFlags.HideInInspector;
        AddPoint(orientation, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return orientation;
    }

    public void RemovePoint(CameraPathOrientation orientation)
    {
        base.RemovePoint(orientation);
        RecalculatePoints();
    }

    public Quaternion GetOrientation(float percentage)
    {
        if (realNumberOfPoints < 2)
        {
            if (realNumberOfPoints == 1)
                return (this[0]).rotation;
            return Quaternion.identity;
        }

        if(float.IsNaN(percentage))
            percentage = 0;

        percentage = Mathf.Clamp(percentage, 0.0f, 1.0f);

        Quaternion returnQ = Quaternion.identity;
        switch (interpolation)
        {
            case Interpolation.Cubic:
                returnQ = CubicInterpolation(percentage);
                break;

            case Interpolation.Hermite:
                returnQ = CubicInterpolation(percentage);
                break;

            case Interpolation.SmoothStep:
                returnQ = SmootStepInterpolation(percentage);
                break;

            case Interpolation.Linear:
                returnQ = LinearInterpolation(percentage);
                break;

            case Interpolation.None:
                CameraPathOrientation point = (CameraPathOrientation)GetPoint(GetNextPointIndex(percentage));
                returnQ = point.rotation;
                break;

            default:
                returnQ = Quaternion.LookRotation(Vector3.forward);
                break;
        }
        if(float.IsNaN(returnQ.x))
            return Quaternion.identity;
        return returnQ;
    }

    private Quaternion LinearInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathOrientation pointP = (CameraPathOrientation)GetPoint(index);
        CameraPathOrientation pointQ = (CameraPathOrientation)GetPoint(index + 1);
        
        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        return Quaternion.Lerp(pointP.rotation, pointQ.rotation, ct);
    }

    private Quaternion SmootStepInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathOrientation pointP = (CameraPathOrientation)GetPoint(index);
        CameraPathOrientation pointQ = (CameraPathOrientation)GetPoint(index + 1);
        
        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;

        Quaternion returnQ = Quaternion.Lerp(pointP.rotation, pointQ.rotation, CPMath.SmoothStep(ct));
        return returnQ;
    }

    private Quaternion CubicInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathOrientation pointP = (CameraPathOrientation)GetPoint(index);
        CameraPathOrientation pointQ = (CameraPathOrientation)GetPoint(index + 1);
        CameraPathOrientation pointA = (CameraPathOrientation)GetPoint(index - 1);
        CameraPathOrientation pointB = (CameraPathOrientation)GetPoint(index + 2);

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;

        Quaternion returnQ = CPMath.CalculateCubic(pointP.rotation, pointA.rotation, pointB.rotation, pointQ.rotation, ct);

        if(float.IsNaN(returnQ.x))
        {
            Debug.Log(percentage + " " + pointP.fullName + " " + pointQ.fullName + " " + pointA.fullName + " " + pointB.fullName);
            returnQ = pointP.rotation;
        }


        return returnQ;
    }

    protected override void RecalculatePoints()
    {
        base.RecalculatePoints();

        for(int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathOrientation point = this[i];
            if(point.lookAt != null)
                point.rotation = Quaternion.LookRotation(point.lookAt.transform.position - point.worldPosition);
        }
    }
    
#if UNITY_EDITOR
    public override void FromXML(XmlNodeList nodes)
    {
        Clear();
        foreach (XmlNode node in nodes)
        {
            CameraPathOrientation newCameraPathPoint = gameObject.AddComponent<CameraPathOrientation>();//CreateInstance<CameraPathOrientation>();
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
