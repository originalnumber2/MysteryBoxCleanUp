using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{

	//Class is made to handle communication with the mystry box. It provides function for sending and reciving data.
	public class SensorBox
	{
		bool isSenCon, isEmergencyButton, isLubOn, isAbort, isAlarm, isDoorClosed, isLubWanted, isPressure;
		double TraVolt, TraLoc, LatVolt, LatLoc, VerLoc;
		int VerCount;
		TcpClient SenClient;
		NetworkStream SenStream;
        Modbus mod;
        MessageQueue MesQue;
        Mutex SendMutex;

        public SensorBox(Modbus modbus, MessageQueue messageQueue)
		{
			isSenCon = false;

            //Start up the socket for communication with the mystery box
			SenClient = new TcpClient();
            SenClient.Connect(IPAddress.Parse("10.10.6.100"), 23);
            SenStream = SenClient.GetStream();
            mod = modbus;
            MesQue = messageQueue;
            SendMutex = new Mutex();
        }

		// return type should be changed from void
		public void sendCommand(string Message)
		{
			byte[] BytesOut = System.Text.Encoding.ASCII.GetBytes(Message);
			SenStream.Write(BytesOut, 0, BytesOut.Length);
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

        void btnBoxCon_Click(object sender, EventArgs e)
        {
            if (!isSenCon)
            {
                MesQue.WriteMessageQueue("In order to complete connection to sensor box, vertical sensor must pass over reference mark");
                ControlThreadStarter(MysteryBoxConnect, "MysteryBoxConnect");
                boxLub.Visible = true;
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
                    btnSenCon.BackColor = Color.Red;
                    boxLub.Visible = false;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Trying to disconect from Myster Box\n\n" + ex.Message.ToString(), "Mystery Box Disconect Error");
                }
            }
        }

        private void btnLubOn_Click(object sender, EventArgs e)
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
            //Take control of Mutex
            SendMutex.WaitOne();
            SendData += "L1";
            SendMutex.ReleaseMutex();
     
        }
        void LubOff()
        {
            //Take control of Mutex
            SendMutex.WaitOne();
            SendData += "L0";
            SendMutex.ReleaseMutex();
            btnLubOn.BackColor = Color.Red;
            isLubOn = false;
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

    }
}
