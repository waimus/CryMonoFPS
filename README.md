# CryMonoFPS
CryEngine 5 C# FPS Template.</br>
Video: https://youtu.be/QemlT37_fSM </br>
</br>
A custom FPS control in CryEngine C#. A lot of code are adapted from the C# thirdperson template which then modified to be an FPS game. Then more features are added by little research about using the C# API. </br>
</br>
### Features: </br>
- WASD key movement (based on the TPS template)</br>
- Mouse camera move rotation (based on the TPS templated, modeified to be an FPS camera)</br>
- Jump and sprint</br>
- Xbox and Playstation is assigned in the /Assets/libs/config/defaultprofile.xml but has NOT been tested with native controller input.</br>
- Hybrid shooting mechanic, uses raycasting under 20 meters, but bullet will act as projectile if aim target is more than 20 meters away. (Shooting is also based from the TPS template with a modification to use both raycasting and projectile under certain conditions).</br>

### Some Notes:</br>
- This project was created on CryEngine 5.6.6
- There are two solutions files. The one in the root folder is the game logic project solution, contains any gameplay mechanics implementation. The one located at Code/Game.sln is the program solution. Came with the CryEngine blank template, contains the game structure as a running program. So you may want to use the one at the root folder to modify the game logic.
- Input mapping uses the /Assets/libs/config/defaultprofile.xml file and then controlled in the Player.cs
- This is a pretty basic FPS controller but hopefully can help people to get started with C# in CryEngine
