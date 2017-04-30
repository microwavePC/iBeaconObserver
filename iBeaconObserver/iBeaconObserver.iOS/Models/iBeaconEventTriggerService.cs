using CoreLocation;
using Foundation;
using iBeaconObserver.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using UIKit;

namespace iBeaconObserver.iOS.Models
{
    public class iBeaconEventTriggerService : BindableBase, IiBeaconEventTriggerService
    {
        #region PROPERTIES

        private bool _isScanning = false;
        public bool IsScanning
        {
            get { return _isScanning; }
            set { SetProperty(ref _isScanning, value); }
        }

        public List<iBeacon> DetectedBeaconList
        {
            get { return new List<iBeacon>(_detectedBeaconDict.Values); }
        }

        #endregion



        #region FIELDS

        private Dictionary<string, iBeaconEventHolder> _beaconEventHolderDict;
        private Dictionary<string, iBeacon> _detectedBeaconDict;
        private CLLocationManager _locationManager;

        #endregion



        #region CONSTRUCTOR

        public iBeaconEventTriggerService()
        {
            _beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            _detectedBeaconDict = new Dictionary<string, iBeacon>();
            _locationManager = new CLLocationManager();

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _locationManager.RequestWhenInUseAuthorization();
            }
        }

        #endregion



        #region PUBLIC METHODS

        public void AddEvent(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action func)
        {
            //TODO: 非同期メソッドや引数ありのメソッドもセットできるようにしたい
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }

            _beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, func);

            if (IsScanning)
            {
                var nsUuid = new NSUuid(uuid.ToString());
                var beaconRegion = new CLBeaconRegion(nsUuid, major, minor, eventHolder.BeaconIdentifyStr);
                _locationManager.StartRangingBeacons(beaconRegion);
            }
        }


        public void AddEvent(Guid uuid, ushort major, ushort minor)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }

            if (IsScanning)
            {
                var nsUuid = new NSUuid(uuid.ToString());
                var beaconRegion = new CLBeaconRegion(nsUuid, major, minor, eventHolder.BeaconIdentifyStr);
                _locationManager.StartRangingBeacons(beaconRegion);
            }
        }


        public void ClearAllEvent()
        {
            _beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            _detectedBeaconDict = new Dictionary<string, iBeacon>();
        }


        public void StartScan()
        {
            if (IsScanning)
            {
                return;
            }

            _detectedBeaconDict = new Dictionary<string, iBeacon>();
            _locationManager.DidRangeBeacons += didRangeBeacons;

            foreach (var eventHolder in _beaconEventHolderDict)
            {
                var uuid = new NSUuid(eventHolder.Value.ibeacon.Uuid.ToString());
                var beaconRegion = new CLBeaconRegion(uuid,
                                                      eventHolder.Value.ibeacon.Major,
                                                      eventHolder.Value.ibeacon.Minor,
                                                      eventHolder.Value.BeaconIdentifyStr);

                _locationManager.StartRangingBeacons(beaconRegion);
            }

            IsScanning = true;
        }


        public void StopScan()
        {
            if (!IsScanning)
            {
                return;
            }

            foreach (var eventHolder in _beaconEventHolderDict)
            {
                var uuid = new NSUuid(eventHolder.Value.ibeacon.Uuid.ToString());
                var beaconRegion = new CLBeaconRegion(uuid,
                                                      eventHolder.Value.ibeacon.Major,
                                                      eventHolder.Value.ibeacon.Minor,
                                                      eventHolder.Value.BeaconIdentifyStr);

                _locationManager.StopRangingBeacons(beaconRegion);
            }

            IsScanning = false;
        }

        #endregion



        #region PRIVATE METHODS

        private void didRangeBeacons(object s, CLRegionBeaconsRangedEventArgs e)
        {
            foreach (var detectedBeacon in e.Beacons)
            {
                string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(
                    new Guid(detectedBeacon.ProximityUuid.ToString()),
                    detectedBeacon.Major.UInt16Value,
                    detectedBeacon.Minor.UInt16Value);

                if (!_beaconEventHolderDict.ContainsKey(beaconIdentifier))
                {
                    return;
                }

                iBeaconEventHolder eventHolder = _beaconEventHolderDict[beaconIdentifier];

                if (_detectedBeaconDict.ContainsKey(beaconIdentifier))
                {
                    iBeacon detectedBeaconPrev = _detectedBeaconDict[beaconIdentifier];
                    short? rssiPrev = detectedBeaconPrev.Rssi;

                    if (rssiPrev == null || ((short)rssiPrev < detectedBeacon.Rssi))
                    {
                        eventHolder.ibeacon.Rssi = (short)detectedBeacon.Rssi;
						eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.Accuracy;
                        _detectedBeaconDict[beaconIdentifier] = eventHolder.ibeacon;
                    }
                }
                else
                {
                    eventHolder.ibeacon.Rssi = (short)detectedBeacon.Rssi;
					eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.Accuracy;
                    _detectedBeaconDict.Add(beaconIdentifier, eventHolder.ibeacon);
                }

                foreach (iBeaconEventDetail eventDetail in eventHolder.EventList)
                {
                    if (eventDetail.ThresholdRssi < detectedBeacon.Rssi &&
                        eventDetail.LastTriggeredDateTime < DateTime.Now.AddMilliseconds(-1 * eventDetail.EventTriggerIntervalMilliSec))
                    {
                        eventDetail.LastTriggeredDateTime = DateTime.Now;
                        eventDetail.Function();
                    }
                }
            }
        }

        #endregion
    }
}
