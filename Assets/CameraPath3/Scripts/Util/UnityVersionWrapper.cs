using UnityEngine;

namespace CPA
{
    public class UnityVersionWrapper
    {
#if UNITY_EDITOR

        public static void HandlesArrowCap(int controlId, Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
            

            UnityEditor.Handles.ArrowCap(controlId, position, rotation, size);
#endif
        }

        public static void HandlesDotCap(int controlId, Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
            

            UnityEditor.Handles.DotCap(controlId, position, rotation, size);
#endif
        }

        public static void HandlesSphereCap(int controlId, Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
            

            UnityEditor.Handles.SphereCap(controlId, position, rotation, size);
#endif
        }

        public static void HandlesCircleCap(int controlId, Vector3 position, Quaternion rotation, float size)
        {
#if UNITY_5_6_OR_NEWER
           

            UnityEditor.Handles.CircleCap(controlId, position, rotation, size);
#endif
        }

        public static bool HandlesDotButton(Vector3 position, Quaternion rotation, float size, float pickSize)
        {
#if UNITY_5_6_OR_NEWER
            return UnityEditor.Handles.Button(position, rotation, size, pickSize, UnityEditor.Handles.DotHandleCap);
#else
            return UnityEditor.Handles.Button(position, rotation, size, pickSize, UnityEditor.Handles.DotCap);
#endif
        }

        public static Vector3 HandlesSlider(Vector3 position, Vector3 direction, float size, float snap)
        {
#if UNITY_5_6_OR_NEWER
            return UnityEditor.Handles.Slider(position, direction, size, UnityEditor.Handles.ArrowHandleCap, snap);
#else
            return UnityEditor.Handles.Slider(position, direction, size, UnityEditor.Handles.ArrowCap, snap);
#endif
        }

        public static Vector3 HandlesFreeMoveHandle(Vector3 position, Quaternion rotation, float size, Vector3 snap)
        {
#if UNITY_5_6_OR_NEWER
            return UnityEditor.Handles.FreeMoveHandle(position, rotation, size, snap, UnityEditor.Handles.DotHandleCap);
#else
            return UnityEditor.Handles.FreeMoveHandle(position, rotation, size, snap, UnityEditor.Handles.DotCap);
#endif
        }


#endif
    }
}