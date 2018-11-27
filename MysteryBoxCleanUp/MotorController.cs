using System;
using System.Threading;

namespace MysteryBoxCleanUp
{
    public class MotorController
    {
        Modbus modbus;
        UDPCom UDPCom;
        VerticalMotor verticalMotor;
        TransverseMotor transverseMotor;
        LateralMotor lateralMotor;
        SpindleMotor spindleMotor;

        Controller Controller;

        Mutex commandMutex;

        //State Variables of the controller
        double SpiRPM;
        bool SpiDir; //Spindle direction true - Clockwise false - counter clockwise
        double TraIPM { get; set; } //current speed the transverse axis is configured for.
        bool TraDir; //current direction of the transverse axis true - Forward false - reverse
        public double LatIPM; //Current speed (IPM) of the lateral Axis
        public bool LatDir; //Current Direction of the Lateral Axis True - Out False - In

        public MotorController(Controller controller)
        {
            //creation of communication protocals
            modbus = new Modbus();
            UDPCom = new UDPCom();

            Controller = controller;

            //creating the verticle motor, it communicates over USB RS424
            verticalMotor = new VerticalMotor();

            //creating the transverse motor, it communicatates over UDP or Modbus
            transverseMotor = new TransverseMotor(this);

            //creating the Lateral motor, it communicatates over UDP or Modbus
            lateralMotor = new LateralMotor(this);

            //creating the spindle motor, it communicatates over Modbus
            spindleMotor = new SpindleMotor(this);
        }

        internal string MoveVertical()
        {
            string returnMes = "";

            if (verticalMotor.isVerConnected)
            {
                //Vertical motor is moved in a specified way.
                returnMes = returnMes + verticalMotor.move(command);
            }
            else
            {
                returnMes = returnMes + "vertical motor is not connected";
            }
            return returnMes;
        }

        internal string MoveLateral()
        {
            string returnMes = "";

            if (lateralMotor.IsLatCon)
            {
                //need to account for Protocal Scheme and Max Speeds
                returnMes = returnMes + lateralMotor.move(command);
            }
            else
            {
                returnMes = returnMes + "Lateral motor is not connected";
            }
            return returnMes;
        }

        internal string MoveTransverse()
        {
            string returnMes = "";

            if (transverseMotor.IsTraCon)
            {
                //need to account for Protocal Scheme and Max Speeds
                returnMes = returnMes + TransverseMotor.move(command);
            }
            else
            {
                returnMes = returnMes + "Lateral motor is not connected";
            }
            return returnMes;
        }

        internal string StartSpindle()
        {
            string returnMes = "";

            if (spindleMotor.isSpiConnected)
            {
                //need to account for Protocal Scheme and Max Speeds
                returnMes = returnMes + SpindleMotor.move(command);
            }
            else
            {
                returnMes = returnMes + "Lateral motor is not connected";
            }
            return returnMes;
        }

        internal string ConnVertical()
        {
            string returnMes = "";
            if (verticalMotor.isVerConnected)
            {
                //Vertical motor is moved in a specified way.
                returnMes = returnMes + "Vertical motor is already connected";
            }
            else
            {
                returnMes = returnMes + verticalMotor.connect();
            }
            return returnMes;
        }

        internal string ConnLateral()
        {
            string returnMes = "";
            if (lateralMotor.IsLatCon)
            {
                //Vertical motor is moved in a specified way.
                returnMes = returnMes + "Lateral motor is already connected";
            }
            else
            {
                returnMes = returnMes + lateralMotor.connect();
            }
            return returnMes;
        }

        internal string ConnTransverse()
        {
            string returnMes = "";
            if (verticalMotor.isVerCon)
            {
                //Vertical motor is moved in a specified way.
                returnMes = returnMes + "Transverse motor is already connected";
            }
            else
            {
                returnMes = returnMes + transverseMotor.connect();
            }
            return returnMes;
        }

        internal string ConnSpindle()
        {
            string returnMes = "";
            if (spindleMotor.isSpiCon)
            {
                //Vertical motor is moved in a specified way.
                returnMes = returnMes + "Vertical motor is already connected";
            }
            else
            {
                returnMes = returnMes + spindleMotor.connect();
            }
            return returnMes;
        }

        internal void StopVertical()
        {
            verticalMotor.StopVer();
        }

        internal void StopLateral()
        {
            lateralMotor.StopLat();
        }

        internal void StopTransverse()
        {
            transverseMotor.StopTra();
        }

        internal void StopSpindle()
        {
            spindleMotor.StopSpi();
        }



        //Toggles Control between UDP and Modbus Control
        //placeholder
        internal String ToggleControl()
        {
            return "";
        }

    }
}

