// Camera Path 3
// Available on the Unity Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com/camera-path/
// For support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System.Text;
using System.Xml;
#endif

public class CameraPathFOV : CameraPathPoint
{
    public float FOV = 60;
    public float Size = 5;
    
#if UNITY_EDITOR
    public override string ToXML()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(base.ToXML());
        sb.AppendLine("<FOV>" + FOV + "</FOV>");
        sb.AppendLine("<Size>" + Size + "</Size>");
        return sb.ToString();
    }

    public override void FromXML(XmlNode node, CameraPath cameraPath)
    {
        base.FromXML(node, cameraPath);
        if (node["FOV"] != null) FOV = float.Parse(node["FOV"].FirstChild.Value);
        if (node["Size"] != null) Size = float.Parse(node["Size"].FirstChild.Value);
    }
#endif
}