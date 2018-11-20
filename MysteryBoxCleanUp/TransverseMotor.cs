using System;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{
    public class TransverseMotor
    {

        public bool IsTraCon { get; set; }
        public bool IsSimulinkControl { get; set; } //determines if the transverse motor is under modbus control or not
        //Values for describing locations in the traverse
        private double TraIPM { get; set; } //current speed the transverse axis is configured for.
        private bool TraDir; //current direction of the transverse axis true - Forward false - reverse
        public double TraMax { get; set; } //maximum speed the transverse axis (ipm) is allowed to move
        public double TraMin { get; set; } //minimum speed the transverse axis (ipm) is allowed to move
        MotorController controller;
        private double epsilon;

        public TransverseMotor(MotorController motorController)
        {
			IsTraCon = false;
            IsSimulinkControl = false;
            TraIPM = 0;
            TraDir = true;
            TraMax = 15;
            TraMin = 1;
            epsilon = 0.1;

            //inherats this from parent
            controller = motorController;


        }

        //attempts to connect to the transverse motors. returns true if sucessful. Returns false if unsucessful.
        private void Connect()
        {
            //double hz = ((double)nmTraIPM.Value) * 5.3333;
            if (controller.WriteModbusQueue(2, 1798, 1, true))
            {
                controller.WriteModbusQueue(2, 0x0300, 04, false);//give Traverse motor master frequency control to rs-485
                controller.WriteModbusQueue(2, 0x0301, 03, false);//give Traverse motor Source of operation command  to rs-485
                controller.WriteModbusQueue(2, 0x0705, 0, false);//set Traverse speed to zero
                controller.WriteModbusQueue(2, 0x010D, 0, false);//set Traverse motor direcction (Fwd/Rev) to be controled by rs-485
                IsTraCon = true;
            }
            else
            {
                MesQue.WriteMessageQueue("Connection to Traverse Motor Failed");
                IsTraCon = false;
            }
        }

        private void Disconnect()
        {
            controller.WriteModbusQueue(2, 0x0706, 1, false);
            IsTraCon = false;
        }

        public void ConnectToggle()
        {
            if (IsTraCon)
                Disconnect();
            else Connect();
        }

        void StopTra()//Stop the traverse Motor
        {
            controller.WriteModbusQueue(2, 0x0706, 1, false);
            IsTraCon = false;
        }

        private void MoveForwardModbus()
        {
            if (!IsTraCon)
                MessageBox.Show("Traverse not connected");
            else
            {
                Controller.WriteModbusQueue(2, 0x0706, 0x12, false);
                IsTraOn = true;
            }
        }

        private void MoveReverseModbus()
        {
            if (!IsTraCon)
                MessageBox.Show("Traverse not connected");
            else
            {
                mod.WriteModbusQueue(2, 0x0706, 0x22, false);
                IsTraOn = true;
            }
        }

        private bool ChangeIPM(double IPM)
        {
            //allow for checking of maximum speeds and insure IPM is positive
            double CheckIPM = Math.Abs(IPM);
            if (CheckIPM > TraMax)
            {
                CheckIPM = TraMax;
            }
            else
            {
                if (CheckIPM < TraMin)
                {
                    CheckIPM = TraMin;
                }

            }
            if (Math.Abs(CheckIPM - TraIPM) > epsilon)
            {
                TraIPM = CheckIPM;
                double hz = IPM * 2.8599;  //Modified by Brian when larger pulley was installed on traverse drive.
                Controller.WriteModbusQueue(2, 1797, ((int)(hz * 10)), false);
                return true;
            }
            return false;

        }

        private bool ChangeDir(bool dir)
        {
            if (dir = TraDir)
            {
                return false;
            }
            else
            {
                TraDir = dir;
                return true;
            }
        }



        void moveModbus(double IPM, bool dir)
        {
            if (ChangeDir(dir) || ChangeIPM(IPM))
            {
                if (TraDir) { MoveForwardModbus() }
                else MoveReverseModbus();
            }
        }


    }
}
