using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{
    public class MessageQueue
    {

        Semaphore MessageQueueSemaphore;
        Queue Queue;
        Mutex MessageQueueMutex;


        public MessageQueue()
        {
            MessageQueueSemaphore = new Semaphore(0, 50);
            Queue = new Queue();
            MessageQueueMutex = new Mutex();
        }

        internal void WriteMessageQueue(string message)
        {
            message = message + "\r\n";
            MessageQueueMutex.WaitOne();
            Queue.Enqueue(message);
            MessageQueueMutex.ReleaseMutex();
            try { MessageQueueSemaphore.Release(1); }
            catch (System.Threading.SemaphoreFullException ex)
            {
                MessageBox.Show("error releasing MessageQueueSemaphore 5 " + ex.Message.ToString());
            }

        }

        private String ReadMessageQueue()
        {
            string mes = "";
            if (MessageQueueSemaphore.WaitOne(0))
            {
                MessageQueueMutex.WaitOne();
                mes = (string)Queue.Dequeue();
                MessageQueueMutex.ReleaseMutex();
            }
            return mes;
        }

    }
}
