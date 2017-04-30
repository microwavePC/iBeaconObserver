using System;
using Xamarin.Forms;

namespace iBeaconObserver.Models
{
	public class iBeacon
	{
		#region PROPERTIES

		/// <summary>
		/// iBeaconのUUID
		/// </summary>
		public Guid Uuid { get; set; }

		/// <summary>
		/// iBeaconのMAJOR値
		/// </summary>
		public ushort Major { get; set; }

		/// <summary>
		/// iBeaconのMINOR値
		/// </summary>
		public ushort Minor { get; set; }

		/// <summary>
		/// 測定された電波強度（RSSI）
		/// </summary>
		private short? _rssi;
		public short? Rssi
		{
			get { return _rssi; }
			set
			{
				_rssi = value;
				//calcEstimatedDistanceMeter();
			}
		}

		/// <summary>
		/// 発信電波強度（TxPower）
		/// </summary>
		private short? _txPower;
		public short? TxPower
		{
			get { return _txPower; }
			set
			{
				_txPower = value;
				//calcEstimatedDistanceMeter();
			}
		}

		/// <summary>
		/// iBeaconと端末の推定距離（単位はメートル）。
		/// [Android,UWPの場合]
		///   このプロパティに値を代入しても、その値は保存されない。
		///   RSSIとTxPowerを元に計算された推定距離が常にセットされる。
		///   RSSIとTxPowerの一方がnullの場合、この値はnullになる。
		/// </summary>
		private double? _estimatedDistanceMeter = null;
		public double? EstimatedDistanceMeter
		{
			get
			{
				if (Device.OS == TargetPlatform.iOS)
				{
					return _estimatedDistanceMeter;
				}
				else
				{
					return calcEstimatedDistanceMeter();
				}
			}
			set
			{
				if (Device.OS == TargetPlatform.iOS)
				{
					_estimatedDistanceMeter = value;
				}
				else
				{
					return;
				}
			}
		}

		#endregion



		#region PRIVATE METHODS

		private double? calcEstimatedDistanceMeter()
		{
			if (Device.OS == TargetPlatform.iOS)
			{
				return null;
			}

			if (Rssi == null || TxPower == null)
			{
				return null;
			}

			double distanceMeter = Math.Pow(10.0, ((double)TxPower - (double)Rssi) / 20.0);
			return distanceMeter;
		}

		#endregion
	}
}
