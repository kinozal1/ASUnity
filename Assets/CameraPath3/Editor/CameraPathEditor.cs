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

[CanEditMultipleObjects]
[CustomEditor(typeof(CameraPath))]
public class CameraPathEditor : Editor
{
    private CameraPath _cameraPath;
    private CameraPathAnimator _animator;

    private void OnEnable()
    {
        if(target != null)
        {
            _cameraPath = (CameraPath)target;
            _animator = _cameraPath.GetComponent<CameraPathAnimator>();
        }

        CameraPathEditorSceneGUI._cameraPath = _cameraPath;
        CameraPathEditorSceneGUI._animator = _animator;
        CameraPathEditorSceneGUI.colouredText = new GUIStyle();
        CameraPathEditorSceneGUI.colouredText.normal.textColor = _cameraPath.textColour;

        CameraPathEditorInspectorGUI._cameraPath = _cameraPath;
        CameraPathEditorInspectorGUI._animator = _animator;
        CameraPathEditorInspectorGUI.Setup();
    }

    private void OnDisable()
    {
        CleanUp();
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    private void OnSceneGUI()
    {
        CameraPathEditorSceneGUI.OnSceneGUI();

        if(GUI.changed)
        {
            UpdateGui();
        }
    }

    public override void OnInspectorGUI()
    {
//        if (_cameraPath.enableUndo) Undo.RecordObject(_cameraPath, "Modified Camera Path");
        CameraPathEditorInspectorGUI.OnInspectorGUI();

        if(GUI.changed)
        {
            UpdateGui();
        }
    }

    /// <summary>
    /// Handle GUI changes and repaint requests
    /// </summary>
    private void UpdateGui()
    {
        Repaint();
        HandleUtility.Repaint();
        SceneView.RepaintAll();
        _cameraPath.RecalculateStoredValues();
        _cameraPath.ruleOfThirdsOverlay = null;
//        EditorUtility.SetDirty(_cameraPath);
//        if(_animator!=null)
//            EditorUtility.SetDirty(_animator);
    }

    private void CleanUp()
    {
        CameraPathEditorInspectorGUI.CleanUp();

    }
}