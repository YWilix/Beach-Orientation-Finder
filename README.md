# Beach Orientation Finder 🏖️

**Beach Orientation Finder** is a ***fast*** and ***light-weight*** unity project **that provides the shoreline orientation for nearly any coast on earth**.

## The output angle convention :

you can think of the given angle of the coast orientation as ***an arrow that points towards the output angle*** ,
the **land** would be on the **left side** of the arrow and the **water** would be on the **right side**.
You can imagine yourself standing and facing the direction of the arrow so the land would be on your left and the water would be on your right.

The output angle is **in degrees** it starts at **0°** pointing towards the **south** and increases going **clockwise** so **west** would be **90°** , **north** would be **180°** and **east** would be **270°**.

For this to be clear you can check the following examples (the yellow color represents the land and the blue one represents the water) :

<img width="2188" height="824" alt="PreviewImg" src="https://github.com/user-attachments/assets/9bd2a3c7-0cb0-4ed8-9931-a45370ea6b31" />


## How to use this project :

In the "MapScene" scene, there is a Gameobject called Point that contains a Monobehaviour script called BeachDirectionCalculator (found in Assets\Scripts\Main\). 
The script contains a function called GetBeachAngle which represents the main function of this tool. <br> <br>
It takes two arguments a latitude and longitude WGS84 coordinates and returns null if the given position is not onshore other than that it returns a float containing the beach orientation in that location. 
This is the function Signature of GetBeachAngle :

```C#
    public float? GetBeachAngle(float latitude, float longitude)
```

So the usage of this tool is really simple , you call the GetBeachAngle function inside of the Point Gameobject giving it the latitude and longitude of the beach you want and it will return the beach orientation at that location : 

```C#
    float? angle = Point.GetComponent<BeachDirectionCalculator>().GetBeachAngle(latitude, longitude);
```

- #### Usage of the project in a unity project :
  the usage of this tool in a unity project is straight forward. You can import the Unity package provided in this repository , load the MapScene Scene with additive scene loading mode. and just call the GetBeachAngle function as mentioned before.

- #### integration of this tool in a non-unity project :
  Until now, I didn't provide a plug-and-use way to integrate this tool inside a non-unity project since the integration method depends a lot on your app's target platform and used technologies. But you can easily find a way to integrate it using for example ***Unity as a Library*** or ***inter process communication*** etc.

## How does it work under the hood :
The entire world map was recreated using 2D colliders in unity in two versions , one **"filled"** map and one **"empty"** map and I put in the unity world a Gameobject called **Point** that has a circle collider and the BeachDirectionCalculator script. <br>

When the **GetBeachAngle** function within that script gets called with some coordinates, The Point moves to the corresponding position in the Unity world. It then Shoots Raycasts in the shape of a circle with a length in kilometers equal to the **CircleRadius variable** to get intersection points with the **"empty" map**. Then, those points get simply used to get some angle ,lets call it beta, by fitting a line to those points and finding beta with some simple math (please refer to the **GetBeachAngle function** to get into details). <br>

But even with that angle we didn't really get the orientation of the beach, That's because the land can be on either sides of the fitted line. So to know which side is the land side and which side is water, the script activates the **"filled" map** (previously disabled) and asks a simple question : to which direction will the Point gameobject move ? <br>

Since the **"filled" map** is physically static and is filled in the inside (the land side is filled), when we calculate the movement of the 2D rigidbody of Point it will always be from land to water so with that we can know the land side and use that info with beta to determine the orientation angle.

To simplify imagining this functionality I included the following picture:

<img width="1678" height="852" alt="ExplainationImg" src="https://github.com/user-attachments/assets/0bbcdb15-c331-45af-b745-5e3b707a5ee5" />

<br><br>
for more details please refer to the ***BeachDirectionCalculator.cs script***. Within it, you can find all the variables that control the way the tool works like the **CircleRadius** and others.

also, the **"filled" map** is called filled because it's made with the **Polygon Collider 2D** component so it's considered filled in the inside. On the other hand, the **"empty" map** is made with the **Edge Collider 2D** component so it's considered empty in the inside. <br>

A question that you might ask is: ***how was the unity world map created ?*** <br>

The answer is simple, I divided the world map in parts, exported every part as a very high resolution image , imported them as sprites , and then used the custom physics shape tool in the sprite editor to make an accurate outline of those parts with the level of detail I needed , then when you add a Polygon collider 2D to the Gameobject having that picture as a sprite it will automatically have that outline as a collider.<br>
so I used those different parts of the world to build the map in Unity.

To make the **"empty" map** I made a script called OutlineColliderCreator.cs that takes the **"filled" map** and generates the empty map with it excluding the intersection parts denoted with the OverlapAreas colliders.

This project is **light-weight** since the large images of the maps exist only in the editor and do not get bundled with the unity app when you build the project. And it's **fast** since it uses the Unity physics engine to do all the calculations which is very optimized so that make all this functionality run very **fast**
