using System;

namespace MysteryBoxCleanUp
{
    // Simplificatin and lubrication control is needed. if the spindle is going to fast the lube is required.
    public class SpindleMotor
    {

        MotorController controller;

        public bool isSpiConnected;
        public bool isSimulinkControl;
        public double SpiRPM;
        public bool SpiDir; //Spindle direction true - Clockwise false - counter clockwise
        public double RPMmin;
        public double RPMmax;
        private double epsilon;

        public SpindleMotor(MotorController motorController)
        {
            isSpiConnected = false;
            isSimulinkControl = false;
            SpiRPM = 0;
            SpiDir = true;
            RPMmax = 2000;
            RPMmin = 0;

            controller = motorController;

        }

        internal void ConnectionToggle()
        {
            if (isSpiConnected)
                Disconnect();
            else
                Connect();
        }


        private void Connect()
        {
            //attempt to sync with motor
            //int speed = (int)((double)nmSpiRPM.Value * 2.122);
            //int speed = (int)((double)nmSpiRPM.Value * 3.772);
            //int speed = (int)((double)nmSpiRPM.Value * 3.7022); //Adjusted by BG and CC on 9/7/12
            if (mbus.WriteModbusQueue(1, 2000, 0, true))
            {
                isSpiConnected = true;
                message = "Spindle Connected";
            }
            else
                isSpiConnected = false;
            message = "Spindle Failed to connect";
        }

        private void Disconnect()
        {
            mbus.WriteModbusQueue(1, 2000, 0, false);
            isSpiConnected = false;
        }

        public void StopSpi()//Stop the Spindle Motor
        {
            //Stop the spindle
            mbus.WriteModbusQueue(1, 2000, 0, false);
        }

        private bool ChangeDir(bool Dir)
        {
            return true;
        }

        private bool ChangeRPM(Double RPM)
        {
            //allow for checking of maximum speeds and insure IPM is positive
            double CheckRPM = Math.Abs(RPM);
            if (CheckRPM > RPMmax)
            {
                CheckRPM = RPMmax;
            }
            else
            {
                if (CheckRPM < RPMmin)
                {
                    CheckRPM = RPMmin;
                }

            }
            if (Math.Abs(CheckRPM - SpiRPM) > epsilon)
            {
                SpiRPM = CheckRPM;
                //int speed = (int)(RPM * 3.7022); //Adjusted by BG and CC 9/7/12
                controller.WriteModbusQueue(1, 2002, (int)(SpiRPM * 3.772), false);
                return true;
            }
            return false;
        }

        private void MoveCC()
        {}

        private void MoveCCW()
        {}

        public void 

        internal void SpindleRun(double height)
        {
            string message = "";
            {
                //if vertical is high slow it down
                if (height > 1)
                    height = 1;

                if (isSpiCW)
                {
                    StartSpiCW();
                    message = "Spindle was started CW with RPM " + SpiRPM.ToString();
                }
                else
                {
                    StartSpiCCW();
                    message = "Spindle was started CcW with RPM " + SpiRPM.ToString();
                }

            }

            return message;
        }


        internal string ChangeSpiRef(double RPM) //Change the Spindle Reference
        {
            string message = "";
            if (isSpiConnected)
            {
                //old pulley speed
                // int speed = (int)((double)nmSpiRPM.Value * 1.58);

                // int speed = (int)(RPM * 2.122);
                SpiRPM = (int)(RPM * 3.772);
                //int speed = (int)(RPM * 3.7022); //Adjusted by BG and CC 9/7/12
                mbus.WriteModbusQueue(1, 2002, SpiRPM, false);
                message = "Spindle RPM was changed to " + SpiRPM.ToString();
            }
            else
            {
                StopSpi();
                message = "Spindle was Stopped";

            }
            return message;
        }



        internal string StartSpiCW()//Start the spindle motor going clockwise
        {
            String message = "";
            if (isSpiConnected)
            {
                mbus.WriteModbusQueue(1, 2000, 1, false);
                isSpiOn = true;
                message = "Spindle was started CCW with RPM " + SpiRPM.ToString();
            }
            else
            {
                StopSpi();
                message = "Spindle was stopped, spindle not connected";
            }
            return message;
        }

        void StartSpiCCW()//Start the spindle motor going counter-clockwise
        {
            if (isSpiConnected)
            {
                mbus.WriteModbusQueue(1, 2000, 3, false);
                isSpiOn = true;
            }
            else
            {
                StopSpi();
            }
        }

        internal void SpindleUDPControl()
        {
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
            SpiSpeed[0] = -99.9;
            SpiSpeed[1] = SpiSpeed[0];
            SpiSpeed[0] = BitConverter.ToDouble(RecieveBytes, 8);
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
        }
    }
}
