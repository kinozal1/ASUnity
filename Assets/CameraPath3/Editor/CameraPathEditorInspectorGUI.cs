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
using System.IO;
using UnityEngine;
using UnityEditor;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using Object = UnityEngine.Object;

public class CameraPathEditorInspectorGUI
{
    private static GUIContent[] _toolBarGUIContentA;
    private static GUIContent[] _toolBarGUIContentB;

    private static CameraPathAnimator.orientationModes _orientationmode = CameraPathAnimator.orientationModes.custom;

    public static CameraPath _cameraPath;
    public static CameraPathAnimator _animator;

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

    //    private static Vector3 cpPosition;

    //Preview Camera
    //    private static float aspect = 1.7777f;
    //    private static int previewResolution = 800;

    //GUI Styles
    private static GUIStyle unselectedBox;
    private static GUIStyle selectedBox;
    private static GUIStyle redText;

    private static Texture2D unselectedBoxColour;
    private static Texture2D selectedBoxColour;


    public static void Setup()
    {
        if (_cameraPath == null)
            return;

        SetupToolbar();

        unselectedBox = new GUIStyle();
        if (unselectedBoxColour != null)
            Object.DestroyImmediate(unselectedBoxColour);
        unselectedBoxColour = new Texture2D(1, 1);
        unselectedBoxColour.SetPixel(0, 0, CameraPathColours.DARKGREY);
        unselectedBoxColour.Apply();
        unselectedBox.normal.background = unselectedBoxColour;

        selectedBox = new GUIStyle();
        if (selectedBoxColour != null)
            Object.DestroyImmediate(selectedBoxColour);
        selectedBoxColour = new Texture2D(1, 1);
        selectedBoxColour.SetPixel(0, 0, CameraPathColours.DARKGREEN);
        selectedBoxColour.Apply();
        selectedBox.normal.background = selectedBoxColour;

        redText = new GUIStyle();
        redText.normal.textColor = CameraPathColours.RED;

        //Preview Camera
        if (_cameraPath.editorPreview != null)
            Object.DestroyImmediate(_cameraPath.editorPreview);
        if (CameraPathPreviewSupport.previewSupported)
        {
            _cameraPath.editorPreview = new GameObject("Path Point Preview Cam");
            _cameraPath.editorPreview.hideFlags = HideFlags.HideAndDontSave;
            _cameraPath.editorPreview.AddComponent<Camera>();
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = 60;
            _cameraPath.editorPreview.GetComponent<Camera>().depth = -1;
            //Retreive camera settings from the main camera
            Camera[] cams = Camera.allCameras;
            bool sceneHasCamera = cams.Length > 0;
            Camera sceneCamera = null;
            Skybox sceneCameraSkybox = null;
            if (Camera.main)
            {
                sceneCamera = Camera.main;
            }
            else if (sceneHasCamera)
            {
                sceneCamera = cams[0];
            }

            if (sceneCamera != null)
            {
                _cameraPath.editorPreview.GetComponent<Camera>().clearFlags = sceneCamera.clearFlags;
                _cameraPath.editorPreview.GetComponent<Camera>().cullingMask = sceneCamera.cullingMask;
                sceneCameraSkybox = sceneCamera.GetComponent<Skybox>();
                _cameraPath.editorPreview.GetComponent<Camera>().backgroundColor = sceneCamera.backgroundColor;
                if (sceneCameraSkybox != null)
                    _cameraPath.editorPreview.AddComponent<Skybox>().material = sceneCameraSkybox.material;
                else if (RenderSettings.skybox != null)
                    _cameraPath.editorPreview.AddComponent<Skybox>().material = RenderSettings.skybox;

                _cameraPath.editorPreview.GetComponent<Camera>().orthographic = sceneCamera.orthographic;
                _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = sceneCamera.fieldOfView;
                _cameraPath.editorPreview.GetComponent<Camera>().orthographicSize = sceneCamera.orthographicSize;
            }
            _cameraPath.editorPreview.GetComponent<Camera>().enabled = false;
        }

        if (EditorApplication.isPlaying && _cameraPath.editorPreview != null)
            _cameraPath.editorPreview.SetActive(false);
    }

    public static void OnInspectorGUI()
    {
        _pointMode = _cameraPath.pointMode;

        if (_cameraPath.transform.rotation != Quaternion.identity)
        {
            EditorGUILayout.HelpBox("Camera Path does not support rotations of the main game object.", MessageType.Error);
            if (GUILayout.Button("Reset Rotation"))
                _cameraPath.transform.rotation = Quaternion.identity;
            return;
        }

        GUILayout.BeginVertical(GUILayout.Width(400));

        if (_cameraPath.realNumberOfPoints < 2)
        {
            EditorGUILayout.HelpBox("There are no track points defined, add a path point to begin", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Path Length approx. " + (_cameraPath.pathLength).ToString("F2") + " units");
        bool trackloop = EditorGUILayout.Toggle("Is Looped", _cameraPath.loop);
        _cameraPath.loop = trackloop;

        EditorGUILayout.HelpBox("Set a Camera Path to trigger once this one has complete.", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Link Camera Path", GUILayout.Width(110));
        CameraPath nextPath = (CameraPath)EditorGUILayout.ObjectField(_cameraPath.nextPath, typeof(CameraPath), true);
        if (_cameraPath.nextPath != nextPath)
            _cameraPath.nextPath = nextPath;
        EditorGUI.BeginDisabledGroup(nextPath == null);
        EditorGUILayout.LabelField("Interpolate", GUILayout.Width(70));
        bool interpolateNextPath = EditorGUILayout.Toggle(_cameraPath.interpolateNextPath, GUILayout.Width(30));
        _cameraPath.interpolateNextPath = interpolateNextPath;
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (_animator != null && _orientationmode != _animator.orientationMode)
            SetupToolbar();
        ToolbarMenuGUI();

        switch (_pointMode)
        {
            case CameraPath.PointModes.Transform:
                ModifyPointsInspectorGUI();
                break;

            case CameraPath.PointModes.ControlPoints:
                ModifyControlPointsInspector();
                break;

            case CameraPath.PointModes.FOV:
                ModifyFOVInspector();
                break;

            case CameraPath.PointModes.Speed:
                ModifySpeedInspector();
                break;

            case CameraPath.PointModes.Orientations:
                ModifyOrientaionInspector();
                break;

            case CameraPath.PointModes.Tilt:
                ModifyTiltsInspector();
                break;

            case CameraPath.PointModes.Events:
                ModifyEventsInspector();
                break;

            case CameraPath.PointModes.Delay:
                ModifyDelayInspector();
                break;

            case CameraPath.PointModes.Ease:
                ModifyEaseInspector();
                break;

            case CameraPath.PointModes.AddPathPoints:
                ModifyPointsInspectorGUI();
                break;

            case CameraPath.PointModes.RemovePathPoints:
                ModifyPointsInspectorGUI();
                break;

            case CameraPath.PointModes.AddOrientations:
                ModifyOrientaionInspector();
                break;

            case CameraPath.PointModes.AddTilts:
                ModifyTiltsInspector();
                break;

            case CameraPath.PointModes.AddEvents:
                ModifyEventsInspector();
                break;

            case CameraPath.PointModes.AddSpeeds:
                ModifySpeedInspector();
                break;

            case CameraPath.PointModes.AddFovs:
                ModifyFOVInspector();
                break;

            case CameraPath.PointModes.AddDelays:
                ModifyDelayInspector();
                break;

            case CameraPath.PointModes.RemoveOrientations:
                ModifyOrientaionInspector();
                break;

            case CameraPath.PointModes.RemoveTilts:
                ModifyTiltsInspector();
                break;

            case CameraPath.PointModes.RemoveEvents:
                ModifyEventsInspector();
                break;

            case CameraPath.PointModes.RemoveSpeeds:
                ModifySpeedInspector();
                break;

            case CameraPath.PointModes.RemoveFovs:
                ModifyFOVInspector();
                break;

            case CameraPath.PointModes.RemoveDelays:
                ModifyDelayInspector();
                break;

            case CameraPath.PointModes.Options:
                OptionsInspectorGUI();
                break;

        }
        GUILayout.EndVertical();
    }

    private static void ModifyPointsInspectorGUI()
    {
        CameraPathControlPoint point = null;
        if (selectedPointIndex >= _cameraPath.realNumberOfPoints)
            ChangeSelectedPointIndex(_cameraPath.realNumberOfPoints - 1);
        if (_cameraPath.realNumberOfPoints > 0)
            point = _cameraPath[selectedPointIndex];

        if (_cameraPath.enableUndo) Undo.RecordObject(point, "Modify Path Point");
        if (_animator != null && point != null)
            CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.normalisedPercentage, true);
        //            RenderPreview(point.worldPosition, _animator.GetAnimatedOrientation(point.percentage,true), _cameraPath.GetPathFOV(point.percentage));
        PointListGUI();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Selected point " + selectedPointIndex);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Custom Point Name");
        point.customName = EditorGUILayout.TextField(point.customName);
        if (GUILayout.Button("Clear"))
            point.customName = "";
        EditorGUILayout.EndHorizontal();

        Vector3 pointposition = EditorGUILayout.Vector3Field("Point Position", point.localPosition);
        if (pointposition != point.localPosition)
        {
            point.localPosition = pointposition;
        }

        //ADD NEW POINTS
        if (_pointMode != CameraPath.PointModes.AddPathPoints)
        {
            if (GUILayout.Button("Add Path Points"))
            {
                ChangePointMode(CameraPath.PointModes.AddPathPoints);
            }
        }
        else
        {
            if (GUILayout.Button("Done"))
            {
                ChangePointMode(CameraPath.PointModes.Transform);
            }
        }

        if (GUILayout.Button("Add Path Point to End of Path"))
            AddPointToEnd();

        if (_pointMode != CameraPath.PointModes.RemovePathPoints)
        {
            if (GUILayout.Button("Delete Path Points"))
            {
                ChangePointMode(CameraPath.PointModes.RemovePathPoints);
            }
        }
        else
        {
            if (GUILayout.Button("Done"))
            {
                ChangePointMode(CameraPath.PointModes.Transform);
            }
        }
    }

    private static void ModifyControlPointsInspector()
    {
        bool isBezier = _cameraPath.interpolation == CameraPath.Interpolation.Bezier;

        if (!isBezier)
        {
            EditorGUILayout.HelpBox("Path interpolation is currently not set to Bezier. There are no control points to manipulate", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Interpolation Algorithm");
            _cameraPath.interpolation = (CameraPath.Interpolation)EditorGUILayout.EnumPopup(_cameraPath.interpolation);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.BeginDisabledGroup(!isBezier);
        CameraPathControlPoint point = null;
        if (selectedPointIndex >= _cameraPath.realNumberOfPoints)
            ChangeSelectedPointIndex(_cameraPath.realNumberOfPoints - 1);
        if (_cameraPath.realNumberOfPoints > 0)
            point = _cameraPath[selectedPointIndex];

        if (CameraPathPreviewSupport.previewSupported && _cameraPath.editorPreview != null && _animator != null && point != null)
            CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.percentage, true);
        //            RenderPreview(point.worldPosition,_animator.GetAnimatedOrientation(point.percentage,true),_cameraPath.GetPathFOV(point.percentage));

        PointListGUI();

        bool pointsplitControlPoints = EditorGUILayout.Toggle("Split Control Points", point.splitControlPoints);
        if (pointsplitControlPoints != point.splitControlPoints)
        {
            point.splitControlPoints = pointsplitControlPoints;
        }
        Vector3 pointforwardControlPoint = EditorGUILayout.Vector3Field("Control Point Position", point.forwardControlPoint);
        if (pointforwardControlPoint != point.forwardControlPoint)
        {
            point.forwardControlPoint = pointforwardControlPoint;
        }
        EditorGUI.BeginDisabledGroup(!point.splitControlPoints);
        Vector3 pointbackwardControlPoint = EditorGUILayout.Vector3Field("Control Point Reverse Position", point.backwardControlPoint);
        if (pointbackwardControlPoint != point.backwardControlPoint)
        {
            point.backwardControlPoint = pointbackwardControlPoint;
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Auto Place Control Point for " + point.displayName))
            AutoSetControlPoint(point);

        if (GUILayout.Button("Zero Control Points"))
        {
            point.forwardControlPointLocal = Vector3.zero;
            if (point.splitControlPoints)
                point.backwardControlPoint = Vector3.zero;
        }
        EditorGUI.EndDisabledGroup();
    }

    private static void ModifyOrientaionInspector()
    {
        CameraPathOrientationList pointList = _cameraPath.orientationList;
        CameraPathOrientation point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            if (CameraPathPreviewSupport.previewSupported && _cameraPath.editorPreview != null && _animator != null)
                CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.animationPercentage, true);
            //            RenderPreview(point.worldPosition,_animator.GetAnimatedOrientation(point.percent,true),_cameraPath.GetPathFOV(point.percent));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        pointList.interpolation = (CameraPathOrientationList.Interpolation)EditorGUILayout.EnumPopup(pointList.interpolation);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Show Orientation Inidcators", GUILayout.Width(170));
        _cameraPath.showOrientationIndicators = EditorGUILayout.Toggle(_cameraPath.showOrientationIndicators);
        EditorGUILayout.LabelField("Every", GUILayout.Width(40));
        _cameraPath.orientationIndicatorUnitLength = Mathf.Max(EditorGUILayout.FloatField(_cameraPath.orientationIndicatorUnitLength, GUILayout.Width(30)), 0.1f);
        EditorGUILayout.LabelField("units", GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        if (pointList.realNumberOfPoints == 0)
            EditorGUILayout.HelpBox("There are no orientation points in this path.", MessageType.Warning);

        CPPointArrayInspector("Orientation Points", pointList, CameraPath.PointModes.Orientations, CameraPath.PointModes.AddOrientations, CameraPath.PointModes.RemoveOrientations);

        if (point != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name", GUILayout.Width(120));
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            Vector3 currentRotation = point.rotation.eulerAngles;
            EditorGUILayout.LabelField("Angle", GUILayout.Width(60));
            Vector3 newRotation = EditorGUILayout.Vector3Field("", currentRotation);
            if (currentRotation != newRotation)
            {
                point.rotation.eulerAngles = newRotation;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Look at CameraPathOnRailsTarget", GUILayout.Width(100));
            point.lookAt = (Transform)EditorGUILayout.ObjectField(point.lookAt, typeof(Transform), true);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
                point.lookAt = null;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (GUILayout.Button("Reset Angle"))
                point.rotation = Quaternion.identity;

            if (GUILayout.Button("Set to Path Direction"))
                point.rotation.SetLookRotation(_cameraPath.GetPathDirection(point.percent, false));

            //Thanks Perso Jery!
            if (GUILayout.Button("Set All to Path Direction"))
            {
                //Get all points
                CameraPathOrientationList orientationList = _cameraPath.orientationList;
                if (orientationList.realNumberOfPoints > 0)
                {
                    //For each point, do the logic of look rotation (the same than above)
                    for (int i = 0; i < orientationList.realNumberOfPoints; i++)
                    {
                        CameraPathOrientation currentPoint = pointList[i];
                        currentPoint.rotation.SetLookRotation(_cameraPath.GetPathDirection(currentPoint.percent, false));
                    }
                }
            }
        }
    }

    private static void ModifyFOVInspector()
    {
        CameraPathFOVList pointList = _cameraPath.fovList;
        CameraPathFOV point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            if (CameraPathPreviewSupport.previewSupported && _cameraPath.editorPreview != null && _animator != null)
                CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.animationPercentage, true);
            //            RenderPreview(point.worldPosition,_animator.GetAnimatedOrientation(point.percent,true),_cameraPath.GetPathFOV(point.percent));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        pointList.interpolation = (CameraPathFOVList.Interpolation)EditorGUILayout.EnumPopup(pointList.interpolation);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(pointList.realNumberOfPoints == 0);
        EditorGUILayout.LabelField("Enabled");
        pointList.listEnabled = EditorGUILayout.Toggle(pointList.listEnabled);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        if (!pointList.listEnabled)
            EditorGUILayout.HelpBox("FOV is currently disabled", MessageType.Warning);

        if (pointList.realNumberOfPoints == 0)
            EditorGUILayout.HelpBox("There are no FOV points in this path.", MessageType.Warning);

        CPPointArrayInspector("Field of View Points", pointList, CameraPath.PointModes.FOV, CameraPath.PointModes.AddFovs, CameraPath.PointModes.RemoveFovs);

        if (point != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name");
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Field of View Value");
            EditorGUILayout.BeginHorizontal();
            float currentFOV = point.FOV;
            float newFOV = EditorGUILayout.Slider(currentFOV, 0, 180);
            EditorGUILayout.EndHorizontal();
            if (currentFOV != newFOV)
            {
                point.FOV = newFOV;
            }

            if (GUILayout.Button("Set to Camera Default"))
            {
                if (_animator.isCamera)
                    point.FOV = _animator.animationObject.GetComponent<Camera>().fieldOfView;
                else
                    point.FOV = Camera.main.fieldOfView;
            }

            EditorGUILayout.LabelField("Orthographic Size");
            EditorGUILayout.BeginHorizontal();
            float currentSize = point.Size;
            float newSize = EditorGUILayout.FloatField(currentSize);
            EditorGUILayout.EndHorizontal();
            if (currentSize != newSize)
            {
                point.Size = newSize;
            }

            if (GUILayout.Button("Set to Camera Default"))
            {
                if (_animator.isCamera)
                    point.Size = _animator.animationObject.GetComponent<Camera>().orthographicSize;
                else
                    point.Size = Camera.main.orthographicSize;
            }
        }
    }

    private static void ModifyTiltsInspector()
    {
        CameraPathTiltList pointList = _cameraPath.tiltList;
        CameraPathTilt point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            if (CameraPathPreviewSupport.previewSupported && _cameraPath.editorPreview != null && _animator != null)
                CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.animationPercentage, true);
            //            RenderPreview(point.worldPosition,_animator.GetAnimatedOrientation(point.percent,true),_cameraPath.GetPathFOV(point.percent));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        pointList.interpolation = (CameraPathTiltList.Interpolation)EditorGUILayout.EnumPopup(pointList.interpolation);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Show Orientation Inidcators", GUILayout.Width(170));
        _cameraPath.showOrientationIndicators = EditorGUILayout.Toggle(_cameraPath.showOrientationIndicators);
        EditorGUILayout.LabelField("Every", GUILayout.Width(40));
        _cameraPath.orientationIndicatorUnitLength = Mathf.Max(EditorGUILayout.FloatField(_cameraPath.orientationIndicatorUnitLength, GUILayout.Width(30)), 0.1f);
        EditorGUILayout.LabelField("units", GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        if (pointList.realNumberOfPoints == 0)
            EditorGUILayout.HelpBox("There are no tilt points in this path.", MessageType.Warning);

        CPPointArrayInspector("Tilt Points", pointList, CameraPath.PointModes.Tilt, CameraPath.PointModes.AddTilts, CameraPath.PointModes.RemoveTilts);

        if (point != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name");
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Tilt Value");
            EditorGUILayout.BeginHorizontal();
            float currentTilt = point.tilt;
            float newTilt = EditorGUILayout.FloatField(currentTilt);
            EditorGUILayout.EndHorizontal();
            if (currentTilt != newTilt)
            {
                point.tilt = newTilt;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Auto Set Tile Points");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sensitivity");
            _cameraPath.tiltList.autoSensitivity = EditorGUILayout.Slider(_cameraPath.tiltList.autoSensitivity, 0.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Calculate and Assign Selected Path Tilts"))
            {
                _cameraPath.tiltList.AutoSetTilt(point);
            }

            if (GUILayout.Button("Calculate and Assign All Path Tilts"))
            {
                if (EditorUtility.DisplayDialog("Auto Setting All Path Tilt Values", "Are you sure you want to set all the values in this path?", "yes", "noooooo!"))
                    _cameraPath.tiltList.AutoSetTilts();
            }
            EditorGUILayout.EndVertical();
        }
    }

    private static void ModifySpeedInspector()
    {
        CameraPathSpeedList pointList = _cameraPath.speedList;
        CameraPathSpeed point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            if (CameraPathPreviewSupport.previewSupported && _cameraPath.editorPreview != null && _animator != null)
                CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.animationPercentage, true);
            //            RenderPreview(point.worldPosition,_animator.GetAnimatedOrientation(point.percent,true),_cameraPath.GetPathFOV(point.percent));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        pointList.interpolation = (CameraPathSpeedList.Interpolation)EditorGUILayout.EnumPopup(pointList.interpolation);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(pointList.realNumberOfPoints == 0);
        EditorGUILayout.LabelField("Enabled");
        pointList.listEnabled = EditorGUILayout.Toggle(pointList.listEnabled);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        if (pointList.realNumberOfPoints == 0)
            EditorGUILayout.HelpBox("There are no speed points in this path so it is disabled.", MessageType.Warning);
        EditorGUILayout.EndVertical();

        CPPointArrayInspector("Speed Points", pointList, CameraPath.PointModes.Speed, CameraPath.PointModes.AddSpeeds, CameraPath.PointModes.RemoveSpeeds);

        if (point != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name");
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Speed Value");
            EditorGUILayout.BeginHorizontal();
            float currentSpeed = point.speed;
            float newSpeed = EditorGUILayout.FloatField(currentSpeed);
            EditorGUILayout.EndHorizontal();
            point.speed = newSpeed;
        }
    }

    private static void ModifyEventsInspector()
    {
        CameraPathEventList pointList = _cameraPath.eventList;
        CameraPathEvent point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            if (CameraPathPreviewSupport.previewSupported && _cameraPath.editorPreview != null && _animator != null)
                CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.animationPercentage, true);
            //            RenderPreview(point.worldPosition,_animator.GetAnimatedOrientation(point.percent, true),_cameraPath.GetPathFOV(point.percent));
        }

        CPPointArrayInspector("Event Points", pointList, CameraPath.PointModes.Events, CameraPath.PointModes.AddEvents, CameraPath.PointModes.RemoveEvents);

        if (point != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name");
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Event Type");
            point.type = (CameraPathEvent.Types)EditorGUILayout.EnumPopup(point.type);
            EditorGUILayout.EndHorizontal();

            switch (point.type)
            {
                case CameraPathEvent.Types.Broadcast:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Event Name");
                    point.eventName = EditorGUILayout.TextField(point.eventName);
                    EditorGUILayout.EndHorizontal();
                    break;

                case CameraPathEvent.Types.Call:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Event Target");
                    point.target = (GameObject)EditorGUILayout.ObjectField(point.target, typeof(GameObject), true);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Event Call");
                    point.methodName = EditorGUILayout.TextField(point.methodName);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Event Argument");
                    point.argumentType = (CameraPathEvent.ArgumentTypes)EditorGUILayout.EnumPopup(point.argumentType);
                    point.methodArgument = EditorGUILayout.TextField(point.methodArgument);
                    EditorGUILayout.EndHorizontal();
                    switch (point.argumentType)
                    {
                        case CameraPathEvent.ArgumentTypes.Int:
                            int testForInt;
                            if (!int.TryParse(point.methodArgument, out testForInt))
                                EditorGUILayout.HelpBox("Argument specified is not a valid integer", MessageType.Error);
                            break;

                        case CameraPathEvent.ArgumentTypes.Float:
                            float testForFloat;
                            if (!float.TryParse(point.methodArgument, out testForFloat))
                                EditorGUILayout.HelpBox("Argument specified is not a valid number", MessageType.Error);
                            break;
                    }
                    break;
            }
        }
    }

    private static void ModifyDelayInspector()
    {
        CameraPathDelayList pointList = _cameraPath.delayList;
        CameraPathDelay point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            if (CameraPathPreviewSupport.previewSupported && _cameraPath.editorPreview != null && _animator != null)
                CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.animationPercentage, true);
            //            RenderPreview(point.worldPosition,_animator.GetAnimatedOrientation(point.percent, true),_cameraPath.GetPathFOV(point.percent));
        }

        CPPointArrayInspector("Delay Points", pointList, CameraPath.PointModes.Delay, CameraPath.PointModes.AddDelays, CameraPath.PointModes.RemoveDelays);

        if (point != null)
        {
            if (point == pointList.outroPoint)
            {
                EditorGUILayout.LabelField("End Point");
            }
            else
            {
                if (point == pointList.introPoint)
                    EditorGUILayout.LabelField("Start Point");
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Custom Point Name");
                    point.customName = EditorGUILayout.TextField(point.customName);
                    if (GUILayout.Button("Clear"))
                        point.customName = "";
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Delay Time");
                point.time = EditorGUILayout.FloatField(point.time);
                EditorGUILayout.LabelField("seconds", GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private static void ModifyEaseInspector()
    {
        CameraPathDelayList pointList = _cameraPath.delayList;
        CameraPathDelay point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            if (CameraPathPreviewSupport.previewSupported && _cameraPath.editorPreview != null && _animator != null)
                CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, point.animationPercentage, true);
            //            RenderPreview(point.worldPosition,_animator.GetAnimatedOrientation(point.percent, true),_cameraPath.GetPathFOV(point.percent));

        }

        CPPointArrayInspector("Ease Points", pointList, CameraPath.PointModes.Ease, CameraPath.PointModes.Ease, CameraPath.PointModes.Ease);

        if (point != null)
        {
            if (point == pointList.introPoint)
            {
                EditorGUILayout.LabelField("Start Point");
            }
            else if (point == pointList.outroPoint)
            {
                EditorGUILayout.LabelField("End Point");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Custom Point Name");
                point.customName = EditorGUILayout.TextField(point.customName);
                if (GUILayout.Button("Clear"))
                    point.customName = "";
                EditorGUILayout.EndHorizontal();
            }

            if (point != pointList.introPoint)
            {
                EditorGUILayout.LabelField("Ease In Curve");
                point.introCurve = EditorGUILayout.CurveField(point.introCurve, GUILayout.Height(50));

                point.introStartEasePercentage = EditorGUILayout.FloatField(point.introStartEasePercentage);

                if (GUILayout.Button("None"))
                    point.introCurve = AnimationCurve.Linear(0, 1, 1, 1);
                if (GUILayout.Button("Linear"))
                    point.introCurve = AnimationCurve.Linear(0, 1, 1, 0);
                if (GUILayout.Button("Ease In"))
                    point.introCurve = new AnimationCurve(new[] { new Keyframe(0, 1, 0, 0.0f), new Keyframe(1, 0, -1.0f, 0) });

            }
            if (point != pointList.outroPoint)
            {
                EditorGUILayout.LabelField("Ease Out Curve");
                point.outroCurve = EditorGUILayout.CurveField(point.outroCurve, GUILayout.Height(50));
                point.outroEndEasePercentage = EditorGUILayout.FloatField(point.outroEndEasePercentage);

                if (GUILayout.Button("None"))
                    point.outroCurve = AnimationCurve.Linear(0, 1, 1, 1);
                if (GUILayout.Button("Linear"))
                    point.outroCurve = AnimationCurve.Linear(0, 0, 1, 1);
                if (GUILayout.Button("Ease Out"))
                    point.outroCurve = new AnimationCurve(new[] { new Keyframe(0, 0, 0, 1.0f), new Keyframe(1, 1, 0, 0) });
            }
        }
    }

    private static void PointListGUI()
    {

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        _cameraPath.interpolation = (CameraPath.Interpolation)EditorGUILayout.EnumPopup(_cameraPath.interpolation);
        EditorGUILayout.EndHorizontal();

        if (_cameraPath.interpolation == CameraPath.Interpolation.Hermite)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tension");
            _cameraPath.hermiteTension = EditorGUILayout.Slider(_cameraPath.hermiteTension, -1, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bias");
            _cameraPath.hermiteBias = EditorGUILayout.Slider(_cameraPath.hermiteBias, -1, 1);
            EditorGUILayout.EndHorizontal();
        }

        int numberOfPoints = _cameraPath.realNumberOfPoints;
        if (_cameraPath.interpolation == CameraPath.Interpolation.Bezier)
        {
            if (GUILayout.Button("Auto Set All Control Points"))
                for (int i = 0; i < numberOfPoints; i++)
                    AutoSetControlPoint(_cameraPath[i]);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Path Points ");

        for (int i = 0; i < numberOfPoints; i++)
        {
            bool pointIsSelected = i == selectedPointIndex;
            EditorGUILayout.BeginHorizontal((pointIsSelected) ? selectedBox : unselectedBox);
            CameraPathControlPoint cpPoint = _cameraPath[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(cpPoint.displayName, GUILayout.Width(100));
            if (!pointIsSelected)
            {
                if (GUILayout.Button("Select"))
                {
                    ChangeSelectedPointIndex(i);
                    GotoScenePoint(cpPoint.worldPosition);
                }
            }
            else
            {
                if (GUILayout.Button("Goto"))
                    GotoScenePoint(cpPoint.worldPosition);
            }
            if (i < numberOfPoints - 1)
            {
                if (GUILayout.Button("Insert New Point"))
                {
                    int atIndex = cpPoint.index + 1;
                    CameraPathControlPoint pointA = _cameraPath.GetPoint(atIndex - 1);
                    CameraPathControlPoint pointB = _cameraPath.GetPoint(atIndex);
                    float newPointPercent = _cameraPath.GetPathPercentage(pointA, pointB, 0.5f);
                    Vector3 newPointPosition = _cameraPath.GetPathPosition(newPointPercent, true);
                    Vector3 newForwardControlPoint = _cameraPath.GetPathDirection(newPointPercent, true) * ((pointA.forwardControlPointLocal.magnitude + pointB.forwardControlPointLocal.magnitude) * 0.5f);

                    CameraPathControlPoint newPoint = _cameraPath.InsertPoint(atIndex);
                    newPoint.worldPosition = newPointPosition;
                    newPoint.forwardControlPointLocal = newForwardControlPoint;
                }
            }
            else
            {
                if (GUILayout.Button("Add Point to End"))
                {
                    AddPointToEnd();
                }
            }
            EditorGUI.BeginDisabledGroup(numberOfPoints < 3);
            if (GUILayout.Button("Delete"))
            {
                _cameraPath.RemovePoint(cpPoint);
                return;//cheap, but effective. Cancel any further actions on this frame
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }
    }

    private static void CPPointArrayInspector(string title, CameraPathPointList pointList, CameraPath.PointModes deflt, CameraPath.PointModes add, CameraPath.PointModes remove)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(title);
        int numberOfPoints = pointList.realNumberOfPoints;
        if (numberOfPoints == 0)
            EditorGUILayout.LabelField("There are no points", redText);

        CameraPathPoint duplicatePoint = pointList.DuplicatePointCheck();
        if (duplicatePoint != null)
            EditorGUILayout.HelpBox("There are points occuping the same percentage.\n Check " + duplicatePoint.displayName + " thanks.", MessageType.Error);

        for (int i = 0; i < numberOfPoints; i++)
        {
            bool cantDelete = false;
            bool pointIsSelected = i == selectedPointIndex;
            EditorGUILayout.BeginHorizontal((pointIsSelected) ? selectedBox : unselectedBox);
            CameraPathPoint arrayPoint = pointList[i];
            EditorGUILayout.BeginHorizontal();
            if (arrayPoint.customName == "")
                EditorGUILayout.LabelField("Point " + i, GUILayout.Width(85));
            else
                EditorGUILayout.LabelField(arrayPoint.customName, GUILayout.Width(85));

            float valueTextSize = 120;
            switch (deflt)
            {
                case CameraPath.PointModes.FOV:
                    CameraPathFOV fov = (CameraPathFOV)arrayPoint;
                    EditorGUILayout.LabelField("FOV", GUILayout.Width(30));
                    fov.FOV = EditorGUILayout.FloatField(fov.FOV, GUILayout.Width(50));
                    EditorGUILayout.LabelField("Size", GUILayout.Width(30));
                    fov.Size = EditorGUILayout.FloatField(fov.Size, GUILayout.Width(50));
                    break;

                case CameraPath.PointModes.Speed:
                    CameraPathSpeed speed = (CameraPathSpeed)arrayPoint;
                    speed.speed = EditorGUILayout.FloatField(speed.speed, GUILayout.Width(50));
                    break;

                case CameraPath.PointModes.Delay:
                    CameraPathDelay delay = (CameraPathDelay)arrayPoint;
                    if (delay != _cameraPath.delayList.outroPoint)
                    {
                        delay.time = EditorGUILayout.FloatField(delay.time, GUILayout.Width(50));
                        EditorGUILayout.LabelField("secs", GUILayout.Width(40));
                        if (delay != _cameraPath.delayList.introPoint)
                            cantDelete = false;
                        else
                            cantDelete = true;
                    }
                    else
                    {
                        cantDelete = true;
                    }
                    break;

                case CameraPath.PointModes.Ease:
                    cantDelete = true;
                    break;

                case CameraPath.PointModes.Orientations:
                    CameraPathOrientation orientation = (CameraPathOrientation)arrayPoint;
                    EditorGUILayout.LabelField(orientation.rotation.eulerAngles.ToString(), GUILayout.Width(valueTextSize));
                    break;

                case CameraPath.PointModes.Tilt:
                    CameraPathTilt tilt = (CameraPathTilt)arrayPoint;
                    tilt.tilt = EditorGUILayout.FloatField(tilt.tilt, GUILayout.Width(50));

                    break;

                case CameraPath.PointModes.Events:
                    CameraPathEvent point = (CameraPathEvent)arrayPoint;
                    point.type = (CameraPathEvent.Types)EditorGUILayout.EnumPopup(point.type, GUILayout.Width(50));
                    if (point.type == CameraPathEvent.Types.Broadcast)
                        point.eventName = EditorGUILayout.TextField(point.eventName, GUILayout.Width(120));
                    else
                    {
                        point.target = (GameObject)EditorGUILayout.ObjectField(point.target, typeof(GameObject), true);
                        point.methodName = EditorGUILayout.TextField(point.methodName, GUILayout.Width(55));
                    }
                    break;
            }

            if (!pointIsSelected)
            {

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    ChangeSelectedPointIndex(i);
                    //                    GotoScenePoint(arrayPoint.worldPosition);//Stop moving scene view, you can press the button again to move the camera
                }
            }
            else
            {
                if (GUILayout.Button("Go to", GUILayout.Width(60)))
                {
                    GotoScenePoint(arrayPoint.worldPosition);
                }
            }

            if (!cantDelete)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    pointList.RemovePoint(arrayPoint);
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        if (deflt == CameraPath.PointModes.Ease || deflt == CameraPath.PointModes.ControlPoints)
            return;

        //ADD NEW POINTS
        EditorGUILayout.BeginVertical("box");
        EditorGUI.BeginDisabledGroup(_pointMode != deflt);
        if (GUILayout.Button("Add Point From Inspector"))
            AddCPointAtPercent(_cameraPath.addPointAtPercent);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("At Percent", GUILayout.Width(80));
        _cameraPath.addPointAtPercent = EditorGUILayout.Slider(_cameraPath.addPointAtPercent, 0, 1);
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();

        if (_pointMode != add)
        {
            if (GUILayout.Button("Add Points in Scene"))
            {
                ChangePointMode(add);
            }
        }
        else
        {
            if (GUILayout.Button("Done Adding Points"))
            {
                ChangePointMode(deflt);
            }
        }

        EditorGUI.BeginDisabledGroup(numberOfPoints == 0);
        if (_pointMode != remove)
        {
            if (GUILayout.Button("Delete Points in Scene"))
            {
                ChangePointMode(remove);
            }
        }
        else
        {
            if (GUILayout.Button("Done"))
            {
                ChangePointMode(deflt);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    private static void OptionsInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        Texture2D cpLogo = Resources.Load<Texture2D>("Icons/logoDual400");
        GUILayout.Label(cpLogo, GUILayout.Width(400), GUILayout.Height(72));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Version " + _cameraPath.version);
        EditorGUILayout.SelectableLabel("Support Contact: email@jasperstocker.com");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Visit Support Site"))
            Application.OpenURL("http://camerapathanimator.jasperstocker.com");
        if (GUILayout.Button("Documentation"))
            Application.OpenURL("http://camerapathanimator.jasperstocker.com/documentation/");
        if (GUILayout.Button("Contact Jasper"))
            Application.OpenURL("mailto:email@jasperstocker.com");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        _cameraPath.showGizmos = EditorGUILayout.Toggle("Show Gizmos", _cameraPath.showGizmos);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Selected Path Colour");
        _cameraPath.selectedPathColour = EditorGUILayout.ColorField(_cameraPath.selectedPathColour);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unselected Path Colour");
        _cameraPath.unselectedPathColour = EditorGUILayout.ColorField(_cameraPath.unselectedPathColour);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Selected Point Colour");
        _cameraPath.selectedPointColour = EditorGUILayout.ColorField(_cameraPath.selectedPointColour);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unselected Point Colour");
        _cameraPath.unselectedPointColour = EditorGUILayout.ColorField(_cameraPath.unselectedPointColour);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Text Colour");
        _cameraPath.textColour = EditorGUILayout.ColorField(_cameraPath.textColour);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reset Colours"))
        {
            _cameraPath.selectedPathColour = CameraPathColours.GREEN;
            _cameraPath.unselectedPathColour = CameraPathColours.GREY;
            _cameraPath.selectedPointColour = CameraPathColours.RED;
            _cameraPath.unselectedPointColour = CameraPathColours.GREEN;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Preview Camera");

        CameraPathPreviewSupport.RenderPreview(_cameraPath, _animator, _animator.editorPercentage);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Width Resolution", GUILayout.Width(180));
        _cameraPath.previewResolution = EditorGUILayout.IntField(_cameraPath.previewResolution);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Height Resolution", GUILayout.Width(180));
        int height = Mathf.RoundToInt(_cameraPath.previewResolution / _cameraPath.aspect);
        int newHeight = EditorGUILayout.IntField(height);
        if (newHeight != height)
            _cameraPath.previewResolution = Mathf.RoundToInt(newHeight * _cameraPath.aspect);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Aspect Ratio", GUILayout.Width(180));
        _cameraPath.aspect = EditorGUILayout.FloatField(_cameraPath.aspect);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Display Height", GUILayout.Width(180));
        _cameraPath.displayHeight = EditorGUILayout.IntField(Mathf.Clamp(_cameraPath.displayHeight, 100, 500));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Rule of Thirds Overlay", GUILayout.Width(180));
        _cameraPath.ruleOfThirds = EditorGUILayout.Toggle(_cameraPath.ruleOfThirds);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Rule of Thirds Colour");
        _cameraPath.ruleOfThirdsColour = EditorGUILayout.ColorField(_cameraPath.ruleOfThirdsColour);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Preview Overlay", GUILayout.Width(180));
        _cameraPath.previewOverlay = (Texture2D)EditorGUILayout.ObjectField(_cameraPath.previewOverlay, typeof(Texture2D), false);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Clear Preview Overlay"))
            _cameraPath.previewOverlay = null;

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.HelpBox("The Stored Point Resolution is used by Camera Path Animator to help it judge how many samples it needs to take.\nA higher value means there are less points taken so calculation is faster but normalisation may be less accurate.\nBy default this value is dynamic, set as a percent of the path length but you can choose to hard set it.\nDon't touch it if that didn't just make sense though...", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Automatically Calculate Resolution");
        _cameraPath.autoSetStoedPointRes = EditorGUILayout.Toggle(_cameraPath.autoSetStoedPointRes, GUILayout.Width(20));
        EditorGUILayout.EndHorizontal();
        _cameraPath.storedPointResolution = EditorGUILayout.FloatField("Stored Point Resolution", _cameraPath.storedPointResolution);

        if (!_cameraPath.speedList.listEnabled && _animator != null)
        {
            if (_cameraPath.storedPointResolution > _animator.pathSpeed / 10)
                EditorGUILayout.HelpBox("The current stored point resolution is possibly too high. Lower it to less than the speed you're using", MessageType.Error);
        }
        else {
            if (_cameraPath.storedPointResolution > _cameraPath.speedList.GetLowesetSpeed() / 10)
                EditorGUILayout.HelpBox("The current stored point resolution is possibly too high. Lower it to less than the lowest speed you're using", MessageType.Error);
        }

        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.HelpBox("This is the percentage difference used when calculating the direction of a path. Larger values might help smooth out kinks within the path though too large and it might produce unpredicatble results.", MessageType.Info);
        EditorGUILayout.LabelField("Direction Calculation Width");
        _cameraPath.directionWidth = EditorGUILayout.Slider(_cameraPath.directionWidth, 0.0001f, 0.5f);
        EditorGUILayout.EndVertical();


        if (_animator != null)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Ease curves can end up outputing zero which would pause the animation indefinitly. We negate this issue by defining a minimum speed the animation is allowed to move. You can change that value here.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Minimum Animation Speed");
            _animator.minimumCameraSpeed = EditorGUILayout.FloatField(_animator.minimumCameraSpeed, GUILayout.Width(60));
            EditorGUILayout.LabelField("m/s", GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        _cameraPath.enableUndo = EditorGUILayout.Toggle("Enable Undo", _cameraPath.enableUndo);
        _cameraPath.enablePreviews = EditorGUILayout.Toggle("Enable Preview Windows", _cameraPath.enablePreviews);

        EditorGUILayout.Space();
        if (GUILayout.Button("Export to XML"))
            ExportXML();
        if (GUILayout.Button("Import from XML"))
        {
            string xmlpath = EditorUtility.OpenFilePanel("Import Camera Path from XML", "Assets/CameraPath3/", "xml");
            if (xmlpath != "")
                _cameraPath.FromXML(xmlpath);
        }
    }

    /// <summary>
    /// A little hacking of the Unity Editor to allow us to focus on an arbitrary point in 3D Space
    /// We're replicating pressing the F button in scene view to focus on the selected object
    /// Here we can focus on a 3D point
    /// </summary>
    /// <param name="position">The 3D point we want to focus on</param>
    private static void GotoScenePoint(Vector3 position)
    {
        Object[] intialFocus = Selection.objects;
        GameObject tempFocusView = new GameObject("Temp Focus View");
        tempFocusView.transform.position = position;
        try
        {
            Selection.objects = new Object[] { tempFocusView };
            SceneView.lastActiveSceneView.FrameSelected();
            Selection.objects = intialFocus;
        }
        catch (NullReferenceException)
        {
            //do nothing
        }
        Object.DestroyImmediate(tempFocusView);
    }

    private static void ToolbarMenuGUI()
    {
        bool isDefaultMenu = _animator == null || _animator.orientationMode != CameraPathAnimator.orientationModes.custom && _animator.orientationMode != CameraPathAnimator.orientationModes.followpath && _animator.orientationMode != CameraPathAnimator.orientationModes.reverseFollowpath;
        int currentPointModeA = -1;
        int currentPointModeB = -1;
        switch (_pointMode)
        {
            case CameraPath.PointModes.Transform:
                currentPointModeA = 0;
                break;
            case CameraPath.PointModes.AddPathPoints:
                currentPointModeA = 0;
                break;
            case CameraPath.PointModes.RemovePathPoints:
                currentPointModeA = 0;
                break;

            case CameraPath.PointModes.ControlPoints:
                currentPointModeA = 1;
                break;

            case CameraPath.PointModes.FOV:
                currentPointModeA = 2;
                break;
            case CameraPath.PointModes.AddFovs:
                currentPointModeA = 2;
                break;
            case CameraPath.PointModes.RemoveFovs:
                currentPointModeA = 2;
                break;

            case CameraPath.PointModes.Speed:
                currentPointModeA = 3;
                break;
            case CameraPath.PointModes.AddSpeeds:
                currentPointModeA = 3;
                break;
            case CameraPath.PointModes.RemoveSpeeds:
                currentPointModeA = 3;
                break;

            case CameraPath.PointModes.Delay:
                currentPointModeB = 0;
                break;
            case CameraPath.PointModes.AddDelays:
                currentPointModeB = 0;
                break;
            case CameraPath.PointModes.RemoveDelays:
                currentPointModeB = 0;
                break;

            case CameraPath.PointModes.Ease:
                currentPointModeB = 1;
                break;

            case CameraPath.PointModes.Events:
                currentPointModeB = 2;
                break;
            case CameraPath.PointModes.AddEvents:
                currentPointModeB = 2;
                break;
            case CameraPath.PointModes.RemoveEvents:
                currentPointModeB = 2;
                break;

            case CameraPath.PointModes.Orientations:
                currentPointModeB = 3;
                break;
            case CameraPath.PointModes.AddOrientations:
                currentPointModeB = 3;
                break;
            case CameraPath.PointModes.RemoveOrientations:
                currentPointModeB = 3;
                break;

            case CameraPath.PointModes.Tilt:
                currentPointModeB = 3;
                break;
            case CameraPath.PointModes.AddTilts:
                currentPointModeB = 3;
                break;
            case CameraPath.PointModes.RemoveTilts:
                currentPointModeB = 3;
                break;

            case CameraPath.PointModes.Options:
                currentPointModeB = (isDefaultMenu) ? 3 : 4;
                break;
        }
        int newPointModeA = GUILayout.Toolbar(currentPointModeA, _toolBarGUIContentA, GUILayout.Width(320), GUILayout.Height(64));
        int newPointModeB = GUILayout.Toolbar(currentPointModeB, _toolBarGUIContentB, GUILayout.Width((isDefaultMenu) ? 320 : 400), GUILayout.Height(64));

        if (newPointModeA != currentPointModeA)
        {
            switch (newPointModeA)
            {
                case 0:
                    if (_pointMode == CameraPath.PointModes.AddPathPoints)
                        return;
                    if (_pointMode == CameraPath.PointModes.RemovePathPoints)
                        return;
                    ChangePointMode(CameraPath.PointModes.Transform);
                    break;

                case 1:
                    ChangePointMode(CameraPath.PointModes.ControlPoints);
                    break;

                case 2:
                    if (_pointMode == CameraPath.PointModes.AddFovs)
                        return;
                    if (_pointMode == CameraPath.PointModes.RemoveFovs)
                        return;
                    ChangePointMode(CameraPath.PointModes.FOV);
                    break;

                case 3:
                    if (_pointMode == CameraPath.PointModes.AddSpeeds)
                        return;
                    if (_pointMode == CameraPath.PointModes.RemoveSpeeds)
                        return;
                    ChangePointMode(CameraPath.PointModes.Speed);
                    break;
            }
            GUI.changed = true;
        }
        if (newPointModeB != currentPointModeB)
        {
            switch (newPointModeB)
            {
                case 0:
                    if (_pointMode == CameraPath.PointModes.AddDelays)
                        return;
                    if (_pointMode == CameraPath.PointModes.RemoveDelays)
                        return;
                    ChangePointMode(CameraPath.PointModes.Delay);
                    break;

                case 1:
                    ChangePointMode(CameraPath.PointModes.Ease);
                    break;

                case 2:
                    if (_pointMode == CameraPath.PointModes.AddEvents)
                        return;
                    if (_pointMode == CameraPath.PointModes.RemoveEvents)
                        return;
                    ChangePointMode(CameraPath.PointModes.Events);
                    break;

                case 3:
                    if (isDefaultMenu)
                        ChangePointMode(CameraPath.PointModes.Options);
                    else
                    {
                        if (_animator.orientationMode == CameraPathAnimator.orientationModes.custom)
                        {
                            if (_pointMode == CameraPath.PointModes.AddOrientations)
                                return;
                            if (_pointMode == CameraPath.PointModes.RemoveOrientations)
                                return;
                            ChangePointMode(CameraPath.PointModes.Orientations);
                        }
                        else
                        {
                            if (_pointMode == CameraPath.PointModes.AddTilts)
                                return;
                            if (_pointMode == CameraPath.PointModes.RemoveTilts)
                                return;
                            ChangePointMode(CameraPath.PointModes.Tilt);
                        }
                    }
                    break;

                case 4:
                    ChangePointMode(CameraPath.PointModes.Options);
                    break;

            }
            GUI.changed = true;
        }
    }

    //    private static void RenderPreview(Vector3 position, Quaternion rotation, float viewSize)
    //    {
    //        if (_cameraPath.realNumberOfPoints < 2)
    //            return;
    //        if (!CameraPathPreviewSupport.previewSupported || _cameraPath.editorPreview == null)
    //            return;
    //
    //        EditorGUILayout.BeginHorizontal();
    //        EditorGUILayout.LabelField("Preview");
    //        string showPreviewButtonLabel = (_cameraPath.showPreview) ? "hide" : "show";
    //        if (GUILayout.Button(showPreviewButtonLabel, GUILayout.Width(74)))
    //            _cameraPath.showPreview = !_cameraPath.showPreview;
    //        EditorGUILayout.EndHorizontal();
    //
    //        if(!_cameraPath.enablePreviews || !_cameraPath.showPreview)
    //            return;
    //
    //        GameObject editorPreview = _cameraPath.editorPreview;
    //        if (CameraPathPreviewSupport.previewSupported && !EditorApplication.isPlaying)
    //        {
    //            RenderTexture rt = RenderTexture.GetTemporary(previewResolution, Mathf.RoundToInt(previewResolution / _cameraPath.aspect), 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB, 1);
    //
    //            editorPreview.SetActive(true);
    //            editorPreview.transform.position = position;
    //            editorPreview.transform.rotation = rotation;
    //
    //            Camera previewCam = editorPreview.GetComponent<Camera>();
    //            previewCam.enabled = true;
    //            if (previewCam.orthographic)
    //                previewCam.orthographicSize = _cameraPath.GetPathOrthographicSize(_animator.editorPercentage);
    //            else
    //                previewCam.fieldOfView = _cameraPath.GetPathFOV(_animator.editorPercentage);
    //
    //            previewCam.targetTexture = rt;
    //            previewCam.Render();
    //            previewCam.targetTexture = null;
    //            previewCam.enabled = false;
    //            editorPreview.SetActive(false);
    //
    //            GUILayout.Label("", GUILayout.Width(400), GUILayout.Height(225));
    //            Rect guiRect = GUILayoutUtility.GetLastRect();
    //            GUI.DrawTexture(guiRect, rt, ScaleMode.ScaleToFit, false);
    //            RenderTexture.ReleaseTemporary(rt);
    //        }
    //        else
    //        {
    //            string errorMsg = (!CameraPathPreviewSupport.previewSupported) ? CameraPathPreviewSupport.previewSupportedText : "No Preview When Playing.";
    //            EditorGUILayout.LabelField(errorMsg, GUILayout.Height(225));
    //        }
    //    }

    private static void AddPointToEnd()
    {
        CameraPathControlPoint newPoint = _cameraPath.gameObject.AddComponent<CameraPathControlPoint>();//ScriptableObject.CreateInstance<CameraPathControlPoint>();
        Vector3 finalPathPosition = _cameraPath.GetPathPosition(1.0f);
        Vector3 finalPathDirection = _cameraPath.GetPathDirection(1.0f);
        float finalArcLength = _cameraPath.StoredArcLength(_cameraPath.numberOfCurves - 1);
        if(finalArcLength < Mathf.Epsilon) finalArcLength = 1;
        Vector3 newPathPointPosition = finalPathPosition + finalPathDirection * (finalArcLength);
        newPoint.worldPosition = newPathPointPosition;
        newPoint.forwardControlPointLocal = _cameraPath[_cameraPath.realNumberOfPoints - 1].forwardControlPointLocal;
        _cameraPath.AddPoint(newPoint);
        ChangeSelectedPointIndex(_cameraPath.realNumberOfPoints - 1);
        GUI.changed = true;
    }

    private static void AddCPointAtPercent(float percent)
    {
        CameraPathPointList pointList = null;
        switch (_pointMode)
        {
            case CameraPath.PointModes.Orientations:
                pointList = _cameraPath.orientationList;
                break;
            case CameraPath.PointModes.FOV:
                pointList = _cameraPath.fovList;
                break;
            case CameraPath.PointModes.Tilt:
                pointList = _cameraPath.tiltList;
                break;
            case CameraPath.PointModes.Events:
                pointList = _cameraPath.eventList;
                break;
            case CameraPath.PointModes.Speed:
                pointList = _cameraPath.speedList;
                break;
            case CameraPath.PointModes.Delay:
                pointList = _cameraPath.delayList;
                break;
        }
        CameraPathControlPoint curvePointA = _cameraPath[_cameraPath.GetLastPointIndex(percent, false)];
        CameraPathControlPoint curvePointB = _cameraPath[_cameraPath.GetNextPointIndex(percent, false)];
        float curvePercentage = _cameraPath.GetCurvePercentage(curvePointA, curvePointB, percent);
        switch (_pointMode)
        {
            case CameraPath.PointModes.Orientations:
                Quaternion pointRotation = Quaternion.LookRotation(_cameraPath.GetPathDirection(percent));
                CameraPathOrientation newOrientation = ((CameraPathOrientationList)pointList).AddOrientation(curvePointA, curvePointB, curvePercentage, pointRotation);
                selectedPointIndex = (pointList.IndexOf(newOrientation));
                break;

            case CameraPath.PointModes.FOV:
                float pointFOV = _cameraPath.fovList.GetValue(percent, CameraPathFOVList.ProjectionType.FOV);
                float pointSize = _cameraPath.fovList.GetValue(percent, CameraPathFOVList.ProjectionType.Orthographic);
                CameraPathFOV newFOVPoint = ((CameraPathFOVList)pointList).AddFOV(curvePointA, curvePointB, curvePercentage, pointFOV, pointSize);
                selectedPointIndex = (pointList.IndexOf(newFOVPoint));
                break;

            case CameraPath.PointModes.Tilt:
                float pointTilt = _cameraPath.GetPathTilt(percent);
                CameraPathTilt newTiltPoint = ((CameraPathTiltList)pointList).AddTilt(curvePointA, curvePointB, curvePercentage, pointTilt);
                selectedPointIndex = (pointList.IndexOf(newTiltPoint));
                break;

            case CameraPath.PointModes.Events:
                CameraPathEvent newEventPoint = ((CameraPathEventList)pointList).AddEvent(curvePointA, curvePointB, curvePercentage);
                selectedPointIndex = (pointList.IndexOf(newEventPoint));
                break;

            case CameraPath.PointModes.Speed:
                _cameraPath.speedList.listEnabled = true;//if we're adding speeds then we probable want to enable it
                CameraPathSpeed newSpeedPoint = ((CameraPathSpeedList)pointList).AddSpeedPoint(curvePointA, curvePointB, curvePercentage);
                newSpeedPoint.speed = _animator.pathSpeed;
                selectedPointIndex = (pointList.IndexOf(newSpeedPoint));
                break;

            case CameraPath.PointModes.Delay:
                CameraPathDelay newDelayPoint = ((CameraPathDelayList)pointList).AddDelayPoint(curvePointA, curvePointB, curvePercentage);
                selectedPointIndex = (pointList.IndexOf(newDelayPoint));
                break;
        }
        GUI.changed = true;
    }

    private static void ChangePointMode(CameraPath.PointModes newPointMode)
    {
        _pointMode = newPointMode;
        EditorGUIUtility.hotControl = 0;
        EditorGUIUtility.keyboardControl = 0;
    }

    private static void ChangeSelectedPointIndex(int newPointSelected)
    {
        selectedPointIndex = newPointSelected;
        EditorGUIUtility.hotControl = 0;
        EditorGUIUtility.keyboardControl = 0;
    }

    public static void CleanUp()
    {
        if (_cameraPath.editorPreview != null)
            Object.DestroyImmediate(_cameraPath.editorPreview);
        Object.DestroyImmediate(selectedBoxColour);
        Object.DestroyImmediate(unselectedBoxColour);
    }

    private static void SetupToolbar()
    {
        int menuType = 0;
        CameraPathAnimator.orientationModes orientationMode;
        orientationMode = (_animator != null) ? _animator.orientationMode : CameraPathAnimator.orientationModes.none;
        switch (orientationMode)
        {
            case CameraPathAnimator.orientationModes.custom:
                menuType = 1;
                break;

            case CameraPathAnimator.orientationModes.followpath:
                menuType = 2;
                break;

            case CameraPathAnimator.orientationModes.reverseFollowpath:
                menuType = 2;
                break;

            default:
                menuType = 0;
                break;

        }

        int menuLengthA = 0;
        int menuLengthB = 0;
        string[] menuStringA = new string[0];
        string[] menuStringB = new string[0];
        Texture2D[] toolbarTexturesA = new Texture2D[0];
        Texture2D[] toolbarTexturesB = new Texture2D[0];
        switch (menuType)
        {
            default:
                menuLengthA = 4;
                menuLengthB = 4;
                menuStringA = new[] { "Path Points", "Control Points", "FOV", "Speed" };
                menuStringB = new[] { "Delays", "Ease", "Events", "Options" };
                toolbarTexturesA = new Texture2D[menuLengthA];
                toolbarTexturesB = new Texture2D[menuLengthB];
                toolbarTexturesA[0] = Resources.Load<Texture2D>("Icons/pathpoints");
                toolbarTexturesA[1] = Resources.Load<Texture2D>("Icons/controlpoints");
                toolbarTexturesA[2] = Resources.Load<Texture2D>("Icons/fov");
                toolbarTexturesA[3] = Resources.Load<Texture2D>("Icons/speed");
                toolbarTexturesB[0] = Resources.Load<Texture2D>("Icons/delay");
                toolbarTexturesB[1] = Resources.Load<Texture2D>("Icons/easecurves");
                toolbarTexturesB[2] = Resources.Load<Texture2D>("Icons/events");
                toolbarTexturesB[3] = Resources.Load<Texture2D>("Icons/options");
                //                toolbarTexturesB[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/options.png", typeof(Texture2D));
                //                toolbarTexturesA[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/pathpoints.png", typeof(Texture2D));
                //                toolbarTexturesA[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/controlpoints.png", typeof(Texture2D));
                //                toolbarTexturesA[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/fov.png", typeof(Texture2D));
                //                toolbarTexturesA[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/speed.png", typeof(Texture2D));
                //                toolbarTexturesB[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/delay.png", typeof(Texture2D));
                //                toolbarTexturesB[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/easecurves.png", typeof(Texture2D));
                //                toolbarTexturesB[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/events.png", typeof(Texture2D));
                //                toolbarTexturesB[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/options.png", typeof(Texture2D));

                break;
            case 1:
                menuLengthA = 4;
                menuLengthB = 5;
                menuStringA = new[] { "Path Points", "Control Points", "FOV", "Speed" };
                menuStringB = new[] { "Delays", "Ease", "Events", "Orientations", "Options" };
                toolbarTexturesA = new Texture2D[menuLengthA];
                toolbarTexturesB = new Texture2D[menuLengthB];
                //                toolbarTexturesA[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/pathpoints.png", typeof(Texture2D));
                //                toolbarTexturesA[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/controlpoints.png", typeof(Texture2D));
                //                toolbarTexturesA[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/fov.png", typeof(Texture2D));
                //                toolbarTexturesA[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/speed.png", typeof(Texture2D));
                //                toolbarTexturesB[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/delay.png", typeof(Texture2D));
                //                toolbarTexturesB[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/easecurves.png", typeof(Texture2D));
                //                toolbarTexturesB[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/events.png", typeof(Texture2D));
                //                toolbarTexturesB[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/orientation.png", typeof(Texture2D));
                //                toolbarTexturesB[4] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/options.png", typeof(Texture2D));
                toolbarTexturesA[0] = Resources.Load<Texture2D>("Icons/pathpoints");
                toolbarTexturesA[1] = Resources.Load<Texture2D>("Icons/controlpoints");
                toolbarTexturesA[2] = Resources.Load<Texture2D>("Icons/fov");
                toolbarTexturesA[3] = Resources.Load<Texture2D>("Icons/speed");
                toolbarTexturesB[0] = Resources.Load<Texture2D>("Icons/delay");
                toolbarTexturesB[1] = Resources.Load<Texture2D>("Icons/easecurves");
                toolbarTexturesB[2] = Resources.Load<Texture2D>("Icons/events");
                toolbarTexturesB[3] = Resources.Load<Texture2D>("Icons/orientation");
                toolbarTexturesB[4] = Resources.Load<Texture2D>("Icons/options");
                break;
            case 2:
                menuLengthA = 4;
                menuLengthB = 5;
                menuStringA = new[] { "Path Points", "Control Points", "FOV", "Speed" };
                menuStringB = new[] { "Delays", "Ease", "Events", "Tilt", "Options" };
                toolbarTexturesA = new Texture2D[menuLengthA];
                toolbarTexturesB = new Texture2D[menuLengthB];
                //                toolbarTexturesA[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/pathpoints.png", typeof(Texture2D));
                //                toolbarTexturesA[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/controlpoints.png", typeof(Texture2D));
                //                toolbarTexturesA[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/fov.png", typeof(Texture2D));
                //                toolbarTexturesA[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/speed.png", typeof(Texture2D));
                //                toolbarTexturesB[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/delay.png", typeof(Texture2D));
                //                toolbarTexturesB[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/easecurves.png", typeof(Texture2D));
                //                toolbarTexturesB[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/events.png", typeof(Texture2D));
                //                toolbarTexturesB[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/tilt.png", typeof(Texture2D));
                //                toolbarTexturesB[4] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/options.png", typeof(Texture2D));
                toolbarTexturesA[0] = Resources.Load<Texture2D>("Icons/pathpoints");
                toolbarTexturesA[1] = Resources.Load<Texture2D>("Icons/controlpoints");
                toolbarTexturesA[2] = Resources.Load<Texture2D>("Icons/fov");
                toolbarTexturesA[3] = Resources.Load<Texture2D>("Icons/speed");
                toolbarTexturesB[0] = Resources.Load<Texture2D>("Icons/delay");
                toolbarTexturesB[1] = Resources.Load<Texture2D>("Icons/easecurves");
                toolbarTexturesB[2] = Resources.Load<Texture2D>("Icons/events");
                toolbarTexturesB[3] = Resources.Load<Texture2D>("Icons/tilt");
                toolbarTexturesB[4] = Resources.Load<Texture2D>("Icons/options");
                break;
        }
        _toolBarGUIContentA = new GUIContent[menuLengthA];
        for (int i = 0; i < menuLengthA; i++)
            _toolBarGUIContentA[i] = new GUIContent(toolbarTexturesA[i], menuStringA[i]);
        _toolBarGUIContentB = new GUIContent[menuLengthB];
        for (int i = 0; i < menuLengthB; i++)
            _toolBarGUIContentB[i] = new GUIContent(toolbarTexturesB[i], menuStringB[i]);

        if (_animator != null)
            _orientationmode = _animator.orientationMode;
    }

    private static void ExportXML()
    {
        string currentSceneRaw = "";
#if UNITY_5_3_OR_NEWER
        currentSceneRaw = SceneManager.GetActiveScene().name;
#else
        currentSceneRaw = EditorApplication.currentScene;
#endif
        string[] currentScene = currentSceneRaw.Split(char.Parse("/"));
        currentScene = currentScene[currentScene.Length - 1].Split(char.Parse("."));
        string defaultName = string.Format("{0}_{1}", currentScene[0], _cameraPath.name);
        defaultName = defaultName.Replace(" ", "_");
        string filepath = EditorUtility.SaveFilePanel("Export Camera Path Animator to XML", "Assets/CameraPath3", defaultName, "xml");

        if (filepath != "")
        {
            using (StreamWriter sw = new StreamWriter(filepath))
            {
                sw.Write(_cameraPath.ToXML());//write out contents of data to XML
            }
        }
    }

    private static void AutoSetControlPoint(CameraPathControlPoint point)
    {
        CameraPathControlPoint point0 = _cameraPath.GetPoint(point.index - 1);
        CameraPathControlPoint point1 = _cameraPath.GetPoint(point.index + 1);

        float distanceA = Vector3.Distance(point.worldPosition, point0.worldPosition);
        float distanceB = Vector3.Distance(point.worldPosition, point1.worldPosition);
        float controlPointLength = Mathf.Min(distanceA, distanceB) * 0.33333f;
        Vector3 controlPointDirection = ((point.worldPosition - point0.worldPosition) + (point1.worldPosition - point.worldPosition)).normalized;
        point.forwardControlPointLocal = controlPointDirection * controlPointLength;
    }

    private static void ResetFocus()
    {
        EditorGUIUtility.hotControl = 0;
        EditorGUIUtility.keyboardControl = 0;
    }
}
