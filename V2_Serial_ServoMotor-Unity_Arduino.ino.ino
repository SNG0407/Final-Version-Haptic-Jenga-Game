#include <Servo.h>    //서보모터 라이브러리 include

Servo microServo;    //서보모터 객체 선언
const int servoPin = 3;    //서보모터 제어핀 할당
 int angle;
 //Seperators
 char *IsServoWorking ="";
 char *IsMPUWorking ="";
 char *WhatToDo ="";
 
 int iServoWorking;
 int iMPUWorking;
 int iWhatToDo;
  //state
  bool bServoWorking;
  bool bMPUWorking;

void ServoWorking(int input){
  if(input==1)
  {
    Serial.println("Servo start");
    microServo.attach(servoPin);
  }
  else if(input==0)
  {
    Serial.println("Servo stop");
    microServo.detach();
  }
  else //0~180
  {
    int angle = input;
    if(angle>=0 && angle<=180)
    {
      Serial.print("Move Servo : "); Serial.println(angle);
      
    microServo.attach(servoPin);
    microServo.write(angle);
    delay(100);
    microServo.detach();
    bServoWorking = false;
    }
  }
}
void MPUWorking(){
  Serial.println("MPUWorking");
}



  
void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  angle =0 ;    //각도 변수 선언
  //input=0;
  bServoWorking = false;
  bMPUWorking = false;
}

void loop() {
  // put your main code here, to run repeatedly:
  char inputUnity[32];
  String input;

  if(bServoWorking == true)
  {
    ServoWorking(iWhatToDo);
    //Serial.println("ServoWorking");
  }
  if(bMPUWorking == true)
  {
    MPUWorking();
    //Serial.println("MPUWorking");
  }
  
   if(Serial.available())
   {
      input=Serial.readStringUntil('&');     //readStringUntil('\n');
    
      input.toCharArray(inputUnity,32);
     
     //ex) const char inputUnity[] = "True#False#stop&";
     //radio.write(&inputUnity, sizeof(inputUnity));
     //Serial.println(inputUnity);
     //third num 999: stop, 777:start, 
     char*d = strtok(inputUnity,"#");
  int count =0;

  //seperating values
  while (d!= NULL){
    if(count==0)
    {
      IsServoWorking=d;
    }
    else if (count==1)
    {
      IsMPUWorking=d;
    }
    else if(count==2)
    {
      WhatToDo=d;
    }
    count++;
    //Serial.println(d);
    d=strtok(NULL,"#");
  }
    count=0;          //True#False#stop& True#True#True&
    iServoWorking = atoi(IsServoWorking);
    iMPUWorking = atoi(IsMPUWorking);
    iWhatToDo = atoi(WhatToDo);
    
    if(iServoWorking == 1){
      bServoWorking = true;
      Serial.print("IsServoWorking : "); Serial.println(IsServoWorking);
    }else {
      bServoWorking = false;
      Serial.print("IsServoWorking : "); Serial.println("false");
      }
      
    if(iMPUWorking == 1){ //char* to string
      bMPUWorking = true;
      Serial.print("IsMPUWorking : "); Serial.println(IsMPUWorking);
    }else {
      bMPUWorking = false;
      Serial.print("IsMPUWorking : "); Serial.println("false");
      }
      
   }
  
}
