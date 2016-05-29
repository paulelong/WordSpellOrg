using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WordWar
{
    class LetterProp
    {
        static Random r = new Random();

        public int i, j;
        private TileType tt;
        public Button b;

        public enum TileType
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
            public Color foreground;
            public Color background;
            public double depthmod;
            public int level;
            public double levelmod;
        }

        static Dictionary<TileType, TileTypeProperties> TileTypeProp = new Dictionary<TileType, TileTypeProperties>
        {
            {TileType.Normal, new TileTypeProperties { probability = 1.0, foreground = Colors.Black, background = Colors.DarkGray, depthmod = 0.01, level = 0, levelmod = 0.0 } },
            {TileType.LetterDouble, new TileTypeProperties {probability = 0.08, foreground = Colors.Black, background = Colors.LightPink, depthmod = 0.01, level = 2, levelmod = 0.002 }},
            {TileType.WordDouble, new TileTypeProperties {probability = 0.05, foreground = Colors.Black, background = Colors.LightCyan, depthmod = 0.01, level = 6, levelmod = 0.003 }},
            {TileType.LetterTriple, new TileTypeProperties {probability = 0.04, foreground = Colors.Black, background = Colors.Pink, depthmod = 0.01, level = 7, levelmod = 0.002 }},
            {TileType.WordTriple, new TileTypeProperties {probability = 0.03, foreground = Colors.Black, background = Colors.Cyan, depthmod = 0.01, level = 4, levelmod = 0.002 }},
            {TileType.Burning, new TileTypeProperties{probability = 0.12, foreground = Colors.Red, background = Colors.LightGray, depthmod = 1.0, level = 3, levelmod = 0.004 }},
            {TileType.Manna, new TileTypeProperties{probability = 0.15, foreground = Colors.Gold, background = Colors.DarkBlue, depthmod = 1.0, level = 5, levelmod = 0.005 }},
        };

        static Dictionary<TileType, TileTypeProperties> SortedTiles = new Dictionary<TileType, TileTypeProperties>();

        public static void InitLetterPropertyList()
        {
            foreach (KeyValuePair<TileType, TileTypeProperties> pair in TileTypeProp.OrderBy(key => key.Value.probability))
            {
                SortedTiles.Add(pair.Key, pair.Value);
            }
        }

        public LetterProp(int level, bool levelup, int _i, int _j)
        {
            i = _i;
            j = _j;
            tt = CreateNewTile(level, levelup);

            b = new Button();
            b.Name = i.ToString() + "," + j.ToString();
            b.Height = WordWarLogic.ButtonHeight;
            b.Width = WordWarLogic.ButtonWidth;
            b.FontSize = 24;
            b.FontWeight = Windows.UI.Text.FontWeights.Bold;
            b.Content = EngLetterScoring.GetRandomLetter(IsBurning());
            b.Margin = new Thickness(1);
            b.Background = GetBackColor();
            b.Foreground = GetForeColor();
            b.Padding = new Thickness(1);
            b.DataContext = this;

            Grid.SetRow(b, j);
            Grid.SetColumn(b, i);
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

        private TileType CreateNewTile(int CurrentLevel, bool levelup)
        {
            double val = r.NextDouble();

            foreach(KeyValuePair<TileType, TileTypeProperties> pair in SortedTiles)
            {
                if ((pair.Value.level <= CurrentLevel && val < (pair.Value.probability + (CurrentLevel * pair.Value.levelmod))) || (levelup && pair.Value.level == CurrentLevel))
                {
                    return pair.Key;
                }
            }

            return TileType.Normal;
        }

        internal bool IsBurning()
        {
            if(tt == TileType.Burning)
            {
                return true;
            }
            return false;
        }

        internal int GetLetterMult()
        {
            if (tt == TileType.LetterDouble)
            {
                return 2;
            }
            else if (tt == TileType.LetterTriple)
            {
                return 3;
            }

            return 1;
        }

        internal int GetWordMult()
        {
            if (tt == TileType.WordDouble)
            {
                return 2;
            } else if(tt == TileType.WordDouble)
            {
                return 3;
            }

            return 1;
        }

        internal bool IsManna()
        {
            if (tt == TileType.Manna)
            {
                return true;
            }
            return false;
        }

        internal void ChangeTileTo(Button b, TileType _tt)
        {
            switch(_tt)
            {
                case TileType.Burning:
                    tt = _tt;
                    b.Foreground = GetForeColor();
                    b.Background = GetBackColor();
                    break;
            }
        }
    }
}
