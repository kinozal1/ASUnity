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
using System.Xml;
using System.Text;
#endif

public class CameraPathSpeed : CameraPathPoint
{
    public float _speed = 1.0f;

    public float speed
    {
        get {return _speed;}
        set {_speed = Mathf.Max(value, 0.0000001f);}
    }
    
#if UNITY_EDITOR
    public override string ToXML()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(base.ToXML());
        sb.AppendLine("<speed>" + speed + "</speed>");
        return sb.ToString();
    }

    public override void FromXML(XmlNode node, CameraPath cameraPath)
    {
        base.FromXML(node, cameraPath);
        speed = float.Parse(node["speed"].FirstChild.Value);
    }
#endif
}