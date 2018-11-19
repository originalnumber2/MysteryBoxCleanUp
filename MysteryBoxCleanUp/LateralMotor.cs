using System;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{
    public class LateralMotor
    {
		public bool isLatCon;
        public bool isSimulinkControl; // Determines if the motor is under modbus control or not.
        bool isLatOn;
        bool isLatIn;
        //Values for describing locations in the lateral direction
        double LatVolt;
        public double LatLoc;
        public double LatMax;
        public double LatMin;
        MotorController controller;

        public LateralMotor(MotorController motorController)
        {
			isLatCon = false;
            isLatOn = false;
            isLatIn = false;
            isSimulinkControl = false;
            LatVolt = 0;
            LatLoc = -3;
            LatMax = 3.5;
            LatMin = 1.5;

            controller = motorController;


        }

		void btnTraCon_Click(object sender, EventArgs e)
        {
            if (!isTraCon)
            {
                //attempt to sync with motor

                double hz = ((double)nmTraIPM.Value) * 5.3333;
                if (mod.WriteModbusQueue(2, 1798, 1, true))
                {
                    mod.WriteModbusQueue(2, 0x0300, 04, false);//give Traverse motor master frequency control to rs-485
                    mod.WriteModbusQueue(2, 0x0301, 03, false);//give Traverse motor Source of operation command  to rs-485
                    mod.WriteModbusQueue(2, 0x0705, 0, false);//set Traverse speed to zero
                    mod.WriteModbusQueue(2, 0x010D, 0, false);//set Traverse motor direcction (Fwd/Rev) to be controled by rs-485
                    isTraCon = true;
                    btnTraCon.BackColor = Color.Green;
                    boxTrav.Visible = true;
                }
                else
                    MesQue.WriteMessageQueue("Connection to Traverse Motor Failed");


            }
            else
            {
                //Stop the motor
                StopTra();

                isTraCon = false;
                btnTraCon.BackColor = Color.Red;
                boxTrav.Visible = false;
            }
        }
        void btnTraRun_Click(object sender, EventArgs e)
        {
            if (!isTraCon)
                MessageBox.Show("Traverse not connected");
            else
            {
                ChangeTraRef((double)nmTraIPM.Value);
                StartTraFor();
            }
        }
        void btnTraRev_Click(object sender, EventArgs e)
        {
            if (!isTraCon)
                MessageBox.Show("Traverse not connected");
            else
            {
                ChangeTraRef((double)nmTraIPM.Value);
                StartTraRev();
            }
        }
        void btnTraStop_Click(object sender, EventArgs e)
        {
            StopTra();
        }
        void nmTraIPM_ValueChanged(object sender, EventArgs e)
        {
            if (!isSimControl)
            {
                ChangeTraRef((double)nmTraIPM.Value);
            }
        }
        void StopTra()//Stop the traverse Motor
        {
            //if (isTraCon) Dont check to see if its connected safer to just try to stop it incase someting messes up
            {
                Mod.WriteModbusQueue(2, 0x0706, 1, false);
                isTraOn = false;
            }
        }
        void ChangeTraRef(double IPM)//Change the Traverse Reference
        {
            if (isTraCon)
            {
                //double hz = (IPM) * 5.3333;
                //double hz = IPM * 2.135;
                //double hz = IPM * 4.2433;
                double hz = IPM * 2.8599;  //Modified by Brian when larger pulley was installed on traverse drive.
                mod.WriteModbusQueue(2, 1797, ((int)(hz * 10)), false);

            }
            else
            {
                StopTra();
            }
        }
        void StartTraFor()//Start the traverse going forward
        {
            if (isTraCon)
            {
                mod.WriteModbusQueue(2, 0x0706, 0x12, false);
                isTraOn = true;
            }
            else
            {
                StopTra();
            }
        }
        void StartTraRev()//Start the traverse going in reverse
        {
            if (isTraCon)
            {
                mod.WriteModbusQueue(2, 0x0706, 0x22, false);
                isTraOn = true;
            }
            else
            {
                StopTra();
            }
        }
    }
}
