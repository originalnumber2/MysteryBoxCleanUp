using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{
    public class MatlabCom
    {
		Semaphore MatlabQueueSemaphore = new Semaphore(0, 50);
        Queue MatlabQueue = new Queue();
        Mutex MatlabQueueMutex = new Mutex();
        Thread MatlabWriteThread;

		//Constructor for the matlab communicator
		public MatlabCom()
		{
			MLApp.MLApp matlab = new MLApp.MLApp();//Start matlab
			MatlabExecute("load_system('simulink')");
			while (true)
			{
				try
				{
					MatlabQueueSemaphore.WaitOne();
					MatlabQueueMutex.WaitOne();
					matlab.Execute((string)MatlabQueue.Dequeue());
					MatlabQueueMutex.ReleaseMutex();
				}
				catch (System.InvalidOperationException e)
				{
					MessageBox.Show(e.ToString(), "Error reading commands from the matlab queue and sending them to matlab");
				}
			}
		}
              
		public void MatlabUpdateParameters()
        {
            //MatlabExecute("set_param(bdroot(gcs), 'SimulationCommand', 'update');");
            MatlabExecute("UpdateRT");
        }
        
        public void WriteMatlabQueue(string message)
        {
            message = message + "\r\n";
            MatlabQueueMutex.WaitOne();
            MatlabQueue.Enqueue(message);
            MatlabQueueMutex.ReleaseMutex();
            try { MatlabQueueSemaphore.Release(1); }
            catch (System.Threading.SemaphoreFullException ex)
            {
                MessageBox.Show("error releasing MatlabQueueSemaphore 1 " + ex.Message.ToString());
            }
        }
        public void MatlabExecute(string message)
        {
            WriteMessageQueue(message);
            WriteMatlabQueue(message);
        }
        
    }
}
