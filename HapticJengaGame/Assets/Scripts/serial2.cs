using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class serial2 : MonoBehaviour
{

    SerialPort stream = new SerialPort("COM10", 115200); //Set the port (com4) and the baud rate (9600, is standard on most devices)
    //9600 19200 115200

    void Start()
    {
        stream.Open(); //Open the Serial Stream.
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void OnGUI()
    {
        string newString = "Connected: " + transform.rotation.x + ", " + transform.rotation.y + ", " + transform.rotation.z;
        GUI.Label(new Rect(10, 10, 300, 100), newString); //Display new values
        // Though, it seems that it outputs the value in percentage O-o I don't know why.
    }
    public string ReadSerialMessage()
    {
        string value ="";
        if (stream.IsOpen == false)
        {
            stream.Open();
            //value = stream.ReadLine(); //Read the information //ReadLine (stream.ReadByte()).ToString()
            //stream.Close();

        }
        //stream.Open();
        value = stream.ReadLine(); //Read the information //ReadLine (stream.ReadByte()).ToString()

        return value;
        //stream.Close();
    }
}