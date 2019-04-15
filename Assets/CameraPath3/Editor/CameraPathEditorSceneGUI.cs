// Camera Path 3
// Available on the Unity Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com/camera-path/
// For support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using CPA;
using UnityEngine;
using UnityEditor;

public class CameraPathEditorSceneGUI
{
    private const float LINE_RESOLUTION = 0.005f;
    private const float HANDLE_SCALE = 0.1f;

    public static CameraPath _cameraPath;
    public static CameraPathAnimator _animator;
    public static GUIStyle colouredText;

    public static int selectedPointIndex
    {
        get { return _cameraPath.selectedPoint; }
        set { _cameraPath.selectedPoint = value; }
    }

    public static CameraPath.PointModes _pointMode
    {
        get { return _cameraPath.pointMode; }
        set { _cameraPath.pointMode = value; }
    }

    public static void OnSceneGUI()
    {
        if(!_cameraPath.showGizmos)
            return;
        if(_cameraPath.transform.rotation != Quaternion.identity)
            return;

        _pointMode = _cameraPath.pointMode;

        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = false;

        //draw small point indicators
        Handles.color = CameraPathColours.GREY;
        int numberOfCPoints = _cameraPath.fovList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.fovList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                UnityVersionWrapper.HandlesArrowCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }
        numberOfCPoints = _cameraPath.delayList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.delayList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                UnityVersionWrapper.HandlesArrowCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }
        numberOfCPoints = _cameraPath.orientationList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.orientationList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                UnityVersionWrapper.HandlesArrowCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }
        numberOfCPoints = _cameraPath.speedList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.speedList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                UnityVersionWrapper.HandlesArrowCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }
        numberOfCPoints = _cameraPath.tiltList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.tiltList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                UnityVersionWrapper.HandlesArrowCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }

        //draw path outline
        Camera sceneCamera = Camera.current;
        int numberOfPoints = _cameraPath.numberOfPoints;
        Handles.color = _cameraPath.selectedPathColour;
        float pointPercentage = 1.0f / (numberOfPoints - 1);
        for(int i = 0; i < numberOfPoints-1; i++)
        {
            CameraPathControlPoint pointA = _cameraPath.GetPoint(i);
            CameraPathControlPoint pointB = _cameraPath.GetPoint(i+1);

            float dotPA = Vector3.Dot(sceneCamera.transform.forward, pointA.worldPosition - sceneCamera.transform.position);
            float dotPB = Vector3.Dot(sceneCamera.transform.forward, pointB.worldPosition - sceneCamera.transform.position);

            if (dotPA < 0 && dotPB < 0)//points are both behind camera - don't render
                continue;

            float pointAPercentage = pointPercentage * i;
            float pointBPercentage = pointPercentage * (i + 1);
            float arcPercentage = pointBPercentage - pointAPercentage;
            Vector3 arcCentre = (pointA.worldPosition + pointB.worldPosition) * 0.5f;
            float arcLength = _cameraPath.StoredArcLength(_cameraPath.GetCurveIndex(pointA.index));
            if(arcLength < Mathf.Epsilon)
                continue;
            float arcDistance = Vector3.Distance(sceneCamera.transform.position, arcCentre);
            int arcPoints = Mathf.Max(Mathf.RoundToInt(arcLength * (40 / Mathf.Max(arcDistance,20))), 10);
            float arcTime = 1.0f / arcPoints;

            float endLoop = 1.0f - arcTime;
            Vector3 lastPoint = Vector3.zero;
            for (float p = 0; p < endLoop; p += arcTime)
            {
                float p2 = p + arcTime;
                float pathPercentageA = pointAPercentage + arcPercentage * p;
                float pathPercentageB = pointAPercentage + arcPercentage * p2;
                Vector3 lineStart = _cameraPath.GetPathPosition(pathPercentageA, true);
                Vector3 lineEnd = _cameraPath.GetPathPosition(pathPercentageB, true);

                Handles.DrawLine(lineStart, lineEnd);

                lastPoint = lineEnd;
            }
            Handles.DrawLine(lastPoint, _cameraPath.GetPathPosition(pointBPercentage, true));
        }

        switch(_pointMode)
        {
            case CameraPath.PointModes.Transform:
                SceneGUIPointBased();
                break;

            case CameraPath.PointModes.ControlPoints:
                    SceneGUIPointBased();
                break;

            case CameraPath.PointModes.Orientations:
                SceneGUIOrientationBased();
                break;

            case CameraPath.PointModes.FOV:
                SceneGUIFOVBased();
                break;

            case CameraPath.PointModes.Events:
                SceneGUIEventBased();
                break;

            case CameraPath.PointModes.Speed:
                SceneGUISpeedBased();
                break;

            case CameraPath.PointModes.Tilt:
                SceneGUITiltBased();
                break;

            case CameraPath.PointModes.Delay:
                SceneGUIDelayBased();
                break;

            case CameraPath.PointModes.Ease:
                SceneGUIEaseBased();
                break;

            case CameraPath.PointModes.AddPathPoints:
                AddPathPoints();
                break;

            case CameraPath.PointModes.RemovePathPoints:
                RemovePathPoints();
                break;

            case CameraPath.PointModes.AddOrientations:
                AddCPathPoints();
                break;

            case CameraPath.PointModes.AddFovs:
                AddCPathPoints();
                break;

            case CameraPath.PointModes.AddTilts:
                AddCPathPoints();
                break;

            case CameraPath.PointModes.AddEvents:
                AddCPathPoints();
                break;

            case CameraPath.PointModes.AddSpeeds:
                AddCPathPoints();
                break;

            case CameraPath.PointModes.AddDelays:
                AddCPathPoints();
                break;

            case CameraPath.PointModes.RemoveOrientations:
                RemoveCPathPoints();
                break;

            case CameraPath.PointModes.RemoveTilts:
                RemoveCPathPoints();
                break;

            case CameraPath.PointModes.RemoveFovs:
                RemoveCPathPoints();
                break;

            case CameraPath.PointModes.RemoveEvents:
                RemoveCPathPoints();
                break;

            case CameraPath.PointModes.RemoveSpeeds:
                RemoveCPathPoints();
                break;

            case CameraPath.PointModes.RemoveDelays:
                RemoveCPathPoints();
                break;

        }

        
        if (Event.current.type == EventType.ValidateCommand)
        {
            switch (Event.current.commandName)
            {
                case "UndoRedoPerformed":
                    GUI.changed = true;
                    break;
            }
        }
    }

    private static void SceneGUIPointBased()
    {
        Camera sceneCamera = Camera.current;
        int realNumberOfPoints = _cameraPath.realNumberOfPoints;
        for (int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathControlPoint point = _cameraPath[i];
            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            if (_cameraPath.enableUndo) Undo.RecordObject(point, "Modifying Path Point");
            Handles.Label(point.worldPosition, point.displayName+"\n"+(point.percentage*100).ToString("F1")+"%", colouredText);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (UnityVersionWrapper.HandlesDotButton(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize))
            {
                if (i == selectedPointIndex)
                    _cameraPath.pointMode = CameraPath.PointModes.Transform;
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            if(i == selectedPointIndex)
            {
                if (_pointMode == CameraPath.PointModes.Transform || _cameraPath.interpolation != CameraPath.Interpolation.Bezier)
                {
                    Vector3 currentPosition = point.worldPosition;
                    currentPosition = Handles.DoPositionHandle(currentPosition, Quaternion.identity);
                    point.worldPosition = currentPosition;

//                    SerializedObject so = new SerializedObject(point);
//                    SerializedProperty pointPosition = so.FindProperty("_position");
//                    Vector3 currentWorldPosition = point.worldPosition;
//                    currentWorldPosition = Handles.DoPositionHandle(currentWorldPosition, Quaternion.identity);
//                    pointPosition.vector3Value = point.WorldToLocalPosition(currentWorldPosition);

                    if(_cameraPath.interpolation == CameraPath.Interpolation.Bezier)
                    {
                        Handles.color = CameraPathColours.DARKGREY;
                        float pointSize = pointHandleSize * 0.5f;
                        Handles.DrawLine(point.worldPosition, point.forwardControlPointWorld);
                        Handles.DrawLine(point.worldPosition, point.backwardControlPointWorld);
                        if (UnityVersionWrapper.HandlesDotButton(point.forwardControlPointWorld, Quaternion.identity, pointSize, pointSize))
                            _cameraPath.pointMode = CameraPath.PointModes.ControlPoints;
                        if (UnityVersionWrapper.HandlesDotButton(point.backwardControlPointWorld, Quaternion.identity, pointSize, pointSize))
                            _cameraPath.pointMode = CameraPath.PointModes.ControlPoints;
                    }
                }
                else
                {
                    //Backward ControlPoints point - render first so it's behind the forward
                    Handles.DrawLine(point.worldPosition, point.backwardControlPointWorld);
                    point.backwardControlPointWorld = Handles.DoPositionHandle(point.backwardControlPointWorld, Quaternion.identity);
                    if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) > 0)
                        Handles.Label(point.backwardControlPoint, "point " + i + " reverse ControlPoints point", colouredText);

                    //Forward ControlPoints point
                    if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) > 0)
                        Handles.Label(point.forwardControlPoint, "point " + i + " ControlPoints point", colouredText);
                    Handles.color = _cameraPath.selectedPointColour;
                    Handles.DrawLine(point.worldPosition, point.forwardControlPointWorld);
                    point.forwardControlPointWorld = Handles.DoPositionHandle(point.forwardControlPointWorld, Quaternion.identity);
                    
                }
            }
        }
    }

    private static void SceneGUIOrientationBased()
    {
        DisplayAtPoint();

        CameraPathOrientationList orientationList = _cameraPath.orientationList;
        Camera sceneCamera = Camera.current;
        int orientationCount = orientationList.realNumberOfPoints;
        for (int i = 0; i < orientationCount; i++)
        {
            CameraPathOrientation orientation = orientationList[i];
            if (_cameraPath.enableUndo) Undo.RecordObject(orientation, "Modifying Orientation Point");
            if (Vector3.Dot(sceneCamera.transform.forward, orientation.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string orientationLabel = orientation.displayName;
            orientationLabel += "\nat percentage: " + orientation.percent.ToString("F3");
            switch(orientation.positionModes)
            {
                case CameraPathPoint.PositionModes.FixedToPoint:
                    orientationLabel += "\nat point: " + orientation.point.displayName;
                    break;
            }

            Handles.Label(orientation.worldPosition, orientationLabel, colouredText);
            float pointHandleSize = HandleUtility.GetHandleSize(orientation.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? Color.blue : _cameraPath.unselectedPointColour;
            UnityVersionWrapper.HandlesArrowCap(0, orientation.worldPosition, orientation.rotation, pointHandleSize * 4);

            if(i == selectedPointIndex)
            {
                //up arrow
                Handles.color = Color.green;
                Quaternion arrowUp = orientation.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.up);
                UnityVersionWrapper.HandlesArrowCap(0, orientation.worldPosition, arrowUp, pointHandleSize * 4);

                //right arrow
                Handles.color = Color.red;
                Quaternion arrowRight = orientation.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.right);
                UnityVersionWrapper.HandlesArrowCap(0, orientation.worldPosition, arrowRight, pointHandleSize * 4);
            }

            if (UnityVersionWrapper.HandlesDotButton(orientation.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            if (i == selectedPointIndex)
            {
                Quaternion currentRotation = orientation.rotation;
                currentRotation = Handles.DoRotationHandle(currentRotation, orientation.worldPosition);
                if (currentRotation != orientation.rotation)
                {
                    orientation.rotation = currentRotation;
                }
                CPPSlider(orientation);
            }
        }
        
        if(_cameraPath.showOrientationIndicators)//draw orientation indicators
        {
            Handles.color = _cameraPath.orientationIndicatorColours;
            float indicatorLength = _cameraPath.orientationIndicatorUnitLength / _cameraPath.pathLength;
            for(float i = 0; i < 1; i += indicatorLength)
            {
                Vector3 indicatorPosition = _cameraPath.GetPathPosition(i);
                Quaternion inicatorRotation = _cameraPath.GetPathRotation(i,false);
                float indicatorHandleSize = HandleUtility.GetHandleSize(indicatorPosition) * HANDLE_SCALE * 4;
                UnityVersionWrapper.HandlesArrowCap(0, indicatorPosition, inicatorRotation, indicatorHandleSize);
            }
        }
    }

    private static void SceneGUIFOVBased()
    {
        DisplayAtPoint();

        CameraPathFOVList fovList = _cameraPath.fovList;
        Camera sceneCamera = Camera.current;
        int pointCount = fovList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathFOV fovPoint = fovList[i];
            if (_cameraPath.enableUndo) Undo.RecordObject(fovPoint, "Modifying FOV Point");
            if (Vector3.Dot(sceneCamera.transform.forward, fovPoint.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = fovPoint.displayName;
            pointLabel += "\nvalue: " + fovPoint.FOV.ToString("F1");
            if (fovPoint.positionModes == CameraPathPoint.PositionModes.FixedToPoint) pointLabel += "\nat point: " + fovPoint.point.displayName;
            else pointLabel += "\nat percentage: " + fovPoint.percent.ToString("F3");

            Handles.Label(fovPoint.worldPosition, pointLabel, colouredText);
            float pointHandleSize = HandleUtility.GetHandleSize(fovPoint.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (UnityVersionWrapper.HandlesDotButton(fovPoint.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            if (i == selectedPointIndex)
            {
                CPPSlider(fovPoint);
            }
        }
    }

    private static void SceneGUIEventBased()
    {
        DisplayAtPoint();

        CameraPathEventList eventList = _cameraPath.eventList;
        Camera sceneCamera = Camera.current;
        int pointCount = eventList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathEvent eventPoint = eventList[i];
            if (_cameraPath.enableUndo) Undo.RecordObject(eventPoint, "Modifying Event Point");
            if (Vector3.Dot(sceneCamera.transform.forward, eventPoint.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = eventPoint.displayName;
            pointLabel += "\ntype: " + eventPoint.type;
            if (eventPoint.type == CameraPathEvent.Types.Broadcast) pointLabel += "\nevent name: " + eventPoint.eventName;
            if (eventPoint.type == CameraPathEvent.Types.Call)
            {
                if (eventPoint.target != null)
                    pointLabel += "\nevent target: " + eventPoint.target.name + " calling: " + eventPoint.methodName;
                else
                    pointLabel += "\nno target assigned";
            }
            if (eventPoint.positionModes == CameraPathPoint.PositionModes.FixedToPoint) pointLabel += "\nat point: " + eventPoint.point.displayName;
            else pointLabel += "\nat percentage: " + eventPoint.percent.ToString("F3");

            Handles.Label(eventPoint.worldPosition, pointLabel, colouredText);
            float pointHandleSize = HandleUtility.GetHandleSize(eventPoint.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (UnityVersionWrapper.HandlesDotButton(eventPoint.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            if(i == selectedPointIndex)
            {
                CPPSlider(eventPoint);
            }
        }
    }

    private static void SceneGUISpeedBased()
    {
        DisplayAtPoint();

        CameraPathSpeedList pointList = _cameraPath.speedList;
        Camera sceneCamera = Camera.current;
        int pointCount = pointList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathSpeed point = pointList[i];
            if (_cameraPath.enableUndo) Undo.RecordObject(point, "Modifying Speed Point");
            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = point.displayName;
            pointLabel += "\nvalue: " + point.speed + " m/s";
            pointLabel += "\npercent: " + point.percent;
            pointLabel += "\na percent: " + _cameraPath.DeNormalisePercentage(point.percent);

            Handles.Label(point.worldPosition, pointLabel, colouredText);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (UnityVersionWrapper.HandlesDotButton(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            if (i == selectedPointIndex)
            {
                CPPSlider(point);
            }
        }

    }

    private static void SceneGUITiltBased()
    {
        DisplayAtPoint();

        CameraPathTiltList pointList = _cameraPath.tiltList;
        Camera sceneCamera = Camera.current;
        int pointCount = pointList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathTilt point = pointList[i];
            if (_cameraPath.enableUndo) Undo.RecordObject(point, "Modifying Tilt Point");
            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = point.displayName;
            pointLabel += "\nvalue: " + point.tilt.ToString("F1") + "\u00B0";

            Handles.Label(point.worldPosition, pointLabel, colouredText);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            bool pointIsSelected = i == selectedPointIndex;
            Handles.color = (pointIsSelected) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;

            float tiltSize = 2.0f;
            Vector3 pointForwardDirection = _cameraPath.GetPathDirection(_cameraPath.DeNormalisePercentage(point.percent));
            Quaternion qTilt = Quaternion.AngleAxis(-point.tilt, pointForwardDirection);
            Quaternion pointForward = Quaternion.LookRotation(pointForwardDirection);
            UnityVersionWrapper.HandlesCircleCap(0, point.worldPosition, pointForward, tiltSize);
            Vector3 horizontalLineDirection = ((qTilt * Quaternion.AngleAxis(-90, Vector3.up)) * pointForwardDirection).normalized * tiltSize;
            Vector3 horizontalLineStart = point.worldPosition + horizontalLineDirection;
            Vector3 horizontalLineEnd = point.worldPosition - horizontalLineDirection;
            Handles.DrawLine(horizontalLineStart, horizontalLineEnd);

            Vector3 verticalLineDirection = (Quaternion.AngleAxis(-90, pointForwardDirection) * horizontalLineDirection).normalized * tiltSize;
            Vector3 verticalLineStart = point.worldPosition + verticalLineDirection;
            Vector3 verticalLineEnd = point.worldPosition;
            Handles.DrawLine(verticalLineStart, verticalLineEnd);

            if (UnityVersionWrapper.HandlesDotButton(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            if (i == selectedPointIndex)
            {
                CPPSlider(point);
            }
        }

        if (_cameraPath.showOrientationIndicators)//draw orientation indicators
        {
            Handles.color = _cameraPath.orientationIndicatorColours;
            float indicatorLength = _cameraPath.orientationIndicatorUnitLength / _cameraPath.pathLength;
            for (float i = 0; i < 1; i += indicatorLength)
            {
                Vector3 indicatorPosition = _cameraPath.GetPathPosition(i);
                Quaternion inicatorRotation = Quaternion.LookRotation(_cameraPath.GetPathDirection(_cameraPath.DeNormalisePercentage(i), false));
                float indicatorHandleSize = HandleUtility.GetHandleSize(indicatorPosition) * HANDLE_SCALE * 4;
                UnityVersionWrapper.HandlesArrowCap(0, indicatorPosition, inicatorRotation, indicatorHandleSize);
            }
        }
    }

    private static void SceneGUIDelayBased()
    {
        DisplayAtPoint();

        CameraPathDelayList pointList = _cameraPath.delayList;
        Camera sceneCamera = Camera.current;
        int pointCount = pointList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathDelay point = pointList[i];
            if (_cameraPath.enableUndo) Undo.RecordObject(point, "Modifying Delay Point");

            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = "";
            if(point == pointList.introPoint)
            {
                pointLabel += "start point";
                if (point.time > 0)
                    pointLabel += "\ndelay: " + point.time.ToString("F2") + " sec";
                else
                    pointLabel += "\nNo delay";
            }
            else if (point == pointList.outroPoint)
                pointLabel += "end point";
            else
            {
                pointLabel += point.displayName;

                if (point.time > 0)
                    pointLabel += "\ndelay: " + point.time.ToString("F2") + " sec";
                else
                    pointLabel += "\ndelay indefinitely";
            }

            Handles.Label(point.worldPosition, pointLabel, colouredText);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (UnityVersionWrapper.HandlesDotButton(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            if (i == selectedPointIndex)
            {
                CPPSlider(point);
            }
        }
    }

    private static void SceneGUIEaseBased()
    {
        CameraPathDelayList pointList = _cameraPath.delayList;
        Camera sceneCamera = Camera.current;
        int pointCount = pointList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathDelay point = pointList[i];
            if (_cameraPath.enableUndo) Undo.RecordObject(point, "Modifying Ease Curves");

            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = "";
            if (point == pointList.introPoint)
                pointLabel += "start point";
            else if (point == pointList.outroPoint)
                pointLabel += "end point";
            else
            {
                pointLabel += point.displayName;

                if (point.time > 0)
                    pointLabel += "\ndelay: " + point.time.ToString("F2") + " sec";
                else
                    pointLabel += "\ndelay indefinitely";
            }

            Handles.Label(point.worldPosition, pointLabel, colouredText);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (UnityVersionWrapper.HandlesDotButton(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

//            float unitPercent = 0.5f;
            Vector3 easeUp = Vector3.up * _cameraPath.pathLength * 0.1f;
            Handles.color = CameraPathColours.RED;
            if (point != pointList.outroPoint)
            {
                float outroEasePointPercent = _cameraPath.GetOutroEasePercentage(point);
                Vector3 outroEasePoint = _cameraPath.GetPathPosition(outroEasePointPercent, true);
                Vector3 outroeaseDirection = _cameraPath.GetPathDirection(outroEasePointPercent, false);

                Handles.Label(outroEasePoint, "Ease Out\n" + point.displayName, colouredText);
                Vector3 newPosition = Handles.Slider(outroEasePoint, outroeaseDirection);

                float movement = Vector3.Distance(outroEasePoint, newPosition);
                if (movement > Mathf.Epsilon)
                {
                    float newPercent = NearestmMousePercentage();
                    float curvePercent = _cameraPath.GetCurvePercentage(_cameraPath.delayList.GetPoint(point.index), _cameraPath.delayList.GetPoint(point.index + 1), newPercent);
                    point.outroEndEasePercentage = curvePercent;
                }

                float percentWidth = (outroEasePointPercent - point.percent);
//                float easeSpace = _cameraPath.pathLength * percentWidth;
//                float easeLength = unitPercent / percentWidth;
                float percentMovement = percentWidth / 10.0f;
                for (float e = point.percent; e < outroEasePointPercent; e += percentMovement)
                {
                    float eB = e + percentMovement;
                    Vector3 lineStart = _cameraPath.GetPathPosition(e, true);
                    Vector3 lineEnd = _cameraPath.GetPathPosition(eB, true);
                    Handles.DrawLine(lineStart,lineEnd);
                    float animCurvePercentA = (e - point.percent) / percentWidth;
                    float animCurvePercentB = (eB - point.percent) / percentWidth;
                    Vector3 lineEaseUpA = easeUp * point.outroCurve.Evaluate(animCurvePercentA);
                    Vector3 lineEaseUpB = easeUp * point.outroCurve.Evaluate(animCurvePercentB);
                    Handles.DrawLine(lineStart + lineEaseUpA, lineEnd + lineEaseUpB);
                }
            }

            if (point != pointList.introPoint)
            {
                float introEasePointPercent = _cameraPath.GetIntroEasePercentage(point);
                Vector3 introEasePoint = _cameraPath.GetPathPosition(introEasePointPercent, true);
                Vector3 introEaseDirection = _cameraPath.GetPathDirection(introEasePointPercent, false);

                Handles.color = CameraPathColours.RED;
                Handles.Label(introEasePoint, "Ease In\n" + point.displayName, colouredText);
                Vector3 newPosition = Handles.Slider(introEasePoint, -introEaseDirection);

                float movement = Vector3.Distance(introEasePoint, newPosition);
                if (movement > Mathf.Epsilon)
                {
                    float newPercent = NearestmMousePercentage();
                    float curvePercent = 1-_cameraPath.GetCurvePercentage(_cameraPath.delayList.GetPoint(point.index-1), _cameraPath.delayList.GetPoint(point.index), newPercent);
                    point.introStartEasePercentage = curvePercent;
                }

                float percentWidth = (point.percent - introEasePointPercent);
//                float easeSpace = _cameraPath.pathLength * percentWidth;
//                float easeLength = unitPercent / percentWidth;
                float percentMovement = percentWidth / 10.0f;
                for (float e = introEasePointPercent; e < point.percent; e += percentMovement)
                {
                    float eB = e + percentMovement;
                    Vector3 lineStart = _cameraPath.GetPathPosition(e, true);
                    Vector3 lineEnd = _cameraPath.GetPathPosition(eB, true);
                    Handles.DrawLine(lineStart, lineEnd);
                    float animCurvePercentA = (e - introEasePointPercent) / percentWidth;
                    float animCurvePercentB = (eB - introEasePointPercent) / percentWidth;
                    Vector3 lineEaseUpA = easeUp * point.introCurve.Evaluate(animCurvePercentA);
                    Vector3 lineEaseUpB = easeUp * point.introCurve.Evaluate(animCurvePercentB);
                    Handles.DrawLine(lineStart + lineEaseUpA, lineEnd + lineEaseUpB);
                }
            }
        }
    }

    private static void DisplayAtPoint()
    {
        float atPercent = _cameraPath.addPointAtPercent;
        Vector3 atPointVector = _cameraPath.GetPathPosition(atPercent,true);
        float handleSize = HandleUtility.GetHandleSize(atPointVector);
        Handles.color = Color.black;
        UnityVersionWrapper.HandlesArrowCap(0, atPointVector, Quaternion.identity, handleSize*0.05f);
        Handles.Label(atPointVector, "Add Point Here\nfrom Inspector", colouredText);
    }

    private static void AddPathPoints()
    {
        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = true;

        Handles.color = _cameraPath.unselectedPointColour;
        int numberOfPoints = _cameraPath.realNumberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathControlPoint point = _cameraPath[i];
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE * 0.4f;
            UnityVersionWrapper.HandlesArrowCap(0, point.worldPosition, Quaternion.identity, pointHandleSize);
        }

        float mousePercentage = NearestmMousePercentage();// _track.GetNearestPoint(mousePlanePoint);
        Vector3 mouseTrackPoint = _cameraPath.GetPathPosition(mousePercentage, true);
        Handles.Label(mouseTrackPoint, "Add New Path Point", colouredText);
        float newPointHandleSize = HandleUtility.GetHandleSize(mouseTrackPoint) * HANDLE_SCALE;
        Quaternion lookDirection = Quaternion.LookRotation(Camera.current.transform.forward);
        if (UnityVersionWrapper.HandlesDotButton(mouseTrackPoint, lookDirection, newPointHandleSize, newPointHandleSize))
        {
            int newPointIndex = _cameraPath.GetNextPointIndex(mousePercentage,false);
            CameraPathControlPoint newPoint = _cameraPath.gameObject.AddComponent<CameraPathControlPoint>();//ScriptableObject.CreateInstance<CameraPathControlPoint>();
            newPoint.worldPosition = mouseTrackPoint;
            _cameraPath.InsertPoint(newPoint, newPointIndex);
            ChangeSelectedPointIndex(newPointIndex);
            GUI.changed = true;
        }
    }

    private static void RemovePathPoints()
    {
        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = true;

        int numberOfPoints = _cameraPath.realNumberOfPoints;
        Handles.color = _cameraPath.selectedPointColour;
        Ray mouseRay = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
        Quaternion mouseLookDirection = Quaternion.LookRotation(-mouseRay.direction);
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathControlPoint point = _cameraPath[i];
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.Label(point.worldPosition, "Remove Point: "+point.displayName, colouredText);
            if (UnityVersionWrapper.HandlesDotButton(point.worldPosition, mouseLookDirection, pointHandleSize, pointHandleSize))
            {
                _cameraPath.RemovePoint(point);
                GUI.changed = true;
                return;
            }
        }
    }

    private static void AddCPathPoints()
    {
        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = true;

        Handles.color = _cameraPath.selectedPointColour;
        CameraPathPointList pointList = null;
        switch(_pointMode)
        {
            case CameraPath.PointModes.AddOrientations:
                pointList = _cameraPath.orientationList;
                break;
            case CameraPath.PointModes.AddFovs:
                pointList = _cameraPath.fovList;
                break;
            case CameraPath.PointModes.AddTilts:
                pointList = _cameraPath.tiltList;
                break;
            case CameraPath.PointModes.AddEvents:
                pointList = _cameraPath.eventList;
                break;
            case CameraPath.PointModes.AddSpeeds:
                pointList = _cameraPath.speedList;
                break;
            case CameraPath.PointModes.AddDelays:
                pointList = _cameraPath.delayList;
                break;
        }
        int numberOfPoints = pointList.realNumberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathPoint point = pointList[i];
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE * 0.4f;
            UnityVersionWrapper.HandlesArrowCap(0, point.worldPosition, Quaternion.identity, pointHandleSize);
        }

        float mousePercentage = NearestmMousePercentage();// _track.GetNearestPoint(mousePlanePoint);
        Vector3 mouseTrackPoint = _cameraPath.GetPathPosition(mousePercentage, true);
        Handles.Label(mouseTrackPoint, "Add New Point", colouredText);
        float newPointHandleSize = HandleUtility.GetHandleSize(mouseTrackPoint) * HANDLE_SCALE;
        Ray mouseRay = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
        Quaternion mouseLookDirection = Quaternion.LookRotation(-mouseRay.direction);
        if (UnityVersionWrapper.HandlesDotButton(mouseTrackPoint, mouseLookDirection, newPointHandleSize, newPointHandleSize))
        {
            CameraPathControlPoint curvePointA = _cameraPath[_cameraPath.GetLastPointIndex(mousePercentage,false)];
            CameraPathControlPoint curvePointB = _cameraPath[_cameraPath.GetNextPointIndex(mousePercentage,false)];
            float curvePercentage = _cameraPath.GetCurvePercentage(curvePointA, curvePointB, mousePercentage);
            switch(_pointMode)
            {
                case CameraPath.PointModes.AddOrientations:
                    Quaternion pointRotation = Quaternion.LookRotation(_cameraPath.GetPathDirection(mousePercentage));
                    CameraPathOrientation newOrientation = ((CameraPathOrientationList)pointList).AddOrientation(curvePointA, curvePointB, curvePercentage, pointRotation);
                    ChangeSelectedPointIndex(pointList.IndexOf(newOrientation));
                    break;

                case CameraPath.PointModes.AddFovs:
                    float pointFOV = _cameraPath.fovList.GetValue(mousePercentage, CameraPathFOVList.ProjectionType.FOV);
                    float pointSize = _cameraPath.fovList.GetValue(mousePercentage, CameraPathFOVList.ProjectionType.Orthographic);
                    CameraPathFOV newFOVPoint = ((CameraPathFOVList)pointList).AddFOV(curvePointA, curvePointB, curvePercentage, pointFOV, pointSize);
                    ChangeSelectedPointIndex(pointList.IndexOf(newFOVPoint));
                    break;

                case CameraPath.PointModes.AddTilts:
                    float pointTilt = _cameraPath.GetPathTilt(mousePercentage);
                    CameraPathTilt newTiltPoint = ((CameraPathTiltList)pointList).AddTilt(curvePointA, curvePointB, curvePercentage, pointTilt);
                    ChangeSelectedPointIndex(pointList.IndexOf(newTiltPoint));
                    break;

                case CameraPath.PointModes.AddEvents:
                    CameraPathEvent newEventPoint = ((CameraPathEventList)pointList).AddEvent(curvePointA, curvePointB, curvePercentage);
                    ChangeSelectedPointIndex(pointList.IndexOf(newEventPoint));
                    break;

                case CameraPath.PointModes.AddSpeeds:
                    _cameraPath.speedList.listEnabled = true;//if we're adding speeds then we probable want to enable it
                    CameraPathSpeed newSpeedPoint = ((CameraPathSpeedList)pointList).AddSpeedPoint(curvePointA, curvePointB, curvePercentage);
                    newSpeedPoint.speed = _animator.pathSpeed;
                    ChangeSelectedPointIndex(pointList.IndexOf(newSpeedPoint));
                    break;

                case CameraPath.PointModes.AddDelays:
                    CameraPathDelay newDelayPoint = ((CameraPathDelayList)pointList).AddDelayPoint(curvePointA, curvePointB, curvePercentage);
                    ChangeSelectedPointIndex(pointList.IndexOf(newDelayPoint));
                    break;
            }
            GUI.changed = true;
        }
    }


    private static void RemoveCPathPoints()
    {
        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = true;

        CameraPathPointList pointList = null;
        switch (_pointMode)
        {
            case CameraPath.PointModes.RemoveOrientations:
                pointList = _cameraPath.orientationList;
                break;
            case CameraPath.PointModes.RemoveFovs:
                pointList = _cameraPath.fovList;
                break;
            case CameraPath.PointModes.RemoveTilts:
                pointList = _cameraPath.tiltList;
                break;
            case CameraPath.PointModes.RemoveEvents:
                pointList = _cameraPath.eventList;
                break;
            case CameraPath.PointModes.RemoveSpeeds:
                pointList = _cameraPath.speedList;
                break;
            case CameraPath.PointModes.RemoveDelays:
                pointList = _cameraPath.delayList;
                break;
        }

        int numberOfPoints = pointList.realNumberOfPoints;
        Handles.color = _cameraPath.selectedPointColour;
        Quaternion mouseLookDirection = Quaternion.LookRotation(Camera.current.transform.forward);
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathPoint point = pointList[i];
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.Label(point.worldPosition, "Remove Point " + i, colouredText);
            if (UnityVersionWrapper.HandlesDotButton(point.worldPosition, mouseLookDirection, pointHandleSize, pointHandleSize))
            {
                pointList.RemovePoint(point);
                GUI.changed = true;
                return;
            }
        }
    }

    private static void CPPSlider(CameraPathPoint point)
    {
        if(point.positionModes == CameraPathPoint.PositionModes.FixedToPercent)
            return;//can't move fixed points


        Vector3 pointPathDirection = _cameraPath.GetPathDirection(point.percent, false);
        Handles.color = CameraPathColours.BLUE;
        Vector3 newPosition = Handles.Slider(point.worldPosition, pointPathDirection);
        newPosition = Handles.Slider(newPosition, -pointPathDirection);
        float movement = Vector3.Distance(point.worldPosition, newPosition);
        if (movement > Mathf.Epsilon)
        {
            //float newPercent = _cameraPath.GetNearestPoint(newPosition, false);
            float newPercent = NearestmMousePercentage();

            switch(point.positionModes)
            {
                case CameraPathPoint.PositionModes.Free:
                    CameraPathControlPoint curvePointA = _cameraPath[_cameraPath.GetLastPointIndex(newPercent, false)];
                    CameraPathControlPoint curvePointB = _cameraPath[_cameraPath.GetNextPointIndex(newPercent, false)];
                    point.cpointA = curvePointA;
                    point.cpointB = curvePointB;
                    point.curvePercentage = _cameraPath.GetCurvePercentage(curvePointA, curvePointB, newPercent);
                    break;

                case CameraPathPoint.PositionModes.FixedToPoint:
                    point.positionModes = CameraPathPoint.PositionModes.Free;
                    CameraPathControlPoint newCurvePointA = _cameraPath[_cameraPath.GetLastPointIndex(newPercent, false)];
                    CameraPathControlPoint newCurvePointB = _cameraPath[_cameraPath.GetNextPointIndex(newPercent, false)];
                    if(newCurvePointA == newCurvePointB)
                        newCurvePointB = _cameraPath[_cameraPath.GetPointIndex(newCurvePointB.index- 1)];
                    point.cpointA = newCurvePointA;
                    point.cpointB = newCurvePointB;
                    point.curvePercentage = _cameraPath.GetCurvePercentage(newCurvePointA, newCurvePointB, newPercent);
                    break;
            }
            point.worldPosition = _cameraPath.GetPathPosition(point.percent, false);
            _cameraPath.RecalculateStoredValues();
            selectedPointIndex = point.index;
        }
    }

    /// <summary>
    /// Get the nearest point on the track curve to the  mouse position
    /// We essentailly project the track onto a 2D plane that is the editor camera and then find a point on that
    /// </summary>
    /// <returns>A percentage of the nearest point on the track curve to the nerest metre</returns>
    private static float NearestmMousePercentage()
    {
        Camera cam = Camera.current;
        float screenHeight = cam.pixelHeight;
        Vector2 mousePos = Event.current.mousePosition;
        mousePos.y = screenHeight - mousePos.y;
        int numberOfSearchPoints = _cameraPath.storedValueArraySize;

        Vector2 zeropoint = cam.WorldToScreenPoint(_cameraPath.GetPathPosition(0, true));
        float nearestPointSqrMag = Vector2.SqrMagnitude(zeropoint - mousePos);
        float nearestT = 0;
        float nearestPointSqrMagB = Vector2.SqrMagnitude(zeropoint - mousePos);
        float nearestTb = 0;

        for (int i = 1; i < numberOfSearchPoints; i++)
        {
            float t = i / (float)numberOfSearchPoints;
            Vector2 point = cam.WorldToScreenPoint(_cameraPath.GetPathPosition(t, true));
            float thisPointMag = Vector2.SqrMagnitude(point - mousePos);
            if (thisPointMag < nearestPointSqrMag)
            {
                nearestPointSqrMagB = nearestPointSqrMag;
                nearestTb = nearestT;

                nearestT = t;
                nearestPointSqrMag = thisPointMag;
            }
            else
            {
                if (thisPointMag < nearestPointSqrMagB)
                {
                    nearestTb = t;
                    nearestPointSqrMagB = thisPointMag;
                }
            }
        }
        float pointADist = Mathf.Sqrt(nearestPointSqrMag);
        float pointBDist = Mathf.Sqrt(nearestPointSqrMagB);
        float lerpvalue = pointADist / (pointADist + pointBDist);
        return Mathf.Lerp(nearestT, nearestTb, lerpvalue);
    }


    private static void ChangeSelectedPointIndex(int newPointSelected)
    {
        selectedPointIndex = newPointSelected;
    }
}
