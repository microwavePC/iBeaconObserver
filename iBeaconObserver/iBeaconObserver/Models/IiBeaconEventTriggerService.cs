using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace iBeaconObserver.Models
{
    public interface IiBeaconEventTriggerService : INotifyPropertyChanged
    {
        bool IsScanning { get; }

        List<iBeacon> DetectedBeaconList { get; }

        void AddEvent(Guid uuid,
                      ushort major,
                      ushort minor,
                      short thresholdRssi,
                      int intervalMilliSec,
                      Action function);

        void AddEvent(Guid uuid,
                      ushort major,
                      ushort minor);

        void ClearAllEvent();

        void StartScan();

        void StopScan();
    }
}
