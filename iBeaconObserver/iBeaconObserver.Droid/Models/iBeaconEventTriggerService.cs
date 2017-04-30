using Android.Bluetooth;
using Android.Bluetooth.LE;
using iBeaconObserver.Droid.Utils;
using iBeaconObserver.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace iBeaconObserver.Droid.Models
{
    public class iBeaconEventTriggerService : BindableBase, IiBeaconEventTriggerService
    {
        #region PRERTIES

        private bool _isScanning = false;
        public bool IsScanning
        {
            get { return _isScanning; }
            set { SetProperty(ref _isScanning, value); }
        }

        public List<iBeacon> DetectedBeaconList
        {
            get { return new List<iBeacon>(_scanCallback.DetectedBeaconDict.Values); }
        }

        #endregion



        #region FIELDS

        private BluetoothManager _btManager;
        private BluetoothAdapter _btAdapter;
        private BluetoothLeScanner _bleScanner;
        private BleScanCallback _scanCallback;

        #endregion



        #region CONSTRUCTOR

        public iBeaconEventTriggerService()
        {
            _btManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            _btAdapter = _btManager.Adapter;
            _bleScanner = _btAdapter.BluetoothLeScanner;
            _scanCallback = new BleScanCallback();
        }

        #endregion



        #region PUBLIC METHODS

        public void AddEvent(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action function)
        {
            //TODO: 非同期メソッドや引数ありのメソッドもセットできるようにしたい
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_scanCallback.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _scanCallback.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
            _scanCallback.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
        }


        public void AddEvent(Guid uuid, ushort major, ushort minor)
        {
            //TODO: 非同期メソッドや引数ありのメソッドもセットできるようにしたい
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_scanCallback.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _scanCallback.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
        }


        public void ClearAllEvent()
        {
            _scanCallback.BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            _scanCallback.DetectedBeaconDict = new Dictionary<string, iBeacon>();
        }


        public void StartScan()
        {
            if (IsScanning)
            {
                return;
            }

            _scanCallback.DetectedBeaconDict = new Dictionary<string, iBeacon>();
            _bleScanner.StartScan(_scanCallback);
            IsScanning = true;
        }


        public void StopScan()
        {
            if (!IsScanning)
            {
                return;
            }

            _bleScanner.StopScan(_scanCallback);
            IsScanning = false;
        }

        #endregion
    }


    class BleScanCallback : ScanCallback
    {
        #region PROPERTIES

        public Dictionary<string, iBeaconEventHolder> BeaconEventHolderDict { get; set; }
        public Dictionary<string, iBeacon> DetectedBeaconDict { get; set; }

        #endregion



        #region CONSTRUCTOR

        public BleScanCallback()
        {
            BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            DetectedBeaconDict = new Dictionary<string, iBeacon>();
        }

        #endregion



        public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            if (iBeaconDroidUtility.IsIBeacon(result.ScanRecord))
            {
                Guid uuid = iBeaconDroidUtility.GetUuidFromRecord(result.ScanRecord);
                ushort major = iBeaconDroidUtility.GetMajorFromRecord(result.ScanRecord);
                ushort minor = iBeaconDroidUtility.GetMinorFromRecord(result.ScanRecord);

                string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, major, minor);

                if (!BeaconEventHolderDict.ContainsKey(beaconIdentifier))
                {
                    return;
                }

                iBeaconEventHolder eventHolder = BeaconEventHolderDict[beaconIdentifier];

                if (DetectedBeaconDict.ContainsKey(beaconIdentifier))
                {
                    iBeacon detectedBeaconPrev = DetectedBeaconDict[beaconIdentifier];
                    short? rssiPrev = detectedBeaconPrev.Rssi;

                    if (rssiPrev == null || ((short)rssiPrev < result.Rssi))
                    {
                        eventHolder.ibeacon.Rssi = (short)result.Rssi;
                        DetectedBeaconDict[beaconIdentifier] = eventHolder.ibeacon;
                    }
                }
                else
                {
                    eventHolder.ibeacon.Rssi = (short)result.Rssi;
					eventHolder.ibeacon.TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord);
                    DetectedBeaconDict.Add(beaconIdentifier, eventHolder.ibeacon);
                }

                foreach (var eventDetail in eventHolder.EventList)
                {
                    if (eventDetail.ThresholdRssi < result.Rssi &&
                        eventDetail.LastTriggeredDateTime < DateTime.Now.AddMilliseconds(-1 * eventDetail.EventTriggerIntervalMilliSec))
                    {
                        eventDetail.LastTriggeredDateTime = DateTime.Now;
                        eventDetail.Function();
                    }
                }
            }
        }
    }
}