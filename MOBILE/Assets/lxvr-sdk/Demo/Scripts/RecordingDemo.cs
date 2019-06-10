using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;
using Looxid;
using System.Collections;

public class RecordingDemo : MonoBehaviour
{
    public enum Tutorial2Status
    {
        Intro,
        Image,
        Question1,
        Video,
        Question2,
        End
    }

    private LXVRManager lxvrManager;
    public GameObject StartButton;
    public GameObject Image;
    public GameObject VideoPlayer;
    public GameObject Question;
    public GameObject End;
    public GameObject DisconnectedPanel;

    public Sprite[] Images;

    private Tutorial2Status curState;
    private Tutorial2Status prevState;
    private float timer;
    private int curImageIndex;

    private Transform Option1;
    private Transform Option2;

    private bool isConnected;

    [Serializable]
    public class Answer
    {
        public int UserAnswer;
    }

    void Start()
    {
        Init();
        lxvrManager.StartBroadCasting();
        ChangeMode(Tutorial2Status.Intro);
    }

    private void Update()
    {
        prevState = curState;

        if (curState != Tutorial2Status.Image)
            return;

        timer += Time.deltaTime;

        if (timer <= 5f)
            return;

        lxvrManager.StopRecording((LXVRResult result) => OnRecordingStopCallback(result, curState));
    }

    private void Init() {
        lxvrManager = LXVRManager.Instance;
        lxvrManager.connectionStatusCallback = OnConnectStatusChanged;
        lxvrManager.eyeTrackerStatusCallback = OnEyeTrackerStatusChanged;
        lxvrManager.eegSensorStatusCallback = OnEEGSensorStatusChanged;
        lxvrManager.recordingStatusCallback = OnRecordingStatusChanged;

        curImageIndex = 0;
        timer = 0f;

        Option1 = Question.transform.GetChild(1);
        Option2 = Question.transform.GetChild(2);
    }

    public void OnConnectStatusChanged(ConnectionStatus connectionStatus)
    {
        Debug.Log("OnConnectStatusChanged : " + connectionStatus);
        switch (connectionStatus)
        {
            case ConnectionStatus.Connected:
                isConnected = true;
                break;
            case ConnectionStatus.Disconnected:
                DisconnectedPanel.SetActive(true);
                isConnected = false;
                break;
        }
    }

    public void OnEyeTrackerStatusChanged(EyeTrackerStatus eyeTrackerStatus)
    {
        Debug.Log("--> OnEyeTrackerStatusChanged : " + eyeTrackerStatus);
        // Do Something...
    }

    public void OnEEGSensorStatusChanged(EEGSensorStatus eegSensorStatus)
    {
        Debug.Log("--> OnEEGSensorStatusChanged : " + eegSensorStatus);
        // Do Something...
    }

    public void OnRecordingStatusChanged(RecordingStatus recordingStatus)
    {
        Debug.Log("--> OnRecordingStatusChanged : " + recordingStatus);
        // Do Something...
    }

    [EnumAction(typeof(Tutorial2Status))]
    public void ChangeMode(int status)
    {
        if (!isConnected)
            return;
        ChangeMode((Tutorial2Status)status);
    }

    public void ChangeMode(Tutorial2Status status)
    {
        if (!isConnected)
            return;

        DeactiveAll();
        curState = status;

        switch (status)
        {
            case Tutorial2Status.Intro:
                StartButton.SetActive(true);
                break;

            case Tutorial2Status.Image:
                Image.GetComponent<Image>().sprite = Images[curImageIndex];
                lxvrManager.StartRecording(curState.ToString(),
                                           Images[curImageIndex].name, false,
                                           (LXVRResult result) => OnRecordingStartCallback(result, Image));
                break;

            case Tutorial2Status.Question1:
                SetQuestion(1);
                lxvrManager.StartRecording(curState.ToString(),
                                          "1", false,
                                          (LXVRResult result) => OnRecordingStartCallback(result, Question));
                break;

            case Tutorial2Status.Video:
                VideoPlayer.GetComponent<VideoPlayer>().loopPointReached +=
                    (VideoPlaer) => lxvrManager.StopRecording((LXVRResult result) => OnRecordingStopCallback(result, curState));
                lxvrManager.StartRecording(curState.ToString(),
                                          VideoPlayer.GetComponent<VideoPlayer>().clip.name, false,
                                          (LXVRResult result) => OnRecordingStartCallback(result, VideoPlayer));
                VideoPlayer.SetActive(true);
                break;

            case Tutorial2Status.Question2:
                SetQuestion(2);
                lxvrManager.StartRecording(curState.ToString(),
                                          "1", false,
                                          (LXVRResult result) => OnRecordingStartCallback(result, Question));
                break;

            case Tutorial2Status.End:
                End.SetActive(true);
                break;

            default:
                break;
        }
    }

    private void DeactiveAll()
    {
        StartButton.SetActive(false);
        Image.SetActive(false);
        VideoPlayer.SetActive(false);
        Question.SetActive(false);
        End.SetActive(false);
        DisconnectedPanel.SetActive(false);
    }

    private void SetQuestion(int questionNo)
    {
        Text QuestionText = Question.transform.GetChild(0).GetComponent<Text>();
        Text Option1Text = Option1.GetChild(0).GetComponent<Text>();
        Text Option2Text = Option2.GetChild(0).GetComponent<Text>();

        Button Option1Button = Option1.GetComponent<Button>();
        Button Option2Button = Option2.GetComponent<Button>();

        QuestionText.text = "This is Question No." + questionNo;
        Option1Text.text = "N" + questionNo + "O1";
        Option2Text.text = "N" + questionNo + "O2";

        List<UnityAction> Option1Callbacks = new List<UnityAction> {
            () => lxvrManager.RecordEvent(JsonUtility.ToJson(new Answer {UserAnswer = 1}), 
                                          OnRecordingEventCallback)
        };

        List<UnityAction> Option2Callbacks = new List<UnityAction> {
            () => lxvrManager.RecordEvent(JsonUtility.ToJson(new Answer {UserAnswer = 2}),
                                          OnRecordingEventCallback)
        };

        AddOptionCallback(Option1Button, Option1Callbacks);
        AddOptionCallback(Option2Button, Option2Callbacks);
    }

    public void OpenImpedancePanel()
    {
        LXVRDelegate impedancePanelClosed = () =>
        {
            OpenETCalPanel();
        };

        bool isOpened = lxvrManager.OpenImpedancePanel(impedancePanelClosed);
        Debug.Log("OpenImpedancePanel: " + isOpened);
    }
    System.Collections.IEnumerator BackImpedancePanel()
    {
        yield return new WaitForSeconds(0.5f);
        OpenImpedancePanel();
    }

    public void ClosedETCalibrationPanel(UIPanelClosed closedStatus)
    {
        Debug.Log("ClosedETCalibrationPanel: " + closedStatus);
        switch (closedStatus)
        {
            case UIPanelClosed.Back:
                StartCoroutine(BackImpedancePanel());
                break;
            case UIPanelClosed.Done:
                ChangeMode(Tutorial2Status.Image);
                break;
            case UIPanelClosed.Disconnected:
                ChangeMode(Tutorial2Status.Intro);
                break;
        }
    }

    public void OpenETCalPanel()
    {
        lxvrManager.OpenEyeTrackerCalibrationPanel(false, ClosedETCalibrationPanel);
    }

    private void AddOptionCallback(Button button, List<UnityAction> callbacks)
    {
        button.onClick.RemoveAllListeners();
        callbacks.ForEach(button.onClick.AddListener);
    }

    private void OnRecordingStartCallback(LXVRResult result, GameObject obj)
    {
        if (result == LXVRResult.Success)
        {
            obj.SetActive(true);
        }
    }

    private void OnRecordingEventCallback(LXVRResult result)
    {
        if (result == LXVRResult.Success)
        {
            lxvrManager.StopRecording((LXVRResult recordingStopResult) =>
                                      {
                                          OnRecordingStopCallback(recordingStopResult,
                                                                  curState);
                                      });
        }
    }

    private void OnRecordingStopCallback(LXVRResult result, Tutorial2Status state)
    {
        if (result == LXVRResult.Success)
        {
            Debug.Log("OnRecordingStopCallback : " + result);
            switch (state)
            {
                case Tutorial2Status.Image:

                    if (curImageIndex == Images.Length - 1)
                    {
                        ChangeMode(Tutorial2Status.Question1);
                        return;
                    }

                    curImageIndex++;
                    timer = 0f;
                    ChangeMode(Tutorial2Status.Image);
                    break;

                case Tutorial2Status.Video:
                    ChangeMode(Tutorial2Status.Question2);
                    break;

                case Tutorial2Status.Question1:
                    ChangeMode(Tutorial2Status.Video);
                    break;

                case Tutorial2Status.Question2:
                    ChangeMode(Tutorial2Status.End);
                    break;
            }
        }
    }

    public void OnAppCloseButton()
    {
        Application.Quit();
    }

    public void OnAppDontCloseButton()
    {
        DisconnectedPanel.SetActive(false);
        lxvrManager.StartBroadCasting();
    }
}