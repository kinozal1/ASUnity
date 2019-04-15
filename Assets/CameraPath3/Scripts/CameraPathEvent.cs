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
using System.Text;
using System.Xml;
#endif

public class CameraPathEvent : CameraPathPoint
{
    public enum Types
    {
        Broadcast,
        Call
    }

    public enum ArgumentTypes
    {
        None,
        Float,
        Int,
        String
    }

    public Types type = Types.Broadcast;
    public string eventName = "Camera Path Event";
    public GameObject target;
    public string methodName;
    public string methodArgument;
    public ArgumentTypes argumentType;
    
#if UNITY_EDITOR
    public override string ToXML()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(base.ToXML());
        sb.AppendLine("<type>" + type + "</type>");
        sb.AppendLine("<eventName>" + eventName + "</eventName>");
        if(target != null)
            sb.AppendLine("<target>" + target.name + "</target>");
        sb.AppendLine("<methodName>" + methodName + "</methodName>");
        sb.AppendLine("<methodArgument>" + methodArgument + "</methodArgument>");
        sb.AppendLine("<argumentType>" + argumentType + "</argumentType>");
        return sb.ToString();
    }

    public override void FromXML(XmlNode node, CameraPath cameraPath)
    {
        base.FromXML(node, cameraPath);
        type = (Types)System.Enum.Parse(typeof(Types), node["type"].FirstChild.Value);
        eventName = node["eventName"].FirstChild.Value;

        if (node["target"] != null && node["target"].HasChildNodes)
                target = GameObject.Find(node["target"].FirstChild.Value);

        if (node["methodName"] != null && node["methodName"].HasChildNodes)
            methodName = node["methodName"].FirstChild.Value;

        if (node["methodArgument"] != null && node["methodArgument"].HasChildNodes)
            methodArgument = node["methodArgument"].FirstChild.Value;
        argumentType = (ArgumentTypes)System.Enum.Parse(typeof(ArgumentTypes), node["argumentType"].FirstChild.Value);
    }
#endif
}
