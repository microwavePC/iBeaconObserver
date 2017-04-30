# iBeacon handling as a trigger (BeaHat) #

### 概要

iBeacon検出用のクラスは、「iBeaconEventTriggerService」。
これを共通プロジェクトのViewModelで利用する。

### メソッド・プロパティ

##### メソッド

* **AddEvent**

  検出対象のiBeaconと検出時に実行したい処理をサービスに登録する。
  iBeaconのみを登録することもできる。その場合、検出時には検出ビーコン一覧（DetectedBeaconList）への登録だけ行われる。

* **ClearEvent**

  AddEventで登録した情報を全て消去し、初期化する。

* **StartScan**

  iBeaconのスキャンを開始する。検出ビーコン一覧（DetectedBeaconList）は初期化する。

* **StopScan**

  iBeaconのスキャンを停止する。

##### プロパティ

* **IsScanning**

  スキャン中はtrue、スキャン停止中はfalseを返す。

* **DetectedBeaconList**

  直近のスキャン、あるいは実行中のスキャンで検出されたiBeaconの情報を持つリスト。1種類の検出iBeaconにつき、1つの要素を持つ。同じiBeaconを複数回検出した場合、最も近付いたときの情報のみがこのリストに残される。


### 使い方

1. iBeaconEventTriggerServiceのインスタンスを作る。

2. メソッドAddEventで、検出対象のiBeacon（と、検出時に実行させたい処理があるなら、その処理）を登録する。

3. メソッドStartScanで、iBeaconの検出を開始する。

4. 検出できたiBeaconの近接状況は、DetectedBeaconListを参照することで把握できる。

5. 検出を停止したくなったら、メソッドStopScanを実行する。
