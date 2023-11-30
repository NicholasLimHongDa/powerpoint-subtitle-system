
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;
using System;
using System.IO;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;
//using UnityEngine;

namespace PowerPointAddIn1
{
    public class AzureAPI/* : MonoBehaviour*/
    {
        async public static Task FromMic(SpeechConfig speechConfig)
        {
            using (var audioConfig = AudioConfig.FromDefaultMicrophoneInput())
            {
                using (var recognizer = new SpeechRecognizer(speechConfig, "ja-JP", audioConfig))
                {
                    //認識停止関数の宣言
                    var stopRecognition = new TaskCompletionSource<int>();

                    //Added
                    string Result;

                    Console.WriteLine("Speak into your microphone.");
                    //Julius_Config.begin_getstream();

                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                        Result = $"{e.Result.Text}";
                        OpenSesame.Initialisation(Result);
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            Result = $"{e.Result.Text}";
                            OpenSesame.Initialisation(Result);
                            
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Debug.WriteLine("\n    Session stopped event.");
                        stopRecognition.TrySetResult(0);
                    };
                    //Starts continuous recognition
                    //If Slideshow ends, stops recognition
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
                    Task.WaitAny(new[] { stopRecognition.Task });
                    //await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }
    }
}