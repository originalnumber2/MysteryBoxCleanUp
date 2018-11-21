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
            throw new NotImplementedException();
        }

        private double GetTraLoc()
        {
            throw new NotImplementedException();
        }

        private double GetLatLoc()
        {
            throw new NotImplementedException();
        }

        void MoveTo(double LatLoc, double TraLoc, double VerLoc)
        {
            MoveToLatLoc(LatLoc);
            MoveToTraLoc(TraLoc);
            MoveToVerLoc(VerLoc);

        }

        internal  MoveToPlane(double LatLoc, double TraLoc)
        {
            MoveToLatLoc(LatLoc);
            MoveToTraLoc(TraLoc);
        }

        private void MoveToVerLoc(double verLoc)
        {
            throw new NotImplementedException();
        }

        private void MoveToTraLoc(double traLoc)
        {
            throw new NotImplementedException();
        }

        private void MoveToLatLoc(double latLoc)
        {
            throw new NotImplementedException();
        }

      


    }


}
