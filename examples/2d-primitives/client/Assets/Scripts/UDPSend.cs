using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

public class UDPSend : MonoBehaviour
{
    string IP = "127.0.0.1";  // define in init
    int port =   9001;        // define in init

    // "connection" things
    IPEndPoint m_Remote;
    UdpClient m_Client;

	float m_LastSendTime;
	public float m_SendingRate = 0.03f;

	public ObjectsManager m_ObjectsManager;


    public void Start()
    {
        Init();
		m_LastSendTime = 0.0f;
    }
		
	void FixedUpdate() {

		if (Time.time - m_LastSendTime < m_SendingRate) {
			return;
		}

		string jsonString = buildJSONFromObjects (m_ObjectsManager.GetVisible());
//		Debug.Log (jsonString);

		byte[] bytes = UTF8Encoding.UTF8.GetBytes (jsonString);

		m_Client.Send (bytes, bytes.Length, m_Remote);

		m_LastSendTime = Time.time;
	}

	string buildJSONFromObjects(List<ObjectDescription> objects) {
		List<string> stringObjects = new List<string> ();

		foreach (ObjectDescription o in objects) {
			stringObjects.Add (JsonUtility.ToJson (o));
		}

		return string.Format("{{\"data\":[{0}]}}", string.Join (",", stringObjects.ToArray()));
	}
		
    void Init()
    {
		m_Remote= new IPEndPoint(IPAddress.Parse(IP), port);
		m_Client = new UdpClient();
    }
}
