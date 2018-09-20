using System;
namespace MysteryBoxCleanUp
{
    public class SpindleMotor
    {
        public SpindleMotor()
        {
        }

		void btnSpiCon_Click(object sender, EventArgs e)
        {
            if (!isSpiCon)
            {
                //attempt to sync with motor
                //int speed = (int)((double)nmSpiRPM.Value * 2.122);
                int speed = (int)((double)nmSpiRPM.Value * 3.772);
                //int speed = (int)((double)nmSpiRPM.Value * 3.7022); //Adjusted by BG and CC on 9/7/12
                if (WriteModbusQueue(1, 2000, 0, true))
                {
                    isSpiCon = true;
                    btnSpiCon.BackColor = Color.Green;
                    boxSpi.Visible = true;
                }
                else
                    WriteMessageQueue("Connection to Spindle Motor Failed");
            }
            else
            {
                StopSpi();//Stop the motor
                isSpiCon = false;
                btnSpiCon.BackColor = Color.Red;
                boxSpi.Visible = false;
            }
        }
        void nmSpiRPM_ValueChanged(object sender, EventArgs e)
        {
            if (!isSimControl)
            {
                ChangeSpiRef((double)nmSpiRPM.Value);
            }
        }
        void btnSpiRun_Click(object sender, EventArgs e)
        {
            //if (!isSpiCon)
            //{
            //    WriteMessageQueue("Spindle not connected");
            //}
            //else if (!isPressure)
            //{
            //    MessageBox.Show("No Pressure");
            //}
            //else if (!isLubOn)
            //{
            //    MessageBox.Show("The Lubricator is off");
            //}
            //else
            {
                //if vertical is high slow it down
                if (nmVerVel.Value > 1)
                    nmVerVel.Value = 1;

                if (rbSpiCW.Checked)
                    StartSpiCW();
                else
                    StartSpiCCW();
            }

        }
        private void btnSpiStop_Click(object sender, EventArgs e)
        {
            StopSpi();
        }
        void ChangeSpiRef(double RPM) //Change the Spindle Reference
        {
            if (isSpiCon)
            {
                //old pulley speed
                // int speed = (int)((double)nmSpiRPM.Value * 1.58);

                // int speed = (int)(RPM * 2.122);
                int speed = (int)(RPM * 3.772);
                //int speed = (int)(RPM * 3.7022); //Adjusted by BG and CC 9/7/12
                WriteModbusQueue(1, 2002, speed, false);
            }
            else
            {
                StopSpi();
            }
        }
        void StopSpi()//Stop the Spindle Motor
        {
            //Stop the spindle
            WriteModbusQueue(1, 2000, 0, false);
            isSpiOn = false;
        }
        void StartSpiCW()//Start the spindle motor going clockwise
        {
            if (isSpiCon)
            {
                WriteModbusQueue(1, 2000, 1, false);
                isSpiOn = true;
            }
            else
            {
                StopSpi();
            }
        }
        void StartSpiCCW()//Start the spindle motor going counter-clockwise
        {
            if (isSpiCon)
            {
                WriteModbusQueue(1, 2000, 3, false);
                isSpiOn = true;
            }
            else
            {
                StopSpi();
            }
        }
    }
}
