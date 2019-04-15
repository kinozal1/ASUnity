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
#endif

[ExecuteInEditMode]
public class CameraPathFOVList : CameraPathPointList
{
    public enum ProjectionType
    {
        FOV,
        Orthographic
    }

    public enum Interpolation
    {
        None,
        Linear,
        SmoothStep
    }

    public Interpolation interpolation = Interpolation.SmoothStep;

    private const float DEFAULT_FOV = 60;
    private const float DEFAULT_SIZE = 5;

    public bool listEnabled = false;

    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public override void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;
        pointTypeName = "FOV";
        base.Init(_cameraPath);
        cameraPath.PathPointAddedEvent += AddFOV;
        initialised = true;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        cameraPath.PathPointAddedEvent -= AddFOV;
        initialised = false;
    }

    public new CameraPathFOV this[int index]
    {
        get { return ((CameraPathFOV)(base[index])); }
    }

    public void AddFOV(CameraPathControlPoint atPoint)
    {
        CameraPathFOV fovpoint = gameObject.AddComponent<CameraPathFOV>();
        fovpoint.FOV = defaultFOV;
        fovpoint.Size = defaultSize;
        fovpoint.hideFlags = HideFlags.HideInInspector;
        AddPoint(fovpoint, atPoint);
        RecalculatePoints();
    }

    public CameraPathFOV AddFOV(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage, float fov, float size)
    {
        CameraPathFOV fovpoint = gameObject.AddComponent<CameraPathFOV>();
        fovpoint.hideFlags = HideFlags.HideInInspector;
        fovpoint.FOV = fov;
        fovpoint.Size = size;
        fovpoint.Size = defaultSize;
        AddPoint(fovpoint, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return fovpoint;
    }

    public float GetValue(float percentage, ProjectionType type)
    {
        if (realNumberOfPoints < 2)
        {
            if(type == ProjectionType.FOV)
            {
                if(realNumberOfPoints == 1)
                    return (this[0]).FOV;
                return defaultFOV;
            }
            else
            {
                if (realNumberOfPoints == 1)
                    return (this[0]).Size;
                return defaultSize;
            }
        }

        percentage = Mathf.Clamp(percentage, 0.0f, 1.0f);

        switch (interpolation)
        {
            case Interpolation.SmoothStep:
                return SmoothStepInterpolation(percentage, type);

            case Interpolation.Linear:
                return LinearInterpolation(percentage, type);

            default:
                return LinearInterpolation(percentage, type);
        }
    }

    private float LinearInterpolation(float percentage, ProjectionType projectionType)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathFOV pointP = (CameraPathFOV)GetPoint(index);
        CameraPathFOV pointQ = (CameraPathFOV)GetPoint(index + 1);

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        float valueA = (projectionType == ProjectionType.FOV) ? pointP.FOV : pointP.Size;
        float valueB = (projectionType == ProjectionType.FOV) ? pointQ.FOV : pointQ.Size;
        return Mathf.Lerp(valueA, valueB, ct);
    }

    private float SmoothStepInterpolation(float percentage, ProjectionType projectionType)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathFOV pointP = (CameraPathFOV)GetPoint(index);
        CameraPathFOV pointQ = (CameraPathFOV)GetPoint(index + 1);

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;

        float valueA = (projectionType == ProjectionType.FOV) ? pointP.FOV : pointP.Size;
        float valueB = (projectionType == ProjectionType.FOV) ? pointQ.FOV : pointQ.Size;
        return Mathf.Lerp(valueA, valueB, CPMath.SmoothStep(ct));
    }

    /// <summary>
    /// Attempt to find the camera in use for this scene and apply the field of view as default
    /// </summary>
    private float defaultFOV
    {
        get
        {
            if (Camera.current)
                return Camera.current.fieldOfView;

            Camera[] cams = Camera.allCameras;
            bool sceneHasCamera = cams.Length > 0;
            if (sceneHasCamera)
                return cams[0].fieldOfView;
            return DEFAULT_FOV;
        }
    }

    /// <summary>
    /// Attempt to find the camera in use for this scene and apply the field of view as default
    /// </summary>
    private float defaultSize
    {
        get
        {
            if (Camera.current)
                return Camera.current.orthographicSize;

            Camera[] cams = Camera.allCameras;
            bool sceneHasCamera = cams.Length > 0;
            if (sceneHasCamera)
                return cams[0].orthographicSize;
            return DEFAULT_SIZE;
        }
    }

#if UNITY_EDITOR
    public override void FromXML(XmlNodeList nodes)
    {
        Clear();
        foreach (XmlNode node in nodes)
        {
            CameraPathFOV newCameraPathPoint = gameObject.AddComponent<CameraPathFOV>();//CreateInstance<CameraPathFOV>();
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
