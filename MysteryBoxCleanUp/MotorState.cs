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
        public double epsilon;

        public MotorState(double MaxSpeed, double MinSpeed, double error)
        {
            IsCon = false;
            IsSimulinkControl = false;
            Speed = 0;
            Dir = true;
            Max = MaxSpeed;
            Min = MinSpeed;
            epsilon = error;
        }

        public override string ToString()
        {
            string mes = " Connected: " + IsCon.ToString() + " " +
                "Simulink: " + IsSimulinkControl.ToString() + " " +
                "Speed: " + Speed.ToString() + " " +
                "Direction: " + Dir.ToString() + " " +
                "Max Speed: " + Max.ToString() + " " +
                "Min Speed: " + Min.ToString() + " " +
                "epsilon: " + epsilon.ToString();
            return string.Format(mes);
        }

        //this function checks if the IPM of the Lateral motor changes then changes it. returns true if so
        private (bool, double) ChangeSpeed(double SetSpeed)
        {
            //allow for checking of maximum speeds and insure IPM is positive
            double CheckSpeed = Math.Abs(SetSpeed);
            if (CheckSpeed > Max)
            {
                CheckSpeed = Max;
            }
            else
            {
                if (CheckSpeed < Min)
                {
                    CheckSpeed = Min;
                }

            }
            if (Math.Abs(CheckSpeed - Speed) > epsilon)
            {
                Speed = CheckSpeed;
                return (true, Speed);
            }
            return (false, 0);
        }

        //this function check if the direction of the lateral motor changes. changes it if required. Returns true if so.
        private (bool, bool) ChangeDir(bool dir)
        {
            if (dir = Dir)
            {
                return (false, Dir);
            }
            Dir = dir;
            return (true, Dir);
        }
    }
}