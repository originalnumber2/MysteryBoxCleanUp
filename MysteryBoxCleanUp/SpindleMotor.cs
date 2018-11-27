using System;

namespace MysteryBoxCleanUp
{
    // Simplificatin and lubrication control is needed. if the spindle is going to fast the lube is required.
    public class SpindleMotor
    {

        MotorController controller;

        public bool isSpiCon;
        public bool isSimulinkControl;
        public double SpiRPM;
        public bool SpiDir; //Spindle direction true - Clockwise false - counter clockwise
        public double RPMmin;
        public double RPMmax;
        private double epsilon;

        public SpindleMotor(MotorController motorController)
        {
            isSpiCon = false;
            isSimulinkControl = false;
            SpiRPM = 0;
            SpiDir = true;
            RPMmax = 2000;
            RPMmin = 0;

            controller = motorController;

        }

        internal void ConnectionToggle()
        {
            if (isSpiCon)
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
                isSpiCon = true;
                message = "Spindle Connected";
            }
            else
                isSpiCon = false;
            message = "Spindle Failed to connect";
        }

        private void Disconnect()
        {
            mbus.WriteModbusQueue(1, 2000, 0, false);
            isSpiCon = false;
        }

        public void StopSpi()//Stop the Spindle Motor
        {
            //Stop the spindle
            mbus.WriteModbusQueue(1, 2000, 0, false);
        }

        private bool ChangeDir(bool dir)
        {
            if (dir = SpiDir)
            {
                return false;
            }
            SpiDir = dir;
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

        private void MoveCCModbus()
        {
            if (isSpiCon)
            {
                mbus.WriteModbusQueue(1, 2000, 1, false);
            }
        }

        private void MoveCCWModbus()
        {
            if (isSpiCon)
            {
                mbus.WriteModbusQueue(1, 2000, 1, false);
            }
        }

        public void moveModbus(double IPM, bool dir)
        {
            if (ChangeDir(dir) || ChangeIPM(IPM))
            {
                if (SpiDir) { MoveCCModbus() }
                else MoveCCWModbus();
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
