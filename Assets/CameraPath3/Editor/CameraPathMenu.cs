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
using UnityEditor;

public class CameraPathMenu : EditorWindow
{
    [MenuItem("GameObject/Create New Camera Path", false, 0)]
    public static void CreateNewCameraPath()
    {
        GameObject newCameraPath = new GameObject("New Camera Path");
        Undo.RegisterCreatedObjectUndo(newCameraPath, "Created New Camera Path");
        newCameraPath.AddComponent<CameraPath>();
        CameraPathAnimator animator = newCameraPath.AddComponent<CameraPathAnimator>();
        if(Camera.main != null)
            animator.animationObject = Camera.main.transform;
        Selection.objects = new Object[] { newCameraPath };
        SceneView.lastActiveSceneView.FrameSelected();
//        EditorUtility.SetDirty(newCameraPath);
    }

    [MenuItem("GameObject/Align Camera Path Point to View", false, 1001)]
    public static void AlignPathPoint()
    {
        GameObject selected = Selection.activeGameObject;
        CameraPath camPath = selected.GetComponent<CameraPath>();
        CameraPathAnimator animator = selected.GetComponent<CameraPathAnimator>();
        Undo.RecordObject(camPath,"Align Camera Path Point to View");
        if (camPath != null && animator!= null)
        {
            switch (animator.orientationMode)
            {
                case CameraPathAnimator.orientationModes.custom:
                    if (camPath.pointMode == CameraPath.PointModes.Orientations)
                    {
                        int selectedPoint = camPath.selectedPoint;
                        CameraPathOrientation point = camPath.orientationList[selectedPoint];
                        Camera sceneCam = SceneView.GetAllSceneCameras()[0];
                        Quaternion lookRotation = Quaternion.LookRotation(sceneCam.transform.forward);
                        point.rotation = lookRotation;
                        if (point.positionModes == CameraPathPoint.PositionModes.FixedToPoint)
                        {
                            CameraPathControlPoint cPoint = point.point;
                            cPoint.worldPosition = sceneCam.transform.position;
                        }

                        camPath.RecalculateStoredValues();
                    }

                    if (camPath.pointMode == CameraPath.PointModes.Transform || camPath.pointMode == CameraPath.PointModes.ControlPoints)
                    {
                        int selectedPoint = camPath.selectedPoint;
                        CameraPathControlPoint cPoint = camPath[selectedPoint];
                        Camera sceneCam = SceneView.GetAllSceneCameras()[0];
                        cPoint.worldPosition = sceneCam.transform.position;
                        CameraPathOrientation point = (CameraPathOrientation)camPath.orientationList.GetPoint(cPoint);
                        if(point != null)
                        {                          
                            Quaternion lookRotation = Quaternion.LookRotation(sceneCam.transform.forward);
                            point.rotation = lookRotation;
                        }
                        camPath.RecalculateStoredValues();
                    }
                    break;
                default:
                    if (camPath.pointMode == CameraPath.PointModes.Transform || camPath.pointMode == CameraPath.PointModes.ControlPoints)
                    {
                        int selectedPoint = camPath.selectedPoint;
                        CameraPathControlPoint point = camPath[selectedPoint];
                        Camera sceneCam = SceneView.GetAllSceneCameras()[0];
                        float forwardArcLength = camPath.StoredArcLength(selectedPoint);
                        point.forwardControlPointLocal = sceneCam.transform.forward * (Mathf.Max(forwardArcLength,0.1f) * 0.33f);
                        point.worldPosition = sceneCam.transform.position;
                        camPath.RecalculateStoredValues();
                    }
                    break;
            }

        }
    }

    // Validate the menu item defined by the function above.
    // The menu item will be disabled if this function returns false.
    [MenuItem("GameObject/Align Camera Path Point to View", true)]
    static bool ValidateAlignPathPointtoView ()
    {
        GameObject selected = Selection.activeGameObject;
        if(selected == null)
            return false;
        CameraPath camPath = selected.GetComponent<CameraPath>();
        if (camPath==null)
            return false;
        switch(camPath.pointMode)
        {
            case CameraPath.PointModes.ControlPoints:
                return true;

            case CameraPath.PointModes.Transform:
                return true;

            case CameraPath.PointModes.Orientations:
                return true;

            default:
                return false;
        }
    }


    [MenuItem("GameObject/Align View to Camera Path Point", false, 1002)]
    public static void AlignCamera()
    {
        GameObject selected = Selection.activeGameObject;
        CameraPath camPath = selected.GetComponent<CameraPath>();
        CameraPathAnimator animator = selected.GetComponent<CameraPathAnimator>();
        Undo.RecordObject(camPath, "Align View to Camera Path Point");
        if (camPath != null && animator != null)
        {
            switch (animator.orientationMode)
            {
                case CameraPathAnimator.orientationModes.custom:
                    if (camPath.pointMode == CameraPath.PointModes.Orientations)
                    {
                        int selectedPoint = camPath.selectedPoint;
                        CameraPathOrientation point = camPath.orientationList[selectedPoint];
                        GameObject tempPos = new GameObject("temp");
                        tempPos.transform.position = point.worldPosition;
                        tempPos.transform.rotation = point.rotation;
                        SceneView.currentDrawingSceneView.AlignViewToObject(tempPos.transform);
                        DestroyImmediate(tempPos);
                    }

                    if (camPath.pointMode == CameraPath.PointModes.Transform || camPath.pointMode == CameraPath.PointModes.ControlPoints)
                    {
                        int selectedPoint = camPath.selectedPoint;
                        CameraPathControlPoint cPoint = camPath[selectedPoint];
                        CameraPathOrientation point = (CameraPathOrientation)camPath.orientationList.GetPoint(cPoint);
                        GameObject tempPos = new GameObject("temp");
                        tempPos.transform.position = cPoint.worldPosition;
                        if(Camera.current != null)
                        {
                            Camera.current.transform.position = cPoint.worldPosition;
                        }
                        if(point != null)
                        {
                            tempPos.transform.rotation = point.rotation;
                        }
                        else
                        {
                            tempPos.transform.rotation = Quaternion.LookRotation(camPath.GetPathDirection(cPoint.percentage));
                        }
                        SceneView.currentDrawingSceneView.AlignViewToObject(tempPos.transform);
                        DestroyImmediate(tempPos);
                    }
                    break;
                default:
                    if (camPath.pointMode == CameraPath.PointModes.Transform || camPath.pointMode == CameraPath.PointModes.ControlPoints)
                    {
                        int selectedPoint = camPath.selectedPoint;
                        CameraPathControlPoint point = camPath[selectedPoint];
                        GameObject tempPos = new GameObject("temp");
                        tempPos.transform.position = point.worldPosition;
                        tempPos.transform.rotation = Quaternion.LookRotation(point.forwardControlPoint.normalized);
                        SceneView.currentDrawingSceneView.AlignViewToObject(tempPos.transform);
                        DestroyImmediate(tempPos);
                    }
                    break;
            }

        }
    }

    // Validate the menu item defined by the function above.
    // The menu item will be disabled if this function returns false.
    [MenuItem("GameObject/Align View to Camera Path Point", true)]
    static bool ValidateAlignViewtoPathPoint()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
            return false;
        CameraPath camPath = selected.GetComponent<CameraPath>();
        if (camPath == null)
            return false;
        switch (camPath.pointMode)
        {
            case CameraPath.PointModes.ControlPoints:
                return true;

            case CameraPath.PointModes.Transform:
                return true;

            case CameraPath.PointModes.Orientations:
                return true;

            default:
                return false;
        }
    }
}