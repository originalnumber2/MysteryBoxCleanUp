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

        Mutex commandMutex;

        public MotorController()
        {
            //creation of communication protocals
            modbus = new Modbus();
            UDPCom = new UDPCom();


            //creating the verticle motor, it communicates over USB RS424
            verticalMotor = new VerticalMotor();

            //creating the transverse motor, it communicatates over UDP or Modbus
            transverseMotor = new TransverseMotor();

            //creating the Lateral motor, it communicatates over UDP or Modbus
            lateralMotor = new LateralMotor();

            //creating the spindle motor, it communicatates over Modbus
            spindleMotor = new SpindleMotor();
        }

        internal string MoveVertical()
        {
            string returnMes = "";

            if (verticalMotor.isVerCon)
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

            if (lateralMotor.isLatCon)
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

            if (transverseMotor.isTraCon)
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

            if (spindleMotor.isSpiCon)
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
            if (verticalMotor.isVerCon)
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
            if (lateralMotor.isLatCon)
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

    }
}

