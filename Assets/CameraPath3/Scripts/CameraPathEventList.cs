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
#if UNITY_EDITOR
using System;
using System.Xml;
#endif

[ExecuteInEditMode]
public class CameraPathEventList : CameraPathPointList
{
    public delegate void CameraPathEventPointHandler(string name);
    public event CameraPathEventPointHandler CameraPathEventPoint;
    private float _lastPercentage;

    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public new CameraPathEvent this[int index]
    {
        get { return ((CameraPathEvent)(base[index])); }
    }

    public override void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;
        pointTypeName = "Event";
        base.Init(_cameraPath);
    }

    public void AddEvent(CameraPathControlPoint atPoint)
    {
        CameraPathEvent point = gameObject.AddComponent<CameraPathEvent>();//CreateInstance<CameraPathEvent>();
        point.hideFlags = HideFlags.HideInInspector;
        AddPoint(point, atPoint);
        RecalculatePoints();
    }

    public CameraPathEvent AddEvent(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage)
    {
        CameraPathEvent eventPoint = gameObject.AddComponent<CameraPathEvent>();//CreateInstance<CameraPathEvent>();
        eventPoint.hideFlags = HideFlags.HideInInspector;
        AddPoint(eventPoint, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return eventPoint;
    }

    public void OnAnimationStart(float startPercentage)
    {
        _lastPercentage = startPercentage;
    }

    public void CheckEvents(float percentage)
    {
        if(Mathf.Abs(percentage - _lastPercentage) > 0.5f)
        {
            _lastPercentage = percentage;//probable loop
            return;
        }

        for(int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathEvent eventPoint = this[i];
            bool eventBetweenAnimationDelta = (eventPoint.percent >= _lastPercentage && eventPoint.percent <= percentage) || (eventPoint.percent >= percentage && eventPoint.percent <= _lastPercentage);
            if(eventBetweenAnimationDelta)
            {
                switch(eventPoint.type)
                {
                    case CameraPathEvent.Types.Broadcast:
                        BroadCast(eventPoint);
                        break;

                    case CameraPathEvent.Types.Call:
                        Call(eventPoint);
                        break;
                }
            }
        }

        _lastPercentage = percentage;
    }

    public void BroadCast(CameraPathEvent eventPoint)
    {
        if(CameraPathEventPoint != null)
        {
            CameraPathEventPoint(eventPoint.eventName);
        }
    }

    public void Call(CameraPathEvent eventPoint)
    {
        if(eventPoint.target == null)
        {
            Debug.LogError("Camera Path Event Error: There is an event call without a specified target. Please check "+eventPoint.displayName, cameraPath);
            return;
        }

        switch(eventPoint.argumentType)
        {
            case CameraPathEvent.ArgumentTypes.None:
                eventPoint.target.SendMessage(eventPoint.methodName, SendMessageOptions.DontRequireReceiver);
                break;
            case CameraPathEvent.ArgumentTypes.Int:
                int intValue;
                if (int.TryParse(eventPoint.methodArgument, out intValue))
                    eventPoint.target.SendMessage(eventPoint.methodName, intValue, SendMessageOptions.DontRequireReceiver);
                else
                    Debug.LogError("Camera Path Aniamtor: The argument specified is not an integer");
                break;
            case CameraPathEvent.ArgumentTypes.Float:
                float floatValue = float.Parse(eventPoint.methodArgument);
                if(float.IsNaN(floatValue))
                    Debug.LogError("Camera Path Aniamtor: The argument specified is not a float");
                eventPoint.target.SendMessage(eventPoint.methodName, floatValue, SendMessageOptions.DontRequireReceiver);
                break;
            case CameraPathEvent.ArgumentTypes.String:
                string sendValue = eventPoint.methodArgument;
                eventPoint.target.SendMessage(eventPoint.methodName, sendValue, SendMessageOptions.DontRequireReceiver);
                break;
        }
    }
    
#if UNITY_EDITOR
    public override void FromXML(XmlNodeList nodes)
    {
        Clear();
        foreach (XmlNode node in nodes)
        {
            CameraPathEvent newCameraPathPoint = gameObject.AddComponent<CameraPathEvent>();//CreateInstance<CameraPathEvent>();
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
