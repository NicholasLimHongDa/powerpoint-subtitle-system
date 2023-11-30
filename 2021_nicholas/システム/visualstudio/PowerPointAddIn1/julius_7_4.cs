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

        //--------------------�ϐ�-----------------------------------------------------------
        /* �X�g���[���̒���*/
        public static int STREAM_LEN = 6000;

        //julius����̌��ʗp

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

        //�����ݒ�p

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

        //getstream����
        public static string stream_result = string.Empty;

        //TCP/IP�p

        public static bool connect = false;

        private static TcpClient tcpip = null;

        private static NetworkStream net;

        private static string stream;

        //private float wait_time = 1;



        //XML�����p

        public static string regular = "WORD=\"([^�B\"]+)\"";

        private static string tmp = string.Empty;

        private static string _tmp = string.Empty;

        public static string words = "HogeHoge";

        private static byte[] data = new byte[STREAM_LEN];//�X�g���[���̒���

        private static byte[] data_delete = new byte[STREAM_LEN];

        private static int read = 0;

        private static Match sampling;

        private static Regex xml_data;


        //�O���v���O�����p

        private static System.Diagnostics.Process julius_process;

        private static bool run = false;

        //private mecab m = new mecab();

        //�}���`�X���b�h�p

        private static Thread julius_thread;

        //-------------------------------------------------------------------------------------



        //---------------------------------julius.exe------------------------------------------


        /*�O���v���O����julis�̃v���Z�X�������I��*/

        private static void kill_julius_server()
        {

            //�v���Z�X�̋����I��
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

                Debug.WriteLine("julius�؂ꂽ");
                //julius_thread.Abort();

                //julius_process.Close();
                //julius_process.Kill();
                //julius_process.Dispose();
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine("�X���b�h�����܂��؂�Ȃ�����");
            }
            catch
            {
                Debug.WriteLine("�v���Z�X�ؒf���ɃG���[���N���܂���");
            }


        }



        /*julius�T�[�o�[�֐ڑ�����*/

        public static void initialize_julius_client()
        {
            try
            {
                /*mecab���N������*/
                mecab m = new mecab();
                string mecab_result = "";
                //string s = "���[�A�܂��A�P�N���߂���̂Ȃ�ĂˁA�����Ƃ����ԂȂ�ł���B�{���ɂˁB�������������Ǝv������A�������̋G�߂ł���B�����āA�S�[���f���E�C�[�N���̔~�J���̉߂�����A�������N�I���ł��傤�H";
                //m.run_mecab(s);
                string s = "���[���ƁA���[�ł��ˁ[�A���[�ƁA���[��A�܂��A�����B���[�ށc�c���������B���[�ł���˂��c�c����A�m���ɁB���[�A�����Ă݂�΁A�܂��A�v��������悤��";
                mecab_result = m.Run_mecab(s);
                Debug.WriteLine(mecab_result);
                //m.run_mecab(s);

                //TCP/IP�̏�������julius�T�[�o�[�֐ڑ�

                tcpip = new TcpClient(IPAddress, port);

                //�N���C�A���g���擾�o�������ǂ���

                message = "Connect Success.";

                connect = true;

                return_num = 1;
                //�X�g���[���̎擾

                net = tcpip.GetStream();

                //�}���`�X���b�h�֓o�^���J�n

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



        /*julius�T�[�o�[����ؒf*/

        private static void close_julius()
        {

            //TCP/IP�̐ؒf����

            connect = false;
            net.Close();
            Debug.WriteLine("julius�Ƃ̐ڑ���؂�");
            //julius�T�[�o�[�̃v���Z�X�������I��

            kill_julius_server();
            //net.Close();
        }

        //--------------------------------------------------------------------



        //-----------------------------Stream---------------------------------

        /*julius�T�[�o�[�����M*/

        private static void get_stream()
        {//**�}���`�X���b�h�֐�**
            string stream_tmp;
            string stream_old = stream_result;
            int count = 0;

            while (connect == true)
            {
                count++;
                //message = "count:" + count;
                //�}���`�X���b�h�̑��x�H

                Thread.Sleep(0);

                //�X�g���[���̎�M

                read = net.Read(data, 0, data.Length);
                //if (read < 150) data = new byte[1000];

                Array.Copy(data_delete, read, data, read, STREAM_LEN - read);

                //message = "read:" + read;
                //message = "data:" + data;

                stream = System.Text.Encoding.Default.GetString(data);

                //message = "stream:" + stream;
                //Debug.WriteLine("get_stream"+stream);

                stream_tmp = string.Empty;

                //XML�f�[�^���當����̒��o

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



        /*julius�T�[�o�[�֑��M*/
        /*
        private void send_stream(string msg)
        {
            if (connect == true)
            {
                //net = tcpip.GetStream ();

                byte[] send_byte = Encoding.UTF8.GetBytes(msg);

                //�X�g���[���̑��M

                net.Write(send_byte, 0, send_byte.Length);
            }


        }
        /*


        /*�X�g���[����񂩂琳�K�\���𗘗p���ĕ�����𒊏o����*/

        private static string XML_search(string stream)
        {
            string search_result = string.Empty;
            //string tmp = "aa";



            //���K�\��

            xml_data = new Regex(regular);

            //Debug.WriteLine(regular);

            //���񒊏o(NextMatch()���g������)

            sampling = xml_data.Match(stream);


            Debug.WriteLine(sampling.Success);
            //Debug.WriteLine("sampling===================================" + sampling.Groups[1].Value);
            while (sampling.Success)/*������̎�*/
            {//�Ō�܂Œ��o
                //Debug.WriteLine("sampling==================================="+ sampling.Groups[0].Value);
                //Debug.WriteLine("sampling=========\\\\\\=====" + sampling.Groups[1].Value);
                //��������

                for (int i = 1; i < sampling.Groups.Count; i++)
                {//�Ȃ���i = 1�ɂ����炤�܂��s����

                    search_result += sampling.Groups[i].Value;

                }
                //�������o���Ă���

                sampling = sampling.NextMatch();

            }

            //�ŏI�I�Ɍ��������������Ԃ�
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

                if (stream_result != string.Empty)//��̎�
                {

                    _tmp = stream_result.Replace("<sil>", "");
                    _tmp = _tmp.Replace("<sp>", "");
                    if (_tmp != string.Empty)
                        //Debug.WriteLine("<sil>����������" + _tmp);
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
                        message = "�\��";
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


        //�I�������Ɠ����Ɏ��s�����

        public static void OnApplicationQuit()
        {

            close_julius();
            //�}���`�X���b�h�̏I��

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