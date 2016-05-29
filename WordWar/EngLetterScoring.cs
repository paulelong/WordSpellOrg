using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace WordWar
{
    class EngLetterScoring
    {
        static Random r = new Random();

        static char[] Vowels = { 'A', 'E', 'I', 'O', 'U' };

        static public List<string> DictionaryLookup = new List<string>();

        static public async void LoadDictionary()
        {
            var folders = (await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFoldersAsync()).To‌​List();
            StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Dictionaries\EngDictA.txt");

            IList<String> dictlines = await FileIO.ReadLinesAsync(file);

            foreach (String s in dictlines)
            {
                if (!s.Contains("'") && s.Length > 2)
                {
                    DictionaryLookup.Add(s);
                }
            }
        }



        private static Dictionary<char, int> values = new Dictionary<char, int>()
        {
            {'a', 1 },
            {'b', 3 },
            {'c', 3 },
            {'d', 2 },
            {'e', 1 },
            {'f', 4 },
            {'g', 2 },
            {'h', 4 },
            {'i', 1 },
            {'j', 8 },
            {'k', 5 },
            {'l', 1 },
            {'m', 3 },
            {'n', 1 },
            {'o', 1 },
            {'p', 2 },
            {'q', 10 },
            {'r', 1 },
            {'s', 1 },
            {'t', 1 },
            {'u', 1 },
            {'v', 4 },
            {'w', 4 },
            {'x', 8 },
            {'y', 4 },
            {'z', 10 },

        };

        public static string GetRandomLetter(bool isBurning)
        {

            byte b = (byte)r.Next('A', 'Z');

            if ((b == 'Q' || b == 'Z') && isBurning)
            {
                b = (byte)'E';
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(new[] { b });
        }

        public static int ScoreWord(string s)
        {
            int score = 0;
            foreach (char n in s)
            {
                score += values[n];
            }

            score += LengthBonus(s);

            return score;
        }

        private static int LengthBonus(string s)
        {
            int score = 0;

            if (s.Length > 6)
            {
                score = 2;
            }
            if (s.Length > 7)
            {
                score = 1;
            }
            if (s.Length > 8)
            {
                score = 4;
            }
            if (s.Length > 9)
            {
                score = 5;
            }
            if (s.Length > 10)
            {
                score = 10;
            }
            if (s.Length > 11)
            {
                score = 10;
            }
            if (s.Length > 12)
            {
                score = 11;
            }
            if (s.Length > 13)
            {
                score = 12;
            }
            if (s.Length > 14)
            {
                score = 20;
            }
            if (s.Length > 15)
            {
                score = 30;
            }
            if (s.Length > 16)
            {
                score = 50;
            }
            if (s.Length > 17)
            {
                score = 100;
            }

            return score;
        }

        internal static int ScoreWord(List<Button> buttonList)
        {
            int _score = 0;
            int wordMult = 1;
            string word = "";

            foreach (Button b in buttonList)
            {
                string s = (b.Content as string).ToLower();
                word += s;

                LetterProp lp = b.DataContext as LetterProp;

                _score += lp.GetLetterMult() * values[s[0]];
                wordMult *= lp.GetWordMult();
            }
            return _score * wordMult + LengthBonus(word);
        }

        internal static string ScoreWordString(List<Button> buttonList)
        {
            string _scorestr = "";
            string wordMultTotal = "";
            int totalmult = 1;
            string word = "";

            foreach (Button b in buttonList)
            {
                string s = (b.Content as string).ToLower();
                word += s;

                LetterProp lp = b.DataContext as LetterProp;

                _scorestr += s.ToUpper() + values[s[0]].ToString();
                

                int lettermult = lp.GetLetterMult() ;
                if (lettermult > 1)
                {
                    _scorestr += "x" + lettermult.ToString();
                }
                _scorestr += " ";

                int wordmult = lp.GetWordMult();
                totalmult *= wordmult;
                if (wordmult > 1)
                {
                    wordMultTotal += lp.GetWordMult().ToString() + "x";
                }
            }
            if(totalmult > 1)
            {
                if (totalmult > 2)
                {
                    return _scorestr + "x" + totalmult.ToString() + "[" + wordMultTotal + "]";
                }
                else
                {
                    return _scorestr + "x" + totalmult.ToString();
                }
            }

            if(LengthBonus(word) > 0)
            {
                return _scorestr + " BONUS(" + LengthBonus(word) + ")" + wordMultTotal;
            }
            else
            {
                return _scorestr + wordMultTotal;
            }
        }

        public static string GetWordTally(List<Button> buttonList)
        {
            return GetCurrentWord(buttonList) + " " + ScoreWord(buttonList) + "=>" + EngLetterScoring.ScoreWordString(buttonList);
        }

        public static string GetCurrentWord(List<Button> buttonList)
        {
            string s = "";

            foreach (Button b in buttonList)
            {
                s += b.Content;
            }
            return s;
        }


        internal static int ScoreManna(List<Button> buttonList)
        {
            int lettercnt = 0;
            int mannacnt = 0;

            foreach (Button b in buttonList)
            {
                LetterProp lp = b.DataContext as LetterProp;
                if (lp.IsManna())
                {
                    mannacnt++;
                }
                lettercnt++;
            }

            return (lettercnt * mannacnt);
            
        }

        internal static bool IsConsonant(string content)
        {
            if(content == "A" || content == "E" || content == "I" || content == "O" || content == "U")
            {
                return false;
            }
            return true;         
        }

        internal static string RandomVowel()
        {
            int vowelnum = r.Next(5);
            return Vowels[vowelnum].ToString();
        }
    }
}
