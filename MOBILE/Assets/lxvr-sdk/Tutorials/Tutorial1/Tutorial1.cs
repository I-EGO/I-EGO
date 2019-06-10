using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;
using Looxid;

public class Tutorial1 : MonoBehaviour
{
    LXVRManager lxvrManager;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        lxvrManager = LXVRManager.Instance;
        lxvrManager.connectionStatusCallback = OnConnectStatusCallback;
        lxvrManager.lxvrStatusChangedCallback = OnStatusChanged;
    }

    public void OnConnectStatusCallback(ConnectionStatus connectionStatus)
    {
        Debug.Log("OnConnectStatusCallback : " + connectionStatus);

        // Do something...
    }

    public void OnStatusChanged(LXVRStatus status)
    {
        Debug.Log("OnStatusChanged");
        Debug.Log("------> coreStatus " + status.coreStatus);
        Debug.Log("------> eegSensor " + status.eegSensorStatus);
        Debug.Log("------> eyeCamera " + status.eyeCameraStatus);

        // Do something...
    }

    public void OpenDeviceStatusPanel()
    {
        bool isOpened = lxvrManager.OpenDeviceStatusPanel();
        Debug.Log("OpenDeviceStatusPanel : " + isOpened);
    }

    public void OpenEyeCalibration()
    {
        LXVRDelegate eyeTrackingCalibrationPanelClosed = () =>
        {
            Debug.Log("eyeTrackingCalibrationPanelClosed");
        };

        bool isOpened = lxvrManager.OpenEyeTrackingCalibrationPanel(false, 
                                                                    eyeTrackingCalibrationPanelClosed);
        Debug.Log("OpenEyeCalibration : " + isOpened);
    }
}