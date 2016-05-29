using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WordWar
{
    partial class WordWarLogic
    {
        static Grid LetterGrid;

        static public int ButtonWidth;
        static public int ButtonHeight;

        static private LetterProp[,] LetterButtons = null;
        static List<Button> ButtonList = new List<Button>();

        static SolidColorBrush SelectedButtonColor = new SolidColorBrush(Colors.Purple);
        static SolidColorBrush GoodWordColor = new SolidColorBrush(Colors.Green);
        static SolidColorBrush NormalWordColor = new SolidColorBrush(Colors.White);

        public const int gridsize = 10;

        private static int level = 1;

        static Random r = new Random();        

        public static TextBlock CurrentWord { get; private set; }
        public static int CurrentLevel { get { return level; } private set { } }

        public static double Efficiency { get; internal set; }

        public static int Manna = 0;

        private static SpellInfo NextSpell = null;

        private static int[] Levels = {0, 25, 60, 100, 160, 230, 310, 400, 500, 650, 850, 1000, 1300, 1600, 2000, 5000, 10000 };
        private static bool levelup = false;

        public static int totalScore;
        public static int HighScoreWordValue = 0;
        public static string HighScoreWord;
        public static string HighScoreWordTally;
        public static  int totalwords = 0;
        private static ListBox TryList;

        public static void InitializeLetterButtonGrid(Grid _LetterGrid, TextBlock _CurrentWord, ListBox _TryList)
        {
            LetterGrid = _LetterGrid;
            CurrentWord = _CurrentWord;
            TryList = _TryList;

            ButtonWidth = (int)LetterGrid.Width / gridsize;
            ButtonHeight = (int)LetterGrid.Height / gridsize;

            // create buttons
            LetterButtons = new LetterProp[gridsize, gridsize];

            RowDefinition[] rows = new RowDefinition[gridsize];
            ColumnDefinition[] columns = new ColumnDefinition[gridsize];

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new ColumnDefinition();
                LetterGrid.ColumnDefinitions.Add(columns[i]);
            }

            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new RowDefinition();
                LetterGrid.RowDefinitions.Add(rows[i]);
            }

            NewLetterButtonGrid();
        }

        internal static string GetWordTally()
        {
            return EngLetterScoring.GetWordTally(ButtonList);
        }

        private static void NewLetterButtonGrid()
        {
            for (int i = 0; i < gridsize; i++)
            {
                for (int j = 0; j < gridsize; j++)
                {
                    NewLetter(i, j);
                }
            }
        }

        private static void NewLetter(int i, int j)
        {
            LetterButtons[i, j] = new LetterProp(level, levelup, i, j);

            if(levelup == true)
            {
                levelup = false;
            }

            LetterButtons[i, j].b.Click += LetterClick;

            LetterGrid.Children.Add(LetterButtons[i, j].b);
        }

        private static void LetterClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            if(NextSpell != null)
            {
                Manna -= NextSpell.MannaPoints;
                NextSpell = CastSpell(NextSpell, b);
            }
            else
            {
                if (ButtonList.Count >= 0)
                {
                    string curword = GetCurrentWord().ToLower();
                    // Check if button is adject to the last
                    if (!(IsLetterAdjacentToLastButton(b) && !ButtonList.Contains(b)))
                    {
                        // if it's a word, remember it.
                        if (EngLetterScoring.DictionaryLookup.Contains(curword))
                        {
                            TryList.Items.Insert(0, curword + " " + ScoreWord());
                        }

                        Deselect();
                    }
                    else
                    {
                    }
                    b.Background = SelectedButtonColor;
                    ButtonList.Add(b);
                    CurrentWord.Text = GetWordTally();

                    // if it's a word, update color to green
                    if (ButtonList.Count > 2 && EngLetterScoring.DictionaryLookup.Contains(GetCurrentWord().ToLower()))
                    {
                        CurrentWord.Foreground = GoodWordColor;
                    }
                    else
                    {
                        CurrentWord.Foreground = NormalWordColor;
                    }
                }
            }
        }


        internal static void Replay()
        {          
            totalScore = 0;
            level = 1;
            Efficiency = 0;            
            HighScoreWordValue = 0;
            HighScoreWord = "";
            totalwords = 0;
            Manna = 100;

            ButtonList.Clear();

            LetterGrid.Children.Clear();

            NewLetterButtonGrid();
        }

        internal static bool CheckNextLevel(int totalScore)
        {
            if(totalScore >= Levels[level])
            {
                level++;
                levelup = true;

                if(level >= 5)
                {
                    Manna += 6;
                }

                Spells.UpdateSpellsForLevel(level);
            }

            return levelup;
        }

        internal static int ScoreWord()
        {
            return EngLetterScoring.ScoreWord(ButtonList);
        }

        internal static string ScoreWordString()
        {
            return EngLetterScoring.ScoreWordString(ButtonList);
        }

        internal static void RecordWordScore()
        {
            int wordTotal = WordWarLogic.ScoreWord();

            totalScore += wordTotal;
            if (wordTotal > WordWarLogic.HighScoreWordValue)
            {
                WordWarLogic.HighScoreWordValue = wordTotal;
                WordWarLogic.HighScoreWord = GetCurrentWord() ;
                HighScoreWordTally = EngLetterScoring.GetWordTally(ButtonList); 
            }

            ScoreManna();

            WordWarLogic.totalwords++;

            Efficiency = totalScore / totalwords;
        }

        private static void ScoreManna()
        {
            Manna += EngLetterScoring.ScoreManna(ButtonList);
        }

        private static bool IsLetterAdjacentToLastButton(Button b)
        {
            if (ButtonList.Count <= 0)
            {
                return true;
            }

            LetterProp lp1 = b.DataContext as LetterProp;
            LetterProp lp2 = ButtonList.Last().DataContext as LetterProp;

            if(Math.Abs(lp1.i - lp2.i) <= 1 && Math.Abs(lp1.j - lp2.j) <= 1)
            {
                return true;
            }

            return false;
        }

        public static void Deselect()
        {
            // Deselect everything and add this as the start
            foreach (Button b in ButtonList)
            {
                LetterProp curlp = b.DataContext as LetterProp;
                b.Background = curlp.GetBackColor();
            }

            ButtonList.Clear();
        }

        internal static bool ProcessLetters()
        {
            for(int i= gridsize-1; i>=0;i--)
            {
                for(int j= gridsize-1; j>=0;j--)
                {
                    LetterProp curlp = LetterButtons[i,j];
                    if (curlp.IsBurning())
                    {
                        if(curlp.j >= gridsize - 1)
                        {
                            // GameOver
                            return true;
                        }

                        RemoveAndReplaceTile(curlp.i, curlp.j + 1);
                    }
                }
            }

            return false;
        }

        public static void RemoveAndReplaceTile(int i, int j)
        {
            LetterGrid.Children.Remove(LetterButtons[i, j].b);

            for (int jp = j; jp > 0; jp--)
            {
                LetterButtons[i, jp] = LetterButtons[i, jp - 1];
                Grid.SetRow(LetterButtons[i, jp].b, jp);
                LetterButtons[i, jp].j = jp;
            }

            NewLetter(i, 0);
        }

        public static void RemoveWordAndReplaceTiles()
        {
            foreach (Button b in ButtonList)
            {
                LetterProp lp = b.DataContext as LetterProp;
                RemoveAndReplaceTile(lp.i, lp.j);
            }
        }
    }
}
