using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace iBeaconObserver.Models
{
    public interface IiBeaconEventTriggerService : INotifyPropertyChanged
    {
        bool IsScanning { get; }

        List<iBeacon> DetectedBeaconList { get; }

		/// <summary>
		/// 端末がBluetoothに対応しているかどうかを取得します。
		/// </summary>
		/// <returns><c>true</c>対応<c>false</c>非対応</returns>
		bool BluetoothIsAvailableOnThisDevice();

		/// <summary>
		/// 端末のBluetooth機能がオンにされているかどうかを取得します。
		/// 端末がBluetooth機能をオンにしていない場合、falseを返します。
		/// また、端末がBluetoothをサポートしていない場合もfalseを返します。
		/// </summary>
		/// <returns><c>true</c>オン<c>false</c>オフ</returns>
		bool BluetoothIsEnableOnThisDevice();

		/// <summary>
		/// 端末のBluetooth機能をオンにするためのダイアログを表示します。
		/// UWPには対応する機能がないため、何もしません。
		/// 端末がBluetoothに対応していない場合、例外BluetoothUnsupportedExceptionをthrowします。
		/// </summary>
		void RequestUserToTurnOnBluetooth();

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
