using System;
namespace MysteryBoxCleanUp
{
    public class PositionController
    {
        double LatLoc; //position of the lateral axis
        double TraLoc; //Position of the Transverse axis
        double VerLoc; // Position of the Verticle axis

        double LatMax, LatMin; //maximum and minimum Lateral Values
        double TraMax, TraMin; //maximum and minimum transverse Values
        double VerMax, VerMin; //maximum and minimum Vertical values

        double EpsilonLat; //accuracy variable for the Lateral Axis
        double EpsilonTra; //accuracy variable for the transverse axis
        double EpsilonVer; //accuracy variable for the Verticle Axis

        Controller controller; //Pass the controller for access to motor controller. Might want to change this to motor controller eventually
        //Having them isolated like this will be better for threading and passing data from thread to thread

        public PositionController(Controller cont)
        {

            controller = cont;

            //might want to update these to an getter that works for not connected errors
            LatLoc = GetLatLoc();
            TraLoc = GetTraLoc();
            VerLoc = GetVerLoc();

            SetAllLimits(3.5, 1.5, 27.5, 9, 7.5, 1);

        }


        public bool CheckLatLimits()
        {
            return LatMax >= LatLoc && LatMin <= LatLoc;
        }

        public bool CheckTraLimits()
        {
            return TraMax >= TraLoc && TraMin <= TraLoc;
        }

        public bool CheckVerLimits()
        {
            return VerMax >= VerLoc && VerMin <= VerLoc;
        }

        void SetAllLimits(double latMax, double latMin, double traMax, double traMin, double verMax, double verMin)
        {
            SetLatLimits(latMax, latMin);
            SetTraLimits(traMax, traMin);
            SetVerLimits(verMax, verMin);
        }

        void SetLatLimits(double max, double min)
        {
            LatMax = max;
            LatMin = min;
        }

        void SetTraLimits(double max, double min)
        {
            TraMax = max;
            TraMin = min;
        }

        void SetVerLimits(double max, double min)
        {
            VerMax = max;
            VerMin = min;
        }

        private double GetVerLoc()
        {
            return controller.LatLoc;
        }

        private double GetTraLoc()
        {
            return controller.TraLoc;
        }

        private double GetLatLoc()
        {
            return controller.VerLoc;
        }

        void MoveTo(double latLoc, double traLoc, double verLoc)
        {
            //doesnt move simultanously. would need to spin up seperate threads
            MoveToLatLoc(latLoc);
            MoveToTraLoc(traLoc);
            MoveToVerLoc(verLoc);
        }

        internal void MoveToPlane(double latLoc, double traLoc)
        {   
            //doesnt move simultanously. would need to spin up seperate threads
            MoveToLatLoc(latLoc);
            MoveToTraLoc(traLoc);
        }

        private void MoveToVerLoc(double verLoc)
        {
            while(Math.Abs(verLoc - VerLoc) > EpsilonVer)
            {
                controller.MotorController.MoveVertical();
            }
                controller.MotorController.StopVertical();
        }

        private void MoveToTraLoc(double traLoc)
        {
            while(Math.Abs(traLoc - TraLoc) > EpsilonTra)
            {
                controller.MotorController.MoveTransverse();
            }
                controller.MotorController.StopTransverse();
        }

        private void MoveToLatLoc(double latLoc)
        {
            while(Math.Abs(latLoc - LatLoc) > EpsilonLat)
            {
                controller.MotorController.MoveLateral();
            }
            controller.MotorController.StopLateral();
        }
    }
}
