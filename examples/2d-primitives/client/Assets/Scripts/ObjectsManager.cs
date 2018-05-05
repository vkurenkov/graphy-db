using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsManager : MonoBehaviour {

	List<GameObject> m_Objects;

	// Use this for initialization
	void Start () {
		m_Objects = new List<GameObject> (transform.childCount);

		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).gameObject.activeSelf) {
				m_Objects.Add (transform.GetChild (i).gameObject);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public List<ObjectDescription> GetVisible() {
		List<ObjectDescription> visible = new List<ObjectDescription> ();

		for (int i = 0; i < transform.childCount; i++) {
			GameObject child = transform.GetChild (i).gameObject;
			if (child.activeSelf && child.GetComponent<SpriteRenderer> ().isVisible) {
				visible.Add(new ObjectDescription(child.transform.position.x, child.transform.position.y));
			}
		}

		return visible;
	}
}
