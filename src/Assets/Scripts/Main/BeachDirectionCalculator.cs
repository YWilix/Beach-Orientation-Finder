using System.Collections.Generic;
using UnityEngine;
using System;


public class BeachDirectionCalculator : MonoBehaviour
{

    public double MapScaleMultiplier = 1E-05; 
    // 1E-05 is just a chosen value that I build all the map according to
    // you can change this but you will have to scale and change all the map accordingly


    [Space(10)]
    private Vector2 RefPointRealMap = new Vector2(1.619014050484291f, 7.397322120067738f);
    // The vector components are aranged as (latitude,longitude)
    // This is the real WGS84 map coordinates of the ref point on the globe

    private Vector2 Pos;
    [Space(20)]
    [ContextMenuItem("Get Beach Direction", "TestScript")]
    [ContextMenuItem("Move Point", "SetupPointFromCoords")]
    public string CoordStr;

    [Space(10)]
    public Transform RefPoint; 
    //the refrence point Gameobject's Transform in Unity
    //when we move the point to a specific coordinate on the map it's places in the Unity's map relative to the ref point
    
    [Space(10)]
    public GameObject FilledMap;
    // The filled map parent Gameobject

    [Space(10)]
    public string OutlineCollsTag;
    public string FilledCollsTag;


    private const int RaysNumber = 60; // The Number of Rays going outwards to get points of intersection

    /// <summary>
    /// The radius in kilometers of the circle to intersect with the map to calculate the beach direction 
    /// </summary>
    private const float CircleRadius = 20;

    /// <summary>
    /// the intersection-rays length according to the position of the point (get set by the SetupPoint method)
    /// <para>
    /// it's used to know if the point is on shore or not and gets multiplied by the RayLenInterMultiplier for shore intersections 
    /// </para>
    /// </summary>
    private float RaysLength = 1f;
    
    /// <summary>
    /// The maximum multiplication happening when searching for the smallest raylength for intersection
    /// </summary>
    private float MaxRayMultiplier = 1f;

    private float RayLengthSearchStep = 0.2f;
    // the step with which the circle of rays gets bigger and bigger until finding enough intersection points

    private const int EnoughPointsNumber = 5;
    //the number of points that feels enough to stop the search for intersection points

    private const double LeftLimit = -169.4139607838994;

    /// <summary>
    /// the current latitude of the point (used just to display the right gizmos with the scaling factor)
    /// </summary>
    private float CurrentPointLatitude = 0f;

    /// <summary>
    /// The rays length multiplier , it gets inceased gradually (so the rays gets longer with it) until enough intersection points are found
    /// mult = 0.25f means that the rays are 25% the length of the chosen circle radius
    /// </summary>
    private float mult = 1;


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////// Main Functions /////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void Start() 
    {
        Physics.simulationMode = SimulationMode.Script;
        Physics2D.simulationMode = SimulationMode2D.Script;
    }

    private RaycastHit2D? GetFirstHitWithTag(RaycastHit2D[] Hits , string Tag)
    {
        foreach (var Hit in Hits)
            if(Hit.collider.tag == Tag) return Hit;

        return null;
    }

    private Collider2D GetFirstColliderWithTag(Collider2D[] Colls, string Tag)
    {
        foreach (var Coll in Colls)
            if (Coll.tag == Tag) return Coll;

        return null;
    }

    /// <summary>
    /// checks if all the points in a list are alligned vertically (have the same x coordinate)
    /// </summary>
    /// <param name="points">the points list</param>
    private bool AreAllAlignedVertically(List<Vector2> points)
    {
        float x = points[0].x;

        foreach (var pt in points)
        {
            if (pt.x != x)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Tests if the point is near the beach or not (if it's close up to 5km from the beach we consider it on shore)
    /// </summary>
    /// <returns>returns true if the point is on shore and false if it's not</returns>
    private bool IsOnShore()
    {
        for (int i = 0; i < RaysNumber; i++)
        {
            Vector2 Dir = new Vector2(Mathf.Cos(((Mathf.PI * 2) / RaysNumber) * i), Mathf.Sin(((Mathf.PI * 2) / RaysNumber) * i));

            var hits = Physics2D.RaycastAll(transform.position, Dir, RaysLength);

            if (GetFirstHitWithTag(hits,OutlineCollsTag) != null) return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the beach orientation at a given WGS84 latitde and longitude
    /// </summary>
    /// <returns>returns the beach orientation or null if the position is not onshore</returns>
    public float? GetBeachAngle(float latitude, float longitude)
    {
        var coll = GetComponent<CircleCollider2D>();
        Rigidbody2D rb = coll.attachedRigidbody;

        Pos = new Vector2(latitude, longitude);// setting the position where the point should move
        SetupPoint();

        mult = MaxRayMultiplier; 
        // setting the multiplier to 1 so if the point is not onshore and the func return null
        // then the rays gizmos will draw as if we searched the entire radius without finding nothing

        if (!IsOnShore()) return null;
        // if it's not near the beach we return null to tell the function caller that the given coordinates are wrong (not near the beach)

        List<Vector2> ContactPts = new List<Vector2>(); // the contact points of the position point with the map collider

        // Searching for the smallest possible radius for intesection for enought data
        for (mult = RayLengthSearchStep; mult <= MaxRayMultiplier ; mult += RayLengthSearchStep)
        {
            ContactPts = new List<Vector2>();
            
            for (int i = 0; i < RaysNumber; i++) // Filling the contact points list
            {
                Vector2 Dir = new Vector2(Mathf.Cos(((Mathf.PI * 2) / RaysNumber) * i), Mathf.Sin(((Mathf.PI * 2) / RaysNumber) * i));

                var hits = Physics2D.RaycastAll(transform.position, Dir, RaysLength * mult);

                var outlinehit = GetFirstHitWithTag(hits, OutlineCollsTag);

                if (outlinehit != null) ContactPts.Add(outlinehit.Value.point);
            }

            if (ContactPts.Count >= EnoughPointsNumber) // we collected enough points
                break;
        }

        mult = Math.Min(mult, MaxRayMultiplier); // if the multiplier got out of the limit

        //the algorithm to know the side of the beach
        //it produces an TowardsBeachAngle float variable that can be used later to change the beach angle according to the beach side
        transform.position = ContactPts[0]; // we move the collider to the edge of the collider because it doesnt work otherwise 
        transform.localScale *= mult; // we set the collider size according to the multiplier of the intersection

        if (rb != null)
        {
            rb.simulated = false; // This "detaches" it from the physics world
            rb.simulated = true;  // This "re-inserts" it as a brand new object
        }
        Physics2D.SyncTransforms();

        List<Collider2D> results = new List<Collider2D>();
        coll.Overlap(results);
        Collider2D filledcoll = GetFirstColliderWithTag(results.ToArray(), FilledCollsTag); // getting the filled coll from all the colls
        ColliderDistance2D distance = coll.Distance(filledcoll);//getting the collision's movement (that should happen) direction 

        Vector2 BeachNormal = -distance.normal; // a vector that points from the land to the sea normally to the coastal orientation

        float ToSeaAngle = Atan2AngleToNormalAngle(Mathf.Atan2(BeachNormal.y, BeachNormal.x) * Mathf.Rad2Deg);


        
        
        float angle = 90; // the angle of the beach orientation we are searching
        //By convention the beach is on the right of this angle : for a 0° angle the sand is on the right

        float OppositeAngle = -90; // the angle of the opposite vector to the vector created by angle's direction

        if (!AreAllAlignedVertically(ContactPts))
        {
            float slope = Covariance(ContactPts) / XVariance(ContactPts);
            angle = Mathf.Atan2(slope, 1) * Mathf.Rad2Deg;
            OppositeAngle = Mathf.Atan2(-slope, -1) * Mathf.Rad2Deg;
        }

        angle = Atan2AngleToNormalAngle(angle);
        OppositeAngle = Atan2AngleToNormalAngle(OppositeAngle);

        if (ToSeaAngle >= OppositeAngle && ToSeaAngle <= angle) // then the Beach and sea positions are inversed
            return OppositeAngle; // we return return the opposite angle to make sure the Beach-On-Right convention is true

        return angle;
    }

    /// <summary>
    /// Moves the point to the selected position (Pos) and sets up the rays length and the colliders size according to the position
    /// </summary>
    public void SetupPoint()
    {
        CurrentPointLatitude = Pos.x;

        float OneKmInUnity = (float)MapScaleMultiplier * 1000;

        RaysLength =  OneKmInUnity * CircleRadius * Sec(Pos.x);

        var RefPointConverted = CoordinateConverter.Wgs84ToWebMercator(RefPointRealMap.x, RefPointRealMap.y);
        var PosConverted = CoordinateConverter.Wgs84ToWebMercator(Pos.x, Pos.y);

        double x = PosConverted.x - RefPointConverted.x;
        double y = PosConverted.y - RefPointConverted.y;

        transform.position = new Vector2(RefPoint.position.x + (float)(x * MapScaleMultiplier), RefPoint.position.y + (float)(y * MapScaleMultiplier));

        transform.localScale = Sec(CurrentPointLatitude) * new Vector3(1, 1, 1); //scaling the colliders with the web mercator stretch factor

        if (Pos.y >= LeftLimit) return; // no position correction is needed

        // the API's map's most left and most right points (at 180 and -180 longitude) aren't like the same as the unity's ones 
        // so it's like the unity's map is offset by some distance relative to the API map on the longitude axis
        // that's why this logic is here to treat that offset for some portion of positions that need to be treated
        // these positions are positions that have a longitude less than the LeftLimit constant variable
        // see LeftLimit and RightLimit Gameobjects in Unity to understand where those limits and problems are
        // the value of the LeftLimit Variable is chosen according to the last left land in the Unity's map
        // that means that all left land on the Unity's map will get it's coordinate convertion without this correction
        // this correction is meant only for the wrong point's coordinates that come after the last left land in Unity
        // those points will be transformed accordingly to the right portion of the unity's map
        // the correction algorithm :

        double UnityMaxLeftPoint = RefPoint.position.x + (CoordinateConverter.Wgs84ToWebMercator(0, -180).x - RefPointConverted.x) * MapScaleMultiplier;

        double UnityMaxRightPoint = RefPoint.position.x + (CoordinateConverter.Wgs84ToWebMercator(0, 180).x - RefPointConverted.x) * MapScaleMultiplier;

        double Xoffset = transform.position.x - UnityMaxLeftPoint;

        transform.position = new Vector2((float)(UnityMaxRightPoint + Xoffset) , transform.position.y);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////// Math Functions /////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Returns the Sec of the angle
    /// </summary>
    /// <param name="Angle">the angle in degrees</param>
    /// <returns></returns>
    private static float Sec(float Angle)
    {
        return 1 / Mathf.Cos(Angle * Mathf.Deg2Rad);
    }

    private float Covariance(List<Vector2> pts)
    {
        float Xmean = 0;
        float Ymean = 0;

        for (int i = 0; i < pts.Count; i++)
        {
            Xmean += pts[i].x;
            Ymean += pts[i].y;
        }

        Xmean /= pts.Count;
        Ymean /= pts.Count;

        float cov = 0;

        for (int i = 0; i < pts.Count; i++)
        {
            cov += (pts[i].x - Xmean) * (pts[i].y - Ymean);
        }

        cov /= pts.Count;

        return cov;
    }

    private float XVariance(List<Vector2> pts)
    {
        float Xmean = 0;

        for (int i = 0; i < pts.Count; i++)
            Xmean += pts[i].x;

        Xmean /= pts.Count;

        float Variance = 0;

        for (int i = 0; i < pts.Count; i++)
        {
            Variance += Mathf.Pow((pts[i].x - Xmean), 2);
        }

        Variance /= pts.Count;

        return Variance;
    }

    private float Atan2AngleToNormalAngle(float angle)
    {
        angle += 180;

        if (0 <= angle && angle < 90)
            angle += 270;
        else // then the angle is between 90 and 360 (including 90 and 360)
            angle -= 90;

        return angle == 0 ? angle : 360f - angle;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////// Dev Functions //////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void TestScript()
    {
        var Coords = CoordinateConverter.TryParseCoordinates(CoordStr).Value;
        Debug.Log($"BEACH ORIENTATION : {GetBeachAngle(Coords.Lat, Coords.Long)}");
    }

    public void SetupPointFromCoords()
    {
        var Coords = CoordinateConverter.TryParseCoordinates(CoordStr).Value;

        Pos.x = Coords.Lat;
        Pos.y = Coords.Long;

        SetupPoint();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < RaysNumber; i++)
        {
            Vector2 Dir = new Vector2(Mathf.Cos(((Mathf.PI * 2) / RaysNumber) * i), Mathf.Sin(((Mathf.PI * 2) / RaysNumber) * i));

            Gizmos.DrawLine(transform.position, transform.position + new Vector3(Dir.x, Dir.y) * RaysLength * mult);
        }
    }
}
