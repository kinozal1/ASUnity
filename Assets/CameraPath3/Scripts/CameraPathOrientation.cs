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

[ExecuteInEditMode]
public class CameraPathOrientation : CameraPathPoint
{
    public Quaternion rotation = Quaternion.identity;
    public Transform lookAt = null;

    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }
    
#if UNITY_EDITOR
    public override string ToXML()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(base.ToXML());
        sb.AppendLine("<rotationX>" + rotation.x + "</rotationX>");
        sb.AppendLine("<rotationY>" + rotation.y + "</rotationY>");
        sb.AppendLine("<rotationZ>" + rotation.z + "</rotationZ>");
        sb.AppendLine("<rotationW>" + rotation.w + "</rotationW>");
        if(lookAt != null)
            sb.AppendLine("<lookAt>" + lookAt.gameObject.name + "</lookAt>");
        return sb.ToString();
    }

    public override void FromXML(XmlNode node, CameraPath cameraPath)
    {
        base.FromXML(node, cameraPath);
        rotation.x = float.Parse(node["rotationX"].FirstChild.Value);
        rotation.y = float.Parse(node["rotationY"].FirstChild.Value);
        rotation.z = float.Parse(node["rotationZ"].FirstChild.Value);
        rotation.w = float.Parse(node["rotationW"].FirstChild.Value);

        if (node["lookAt"] != null && node["lookAt"].HasChildNodes)
            lookAt = GameObject.Find(node["lookAt"].FirstChild.Value).transform;
    }
#endif
}
