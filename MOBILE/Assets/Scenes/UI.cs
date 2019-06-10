using UnityEngine;
using UnityEngine.UI;
using Looxid;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace GoogleVR.VideoDemo
{
    public class UI : MonoBehaviour
    {
        public Text loading;
        public Text rN, rE, rC, rA, rO;
        public GameObject goN, goE, goC, goA, goO;
        private int N = 0, E = 0, O = 0, C = 0, A = 0;
        private testStatus curState = 0;
        private testStatus changedState = 0;
        private bool tf = true;
        private int param1;
        private int param2;
        private int datasize = 0;
        private int loaded = 0;
        private int progress = 0;
        private Queue<byte[]> queue= new Queue<byte[]>();
        private Queue<int> queue1 = new Queue<int>();
        private Queue<int> queue2 = new Queue<int>();
        /*
         * 스테이터스 종류
         * 프로그램 시작
         * TCP연결
         * 이미지, 비디오 출력
         * 로딩
         * 결과창
         * 프로그램 종료
         */
        public enum testStatus
        {
            init,
            Intro,
            Connect,
            Image,
            Video,
            Loading,
            Result,
            End,
        }
        public Texture2D tex;
        public static UI instance = null;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }
        private void Update()
        {
            if (curState != changedState)
            {
                curState = changedState;
                ChangeMode(curState, param1, param2);
            }
            if (curState == testStatus.Loading)
            {
                if (datasize != 0)
                    loading.text = "loading : " + progress + "%";
            }
            else if (curState == testStatus.Result)
            {
                RectTransform rt = goN.GetComponent<RectTransform>();
                if (rt.anchoredPosition.x < N)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 1, rt.anchoredPosition.y);
                }
                else if (rt.anchoredPosition.x > N)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 1, rt.anchoredPosition.y);
                }

                rt = goE.GetComponent<RectTransform>();
                if (rt.anchoredPosition.x < E)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 1, rt.anchoredPosition.y);
                }
                else if (rt.anchoredPosition.x > E)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 1, rt.anchoredPosition.y);
                }

                rt = goO.GetComponent<RectTransform>();
                if (rt.anchoredPosition.x < O)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 1, rt.anchoredPosition.y);
                }
                else if (rt.anchoredPosition.x > O)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 1, rt.anchoredPosition.y);
                }

                rt = goC.GetComponent<RectTransform>();
                if (rt.anchoredPosition.x < C)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 1, rt.anchoredPosition.y);
                }
                else if (rt.anchoredPosition.x > C)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 1, rt.anchoredPosition.y);
                }

                rt = goA.GetComponent<RectTransform>();
                if (rt.anchoredPosition.x < A)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + 1, rt.anchoredPosition.y);
                }
                else if (rt.anchoredPosition.x > A)
                {
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - 1, rt.anchoredPosition.y);
                }
                rN.text = "Neuroticism : " + (N + 50).ToString();
                rE.text = "Extraversion : " + (E + 50).ToString();
                rO.text = "Openness : " + (O + 50).ToString();
                rC.text = "Conscientiousness : " + (C + 50).ToString();
                rA.text = "Agreeableness : " + (A + 50).ToString();
            }
        }
        /*
         * 실행중인 gameobject 초기화
         */
        private void DeactiveAll()
        {
            test.instance.image.gameObject.SetActive(false);
            test.instance.loading.SetActive(false);
            test.instance.result.SetActive(false);
            SwitchVideos_S.instance.ShowSample(-1);
        }
        [EnumAction(typeof(testStatus))]
        private void ChangeMode(int status, int param1, int param2)
        {
            ChangeMode((testStatus)status, param1, param2);
        }

        private void ChangeMode(testStatus status, int param1, int param2)
        {
            DeactiveAll();

            switch (status)
            {
                //프로그램 시작시
                case testStatus.Intro:
                    test.instance.startT2();
                    test.instance.Init();
                    tex = new Texture2D(2, 2);
                    Debug.Log("status : intro");
                    break;
                //TCP 연결시
                case testStatus.Connect:
                    test.instance.startT1();
                    Debug.Log("status : connect");
                    break;
                //video(mp4) 데이터 수신시
                case testStatus.Video:
                    /*
                     * param1 : 1 = stereo 2 = 180 angle 3 = 360 angle
                     * param2 : 1 = none 2 = topbottom 3 = leftright
                     */
                    byte[] temp = test.instance.getData();
                    queue.Enqueue(temp);
                    queue1.Enqueue(param1);
                    queue2.Enqueue(param2);
                    if (tf)
                    {
                        videostart();
                        tf = false;
                    }
                    break;
                //image(jpg) 데이터 수신시
                case testStatus.Image:
                    tex.LoadImage(test.instance.getData());
                    float width = tex.width;
                    float height = tex.height;
                    test.instance.image.sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2());
                    test.instance.image.gameObject.SetActive(true);
                    test.instance.setData(null);
                    Debug.Log("status : image");
                    if (tf)
                    {
                        tf = false;
                    }
                    break;
                //예정 : TCP 연결전 대기화면 ,PC에 녹화된 뇌파 데이터 분석 요구
                case testStatus.Loading:
                    if (tf)
                    {
                        if (datasize == 0)
                            loading.text = "Analysis Loading.......";
                        test.instance.loading.SetActive(true);
                        Debug.Log("status : loading");
                        
                    }
                    break;
                //예정 : PC에서 분석된 결과 표시
                case testStatus.Result:
                    N = Random.Range(-50, 51);
                    rN.text = "Neuroticism : " + (N + 50).ToString();
                    E = Random.Range(-50, 51);
                    rE.text = "Extraversion : " + (E + 50).ToString();
                    O = Random.Range(-50, 51);
                    rO.text = "Openness : " + (O + 50).ToString();
                    C = Random.Range(-50, 51);
                    rC.text = "Conscientiousness : " + (C + 50).ToString();
                    A = Random.Range(-50, 51);
                    rA.text = "Agreeableness : " + (A + 50).ToString();
                    /*
                     * 계획 : testclass에서 받은 결과 저장
                    */
                    test.instance.result.SetActive(true);
                    Debug.Log("status : result");
                    break;
                //프로그램 종료
                case testStatus.End:
                    test.instance.joinT1();
                    test.instance.joinT2();
                    Debug.Log("status : end");
                    break;

                default:
                    break;
            }
        }
        public bool getQueue()
        {
            if (queue.Count == 0)
                return false;
            else
                return true;
        }
        public void settf(bool b)
        {
            tf = b;
        }
        public bool gettf()
        {
            return tf;
        }
        public void videostart()
        {
            string path = Application.persistentDataPath + "/test.mp4";
            //path = path.Substring(0,path.LastIndexOf('/'));
            //path = Path.Combine(path, "test.mp4");
            byte[] data = queue.Dequeue();
            int p1 = queue1.Dequeue();
            int p2 = queue2.Dequeue();
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            fs.Write(data, 0, (int)data.Length);
            fs.Close();
            Debug.Log(path);
            int show = -1;
            GameObject target = null;
            Debug.Log("param1 : " + param1);
            Debug.Log("param2 : " + param2);
            switch (p1)
            {
                case 1:
                    show = 0;
                    target = test.instance.odd;
                    Debug.Log("odd");
                    break;
                case 2:
                    show = 1;
                    target = test.instance.hemi;
                    Debug.Log("hemi");
                    break;
                case 3:
                    show = 2;
                    target = test.instance.sphere;
                    Debug.Log("sphere");
                    break;
            }
            Debug.Log("set path");
            target.GetComponent<GvrVideoPlayerTexture>().videoURL = "file://${Application.persistentDataPath}" + "/test.mp4";
            Debug.Log("change render mode");
            target.GetComponent<Renderer>().material.SetFloat("Stereo mode", p2);
            Debug.Log("showsample");
            SwitchVideos_S.instance.ShowSample(show);
            Debug.Log("resize data");
            test.instance.setData(null);
            Debug.Log("status : video");
        }
        public int getparam1()
        {
            return param1;
        }
        public int getparam2()
        {
            return param2;
        }
        public int getdatasize()
        {
            return datasize;
        }
        public int getloaded()
        {
            return loaded;
        }
        public int getprogress()
        {
            return progress;
        }
        public testStatus getstatus()
        {
            return curState;
        }
        public void setparam1(int p)
        {
            param1 = p;
        }
        public void setparam2(int p)
        {
            param2 = p;
        }
        public void setstatus(testStatus s)
        {
            changedState = s;
        }
        public void setdatasize(int d)
        {
            datasize = d;
        }
        public void setloaded(int d)
        {
            loaded = d;
        }
        public void setprogress(int d)
        {
            progress = d;
        }
    }
}