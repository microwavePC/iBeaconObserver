using Android.Bluetooth;
using Android.Bluetooth.LE;
using iBeaconObserver.Droid.Utils;
using iBeaconObserver.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using Android.OS;

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
			get
			{
				if (OS_VER < BuildVersionCodes.Lollipop)
				{
					return new List<iBeacon>(_scanCallbackOld.DetectedBeaconDict.Values);
				}
				else
				{
					return new List<iBeacon>(_scanCallbackNew.DetectedBeaconDict.Values);
				}
			}
		}

		#endregion



		#region FIELDS

		private BluetoothManager _btManager;
		private BluetoothAdapter _btAdapter;
		private BluetoothLeScanner _bleScanner;
		private BleScanCallback _scanCallbackNew;
		private LeScanCallback _scanCallbackOld;

		private readonly BuildVersionCodes OS_VER;

		#endregion



		#region CONSTRUCTOR

		public iBeaconEventTriggerService()
		{
			/*
            _btManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            _btAdapter = _btManager.Adapter;
            _bleScanner = _btAdapter.BluetoothLeScanner;
            _scanCallback = new BleScanCallback();
            */

			// OSバージョンを取得
			OS_VER = Build.VERSION.SdkInt;

			_btManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
			_btAdapter = _btManager.Adapter;

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				// 4.4以下はこちら
				_scanCallbackOld = new LeScanCallback();
			}
			else
			{
				// 5.0以上はこちら
				_scanCallbackNew = new BleScanCallback();
				_bleScanner = _btAdapter?.BluetoothLeScanner;
			}
		}

		#endregion



		#region PUBLIC METHODS

		public void AddEvent(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action function)
		{
			iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				if (!_scanCallbackOld.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
				{
					_scanCallbackOld.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
				}
				_scanCallbackOld.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
			}
			else
			{
				if (!_scanCallbackNew.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
				{
					_scanCallbackNew.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
				}
				_scanCallbackNew.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
			}
		}


		public void AddEvent(Guid uuid, ushort major, ushort minor)
		{
			iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				if (!_scanCallbackOld.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
				{
					_scanCallbackOld.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
				}
			}
			else
			{
				if (!_scanCallbackNew.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
				{
					_scanCallbackNew.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
				}
			}
		}


		public void ClearAllEvent()
		{
			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				_scanCallbackOld.BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
				_scanCallbackOld.DetectedBeaconDict = new Dictionary<string, iBeacon>();
			}
			else
			{
				_scanCallbackNew.BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
				_scanCallbackNew.DetectedBeaconDict = new Dictionary<string, iBeacon>();
			}
		}


		public void StartScan()
		{
			if (IsScanning)
			{
				return;
			}

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				_scanCallbackOld.DetectedBeaconDict = new Dictionary<string, iBeacon>();
				_btAdapter?.StartLeScan(_scanCallbackOld);
			}
			else
			{
				_scanCallbackNew.DetectedBeaconDict = new Dictionary<string, iBeacon>();
				_bleScanner.StartScan(_scanCallbackNew);
			}

			IsScanning = true;
		}


		public void StopScan()
		{
			if (!IsScanning)
			{
				return;
			}

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				_btAdapter.StopLeScan(_scanCallbackOld);
			}
			else
			{
				_bleScanner.StopScan(_scanCallbackNew);
			}

			IsScanning = false;
		}

		#endregion
	}


	#region CALLBACK FOR ANDROID VER 4.4

	class LeScanCallback : Java.Lang.Object, BluetoothAdapter.ILeScanCallback
	{
		
		#region PROPERTIES

		public Dictionary<string, iBeaconEventHolder> BeaconEventHolderDict { get; set; }
		public Dictionary<string, iBeacon> DetectedBeaconDict { get; set; }

		#endregion


		#region CONSTRUCTOR

		public LeScanCallback()
		{
			BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
			DetectedBeaconDict = new Dictionary<string, iBeacon>();
		}

		#endregion


		public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
		{
			if (iBeaconDroidUtility.IsIBeacon(scanRecord))
			{
				Guid uuid = iBeaconDroidUtility.GetUuidFromRecord(scanRecord);
				ushort major = iBeaconDroidUtility.GetMajorFromRecord(scanRecord);
				ushort minor = iBeaconDroidUtility.GetMinorFromRecord(scanRecord);

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

					if (rssiPrev == null || ((short)rssiPrev < rssi))
					{
						eventHolder.ibeacon.Rssi = (short)rssi;
						DetectedBeaconDict[beaconIdentifier] = eventHolder.ibeacon;
					}
				}
				else
				{
					eventHolder.ibeacon.Rssi = (short)rssi;
					eventHolder.ibeacon.TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(scanRecord);
					DetectedBeaconDict.Add(beaconIdentifier, eventHolder.ibeacon);
				}

				foreach (var eventDetail in eventHolder.EventList)
				{
					if (eventDetail.ThresholdRssi < rssi &&
						eventDetail.LastTriggeredDateTime < DateTime.Now.AddMilliseconds(-1 * eventDetail.EventTriggerIntervalMilliSec))
					{
						eventDetail.LastTriggeredDateTime = DateTime.Now;
						eventDetail.Function();
					}
				}
			}
		}
	}

	#endregion


	#region CALLBACK FOR ANDROID VER 5.0 AND OVER

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

	#endregion
}