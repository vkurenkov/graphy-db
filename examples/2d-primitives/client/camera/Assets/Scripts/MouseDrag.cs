using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag: MonoBehaviour {

	bool m_Dragging = false;
	float m_LastX = 0.0f;
	float m_LastY = 0.0f;

	public float m_UnitsPerPixel = 0.1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			m_Dragging = true;

			m_LastX = Input.mousePosition.x;
			m_LastY = Input.mousePosition.y;
		} else if (Input.GetMouseButtonUp (0)) {
			m_Dragging = false;
		}

		if (m_Dragging) {
			float dx = Input.mousePosition.x - m_LastX;
			float dy = Input.mousePosition.y - m_LastY;

			this.transform.position = this.transform.position - new Vector3 (dx, dy, 0) * m_UnitsPerPixel;

			m_LastX = Input.mousePosition.x;
			m_LastY = Input.mousePosition.y;
		}
	} 
}
