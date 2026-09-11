using UnityEngine;

public class PathDetector : MonoBehaviour
{
    [ContextMenuItem("Detect Shapes" , "SearchForShapes")]
    public int ShapeNumberOfPoints;
    [ContextMenuItem("Create Shape" , "CreateShapeAtTransfrom")]
    public Transform ShapeCreationPosition;

    public void SearchForShapes()
    {
        var Collider = GetComponent<PolygonCollider2D>();

        for (int i = 0; i < Collider.pathCount; i++)
        {
            if (Collider.GetPath(i).Length == ShapeNumberOfPoints)
                Debug.Log($"DETECTED Shape Number {i}");
        }
    }
    
    public void CreateShapeAtTransfrom()
    {
        var Collider = GetComponent<PolygonCollider2D>();

        var pos = (ShapeCreationPosition.position - transform.position) / transform.lossyScale.x;

        Collider.pathCount++;

        Collider.SetPath(Collider.pathCount - 1, new Vector2[3] { pos + new Vector3(-0.125f,0) , pos + new Vector3(0.125f, 0), pos + new Vector3(0, 0.125f) });
    }

}
