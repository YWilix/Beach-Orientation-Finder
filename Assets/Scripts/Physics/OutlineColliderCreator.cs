using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

public class OutlineColliderCreator : MonoBehaviour
{
    [ContextMenuItem("Create Outlines" , "CreateOutlineColliders")]
    [ContextMenuItem("Delete Outlines", "DeleteOutlineColliders")]
    public Transform OutlineParent;
    public GameObject OutlineColliderPrefab;

    public string OverlapTag;
    private bool RaycastsContainTag(RaycastHit2D[] casts,  string tag)
    {
        foreach (var cast in casts)
            if (cast.collider.tag == tag) 
                return true;

        return false;
    }

    public void CreateOutlineColliders()
    {
        var Colliders = GetComponentsInChildren<PolygonCollider2D>();

        foreach (var coll in Colliders)
        {
            var scale = coll.transform.localScale;

            for (int i = 0; i < coll.pathCount; i++)
            {
                var Path = new List<Vector2>(coll.GetPath(i));
                Path.Add(Path[0]);

                var NewPathList = new List<Vector2>();


                for (int j = 0; j < Path.Count - 1; j++)
                {
                    NewPathList.Clear();

                    var rayhits = Physics2D.RaycastAll(
                        new Vector2(coll.transform.position.x, coll.transform.position.y) + Path[j] * scale,
                        (Path[j + 1] - Path[j]).normalized,
                        Vector2.Distance(Path[j + 1] * scale, Path[j] * scale)
                        );

                    while (j < Path.Count - 2 && !RaycastsContainTag(rayhits , OverlapTag))
                    {
                        NewPathList.Add(Path[j]);
                        j++;

                        rayhits = Physics2D.RaycastAll(
                            new Vector2(coll.transform.position.x , coll.transform.position.y) + Path[j] * scale,
                            (Path[j + 1] - Path[j]).normalized,
                            Vector2.Distance(Path[j + 1] * scale, Path[j] * scale)
                            );
                    }
                    
                    if(NewPathList.Count != 0)
                    {
                        NewPathList.Add(Path[j]);
                        
                        if(!RaycastsContainTag(rayhits, OverlapTag)) 
                            NewPathList.Add(Path[j+1]);

                        GameObject NewColl = Instantiate<GameObject>(OutlineColliderPrefab, coll.transform.position, Quaternion.identity, OutlineParent);

                        NewColl.transform.localScale = scale;

                        NewColl.GetComponent<EdgeCollider2D>().points = NewPathList.ToArray();
                    }

                }
            }
        }
    }

    public void DeleteOutlineColliders()
    {
        foreach (var child in OutlineParent.GetComponentsInChildren<Transform>())
            if (child != OutlineParent) 
                DestroyImmediate(child.gameObject);
    }
}
