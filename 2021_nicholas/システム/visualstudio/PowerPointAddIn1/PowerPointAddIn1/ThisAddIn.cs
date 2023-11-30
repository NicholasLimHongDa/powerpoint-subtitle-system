using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Office.Core;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace PowerPointAddIn1
{
    public partial class ThisAddIn
    {
        public Microsoft.Office.Tools.CustomTaskPane Subtitle_Panel;
        public static int slide_num = 0;
        static PowerPoint.Slide Sld;
        public static bool subtitledisplay = false;
        public static bool processhide = false;
        public static bool newBox = false;
        public static PowerPoint.Shape textBox;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            this.Application.SlideShowNextSlide += new PowerPoint.EApplication_SlideShowNextSlideEventHandler(Application_SlideShowNextSlide);
            this.Application.SlideShowBegin += new PowerPoint.EApplication_SlideShowBeginEventHandler(Application_SlideShowBegin);
            this.Application.SlideShowEnd += new PowerPoint.EApplication_SlideShowEndEventHandler(Application_SlideShowEnd);
            Subtitle_Panel = this.CustomTaskPanes.Add(new UserControl1(), "Subtitles");

            Debug.WriteLine("Application has started.");
        }

        public void ShowPanel_Subtitles ()
        {
            Subtitle_Panel.Visible = true;
            Subtitle_Panel.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionBottom;
            Subtitle_Panel.Height = 100;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        void Application_SlideShowNextSlide(PowerPoint.SlideShowWindow Wn)
        {
            newBox = true;
            slide_num++;
            Debug.WriteLine("現在の枚数は" + slide_num);
            Sld = Wn.View.Slide;
            if ((slide_num > 1) && (subtitledisplay == true))  textBox.Delete();
        }
        // スライドからオブジェクトとテキストを取得する


        public static async void PrintSubtitle(List<string> empwords) //字幕の表示
        {
            if (newBox == true)
            {
                textBox = Sld.Shapes.AddTextbox(Office.MsoTextOrientation.msoTextOrientationHorizontal, 200, 450, 600, 600);
                textBox.Fill.BackColor.RGB = System.Drawing.Color.FromArgb(255, 255, 255).ToArgb();
                textBox.TextFrame2.TextRange.Font.Fill.ForeColor.RGB = System.Drawing.Color.FromArgb(1, 1, 1).ToArgb();
                textBox.TextFrame.TextRange.InsertAfter(" " + string.Join("", empwords) + "\n\n\n\n\n");
                empwords.Clear();
                newBox = false;
            }

            else
            {
                textBox.TextFrame.TextRange.Replace(" ", (" " + string.Join("", empwords) + "\n\n\n\n\n"));
                empwords.Clear();
            }
        }

        async void Application_SlideShowBegin(PowerPoint.SlideShowWindow Wn)
        {
            processhide = false;
            if (subtitledisplay == true)
            {
                var speechConfig = SpeechConfig.FromSubscription("0f2d4b927d4e41cba77d0a1e4dc4f5ea", "japaneast");
                await AzureAPI.FromMic(speechConfig);
            }
            Debug.WriteLine("Slideshow has begun");
        }

        //スライドショーを終了
        async void Application_SlideShowEnd(PowerPoint.Presentation Pres)
        {
            processhide = true;
            await Task.Delay(3000);
            if (subtitledisplay == true)    textBox.Delete();
        }

        #region VSTO で生成されたコード
        /// <summary>
        /// デザイナーのサポートに必要なメソッドです。
        /// このメソッドの内容をコード エディターで変更しないでください。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}