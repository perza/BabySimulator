using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraViewManager : MonoBehaviour
{
    public bool isMovable = true;
    public int maxHeight = 30;
    public int minHeight = 5;
    Camera m_MainCamera;
    Vector3 move = new Vector3();
    Vector3 zoom = new Vector3();
    // Start is called before the first frame update
    void Awake()
    {
        m_MainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMovable) return;

        int horizontal = (int)Input.GetAxisRaw("Horizontal");
        int vertical = (int)Input.GetAxisRaw("Vertical");
        float depth = Input.GetAxis("Mouse ScrollWheel");


        move.x = horizontal;
        move.z = vertical;
        float currentDepth = m_MainCamera.transform.position.y;
        zoom.z = currentDepth - depth > 5 && currentDepth - depth < 30 ? depth : 0;

        m_MainCamera.transform.Translate(move, Space.World);
        m_MainCamera.transform.Translate(zoom);

    }
}
