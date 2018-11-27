using System;
using System.IO.Ports;
using System.Threading;

namespace MysteryBoxCleanUp
{
    public class VerticalMotor
    {

        //bools dealing with the state of connection of the motor communication
        public bool isVerCon;
        public bool isVerContinous;
        public bool isSimulinkControl;

        bool isSetVerWeld; //not sure what this variable is used for

        //serial port for communicating with the vertical motor
        SerialPort VerPort;

        //motor state variables
        int VerCount;
        int verTurns;
        public double VerSpeed; //two iteration history of the vertical motor speed.

        public double VerAccel;
        public bool VerDir;

        //safty limits for the speed
        double VerSpeedMax, VerSpeedMin; 
        private double epsilon;

        public VerticalMotor()
        {

            //need to do something about the modbus UDP problem and bring together
            isVerContinous = true;
            isVerCon = false;
            isSetVerWeld = false; //I dont know what this is used for
            isSimulinkControl = false; // Determine if simulink control is on Might want to bubble this up to a higher class
            VerCount = 0; //Number of rotations to preform
            verTurns = 0;
            epsilon = 0.001;

            #region Speed Control of the Verticle motor
            VerSpeed = -99.9;
            VerSpeedMax = 5;
            VerAccel = 10;
            VerSpeedMin = 0.00001;//from motor guide
            VerDir = true; // true is the vertical motor down
            # endregion


            #region Setting up the serial communication port
            VerPort = new SerialPort();
            VerPort.BaudRate = 9600;
            VerPort.PortName = "COM5";
            VerPort.DataBits = 8;
            VerPort.Parity = System.IO.Ports.Parity.None;
            VerPort.StopBits = System.IO.Ports.StopBits.One;
            VerPort.Open();
            #endregion


        }

        //movement needs to be cleaned up and improved to account for UDP and insure consistency though methods.
        internal void Move(double IPM, bool dir)
        {
            if (ChangeDir(dir) || ChangeIPM(IPM))
            {
                if (VerDir) { LowerTableModbus(); }
                else RaiseTableModbus();
            }
        }

        private bool ChangeIPM(double IPM)
        {
            //allow for checking of maximum speeds and insure IPM is positive
            double CheckIPM = Math.Abs(IPM);
            if (CheckIPM > VerSpeedMax)
            {
                CheckIPM = VerSpeedMax;
            }
            else
            {
                if (CheckIPM < VerSpeedMin)
                {
                    CheckIPM = VerSpeedMin;
                }

            }
            if (Math.Abs(CheckIPM - VerSpeed) > epsilon)
            {
                VerSpeed = CheckIPM;
                return true;
            }
            return false;

        }

        private bool ChangeDir(bool dir)
        {
            if (dir = VerDir)
            {
                return false;
            }
            VerDir = dir;
            return true;
        }

        //Setter for the Vertical Speed
        internal string SetSpeed(double speed)
        {
            double VerSpeedMagnitude = Math.Abs(speed);
            if (VerSpeedMagnitude > VerSpeedMax)
                VerSpeedMagnitude = VerSpeedMax;
            if (VerSpeedMagnitude < VerSpeedMin)
                VerSpeedMagnitude = VerSpeedMin;
            VerSpeed = VerSpeedMagnitude;
            return "Setting Vertical Speed to " + VerSpeedMagnitude.ToString();
        }


        void RaiseTableModbus()
        {
            String VerMessage;
            VerMessage = String.Empty;

            if (isVerContinous)
            {
                VerMessage = "E MC H";
                VerMessage += "+";

                VerMessage += "A" + VerAccel + " V" + VerSpeed.ToString() + " G\r";
            }
            else
            {
                VerMessage = "E MN A" + VerAccel + " V" + VerSpeed.ToString() + " D";
                VerMessage += verTurns.ToString() + " G\r";
            }

            VerPort.Write(VerMessage);
        }

        void LowerTableModbus()
        {
            String VerMessage;
            VerMessage = String.Empty;

            if (isVerContinous)
            {
                VerMessage = "E MC H";
                VerMessage += "-";
                VerMessage += "A" + VerAccel + " V" + VerSpeed.ToString() + " G\r";
            }
            else
            {
                VerMessage = "E MN A" + VerAccel + " V" + VerSpeed.ToString() + " D";
                VerMessage += "-";
                VerMessage += verTurns.ToString() + " G\r";
            }

            VerPort.Write(VerMessage);
        }

        internal string VerticalConnectToggle()
        {
            if (!isVerCon)//connect to vertical motor
            {
                return VerConnect();
            }
                return VerDisconnect();
        }

        void VerticalStop() // Used with the stop verticle button I dont know what the E does
        {
            VerPort.Write("E S\r");
        }

        internal void StopVer() //Stop the vertical Motor
        {
            VerPort.Write("S\r");
        }

        //Connect to the Vertical Motor
        internal string VerConnect()
        {
            string VerMessage, responce;
            //Clear out port
            VerPort.ReadExisting(); //this was edited out

            //Query the motor and establish RS-232 control
            VerPort.Write("E ON 1R\r"); //this was edited out
            Thread.Sleep(300); //this was edited out

            //Turn off limits
            VerPort.Write("1LD3\r"); //this was edited out
            Thread.Sleep(30); //this was edited out

            //Initialize Now
            VerPort.Write("1E 1MN 1A10 1V10 1D0 G\r"); //this was edited out
            Thread.Sleep(30); //this was edited out

            //Clear out port and place in holding string
            VerMessage = VerPort.ReadExisting(); //this was edited out

            if (VerMessage.Length < 2) //this was edited out
            {
                responce = "Connection to vertical motor failed";
            }
            else //this was edited out
            {
                //Update the boolean
                isVerCon = true;
                responce = "Connected to vertical motor";

            }
            return responce;
        }

        //Disconect and turn off the Vertical Motor
        internal string VerDisconnect()
        {
            //Turn off the motor for starters
            VerPort.Write("S\r");
            Thread.Sleep(300);
            VerPort.Write("OFF\r");
            isVerCon = false;
            //show that vertical motor is disconected on gui
            return "Disconnected from vertical motor";
        }

        internal string BeginWeldControl()
        {
            VerPort.Write("E MC"); //I dont know why this is needed;
            VerSpeedMax = 5;
            VerAccel = 10;
            VerSpeedMin = 0.00001;//from motor guide
            return "Vertical Motor ready for weld";
        }

        internal string UDPVerticalMove(double speed)
        {
            string VerMessage;
            double VerSpeedMagnitude = Math.Abs(speed);

            if (Math.Abs(VerSpeed - speed) > epsilon)
            {
                VerDir = trueifpositive(VerSpeed);
                VerSpeedMagnitude = Math.Abs(VerSpeed);
                SetSpeed(speed);
                VerMessage = "H";
                if (VerDir)
                    VerMessage += "+";
                else
                    VerMessage += "-";

                VerMessage += "A" + VerAccel.ToString("F2") + " V" + VerSpeedMagnitude.ToString("F5") + " G\r";
                VerPort.Write(VerMessage);
                return VerMessage;
            }
            return "";
        }

        bool trueifpositive(double n)
        {
            if (n >= 0)
                return true;
                return false;
        }


    }
}


