using System;

namespace PowerPointAddIn1
{
    public class julius
    {
        /**/
        public string program_name = "julius_server.exe";

        public string file = @".\Assets\julius\core";

        public static string IPAddress = "localhost";
        public static int port = 10500;

        public float keep_time = 0;

        public float timer = 0;

        private static Process julius_process;


        public static void initialize()
        {
            mecab m = new mecab();

        }
    }

}
