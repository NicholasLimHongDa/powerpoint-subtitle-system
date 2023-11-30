using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMeCab;

namespace NMecabTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string sentence = "";

                //MeCabParam param = new MeCabParam();
                //param.DicDir = @"E:\sunohara\visualstudio\PowerPointAddIn1\packages\NMeCab.0.06.4\content\net45\dic\ipadic";

                MeCabTagger t = MeCabTagger.Create();//(param);
                MeCabNode node = t.ParseToNode(sentence);
                while (node != null)
                {
                    if (node.CharType > 0)
                    {
                        Console.WriteLine(node.Surface + "\t" + node.Feature);
                    }
                    node = node.Next;
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.Read();
            }
        }
    }
}