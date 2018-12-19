using System;
namespace MysteryBoxCleanUp
{
    public interface NIDaqInt
    {

        double[] ReadUSBData();

        void Setup_USB6008();

        void ConvertData(double[] data);
    }
}
