using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraPathPreviewSupport
{
    public static bool previewSupported
    {
        get
        {
#if UNITY_EDITOR
#if !UNITY_5_6_OR_NEWER
            if (!SystemInfo.supportsRenderTextures) return false;
#endif
#endif
            return true;
        }
    }
    public static string previewSupportedText
    {
        get
        {
#if UNITY_EDITOR
#if !UNITY_5_6_OR_NEWER
            if (!SystemInfo.supportsRenderTextures) return "Render Textures is not support now";
#endif
#endif
            return "";
        }
    }

#if UNITY_EDITOR
    public static void RenderPreview(CameraPath path, CameraPathAnimator animator, float percent, bool ignoreNormalisation = false)//, float viewSize)
    {
        if (path.realNumberOfPoints < 2)
            return;
        if (!previewSupported || path.editorPreview == null)
            return;

        //Get animation values and apply them to the preview camera
        Vector3 position = path.GetPathPosition(percent, ignoreNormalisation);
        Quaternion rotation = animator.GetAnimatedOrientation(percent, false);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Preview");
        string showPreviewButtonLabel = (path.showPreview) ? "hide" : "show";
        if (GUILayout.Button(showPreviewButtonLabel, GUILayout.Width(50)))
            path.showPreview = !path.showPreview;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (!path.enablePreviews || !path.showPreview)
            return;

        GameObject editorPreview = path.editorPreview;
        if (previewSupported && !EditorApplication.isPlaying)
        {
            int width = TextureWidth(path);
            int height = TextureHeight(path);
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB, 1);

            editorPreview.SetActive(true);
            editorPreview.transform.position = position;
            editorPreview.transform.rotation = rotation;

            Camera previewCam = editorPreview.GetComponent<Camera>();
            previewCam.enabled = true;
            if (path.fovList.listEnabled)
            {
                if (previewCam.orthographic)
                    previewCam.orthographicSize = path.GetPathOrthographicSize(percent);
                else
                    previewCam.fieldOfView = path.GetPathFOV(percent);
            }
            else
            {
                previewCam.fieldOfView = 60;
            }
            previewCam.farClipPlane = path.drawDistance;

            previewCam.targetTexture = rt;
            previewCam.Render();
            previewCam.targetTexture = null;
            previewCam.enabled = false;
            editorPreview.SetActive(false);

            //            GUILayout.Label("", GUILayout.Width(400), GUILayout.Height(path.displayHeight));
            Rect guiRect = GUILayoutUtility.GetAspectRect(path.aspect, GUILayout.Width(400), GUILayout.Height(path.displayHeight));
            GUI.DrawTexture(guiRect, rt, ScaleMode.ScaleToFit, false);

            if (path.ruleOfThirds)
                GUI.DrawTexture(guiRect, ROTPreview(path), ScaleMode.StretchToFill, true);
            if (path.previewOverlay != null)
                GUI.DrawTexture(guiRect, path.previewOverlay, ScaleMode.StretchToFill, true);
            RenderTexture.ReleaseTemporary(rt);
        }
        else
        {
            string errorMsg = (!previewSupported) ? previewSupportedText : "No Preview When Playing.";
            EditorGUILayout.LabelField(errorMsg, GUILayout.Height(path.displayHeight));
        }
        EditorGUILayout.Space();
    }

    public static Camera GetSceneCamera()
    {
        Camera[] cams = Camera.allCameras;
        bool sceneHasCamera = cams.Length > 0;
        Camera sceneCamera = null;
        if (Camera.main)
        {
            sceneCamera = Camera.main;
        }
        else if (sceneHasCamera)
        {
            sceneCamera = cams[0];
        }

        return sceneCamera;
    }

    public static int TextureWidth(CameraPath path)
    {
        return Mathf.Clamp(path.previewResolution, 1, 1024);
    }


    public static int TextureHeight(CameraPath path)
    {
        return Mathf.Clamp(Mathf.RoundToInt(path.previewResolution / path.aspect), 1, 1024);
    }

    public static Texture2D ROTPreview(CameraPath path)
    {
        if(!path.ruleOfThirds) return null;
        if (path.ruleOfThirdsOverlay == null)
        {
            int previewWidth = TextureWidth(path);
            int previewHeight = TextureHeight(path);
            Texture2D overlay = new Texture2D(previewWidth, previewHeight, TextureFormat.ARGB32, false);

            int h1 = Mathf.RoundToInt(previewWidth / 3f);
            int h2 = h1 * 2;
            int v1 = Mathf.RoundToInt(previewHeight / 3f);
            int v2 = v1 * 2;

            Color col = path.ruleOfThirdsColour;
            Color blank = new Color(0,0,0,0);
            for(int x = 0; x < previewWidth; x++)
            {
                for(int y = 0; y < previewHeight; y++)
                {
                    bool isBlank = true;
                    if(x == h1 || x == h2) isBlank = false;
                    if(y == v1 || y == v2) isBlank = false;
                    overlay.SetPixel(x, y, isBlank ? blank : col);
                }
            }
//            for (int h = 0; h < previewWidth; h++)
//            {
//                overlay.SetPixel(h, v1, col);
//                overlay.SetPixel(h, v2, col);
//            }
//            for (int v = 0; v < previewHeight; v++)
//            {
//                overlay.SetPixel(h1, v, col);
//                overlay.SetPixel(h2, v, col);
//            }
            overlay.Apply(false);
            path.ruleOfThirdsOverlay = overlay;
        }
        return path.ruleOfThirdsOverlay;
    }
#endif
}
