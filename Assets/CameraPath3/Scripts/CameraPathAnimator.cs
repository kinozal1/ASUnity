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
using System.Text;
using System.Xml;
#endif

public class CameraPathAnimator : MonoBehaviour
{
    public float minimumCameraSpeed = 0.01f;

    public enum animationModes
    {
        once,
        loop,
        reverse,
        reverseLoop,
        pingPong,
        still
    }

    public enum orientationModes
    {
        custom,//rotations will be decided by defining orientations along the curve
        target,//camera will always face a defined transform
        mouselook,//camera will have a mouse free look
        followpath,//camera will use the path to determine where to face - maintaining world up as up
        reverseFollowpath,//camera will use the path to determine where to face, looking back on the path
        followTransform,//move the object to the nearest point on the path and look at target
        twoDimentions,
        fixedOrientation,
        none
    }

    public Transform orientationTarget;
    [SerializeField]
    private CameraPath _cameraPath;
    //do you want this path to automatically animate at the start of your scene
    public bool playOnStart = true;
    //the actual transform you want to animate
    public Transform animationObject;
    //a link to the camera component
    private Camera animationObjectCamera;
    //is the transform you are animating a camera?
    private bool _isCamera = true;
    private bool _playing;
    public animationModes animationMode = animationModes.once;
    [SerializeField]
    private orientationModes _orientationMode = orientationModes.custom;
    public bool smoothOrientationModeChanges = false;
    public float orientationModeLerpTime = 0.3f;
    private float _orientationModeLerpTimer = 0;
    private orientationModes _previousOrientationMode = orientationModes.custom;

    private float pingPongDirection = 1;
    public Vector3 fixedOrientaion = Vector3.forward;
    public Vector3 fixedPosition;

    public bool normalised = true;
    [SerializeField]
    private bool _dynamicNormalisation = false;

    //the time used in the editor to preview the path animation
    public float editorPercentage = 0;
    //the time the path animation should last for
    [SerializeField]
    private float _pathTime = 10;
    //the time the path animation should last for
    [SerializeField]
    private float _pathSpeed = 10;
    private float _percentage;
    private float _lastPercentage;
    public float nearestOffset = 0;
    private float _delayTime;
    public float startPercent = 0;
    public bool animateFOV = true;
    public Vector3 targetModeUp = Vector3.up;

    //the sensitivity of the mouse in mouselook
    public float sensitivity = 5.0f;
    //the minimum the mouse can move down
    public float minX = -90.0f;
    //the maximum the mouse can move up
    public float maxX = 90.0f;

    private bool _animateSceneObjectInEditor;

    public Vector3 animatedObjectStartPosition;
    public Quaternion animatedObjectStartRotation;

    //Events
    public delegate void AnimationStartedEventHandler();
    public delegate void AnimationPausedEventHandler();
    public delegate void AnimationStoppedEventHandler();
    public delegate void AnimationFinishedEventHandler();
    public delegate void AnimationLoopedEventHandler();
    public delegate void AnimationPingPongEventHandler();
    public delegate void AnimationPointReachedEventHandler();
    public delegate void AnimationCustomEventHandler(string eventName);

    public delegate void AnimationPointReachedWithNumberEventHandler(int pointNumber);

    /// <summary>
    /// Broadcast when the Animation has begun
    /// </summary>
    public event AnimationStartedEventHandler AnimationStartedEvent;
    /// <summary>
    /// Broadcast when the animation is paused
    /// </summary>
    public event AnimationPausedEventHandler AnimationPausedEvent;
    /// <summary>
    /// Broadcast when the animation is stopped
    /// </summary>
    public event AnimationStoppedEventHandler AnimationStoppedEvent;
    /// <summary>
    /// Broadcast when the animation is complete
    /// </summary>
    public event AnimationFinishedEventHandler AnimationFinishedEvent;
    /// <summary>
    /// Broadcast when the animation has reached the end of the loop and begins the animation again
    /// </summary>
    public event AnimationLoopedEventHandler AnimationLoopedEvent;
    /// <summary>
    /// Broadcast when the end of a path animation is reached and the animation ping pongs back
    /// </summary>
    public event AnimationPingPongEventHandler AnimationPingPongEvent;
    /// <summary>
    /// Broadcast when a point is reached
    /// </summary>
    public event AnimationPointReachedEventHandler AnimationPointReachedEvent;
    /// <summary>
    /// Broadcast when a point is reached sending the point number index with it
    /// </summary>
    public event AnimationPointReachedWithNumberEventHandler AnimationPointReachedWithNumberEvent;
    /// <summary>
    /// Broadcast when a user defined event is fired sending the event name as a string
    /// </summary>
    public event AnimationCustomEventHandler AnimationCustomEvent;

    //PUBLIC METHODS

    //Script based controls - hook up your scripts to these to control your

    /// <summary>
    /// Gets or sets the path speed.
    /// </summary>
    /// <value>
    /// The path speed.
    /// </value>
    public virtual float pathSpeed
    {
        get
        {
            return _pathSpeed;
        }
        set
        {
            if (_cameraPath.speedList.listEnabled)
                Debug.LogWarning("Path Speed in Animator component is ignored and overridden by Camera Path speed points.");
            _pathSpeed = Mathf.Max(value, minimumCameraSpeed);
        }
    }

    /// <summary>
    /// Gets or sets the path time (use only in the Animation Mode Still.)
    /// </summary>
    /// <value>
    /// The animation time.
    /// </value>
    public virtual float animationTime
    {
        get
        {
            return _pathTime;
        }
        set
        {
            if (animationMode != animationModes.still)
                Debug.LogWarning("Path time is ignored and overridden during animation when not in Animation Mode Still.");
            _pathTime = Mathf.Max(value, 0.0f);
        }
    }

    /// <summary>
    /// Retreive the current time of the path animation
    /// </summary>
    public virtual float currentTime
    {
        get { return _pathTime * _percentage; }
    }

    /// <summary>
    /// Play the path. If path has finished do not play it.
    /// </summary>
    public virtual void Play()
    {
        _playing = true;
        if (!isReversed)
        {
            if(_percentage == 0)
            {
                if (AnimationStartedEvent != null) AnimationStartedEvent();
                cameraPath.eventList.OnAnimationStart(0);
            }
        }
        else
        {
            if(_percentage == 1)
            {
                if (AnimationStartedEvent != null) AnimationStartedEvent();
                cameraPath.eventList.OnAnimationStart(1);
            }
        }
        _lastPercentage = _percentage;
    }

    /// <summary>
    /// Stop and reset the animation back to the beginning
    /// </summary>
    public virtual void Stop()
    {
        _playing = false;
        _percentage = 0;
        if (AnimationStoppedEvent != null) AnimationStoppedEvent();
    }

    /// <summary>
    /// Pause the animation where it is
    /// </summary>
    public virtual void Pause()
    {
        _playing = false;
        if (AnimationPausedEvent != null) AnimationPausedEvent();
    }

    /// <summary>
    /// set the time of the animtion
    /// </summary>
    /// <param name="value">Seek Percent 0-1</param>
    public virtual void Seek(float value)
    {
        _percentage = Mathf.Clamp01(value);
        _lastPercentage = _percentage;
        //thanks kelnishi!
        UpdateAnimationTime(false);
        UpdatePointReached();
        bool p = _playing;
        _playing = true;
        UpdateAnimation();
        _playing = p;
    }

    /// <summary>
    /// Is the animation playing
    /// </summary>
    public virtual bool isPlaying
    {
        get { return _playing; }
    }

    /// <summary>
    /// Current percent of animation
    /// </summary>
    public virtual float percentage
    {
        get { return _percentage; }
    }

    /// <summary>
    /// Is the animation ping pong direction forward
    /// </summary>
    public virtual bool pingPongGoingForward
    {
        get { return pingPongDirection == 1; }
    }

    /// <summary>
    /// Reverse the animation
    /// </summary>
    public virtual void Reverse()
    {
        switch (animationMode)
        {
            case animationModes.once:
                animationMode = animationModes.reverse;
                break;
            case animationModes.reverse:
                animationMode = animationModes.once;
                break;
            case animationModes.pingPong:
                pingPongDirection = pingPongDirection == -1 ? 1 : -1;
                break;
            case animationModes.loop:
                animationMode = animationModes.reverseLoop;
                break;
            case animationModes.reverseLoop:
                animationMode = animationModes.loop;
                break;
        }
    }

    /// <summary>
    /// A link to the Camera Path component
    /// </summary>
    public virtual CameraPath cameraPath
    {
        get
        {
            if (!_cameraPath)
                _cameraPath = GetComponent<CameraPath>();
            return _cameraPath;
        }
    }

    public virtual bool dynamicNormalisation
    {
        get {return _dynamicNormalisation;}
        set
        {
            if(value)
            {
                _dynamicNormalisation = true;
                _cameraPath.normalised = false;
            }
            else
            {
                _dynamicNormalisation = false;
            }
        }
    }

    public virtual orientationModes orientationMode
    {
        get { return _orientationMode; }
        set
        {
            if(_orientationMode != value)
            {
                _orientationModeLerpTimer = 0;
                _previousOrientationMode = _orientationMode;
                _orientationMode = value;
            }
        }
    }

    public virtual Quaternion GetOrientation(orientationModes mode, float percent, bool ignoreNormalisation)
    {
        Quaternion output = Quaternion.identity;
        Vector3 currentPosition, forward;
        switch (mode)
        {
            case orientationModes.custom:
                output = cameraPath.GetPathRotation(percent, ignoreNormalisation);
                break;

            case orientationModes.target:
                currentPosition = cameraPath.GetPathPosition(percent);//transform.TransformPoint(cameraPath.GetPathPosition(percent));
                if (orientationTarget != null)
                    forward = orientationTarget.transform.position - currentPosition;
                else
                    forward = Vector3.forward;
                output = Quaternion.LookRotation(forward, targetModeUp);
                break;

            case orientationModes.followpath:
                output = Quaternion.LookRotation(cameraPath.GetPathDirection(percent));
                output *= Quaternion.Euler(transform.forward * -cameraPath.GetPathTilt(percent));
                break;

            case orientationModes.reverseFollowpath:
                output = Quaternion.LookRotation(-cameraPath.GetPathDirection(percent));
                output *= Quaternion.Euler(transform.forward * -cameraPath.GetPathTilt(percent));
                break;

            case orientationModes.mouselook:
                if (!Application.isPlaying)
                {
                    output = Quaternion.LookRotation(cameraPath.GetPathDirection(percent));
                    output *= Quaternion.Euler(transform.forward * -cameraPath.GetPathTilt(percent));
                }
                else
                {
                    //                    output = GetMouseLook();
                    output = Quaternion.LookRotation(cameraPath.GetPathDirection(percent));
                    output *= GetMouseLook();
                }
                break;

            case orientationModes.followTransform:
                if (orientationTarget == null)
                    return Quaternion.identity;
                float nearestPerc = cameraPath.GetNearestPoint(orientationTarget.position);
                nearestPerc = Mathf.Clamp01(nearestPerc + nearestOffset);
                currentPosition = cameraPath.GetPathPosition(nearestPerc);
                forward = orientationTarget.transform.position - currentPosition;
                output = Quaternion.LookRotation(forward);
                break;

            case orientationModes.twoDimentions:
                output = Quaternion.LookRotation(Vector3.forward);
                break;

            case orientationModes.fixedOrientation:
                output = Quaternion.LookRotation(fixedOrientaion);
                break;

            case orientationModes.none:
                output = animationObject.rotation;
                break;

        }
        return output;
    }

    /// <summary>
    /// Retrieve the animation orientation at a percent based on the animation mode
    /// </summary>
    /// <param name="percent">Path Percent 0-1</param>
    /// <param name="ignoreNormalisation">Should the percetage be normalised</param>
    /// <returns>A rotation</returns>
    public virtual Quaternion GetAnimatedOrientation(float percent, bool ignoreNormalisation)
    {
        Quaternion output = GetOrientation(_orientationMode, percent, ignoreNormalisation);

        if(smoothOrientationModeChanges && _orientationModeLerpTimer < orientationModeLerpTime)
        {
            Quaternion previous = GetOrientation(_previousOrientationMode, percent, ignoreNormalisation);
            float lerpPercent = _orientationModeLerpTimer / orientationModeLerpTime;
            float smoothStep = Mathf.SmoothStep(0, 1, lerpPercent);
//            smoothStep = Mathf.SmoothStep(0, 1, smoothStep);
//            float smoothStep = CPMath.SmoothStep(lerpPercent);
            output = Quaternion.Slerp(previous, output, smoothStep);
//            Quaternion.
        }

        output *= transform.rotation;

        return output;
    }
    
    public virtual bool isCamera
    {
        get
        {
            if (animationObject == null)
                _isCamera = false;
            else
            {
                _isCamera = animationObjectCamera != null;
            }
            return _isCamera;
        }
    }

    public virtual bool animateSceneObjectInEditor
    {
        get { return _animateSceneObjectInEditor; }
        set
        {
            if (value != _animateSceneObjectInEditor)
            {
                _animateSceneObjectInEditor = value;
                if (animationObject != null && animationMode != animationModes.still)
                {
                    if (_animateSceneObjectInEditor)
                    {
                        animatedObjectStartPosition = animationObject.transform.position;
                        animatedObjectStartRotation = animationObject.transform.rotation;
                    }
                    else
                    {
                        animationObject.transform.position = animatedObjectStartPosition;
                        animationObject.transform.rotation = animatedObjectStartRotation;
                    }
                }
            }
            _animateSceneObjectInEditor = value;
        }
    }

    //MONOBEHAVIOURS

    protected virtual void Awake()
    {
        if(animationObject == null)
            _isCamera = false;
        else
        {
            animationObjectCamera = animationObject.GetComponentInChildren<Camera>();
            _isCamera = animationObjectCamera != null;
        }

        Camera[] cams = Camera.allCameras;
        if (cams.Length == 0)
        {
            Debug.LogWarning("Warning: There are no cameras in the scene");
            _isCamera = false;
        }

        if (!isReversed)
        {
            _percentage = 0+startPercent;
        }
        else
        {
            _percentage = 1-startPercent;
        }

//        Vector3 initalRotation = cameraPath.GetPathRotation(_percentage, false).eulerAngles;
//        rotationX = initalRotation.y;
//        rotationY = initalRotation.x;
    }

    protected virtual void OnEnable()
    {
        cameraPath.eventList.CameraPathEventPoint += OnCustomEvent;
        cameraPath.delayList.CameraPathDelayEvent += OnDelayEvent;
        if (animationObject != null)
            animationObjectCamera = animationObject.GetComponentInChildren<Camera>();
    }

    protected virtual void Start()
    {
        if (playOnStart)
            Play();

        if (Application.isPlaying && orientationTarget == null && (_orientationMode == orientationModes.followTransform || _orientationMode == orientationModes.target))
            Debug.LogWarning("There has not been an orientation target specified in the Animation component of Camera Path.",transform);

//        _storedMousePosition = Input.mousePosition;
    }

    protected virtual void Update()
    {
        if (!isCamera)
        {
            if (_playing)
            {
                UpdateAnimation();
                UpdatePointReached();
                UpdateAnimationTime();
            }
            else
            {
                if (_cameraPath.nextPath != null && _percentage >= 1)
                {
                    PlayNextAnimation();
                }
            }
        }
    }

    protected virtual void LateUpdate()
    {
        if (isCamera)
        {
            if (_playing)
            {
                UpdateAnimation();
                UpdatePointReached();
                UpdateAnimationTime();
            }
            else
            {
                if (_cameraPath.nextPath != null && _percentage >= 1)
                {
                    PlayNextAnimation();
                }
            }
        }
    }

    protected virtual void OnDisable()
    {
        CleanUp();
    }

    protected virtual void OnDestroy()
    {
        CleanUp();
    }

    //PRIVATE METHODS

    protected virtual void PlayNextAnimation()
    {
        if (_cameraPath.nextPath != null)
        {
            CameraPathAnimator _animator = _cameraPath.nextPath.GetComponent<CameraPathAnimator>();
            float nextPercent = _cameraPath.interpolateNextPath ? _percentage % 1 : 0;
            _animator.Seek(nextPercent);
            _animator.Play();
            Stop();
        }
    }

    protected virtual void UpdateAnimation()
    {
        if (animationObject == null)
        {
            Debug.LogError("There is no animation object specified in the Camera Path Animator component. Nothing to animate.\nYou can find this component in the main camera path game object called "+gameObject.name+".");
            Stop();
            return;
        }

        if (!_playing)
            return;

        if(animationMode != animationModes.still)
        {
            if (cameraPath.speedList.listEnabled)
                _pathTime = _cameraPath.pathLength / Mathf.Max(cameraPath.GetPathSpeed(_percentage), minimumCameraSpeed);
            else
                _pathTime = _cameraPath.pathLength / Mathf.Max(_pathSpeed * cameraPath.GetPathEase(_percentage), minimumCameraSpeed);
            
            animationObject.position = cameraPath.GetPathPosition(_percentage);
        }

        if (_orientationMode != orientationModes.none)
            animationObject.rotation = GetAnimatedOrientation(_percentage,false);

        if(isCamera && _cameraPath.fovList.listEnabled)
        {
            if (!animationObjectCamera.orthographic)
                animationObjectCamera.fieldOfView = _cameraPath.GetPathFOV(_percentage);
            else
                animationObjectCamera.orthographicSize = _cameraPath.GetPathOrthographicSize(_percentage);
        }

        CheckEvents();
    }

    protected virtual void UpdatePointReached()
    {
        if(_percentage == _lastPercentage)//no movement
            return;

        if (Mathf.Abs(percentage - _lastPercentage) > 0.999f)
        {
            _lastPercentage = percentage;//probable loop
            return;
        }

        for (int i = 0; i < cameraPath.realNumberOfPoints; i++)
        {
            CameraPathControlPoint point = cameraPath[i];
            bool eventBetweenAnimationDelta = (point.percentage >= _lastPercentage && point.percentage <= percentage) || (point.percentage >= percentage && point.percentage <= _lastPercentage);
            if (eventBetweenAnimationDelta)
            {
                if (AnimationPointReachedEvent != null) AnimationPointReachedEvent();
                if (AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(i);
            }
        }

        _lastPercentage = percentage;
    }

    protected virtual void UpdateAnimationTime()
    {
        UpdateAnimationTime(true);
    }

    protected virtual void UpdateAnimationTime(bool advance)
    {
        if (_orientationMode == orientationModes.followTransform)
            return;

        if(_delayTime > 0)
        {
            _delayTime += -Time.deltaTime;
            return;
        }

        if (advance)
        {
            switch(animationMode)
            {

                case animationModes.once:
                    if(_percentage >= 1)
                    {
                        _playing = false;
                      if(AnimationFinishedEvent != null) AnimationFinishedEvent();
                    }
                    else
                    {
                        _percentage += Time.deltaTime * (1.0f / _pathTime);
                    }
                    break;

                case animationModes.loop:
                    _percentage += Time.deltaTime * (1.0f / _pathTime);
                    if (_percentage >= 1)
                    {
                        _percentage -= 1;
                        _lastPercentage = 0;
                        if(AnimationLoopedEvent != null) AnimationLoopedEvent();
                    }
                    break;

                case animationModes.reverseLoop:
                    _percentage += -Time.deltaTime * (1.0f / _pathTime);
                    if (_percentage <= 0)
                    {
                        _percentage += 1;
                        _lastPercentage = 1;
                        if(AnimationLoopedEvent != null) AnimationLoopedEvent();
                    }
                    break;

                case animationModes.reverse:
                    if(_percentage <= 0.0f)
                    {
                        _percentage = 0.0f;
                        _playing = false;
                        if(AnimationFinishedEvent != null) AnimationFinishedEvent();
                    }
                    else
                    {
                        _percentage += -Time.deltaTime * (1.0f / _pathTime);
                    }
                    break;

                case animationModes.pingPong:
                    float timeStep = Time.deltaTime * (1.0f / _pathTime);
                    _percentage += timeStep * pingPongDirection;
                    if(_percentage >= 1)
                    {
                        _percentage = 1.0f - timeStep;
                        _lastPercentage = 1;
                        pingPongDirection = -1;
                        if(AnimationPingPongEvent != null) AnimationPingPongEvent();
                    }
                    if(_percentage <= 0)
                    {
                        _percentage = timeStep;
                        _lastPercentage = 0;
                        pingPongDirection = 1;
                        if(AnimationPingPongEvent != null) AnimationPingPongEvent();
                    }
                    break;

                case animationModes.still:
                    if(_percentage >= 1)
                    {
                        _playing = false;
                        if (AnimationFinishedEvent != null) AnimationFinishedEvent();
                    }
                    else
                    {
                        _percentage += Time.deltaTime * (1.0f / _pathTime);
                    }
                    break;
            }

            if (smoothOrientationModeChanges)
            {
                if(_orientationModeLerpTimer < orientationModeLerpTime)
                    _orientationModeLerpTimer += Time.deltaTime;
                else
                    _orientationModeLerpTimer = orientationModeLerpTime;
            }
        }
        _percentage = Mathf.Clamp01(_percentage);
    }

    protected virtual Quaternion GetMouseLook()
    {
        if (animationObject == null)
            return Quaternion.identity;

        float screenHalfWidth = Screen.width/2f;
        float screenHalfHeight = Screen.height/2f;

        float xRot = (Input.mousePosition.x - screenHalfWidth) / Screen.width * 180f;
        float yRot = (Screen.height - Input.mousePosition.y - screenHalfHeight) / Screen.height * 180f;
        yRot = Mathf.Clamp(yRot, minX, maxX);
        return Quaternion.Euler(new Vector3(yRot, xRot, 0));
//        rotationX += Input.GetAxis("Mouse X") * sensitivity;
//        rotationY += -Input.GetAxis("Mouse Y") * sensitivity;
//
//        rotationY = Mathf.Clamp(rotationY, minX, maxX);
//
//        Debug.Log(Input.GetAxis("Mouse X") + " " + sensitivity + " " + Input.mousePosition);
//        if(Input.touchCount>0)
//            Debug.Log(Input.touches[0].deltaPosition.x);

//        return Quaternion.Euler(new Vector3(rotationY, rotationX, 0));
    }

    protected virtual void CheckEvents()
    {
        cameraPath.CheckEvents(_percentage);
    }

    protected virtual bool isReversed
    {
        get { return (animationMode == animationModes.reverse || animationMode == animationModes.reverseLoop || pingPongDirection < 0); }
    }

    protected virtual void CleanUp()
    {
        cameraPath.eventList.CameraPathEventPoint += OnCustomEvent;
        cameraPath.delayList.CameraPathDelayEvent += OnDelayEvent;
    }

    protected virtual void OnDelayEvent(float time)
    {
        if(time > 0)
            _delayTime = time;//start delay timer
        else
            Pause();//indeffinite delay
    }

    protected virtual void OnCustomEvent(string eventName)
    {
        if(AnimationCustomEvent != null)
            AnimationCustomEvent(eventName);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Convert this camera path into an xml string for export
    /// </summary>
    /// <returns>A generated XML string</returns>
    public string ToXML()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("<animator>");
        sb.AppendLine("<animationObject>" + ((animationObject != null) ? animationObject.name : "null") + "</animationObject>");
        sb.AppendLine("<orientationTarget>" + ((orientationTarget != null) ? orientationTarget.name : "null") + "</orientationTarget>");
        sb.AppendLine("<animateSceneObjectInEditor>" + _animateSceneObjectInEditor + "</animateSceneObjectInEditor>");
        sb.AppendLine("<playOnStart>" + playOnStart + "</playOnStart>");
        sb.AppendLine("<animationMode>" + animationMode + "</animationMode>");
        sb.AppendLine("<orientationMode>" + _orientationMode + "</orientationMode>");
        sb.AppendLine("<normalised>" + normalised + "</normalised>");
        sb.AppendLine("<pathSpeed>" + _pathSpeed + "</pathSpeed>");

        sb.AppendLine("<smoothOrientationModeChanges>" + smoothOrientationModeChanges + "</smoothOrientationModeChanges>");
        sb.AppendLine("<orientationModeLerpTime>" + orientationModeLerpTime + "</orientationModeLerpTime>");

        sb.AppendLine("<fixedOrientaionx>" + fixedOrientaion.x + "</fixedOrientaionx>");
        sb.AppendLine("<fixedOrientaiony>" + fixedOrientaion.y + "</fixedOrientaiony>");
        sb.AppendLine("<fixedOrientaionz>" + fixedOrientaion.z + "</fixedOrientaionz>");

        sb.AppendLine("<fixedPositionx>" + fixedPosition.x + "</fixedPositionx>");
        sb.AppendLine("<fixedPositiony>" + fixedPosition.y + "</fixedPositiony>");
        sb.AppendLine("<fixedPositionz>" + fixedPosition.z + "</fixedPositionz>");

        sb.AppendLine("<sensitivity>" + sensitivity + "</sensitivity>");
        sb.AppendLine("<minX>" + minX + "</minX>");
        sb.AppendLine("<maxX>" + maxX + "</maxX>");

        sb.AppendLine("</animator>");

        return sb.ToString();
    }

    /// <summary>
    /// Import XML data into this camera path overwriting the current data
    /// </summary>
    /// <param name="XMLPath">An XML file path</param>
    public void FromXML(XmlNode xml)
    {
        if(xml == null)
            return;

        GameObject animationObjectGO = GameObject.Find(xml["animationObject"].FirstChild.Value);
        if(animationObjectGO != null)
            animationObject = animationObjectGO.transform;


        GameObject orientationTargetGO = GameObject.Find(xml["orientationTarget"].FirstChild.Value);
        if (orientationTargetGO != null)
            orientationTarget = orientationTargetGO.transform;

        _animateSceneObjectInEditor = bool.Parse(xml["animateSceneObjectInEditor"].FirstChild.Value);
        playOnStart = bool.Parse(xml["playOnStart"].FirstChild.Value);

        animationMode = (animationModes)Enum.Parse(typeof(animationModes), xml["animationMode"].FirstChild.Value);
        _orientationMode = (orientationModes)Enum.Parse(typeof(orientationModes), xml["orientationMode"].FirstChild.Value);

        normalised = bool.Parse(xml["normalised"].FirstChild.Value);
        _pathSpeed = float.Parse(xml["pathSpeed"].FirstChild.Value);

        if (xml["smoothOrientationModeChanges"] != null)
            smoothOrientationModeChanges = bool.Parse(xml["smoothOrientationModeChanges"].FirstChild.Value);
        if (xml["orientationModeLerpTime"] != null)
            orientationModeLerpTime = float.Parse(xml["orientationModeLerpTime"].FirstChild.Value);


        if(xml["fixedOrientaionx"] != null)
        {
            Vector3 newOrientation = new Vector3();
            newOrientation.x = float.Parse(xml["fixedOrientaionx"].FirstChild.Value);
            newOrientation.y = float.Parse(xml["fixedOrientaiony"].FirstChild.Value);
            newOrientation.z = float.Parse(xml["fixedOrientaionz"].FirstChild.Value);
            fixedOrientaion = newOrientation;
        }

        if(xml["fixedPositionx"] != null)
        {
            Vector3 newPosition = new Vector3();
            newPosition.x = float.Parse(xml["fixedPositionx"].FirstChild.Value);
            newPosition.y = float.Parse(xml["fixedPositiony"].FirstChild.Value);
            newPosition.z = float.Parse(xml["fixedPositionz"].FirstChild.Value);
            fixedPosition = newPosition;
        }


        if (xml["sensitivity"] != null)
            sensitivity = float.Parse(xml["sensitivity"].FirstChild.Value);
        if (xml["minX"] != null)
            minX = float.Parse(xml["minX"].FirstChild.Value);
        if (xml["maxX"] != null)
            maxX = float.Parse(xml["maxX"].FirstChild.Value);
    }
#endif
}
