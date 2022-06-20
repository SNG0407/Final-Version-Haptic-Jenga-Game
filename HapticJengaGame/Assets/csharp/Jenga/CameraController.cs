using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    private Tower refTower;
    private Vector3 previousPosition;
    private Vector3 refPosition;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        refTower = GameObject.Find("Tower").GetComponent<Tower>();
        refPosition = transform.position;
    }

    void Update()
    {
        //if(Input.GetMouseButtonDown(0))
        //{
        //    previousPosition = cam.ScreenToViewportPoint(Input.mousePosition); 
        //}

        //if(Input.GetMouseButton(0))
        //{
        //    Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

        //    cam.transform.position = new Vector3();

        //    cam.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180);
        //    cam.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World);
        //    cam.transform.Translate(refPosition);

        //    previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        //}
    }
}
