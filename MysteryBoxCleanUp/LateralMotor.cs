using System;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{
    public class LateralMotor
    {
        public bool IsLatCon;
        public bool IsSimulinkControl; // Determines if the motor is under modbus control or not.
        public double LatIPM; //Current speed (IPM) of the lateral Axis
        public bool LatDir; //Current Direction of the Lateral Axis True - Out False - In
        public double LatMax;
        public double LatMin;
        MotorController controller;
        private double epsilon;

        public LateralMotor(MotorController motorController)
        {
            IsLatCon = false;
            IsSimulinkControl = false;
            LatIPM = 0;
            LatDir = true;
            LatMax = 5;
            LatMin = 1;
            epsilon = 0.1;

            controller = motorController;
        }

        private void Connect()
        {

            if (controller.WriteModbusQueue(2, 1798, 1, true))
            {
                controller.WriteModbusQueue(3, 0x0300, 04, false);//give lateral motor master frequency control to rs-485
                controller.WriteModbusQueue(3, 0x0301, 03, false);//give lateral motor Source of operation command  to rs-485
                controller.WriteModbusQueue(3, 0x010D, 0, false);//set Traverse motor direcction (Fwd/Rev) to be controled by rs-485
                controller.WriteModbusQueue(3, 0x0705, 0, false);//set lateral speed to zero
                IsLatCon = true;
            }
            else
            {
                MesQue.WriteMessageQueue("Connection to Traverse Motor Failed");
                IsLatCon = false;
            }
        }

        private void Disconnect()
        {
            controller.WriteModbusQueue(3, 0x0706, 1, false);
            IsLatCon = false;
        }

        public void ConnectionToggle()
        {
            if (IsLatCon)
            {
                Disconnect();
            }
            else Connect();

        }


        //Slow Movement Commands Forward Using Modbus
        private void MoveOutModbus()
        {
            if (!IsLatCon)
                MessageBox.Show("Traverse not connected");
            else
            {
                Controller.WriteModbusQueue(3, 0x0706, 0x22, false);
            }
        }

        //Slow Movement Commands Reverse Using Modbus
        private void MoveInModbus()
        {
            if (!IsLatCon)
                MessageBox.Show("Traverse not connected");
            else
            {
                controller.WriteModbusQueue(3, 0x0706, 0x12, false);
            }
        }

       public void Stop()
        {
            Controller.WriteModbusQueue(3, 0x0706, 1, false);
            IsLatCon = false;
        }

        //this function checks if the IPM of the Lateral motor changes then changes it. returns true if so
        private bool ChangeIPM(double IPM)
        {
            //allow for checking of maximum speeds and insure IPM is positive
            double CheckIPM = Math.Abs(IPM);
            if (CheckIPM > LatMax)
            {
                CheckIPM = LatMax;
            }
            else
            {
                if(CheckIPM < LatMin)
                {
                    CheckIPM = LatMin;
                }

            }
            if (Math.Abs(CheckIPM - LatIPM) > epsilon)
            {
                LatIPM = CheckIPM;
                double LatinRPM = IPM * 54.5;
                double hz = LatinRPM / 60.0;
                controller.WriteModbusQueue(3, 0x0705, ((int)(hz * 10)), false);
                return true;
            }
            return false;    
        }

        //this function check if the direction of the lateral motor changes. changes it if required. Returns true if so.
        private bool ChangeDir(bool dir)
        {
            if (dir = LatDir)
            {
                return false;
            }
            LatDir = dir;
            return true;
        }

        //This function moves the Lateral Motor checking for direction and speed changes
        public void MoveModbus(double IPM, bool dir)
        {
            if(ChangeDir(dir) || ChangeIPM(IPM))
            {
                if (LatDir) { MoveOutModbus() }
                else MoveInModbus();
            }

        }



        }
    }
}
