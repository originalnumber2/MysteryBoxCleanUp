using System;
namespace MysteryBoxCleanUp
{
    public class MotorState
    {
        public bool IsCon;
        public bool IsSimulinkControl; // Determines if the motor is under modbus control or not.
        public double Speed; //Current speed (IPM) of the lateral Axis
        public bool Dir; //Current Direction of the Lateral Axis True - Out False - In
        public double Max;
        public double Min;

        public MotorState(double MaxSpeed, double MinSpeed)
        {
            IsCon = false;
            IsSimulinkControl = false;
            Speed = 0;
            Dir = true;
            Max = MaxSpeed;
            Min = MinSpeed;
        }

        public override string ToString()
        {
            return string.Format("[MotorState]");
        }
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
            if (CheckIPM < LatMin)
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
}
