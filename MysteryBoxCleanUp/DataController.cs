using System;
namespace MysteryBoxCleanUp
{
    public class DataController
    {
        internal Controller Controller;
        internal SensorBox SensorBox;
        internal Dyno Dyno;
        internal NIDaq NIDaq;
        internal NIDaqXY NIDaqXY;

        bool isDynCon;

        double XForce;
        double YForce;
        double ZForceDyno;
        double TForce;
        double VAngle;
        double XYForce;
        double XYForceAverage;

        double SpiCurrent;
        double ZForce;
        double ZMaxHistory;

        bool isSenCon;
        bool isEmergencyButton;
        bool isLubOn;
        bool isPressure;
        double TraLoc;
        double LatLoc;
        double VerLoc;


        public DataController(Controller controller)
        {
            Controller = controller;

            SensorBox = new SensorBox(this);
            Dyno = new Dyno();
            NIDaq = new NIDaq();
            NIDaqXY = new NIDaqXY();

            isDynCon = false;

            XForce = 0;
            YForce = 0;
            ZForceDyno = 0;
            TForce = 0;
            VAngle = 0;
            XYForce = 0;
            XYForceAverage = 0;

            SpiCurrent = 0;
            ZForce = 0;
            ZMaxHistory = 0;

            isSenCon = false;
            isEmergencyButton = false;
            isLubOn = false;
            isPressure = false;
            TraLoc = 0;
            LatLoc = 0;
            VerLoc = 0;

            GetConnections();
            GetData();

        }

        void GetConnections()
        {
            isDynCon = Dyno.isDynCon;
            isSenCon = SensorBox.isSenCon;
        }

        void GetData()
        {
            if (isDynCon)
            {
                GetDynoData();
            }
            if (isSenCon)
            {
                GetSensorBoxConnections();
                GetSensorBoxData();
            }

            //needs proctection for connection
            GetNIDaqData();
            GetNIDaqXYData();
        }

        void GetDynoData()
        {
            XForce = Dyno.XForce;
            YForce = Dyno.YForce;
            ZForceDyno = Dyno.ZForceDyno;
            TForce = Dyno.TForce;
            VAngle = Dyno.VAngle;
            XYForce = Dyno.XYForce;
            XYForceAverage = Dyno.XYForceAverage;
        }

        void GetSensorBoxConnections()
        {
            isLubOn = SensorBox.isLubOn;
            isPressure = SensorBox.isPressure;
            isEmergencyButton = SensorBox.isEmergencyButton;
        }

        void GetSensorBoxData()
        {
            TraLoc = SensorBox.TraLoc;
            LatLoc = SensorBox.LatLoc;
            VerLoc = SensorBox.VerLoc;

        }

        void GetNIDaqData()
        {
            SpiCurrent = NIDaq.SpiCurrent;
            ZForce = NIDaq.ZForce;
            ZMaxHistory = NIDaq.ZMaxHistory;
        }

        void GetNIDaqXYData()
        {
            LatLoc = NIDaqXY.LatLoc;
            TraLoc = NIDaqXY.TraLoc;
         }
    }
}
