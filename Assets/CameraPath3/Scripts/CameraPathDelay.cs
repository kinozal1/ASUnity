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

public class CameraPathDelay : CameraPathPoint
{
    public float time = 0.0f;

    //intro ease curve
    public float introStartEasePercentage = 0.1f;
    public AnimationCurve introCurve = AnimationCurve.Linear(0, 1, 1, 1);

    //exit ease curve
    public float outroEndEasePercentage = 0.1f;
    public AnimationCurve outroCurve = AnimationCurve.Linear(0, 1, 1, 1);
    
#if UNITY_EDITOR
    public override string ToXML()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(base.ToXML());
        sb.AppendLine("<time>" + time + "</time>");
        sb.AppendLine("<introStartEasePercentage>" + introStartEasePercentage + "</introStartEasePercentage>");
        sb.AppendLine("<outroEndEasePercentage>" + outroEndEasePercentage + "</outroEndEasePercentage>");
        sb.AppendLine(XMLVariableConverter.ToXML(introCurve, "introCurve"));
        sb.AppendLine(XMLVariableConverter.ToXML(outroCurve, "outroCurve"));
        return sb.ToString();
    }

    public override void FromXML(XmlNode node, CameraPath cameraPath)
    {
        base.FromXML(node, cameraPath);
        time = float.Parse(node["time"].FirstChild.Value);
        introStartEasePercentage = float.Parse(node["introStartEasePercentage"].FirstChild.Value);
        outroEndEasePercentage = float.Parse(node["outroEndEasePercentage"].FirstChild.Value);
        introCurve = XMLVariableConverter.FromXMLtoAnimationCurve(node["introCurve"]);
        outroCurve = XMLVariableConverter.FromXMLtoAnimationCurve(node["outroCurve"]);
    }
#endif
}