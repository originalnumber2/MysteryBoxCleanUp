using System;
using System.Threading;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{
    public class NIDaq
    {

        double NIMaxVolt = 1;

        //private NationalInstruments.DAQmx.Task USB6008_2_AITask;
        int count; //number of samples to take when zeroing the Straingauges
        internal double Zvolt, SpiCurrent, Ztemp, ZForce, Zoffset, ZMaxHistory;
        public double MaxZForce = 18000;
        Semaphore USB6008_Mutex = new Semaphore(1, 1);//I know, it is not a mutex, but had some weird issues using mutex across multiple threads so this is my solution to use a sempafore as a mutex

        private NationalInstruments.DAQmx.Task USB6008_AITask;
        private NationalInstruments.DAQmx.Task USB6008_AOTask;
        private NationalInstruments.DAQmx.Task USB6008_DOTask;
        private AnalogMultiChannelReader USB6008_Reader; 
        private AnalogMultiChannelWriter USB6008_Analog_Writter;
        private DigitalSingleChannelWriter USB6008_Digital_Writter;

        public NIDaq()
        {
            count = 5; //I am not so sure i like this implementation of the counter. 
            Zvolt = 0;
            SpiCurrent = 0;
            ZForce = 0;
            Zoffset = 0;
            ZMaxHistory = 0;
            Setup_USB6008();
            SetOffset();
        }


        void ReadUSBDa()
        {
            double[] data1 = { 0, 0 };
            data1[0] = 0;
            data1[1] = 0;
            try
            {
                USB6008_1_Mutex.WaitOne();
                data1 = USB6008_Reader.ReadSingleSample();
                USB6008_1_Mutex.Release();
            }
            catch (NationalInstruments.DAQmx.DaqException ex)
            {
                onAlarm("usb6008 1 read error" + ex.Message.ToString());
                //error has occured need to reset daq
                Setup_USB6008();
                Thread.Sleep(50);
            }
            Zvolt = data1[0];
            SpiCurrent = data1[1] * 10;
            Ztemp = 21050 * (Zvolt - Zoffset);  //Subtract offset and multiply gain....Recalibrated from 17760 N/V on 9/16/2011, Recalibrated from 18843 N/V on 10/5/2011, Recalibrated from 19785 N/V on 5/7/2012
            ZForce = Ztemp; //BG commented out above 4 lines and added this to report full range (+ and -) Z values for Russ's bobbin welding
                            //Read in rotation angles of the lateral traverse and vertical using the second ni usb6008
        }

        public void Setup_USB6008()
        {
                
            //Resets and configures the NI USB6008 Daq boards
            Device dev = DaqSystem.Local.LoadDevice("dev3");//added to reset the DAQ boards if they fail to comunicate giving error code 50405
            dev.Reset(); 
            AIChannel StrainChannel, CurrentChannel;
            AOChannel LateralMotorChannel, TraverseMotorChannel;
            try
            {
                //Setting up NI DAQ for Axial Force Measurment via Strain Circuit and current Measurment of Spindle Motor for torque 
                USB6008_AITask = new NationalInstruments.DAQmx.Task();

                StrainChannel = USB6008_AITask.AIChannels.CreateVoltageChannel(
                    "dev3/ai0",  //Physical name of channel
                    "strainChannel",  //The name to associate with this channel
                    AITerminalConfiguration.Differential,  //Differential Wiring
                    -0.1,  //-0.1v minimum
                    NIMaxVolt,  //1v maximum
                    AIVoltageUnits.Volts  //Use volts
                    );
                CurrentChannel = USB6008_AITask.AIChannels.CreateVoltageChannel(
                   "dev3/ai1",  //Physical name of channel
                   "CurrentChannel",  //The name to associate with this channel
                   AITerminalConfiguration.Differential,  //Differential Wiring
                   -0.1,  //-0.1v minimum
                   10,  //10v maximum
                   AIVoltageUnits.Volts  //Use volts
                   );
                USB6008_Reader = new AnalogMultiChannelReader(USB6008_1_AITask.Stream);
                ////////////////////////////////////////////////////////////
                USB6008_AOTask = new NationalInstruments.DAQmx.Task();
                TraverseMotorChannel = USB6008_AOTask.AOChannels.CreateVoltageChannel(
                    "dev3/ao0",  //Physical name of channel)
                    "TravverseMotorChannel",  //The name to associate with this channel
                    0,  //0v minimum
                    5,  //5v maximum
                    AOVoltageUnits.Volts
                    );
                LateralMotorChannel = USB6008_AOTask.AOChannels.CreateVoltageChannel(
                    "dev3/ao1",  //Physical name of channel)
                    "LateralMotorChannel",  //The name to associate with this channel
                    0,  //0v minimum
                    5,  //5v maximum
                    AOVoltageUnits.Volts
                    );
                USB6008_Analog_Writter = new AnalogMultiChannelWriter(USB6008_AOTask.Stream);
                ////////////////////////////////////////////////////////////
                USB6008_DOTask = new NationalInstruments.DAQmx.Task();
                USB6008_DOTask.DOChannels.CreateChannel("dev3/port0", "port0", ChannelLineGrouping.OneChannelForAllLines);
                USB6008_Digital_Writter = new DigitalSingleChannelWriter(USB6008_DOTask.Stream);
            }
            catch (NationalInstruments.DAQmx.DaqException e)
            {
                MessageBox.Show("Error?\n\n" + e.ToString(), "NI USB 6008 1 Error");
            }
        }

        double ZeroStrainGauges(int count)
        {
            double ZMeasureSum = 0;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    USB6008_Mutex.WaitOne();
                    ZMeasureSum += USB6008_Reader.ReadSingleSample()[0];
                    USB6008_Mutex.Release();
                }
                catch (NationalInstruments.DAQmx.DaqException ex)
                {
                    MessageBox.Show("Error Reading Analog input for zero button " + ex.Message.ToString());
                }
                Thread.Sleep(5);
            }
            return ZMeasureSum / count;
        }

        void SetOffset()
        {
            Zoffset = ZeroStrainGauges(count);
        }
    }
}

