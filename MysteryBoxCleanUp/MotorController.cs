using System;
using System.Threading;

namespace MysteryBoxCleanUp
{
    public class MotorController
    {
        internal Modbus modbus;
        internal UDPCom UDPCom;
        VerticalMotor verticalMotor;
        TransverseMotor transverseMotor;
        LateralMotor lateralMotor;
        SpindleMotor spindleMotor;

        internal Controller Controller;

        internal Mutex commandMutex;

        //motor connection variables
        bool IsTraCon;
        bool IsLatCon;
        bool IsSpiCon;
        bool IsVerCon;

        //simulink UDP connection variables
        bool IsSimulinkConnected;

        //State Variables of the controller
        double SpiRPM;
        bool SpiDir; //Spindle direction true - Clockwise false - counter clockwise
        double TraIPM { get; set; } //current speed the transverse axis is configured for.
        bool TraDir; //current direction of the transverse axis true - Forward false - reverse
        public double LatIPM; //Current speed (IPM) of the lateral Axis
        public bool LatDir; //Current Direction of the Lateral Axis True - Out False - In
        double VerSpeed; //Current vertical motor speed
        double VerAccel; //current vertical motor acceleration
        internal bool VerDir; // vertical motor direction


        //not completely happy with this constructor
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

            UpdateParameters();
        }

        void UpdateParameters()
        {
            if (IsLatCon && IsSpiCon && IsTraCon && IsVerCon)
            {
                GetMotorStates();
                GetMotorDirection();
            }
            else mes.messageQueue("connect the motors");
        }

        void GetMotorConnections()
        {
            IsLatCon = lateralMotor.IsLatCon;
            IsTraCon = transverseMotor.IsTraCon;
            IsSpiCon = spindleMotor.isSpiCon;
            IsVerCon = verticalMotor.isVerCon;
        }

        //toggles to simulink control
        void SetInterfaceToggle()
        {
                transverseMotor.IsSimulinkControl = IsSimulinkConnected;
                lateralMotor.IsSimulinkControl = IsSimulinkConnected;
                verticalMotor.isSimulinkControl = IsSimulinkConnected;
                spindleMotor.isSimulinkControl = IsSimulinkConnected;
            //if(IsSimulinkConnected)
            //{
            //    transverseMotor.IsSimulinkControl = true;
            //    lateralMotor.IsSimulinkControl = true;
            //    verticalMotor.isSimulinkControl = true;
            //    spindleMotor.isSimulinkControl = true;
            //}
            //else
            //{
            //    transverseMotor.IsSimulinkControl = false;
            //    lateralMotor.IsSimulinkControl = false;
            //    verticalMotor.isSimulinkControl = false;
            //    spindleMotor.isSimulinkControl = false;
            //}

        }

        void GetMotorStates()
        {
            SpiRPM = spindleMotor.SpiRPM;
            SpiDir = spindleMotor.SpiDir;
            TraIPM = transverseMotor.TraIPM;
            TraDir = transverseMotor.TraDir;
            LatIPM = lateralMotor.LatIPM;
            LatDir = lateralMotor.LatDir;
            VerSpeed = verticalMotor.VerSpeed;
            VerAccel = verticalMotor.VerAccel;
        }

        void GetMotorDirection()
        {
            SpiDir = spindleMotor.SpiDir;
            TraDir = transverseMotor.TraDir;
            LatDir = lateralMotor.LatDir;
            isVerDown = verticalMotor.isVerDown;
        }

        internal string MoveVertical()
        {
            string returnMes = "";

            if (IsVerCon)
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

            if (IsLatCon)
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

            if (IsTraCon)
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

            if (IsSpiCon)
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
            if (IsVerCon)
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
            if (IsLatCon)
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
            if (IsTraCon)
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
            if (IsSpiCon)
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

    }
}

