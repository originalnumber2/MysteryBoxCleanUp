using System;
namespace MysteryBoxCleanUp
{
    public class Controller
    {
        MotorController motorController;
        MessageQueue messageQueue;

        public Controller()
        {
            //Que for managing all messages;
            messageQueue = new MessageQueue();
            //controller for managing all of the motors. 
            motorController = new MotorController();
        }

        void surfaceprobe()
        {

        }
    }
}
