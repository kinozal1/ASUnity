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
public class CameraPathControlPoint : MonoBehaviour
{
    public string givenName = "";
    public string customName = "";
    public string fullName = "";//used in debugging mostly, includes Path name

    [SerializeField]
    private Vector3 _position;

    //Bezier Control Points
    [SerializeField]
    private bool _splitControlPoints = false;
    [SerializeField]
    private Vector3 _forwardControlPoint;
    [SerializeField]
    private Vector3 _backwardControlPoint;

    //Internal stored calculations
    [SerializeField]
    private Vector3 _pathDirection = Vector3.forward;

    public int index = 0;
    public float percentage = 0;
    public float normalisedPercentage = 0;
    
    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public Vector3 localPosition
    {
        get{return transform.rotation * _position;}
        set{_position = Quaternion.Inverse(transform.rotation) * value;}
    }

    public Vector3 worldPosition
    {
        get {return LocalToWorldPosition(_position);}
        set{_position = WorldToLocalPosition(value);}
    }

    public Vector3 WorldToLocalPosition(Vector3 _worldPosition)
    {
        Vector3 newValue = _worldPosition - transform.position;
        return Quaternion.Inverse(transform.rotation) * newValue;
    }

    public Vector3 LocalToWorldPosition(Vector3 _localPosition)
    {
        return transform.rotation * _localPosition + transform.position;
    }

    public Vector3 forwardControlPointWorld
    {
        set { forwardControlPoint = value - transform.position; }
        get { return forwardControlPoint + transform.position; }
    }

    public Vector3 forwardControlPoint
    {
        get
        {
            return transform.rotation * (_forwardControlPoint + _position);
        }
        set
        {
            Vector3 newValue = value;
            newValue = Quaternion.Inverse(transform.rotation) * newValue;
            newValue += -_position;
            _forwardControlPoint = newValue;
        }
    }

    public Vector3 forwardControlPointLocal
    {
        get
        {
            return transform.rotation * _forwardControlPoint;
        }
        set
        {
            Vector3 newValue = value;
            newValue = Quaternion.Inverse(transform.rotation) * newValue;
            _forwardControlPoint = newValue;
        }
    }

    public Vector3 backwardControlPointWorld
    {
        set { backwardControlPoint = value - transform.position; }
        get { return backwardControlPoint + transform.position; }
    }

    public Vector3 backwardControlPoint
    {
        get
        {
            Vector3 controlPoint = (_splitControlPoints) ? _backwardControlPoint : -_forwardControlPoint;
            return transform.rotation * (controlPoint + _position);
        }
        set
        {
            Vector3 newValue = value;
            newValue = Quaternion.Inverse(transform.rotation) * newValue;
            newValue += -_position;
            if (_splitControlPoints)
                _backwardControlPoint = newValue;
            else
                _forwardControlPoint = -newValue;
        }
    }

    public bool splitControlPoints
    {
        get { return _splitControlPoints; }
        set
        {
            if (value != _splitControlPoints)
                _backwardControlPoint = -_forwardControlPoint;
            _splitControlPoints = value;
        }
    }

    public Vector3 trackDirection
    {
        get
        {
            return _pathDirection;
        }
        set
        {
            if (value == Vector3.zero)
                return;
            _pathDirection = value.normalized;
        }
    }

    public string displayName
    {
        get
        {
            if (customName != "")
                return customName;
            return givenName;
        }
    }
    
#if UNITY_EDITOR
    public string ToXML()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<customName>"+customName+"</customName>");
        sb.AppendLine("<index>"+index+"</index>");
        sb.AppendLine("<percentage>"+percentage+"</percentage>");
        sb.AppendLine("<normalisedPercentage>"+normalisedPercentage+"</normalisedPercentage>");
        sb.AppendLine("<_positionX>"+_position.x+"</_positionX>");
        sb.AppendLine("<_positionY>"+_position.y+"</_positionY>");
        sb.AppendLine("<_positionZ>"+_position.z+"</_positionZ>");
        sb.AppendLine("<_splitControlPoints>"+_splitControlPoints+"</_splitControlPoints>");
        sb.AppendLine("<_forwardControlPointX>"+_forwardControlPoint.x+"</_forwardControlPointX>");
        sb.AppendLine("<_forwardControlPointY>"+_forwardControlPoint.y+"</_forwardControlPointY>");
        sb.AppendLine("<_forwardControlPointZ>"+_forwardControlPoint.z+"</_forwardControlPointZ>");
        sb.AppendLine("<_backwardControlPointX>"+_backwardControlPoint.x+"</_backwardControlPointX>");
        sb.AppendLine("<_backwardControlPointY>"+_backwardControlPoint.y+"</_backwardControlPointY>");
        sb.AppendLine("<_backwardControlPointZ>"+_backwardControlPoint.z+"</_backwardControlPointZ>");
        return sb.ToString();
    }

    public void FromXML(XmlNode node)
    {
        if(node["customName"].HasChildNodes)
            customName = node["customName"].FirstChild.Value;
        index = int.Parse(node["index"].FirstChild.Value);
        percentage = float.Parse(node["percentage"].FirstChild.Value);
        normalisedPercentage = float.Parse(node["normalisedPercentage"].FirstChild.Value);
        _position.x = float.Parse(node["_positionX"].FirstChild.Value);
        _position.y = float.Parse(node["_positionY"].FirstChild.Value);
        _position.z = float.Parse(node["_positionZ"].FirstChild.Value);
        _splitControlPoints = bool.Parse(node["_splitControlPoints"].FirstChild.Value);
        _forwardControlPoint.x = float.Parse(node["_forwardControlPointX"].FirstChild.Value);
        _forwardControlPoint.y = float.Parse(node["_forwardControlPointY"].FirstChild.Value);
        _forwardControlPoint.z = float.Parse(node["_forwardControlPointZ"].FirstChild.Value);
        _backwardControlPoint.x = float.Parse(node["_backwardControlPointX"].FirstChild.Value);
        _backwardControlPoint.y = float.Parse(node["_backwardControlPointY"].FirstChild.Value);
        _backwardControlPoint.z = float.Parse(node["_backwardControlPointZ"].FirstChild.Value);
    }
#endif

    public void CopyData(CameraPathControlPoint to)
    {
        to.customName = customName;
        to.index = index;
        to.percentage = percentage;
        to.normalisedPercentage = normalisedPercentage;
        to.worldPosition = worldPosition;
        to.splitControlPoints = _splitControlPoints;
        to.forwardControlPoint = _forwardControlPoint;
        to.backwardControlPoint = _backwardControlPoint;
    }
}
