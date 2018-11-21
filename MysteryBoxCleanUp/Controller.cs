using System;
namespace MysteryBoxCleanUp
{
    public class Controller
    {
        MotorController motorController;
        PositionController positionController;
        DataController dataController;
        MessageQueue messageQueue;

        public Controller()
        {
            //Que for managing all messages;
            messageQueue = new MessageQueue();
            //controller for managing all of the motors. 
            motorController = new MotorController();
            //new controller for the control of the position
            positionController = new PositionController(this);
            //new conroller for the flow of data
            dataController = new DataController();
        }

        void surfaceprobe()
        {
            positionController.MoveToPlane(latLoc, traLoc);
            AutoZero()
        }

        void AutoZero()
        {

        }
    }
}
