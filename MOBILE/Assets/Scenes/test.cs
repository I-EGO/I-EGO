using UnityEngine;
using UnityEngine.UI;
using Looxid;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System;
namespace GoogleVR.VideoDemo
{
    public class test : MonoBehaviour
    {
        /*
         * 변수 선언부
         * gameobjects
         * threads
         * status
         * data (null)
         */
        private LXVRManager lxvrManager;
        private bool isConnected;
        public Image image;
        public GameObject loading;
        public GameObject result;
        public GameObject odd;
        public GameObject hemi;
        public GameObject sphere;
        private string model;
        private string str;
        private string state;
        private Thread t1;
        private Thread t2;
        private bool TF;
        private bool TF1;
        private byte[] Data = null;
        
        //looxidVR Manager 초기화
        public static test instance = null;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }
        public void Init()
        {
            lxvrManager = LXVRManager.Instance;
            model = SystemInfo.deviceModel;
            TF = true;
            TF1 = true;
        }
        private void Start()
        {
            t1 = new Thread(new ThreadStart(TCP));
            t2 = new Thread(new ThreadStart(UDP));
            UI.instance.setparam1(-1);
            UI.instance.setparam2(-1);
            UI.instance.setstatus(UI.testStatus.Intro);
        }
        private void OnDisable()
        {
            UI.instance.setparam1(-1);
            UI.instance.setparam2(-1);
            UI.instance.setstatus(UI.testStatus.End);
            Debug.Log("disable");
        }
        private void Update()
        {
        }
        /*
         * UDP에서 PC와 연결 확인되면 TCP실행
         */
        private void TCP()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 7001);
            listener.Start();
            byte[] buff = new byte[1000000];
            byte[] D = null; //buff로 받아서 D에서 통합
            int size = 0; //데이터의 크기
            int count = 0; //버퍼의 개수
            int param1 = -1;
            int param2 = -1;
            bool toggle = false; //data 전송중인지 확인
            string name = null;
            while (TF1)
            {
                TcpClient tc = listener.AcceptTcpClient();
                NetworkStream stream = tc.GetStream();
                int n;
                
                while ((n = stream.Read(buff, 0, buff.Length)) > 0)
                {
                    string data = Encoding.UTF8.GetString(buff, 0, n);
                    Debug.Log(data);

                    string[] command = data.Split(' ');
                    //데이터 수신 시작
                    if (command[0].Equals("data"))
                    {
                        Debug.Log("1");
                        name = command[1];
                        size = Convert.ToInt32(command[4]);
                        param1 = Convert.ToInt32(command[2]);
                        param2 = Convert.ToInt32(command[3]);
                        D = new byte[size];
                        Debug.Log(size);
                        toggle = true;
                        UI.instance.setdatasize(size);
                        UI.instance.setparam1(-1);
                        UI.instance.setparam2(-1);
                        UI.instance.setstatus(UI.testStatus.Loading);
                    }
                    //뇌파 녹화 시작
                    else if (command[0].Equals("start"))
                    {
                        lxvrManager.StartRecording(UI.instance.getstatus().ToString(),
                                           name, 
                                           false,
                                           (LXVRResult result) => OnRecordingStartCallback(result));
                    }
                    //뇌파 녹화 중지
                    else if (command[0].Equals("stop"))
                    {
                        lxvrManager.StopRecording((LXVRResult result) => OnRecordingStopCallback(result));
                    }
                    //분석 로딩
                    else if (command[0].Equals("loading"))
                    {
                        /*
                         * 계획 : UIclass에 결과 전송
                         */
                        UI.instance.setparam1(-1);
                        UI.instance.setparam2(-1);
                        UI.instance.setstatus(UI.testStatus.Loading);
                    }
                    //결과 수신
                    else if (command[0].Equals("result"))
                    {
                        /*
                         * 계획 : UIclass에 결과 전송
                         */
                        UI.instance.setparam1(-1);
                        UI.instance.setparam2(-1);
                        UI.instance.setstatus(UI.testStatus.Result);
                    }
                    //데이터 수신 중지
                    else if (command[0].Equals("end"))
                    {
                        Debug.Log(name);
                        toggle = false;
                        size = 0;
                        count = 0;
                        Data = D;
                        string[] file = name.Split('.');
                        Debug.Log(name);
                        Debug.Log(file);
                        Debug.Log(file[file.Length - 1]);
                        if (file[file.Length - 1].Equals("jpg"))
                        {
                            UI.instance.setparam1(param1);
                            UI.instance.setparam2(param2);
                            UI.instance.setstatus(UI.testStatus.Image);
                        }
                        else if (file[file.Length - 1].Equals("mp4"))
                        {
                            UI.instance.setparam1(param1);
                            UI.instance.setparam2(param2);
                            UI.instance.setstatus(UI.testStatus.Video);
                        }
                        param1 = -1;
                        param2 = -1;
                        UI.instance.setdatasize(0);
                        UI.instance.setloaded(0);
                        UI.instance.setprogress(0);
                    }
                    //데이터 수신
                    else if (toggle)
                    {
                        Debug.Log(toggle);
                        for (int i = 0; i < n; i++)
                        {
                            D[count] = buff[i];
                            count++;
                        }
                        UI.instance.setloaded(count);
                        int pro = count/(size / 100);
                        UI.instance.setprogress(pro);
                    }
                }
                stream.Close();
                tc.Close();
            }
        }
        /*
         * 프로그램 시작 시 브로드캐스트 수신대기
         * 브로드캐스트 받으면 자신의 IP전송
         * 자신의 IP를 받으면 TCP연결 시작
         */
        private void UDP()
        {
            UdpClient udp = new UdpClient(new IPEndPoint(IPAddress.Any, 8000));
            try
            {
                udp.EnableBroadcast = true;
                IPEndPoint iep = new IPEndPoint(IPAddress.Any, 0);
                IPEndPoint broad = new IPEndPoint(IPAddress.Broadcast, 8002);
                while (TF)
                {
                    byte[] data = udp.Receive(ref iep);
                    str = Encoding.Default.GetString(data);
                    Debug.Log(iep);
                    Debug.Log(str);
                    //브로드 캐스트 수신
                    if (Equals("broadcast", str))
                    {
                        state = "broad";
                        Debug.Log(state);
                        byte[] sdata = Encoding.UTF8.GetBytes(model);
                        udp.Send(sdata, sdata.Length, broad);
                        Debug.Log(sdata);
                        state = "send: " + sdata;
                    }
                    //자신의 IP 수신
                    else if (Equals(model, str))
                    {
                        state = "model";
                        UI.instance.setparam1(-1);
                        UI.instance.setparam2(-1);
                        UI.instance.setstatus(UI.testStatus.Connect);  
                    }
                    //종료 매세지 수신
                    else if (Equals("abort", str))
                    {
                        state = "abort";
                        TF1 = false;
                        t1.Join();
                    }
                    //기타 : 다른 기기의 IP 수신
                    else
                    {
                        state = "else";
                    }
                    Debug.Log(state);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            finally
            {
                udp.Close();
                t2.Join();
            }
        }

        /*
         * LooxidVR Manager 관련 함수들
         */
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

        private void OnRecordingStartCallback(LXVRResult result)
        {
            if (result == LXVRResult.Success)
            {
            }
        }

        private void OnRecordingEventCallback(LXVRResult result)
        {
            if (result == LXVRResult.Success)
            {
            }
        }

        private void OnRecordingStopCallback(LXVRResult result)
        {
            if (result == LXVRResult.Success)
            {
                Debug.Log("OnRecordingStopCallback : " + result);
            }
        }
        LXVRDelegate resultSuccessCallback = () =>
        {
            Debug.Log("Success!");
        };
        LXVRDelegate resultFailCallback = () =>
        {
            Debug.Log("Fail!");
        };
        public void startT1()
        {
            if (t1.IsAlive)
            {
                TF1 = false;
                t1.Join();
            }
            TF1 = true;
            t1 = new Thread(new ThreadStart(TCP));
            t1.Start();
        }
        public void startT2()
        {
            if (t2.IsAlive)
            {
                TF = false;
                t2.Join();
            }
            TF = true;
            t2 = new Thread(new ThreadStart(UDP));
            t2.Start();
        }
        public void joinT1()
        {
            TF1 = false;
            t1.Join();
        }
        public void joinT2()
        {
            TF = false;
            t2.Join();
        }
        public byte[] getData()
        {
            return Data;
        }
        public void setData(byte[] d)
        {
            Data = d;
        }
    }
}