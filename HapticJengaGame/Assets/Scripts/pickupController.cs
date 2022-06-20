using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class pickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] Transform holdArea;
    private GameObject heldObj;
    private Rigidbody heldObjRB;

    [Header("Phusics Parameters")]
    [SerializeField] private float pickupRange = 15.0f;
    [SerializeField] private float pickupForce = 150.0f;
    public bool CheckClick = false;
    private bool stop = false;


    Vector3 dir;
    ContactPoint cp;

    private void Update()
    {
        //if (CheckClick == false)
        //{
        //    heldObj.GetComponent<HapticMesh>().enabled = true;
        //    heldObjRB.transform.parent = null;
        //    heldObj = null;
        //    stop = false;
        //    //GameObject.Find("SerialReader").GetComponent<GyroV2>().jenga = null;
        //}
        CheckClick = GameObject.Find("SerialReader").GetComponent<GyroV2>().ButtonClicked;


        if (CheckClick == false && (heldObj != null))
        {
            //OnCollisionStay(Collision collision);
            //Drop
            DropObject();
            Debug.Log("DropObject");
        }
        else
        {
            if (CheckClick == true && heldObj != null)
            {
                ///Move
                MoveObejct();
                Debug.Log("MoveObejct");

            }
        }
        //Debug.Log(CheckClick);
        //if (Input.GetMouseButtonDown(0)|| CheckClick == true)
        //{
        //    Debug.Log("GetMouseButtonDown");
        //    if (heldObj == null)
        //    {
        //        RaycastHit hit;
        //        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange))
        //        {
        //            Debug.DrawRay(transform.position, transform.forward * pickupRange, Color.red);
        //            //Pickup
        //            PickupObject(hit.transform.gameObject);
        //            Debug.Log("PickupObject");
        //        }
        //    }
        //    else
        //    {
        //        //Drop
        //        DropObject();
        //        Debug.Log("DropObject");
        //    }
        //}
        //if (heldObj != null)
        //{
        //    ///Move
        //    MoveObejct();
        //    Debug.Log("MoveObejct");

        //}
    }
    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log(collision.gameObject.name + " : Ãæµ¹ Áß");
        //cp = collision.GetContact(0);
        //dir = gameObject.transform.position - cp.point;
        if(collision.gameObject.GetComponent<Block>() != null)
        {
            collision.gameObject.GetComponent<Block>().MakeObjectGlow();
            collision.gameObject.GetComponent<Block>().ArrowGuideOn();
        }
        if (CheckClick == true && stop == false)
        {
            //Debug.Log("GetMouseButtonDown");
            if (heldObj == null)
            {
                dir = collision.gameObject.transform.position - transform.position;
                //Pickup
                PickupObject(collision.transform.gameObject);
                Debug.Log("PickupObject");
            }
            //    else
            //    {
            //        //Drop
            //        DropObject();
            //        Debug.Log("DropObject");
            //    }
            //}
            //if (heldObj != null)
            //{
            //    ///Move
            //    MoveObejct();
            //    Debug.Log("MoveObejct");

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        try
        {
            if (collision.gameObject.GetComponent<Block>() != null)
            {
                collision.gameObject.GetComponent<Block>().MakeObjectToOriginalState();
                collision.gameObject.GetComponent<Block>().ArrowGuideOff();
            }
            //heldObj.GetComponent<HapticMesh>().enabled = true;
            
            //GameObject.Find("SerialReader").GetComponent<GyroV2>().jenga = null;
        }
        catch (NullReferenceException)
        {
            Debug.Log("NullReferenceException");
        }
        catch (Exception)
        {
            //Debug.Log("Exception");
        }

    }


    void MoveObejct()
    {
        //heldObjRB.AddForce(dir * pickupForce);
        //if (Vector3.Distance(heldObj.transform.position - dir, holdArea.position) > 0.1f)
        if (Vector3.Distance(heldObj.transform.position - dir, holdArea.position) > 0.1f)
        {
            //Vector3 moveDirection = (holdArea.position - heldObj.transform.position);
            //heldObjRB.AddForce(dir * pickupForce);
            heldObj.transform.position = gameObject.transform.position + dir;
        }
    }

    void PickupObject(GameObject pickObj)
    {
        stop = true;
        if (pickObj.GetComponent<Rigidbody>())
        {
            //GameObject.Find("SerialReader").GetComponent<GyroV2>().enabled = true;
            

            //gameObject.transform.position = pickObj.GetComponent<Renderer>().bounds.center;
            heldObjRB = pickObj.GetComponent<Rigidbody>();
            heldObjRB.useGravity = false;
            heldObjRB.drag = 10;
            heldObjRB.constraints = RigidbodyConstraints.FreezeRotation;

            heldObjRB.transform.parent = holdArea;
            pickObj.GetComponent<Block>().IsSelected = true;
            heldObj = pickObj;
            heldObj.transform.position = gameObject.transform.position;//GetComponent<Renderer>().bounds.center;
            
            heldObj.GetComponent<HapticMesh>().enabled = false;
            heldObj.GetComponent<ObjectControl>().enabled = true;
            heldObj.GetComponent<ObjectControl>().moveServo = true;

            //GameObject.Find("SerialReader").GetComponent<GyroV2>().jenga = heldObj.GetComponent<JengaController>();
            //GameObject.Find("SerialReader").GetComponent<GyroV2>().firstTimeReading = true;

            StartCoroutine(DelayPickUpObject(heldObj));

        }
    }
    void DropObject()
    {
        heldObjRB.useGravity = true;
        heldObjRB.drag = 1;
        heldObjRB.constraints = RigidbodyConstraints.None;
        heldObj.GetComponent<ObjectControl>().enabled = false;
        heldObj.GetComponent<Block>().IsSelected = false;
        GameObject.Find("SerialReader").GetComponent<GyroV2>().firstTimeReading = false;

        heldObj.GetComponent<HapticMesh>().enabled = true;
        heldObjRB.transform.parent = null;
        heldObj = null;
        stop = false;
        //StartCoroutine(delay(heldObj, heldObjRB)) ;
        GameObject.Find("SerialReader").GetComponent<GyroV2>().jenga = null;
    }

    private IEnumerator DelayPickUpObject(GameObject heldObj)
    {
        var wait = new WaitForSeconds(2f);
        yield return wait;
        
        try
        {
            GameObject.Find("SerialReader").GetComponent<GyroV2>().jenga = heldObj.GetComponent<JengaController>();
            GameObject.Find("SerialReader").GetComponent<GyroV2>().firstTimeReading = true;
            heldObj.GetComponent<ObjectControl>().moveServo = false;
        }
        catch (Exception)
        {

        }
        
    }
}
