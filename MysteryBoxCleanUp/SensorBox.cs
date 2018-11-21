using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{

	//Class is made to handle communication with the mystry box. It provides function for sending and reciving data.
	public class SensorBox
	{
        bool isSenCon;
        bool isEmergencyButton;
        bool isLubOn;
        //bool isAbort; //i think we can do away with this
        //bool isAlarm; //i think we can do away with this
        bool isDoorClosed;
        bool isPressure;
        //double TraVolt; // i think this can be a functional variable
        double TraLoc; 
        //double LatVolt; // I think this can be a functional variable
        double LatLoc;
        double VerLoc;
		//int VerCount; // I think this can be a functional variable
		TcpClient SenClient;
		NetworkStream SenStream;
        Mutex SendMutex;

        public SensorBox()
		{
			isSenCon = false;
            isLubWanted = false;
            isPressure = false;
            isEmergencyButton = false;
            isLubOn = false;
            //isAbort = false;
            //isAlarm = false;
            isDoorClosed = true;

        //Start up the socket for communication with the mystery box
        SenClient = new TcpClient();
            SenClient.Connect(IPAddress.Parse("10.10.6.100"), 23);
            SenStream = SenClient.GetStream();
            SendMutex = new Mutex();
        }

		// return type should be changed from void
		public void sendCommand(string Message)
		{
			byte[] BytesOut = System.Text.Encoding.ASCII.GetBytes(Message);
			SenStream.Write(BytesOut, 0, BytesOut.Length);
		}


        void Connect()
        {
            if (!isSenCon)
            {
                MesQue.WriteMessageQueue("In order to complete connection to sensor box, vertical sensor must pass over reference mark");
                ControlThreadStarter(MysteryBoxConnect, "MysteryBoxConnect"); //Starts the Mysterybox connect as its own thread
            }
            else
            {
                try
                {
                    isSenCon = false;
                    Thread.Sleep(500);
                    SenClient.Close();
                    SenClient = new TcpClient();
                    //Update Bools
                    isSenCon = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Trying to disconect from Myster Box\n\n" + ex.Message.ToString(), "Mystery Box Disconect Error");
                }
            }
        }

        private void LubeToggle(object sender, EventArgs e)
        {
            if (isSenCon)
            {
                if (!isLubOn)
                {
                    LubOn();
                }
                else
                {
                    LubOff();
                }
            }
            else
            {
                MesQue.WriteMessageQueue("Need to be connected to Sensor Box");
            }
        }

        void LubOn()
        {
            //might need to make send data a bit differnt Lead with an A and trail with an X.
            //Take control of Mutex
            string SendData = "L1";
            byte[] BytesOut = Encoding.ASCII.GetBytes(SendData);
            SenStream.Write(BytesOut, 0, BytesOut.Length);

        }

        void LubOff()
        {
            //might need to make send data a bit differnt Lead with an A and trail with an X.
            //Take control of Mutex
            string SendData = "L0";
            byte[] BytesOut = Encoding.ASCII.GetBytes(SendData);
            SenStream.Write(BytesOut, 0, BytesOut.Length);
        }

        void MysteryBoxConnect()
        {
            int ByteCount;
            String RecieveData;
            try
            {
                SenClient = new TcpClient();
                SenClient.Connect(IPAddress.Parse("10.10.6.100"), 23);
                SenStream = SenClient.GetStream();

                SendData = "A";
                SendData += "X";

                //Clear BytesOut
                //Translate the passed message
                BytesOut = Encoding.ASCII.GetBytes(SendData);
                //Send the message to the sensor client

                SenStream.Write(BytesOut, 0, BytesOut.Length);
                //Machine must cross over the reference mark on the vertical motor to start
                //the connection
                //Read the incoming data
                ByteCount = SenStream.Read(BytesIn, 0, BytesIn.Length);
                RecieveData = Encoding.ASCII.GetString(BytesIn, 0, ByteCount);
                isSenCon = true;
                btnSenCon.BackColor = Color.Green;
                StopVer();
            }
            catch (Exception ex)
            {
                StopVer();
                MessageBox.Show(ex.Message.ToString(), "Mystery Box Connect Error");
            }
            WriteMessageQueue("MysteryBoxConnect Completed");
            try { ControlSemaphore.Release(1); }
            catch (System.Threading.SemaphoreFullException ex)
            {
                MessageBox.Show("error releasing ControlSemaphore 4 " + ex.Message.ToString());
            }
        }

        public void ControlThreadStarter(Action MethodName, string name)
        {
            if (!isAlarm)
            {
                if (!isAbort)
                {
                    if (ControlSemaphore.WaitOne(0))
                    {
                        WriteMessageQueue(name + " started");
                        MachineControlThread = new Thread(new ThreadStart(MethodName));
                        MachineControlThread.Start();
                    }
                    else
                    {
                        WriteMessageQueue("Thread already working");
                    }
                }
                else
                {
                    WriteMessageQueue("Turn off Abort");
                }
            }
            else
            {
                WriteMessageQueue("THE ALARM IS ON TURN OFF THE ALARM FIRST");
            }
        }

        private void ReceiveData()
        {
            string SendData = "";
            int ByteCount = 0;
            int TermIndex;
            String RecieveData;
            Byte[] BytesOut = new Byte[256];
            Byte[] BytesIn = new Byte[256];
            if (isSenCon)
            {
                try
                {
                    //Apend end code to Send Data
                    SendData += "X";

                    //Clear BytesOut
                    //Translate the passed message
                    BytesOut = Encoding.ASCII.GetBytes(SendData);

                    //Clear out Send Data
                    SendData = "A";

                    //Send the message to the sensor client

                    SenStream.Write(BytesOut, 0, BytesOut.Length);

                    //Read the incoming data
                    ByteCount = SenStream.Read(BytesIn, 0, BytesIn.Length);
                    RecieveData = Encoding.ASCII.GetString(BytesIn, 0, ByteCount);
                    //Parse and process the incoming data

                    //Get and check the vertical
                    TermIndex = RecieveData.IndexOf("X", StringComparison.Ordinal);

                    VerLoc = VerticalCount2Loc(int.Parse(RecieveData.Substring(22, TermIndex - 22)));

                    //Get and check the traverse
                    TraLoc = TransverseVolt2Loc(double.Parse(RecieveData.Substring(6, 7)));

                    //Get and check the lateral 
                    LatLoc = LateralVolt2Loc(double.Parse(RecieveData.Substring(14, 7)));//Recalibrated by BG, 2/8/13, for use with 4X loop on sensor

                    //Check the pressure, door, and E-stop button
                    //isDoorClosed = (int.Parse(RecieveData.Substring(0, 1)) == 1);
                    isDoorClosed = true;
                    isPressure = (int.Parse(RecieveData.Substring(2, 1)) == 1);
                    isEmergencyButton = (int.Parse(RecieveData.Substring(4, 1)) == 1);
                }
                catch (ArgumentNullException ex)
                {
                    MessageBox.Show("Error Receiving Data From Mystery Box, I would try to disconecct and power cycle the Mystery Box and connect again. -J\n\n" + ex.Message.ToString(), "Error Receiving Data From Mystery Box");
                }
            }
        }

        public double TransverseVolt2Loc(double TrasverseVolt)
        {
            return (4.29 * TrasverseVolt) - 2.317;
        }

        public double LateralVolt2Loc(double LateralVolt)
        {
            return (1.075945 * LateralVolt) - 7.1; //Recalibrated by BG, 2/8/13, for use with 4X loop on sensor
        }

        public double VerticalCount2Loc(double VerticalCount)
        {
            return (VerticalCount * -.000393700787) + 2.79

        }
    }
}
