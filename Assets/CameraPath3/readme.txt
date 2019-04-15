------------------------------------------------------
Camera Path Animator
------------------------------------------------------
Thanks for purchasing Camera Path Animator from the Unity Asset Store (https://www.assetstore.unity3d.com/#/content/617).
Create, edit and preview camera animations within the editor and avoid having to compile the scene to see every change.
PLEASE BACKUP PROJECTS BEFORE UPDATING.
Don't forget to leave us a review! :o) Hopefully a nice one...
NOTE: The Camera Path Animator Asset Store license is for a single user at a time. If multiple people use this, you need multiple licenses!

------------------------------------------------------
GETTING STARTED:
------------------------------------------------------
Loads of information can be found at http://support.jasperstocker.com/camera-path-animator/
Remember to watch our lovingly created videos http://support.jasperstocker.com/camera-path-animator/videos/
Check out our demos online http://support.jasperstocker.com/camera-path-animator/demos/

------------------------------------------------------
INSTALLATION:
------------------------------------------------------
To install Camera Path Animator simply import all files from the unitypackage.
Note that Camera Path version 2.0 and 3.0 are incompatible.
Also note that version 3.1x is not compatible with 3.0x

------------------------------------------------------
QUICKSTART:
------------------------------------------------------
Once you have installed Camera Path you should be able to see the Camera Path 3 directory in your project view.
To start creating your new animation path, click on GameObject > Create New Camera Path
This will place a new animation path in the scene with the animator and path components attached.

------------------------------------------------------
BASIC OVERVIEW:
------------------------------------------------------
There are two components in a Camera Path object. 
The Main Camera Path which allows you to create and edit the path.
The Camera Path Animator which controls how the path behaves in your game.

In the Camera Path component we have the following modes
Path Points - these define where the path goes.
Control Points - only used in Bezier curves, these control the shape of the curves.
FOV - is used to control the Field of View of the Camera during the animation.
Speed - is used to control the speed of the animation over the curve.
Delay - you can add points to delay the animation by specific time periods.
Ease Curves - Control the ease of the animation speed at specific stop points on the animation.
Events - add events to the path that can either call functions in other objects, or broadcast events.
Orientations - Control the orientation of the camera through the path, only used in the Custom animation mode.
Tilts - control the camera tilt through the path used in follow path animation modes
Options

------------------------------------------------------
FAQ:
------------------------------------------------------
Where are the examples?
	They are located in a UnityPackage called ExamplesII_U5.unitypackage at the root of the Camera Path folder.
How can I set up Camera Path for 2D?
	The Camera Path Animator component has a 2D orientation mode that you can switch to. Then you can just create a spline in front of your 2D scene as use it as you would a 3D scene.
How do I set up Occulus/VR content?
	Camera Path is capable of animating any GameObject so just create a base VR object that deals with the cameras and animate that with Camera Path.
I have a very long path that has become slightly jerky in movement. How can I stop this?
	Under the options tab there is a check box to "Automatically Calculate Stored Point Resolution". Uncheck that and maintain a smaller value then the current one. Keep reducing it until you get the smoothness you're looking for.	
	
------------------------------------------------------
CONTACT:
------------------------------------------------------
For anything releated to Camera Path Animator, don't hesitate to contact me at
email@jasperstocker.com
Include: Invoice Number, Unity Version - you will get a resolution quicker if I don't have to ask for these.
Ideally send me as much information as possible. Screenshots, videos, example scenes, what happened, how it happened.
You can also send me your path by exporting it to XML. Go to Options tab and export it to XML.

Thanks!
Jasper