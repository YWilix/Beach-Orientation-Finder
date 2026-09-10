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
The script contains a function called GetBeachAngle which represents the main function of this tool. It takes two arguments a latitude and longitude WGS84 coordinates and returns null if the given position is not onshore other than that it returns a float containing the beach orientation in that location. 
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

- #### integration of this tool in a non unity project :
  Until now, I didn't provide a straightforward way to integrate this tool inside a non unity project since the integration method depends a lot on your app's target platform and used technologies. But you can easily find a way to integrate it using for example ***Unity as a Library*** or ***inter process communication***

## How does it work under the hood :
