using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;
using Looxid;

public class Tutorial2 : MonoBehaviour
{

    private LXVRManager lxvrManager;
    public GameObject StartButton;
    public GameObject Image;
    public GameObject VideoPlayer;
    public GameObject Question;
    public GameObject End;

    public Sprite[] Images;

    private Tutorial2Status curState;
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

    public enum Tutorial2Status
    {
        Intro,
        Image,
        Question1,
        Video,
        Question2,
        End,
    }

    void Start()
    {
        Init();
        ChangeMode(Tutorial2Status.Intro);
    }

    private void Update()
    {
        if (curState != Tutorial2Status.Image)
            return;
        timer += Time.deltaTime;

        if (timer <= 5f)
            return;

        lxvrManager.RecordingStop(curState.ToString(),
                                 Images[curImageIndex].name,
                                 () => OnRecordingStopCallback(curState));
    }

    private void Init() {
        lxvrManager = LXVRManager.Instance;
        lxvrManager.connectionStatusCallback = OnConnectStatusCallback;
        lxvrManager.lxvrStatusChangedCallback = OnStatusChanged;

        Images = Resources.LoadAll<Sprite>("TestImages");
        curImageIndex = 0;
        timer = 0f;

        Option1 = Question.transform.GetChild(1);
        Option2 = Question.transform.GetChild(2);
    }

    public void OnConnectStatusCallback(ConnectionStatus connectionStatus)
    {
        Debug.Log("OnConnectStatusCallback : " + connectionStatus);
        switch (connectionStatus)
        {
            case ConnectionStatus.Connected:
                isConnected = true;
                break;
            case ConnectionStatus.Disconnected:
                isConnected = false;
                break;
        }
    }

    public void OnStatusChanged(LXVRStatus status)
    {
        Debug.Log("OnStatusChanged");
        Debug.Log("------> coreStatus " + status.coreStatus);
        Debug.Log("------> eegSensor " + status.eegSensorStatus);
        Debug.Log("------> eyeCamera " + status.eyeCameraStatus);

        // Do something...
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
                lxvrManager.RecordingStart(curState.ToString(),
                                           Images[curImageIndex].name, false,
                                          () => OnRecordingStartCallback(Image));
                break;

            case Tutorial2Status.Question1:
                SetQuestion(1);
                lxvrManager.RecordingStart(curState.ToString(),
                                          "1", false,
                                          () => OnRecordingStartCallback(Question));
                break;

            case Tutorial2Status.Video:
                VideoPlayer.GetComponent<VideoPlayer>().loopPointReached +=
                    (VideoPlaer) => lxvrManager.RecordingStop(curState.ToString(),
                                                             VideoPlayer.GetComponent<VideoPlayer>().clip.name,
                                                             () => OnRecordingStopCallback(curState));
                lxvrManager.RecordingStart(curState.ToString(),
                                          VideoPlayer.GetComponent<VideoPlayer>().clip.name, false,
                                          () => OnRecordingStartCallback(VideoPlayer));
                VideoPlayer.SetActive(true);
                break;

            case Tutorial2Status.Question2:
                SetQuestion(2);
                lxvrManager.RecordingStart(curState.ToString(),
                                          "1", false,
                                          () => OnRecordingStartCallback(Question));
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
            () => lxvrManager.RecordingEvent(curState.ToString(), "1", JsonUtility.ToJson(new Answer {UserAnswer = 1}),
                                          () => OnSendEventCallback(
                                                () => lxvrManager.RecordingStop(curState.ToString(), 
                                                                               "1",
                                                                               () => OnRecordingStopCallback(curState))))
        };

        List<UnityAction> Option2Callbacks = new List<UnityAction> {
            () => lxvrManager.RecordingEvent(curState.ToString(), "1", JsonUtility.ToJson(new Answer {UserAnswer = 2}),
                                          () => OnSendEventCallback(
                                                () => lxvrManager.RecordingStop(curState.ToString(), 
                                                                               "1",
                                                                               () => OnRecordingStopCallback(curState))))
        };

        AddOptionCallback(Option1Button, Option1Callbacks);
        AddOptionCallback(Option2Button, Option2Callbacks);

    }

    public void OpenETCalPanel() {
        lxvrManager.OpenEyeTrackingCalibrationPanel(false, 
                                                    () => ChangeMode(Tutorial2Status.Image));
    }

    private void AddOptionCallback(Button button, List<UnityAction> callbacks)
    {
        button.onClick.RemoveAllListeners();
        callbacks.ForEach(button.onClick.AddListener);
    }

    private void OnRecordingStartCallback(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnRecordingStopCallback(Tutorial2Status state)
    {
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

    private void OnSendEventCallback(UnityAction callback)
    {
        callback();
    }
}