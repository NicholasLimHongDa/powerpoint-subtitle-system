//using UnityEngine;

using System.Collections;

using System.Threading;

using System.Net.Sockets;

using System.Net;

using System.Text.RegularExpressions;

using System.Text;

using System.Diagnostics;


namespace PowerPointAddIn1
{
    public class Julius_Client/* : MonoBehaviour*/
    {

        //--------------------変数-----------------------------------------------------------



        //juliusからの結果用

        public static string Result = string.Empty;

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



        //TCP/IP用

        public static bool connect = false;

        private static TcpClient tcpip = null;

        private static NetworkStream net;

        private static string stream;

        private float wait_time = 1;



        //XML処理用

        public static string regular = "WORD=\"([^。\"]+)\"";

        private static string tmp = string.Empty;

        public static string words = "HogeHoge";

        private static byte[] data = new byte[10000];

        private static Match sampling;

        private static Regex xml_data;


        //外部プログラム用

        private static System.Diagnostics.Process julius_process;

        private static bool run = false;



        //マルチスレッド用

        private static Thread julius_thread;



        //-------------------------------------------------------------------------------------



        //---------------------------------julius.exe------------------------------------------

        /*外部プログラムjuliusをコマンド付きで起動*/
        /*
        private bool open_julius_server()
        {

            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();

            info.FileName = program_name;

            info.WorkingDirectory = file;

            info.Arguments = command;

            if (windowtype_hidden)
            {

                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            }

            //juliusプロセスをjulius_processに登録

            try
            {

                julius_process = System.Diagnostics.Process.Start(info);

            }
            catch (System.ComponentModel.Win32Exception w)
            {

                //Debug.Log("Not Found." + w);

                message = "Not Found.";

                return_num = 0;

                return false;

            }

            return true;

        }
        */


        /*外部プログラムjulisのプロセスを強制終了*/

        private static void kill_julius_server()
        {

            //プロセスの強制終了

            julius_process.Kill();

            if (julius_process.HasExited)
            {

                //Debug.Log("Kill julius server.");

                message = "Kill julius server.";

                return_num = 1;

            }
            else
            {

                julius_process.Kill();

            }

            julius_process.Close();

            julius_process.Dispose();

        }



        /*juliusサーバーへ接続する*/

        public static bool initialize_julius_client()
        {

            //TCP/IPの初期化＆juliusサーバーへ接続

            tcpip = new TcpClient(IPAddress, port);

            //クライアントが取得出来たかどうか

            if (tcpip == null)
            {

                //Debug.Log("Connect Fall.");

                message = "Connect Fall.";

                return_num = 0;

                return false;

            }
            else
            {

                //Debug.Log("Connect Success.");

                message = "Connect Success.";

                return_num = 1;

                //ストリームの取得

                net = tcpip.GetStream();

                //マルチスレッドへ登録＆開始

                julius_thread = new Thread(new ThreadStart(get_stream));

                julius_thread.Start();

                return true;

            }

        }



        /*juliusサーバーから切断*/

        private static void close_julius()
        {

            //TCP/IPの切断処理

            connect = false;
            net.Close();

            //juliusサーバーのプロセスを強制終了

            //kill_julius_server();

        }



        /*サーバーが起動するまで時間があるので少し待つ*/
        /*
        private IEnumerator start_julius_server()
        {

            //Debug.Log("Julius Initialize...");

            message = "Julius Initialize...";

            return_num = 1;

            yield return new WaitForSeconds(wait_time);

            //Debug.Log("Connect start");

            message = "Connect start";

            return_num = 1;

            connect = initialize_julius_client();

        }
        */
        //--------------------------------------------------------------------



        //-----------------------------Stream---------------------------------

        /*juliusサーバーから受信*/

        private static void get_stream()
        {//**マルチスレッド関数**

            while (true)
            {

                //マルチスレッドの速度？

                Thread.Sleep(0);

                //ストリームの受信

                net.Read(data, 0, data.Length);

                stream = System.Text.Encoding.Default.GetString(data);

                //Debug.Log (stream);



                //Debug.Log ("tmp_s : "+words)

                tmp = string.Empty;

                //XMLデータから文字列の抽出

                tmp = XML_search(stream);

            }

        }



        /*juliusサーバーへ送信*/

        private void send_stream(string msg)
        {

            //net = tcpip.GetStream ();

            byte[] send_byte = Encoding.UTF8.GetBytes(msg);

            //ストリームの送信

            net.Write(send_byte, 0, send_byte.Length);

            //Debug.Log ("Send Message -> "+msg);

        }



        /*ストリーム情報から正規表現を利用して文字列を抽出する*/

        private static string XML_search(string stream)
        {

            string tmp = string.Empty;



            //正規表現

            xml_data = new Regex(regular);

            //初回抽出(NextMatch()を使うため)

            sampling = xml_data.Match(stream);

            while (sampling.Success)
            {//最後まで抽出

                //結合処理

                for (int i = 1; i < sampling.Groups.Count; i++)
                {//なぜかi = 1にしたらうまく行った

                    tmp += sampling.Groups[i].Value;

                }

                //順次抽出していく

                sampling = sampling.NextMatch();

            }

            //最終的に結合した文字列を返す

            return tmp;

        }

        //--------------------------------------------------------------



        private void timer_reset()
        {

            timer = 0f;

        }





        //--------------------------Main--------------------------------

        // Use this for initialization
        /*
        void Start()
        {

            //juliusサーバーを起動

            run = open_julius_server();

            //起動確認

            if (run)
            {

                //juliusシステムの起動

                StartCoroutine("start_julius_server");

            }

        }
        */


        // Update is called once per frame

        /*void Update()*/
        public static void mugen_julius()
        {
            while (true)
            {

                //結果を常に受け取る

                if (connect/*run=true*/)
                {

                    if (tmp != string.Empty)
                    {

                        /*

                        //wordsの利用時間

                        if(tmp == words){

                            timer += Time.deltaTime;

                            if(timer >= keep_time){

                                //初期化

                                words = "";

                            }

                        }else{

                            tmp = words;

                            timer_reset();

                        }

                        */

                        //Result = words;



                        //Debug.Log("tmp : " + tmp + " " + "word : " + words + " Result : " + Result);

                        message = "Ready!";

                        return_num = 1;

                        if (tmp != words)
                        {
                            //Debug.WriteLine("tmp:"+tmp);
                            if (tmp != string.Empty)
                            {
                                words = tmp;

                                Result = tmp;
                                Debug.WriteLine("Result:" + Result);
                            }
                        }
                        /*else
                        {

                            Result = string.Empty;

                        }*/



                    }
                    else
                    {

                        //Debug.Log("Wait for response...");

                        message = "Wait for response...";

                        return_num = 1;

                    }

                }
                else
                {

                    //Debug.Log("Error");

                    message = "Error";

                    return_num = 0;

                }

            }
        }


        //終了処理と同時に実行される

        public static void OnApplicationQuit()
        {

            if (connect)
            {

                //juliusサーバーを切断

                close_julius();

                //マルチスレッドの終了

                julius_thread.Abort();

            }

        }

        //-------------------------------------------------------------

    }
}