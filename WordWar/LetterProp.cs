using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace WordWar
{
    class LetterProp
    {
        public int i, j;
        private TileTypes tt;
        public Button b;
        private byte _letter;
        static private int prob_total;

        public TileTypes TileType
        {
            get { return tt;  }
        }

        static Dictionary<TileTypes, TileTypeProperties> SortedTiles = new Dictionary<TileTypes, TileTypeProperties>();
        static List<TileProb> TilesForLevel = new List<TileProb>();

        static Random r = new Random();

        public byte letter
        {
            get { return _letter;  }
            set { _letter = value; b.Content = Convert.ToChar(_letter).ToString(); }
        }

        public enum TileTypes
        {
            WordDouble,
            LetterDouble,
            WordTriple,
            LetterTriple,
            Burning,
            Manna,
            Normal,
        };

        struct TileTypeProperties
        {
            public double probability;
            public int prob2;
            public Color foreground;
            public Color background;
            public double depthmod;
            public int level;
            public double levelmod;
        }

        struct TileProb
        {
            public int probability;
            public TileTypes tt;
        }

        static Dictionary<TileTypes, TileTypeProperties> TileTypeProp = new Dictionary<TileTypes, TileTypeProperties>
        {
            {TileTypes.Normal, new TileTypeProperties { prob2 = 150, probability = 1.0, background = Colors.Black, foreground = Colors.DarkGray, depthmod = 0.01, level = 0, levelmod = 0.0 } },
            {TileTypes.LetterDouble, new TileTypeProperties { prob2 = 10, probability = 0.08, foreground = Colors.Black, background = Colors.LightPink, depthmod = 0.01, level = 2, levelmod = 0.2 }},
            {TileTypes.WordDouble, new TileTypeProperties { prob2 = 8, probability = 0.05, foreground = Colors.Black, background = Colors.LightCyan, depthmod = 0.01, level = 4, levelmod = 0.2 }},
            {TileTypes.LetterTriple, new TileTypeProperties { prob2 = 5, probability = 0.04, foreground = Colors.Black, background = Colors.DeepPink, depthmod = 0.01, level = 7, levelmod = 0.2 }},
            {TileTypes.WordTriple, new TileTypeProperties { prob2 = 4, probability = 0.03, foreground = Colors.Black, background = Colors.Cyan, depthmod = 0.01, level = 6, levelmod = 0.2 }},
            {TileTypes.Burning, new TileTypeProperties{ prob2 = 10, probability = 0.12, background = Colors.Black, foreground = Colors.Red, depthmod = 1.0, level = 3, levelmod = 0.5 }},
            {TileTypes.Manna, new TileTypeProperties{ prob2 = 8, probability = 0.15, foreground = Colors.Gold, background = Colors.DarkBlue, depthmod = 1.0, level = 5, levelmod = 0.4 }},
        };


        public static void InitLetterPropertyList()
        {
            foreach (KeyValuePair<TileTypes, TileTypeProperties> pair in TileTypeProp.OrderBy(key => key.Value.probability))
            {
                SortedTiles.Add(pair.Key, pair.Value);
            }
        }

        public static void InitProbability(int level)
        {
            prob_total = 0;
            TilesForLevel.Clear();

            foreach (KeyValuePair<TileTypes, TileTypeProperties> pair in TileTypeProp)
            {
                if(level >= pair.Value.level)
                {
                    TileProb tp = new TileProb();
                    tp.probability = (pair.Value.prob2 + (int)(pair.Value.levelmod * level));
                    tp.tt = pair.Key;
                    prob_total += tp.probability;
                    TilesForLevel.Add(tp);
                }
            }
        }

        public LetterProp(byte _letter, TileTypes _tt, int _i, int _j)
        {
            i = _i;
            j = _j;
            tt = _tt;

            CreateButton();

            letter = _letter;
        }

        public LetterProp(int level, bool levelup, int _i, int _j)
        {
            i = _i;
            j = _j;
            tt = CreateNewTile(level, levelup);

            CreateButton();

            letter = EngLetterScoring.GetRandomLetter(IsBurning(), WordWarLogic.GetFortune());
        }

        private void CreateButton()
        { 
            b = new Button();
            b.Name = i.ToString() + "," + j.ToString();
            b.Height = WordWarLogic.ButtonHeight - 1;
            b.Width = WordWarLogic.ButtonWidth - 1;
            b.FontSize = GetFontSize();
            b.FontWeight = Windows.UI.Text.FontWeights.Bold;
            b.Margin = new Thickness(0, 0, 0, 0);
            b.Background = GetBackColor();
            b.Foreground = GetForeColor();
            b.Padding = new Thickness(0,-3,0,0);
            b.DataContext = this;
            b.BorderBrush = WordWarLogic.GetFortuneColor();
            b.BorderThickness = new Thickness(1);

            Grid.SetRow(b, j);
            Grid.SetColumn(b, i);
        }

        public static double GetFontSize()
        {
            return WordWarLogic.ButtonHeight / 2 + 10;
        }

        public SolidColorBrush GetBackColor()
        {
            TileTypeProperties ttp = TileTypeProp[tt];
            return new SolidColorBrush(ttp.background);
        }

        public SolidColorBrush GetForeColor()
        {
            TileTypeProperties ttp = TileTypeProp[tt];
            return new SolidColorBrush(ttp.foreground);
        }

        private TileTypes CreateNewTile(int CurrentLevel, bool levelup)
        {
            if (levelup)
            {
                foreach (KeyValuePair<TileTypes, TileTypeProperties> pair in TileTypeProp)
                {
                    if(pair.Value.level == CurrentLevel)
                    {
                        return pair.Key;
                    }
                }
            }

            int index = r.Next(prob_total);
            int sum = 0;
            int i = 0;

            while (sum < index)
            {                
                sum = sum + TilesForLevel[i++].probability;
            }
            return TilesForLevel[Math.Max(0, i - 1)].tt;
        }

        internal bool IsBurning()
        {
            if(tt == TileTypes.Burning)
            {
                return true;
            }
            return false;
        }

        public string LetterPopup()
        {
            string ret = Convert.ToChar(letter).ToString() + "=" + EngLetterScoring.LetterValue(letter).ToString();
            if (GetLetterMult() > 1)
            {
                ret += " Letter x" + GetLetterMult().ToString();
            } else if(GetWordMult() >= 1)
            {
                ret += " Word x" + (GetWordMult()+1).ToString();
            }

            return ret;
        }

        internal int GetLetterMult()
        {
            if (tt == TileTypes.LetterDouble)
            {
                return 2;
            }
            else if (tt == TileTypes.LetterTriple)
            {
                return 3;
            }

            return 1;
        }

        internal int GetWordMult()
        {
            if (tt == TileTypes.WordDouble)
            {
                return 2;
            } else if(tt == TileTypes.WordTriple)
            {
                return 3;
            }

            return 0;
        }

        internal bool IsManna()
        {
            if (tt == TileTypes.Manna)
            {
                return true;
            }
            return false;
        }

        internal void ChangeTileTo(TileTypes _tt)
        {
            switch(_tt)
            {
                case TileTypes.Burning:
                    tt = _tt;
                    b.Foreground = GetForeColor();
                    b.Background = GetBackColor();
                    break;
            }
        }
    }
}
