
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;



public class ObjectControl : MonoBehaviour
{


    void InvokePrint()
    {
        Debug.Log("data's sent!");
    }

    void JengaClicked()
    {
        //get a difference of angles between user and Jenga
        IsClicked = true;
        IsServoWorking = true;
        IsMPUWorking = false;
        SendState(IsServoWorking, IsMPUWorking, MoveServoMotor(angle));

        //release the jenga

        IsServoWorking = true;
        IsMPUWorking = false;
    }
    
 
    void JengaReleased()
    {
        IsClicked = false;
        IsServoWorking = false;
        IsMPUWorking = true;
        SendState(IsServoWorking, IsMPUWorking, StopServoMotor());

        //UseMPU();
    }

    string StartServoMotor()
    {
        Debug.Log("Start Servo Motor");
        return "777";

    }
    string StopServoMotor() //, float fTime, float fFrequency, float fAmplitude
    {
        Debug.Log("Stop Servo Motor");
        return "999";

    }

    float getAngle()
    {
        //get a difference of angles between user and Jenga
        return 90; //default
    }

    string MoveServoMotor(int angle)
    {

        string strNum = angle.ToString();
        return strNum;

    }
    void SendState(bool IsServoWorking, bool IsMPUWorking, string output)
    {

        if (serial.IsOpen == false)
        {
            serial.Open();
            //serial.Close();
        }

        string strIsServoWorking;
        string strIsMPUWorking;
        //string strIsClicked = IsClicked.ToString();
        if (IsServoWorking == true)
        {
            strIsServoWorking = "1";
        }
        else
        {
            strIsServoWorking = "0";
        }

        if (IsMPUWorking == true)
        {
            strIsMPUWorking = "1";
        }
        else
        {
            strIsMPUWorking = "0";
        }


        string data = strIsServoWorking + "#" + strIsMPUWorking + "#" + output + "&";


        //stop the Servo Motor
        serial.Write(data);
        Debug.Log("SendState");
        Debug.Log(data);

        serial.Close();
    }

    public SerialPort serial = new SerialPort("COM5", 9600); //here change port - where you have connected arduino to computer


    private Rigidbody rb;
    //public GameObject mpu = GameObject.Find("serial"); //.FindObjectsWithTag("MPU"); 
    // public GameObject mpu1 = GameObject.Find("SerialReader"); //.FindObjectsWithTag("MPU"); 
    private string receivedString;

    private int angle = 0; //Actual current angle of Jenga
    private bool IsClicked = false;
    private bool IsMPUWorking = false;
    private bool IsServoWorking = false;
    bool firstTimeReading = true;
    public Vector3 offset;
    public Vector3 error;
    Vector3 lastInput;

    public bool moveServo = false;

    void Start()
    {
        //serial.Open();
        //serial.Close();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 EulerCamera = Camera.main.transform.rotation.eulerAngles;
        Vector3 EulerObejct = gameObject.transform.rotation.eulerAngles;

        float angle2 = EulerCamera.y + EulerObejct.y;

        //Debug.Log(angle2);

        float ToArduinoAngle = 90 - angle2;
        if (ToArduinoAngle >= 180) ToArduinoAngle = 180;
        if (ToArduinoAngle <=0) ToArduinoAngle = 0;

        //Debug.Log("Camera         :" + EulerCamera.y);
        //Debug.Log("Object         :" + EulerObejct.y);
        if (moveServo == true)
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
            }
           
            /*
            Debug.Log("--------Angle--------");

            Debug.Log("Camera         :" + EulerCamera.y);
            Debug.Log("Object         :" + EulerObejct.y);
            Debug.Log("angle2         :" + angle2);
            Debug.Log("ToArduinoAngle :"+ToArduinoAngle);
            */
            string strAngle = MoveServoMotor((int)ToArduinoAngle);
            string data = "1#0#" + strAngle + "&";
            serial.Write(data);
            //SendState(IsServoWorking, IsMPUWorking, MoveServoMotor((int)angle2));;
            Debug.Log("MoveServo!!");
            moveServo = false;
        }
        if (Input.GetKeyDown(KeyCode.Q) == true) //test
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
            }

            serial.Write("1#0#0&");
            Debug.Log("1#0#0&");
            //serial.Close();
        }
        if (Input.GetKeyDown(KeyCode.W) == true) //test
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
            }
            serial.Write("1#0#180&");
            Debug.Log("1#0#180&");
        }
        serial.Close();
        /*
        if (Input.GetKeyDown(KeyCode.LeftArrow) == true) //start the Servo Motor
        {
            angle = angle + 10;
            if (angle >= 180) angle = 180;
            Debug.Log(angle);
            MoveServoMotor(angle);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) == true) //stop the Servo Motor
        {
            angle = angle - 10;
            if (angle <= 0) angle = 0;
            Debug.Log(angle);
            MoveServoMotor(angle);
        }

        if (Input.GetKeyDown(KeyCode.A) == true) //start the Servo Motor
        {

            //mpu1.gameObject.GetComponent<GyroReader>().enabled = false;
            //mpu.gameObject.GetComponent<SerialController>().enabled = false;

            JengaClicked();

            //mpu1.gameObject.GetComponent<GyroReader>().enabled = true;
            //mpu.gameObject.GetComponent<SerialController>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.S) == true) //start the Servo Motor
        {
            //    mpu1.gameObject.GetComponent<GyroReader>().enabled = true;
            //    mpu.gameObject.GetComponent<SerialController>().enabled = true;
            JengaReleased();
        }
        if (Input.GetKeyDown(KeyCode.Q) == true) //test
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
            }

            //stop the Servo Motor
            //string strNum = angle.ToString();
            //string data = strNum + "&";
            //string data = strNum;
            serial.Write("1#0#0&");
            //Debug.Log("Test");
            Debug.Log("1#0#0&");
            //serial.Close();
        }
        if (Input.GetKeyDown(KeyCode.W) == true) //test
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
            }

            //stop the Servo Motor
            //string strNum = angle.ToString();
            //string data = strNum + "&";
            //string data = strNum;
            serial.Write("1#0#180&");
            //Debug.Log("Test");
            Debug.Log("1#0#180&");
            //serial.Close();
        }
        */
    }

}


