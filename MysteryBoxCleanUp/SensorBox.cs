using System;
using System.Net;
using System.Net.Sockets;

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

		public SensorBox()
		{
			isSenCon = false;

            //Start up the socket for communication with the mystery box
			SenClient = new TcpClient();
            SenClient.Connect(IPAddress.Parse("10.10.6.100"), 23);
            SenStream = SenClient.GetStream();
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

    }
}
