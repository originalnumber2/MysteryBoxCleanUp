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

        void LinearWeld()
        {

        }

        void surfaceprobe()
        {
            double[] latLocs;
            double[] TraLocs;
            double latLoc = 0;
            double traLoc = 0;

            foreach (double i in latLocs)
            {
                foreach (double j in traLoc)
                {
                    positionController.MoveToPlane(i, j);
                    AutoZero();


                }
            }
        }
    
        double AutoZero()
        {
            double height = 0;
            double epsilon = 0.0006;
          

                                  MessageBox.Show("AutoAUTOzero:\nMake sure tool positioned over set piece\n");
            double autoautozerocurrent, autoautozeroprevious;
            bool isAutoAutoZeroZeroed;
            //*********Zero the axial force reading************
            btnZzero_Click(new object(), new EventArgs());
            if (MaxZForce < 800)
                MaxZForce = 800;
            Thread.Sleep(1000);
            double azforce;
            azforce = (double)nmPinchForce.Value; //from the spot welding box added by chase on 4/9/12

            if (ZForce > 100) // This is a test for initial force
                MessageBox.Show("Force Already Exceeds Turn-On Force");
            else
            {
                WriteMessageQueue("The AutoAutoZero Process Will Now Begin\nNOTE:Currently using .0006 as tolerance");
                //Alert GUI of first upward motion
                //isAutoisfirstup = true;

                //initiate table raising
                VerPort.Write("MC H+ A10 V.3 G\r");

                while (ZForce < azforce) //changed from 100 to azforce by Chase 4/9/2012
                {
                    Thread.Sleep(0);//changed to sleep 0 by Jay 6/10/2015
                }
                //Stop the motor
                StopVer();

                Thread.Sleep(500);

                ////Alert GUI of first downward motion
                //isAutoisfirstdown = true;

                ////initiate table lowering
                VerPort.Write("MN A10 V2 D-3000 G\r");

                ////Sleep for one second
                Thread.Sleep(1000);

                ////Alert GUI of final upward motion
                //isAutoissecondup = true;
                autoautozerocurrent = -2;
                autoautozeroprevious = -1;
                bool LoopRepeat = true;
                isAutoAutoZeroZeroed = false;
                while (!isAutoAutoZeroZeroed)
                {
                    autoautozeroprevious = autoautozerocurrent;
                    ////Do final upward in a loop
                    LoopRepeat = true;

                    while (LoopRepeat)
                    {
                        //Wait for turn off signal
                        if (ZForce > azforce) //changed from ZForce changed by Chase on 11/21/11 //changed from 100 to azforce by Chase 4/9/2012
                        {
                            LoopRepeat = false;
                        }
                        else
                        {
                            VerPort.Write("MN A1 V1 D100 G\r");
                        }
                        Thread.Sleep(1000);
                    }

                    ////Sleep for stability
                    Thread.Sleep(500);
                    autoautozerocurrent = VerLoc;
                    if (Math.Abs(autoautozerocurrent - autoautozeroprevious) < .0006)
                    {
                        isAutoAutoZeroZeroed = true;
                    }
                    else
                    {
                        //move table down
                        VerPort.Write("MN A10 V2 D-3000 G\r");

                        ////Sleep for one second
                        Thread.Sleep(1000);
                    }
                    WriteMessageQueue(autoautozerocurrent.ToString("F4"));
                }
                ////Set weld height
                isSetVerWeld = true;
                VerWeld = VerLoc;


                ////Sleep for assurance of GUI update
                Thread.Sleep(1000);
                //move table down
                VerPort.Write("MN A10 V2 D-3000 G\r");
                Thread.Sleep(1000);
                //Clear out port
                VerPort.ReadExisting(); //this was edited out

                //Query the motor and establish RS-232 control
                VerPort.Write("E ON 1R\r"); //this was edited out
                Thread.Sleep(300); //this was edited out

                //Turn off limits
                VerPort.Write("1LD3\r"); //this was edited out
                Thread.Sleep(30); //this was edited out

                //Clear out port and place in holding string
                VerPort.ReadExisting(); //this was edited out
                double tempver = VerLoc;
                //initiate table raising
                VerPort.Write("MC H- A10 V2 G\r");
                while (VerLoc > (tempver - .125))
                {
                    Thread.Sleep(30);
                }
                StopVer();
                UpdateLimitValues();
                WriteMessageQueue("AutoAutoZero Completed");
                //Always release the control semaphore before finishing a Machine Control Task
                try { ControlSemaphore.Release(1); }
                catch (System.Threading.SemaphoreFullException ex)
                {
                    MessageBox.Show("error releasing ControlSemaphore 6 " + ex.Message.ToString());
                }

            }

            //return height;
        }
    }
}
