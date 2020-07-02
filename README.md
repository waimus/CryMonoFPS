# CryMonoFPS
CryEngine 5 C# FPS Template.
</br>
A custom FPS control in CryEngine C#. A lot of code are adapted from the C# thirdperson template which then modified to be an FPS game. Then more features are added by little research about using the C# API. </br>
</br>
Features: </br>
- WASD key movement (based on the TPS template)</br>
- Mouse camera move rotation (based on the TPS templated, modeified to be an FPS camera)</br>
- Jump and sprint</br>
- Xbox and Playstation is assigned in the /Assets/libs/config/defaultprofile.xml but has NOT been tested with native controller input.</br>
- Hybrid shooting mechanic, uses raycasting under 15 meters, but bullet will act as projectile if aim target is more than 15 meters away. (Shooting is also based from the TPS template with a modification to use both raycasting and projectile under certain conditions).</br>
