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
    public class Julius_Config
    {

        public bool connect = false;


        public string program_name = "julius_server.exe";

        public string file = @".\Assets\julius\core";

        public static string IPAddress = "localhost";

        public static int port = 10500;

        private static System.Diagnostics.Process julius_process;
        /*外部プログラムjulisのプロセスを強制終了*/



        private static void kill_julius_server()
        {

            //プロセスの強制終了
            try
            {
                //julius_process.Kill();

                if (julius_process.HasExited)
                {

                    Debug.WriteLine("juliusを切る");

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

        /*juliusサーバーから切断*/

        public static void close_julius()
        {

            //TCP/IPの切断処理

            connect = false;
            net.Close();
            Debug.WriteLine("juliusとの接続を切る");
            //juliusサーバーのプロセスを強制終了

            kill_julius_server();
            //net.Close();
        }


        //終了処理と同時に実行される

        public static void OnApplicationQuit()
        {
            
            close_julius();
            //マルチスレッドの終了

        }
    }
}