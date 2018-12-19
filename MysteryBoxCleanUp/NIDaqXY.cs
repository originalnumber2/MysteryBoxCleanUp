using System;
using System.Threading;
using NationalInstruments.DAQmx;

namespace MysteryBoxCleanUp
{
    public class NIDaqXY:NIDaqInt
    {
        readonly double NIMaxVolt = 10;
        readonly double NIMinVolt = -10;
        readonly string loc = "dev2";
        bool IsXYCon;

        //varaibles to hold data
        internal double TraLoc, LatLoc;

        Semaphore USB6008_Mutex = new Semaphore(1, 1);//I know, it is not a mutex, but had some weird issues using mutex across multiple threads so this is my solution to use a sempafore as a mutex

        //Variables for setting up the USB Daq
        private NationalInstruments.DAQmx.Task USB6008_AITask;
        private NationalInstruments.DAQmx.Task USB6008_AOTask;
        //private NationalInstruments.DAQmx.Task USB6008_DOTask;
        private AnalogMultiChannelReader USB6008_Reader;
        //private AnalogMultiChannelWriter USB6008_Analog_Writter;
        //private DigitalSingleChannelWriter USB6008_Digital_Writter;

        public NIDaqXY()
        {
            TraLoc = 0;
            LatLoc = 0;
       
            USB6008_Mutex = new Semaphore(1, 1);//I know, it is not a mutex, but had some weird issues using mutex across multiple threads so this is my solution to use a sempafore as a mutex
            IsXYCon = true;
            Setup_USB6008();
        }


        public double[] ReadUSBData()
        {
            double[] data = { 0, 0 };
            try
            {
                USB6008_Mutex.WaitOne();
                data = USB6008_Reader.ReadSingleSample();
                USB6008_Mutex.Release();
            }
            catch (NationalInstruments.DAQmx.DaqException ex)
            {
                Console.WriteLine("usb6008 1 read error" + ex.Message.ToString());
                //error has occured need to reset daq
                Setup_USB6008();
                Thread.Sleep(50);
            }

            return data;
        }

        void ConvertData(double[] data)
        {
            TraLoc = 1*data[0]+0;
            LatLoc = 1*data[1]+0;
            //Scaling factors need to be added.
        }

        public void Setup_USB6008()
        {

            //Resets and configures the NI USB6008 Daq boards
            Device dev = DaqSystem.Local.LoadDevice(loc);//added to reset the DAQ boards if they fail to comunicate giving error code 50405
            dev.Reset();
            AIChannel TransverseChannel, LateralChannel;
            try
            {
                //Setting up NI DAQ for Axial Force Measurment via Strain Circuit and current Measurment of Spindle Motor for torque 
                USB6008_AITask = new NationalInstruments.DAQmx.Task();

                TransverseChannel = USB6008_AITask.AIChannels.CreateVoltageChannel(
                    loc + "/ai0",  //Physical name of channel
                    "TransverseChannel",  //The name to associate with this channel
                    AITerminalConfiguration.Differential,  //Differential Wiring
                    -10,  //-10v minimum
                    10,  //10v maximum
                    AIVoltageUnits.Volts  //Use volts
                    );
                LateralChannel = USB6008_AITask.AIChannels.CreateVoltageChannel(
                   loc + "/ai1",  //Physical name of channel
                   "LateralChannel",  //The name to associate with this channel
                   AITerminalConfiguration.Differential,  //Differential Wiring
                   -10,  //-0.1v minimum
                   10,  //10v maximum
                   AIVoltageUnits.Volts  //Use volts
                   );
                USB6008_Reader = new AnalogMultiChannelReader(USB6008_AITask.Stream);
                ////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////
                USB6008_DOTask = new NationalInstruments.DAQmx.Task();
                USB6008_DOTask.DOChannels.CreateChannel(loc + "/port0", "port0", ChannelLineGrouping.OneChannelForAllLines);
                USB6008_Digital_Writter = new DigitalSingleChannelWriter(USB6008_DOTask.Stream);
                IsXYCon = true;
            }
            catch (NationalInstruments.DAQmx.DaqException e)
            {
                IsXYCon = false;
                Console.WriteLine("Error?\n\n" + e.ToString(), "NI USB 6008 1 Error");
            }

        }
    }
}
