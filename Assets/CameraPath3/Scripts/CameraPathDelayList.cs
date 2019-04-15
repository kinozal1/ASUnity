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
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Xml;
#endif

[ExecuteInEditMode]
public class CameraPathDelayList : CameraPathPointList
{
    public float MINIMUM_EASE_VALUE = 0.01f;
    public delegate void CameraPathDelayEventHandler(float time);
    public event CameraPathDelayEventHandler CameraPathDelayEvent;
    private float _lastPercentage = 0;

    [SerializeField]
    private CameraPathDelay _introPoint;
    [SerializeField]
    private CameraPathDelay _outroPoint;

    [SerializeField]
    private bool delayInitialised;

    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public new CameraPathDelay this[int index]
    {
        get { return ((CameraPathDelay)(base[index])); }
    }

    public CameraPathDelay introPoint {get {return _introPoint;}}

    public CameraPathDelay outroPoint {get {return _outroPoint;}}

    public override void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;
        pointTypeName = "Delay";
        base.Init(_cameraPath);

        if(!delayInitialised)
        {
            _introPoint = gameObject.AddComponent<CameraPathDelay>();//  CreateInstance<CameraPathDelay>();
            _introPoint.customName = "Start Point";
            _introPoint.hideFlags = HideFlags.HideInInspector;
            AddPoint(introPoint, 0);
            _outroPoint = gameObject.AddComponent<CameraPathDelay>();//CreateInstance<CameraPathDelay>();
            _outroPoint.customName = "End Point";
            _outroPoint.hideFlags = HideFlags.HideInInspector;
            AddPoint(outroPoint, 1);
            RecalculatePoints();
            delayInitialised = true;
        }
    }

    public void AddDelayPoint(CameraPathControlPoint atPoint)
    {
        CameraPathDelay point = gameObject.AddComponent<CameraPathDelay>();//CreateInstance<CameraPathDelay>();
        point.hideFlags = HideFlags.HideInInspector;
        AddPoint(point, atPoint);
        RecalculatePoints();
    }

    public CameraPathDelay AddDelayPoint(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage)
    {
        CameraPathDelay point = gameObject.AddComponent<CameraPathDelay>();//CreateInstance<CameraPathDelay>();
        point.hideFlags = HideFlags.HideInInspector;
        AddPoint(point, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return point;
    }

    public void OnAnimationStart(float startPercentage)
    {
        _lastPercentage = startPercentage;
    }

    public void CheckEvents(float percentage)
    {
        if (Mathf.Abs(percentage - _lastPercentage) > 0.1f)
        {
            _lastPercentage = percentage;//probable loop/seek
            return;
        }

        if(_lastPercentage == percentage)
            return;

        for (int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathDelay eventPoint = this[i];

            if(eventPoint == outroPoint)//outro doesn't delay stuff
                continue;

            if (eventPoint.percent >= _lastPercentage && eventPoint.percent <= percentage)//forward animation
            {
                if (eventPoint != introPoint)
                    FireDelay(eventPoint);
                else if(eventPoint.time > 0)
                    FireDelay(eventPoint);
                continue;

            }
            if (eventPoint.percent >= percentage && eventPoint.percent <= _lastPercentage)//backward animation
            {
                if (eventPoint != introPoint)
                    FireDelay(eventPoint);
                else if (eventPoint.time > 0)
                    FireDelay(eventPoint);
                continue;
            }
        }

        _lastPercentage = percentage;
    }

    public float CheckEase(float percent)
    {
        float output = 1.0f;

        for (int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathDelay eventPoint = this[i];

            if(eventPoint != introPoint)
            {
                CameraPathDelay earlierPoint = (CameraPathDelay)GetPoint(i - 1);
                float pathIntroPercent = cameraPath.GetPathPercentage(earlierPoint.percent, eventPoint.percent, 1-eventPoint.introStartEasePercentage);
                if (pathIntroPercent < percent && eventPoint.percent > percent)
                {
                    float animCurvePercent = (percent - pathIntroPercent) / (eventPoint.percent - pathIntroPercent);
                    output = eventPoint.introCurve.Evaluate(animCurvePercent);
                }
            }

            if(eventPoint != outroPoint)
            {
                CameraPathDelay laterPoint = (CameraPathDelay)GetPoint(i + 1);
                float pathOutroPercent = cameraPath.GetPathPercentage(eventPoint.percent, laterPoint.percent, eventPoint.outroEndEasePercentage);
                if (eventPoint.percent < percent && pathOutroPercent > percent)
                {
                    float animCurvePercent = (percent - eventPoint.percent) / (pathOutroPercent - eventPoint.percent);
                    output = eventPoint.outroCurve.Evaluate(animCurvePercent);
                }
            }
        }
        return Math.Max(output, MINIMUM_EASE_VALUE);
    }

    public void FireDelay(CameraPathDelay eventPoint)
    {
        if (CameraPathDelayEvent != null)
            CameraPathDelayEvent(eventPoint.time);
    }
    
#if UNITY_EDITOR
    public override void FromXML(XmlNodeList nodes)
    {
        Clear();
        foreach (XmlNode node in nodes)
        {
            CameraPathDelay newCameraPathPoint = gameObject.AddComponent<CameraPathDelay>();//CreateInstance<CameraPathDelay>();
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

                case CameraPathPoint.PositionModes.FixedToPercent:
                    switch (node["customName"].FirstChild.Value)
                    {
                        case "Start Point":
                            _introPoint = newCameraPathPoint;
                            AddPoint(newCameraPathPoint, 0);
                            break;
                        case "End Point":
                            _outroPoint = newCameraPathPoint;
                            AddPoint(newCameraPathPoint, 1);
                            break;
                        default:
                            float atPercentage = float.Parse(node["_percent"].FirstChild.Value);
                            AddPoint(newCameraPathPoint, atPercentage);
                            break;
                    }
                    break;
            }
            newCameraPathPoint.FromXML(node, cameraPath);
        }
    }
#endif
}