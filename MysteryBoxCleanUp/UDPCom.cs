using System;
using System.Net;
using System.Net.Sockets;

namespace MysteryBoxCleanUp
{

    public class UDPCom
    //Class that deals with communicating with the Simulink Target. Passing Sensor data and motor commands
    {

        public bool isSimControl, isSendUDP, isUDPWorking;
        UdpClient SimulinkReviceUDP;//recieves from 12000
        UdpClient SimulinkSendUDP;//sends to 12010 //rapid info sent in this channel, like position forces etc
        IPEndPoint IPSendtoSimulink, IPRecivefromSimulink, IPSendtoSimulinkCommands;
        public int SoftStop;

        // inheriting motors
        SpindleMotor spi;
        LateralMotor lat;
        TransverseMotor trans;
        VerticalMotor vert;

        //enabling messaging queues.
        MessageQueue mesQue;

        public UDPCom(SpindleMotor spindle, LateralMotor lateral, TransverseMotor transverse, VerticalMotor vertical, MessageQueue messagequeue)
        {
            isSimControl = false;
            isSendUDP = false;
            isUDPWorking = true;
            SimulinkReviceUDP = new UdpClient(12005);
            SimulinkSendUDP = new UdpClient(12006);
            SoftStop = 0;

            //IPSendtoSimulink = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12010); //For use with simulink Desktop real time
            IPSendtoSimulink = new IPEndPoint(IPAddress.Parse("192.168.1.102"), 12012);//For use with simulink real time xpc target
            //IPRecivefromSimulink = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12000);//For use with simulink Desktop real time
            IPRecivefromSimulink = new IPEndPoint(IPAddress.Parse("192.168.1.102"), 12000);//For use with simulink real time xpc target
            SimulinkSendUDP.Connect(IPSendtoSimulink);
            SimulinkReviceUDP.Connect(IPRecivefromSimulink);

            spi = spindle;
            lat = lateral;
            trans = transverse;
            vert = vertical;

            mesQue = messagequeue;
        }

        public void SimulinkReciveLoop()
        {
            //need to add code to set motor parameters to be snapper in simulink control but less jumpy normally
            bool isSimDone = false;
            isSimControl = true;
            Byte[] RecieveBytes = new Byte[256];
            double tempvar;
           
            StartUDP();
            WriteMessageQueue("waiting for communication from simulink..");
            Thread.Sleep(500);
            while (SimulinkReviceUDP.Available > 0)//clear udp buffer, want to recieve latest packet make sure simulink is running
            {
                RecieveBytes = SimulinkReviceUDP.Receive(ref IPRecivefromSimulink);
            }
            RecieveBytes = SimulinkReviceUDP.Receive(ref IPRecivefromSimulink);
            WriteMessageQueue("Giving control to simulink");
            RecieveBytes = SimulinkReviceUDP.Receive(ref IPRecivefromSimulink);
            VerSpeed[0] = BitConverter.ToDouble(RecieveBytes, 0);
            SpiSpeed[0] = BitConverter.ToDouble(RecieveBytes, 8);
            tempvar = BitConverter.ToDouble(RecieveBytes, 16);

            #region Initialize Traverse Lateral Motor for Analog Control
            AnalogControl();
            #endregion
            #region Initialize Spindle Speed
            isSpiSpeedCW = trueifpositive(SpiSpeed[0]);
            if (isSpiSpeedCW)
            {
                StartSpiCW();
                isSpiCW = true;
            }
            else
            {
                StartSpiCCW();
                isSpiCW = false;
            }
            #endregion
            VerSpeed[0] = -99.9;
            SpiSpeed[0] = -99.9;
            AnalogControl();
            while (!isAbort && !isSimDone && !isAlarm)
            {
                //Try to recieve data
                if (SimulinkReviceUDP.Available > 0)
                {
                    RecieveBytes = SimulinkReviceUDP.Receive(ref IPRecivefromSimulink);
                }
                //if no data has been sent assume welding computer has crashed
                else
                {
                    WriteMessageQueue("Data Not recieved from the simulink real time computer");
                    //Break out of while loop then have rest of thread stop motors
                    break;
                }
                RecieveBytes = SimulinkReviceUDP.Receive(ref IPRecivefromSimulink);
                VerSpeed[1] = VerSpeed[0];
                VerSpeed[0] = BitConverter.ToDouble(RecieveBytes, 0);
                SpiSpeed[1] = SpiSpeed[0];
                SpiSpeed[0] = BitConverter.ToDouble(RecieveBytes, 8);
                tempvar = BitConverter.ToDouble(RecieveBytes, 16);
                isSimDone = System.Convert.ToBoolean(tempvar);
                #region Update Vertical Speed
                if (VerSpeed[0] != VerSpeed[1])
                {
                    isVerDown = trueifpositive(VerSpeed[0]);
                    VerSpeedMagnitude = Math.Abs(VerSpeed[0]);
                    if (VerSpeedMagnitude > VerSpeedLimit)
                        VerSpeedMagnitude = VerSpeedLimit;
                    if (VerSpeedMagnitude < VerSpeedMinimum)
                        VerSpeedMagnitude = VerSpeedMinimum;
                    VerMessage = "H";
                    if (isVerDown)
                        VerMessage += "+";
                    else
                        VerMessage += "-";

                    VerMessage += "A" + VerAccel.ToString("F2") + " V" + VerSpeedMagnitude.ToString("F5") + " G\r";
                    VerPort.Write(VerMessage);
                }
                #endregion
                #region Update Spindle Speed
                if (SpiSpeed[0] != SpiSpeed[1])
                {
                    isSpiSpeedCW = trueifpositive(SpiSpeed[0]);
                    SpiSpeedMagnitude = Math.Abs(SpiSpeed[0]);
                    if (SpiSpeedMagnitude > 100)
                        isLubWanted = true;
                    else
                        isLubWanted = false;
                    //Limit Spindle speed
                    if (SpiSpeedMagnitude > SpiSpeedLimit) SpiSpeedMagnitude = SpiSpeedLimit;
                    if (isSpiSpeedCW)
                    {
                        if (!isSpiCW)
                        {
                            StartSpiCW();
                            isSpiCW = true;
                        }
                        SpiMessage = "CW";
                    }
                    else
                    {
                        if (isSpiCW)
                        {
                            StartSpiCCW();
                            isSpiCW = false;
                        }
                        SpiMessage = "CCW";
                    }
                    ChangeSpiRef(SpiSpeedMagnitude);
                    WriteMessageQueue("Spi set to:" + SpiSpeedMagnitude.ToString("F0") + SpiMessage);

                }
                #endregion
                //clear udp buffer, want to recieve latest packet
                while (SimulinkReviceUDP.Available > 0)
                {
                    RecieveBytes = SimulinkReviceUDP.Receive(ref IPRecivefromSimulink);
                }
                Thread.Sleep(25);
            }
            StopAllMotors();
            isLubWanted = false;
            btnZzero_Click(new object(), new EventArgs());
            Program.Safetyfrm.UpdateLimitsSafe();
            WriteModbusQueue(3, 0x0300, 04, false);//give lateral motor master frequency control to rs-485
            WriteModbusQueue(3, 0x0301, 03, false);//give lateral motor Source of operation command  to rs-485
            WriteModbusQueue(3, 0x0705, 0, false);//set lateral speed to zero
            WriteModbusQueue(3, 0x010D, 0, false);//set Lateral motor direcction (Fwd/Rev) to be controled by rs-485

            WriteModbusQueue(2, 0x0300, 04, false);//give Traverse motor master frequency control to rs-485
            WriteModbusQueue(2, 0x0301, 03, false);//give Traverse motor Source of operation command  to rs-485
            WriteModbusQueue(2, 0x0705, 0, false);//set Traverse speed to zero
            WriteModbusQueue(2, 0x010D, 0, false);//set Traverse motor direcction (Fwd/Rev) to be controled by rs-485
            SoftStop = 1;
            Thread.Sleep(25);
            isSimControl = false;
            MatlabExecute("SaveData");//run script to save data
            WriteMessageQueue("SimulinkRecieve Completed");
            try { ControlSemaphore.Release(1); }
            catch (System.Threading.SemaphoreFullException ex)
            {
                MessageBox.Show("error releasing ControlSemaphore 3 " + ex.Message.ToString());
            }


        }
        private void StartUDP()
        {
            isSendUDP = false;
            SimulinkSendUDP.Close();
            Thread.Sleep(100);
            SimulinkSendUDP = new UdpClient(12006);
            SimulinkSendUDP.Connect(IPSendtoSimulink);
            Thread.Sleep(100);
            isSendUDP = true;

        }
        private void AnalogControl()
        {
            Thread.Sleep(500);
            WriteModbusQueue(2, 0x0300, 02, false);//set source of master frequency to be from analog input for Traverse motor
            WriteModbusQueue(2, 0x010D, 15, false);//set Traverse motor direcction (Fwd/Rev) to be controled digital input terminal DI5
            WriteModbusQueue(2, 0x0706, 0x02, false);//set Traverse motor to run, but not to set dirrection
                                                     //30.02 0.0 minimum reverence value 0 to 10 Volts
                                                     //30.02 10.0 maximum reverence value 0 to 10 Volts
                                                     //30.03 00 Invert Reverence signal, not inverted
                                                     //30.07 00 potentiometer offset 0.0-100.0, 0 offset
                                                     //30.10 00 potentiometer Direction, 00 do not have voltage value control direction

            Thread.Sleep(500);
            WriteModbusQueue(3, 0x0300, 02, false);//set source of master frequency to be from analog input for lateral motor
            WriteModbusQueue(3, 0x010D, 15, false);//set lateral motor direcction (Fwd/Rev) to be controled digital input terminal DI5
            WriteModbusQueue(3, 0x0706, 0x02, false);//set lateral motor to run, but not to set dirrection
            Thread.Sleep(500);
        }
        private void simulinkControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SoftStop = 1;
            ControlThreadStarter(SimulinkReciveLoop, "Simulink Recieve");
        }
        private void sendUdpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartUDP();
        }
    }
}
