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
using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CameraPath : MonoBehaviour
{
    public static float CURRENT_VERSION_NUMBER = 3.61f;
    public float version = CURRENT_VERSION_NUMBER;

    public enum PointModes
    {
        Transform,
        ControlPoints,
        FOV,
        Events,
        Speed,
        Delay,
        Ease,
        Orientations,
        Tilt,
        AddPathPoints,
        RemovePathPoints,
        AddOrientations,
        RemoveOrientations,
        TargetOrientation,
        AddFovs,
        RemoveFovs,
        AddTilts,
        RemoveTilts,
        AddEvents,
        RemoveEvents,
        AddSpeeds,
        RemoveSpeeds,
        AddDelays,
        RemoveDelays,
        Options
    }

    [SerializeField]
    private List<CameraPathControlPoint> _points = new List<CameraPathControlPoint>();

    public enum Interpolation
    {
        Linear,
        SmoothStep,
        CatmullRom,
        Hermite,
        Bezier
    }

    [SerializeField]
    private Interpolation _interpolation = Interpolation.Bezier;

    [SerializeField]
    private bool initialised;

    //this is the length of the arc of the entire bezier curve
    [SerializeField]
    private float _storedTotalArcLength;
    //this is an arroy of arc lengths in a point by point basis
    [SerializeField]
    private float[] _storedArcLengths;
    //this is an array of arc lenths are intervals specified by storedValueArraySize
    //it is the main data used in normalising the bezier curve to acheive a constant speed thoughout
    [SerializeField]
    private float[] _storedArcLengthsFull;

    [SerializeField]
    private Vector3[] _storedPoints;

    [SerializeField]
    private float[] _normalisedPercentages;

    //the unity distance of intervals to precalculate points
    //you can modify this number to get a faster output for RecalculateStoredValues
    //higher = faster recalculation/lower accuracy
    //lower = slower recalculation/higher accuracy
    [SerializeField]
    private float _storedPointResolution = 0.1f;//world units
    [SerializeField]
    private int _storedValueArraySize;//calculated from above based on path length and resolution

    [SerializeField]
    private Vector3[] _storedPathDirections;//a list of path directions stored for other calculation

    [SerializeField]
    private float _directionWidth = 0.05f;

    [SerializeField]
    private CameraPathControlPoint[] _pointALink = null;//a link to the point a for each stored point
    [SerializeField]
    private CameraPathControlPoint[] _pointBLink = null;//a link to the point a for each stored point

    [SerializeField]
    private CameraPathOrientationList _orientationList;

    [SerializeField]
    private CameraPathFOVList _fovList;//the list of FOV points

    [SerializeField]
    private CameraPathTiltList _tiltList;

    [SerializeField]
    private CameraPathSpeedList _speedList;

    [SerializeField]
    private CameraPathEventList _eventList;

    [SerializeField]
    private CameraPathDelayList _delayList;

    [SerializeField]
    private bool _addOrientationsWithPoints = true;
    
    [SerializeField]
    private bool _looped;//is the path looped

    [SerializeField]
    private bool _normalised = true;

    [SerializeField]
    private Bounds _pathBounds = new Bounds();

    public float hermiteTension = 0;
    public float hermiteBias = 0;

    //Editor Values
    public GameObject editorPreview = null;
    public int selectedPoint = 0;
    public PointModes pointMode = PointModes.Transform;
    public float addPointAtPercent = 0;
    [SerializeField]
    private float _aspect = 1.7778f;
    [SerializeField]
    private int _previewResolution = 800;//wide
    public float drawDistance = 1000;
    [SerializeField]
    private int _displayHeight = 225;

    public Texture2D previewOverlay = null;
    public bool ruleOfThirds = false;
    public Color ruleOfThirdsColour = Color.magenta;
    public Texture2D ruleOfThirdsOverlay;

    [SerializeField]
    private CameraPath _nextPath;//link this path to a second one

    [SerializeField]
    private bool _interpolateNextPath;//should we interpolate to that next path?

    //Camera Path Options
    public bool showGizmos = true;
    public Color selectedPathColour = CameraPathColours.WHITE;
    public Color unselectedPathColour = CameraPathColours.GREY;
    public Color selectedPointColour = CameraPathColours.RED;
    public Color unselectedPointColour = CameraPathColours.GREEN;
    public Color textColour = Color.white;
    public bool showOrientationIndicators = false;
    public float orientationIndicatorUnitLength = 2.5f;
    public Color orientationIndicatorColours = CameraPathColours.PURPLE;
    public bool autoSetStoedPointRes = true;
    public bool enableUndo = true;
    public bool showPreview = true;
    public bool enablePreviews = true;

    //Camera Path Events
    public delegate void RecalculateCurvesHandler();
    public delegate void PathPointAddedHandler(CameraPathControlPoint point);
    public delegate void PathPointRemovedHandler(CameraPathControlPoint point);
    public delegate void CheckStartPointCullHandler(float percentage);
    public delegate void CheckEndPointCullHandler(float percentage);
    public delegate void CleanUpListsHandler();

    public event RecalculateCurvesHandler RecalculateCurvesEvent;
    public event PathPointAddedHandler PathPointAddedEvent;
    public event PathPointRemovedHandler PathPointRemovedEvent;
    public event CheckStartPointCullHandler CheckStartPointCullEvent;
    public event CheckEndPointCullHandler CheckEndPointCullEvent;
    public event CleanUpListsHandler CleanUpListsEvent;

    /// <summary>
    /// Get a point in the path list
    /// Handles looping, next path interpolation and index outside of range
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public CameraPathControlPoint this[int index]
    {
        get
        {
            int pointCount = _points.Count;
            if(_looped)
            {
                if(shouldInterpolateNextPath)
                {
                    if(index == pointCount)
                        index = 0;
                    else if(index > pointCount)
                        return _nextPath[index%pointCount];
                    else if (index < 0)
                        Debug.LogError("Index out of range");
                }
                else
                {
                    index = index % pointCount;
                }
            }
            else
            {
                if (index < 0)
                    Debug.LogError("Index can't be minus");
                if (index >= _points.Count)
                {
                    if (index >= _points.Count && shouldInterpolateNextPath)
                        return nextPath[index % pointCount];
                    else 
                        Debug.LogError("Index out of range");
                }
            }
            return _points[index];
        }
    }

    /// <summary>
    /// The number of points in thie path including duplcates for looping or linked path interpolation
    /// </summary>
    public int numberOfPoints
    {
        get
        {
            if (_points.Count == 0)
                return 0;
            int output = (_looped) ? _points.Count + 1 : _points.Count;
            if (shouldInterpolateNextPath)
                output = output + 1;
            return output;
        }
    }

    /// <summary>
    /// The physical number of points this camera path has
    /// </summary>
    public int realNumberOfPoints { get { return _points.Count; } }

    /// <summary>
    /// The number of curves in this path including any additional curves generated by looping or linked paths
    /// </summary>
    public int numberOfCurves
    {
        get
        {
            if (_points.Count < 2)
                return 0;
            return numberOfPoints - 1;
        }
    }

    /// <summary>
    /// Does this path loop back on itself
    /// </summary>
    public bool loop
    {
        get { return _looped; }
        set
        {
            if (_looped != value)
            {
                _looped = value;
                RecalculateStoredValues();
            }
        }
    }

    /// <summary>
    /// The length in world units of the path
    /// </summary>
    public float pathLength { get { return _storedTotalArcLength; } }

    public CameraPathOrientationList orientationList {get {return _orientationList;} set { _orientationList = value; } }
    public CameraPathFOVList fovList {get {return _fovList;}}
    public CameraPathTiltList tiltList {get {return _tiltList;}}
    public CameraPathSpeedList speedList {get {return _speedList;}}
    public CameraPathEventList eventList {get {return _eventList;}}
    public CameraPathDelayList delayList {get {return _delayList;}}

    /// <summary>
    /// The bounds this path occupies
    /// </summary>
    public Bounds bounds {get {return _pathBounds;}}

    /// <summary>
    /// The arc length of a specified curve in world units
    /// </summary>
    /// <param name="curve">The index of the curve</param>
    /// <returns></returns>
    public float StoredArcLength(int curve)
    {
        if (_storedArcLengths.Length == 0) return 0.01f;
        if (_looped)
            curve = curve % (numberOfCurves-1);
        else
            curve = Mathf.Clamp(curve, 0, numberOfCurves - 1);
        curve = Mathf.Clamp(curve, 0, _storedArcLengths.Length - 1);
        return _storedArcLengths[curve];
    }

    public int storedValueArraySize
    {
        get
        {
            return _storedValueArraySize;
        }
    }

    public CameraPathControlPoint[] pointALink {get {return _pointALink;}}

    public CameraPathControlPoint[] pointBLink {get {return _pointBLink;}}

    public Vector3[] storedPoints {get {return _storedPoints;}}

    /// <summary>
    /// Is the path normalised so that speed and be constant throughout the animation
    /// </summary>
    public bool normalised
    {
        get
        {
            return _normalised;
        }
        set
        {
            _normalised = value;
        }
    }

    /// <summary>
    /// What kind of path interpolation is used for this path?
    /// </summary>
    public Interpolation interpolation
    {
        get {return _interpolation;} 
        set
        {
            if(value != _interpolation)
            {
                _interpolation = value;
                RecalculateStoredValues();
            }
        }
    }

    /// <summary>
    /// Link another Camera Path to the end of this one.
    /// </summary>
    public CameraPath nextPath
    {
        get {return _nextPath;} 
        set
        {
            if(value != _nextPath)
            {
                if(value == this)
                {
                    Debug.LogError("Do not link a path to itself! The Universe would crumble and it would be your fault!! If you want to loop a path, just toggle the loop option...");
                    return;
                }
                _nextPath = value;
                _nextPath.GetComponent<CameraPathAnimator>().playOnStart = false;
                RecalculateStoredValues();
            }
        }
    }

    /// <summary>
    /// Should we interpolate this path into a linked one
    /// </summary>
    public bool interpolateNextPath
    {
        get { return _interpolateNextPath; } 
        set
        {
            if(_interpolateNextPath != value)
            {
                _interpolateNextPath = value;
                RecalculateStoredValues();
            }
        }
    }

    public bool shouldInterpolateNextPath
    {
        get {return nextPath != null && interpolateNextPath;}
    }

    public float storedPointResolution
    {
        get {return _storedPointResolution;} 
        set {_storedPointResolution = Mathf.Clamp(value,_storedTotalArcLength/10000, 10);}
    }

    public float directionWidth {get {return _directionWidth;} set {_directionWidth = value;}}

    public int displayHeight
    {
        get {return _displayHeight;} 
        set {_displayHeight = Mathf.Clamp(value,100,500);}
    }

    public float aspect
    {
        get {return _aspect;} 
        set {_aspect = Mathf.Clamp(value,0.1f,10.0f);}
    }

    public int previewResolution 
    {
        get {return _previewResolution;} 
        set {_previewResolution = Mathf.Clamp(value,1,1024);}
    }

    public int StoredValueIndex(float percentage)
    {
        int max = storedValueArraySize - 1;
        return Mathf.Clamp(Mathf.RoundToInt(max * percentage), 0, max);
    }

    /// <summary>
    /// Add a point to the camera path by position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public CameraPathControlPoint AddPoint(Vector3 position)
    {
        CameraPathControlPoint point = gameObject.AddComponent<CameraPathControlPoint>();// ScriptableObject.CreateInstance<CameraPathControlPoint>();
        point.hideFlags = HideFlags.HideInInspector;
        point.localPosition = position;
        _points.Add(point);

        if (_addOrientationsWithPoints) orientationList.AddOrientation(point);
        RecalculateStoredValues();
        PathPointAddedEvent(point);
        return point;
    }

    /// <summary>
    /// Add a specified point to the camera path
    /// </summary>
    /// <param name="point"></param>
    public void AddPoint(CameraPathControlPoint point)
    {
        _points.Add(point);
        RecalculateStoredValues();
        PathPointAddedEvent(point);
    }

    /// <summary>
    /// Insert a specified point into the camera path at an index
    /// </summary>
    /// <param name="point"></param>
    /// <param name="index"></param>
    public void InsertPoint(CameraPathControlPoint point, int index)
    {
        _points.Insert(index, point);
        RecalculateStoredValues();
        PathPointAddedEvent(point);
    }

    public CameraPathControlPoint InsertPoint(int index)
    {
        CameraPathControlPoint point = gameObject.AddComponent<CameraPathControlPoint>();//ScriptableObject.CreateInstance<CameraPathControlPoint>();
        point.hideFlags = HideFlags.HideInInspector;
        _points.Insert(index, point);
        RecalculateStoredValues();
        PathPointAddedEvent(point);
        return point;
    }

    /// <summary>
    /// Remove a point from the path by specifing an index
    /// </summary>
    /// <param name="index">Index of the point to remove</param>
    public void RemovePoint(int index)
    {
        RemovePoint(this[index]);
    }

    /// <summary>
    /// Remove a point from the path by the name it's given
    /// This will be the custom name if given
    /// Otherwise if there is no custom name, it will test against the generated default name
    /// </summary>
    /// <param name="pointName">The name of a given point</param>
    /// <returns>Whether a point was removed</returns>
    public bool RemovePoint(string pointName)
    {
        foreach (CameraPathControlPoint point in _points)
        {
            if (point.displayName == pointName)
            {
                RemovePoint(point);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Remove a point from the path by specifing a world vector position
    /// If that position matches a point world position it is remove
    /// otherwise it will locate the nearest point to that position and remove that
    /// </summary>
    /// <param name="pointPosition">A world position</param>
    public void RemovePoint(Vector3 pointPosition)
    {
        foreach (CameraPathControlPoint point in _points)
        {
            if (point.worldPosition == pointPosition)
            {
                RemovePoint(point);
            }
        }
        float nearestPointPercentage = GetNearestPoint(pointPosition, true);
        RemovePoint(GetNearestPointIndex(nearestPointPercentage));
    }

    /// <summary>
    /// Remove a point from the path by specify the point to remove
    /// </summary>
    /// <param name="point">The point you want to remove</param>
    public void RemovePoint(CameraPathControlPoint point)
    {
        if (_points.Count < 3)
        {
            Debug.Log("We can't see any point in allowing you to delete any more points so we're not going to do it.");
            return;
        }
        PathPointRemovedEvent(point);

        int pointIndex = _points.IndexOf(point);
        if(pointIndex == 0)
        {
            //check other points
            float percentageCull = GetPathPercentage(1);
            CheckStartPointCullEvent(percentageCull);
        }
        if (pointIndex == realNumberOfPoints - 1)
        {
            //check other points
            float percentageCull = GetPathPercentage(realNumberOfPoints - 2);
            CheckEndPointCullEvent(percentageCull);
        }   

        _points.Remove(point);
        RecalculateStoredValues();
    }

    /// <summary>
    /// Parse a percent value so it can take into account any looping or normalisation
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <returns>A processed percentage</returns>
    private float ParsePercentage(float percentage)
    {
        if(percentage == 0)
            return 0;

        if(percentage == 1)
            return 1;

        if(_looped)
            percentage = percentage % 1.0f;
        else
            percentage = Mathf.Clamp01(percentage);

        if(_normalised)
        {
            int max = storedValueArraySize - 1;
            float storedPointSize = (1.0f / max);
            int normalisationIndex = Mathf.Clamp(Mathf.FloorToInt(max * percentage), 0, max);
            int nextNormalisationIndex = Mathf.Clamp(normalisationIndex + 1, 0, max);
            float normalisationPercentA = normalisationIndex * storedPointSize;
            float normalisationPercentB = nextNormalisationIndex * storedPointSize;
            float normPercentA = _normalisedPercentages[normalisationIndex];
            float normPercentB = _normalisedPercentages[nextNormalisationIndex];
            if (normPercentA == normPercentB) return normPercentA;
            float lerpValue = (percentage - normalisationPercentA) / (normalisationPercentB - normalisationPercentA);
            percentage = Mathf.Lerp(normPercentA, normPercentB, lerpValue);
        }
        return percentage;
    }

    /// <summary>
    /// Normalise the time based on the curve point
    /// Put in a time and it returns a time that will account for arc lengths
    /// Useful to ensure that path is animated at a constant speed
    /// </summary>
    /// <param name="percentage">Path Percentage - 0-1</param>
    /// <returns></returns>
    public float CalculateNormalisedPercentage(float percentage)
    {
        if(realNumberOfPoints < 2)
            return percentage;
        if (percentage <= 0)
            return 0;
        if (percentage >= 1)
            return 1;
        if(_storedTotalArcLength == 0)
            return percentage;

        float targetLength = percentage * _storedTotalArcLength;

        int low = 0;
        int high = storedValueArraySize-2;
        int index = 0;
        while (low < high)
        {
            index = low + ((high - low) / 2);
            if (_storedArcLengthsFull[index] < targetLength)
                low = index + 1;
            else
                high = index;
        }

        if (_storedArcLengthsFull[index] > targetLength && index > 0)
            index--;

        float lengthBefore = _storedArcLengthsFull[index];
        float currentT = (float)index / (float)(storedValueArraySize-1);
        if (lengthBefore == targetLength)
        {
            return currentT;
        }
        else
        {
            return (index + (targetLength - lengthBefore) / (_storedArcLengthsFull[index + 1] - lengthBefore)) / (storedValueArraySize);
        }
    }

    public float DeNormalisePercentage(float normalisedPercent)
    {
        int normPercetArrayLength = _normalisedPercentages.Length;
        for(int i = 0; i < normPercetArrayLength; i++)
        {
            if(_normalisedPercentages[i] > normalisedPercent)
            {
                if(i == 0)
                    return 0;

                float percentA = (i - 1) / (float)normPercetArrayLength;
                float percentB = (i) / (float)normPercetArrayLength;

                float nPercentA = _normalisedPercentages[i-1];
                float nPercentB = _normalisedPercentages[i];

                float lerp = (normalisedPercent - nPercentA) / (nPercentB - nPercentA);
                return Mathf.Lerp(percentA, percentB, lerp);
            }
        }
        return 1;
    }

    /// <summary>
    /// Get the index of a point at the start of a curve based on path percentage
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <returns>Index of point</returns>
    public int GetPointNumber(float percentage)
    {
        percentage = ParsePercentage(percentage);
        float curveT = 1.0f / numberOfCurves;
        return Mathf.Clamp(Mathf.FloorToInt(percentage / curveT), 0, (_points.Count - 1));
    }

    /// <summary>
    /// Get a normalised position based on a percent of the path
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <returns>Path Postion</returns>
    public Vector3 GetPathPosition(float percentage)
    {
        return GetPathPosition(percentage, false);
    }

    /// <summary>
    /// Get a position based on a percent of the path specifying the result will be normalised or not
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <param name="ignoreNormalisation">Should we ignore path normalisation</param>
    /// <returns>Path Postion</returns>
    public Vector3 GetPathPosition(float percentage, bool ignoreNormalisation)
    {
        if (realNumberOfPoints < 2)
        {
            Debug.LogError("Not enough points to define a curve");
            if(realNumberOfPoints == 1)
                return _points[0].worldPosition;
            return Vector3.zero;
        }
        if (!ignoreNormalisation) 
            percentage = ParsePercentage(percentage);
        float curveT = 1.0f / numberOfCurves;
        int point = Mathf.FloorToInt(percentage / curveT);
        float ct = Mathf.Clamp01((percentage - point * curveT) * numberOfCurves);
        CameraPathControlPoint pointA = GetPoint(point);
        CameraPathControlPoint pointB = GetPoint(point + 1);

        if (pointA == null || pointB == null)
            return Vector3.zero;

        CameraPathControlPoint pointC, pointD;
        switch(interpolation)
        {
            case Interpolation.Bezier:
                return CPMath.CalculateBezier(ct, pointA.worldPosition, pointA.forwardControlPointWorld, pointB.backwardControlPointWorld, pointB.worldPosition);

            case Interpolation.Hermite:
                pointC = GetPoint(point - 1);
                pointD = GetPoint(point + 2);
                return CPMath.CalculateHermite(pointC.worldPosition, pointA.worldPosition, pointB.worldPosition, pointD.worldPosition, ct, hermiteTension, hermiteBias);
            
            case Interpolation.CatmullRom:
                pointC = GetPoint(point - 1);
                pointD = GetPoint(point + 2);
                return CPMath.CalculateCatmullRom(pointC.worldPosition, pointA.worldPosition, pointB.worldPosition, pointD.worldPosition, ct);

            case Interpolation.SmoothStep:
                return Vector3.Lerp(pointA.worldPosition, pointB.worldPosition, CPMath.SmoothStep(ct));

            case Interpolation.Linear:
                return Vector3.Lerp(pointA.worldPosition, pointB.worldPosition, ct);
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Retreive a rotation from the orientation list
    /// </summary>
    /// <param name="percentage">Path Percentage</param>
    /// <returns>A path rotation</returns>
    public Quaternion GetPathRotation(float percentage, bool ignoreNormalisation)
    {
        if (!ignoreNormalisation)
            percentage = ParsePercentage(percentage);
        return orientationList.GetOrientation(percentage);
    }

    /// <summary>
    /// Retrive a path direction from stored values
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <returns>The direction of the path at this percent</returns>
    public Vector3 GetPathDirection(float percentage)
    {
        return GetPathDirection(percentage, true);
    }

    /// <summary>
    /// Retrive a path direction from stored values
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <param name="normalisePercent">Should we normalise the result</param>
    /// <returns>The direction of the path at this percent</returns>
    public Vector3 GetPathDirection(float percentage, bool normalisePercent)
    {
        int max = storedValueArraySize - 1;
        int indexa = Mathf.Clamp(Mathf.FloorToInt(max * percentage), 0, max);
        int indexb = Mathf.Clamp(Mathf.CeilToInt(max * percentage), 0, max);

        if(indexa == indexb)
            return _storedPathDirections[indexa];

        float percentA = indexa / (float)max;
        float percentB = indexb / (float)max;

        float lerpVal = (percentage - percentA) / (percentB - percentA);
        Vector3 dirA = _storedPathDirections[indexa];
        Vector3 dirB = _storedPathDirections[indexb];
        
        return Vector3.Lerp(dirA, dirB, lerpVal);
    }

    /// <summary>
    /// Retreive a tilt from the tilt list
    /// </summary>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public float GetPathTilt(float percentage)
    {
        percentage = ParsePercentage(percentage);
        return _tiltList.GetTilt(percentage);
    }

    /// <summary>
    /// Get the Field of View value from the path FOV list based on a percentage
    /// </summary>
    /// <param name="percentage">The path percentage (0-1)</param>
    /// <returns>A field of view value</returns>
    public float GetPathFOV(float percentage)
    {
        percentage = ParsePercentage(percentage);
        return _fovList.GetValue(percentage, CameraPathFOVList.ProjectionType.FOV);
    }

    /// <summary>
    /// Get the Orthogrphic size from the path FOV list based on a percentage
    /// </summary>
    /// <param name="percentage">The path percentage (0-1)</param>
    /// <returns>An orthographic size value</returns>
    public float GetPathOrthographicSize(float percentage)
    {
        percentage = ParsePercentage(percentage);
        return _fovList.GetValue(percentage, CameraPathFOVList.ProjectionType.Orthographic);
    }

    /// <summary>
    /// Get the Speed value from the path based on a path percent
    /// </summary>
    /// <param name="percentage">The path parcent point you wish to sample (0-1)</param>
    /// <returns>A speed value</returns>
    public float GetPathSpeed(float percentage)
    {
        percentage = ParsePercentage(percentage);
        float speed = _speedList.GetSpeed(percentage);
        speed *= _delayList.CheckEase(percentage);
        return speed;
    }

    /// <summary>
    /// Get the animation ease value at a specific point on the path
    /// </summary>
    /// <param name="percentage">The percent point you wish to sample from (0-1)</param>
    /// <returns>An ease value</returns>
    public float GetPathEase(float percentage)
    {
        percentage = ParsePercentage(percentage);
        float output = _delayList.CheckEase(percentage);
        return output;
    }

    /// <summary>
    /// Check the event list for any events that should have been fired since last call
    /// </summary>
    /// <param name="percentage">The current path percent 0-1</param>
    public void CheckEvents(float percentage)
    {
        percentage = ParsePercentage(percentage);
        _eventList.CheckEvents(percentage);
        _delayList.CheckEvents(percentage);
    }

    /// <summary>
    /// Get the unnormalised percent value at a point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public float GetPathPercentage(CameraPathControlPoint point)
    {
        int index = _points.IndexOf(point);
        return index / (float)numberOfCurves;
    }

    /// <summary>
    /// Get the unnormalised percent value at a point
    /// </summary>
    /// <param name="pointIndex"></param>
    /// <returns></returns>
    public float GetPathPercentage(int pointIndex)
    {
        return (pointIndex) / (float)(numberOfCurves);
    }

    /// <summary>
    /// Get the index of a point nearest to the specifiec percent
    /// </summary>
    /// <param name="percentage">The path percent  (0-1)</param>
    /// <returns>An index of a path point</returns>
    public int GetNearestPointIndex(float percentage)
    {
        percentage = ParsePercentage(percentage);
        return Mathf.RoundToInt(numberOfCurves * percentage);
    }

    /// <summary>
    /// Get the point index value based on a percent value
    /// </summary>
    /// <param name="percentage">The path percentage point</param>
    /// <param name="isNormalised">Should the percentage be normalised</param>
    /// <returns>The previous point on the Path</returns>
    public int GetLastPointIndex(float percentage, bool isNormalised)
    {
        if (isNormalised) percentage = ParsePercentage(percentage);
        return Mathf.FloorToInt(numberOfCurves * percentage);
    }

    /// <summary>
    /// Get the point index value based on a percent value
    /// </summary>
    /// <param name="percentage">The path percentage point</param>
    /// <param name="isNormalised">Should the percentage be normalised</param>
    /// <returns>The next point on the Path</returns>
    public int GetNextPointIndex(float percentage, bool isNormalised)
    {
        if (isNormalised) percentage = ParsePercentage(percentage);
        return Mathf.CeilToInt(numberOfCurves * percentage);
    }

    /// <summary>
    /// Get the percentage on the curve between two path points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public float GetCurvePercentage(CameraPathControlPoint pointA, CameraPathControlPoint pointB, float percentage)
    {
        float pointAPerc = GetPathPercentage(pointA);
        float pointBPerc = GetPathPercentage(pointB);
        if(pointAPerc == pointBPerc)
            return pointAPerc;
        if(pointAPerc > pointBPerc)//flip percentages if wrong way around
        {
            float newPointAPerc = pointBPerc;
            pointBPerc = pointAPerc;
            pointAPerc = newPointAPerc;
        }
        return Mathf.Clamp01((percentage - pointAPerc) / (pointBPerc - pointAPerc));
    }

    /// <summary>
    /// Get the percentage of the curve between two points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public float GetCurvePercentage(CameraPathPoint pointA, CameraPathPoint pointB, float percentage)
    {
        float pointAPerc = pointA.percent;
        float pointBPerc = pointB.percent;
        if (pointAPerc > pointBPerc)//flip percentages if wrong way around
        {
            float newPointAPerc = pointBPerc;
            pointBPerc = pointAPerc;
            pointAPerc = newPointAPerc;
        }
        return Mathf.Clamp01((percentage - pointAPerc) / (pointBPerc - pointAPerc));
    }

    /// <summary>
    /// Calculate the curve percenteage of a point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public float GetCurvePercentage(CameraPathPoint point)
    {
        float pointAPerc = GetPathPercentage(point.cpointA);
        float pointBPerc = GetPathPercentage(point.cpointB);
        if (pointAPerc > pointBPerc)//flip percentages if wrong way around
        {
            float newPointAPerc = pointBPerc;
            pointBPerc = pointAPerc;
            pointAPerc = newPointAPerc;
        }
        point.curvePercentage = Mathf.Clamp01((point.percent - pointAPerc) / (pointBPerc - pointAPerc));
        return point.curvePercentage;
    }

    /// <summary>
    /// Retrieve the ease value of any ease outros at the specified percentage
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public float GetOutroEasePercentage(CameraPathDelay point)
    {
        float pointAPerc = point.percent;
        float pointBPerc = _delayList.GetPoint(point.index + 1).percent;
        if (pointAPerc > pointBPerc)//flip percentages if wrong way around
        {
            float newPointAPerc = pointBPerc;
            pointBPerc = pointAPerc;
            pointAPerc = newPointAPerc;
        }
        return Mathf.Lerp(pointAPerc, pointBPerc, point.outroEndEasePercentage);
    }

    /// <summary>
    ///  Retrieve the ease value of any ease intros at the specified percentage
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public float GetIntroEasePercentage(CameraPathDelay point)
    {
        float pointAPerc = _delayList.GetPoint(point.index - 1).percent;
        float pointBPerc = point.percent;
        return Mathf.Lerp(pointAPerc, pointBPerc, 1-point.introStartEasePercentage);
    }

    /// <summary>
    /// Get the path percentage from a curve percent between two points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="curvePercentage"></param>
    /// <returns></returns>
    public float GetPathPercentage(CameraPathControlPoint pointA, CameraPathControlPoint pointB, float curvePercentage)
    {
        float pointAPerc = GetPathPercentage(pointA);
        float pointBPerc = GetPathPercentage(pointB);
        return Mathf.Lerp(pointAPerc, pointBPerc, curvePercentage);
    }

    /// <summary>
    /// Get the path percentage from a curve percent between two points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="curvePercentage"></param>
    /// <returns></returns>
    public float GetPathPercentage(float pointA, float pointB, float curvePercentage)
    {
        return Mathf.Lerp(pointA, pointB, curvePercentage);
    }

    /// <summary>
    /// Get a precalculated point inbex based on the path percentage
    /// </summary>
    /// <param name="percentage">Percentage point on path (0-1)</param>
    /// <returns>The index of a staored point</returns>
    public int GetStoredPoint(float percentage)
    {
        percentage = ParsePercentage(percentage);
        int returnIndex = Mathf.Clamp(Mathf.FloorToInt(storedValueArraySize* percentage),0,storedValueArraySize-1);
        return returnIndex;
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        if (!Application.isPlaying)
        {
            //Upgrade Camera Path Version
            if (version == CURRENT_VERSION_NUMBER)
            {
                //The data matches the current version of Buildr - do nothing.
                return;
            }

            if (version > CURRENT_VERSION_NUMBER)
            {
                Debug.LogError("Camera Path v." + version + ": Great scot! This data is from the future! (version:" + CURRENT_VERSION_NUMBER + ") - need to avoid contact to ensure the survival of the universe...");
                return;//Don't. Touch. ANYTHING!
            }

            Debug.Log("Camera Path v." + version + " Upgrading to version " + CURRENT_VERSION_NUMBER + "\nRemember to backup your data!");

            version = CURRENT_VERSION_NUMBER;//update the data version number once upgrade is complete
        }
    }

    private void OnValidate()
    {
        //on script recompilation
        InitialiseLists();
#if UNITY_EDITOR
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
            return;
#endif
        if (!Application.isPlaying)
            RecalculateStoredValues();
    }

    private void OnDestroy()
    {
        Clear();
        if(CleanUpListsEvent != null)
            CleanUpListsEvent();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!showGizmos)//if usser selected the option to hide gizmos - STOP!
            return;

        if(Selection.Contains(gameObject))//camera path selected - don't draw the gizmo outline
            return;

        if (numberOfCurves < 1)//no path to draw
            return;

        //Draw path outline
        Camera sceneCamera = Camera.current;
        if(sceneCamera == null)
            return;
        Gizmos.color = unselectedPathColour;

        float pointPercentage = 1.0f / (numberOfPoints-1);
        for (int i = 0; i < numberOfPoints - 1; i++)
        {
            CameraPathControlPoint pointA = GetPoint(i);
            CameraPathControlPoint pointB = GetPoint(i + 1);
            
            float dotPA = Vector3.Dot(sceneCamera.transform.forward, pointA.worldPosition - sceneCamera.transform.position);
            float dotPB = Vector3.Dot(sceneCamera.transform.forward, pointB.worldPosition - sceneCamera.transform.position);

            if (dotPA < 0 && dotPB < 0)//points are both behind camera - don't render
                continue;

            float pointAPercentage = pointPercentage * i;
            float pointBPercentage = pointPercentage * (i+1);
            float arcPercentage = pointBPercentage - pointAPercentage;
            Vector3 arcCentre = Vector3.Lerp(pointA.worldPosition, pointB.worldPosition, 0.5f);
            float arcLength = StoredArcLength(GetCurveIndex(pointA.index));
            if(arcLength < Mathf.Epsilon) continue;
            float arcDistance = Vector3.Distance(sceneCamera.transform.position, arcCentre);
            int arcPoints = Mathf.CeilToInt(Mathf.Min(arcLength, 50) / (Mathf.Max(arcDistance, 20)/2000));//Mathf.RoundToInt(arcLength * (40 / Mathf.Max(arcDistance, 20)));
            float arcTime = 1.0f / arcPoints;

            float endLoop = 1.0f - arcTime;
            Vector3 lastPoint = Vector3.zero;
            for (float p = 0; p <= endLoop; p += arcTime)
            {
                float p2 = p + arcTime;
                float pathPercentageA = pointAPercentage + arcPercentage * p;
                float pathPercentageB = pointAPercentage + arcPercentage * p2;
                Vector3 lineStart = GetPathPosition(pathPercentageA, true);
                Vector3 lineEnd = GetPathPosition(pathPercentageB, true);
                Gizmos.DrawLine(lineStart, lineEnd);
                lastPoint = lineEnd;
            }
            if(lastPoint==Vector3.zero)
                return;
            Gizmos.DrawLine(lastPoint, GetPathPosition(pointBPercentage, true));
        }
    }
#endif

    /// <summary>
    /// Calculate stored values that camera path uses
    /// Mostly this is used to establish a normalised curve so speed can be maintained.
    /// A few other functions are completed too like assigning values to points like name
    /// </summary>
    public void RecalculateStoredValues()
    {
        if(!_normalised)
        {
            _storedTotalArcLength = 0;
            _storedArcLengths = new float[0];
            _storedArcLengthsFull = new float[0];
            _storedPoints = new Vector3[0];
            _normalisedPercentages = new float[0];
            _storedPathDirections = new Vector3[0];
            return;
        }

        if(autoSetStoedPointRes && _storedTotalArcLength > 0)
            _storedPointResolution = _storedTotalArcLength / 1000.0f;//auto set this value so that long and short paths work fast

        //Assign basic values to points
        for (int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathControlPoint point = _points[i];
            point.percentage = GetPathPercentage(i);//assign point percentages
            point.normalisedPercentage = CalculateNormalisedPercentage(_points[i].percentage);//assign point percentages
            point.givenName = "Point " + i;
            point.fullName = name+ " Point " + i;
            point.index = i;
            point.hideFlags = HideFlags.HideInInspector;
        }

        if (_points.Count < 2)
            return;//nothing to cache

        //Calculate some rough arc lengths
        _storedTotalArcLength = 0;
        for (int i = 0; i < numberOfCurves; i++)
        {
            CameraPathControlPoint pointA = GetPoint(i);
            CameraPathControlPoint pointB = GetPoint(i+1);
            float thisArcLength = 0;
            thisArcLength += Vector3.Distance(pointA.worldPosition, pointA.forwardControlPointWorld);
            thisArcLength += Vector3.Distance(pointA.forwardControlPointWorld, pointB.backwardControlPointWorld);
            thisArcLength += Vector3.Distance(pointB.backwardControlPointWorld, pointB.worldPosition);
            _storedTotalArcLength += thisArcLength;
        }

        if(_storedTotalArcLength < Mathf.Epsilon) return;

        _storedValueArraySize = Mathf.Max(Mathf.RoundToInt(_storedTotalArcLength / _storedPointResolution), 1);
        float normilisePercentAmount = 1.0f / (_storedValueArraySize * 10);
        float normalisePercent = 0;
        float targetMovement = _storedTotalArcLength / (_storedValueArraySize - 1);

        List<Vector3> storedPoints = new List<Vector3>();
        List<Vector3> storedDirections = new List<Vector3>();
        List<float> normValues = new List<float>();
        List<float> storedArcLengths = new List<float>();
        List<float> storedArcLengthsFull = new List<float>();

        float currentLength = 0;
        float targetLength = targetMovement;
        float totalArcLength = 0;

        Vector3 pA = GetPathPosition(0, true), pB;

        storedPoints.Add(pA);



        float f0, f1;
        if (!_looped)
        {
            f0 = 0;
            f1 = Mathf.Clamp(normalisePercent + _directionWidth, 0, 1);
        }
        else
        {
            f0 = (normalisePercent - _directionWidth + 1) % 1;
            f1 = (normalisePercent + _directionWidth) % 1;
        }
        Vector3 initDirection = (GetPathPosition(f1, true) - GetPathPosition(f0, true)).normalized;

        storedDirections.Add(initDirection);
        normValues.Add(0);

        for (; normalisePercent < 1.0f; normalisePercent += normilisePercentAmount)
        {
            pB = GetPathPosition(normalisePercent, true);
            float arcLength = Vector3.Distance(pA, pB);

            if (currentLength + arcLength >= targetLength)
            {
                float lerpPoint = Mathf.Clamp01((targetLength - currentLength) / arcLength);

                float normValue = Mathf.Lerp(normalisePercent, normalisePercent + normilisePercentAmount, lerpPoint);
                normValues.Add(normValue);
                storedPoints.Add(pB);

                //calculate direction
                float xPercent, yPercent;
                if(!_looped)
                {
                    xPercent = Mathf.Clamp(normalisePercent - _directionWidth, 0, 1);
                    yPercent = Mathf.Clamp(normalisePercent + _directionWidth, 0, 1);
                }
                else
                {
                    xPercent = (normalisePercent - _directionWidth + 1) % 1;
                    yPercent = (normalisePercent + _directionWidth) % 1;
                }
                Vector3 pX = GetPathPosition(xPercent, true);
                Vector3 pY = GetPathPosition(yPercent, true);

                Vector3 pointDireciton = ((pA - pX) + (pY - pA)).normalized;
                storedDirections.Add(pointDireciton);

                storedArcLengths.Add(currentLength);
                storedArcLengthsFull.Add(totalArcLength);

                currentLength = targetLength;
                targetLength += targetMovement;
            }

            currentLength += arcLength;
            totalArcLength += arcLength;
            pA = pB;
        }
        normValues.Add(1);
        storedPoints.Add(GetPathPosition(1, true));
        
        if (!_looped)
        {
            f0 = Mathf.Clamp(normalisePercent - _directionWidth, 0, 1);
            f1 = 1;
        }
        else
        {
            f0 = (normalisePercent - _directionWidth + 1) % 1;
            f1 = (normalisePercent + _directionWidth) % 1;
        }

        Vector3 pf = GetPathPosition(f0, true);//penultimate position
        Vector3 ff = GetPathPosition(f1, true);//final position
        Vector3 finalDirection = (ff - pf).normalized;
        storedDirections.Add(finalDirection);

        _storedValueArraySize = normValues.Count;//storedPointSize
        _normalisedPercentages = normValues.ToArray();
        _storedTotalArcLength = totalArcLength;
        _storedPoints = storedPoints.ToArray();
        _storedPathDirections = storedDirections.ToArray();
        _storedArcLengths = storedArcLengths.ToArray();
        _storedArcLengthsFull = storedArcLengthsFull.ToArray();

        if (RecalculateCurvesEvent != null)
            RecalculateCurvesEvent();
    }

    /// <summary>
    /// Find the nearest point on the path to a point in world space
    /// </summary>
    /// <param name="fromPostition">A point in world space</param>
    /// <returns></returns>
    public float GetNearestPoint(Vector3 fromPostition)
    {
        return GetNearestPoint(fromPostition, false, 4);
    }

    /// <summary>
    /// Find the nearest point on the path to a point in world space
    /// </summary>
    /// <param name="fromPostition">A point in world space</param>
    /// <returns></returns>
    public float GetNearestPoint(Vector3 fromPostition, bool ignoreNormalisation)
    {
        return GetNearestPoint(fromPostition, ignoreNormalisation, 4);
    }

    /// <summary>
    /// Find the nearest point on the path to a point in world space
    /// </summary>
    /// <param name="fromPostition">A point in world space</param>
    /// <returns></returns>
    public float GetNearestPoint(Vector3 fromPostition, bool ignoreNormalisation, int refinments)
    {
        int testPoints = 10;
        float testResolution = 1.0f / testPoints;
        float nearestPercentage = 0;
        float nextNearestPercentage = 0;
        float nearestPercentageSqrDistance = Mathf.Infinity;
        float nextNearestPercentageSqrDistance = Mathf.Infinity;
        for (float i = 0; i < 1; i += testResolution)
        {
            Vector3 point = GetPathPosition(i, ignoreNormalisation);
            Vector3 difference = point - fromPostition;
            float newSqrDistance = Vector3.SqrMagnitude(difference);
            if (nearestPercentageSqrDistance > newSqrDistance)
            {
                nearestPercentage = i;
                nearestPercentageSqrDistance = newSqrDistance;
            }
        }
        nextNearestPercentage = nearestPercentage;
        nextNearestPercentageSqrDistance = nearestPercentageSqrDistance;
        int numberOfRefinments = refinments;// Mathf.RoundToInt(Mathf.Pow(pathLength * 10, 1.0f / 5.0f));
        for (int r = 0; r < numberOfRefinments; r++)
        {
            float searchSize = testResolution / 1.8f;
            float startSearch = nearestPercentage - searchSize;
            float endSearch = nearestPercentage + searchSize;
            float refinedResolution = testResolution / testPoints;
            for (float i = startSearch; i < endSearch; i += refinedResolution)
            {
                float perc = i % 1.0f;
                if(perc < 0)
                    perc += 1.0f;
                Vector3 point = GetPathPosition(perc, ignoreNormalisation);
                Vector3 difference = point - fromPostition;
                float newSqrDistance = Vector3.SqrMagnitude(difference);
                if (nearestPercentageSqrDistance > newSqrDistance)
                {
                    nextNearestPercentage = nearestPercentage;
                    nextNearestPercentageSqrDistance = nearestPercentageSqrDistance;

                    nearestPercentage = perc;
                    nearestPercentageSqrDistance = newSqrDistance;
                }
                else
                {
                    if(nextNearestPercentageSqrDistance > newSqrDistance)
                    {
                        nextNearestPercentage = perc;
                        nextNearestPercentageSqrDistance = newSqrDistance;
                    }
                }
            }
            testResolution = refinedResolution;
        }
        float lerpvalue = nearestPercentageSqrDistance / (nearestPercentageSqrDistance + nextNearestPercentageSqrDistance);
        return Mathf.Clamp01(Mathf.Lerp(nearestPercentage, nextNearestPercentage, lerpvalue));
    }

    /// <summary>
    /// Thanks to Antti Luukka for this! :)
    /// Retrieves the percentage nearest to the specified point
    /// Using a previous position
    /// </summary>
    /// <param name="fromPostition"></param>
    /// <param name="prevPercentage"></param>
    /// <param name="prevPosition"></param>
    /// <param name="ignoreNormalisation"></param>
    /// <param name="refinments"></param>
    /// <returns></returns>
    public float GetNearestPointNear(Vector3 fromPostition, float prevPercentage, Vector3 prevPosition, bool ignoreNormalisation, int refinments)
    {
        int testPoints = 10;
        float testResolution = 1.0f / testPoints;
        float nearestPercentage = prevPercentage;
        float nextNearestPercentage = nearestPercentage;
        float nearestPercentageSqrDistance = Vector3.SqrMagnitude(prevPosition - fromPostition);
        float nextNearestPercentageSqrDistance = nearestPercentageSqrDistance;

        int numberOfRefinments = refinments;// Mathf.RoundToInt(Mathf.Pow(pathLength * 10, 1.0f / 5.0f));
        for (int r = 0; r < numberOfRefinments; r++)
        {
            float searchSize = testResolution / 1.8f;
            float startSearch = nearestPercentage - searchSize;
            float endSearch = nearestPercentage + searchSize;
            float refinedResolution = testResolution / testPoints;
            for (float i = startSearch; i < endSearch; i += refinedResolution)
            {
                float perc = i % 1.0f;
                if (perc < 0)
                    perc += 1.0f;
                Vector3 point = GetPathPosition(perc, ignoreNormalisation);
                Vector3 difference = point - fromPostition;
                float newSqrDistance = Vector3.SqrMagnitude(difference);
                if (nearestPercentageSqrDistance > newSqrDistance)
                {
                    nextNearestPercentage = nearestPercentage;
                    nextNearestPercentageSqrDistance = nearestPercentageSqrDistance;

                    nearestPercentage = perc;
                    nearestPercentageSqrDistance = newSqrDistance;
                }
                else
                {
                    if (nextNearestPercentageSqrDistance > newSqrDistance)
                    {
                        nextNearestPercentage = perc;
                        nextNearestPercentageSqrDistance = newSqrDistance;
                    }
                }
            }
            testResolution = refinedResolution;
        }
        float lerpvalue = nearestPercentageSqrDistance / (nearestPercentageSqrDistance + nextNearestPercentageSqrDistance);
        return Mathf.Clamp01(Mathf.Lerp(nearestPercentage, nextNearestPercentage, lerpvalue));
    }

    /// <summary>
    /// Clear the path of points
    /// </summary>
    public void Clear()
    {
        _points.Clear();
    }

    /// <summary>
    /// Get a path point by specifing an index
    /// Looping, out of range indicies are properly handled
    /// </summary>
    /// <param name="index">The point index</param>
    /// <returns>The path point</returns>
    public CameraPathControlPoint GetPoint(int index)
    {
        return this[GetPointIndex(index)];
    }

    /// <summary>
    /// Get a path point index by specifing an index
    /// Looping, out of range indicies are properly handled
    /// </summary>
    /// <param name="index">The point index</param>
    /// <returns>The path point index</returns>
    public int GetPointIndex(int index)
    {
        if (_points.Count == 0)
            return -1;
        if (!_looped)
        {
            return Mathf.Clamp(index, 0, numberOfCurves);
        }
        if (index >= numberOfCurves)
            index = index - numberOfCurves;
        if (index < 0)
            index = index + numberOfCurves;

        return index;
    }

    /// <summary>
    /// Get the curve index based on a point index
    /// </summary>
    /// <param name="startPointIndex">The first point on the curve</param>
    /// <returns>The curve index</returns>
    public int GetCurveIndex(int startPointIndex)
    {
        if (_points.Count == 0)
            return -1;
        if (!_looped)
        {
            return Mathf.Clamp(startPointIndex, 0, numberOfCurves-1);
        }
        if (startPointIndex >= numberOfCurves - 1)
            startPointIndex = startPointIndex - numberOfCurves - 1;
        if (startPointIndex < 0)
            startPointIndex = startPointIndex + numberOfCurves - 1;

        return startPointIndex;
    }

    private void Init()
    {
        InitialiseLists();

        if(initialised)
            return;

        CameraPathControlPoint p0 = gameObject.AddComponent<CameraPathControlPoint>();
        CameraPathControlPoint p1 = gameObject.AddComponent<CameraPathControlPoint>();
        CameraPathControlPoint p2 = gameObject.AddComponent<CameraPathControlPoint>();
        CameraPathControlPoint p3 = gameObject.AddComponent<CameraPathControlPoint>();

        p0.hideFlags = HideFlags.HideInInspector;
        p1.hideFlags = HideFlags.HideInInspector;
        p2.hideFlags = HideFlags.HideInInspector;
        p3.hideFlags = HideFlags.HideInInspector;

        p0.localPosition = new Vector3(-20, 0, -20);
        p1.localPosition = new Vector3(20, 0, -20);
        p2.localPosition = new Vector3(20, 0, 20);
        p3.localPosition = new Vector3(-20, 0, 20);

        p0.forwardControlPoint = new Vector3(0, 0, -20);
        p1.forwardControlPoint = new Vector3(40, 0, -20);
        p2.forwardControlPoint = new Vector3(0, 0, 20);
        p3.forwardControlPoint = new Vector3(-40, 0, 20);

        AddPoint(p0);
        AddPoint(p1);
        AddPoint(p2);
        AddPoint(p3);

        initialised = true;
    }

    private void InitialiseLists()
    {
        if(_orientationList == null)
            _orientationList = gameObject.AddComponent<CameraPathOrientationList>();
        if (_fovList == null)
            _fovList = gameObject.AddComponent<CameraPathFOVList>();
        if (_tiltList == null)
            _tiltList = gameObject.AddComponent<CameraPathTiltList>();
        if (_speedList == null)
            _speedList = gameObject.AddComponent<CameraPathSpeedList>();
        if (_eventList == null)
            _eventList = gameObject.AddComponent<CameraPathEventList>();
        if (_delayList == null)
            _delayList = gameObject.AddComponent<CameraPathDelayList>();

        _orientationList.Init(this);
        _fovList.Init(this);
        _tiltList.Init(this);
        _speedList.Init(this);
        _eventList.Init(this);
        _delayList.Init(this);
    }


#if UNITY_EDITOR
    /// <summary>
    /// Convert this camera path into an xml string for export
    /// </summary>
    /// <returns>A generated XML string</returns>
    public string ToXML()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("<?xml version='1.0' encoding='ISO-8859-15'?>");
        sb.AppendLine("<!-- Unity3D Asset Camera Path Animator XML Exporter http://camerapathanimator.jasperstocker.com -->");
        sb.AppendLine("<camerapath>");

        sb.AppendLine("<positionx>"+transform.position.x+"</positionx>");
        sb.AppendLine("<positiony>"+transform.position.y+"</positiony>");
        sb.AppendLine("<positionz>"+transform.position.z+"</positionz>");

        sb.AppendLine("<interpolation>" + interpolation + "</interpolation>");
        sb.AppendLine("<linkedPath>" + ((_nextPath!=null)?_nextPath.name:"null") + "</linkedPath>");
        sb.AppendLine("<looped>" + _looped + "</looped>");
        sb.AppendLine("<normalised>" + _normalised + "</normalised>");
        sb.AppendLine("<hermiteTension>" + hermiteTension + "</hermiteTension>");
        sb.AppendLine("<hermiteBias>" + hermiteBias + "</hermiteBias>");
        sb.AppendLine("<aspect>" + _aspect + "</aspect>");
        sb.AppendLine("<previewResolution>" + _previewResolution + "</previewResolution>");
        sb.AppendLine("<drawDistance>" + drawDistance + "</drawDistance>");
        sb.AppendLine("<displayHeight>" + _displayHeight + "</displayHeight>");

        sb.Append(gameObject.GetComponent<CameraPathAnimator>().ToXML());

        sb.AppendLine("<controlpoints>");
        foreach (CameraPathControlPoint point in _points)
        {
            sb.AppendLine("<controlpoint>");
            sb.Append(point.ToXML());
            sb.AppendLine("</controlpoint>");
        }
        sb.AppendLine("</controlpoints>");

        sb.Append(_orientationList.ToXML());
        sb.Append(_tiltList.ToXML());
        sb.Append(_eventList.ToXML());
        sb.Append(_fovList.ToXML());
        sb.Append(_speedList.ToXML());
        sb.Append(_delayList.ToXML());

        sb.AppendLine("</camerapath>");

        return sb.ToString();
    }

    /// <summary>
    /// Import XML data into this camera path overwriting the current data
    /// </summary>
    /// <param name="XMLPath">An XML file path</param>
    public void FromXML(string XMLPath)
    {
        Debug.Log("Import Camera Path XML " + XMLPath);
        XmlDocument xml = new XmlDocument();
        using (StreamReader sr = new StreamReader(XMLPath))
        {
            xml.LoadXml(sr.ReadToEnd());
        }

        Vector3 newPosition = new Vector3();
        XmlNode cameraPathNode = xml.SelectNodes("camerapath")[0];
        newPosition.x = float.Parse(cameraPathNode["positionx"].FirstChild.Value);
        newPosition.y = float.Parse(cameraPathNode["positiony"].FirstChild.Value);
        newPosition.z = float.Parse(cameraPathNode["positionz"].FirstChild.Value);
        transform.position = newPosition;

        if (cameraPathNode["interpolation"] != null)
            interpolation = (Interpolation)Enum.Parse(typeof(Interpolation), cameraPathNode["interpolation"].FirstChild.Value);
        if(cameraPathNode["linkedPath"] != null)
        {
            GameObject nextPathInScene = GameObject.Find(cameraPathNode["linkedPath"].FirstChild.Value);
            if (nextPathInScene != null)
                _nextPath = nextPathInScene.GetComponent<CameraPath>();
        }
        if (cameraPathNode["looped"] != null)
            _looped = bool.Parse(cameraPathNode["looped"].FirstChild.Value);

        if (cameraPathNode["normalised"] != null)
            _normalised = bool.Parse(cameraPathNode["normalised"].FirstChild.Value);

        if (cameraPathNode["hermiteTension"] != null)
            hermiteTension = float.Parse(cameraPathNode["hermiteTension"].FirstChild.Value);

        if (cameraPathNode["hermiteBias"] != null)
            hermiteBias = float.Parse(cameraPathNode["hermiteBias"].FirstChild.Value);

        if (cameraPathNode["aspect"] != null)
            _aspect = float.Parse(cameraPathNode["aspect"].FirstChild.Value);

        if (cameraPathNode["previewResolution"] != null)
            _previewResolution = int.Parse(cameraPathNode["previewResolution"].FirstChild.Value);

        if (cameraPathNode["displayHeight"] != null)
            _displayHeight = int.Parse(cameraPathNode["displayHeight"].FirstChild.Value);

        if (cameraPathNode["drawDistance"] != null)
            drawDistance = float.Parse(cameraPathNode["drawDistance"].FirstChild.Value);

        _points.Clear();
        foreach (XmlNode node in xml.SelectNodes("camerapath/controlpoints/controlpoint"))
        {
            CameraPathControlPoint newControlPoint = gameObject.AddComponent<CameraPathControlPoint>();//ScriptableObject.CreateInstance<CameraPathControlPoint>();
            newControlPoint.hideFlags = HideFlags.HideInInspector;
            newControlPoint.FromXML(node);
            AddPoint(newControlPoint);
        }

        gameObject.GetComponent<CameraPathAnimator>().FromXML(xml.SelectNodes("camerapath/animator")[0]);

        _orientationList.FromXML(xml.SelectNodes("camerapath/Orientations/Orientation"));
        _tiltList.FromXML(xml.SelectNodes("camerapath/Tilts/Tilt"));
        _eventList.FromXML(xml.SelectNodes("camerapath/Events/Event"));
        _fovList.FromXML(xml.SelectNodes("camerapath/FOVs/FOV"));
        _speedList.FromXML(xml.SelectNodes("camerapath/Speeds/Speed"));
        _delayList.FromXML(xml.SelectNodes("camerapath/Delays/Delay"));
    }
#endif
}
