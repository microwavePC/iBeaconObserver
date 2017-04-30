using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth.LE;

namespace iBeaconObserver.Droid.Utils
{
    public static class iBeaconDroidUtility
    {
        public static bool IsIBeacon(ScanRecord scanRecord)
        {
            byte[] recordByteData = scanRecord.GetBytes();
            if (recordByteData.Length > 30 &&
                recordByteData[5] == 0x4c &&
                recordByteData[6] == 0x00 &&
                recordByteData[7] == 0x02 &&
                recordByteData[8] == 0x15)
            {
                return true;
            }

            return false;
        }


        public static Guid GetUuidFromRecord(ScanRecord scanRecord)
        {
            byte[] recordByteData = scanRecord.GetBytes();
            string uuidStr = BitConverter.ToString(recordByteData, 9, 4).Replace("-", "") + "-" +
                             BitConverter.ToString(recordByteData, 13, 2).Replace("-", "") + "-" +
                             BitConverter.ToString(recordByteData, 15, 2).Replace("-", "") + "-" +
                             BitConverter.ToString(recordByteData, 17, 2).Replace("-", "") + "-" +
                             BitConverter.ToString(recordByteData, 19, 6).Replace("-", "");

            return new Guid(uuidStr);
        }


        public static ushort GetMajorFromRecord(ScanRecord scanRecord)
        {
            byte[] recordByteData = scanRecord.GetBytes();
            string majorStr = BitConverter.ToString(recordByteData, 25, 2).Replace("-", "");
            ushort major = Convert.ToUInt16(majorStr, 16);

            return major;
        }


        public static ushort GetMinorFromRecord(ScanRecord scanRecord)
        {
            byte[] recordByteData = scanRecord.GetBytes();
            string minorStr = BitConverter.ToString(recordByteData, 27, 2).Replace("-", "");
            ushort minor = Convert.ToUInt16(minorStr, 16);

            return minor;
        }


		public static short GetTxPowerFromRecord(ScanRecord scanRecord)
		{
            byte[] recordByteData = scanRecord.GetBytes();
			string txPowerStr = BitConverter.ToString(recordByteData, 29 , 1);
			short txPower = Convert.ToSByte(txPowerStr, 16);

            return txPower;
		}
    }
}