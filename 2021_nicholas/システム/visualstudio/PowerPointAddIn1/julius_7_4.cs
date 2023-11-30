//using UnityEngine;

using System.Collections;

using System.Threading;

using System.Net.Sockets;

using System.Net;

using System.Text.RegularExpressions;

using System.Text;

using System.Diagnostics;

using System;

using System.IO;



namespace PowerPointAddIn1
{
    public class Julius_Client/* : MonoBehaviour*/
    {

        //--------------------変数-----------------------------------------------------------
        /* ストリームの長さ*/
        public static int STREAM_LEN = 6000;

        //juliusからの結果用

        public static string Result = string.Empty;

        public static Queue queue = new Queue();

        public static string print = string.Empty, _print = string.Empty;

        public static int zisuu = 0;

        public static DateTime time;

        private static string _message;
        public static string message
        {
            set
            {
                if (_message != value)
                {
                    Debug.WriteLine(value);
                    _message = value;
                }
            }

            get
            {
                return _message;
            }
        }

        public static int return_num = 0;

        //初期設定用

        //public			microphone		mic = null;

        //public			float			vol_line			= 0;		

        public bool windowtype_hidden = false;

        public string program_name = "julius_server.exe";

        public string file = @".\Assets\julius\core";

        public static string IPAddress = "localhost";

        public static int port = 10500;


        public string command = "-C main.jconf -C am-gmm.jconf -input mic -48 -module -charconv utf-8 sjis";

        public float keep_time = 0;

        public float timer = 0;

        //getstream結果
        public static string stream_result = string.Empty;

        //TCP/IP用

        public static bool connect = false;

        private static TcpClient tcpip = null;

        private static NetworkStream net;

        private static string stream;

        //private float wait_time = 1;



        //XML処理用

        public static string regular = "WORD=\"([^。\"]+)\"";

        private static string tmp = string.Empty;

        private static string _tmp = string.Empty;

        public static string words = "HogeHoge";

        private static byte[] data = new byte[STREAM_LEN];//ストリームの長さ

        private static byte[] data_delete = new byte[STREAM_LEN];

        private static int read = 0;

        private static Match sampling;

        private static Regex xml_data;


        //外部プログラム用

        private static System.Diagnostics.Process julius_process;

        private static bool run = false;

        //private mecab m = new mecab();

        //マルチスレッド用

        private static Thread julius_thread;

        //-------------------------------------------------------------------------------------



        //---------------------------------julius.exe------------------------------------------


        /*外部プログラムjulisのプロセスを強制終了*/

        private static void kill_julius_server()
        {

            //プロセスの強制終了
            try
            {
                //julius_process.Kill();

                if (julius_process.HasExited)
                {

                    message = "Kill julius server.";

                }
                else
                {
                    julius_process.Refresh();
                    julius_process.Kill();
                    //julius_thread.Sleap();
                    julius_thread.Abort();
                }

                Debug.WriteLine("julius切れた");
                //julius_thread.Abort();

                //julius_process.Close();
                //julius_process.Kill();
                //julius_process.Dispose();
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine("スレッドがうまく切れなかった");
            }
            catch
            {
                Debug.WriteLine("プロセス切断時にエラーが起きました");
            }


        }



        /*juliusサーバーへ接続する*/

        public static void initialize_julius_client()
        {
            try
            {
                /*mecabを起動する*/
                mecab m = new mecab();
                string mecab_result = "";
                //string s = "えー、まあ、１年が過ぎるのなんてね、あっという間なんですよ。本当にね。お正月が来たと思ったら、もう桜の季節ですよ。そして、ゴールデンウイークだの梅雨だの過ぎたら、もう半年終わるでしょう？";
                //m.run_mecab(s);
                string s = "あーっと、そーですねー、えーと、うーん、まぁ、えぇ。うーむ……そっかぁ。そーですよねぇ……いや、確かに。あー、言われてみれば、まぁ、思い当たるような";
                mecab_result = m.Run_mecab(s);
                Debug.WriteLine(mecab_result);
                //m.run_mecab(s);

                //TCP/IPの初期化＆juliusサーバーへ接続

                tcpip = new TcpClient(IPAddress, port);

                //クライアントが取得出来たかどうか

                message = "Connect Success.";

                connect = true;

                return_num = 1;
                //ストリームの取得

                net = tcpip.GetStream();

                //マルチスレッドへ登録＆開始

                julius_thread = new Thread(new ThreadStart(get_stream));

                julius_thread.Start();
            }
            catch (ArgumentNullException e)
            {
                connect = false;
                //Console.WriteLine("ArgumentNullException:{0}", e);
                Debug.WriteLine("NullException Error" + e);
            }
            catch (SocketException e)
            {
                connect = false;
                //Console.WriteLine("SocketException:{0}", e);
                Debug.WriteLine("socket Error" + e);
            }

        }



        /*juliusサーバーから切断*/

        private static void close_julius()
        {

            //TCP/IPの切断処理

            connect = false;
            net.Close();
            Debug.WriteLine("juliusとの接続を切る");
            //juliusサーバーのプロセスを強制終了

            kill_julius_server();
            //net.Close();
        }

        //--------------------------------------------------------------------



        //-----------------------------Stream---------------------------------

        /*juliusサーバーから受信*/

        private static void get_stream()
        {//**マルチスレッド関数**
            string stream_tmp;
            string stream_old = stream_result;
            int count = 0;

            while (connect == true)
            {
                count++;
                //message = "count:" + count;
                //マルチスレッドの速度？

                Thread.Sleep(0);

                //ストリームの受信

                read = net.Read(data, 0, data.Length);
                //if (read < 150) data = new byte[1000];

                Array.Copy(data_delete, read, data, read, STREAM_LEN - read);

                //message = "read:" + read;
                //message = "data:" + data;

                stream = System.Text.Encoding.Default.GetString(data);

                //message = "stream:" + stream;
                //Debug.WriteLine("get_stream"+stream);

                stream_tmp = string.Empty;

                //XMLデータから文字列の抽出

                stream_result = stream_tmp;
                stream_tmp = XML_search(stream);

                stream_result = stream_tmp;
                Debug.WriteLine(stream_tmp);
                /*if (mecab.categorize(stream_tmp))
                {
                    if (stream_old != stream_tmp)
                    {
                        stream_result = stream_tmp;
                    }
                }*/
            }

        }



        /*juliusサーバーへ送信*/
        /*
        private void send_stream(string msg)
        {
            if (connect == true)
            {
                //net = tcpip.GetStream ();

                byte[] send_byte = Encoding.UTF8.GetBytes(msg);

                //ストリームの送信

                net.Write(send_byte, 0, send_byte.Length);
            }


        }
        /*


        /*ストリーム情報から正規表現を利用して文字列を抽出する*/

        private static string XML_search(string stream)
        {
            string search_result = string.Empty;
            //string tmp = "aa";



            //正規表現

            xml_data = new Regex(regular);

            //Debug.WriteLine(regular);

            //初回抽出(NextMatch()を使うため)

            sampling = xml_data.Match(stream);


            Debug.WriteLine(sampling.Success);
            //Debug.WriteLine("sampling===================================" + sampling.Groups[1].Value);
            while (sampling.Success)/*文字列の時*/
            {//最後まで抽出
                //Debug.WriteLine("sampling==================================="+ sampling.Groups[0].Value);
                //Debug.WriteLine("sampling=========\\\\\\=====" + sampling.Groups[1].Value);
                //結合処理

                for (int i = 1; i < sampling.Groups.Count; i++)
                {//なぜかi = 1にしたらうまく行った

                    search_result += sampling.Groups[i].Value;

                }
                //順次抽出していく

                sampling = sampling.NextMatch();

            }

            //最終的に結合した文字列を返す
            Debug.WriteLine("XML_search_result" + search_result);
            return search_result;

        }

        //--------------------------------------------------------------



        private void timer_reset()
        {

            timer = 0f;

        }

        //--------------------------Main--------------------------------


        // Update is called once per frame

        /*void Update()*/
        public static void mugen_julius()
        {
            time = DateTime.Now;

            //int flag;
            while (connect == true)
            {

                if (stream_result != string.Empty)//空の時
                {

                    _tmp = stream_result.Replace("<sil>", "");
                    _tmp = _tmp.Replace("<sp>", "");
                    if (_tmp != string.Empty)
                        //Debug.WriteLine("<sil>を消した後" + _tmp);
                        message = "Ready!";

                    //return_num = 1;

                    if (_tmp != words)
                    {
                        if (_tmp != string.Empty)
                        {
                            words = _tmp;

                            queue.Enqueue(_tmp);

                            Result = _tmp;
                            Debug.WriteLine("Result:" + Result);
                        }
                    }

                }
                else
                {

                    message = "Wait for response...";

                    //return_num = 1;
                }


                if ((DateTime.Now - time).TotalSeconds > print.Length / 4)
                {
                    if (queue.Count > 0)
                    {
                        print = (string)queue.Dequeue();
                        if (zisuu > 0)
                            print = " " + print;
                        zisuu += print.Length;
                        message = "print:" + print;
                        Debug.WriteLine("zisuu: " + zisuu);
                        //flag = 1;

                        if (zisuu == 0) zisuu += print.Length;
                        message = "表示";
                        ThisAddIn.out_zimaku();
                        time = DateTime.Now;
                        _print = print;
                        //flag = 0;
                    }
                    if (zisuu > 30 || (DateTime.Now - time).TotalSeconds > 10)
                    {
                        ThisAddIn.deleate_zimaku();

                        zisuu = 0;
                    }
                }

            }
        }


        //終了処理と同時に実行される

        public static void OnApplicationQuit()
        {

            close_julius();
            //マルチスレッドの終了

        }

        //-------------------------------------------------------------

    }



    /*

    public class julius_config
    {
        public string program_name = "julius_server.exe";

        public string file = @".\Assets\julius\core";

        public static string IPAddress = "localhost";

        public static int port = 10500;

    }*/
}