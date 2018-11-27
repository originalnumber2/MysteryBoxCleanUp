using System;
namespace MysteryBoxCleanUp
{
    public class PositionController
    {
        double LatLoc; //position of the lateral axis
        double TraLoc; //Position of the Transverse axis
        double VerLoc; // Position of the Verticle axis

        double EpsilonLat; //accuracy variable for the Lateral Axis
        double EpsilonTra; //accuracy variable for the transverse axis
        double EpsilonVer; //accuracy variable for the Verticle Axis

        Controller controller; //Pass the controller for access to motor controller. Might want to change this to motor controller eventually
        //Having them isolated like this will be better for threading and passing data from thread to thread

        public PositionController(Controller cont)
        {

            controller = cont;

            LatLoc = GetLatLoc();
            TraLoc = GetTraLoc();
            VerLoc = GetVerLoc();

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
            if(Math.Abs(verLoc - VerLoc) < EpsilonVer)
            {
                controller.MotorController.MoveVertical();
            }
            else
            {
                controller.MotorController.StopVertical();
            }
        }

        private void MoveToTraLoc(double traLoc)
        {
            if (Math.Abs(traLoc - TraLoc) < EpsilonTra)
            {
                controller.MotorController.MoveTransverse();
            }
            else
            {
                controller.MotorController.StopTransverse();
            }
        }

        private void MoveToLatLoc(double latLoc)
        {
            if (Math.Abs(latLoc - LatLoc) < EpsilonLat)
            {
                controller.MotorController.MoveLateral();
            }
            else
            {
                controller.MotorController.StopLateral();
            }
        }
    }
}
