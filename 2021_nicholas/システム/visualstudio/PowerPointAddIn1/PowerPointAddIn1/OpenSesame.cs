using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace PowerPointAddIn1
{
    class OpenSesame
    {
        public static void Initialisation (string result)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $@"/c echo {result} | cabocha -f1";   // Note the /c command (*)
            process.StartInfo.CreateNoWindow = true;                            //Prevents cmd from popping up and causing screen to flicker
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;          //Prevents cmd from popping up and causing screen to flicker
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            //* Read the output (or the error)
            string output = process.StandardOutput.ReadToEnd();
            Process(output);
            //Console.WriteLine(output);
            string err = process.StandardError.ReadToEnd();
            Console.WriteLine(err);
            process.WaitForExit();
        }

        public static void Process(string output)
        {
            //The first line starts from an asterisk and is separated by spaces, on the last space there is a value that can be used to relate to other 文節
            //Second line onwards starts with a word and then a tab, followed by phonetic information
            //All of the words combined makes a 文節, the main things we want to extract from this is the last value of the first line and the word (to make a 文節)
            int stringlength = 0;
            int displaylength = 0;
            int index = 0;
            bool skip = false;
            List<string> tango = new List<string>();
            List<string> bunsetsu = new List<string>();
            List<string> wordsinfo = new List<string>();
            List<string> tempwordsinfo = new List<string>();
            List<string> completedstring = new List<string>();
            List<double> dependencyscore = new List<double>();
            List<int> phraseID = new List<int>();
            List<int> dependencyID = new List<int>();
            //Using Regex to separate CaboCha`s header data
            //Regex parts = new Regex(@"^\*\x20\d+\x20(^\d+)\x20.+?\x20([+-]?[0-9]+\.?[0-9]*)"); //In order, *_number_number_anything_decimal
            Regex parts = new Regex(@"^\*.+?");
            Regex contents = new Regex(@"^.+?\t");
            StringReader reader = new StringReader(output);
            StringReader contentreader = new StringReader(output);
            string headerline;
            string contentline;

            //Called to ignore the first header line
            contentline = contentreader.ReadLine();

            while ((headerline = reader.ReadLine()) != "EOS")
            {
                //Searches for an asterisk at the start (See Regex parts)
                Match matchheader = parts.Match(headerline);
                if (matchheader.Success)
                {
                    Console.WriteLine("Match succeeded!");
                    string[] items = headerline.Split('\x20'); //Split by white space
                    phraseID.Add(int.Parse(items[1]));
                    dependencyID.Add(int.Parse(String.Join("", items[2].Where(char.IsDigit))));
                    //Searches for a \t (Tab) anywhere in the line (See Regex contents)
                    //Divides the line into sections depending on the location of \t, and then adds the first part (which is the word) into MyList
                    //Second part (morpheme info) is saved in tempwordsinfo for later processing
                    //Stops when a header line is read (If "D" appears, the loop breaks)
                    while ((contentline = contentreader.ReadLine()) != "EOS")
                    {
                        Match matchcontent = contents.Match(contentline);
                        if (matchcontent.Success)
                        {
                            string[] words = contentline.Split('\t');
                            tango.Add(words[0]);
                            //Adds word info into a temporary list to be concatenated
                            tempwordsinfo.Add(words[1]);
                        }
                        if (contentline.Contains("D")) break;
                    }

                    //Parse the dependency score of each 文節
                    float linkId = float.Parse(items[4]);
                    dependencyscore.Add(linkId);
                    Console.WriteLine(linkId);

                    //Combines multiple 単語 into 文節, and then clears the 単語 list for new input
                    string combindString = string.Join("", tango.ToArray());
                    Console.WriteLine(combindString);
                    stringlength += combindString.Length;
                    bunsetsu.Add(combindString);
                    tango.Clear();

                    //Takes the word info for the current bunsetsu, concatenates them, and then clears tempwordsinfo list for new input
                    //One element of words info represents all word info for one bunsetsu
                    wordsinfo.Add(string.Join(",", tempwordsinfo.ToArray()));
                    tempwordsinfo.Clear();
                }

            }
            //ここから改行挿入アルゴリズムを書こうかな
            foreach (string unit in wordsinfo)
            {
                string[] searchdump = unit.Split(',');
                displaylength += bunsetsu[index].Length;

                //Mandatory linefeed insertion
                //Looks for "strong clause boundary" in wordsinfo, inserts a linefeed into completedstring list if found
                //If the current line has less than 16 words, then it will not insert a linefeed (too short)
                if ((searchdump.Contains("接続助詞") == true) || searchdump.Contains("副詞可能") == true)
                {
                    if (displaylength >= 16)
                    {
                        goto ThresholdExceeded;
                    }

                    else
                    {
                        if (skip == false)
                        {
                            completedstring.Add(bunsetsu[index]);
                        }
                        skip = false;
                        index++;
                    }
                    continue;
                }

                //No linefeed insertion
                //Looks for "no clause boundary" conditions in wordsinfo, joins the current 文節 with the next one if found
                //If found at the last 文節, just inserts the 文節 as it is
                //「skip」is used to tell if the current 文節 has already been printed or not (because of bunsetsu[index + 1])
                else if ((searchdump.Contains("副詞") == true) || (searchdump.Contains("形容動詞語幹") == true) || (searchdump.Contains("形容詞") == true) || (searchdump.Contains("連体化") == true) || (searchdump.Contains("連語") == true) || (searchdump.Contains("連体詞") == true))
                {
                    if (index >= (bunsetsu.Count - 1))
                    {
                        completedstring.Add(bunsetsu[index]);
                    }

                    //現在の一行は11文字以上かつ次の文節の係らない場合
                    else if ((displaylength >= 10) && (dependencyID[index] > (index + 1)))
                    {
                        goto ThresholdExceeded;
                    }

                    else
                    {
                        //If previous 文節 satisfies the same conditions, print only the 文節 after it (to prevent double printing)
                        if (skip == true)
                        {
                            completedstring[completedstring.Count - 1] += bunsetsu[index + 1];
                        }
                        else
                        {
                            completedstring.Add(bunsetsu[index] + bunsetsu[index + 1]);
                            skip = true;
                        }
                        index++;
                        continue;
                    }
                }

                //Placeholder for further processing
                //If dependencyID skips to a 文節 further than the next, linefeed is inserted (excluding the last 文節)
                //If skip is detected, put only the linefeed to avoid duplicate printing
                else
                {
                    //係り受け距離
                    if ((displaylength >= 10) && (dependencyID[index] > (index + 1)))
                    {
                        goto ThresholdExceeded;
                    }

                    if (skip == false)
                    {
                        completedstring.Add(bunsetsu[index]);
                    }
                    skip = false;
                }
                index++;
                continue;

            ThresholdExceeded:
                if (skip == true)
                {
                    completedstring[completedstring.Count - 1] += "\n ";
                }
                else if (skip == false)
                {
                    completedstring.Add(bunsetsu[index] + "\n ");
                }
                displaylength = 0; //maybe omit this and skip to secondary algorithm, anyway this is the correct location
                skip = false;
                index++;
                continue;
            }

            //改行済み
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(string.Join("", completedstring));
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Process is over");

            //Send to PrintSubtitle method in ThisAddIn class
            //processhide is checked to prevent anything from running when a slideshow is not present
            if (ThisAddIn.processhide == false) ThisAddIn.PrintSubtitle(completedstring);
            completedstring.Clear();
        }
    }
}
