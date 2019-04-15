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
public class CameraPathTiltList : CameraPathPointList
{
    public enum Interpolation
    {
        None,
        Linear,
        SmoothStep
    }

    public Interpolation interpolation = Interpolation.SmoothStep;

    public bool listEnabled = true;
    public float autoSensitivity = 1.0f;

    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public override void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;
        pointTypeName = "Tilt";
        base.Init(_cameraPath);
        cameraPath.PathPointAddedEvent += AddTilt;
        initialised = true;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        cameraPath.PathPointAddedEvent -= AddTilt;
        initialised = false;
    }

    public new CameraPathTilt this[int index] 
    {
        get { return ((CameraPathTilt)(base[index])); }
    }

    public void AddTilt(CameraPathControlPoint atPoint)
    {
        CameraPathTilt point = gameObject.AddComponent<CameraPathTilt>();//CreateInstance<CameraPathTilt>();
        point.tilt = 0;
        point.hideFlags = HideFlags.HideInInspector;
        AddPoint(point,atPoint);
        RecalculatePoints();
    }

    public CameraPathTilt AddTilt(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage, float tilt)
    {
        CameraPathTilt tiltPoint = gameObject.AddComponent<CameraPathTilt>();//CreateInstance<CameraPathTilt>();
        tiltPoint.tilt = tilt;
        tiltPoint.hideFlags = HideFlags.HideInInspector;
        AddPoint(tiltPoint, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return tiltPoint;
    }

    public float GetTilt(float percentage)
    {
        if (realNumberOfPoints < 2)
        {
            if (realNumberOfPoints == 1)
                return (this[0]).tilt;
            return 0;
        }

//        if (percentage >= 1)
//            return ((CameraPathTilt)GetPoint(realNumberOfPoints - 1)).tilt;

        percentage = Mathf.Clamp(percentage, 0.0f, 1.0f);

        switch(interpolation)
        {
            case Interpolation.SmoothStep:
                return SmoothStepInterpolation(percentage);

            case Interpolation.Linear:
                return LinearInterpolation(percentage);

            case Interpolation.None:
                CameraPathTilt point = (CameraPathTilt)GetPoint(GetNextPointIndex(percentage));
                return point.tilt;

            default:
                return LinearInterpolation(percentage);
        }
    }

    public void AutoSetTilts()
    {
        for(int i = 0; i < realNumberOfPoints; i++)
        {
            AutoSetTilt(this[i]);
        }
    }

    public void AutoSetTilt(CameraPathTilt point)
    {
        float tiltPercentage = point.percent;
        Vector3 pointA = cameraPath.GetPathPosition(tiltPercentage - 0.1f);
        Vector3 pointB = cameraPath.GetPathPosition(tiltPercentage);
        Vector3 pointC = cameraPath.GetPathPosition(tiltPercentage + 0.1f);

        Vector3 directionAB = pointB - pointA;
        Vector3 directionBC = pointC - pointB;
        Quaternion angle = Quaternion.LookRotation(-cameraPath.GetPathDirection(point.percent));
        Vector3 pathCurveDirection = angle * (directionBC - directionAB).normalized;
        float curveAngle = Vector2.Angle(Vector2.up, new Vector2(pathCurveDirection.x, pathCurveDirection.y));
        float ratio = Mathf.Min(Mathf.Abs(pathCurveDirection.x) + Mathf.Abs(pathCurveDirection.y) / Mathf.Abs(pathCurveDirection.z),1.0f);

        point.tilt = -curveAngle * autoSensitivity * ratio;
    }

    private float LinearInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathTilt pointP = (CameraPathTilt)GetPoint(index);
        CameraPathTilt pointQ = (CameraPathTilt)GetPoint(index + 1);

//        if (percentage < pointP.percent)
//            return pointP.tilt;
//        if (percentage > pointQ.percent)
//            return pointQ.tilt;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        return Mathf.LerpAngle(pointP.tilt, pointQ.tilt, ct);
    }

    private float SmoothStepInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathTilt pointP = (CameraPathTilt)GetPoint(index);
        CameraPathTilt pointQ = (CameraPathTilt)GetPoint(index + 1);

//        if (percentage < pointP.percent)
//            return pointP.tilt;
//        if (percentage > pointQ.percent)
//            return pointQ.tilt;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;

        return Mathf.LerpAngle(pointP.tilt, pointQ.tilt, CPMath.SmoothStep(ct));
    }
    
#if UNITY_EDITOR
    public override void FromXML(XmlNodeList nodes)
    {
        Clear();
        foreach (XmlNode node in nodes)
        {
            CameraPathTilt newCameraPathPoint = gameObject.AddComponent<CameraPathTilt>();//CreateInstance<CameraPathTilt>();
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
            newCameraPathPoint.FromXML(node,cameraPath);
        }
    }
#endif
}
