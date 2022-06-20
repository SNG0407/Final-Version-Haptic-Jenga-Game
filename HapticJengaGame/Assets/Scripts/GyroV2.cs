
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GyroV2 : MonoBehaviour
{
    //public SerialController serialController;
    public serial2 serial2;
    public JengaController jenga;
    public bool ButtonClicked =false;

    public bool firstTimeReading = true;
    public Vector3 offset;
    public Vector3 error;
    Vector3 lastInput;
    Vector3 FinallInput;

    // Start is called before the first frame update
    void Awake()
    {
    }
    // Update is called once per frame
    void Update()
    {
        ReadMessage();
        if (Input.GetKeyDown(KeyCode.R))
        {
            firstTimeReading = true;
        }
    }
    void ReadMessage()
    {
        //string message = serialController.ReadSerialMessage();
        string message = serial2.ReadSerialMessage();
        if (message == null)
            return;
        try
        {
            if (message[0] == '#')
                DecryptMessage(message);
            else
                Debug.Log("System message : " + message);
        }
        catch (Exception){

        }
            
    }
    void DecryptMessage(string message)
    {

        string[] s = message.Substring(1).Split('/');
        //Vector3 inputVector = new Vector3(-float.Parse(s[2]), float.Parse(s[0]), float.Parse(s[1]));
        Vector3 inputVector = new Vector3(0, float.Parse(s[0]), 0);
        

        if (s[1] == "1") ButtonClicked=true; //it was s[3] before I sent only Y data
        else if (s[1] == "0") ButtonClicked = false;
        //Debug.Log(ButtonClicked);

        //if (jenga.gameObject)
        //{
        //    if (firstTimeReading)
        //    {
        //        Vector3 currentRotation = new Vector3(jenga.transform.rotation.x, jenga.transform.rotation.y, jenga.transform.rotation.z);
        //        Debug.Log("firstTimeReading : " + inputVector);
        //        lastInput = inputVector;

        //        FinallInput = inputVector - currentRotation;
        //        //jenga.FeedVector(jenga.gameObject.transform.rotation);
        //        //jenga.FeedVector(Vector3.zero);
        //        firstTimeReading = false;


        //    }
        //    else jenga.FeedVector(FinallInput);
        //}
        if (firstTimeReading)
        {
            Vector3 currentRotation = new Vector3(jenga.transform.rotation.eulerAngles.x, jenga.transform.rotation.eulerAngles.y, jenga.transform.rotation.eulerAngles.z);
            offset = inputVector - currentRotation;
            lastInput = inputVector;
            firstTimeReading = false;
            error = Vector3.zero;

            FinallInput = inputVector - currentRotation;
        }
        else
        {
            if (Mathf.Abs(inputVector.y - lastInput.y) <= 0.1f)
                error.y += inputVector.y - lastInput.y;

            //Debug.Log("inputVector : " + inputVector);
            //Debug.Log("offset : " + offset);
            jenga.FeedVector(inputVector - offset - error); //inputVector-inputVector-0 //inputVector2 - inputVector-  (inputVector.y - lastInput.y)
            lastInput = inputVector;
        }

    }
}

