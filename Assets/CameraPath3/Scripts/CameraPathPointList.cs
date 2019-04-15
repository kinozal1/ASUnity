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
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Text;
using System.Xml;
#endif

[ExecuteInEditMode]
public class CameraPathPointList : MonoBehaviour
{

    [SerializeField]
    private List<CameraPathPoint> _points = new List<CameraPathPoint>();

    [SerializeField]
    protected CameraPath cameraPath;//a reference to the camera path class

    protected string pointTypeName = "point";//used in the naming of the points

    [NonSerialized]
    protected bool initialised = false;//ensure we're only initialising once per load - event get called multiple times otherwise

    private void OnEnable()
    {
        hideFlags = HideFlags.HideInInspector;
    }

    public virtual void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;
        hideFlags = HideFlags.HideInInspector;
        CheckListIsNull();
        cameraPath = _cameraPath;
        cameraPath.CleanUpListsEvent += CleanUp;
        cameraPath.RecalculateCurvesEvent += RecalculatePoints;
        cameraPath.PathPointRemovedEvent += PathPointRemovedEvent;
        cameraPath.CheckStartPointCullEvent += CheckPointCullEventFromStart;
        cameraPath.CheckEndPointCullEvent += CheckPointCullEventFromEnd;
        initialised = true;
    }

    public virtual void CleanUp()
    {
        cameraPath.CleanUpListsEvent -= CleanUp;
        cameraPath.RecalculateCurvesEvent -= RecalculatePoints;
        cameraPath.PathPointRemovedEvent -= PathPointRemovedEvent;
        cameraPath.CheckStartPointCullEvent -= CheckPointCullEventFromStart;
        cameraPath.CheckEndPointCullEvent -= CheckPointCullEventFromEnd;
        initialised = false;
    }

    /// <summary>
    /// The a point from the class by index. Deals with index issues gracefully
    /// </summary>
    /// <param name="index">Point Index</param>
    /// <returns></returns>
    public CameraPathPoint this[int index]
    {
        get
        {
            if (cameraPath.loop && index > _points.Count - 1)//loop value around
                index = index % _points.Count;
            if (index < 0)
                Debug.LogError("Index can't be minus");
            if (index >= _points.Count)
                Debug.LogError("Index out of range");
            return _points[index];
        }
    }

    /// <summary>
    /// Number of points in the list taking into account faking the number for looping
    /// </summary>
    public int numberOfPoints
    {
        get
        {
            if (_points.Count == 0)
                return 0;
            return (cameraPath.loop) ? _points.Count + 1 : _points.Count;
        }
    }

    /// <summary>
    /// the real number of points in the list
    /// </summary>
    public int realNumberOfPoints { get { return _points.Count; } }

    /// <summary>
    /// Get the index of a given point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public int IndexOf(CameraPathPoint point)
    {
        return _points.IndexOf(point);
    }

    /// <summary>
    /// Add a point into the list specified by a point on the curve between to points
    /// </summary>
    /// <param name="newPoint">Add this point!</param>
    /// <param name="curvePointA">On the curve from this point</param>
    /// <param name="curvePointB">On the curve to this point</param>
    /// <param name="curvePercetage">On the percent of this curve 0-1</param>
    public void AddPoint(CameraPathPoint newPoint, CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage)
    {
        newPoint.positionModes = CameraPathPoint.PositionModes.Free;
        newPoint.cpointA = curvePointA;
        newPoint.cpointB = curvePointB;
        newPoint.curvePercentage = curvePercetage;
        _points.Add(newPoint);
        RecalculatePoints();
    }

    public void AddPoint(CameraPathPoint newPoint, float fixPercent)
    {
        newPoint.positionModes = CameraPathPoint.PositionModes.FixedToPercent;
        newPoint.percent = fixPercent;
        _points.Add(newPoint);
        RecalculatePoints();
    }

    public void AddPoint(CameraPathPoint newPoint, CameraPathControlPoint atPoint)
    {
        newPoint.positionModes = CameraPathPoint.PositionModes.FixedToPoint;
        newPoint.point = atPoint;
        _points.Add(newPoint);
        RecalculatePoints();
    }

    public void RemovePoint(CameraPathPoint newPoint)
    {
        _points.Remove(newPoint);
        RecalculatePoints();
    }

    /// <summary>
    /// Check the free points and recalculate their values if a point have been added inside it's curve
    /// </summary>
    /// <param name="addedPoint">The added point to the path</param>
    public void PathPointAddedEvent(CameraPathControlPoint addedPoint)
    {
        float pointPercentage = addedPoint.percentage;
        for (int i = 0; i < realNumberOfPoints; i++)//Check freepoints have not been affected by the addition
        {
            CameraPathPoint point = _points[i];
            if(point.positionModes == CameraPathPoint.PositionModes.Free)
            {
                float cPointPercentageA = point.cpointA.percentage;
                float cPointPercentageB = point.cpointB.percentage;
                if(pointPercentage > cPointPercentageA && pointPercentage < cPointPercentageB)
                {
                    if(pointPercentage < point.percent)
                        //new point added before
                        point.cpointA = addedPoint;
                    else
                        //new point added after
                        point.cpointB = addedPoint;
                    cameraPath.GetCurvePercentage(point);//Recalculate free point values
                }
            }
        }
    }

    /// <summary>
    /// Check points and recalculate their values if the point being removed affects its position
    /// </summary>
    /// <param name="removedPathPoint">The point that will be removed</param>
    public void PathPointRemovedEvent(CameraPathControlPoint removedPathPoint)
    {
        for (int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathPoint point = _points[i];
            switch(point.positionModes)
            {
                case CameraPathPoint.PositionModes.FixedToPercent:
                    //do nothing
                    break;

                case CameraPathPoint.PositionModes.FixedToPoint://remove point if it's fixed to the removed one
                    if(point.point == removedPathPoint)
                    {
                        _points.Remove(point);
                        i--;
                    }
                    break;

                case CameraPathPoint.PositionModes.Free://recalculate point curves
                    if(point.cpointA == removedPathPoint)
                    {
                        CameraPathControlPoint earlierPoint = cameraPath.GetPoint(removedPathPoint.index - 1);
                        point.cpointA = earlierPoint;
                        cameraPath.GetCurvePercentage(point);
                    }

                    if(point.cpointB == removedPathPoint)
                    {
                        CameraPathControlPoint laterPoint = cameraPath.GetPoint(removedPathPoint.index + 1);
                        point.cpointB = laterPoint;
                        cameraPath.GetCurvePercentage(point);
                    }
                    break;
            }
        }
        RecalculatePoints();
    }

    public void CheckPointCullEventFromStart(float percent)
    {
        int numberOfPoints = _points.Count;
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathPoint point = _points[i];

            if(point.positionModes == CameraPathPoint.PositionModes.FixedToPercent)
                continue;//nothing affects these points

            if (point.percent < percent)//remove point
            {
                _points.Remove(point);
                i--;
                numberOfPoints--;
            }
        }
        RecalculatePoints();
    }

    public void CheckPointCullEventFromEnd(float percent)
    {
        int numberOfPoints = _points.Count;
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathPoint point = _points[i];

            if (point.positionModes == CameraPathPoint.PositionModes.FixedToPercent)
                continue;//nothing affects these points

            if (point.percent > percent)
            {
                _points.Remove(point);
                i--;
                numberOfPoints--;
            }
        }
        RecalculatePoints();
    }

    protected int GetNextPointIndex(float percent)
    {
        if (realNumberOfPoints == 0)
            Debug.LogError("No points to draw from");
        if (percent == 0)
            return 1;
        if(percent == 1)
            return _points.Count - 1;

        int numberOfPoints = _points.Count;
        int returnIndex = 0;
        for (int i = 1; i < numberOfPoints; i++)
        {
            if (_points[i].percent > percent)
                return returnIndex+1;
            returnIndex = i;
        }
        return returnIndex;
    }

    protected int GetLastPointIndex(float percent)
    {
        if (realNumberOfPoints == 0)
            Debug.LogError("No points to draw from");
        if (percent == 0)
            return 0;
        if (percent == 1)
            return (cameraPath.loop || cameraPath.shouldInterpolateNextPath) ? _points.Count - 1 : _points.Count - 2;

        int numberOfOrientations = _points.Count;
        int returnIndex = 0;
        for (int i = 1; i < numberOfOrientations; i++)
        {
            if (_points[i].percent > percent)
                return returnIndex;
            returnIndex = i;
        }
        return returnIndex;
    }

    public CameraPathPoint GetPoint(int index)
    {
        int numberOfPoints = _points.Count;
        if (numberOfPoints == 0)
            return null;

        CameraPathPointList list = this;
        if(cameraPath.shouldInterpolateNextPath)
        {
            switch(pointTypeName)
            {
                case "Orientation":
                    list = cameraPath.nextPath.orientationList;
                    break;

                case "FOV":
                    list = cameraPath.nextPath.fovList;
                    break;

                case "Tilt":
                    list = cameraPath.nextPath.tiltList;
                    break;
            }
        }

        if(list == this)//we're not interpolating next paths
        {
            if (!cameraPath.loop)
                return _points[Mathf.Clamp(index, 0, numberOfPoints - 1)];
            if (index >= numberOfPoints)
                index = index - numberOfPoints;
            if (index < 0)
                index = index + numberOfPoints;
        }
        else
        {
            if(cameraPath.loop)
            {
                if(index == numberOfPoints)
                {
                    index = 0;
                    list = null;//not using next path
                }
                else if(index > numberOfPoints)
                    index = Mathf.Clamp(index, 0, list.realNumberOfPoints - 1);
                else if(index < 0)
                {
                    index = index + numberOfPoints;
                    list = null;//not using next path
                }
                else
                    list = null;//not using next path
            }
            else
            {
                if (index > numberOfPoints - 1)
                    index = Mathf.Clamp(index - numberOfPoints, 0, list.realNumberOfPoints - 1);
                else if(index < 0)
                {
                    index = 0;
                    list = null;//not using next path
                }
                else
                {
                    index = Mathf.Clamp(index, 0, numberOfPoints - 1);
                    list = null;//not using next path
                }
            }
        }

        if(list != null)
            return list[index];
        else
            return _points[index];
    }

    public CameraPathPoint GetPoint(CameraPathControlPoint atPoint)
    {
        int numberOfPoints = _points.Count;
        if (numberOfPoints == 0)
            return null;
        foreach(CameraPathPoint point in _points)
        {
            if (point.positionModes == CameraPathPoint.PositionModes.FixedToPoint)
                if(point.point == atPoint)
                    return point;
        }
        return null;
    }

    public void Clear()
    {
        _points.Clear();
    }

    public CameraPathPoint DuplicatePointCheck()
    {
        foreach(CameraPathPoint thisPoint in _points)
            foreach(CameraPathPoint otherPoint in _points)
                if(thisPoint != otherPoint && thisPoint.percent == otherPoint.percent)
                    return thisPoint;//there is a duplicate point
        return null;
    }

    protected virtual void RecalculatePoints()
    {
        if(cameraPath == null)
        {
            Debug.LogError("Camera Path Point List was not initialised - run Init();");
            return;
        }

        int numberOfPoints = _points.Count;

        if(numberOfPoints == 0)
            //no points to recalculate
            return;

        List<CameraPathPoint> newPointList = new List<CameraPathPoint>() {};
        for(int i = 0; i < numberOfPoints; i++)
        {
            if(_points[i] == null)
                continue;
            CameraPathPoint point = _points[i];
            if(i == 0)
            {
                newPointList.Add(point);
                continue;
            }
            bool pointAdded = false;
            foreach (CameraPathPoint listPoint in newPointList)
            {
                if (listPoint.percent > point.percent)
                {
                    newPointList.Insert(newPointList.IndexOf(listPoint), point);
                    pointAdded = true;
                    break;
                }
            }
            if(!pointAdded)
                newPointList.Add(point);
        }

        numberOfPoints = newPointList.Count;
        _points = newPointList;
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathPoint point = _points[i];
            point.givenName = pointTypeName + " Point " + i;
            point.fullName = cameraPath.name +" "+pointTypeName + " Point " + i;
            point.index = i;
            if(cameraPath.realNumberOfPoints >= 2)
            {
                switch(point.positionModes)
                {
                    case CameraPathPoint.PositionModes.Free:

                        if(point.cpointA == point.cpointB)
                        {
                            point.positionModes = CameraPathPoint.PositionModes.FixedToPoint;
                            point.point = point.cpointA;
                            point.cpointA = null;
                            point.cpointB = null;
                            point.percent = point.point.percentage;
                            point.animationPercentage = (cameraPath.normalised) ? point.point.normalisedPercentage : point.point.percentage;
                            point.worldPosition = point.point.worldPosition;
                            return;
                        }

                        point.percent = cameraPath.GetPathPercentage(point.cpointA, point.cpointB, point.curvePercentage);
                        point.animationPercentage = (cameraPath.normalised) ? cameraPath.CalculateNormalisedPercentage(point.percent) : point.percent;
                        point.worldPosition = cameraPath.GetPathPosition(point.percent, true);
                        break;

                    case CameraPathPoint.PositionModes.FixedToPercent:
                        point.worldPosition = cameraPath.GetPathPosition(point.percent, true);
                        point.animationPercentage = (cameraPath.normalised) ? cameraPath.CalculateNormalisedPercentage(point.percent) : point.percent;
                        break;

                    case CameraPathPoint.PositionModes.FixedToPoint:
                        if(point.point == null)
                            point.point = cameraPath[cameraPath.GetNearestPointIndex(point.rawPercent)];
                        point.percent = point.point.percentage;
                        point.animationPercentage = (cameraPath.normalised) ? point.point.normalisedPercentage : point.point.percentage;
                        point.worldPosition = point.point.worldPosition;
                        break;
                }
            }
        }
    }

    public void ReassignCP(CameraPathControlPoint from, CameraPathControlPoint to)
    {
        foreach(CameraPathPoint point in _points)
        {
            if(point.point == from)
                point.point = to;
            if(point.cpointA == from)
                point.cpointA = to;
            if(point.cpointB == from)
                point.cpointB = to;
        }
    }

    protected void CheckListIsNull()
    {
        if(_points == null)
        {
            _points = new List<CameraPathPoint>();
        }
    }
    
#if UNITY_EDITOR
    public virtual string ToXML()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<" + pointTypeName + "s>");
        foreach(CameraPathPoint point in _points)
        {
            sb.AppendLine("<" + pointTypeName + ">");
            sb.Append(point.ToXML());
            sb.AppendLine("</" + pointTypeName + ">");
        }
        sb.AppendLine("</" + pointTypeName + "s>");
        return sb.ToString();
    }

    public virtual void FromXML(XmlNodeList nodes)
    {
        _points.Clear();
        foreach (XmlNode node in nodes)
        {
            CameraPathPoint newCameraPathPoint = gameObject.AddComponent<CameraPathPoint>();// CreateInstance<CameraPathPoint>();
            newCameraPathPoint.hideFlags = HideFlags.HideInInspector;
            CameraPathPoint.PositionModes positionModes = (CameraPathPoint.PositionModes)Enum.Parse(typeof(CameraPathPoint.PositionModes), node["positionModes"].FirstChild.Value);
            switch(positionModes)
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
