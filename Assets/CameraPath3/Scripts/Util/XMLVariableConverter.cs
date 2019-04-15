using System.Text;
using System.Xml;
using UnityEngine;

public class XMLVariableConverter
{
    
#if UNITY_EDITOR
    public static string ToXML(Quaternion variable, string variableName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<" + variableName + ">");
        sb.AppendLine("<x>" + variable.x + "</x>");
        sb.AppendLine("<y>" + variable.y + "</y>");
        sb.AppendLine("<z>" + variable.z + "</z>");
        sb.AppendLine("<w>" + variable.w + "</w>");
        sb.AppendLine("</" + variableName + ">");
        return sb.ToString();
    }

    public static string ToXML(Vector3 variable, string variableName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<" + variableName + ">");
        sb.AppendLine("<x>" + variable.x + "</x>");
        sb.AppendLine("<y>" + variable.y + "</y>");
        sb.AppendLine("<z>" + variable.z + "</z>");
        sb.AppendLine("</" + variableName + ">");
        return sb.ToString();
    }

    public static string ToXML(Vector2 variable, string variableName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<" + variableName + ">");
        sb.AppendLine("<x>" + variable.x + "</x>");
        sb.AppendLine("<y>" + variable.y + "</y>");
        sb.AppendLine("</" + variableName + ">");
        return sb.ToString();
    }

    public static string ToXML(Color variable, string variableName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<" + variableName + ">");
        sb.AppendLine("<r>" + variable.r + "</r>");
        sb.AppendLine("<g>" + variable.g + "</g>");
        sb.AppendLine("<b>" + variable.b + "</b>");
        sb.AppendLine("<a>" + variable.a + "</a>");
        sb.AppendLine("</" + variableName + ">");
        return sb.ToString();
    }

    public static string ToXML(AnimationCurve variable, string variableName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<" + variableName + ">");
        foreach(Keyframe keyframe in variable.keys)
        {
            sb.AppendLine("<keyframe>");
            sb.AppendLine("<inTangent>"+keyframe.inTangent+"</inTangent>");
            sb.AppendLine("<outTangent>"+keyframe.outTangent+"</outTangent>");
            sb.AppendLine("<time>"+keyframe.time+"</time>");
            sb.AppendLine("<value>"+keyframe.value+"</value>");
            sb.AppendLine("<value>" + keyframe.value + "</value>");
            sb.AppendLine("</keyframe>");
        }
        sb.AppendLine("</" + variableName + ">");
        return sb.ToString();
    }

    public static Quaternion FromXMLQuaternion(XmlNode node)
    {
        Quaternion output = new Quaternion();
        output.x = float.Parse(node["x"].FirstChild.Value);
        output.y = float.Parse(node["y"].FirstChild.Value);
        output.z = float.Parse(node["z"].FirstChild.Value);
        output.w = float.Parse(node["w"].FirstChild.Value);
        return output;
    }

    public static Vector3 FromXMLVector3(XmlNode node)
    {
        Vector3 output = new Vector3();
        output.x = float.Parse(node["x"].FirstChild.Value);
        output.y = float.Parse(node["y"].FirstChild.Value);
        output.z = float.Parse(node["z"].FirstChild.Value);
        return output;
    }

    public static Vector2 FromXMLVector2(XmlNode node)
    {
        Vector2 output = new Vector3();
        output.x = float.Parse(node["x"].FirstChild.Value);
        output.y = float.Parse(node["y"].FirstChild.Value);
        return output;
    }

    public static Color FromXMLtoColour(XmlNode node)
    {
        Color output = new Color();
        output.r = float.Parse(node["r"].FirstChild.Value);
        output.g = float.Parse(node["g"].FirstChild.Value);
        output.b = float.Parse(node["b"].FirstChild.Value);
        output.a = float.Parse(node["a"].FirstChild.Value);
        return output;
    }

    public static AnimationCurve FromXMLtoAnimationCurve(XmlNode node)
    {
        AnimationCurve output = new AnimationCurve();
        if(node == null || !node.HasChildNodes)
        {
            output.AddKey(0, 1);
            output.AddKey(1, 1);
            return output;
        }
        foreach(XmlNode keyframeNode in node.SelectNodes("keyframe"))
        {
            Keyframe keyFrame = new Keyframe();
            keyFrame.inTangent = float.Parse(keyframeNode["inTangent"].FirstChild.Value);
            keyFrame.outTangent = float.Parse(keyframeNode["outTangent"].FirstChild.Value);
            keyFrame.time = float.Parse(keyframeNode["time"].FirstChild.Value);
            keyFrame.value = float.Parse(keyframeNode["value"].FirstChild.Value);
            output.AddKey(keyFrame);
        }
        return output;
    }
#endif
}
