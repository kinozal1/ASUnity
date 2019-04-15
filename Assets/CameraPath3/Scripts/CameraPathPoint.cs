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
using System.Text;
#endif

[ExecuteInEditMode]
public class CameraPathPoint : MonoBehaviour
{
    public enum PositionModes
    {
        Free,
        FixedToPoint,
        FixedToPercent
    }

    public PositionModes positionModes = PositionModes.Free;
    public string givenName = "";
    public string customName = "";
    /// <summary>
    /// mostly for debug purposes
    /// </summary>
    public string fullName = "";

    [SerializeField]
    protected float _percent = 0;

    [SerializeField]
    protected float _animationPercentage = 0;

    public CameraPathControlPoint point = null;

    public int index = 0;

    //free point values - calculated by the CameraPathPointList
    public CameraPathControlPoint cpointA;
    public CameraPathControlPoint cpointB;
    public float curvePercentage = 0;

    public Vector3 worldPosition;

    public bool lockPoint = false;

    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public float percent
    {
        get
        {
            switch (positionModes)
            {
                case PositionModes.Free:
                    return _percent;

                case PositionModes.FixedToPercent:
                    return _percent;

                case PositionModes.FixedToPoint:
                    return point.percentage;
            }
            return _percent;
        }
        set { _percent = value; }
    }

    public float rawPercent
    {
        get {return _percent;}
    }

    public float animationPercentage
    {
        get
        {
            switch (positionModes)
            {
                case PositionModes.Free:
                    return _animationPercentage;

                case PositionModes.FixedToPercent:
                    return _animationPercentage;

                case PositionModes.FixedToPoint:
                    return point.normalisedPercentage;
            }
            return _percent;
        }
        set { _animationPercentage = value; }
    }

    public string displayName
    {
        get
        {
            if(customName != "")
                return customName;
            else
                return givenName;
        }
    }
    
#if UNITY_EDITOR
    public virtual string ToXML()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<positionModes>"+positionModes+"</positionModes>");
        sb.AppendLine("<customName>"+customName+"</customName>");
        sb.AppendLine("<_percent>"+_percent+"</_percent>");
        sb.AppendLine("<_animationPercentage>"+_animationPercentage+"</_animationPercentage>");
        if(point!= null)
            sb.AppendLine("<point>"+point.index+"</point>");
        sb.AppendLine("<index>"+index+"</index>");
        if (cpointA != null)
            sb.AppendLine("<cpointA>"+cpointA.index+"</cpointA>");
        if(cpointB != null)
            sb.AppendLine("<cpointB>"+cpointB.index+"</cpointB>");
        sb.AppendLine("<curvePercentage>"+curvePercentage+"</curvePercentage>");
        sb.AppendLine("<worldPositionX>"+worldPosition.x+"</worldPositionX>");
        sb.AppendLine("<worldPositionY>"+worldPosition.y+"</worldPositionY>");
        sb.AppendLine("<worldPositionZ>"+worldPosition.z+"</worldPositionZ>");
        sb.AppendLine("<lockPoint>"+lockPoint+"</lockPoint>");
        return sb.ToString();
    }

    public virtual void FromXML(XmlNode node, CameraPath cameraPath)
    {
        if (node["customName"].HasChildNodes)
            customName = node["customName"].FirstChild.Value;
        index = int.Parse(node["index"].FirstChild.Value);
        positionModes = (PositionModes)System.Enum.Parse(typeof(PositionModes), node["positionModes"].FirstChild.Value);
        
        if(node["point"] != null)
            point = cameraPath[int.Parse(node["point"].FirstChild.Value)];
        if (node["cpointA"] != null)
            cpointA = cameraPath[int.Parse(node["cpointA"].FirstChild.Value)];
        if (node["cpointB"] != null)
            cpointB = cameraPath[int.Parse(node["cpointB"].FirstChild.Value)];

        _percent = float.Parse(node["_percent"].FirstChild.Value);
        _animationPercentage = float.Parse(node["_animationPercentage"].FirstChild.Value);
        curvePercentage = float.Parse(node["curvePercentage"].FirstChild.Value);
        worldPosition.x = float.Parse(node["worldPositionX"].FirstChild.Value);
        worldPosition.y = float.Parse(node["worldPositionY"].FirstChild.Value);
        worldPosition.z = float.Parse(node["worldPositionZ"].FirstChild.Value);
        lockPoint = bool.Parse(node["lockPoint"].FirstChild.Value);
    }
#endif
}