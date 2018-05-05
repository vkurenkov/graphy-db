using System.Collections.Generic;
using UnityEngine;

public class ObjectsManager : MonoBehaviour
{
    public List<PrimitiveObject> GetVisible()
    {
        List<PrimitiveObject> visible = new List<PrimitiveObject>();

        foreach(Transform child in transform)
        {
            if (child.gameObject.activeSelf && child.GetComponent<SpriteRenderer>().isVisible)
            {
                visible.Add(child.GetComponent<PrimitiveObject>());
            }
        }

        return visible;
    }
    private void Start()
    {
        m_objects = new List<GameObject>(transform.childCount);

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                m_objects.Add(transform.GetChild(i).gameObject);
            }
        }
    }

    private List<GameObject> m_objects;

}
