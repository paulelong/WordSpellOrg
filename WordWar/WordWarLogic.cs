using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace WordWar
{
    partial class WordWarLogic
    {
        [DataContractAttribute]
        public class WordScoreItem
        {
            [DataMember]
            public string word { get; set; }
            [DataMember]
            public string wordscorestring { get; set; }
            [DataMember]
            public int score { get; set; }
            [DataMember]
            public int simplescore { get; set; }
        }

        static List<WordScoreItem> FortuneWordScoreHistory = new List<WordScoreItem>();
        static List<WordScoreItem> BestWordScores = new List<WordScoreItem>();
        static List<WordScoreItem> BestWordScoresSimple = new List<WordScoreItem>();
        static List<WordScoreItem> LongestWords = new List<WordScoreItem>();
        static List<WordScoreItem> AllWords = new List<WordScoreItem>();
        static List<WordScoreItem> HistoryWords = new List<WordScoreItem>();
        static List<int> BestGameScores = new List<int>();

        static List<WordScoreItem> TryWordList = new List<WordScoreItem>();

        const string BestWordScoreFileName = "BestWordScores.txt";
        const string BestWordScoresSimpleFileName = "BestWordScoresSimple.txt";
        const string BestLongestWordsFileName = "BestLongestWords.txt";
        const string BestOverallScoresFileName = "BestOverallScores.txt";
        const string AllWordScoresFileName = "AllWordScores.txt";
        const string FortuneScoresFileName = "FortuneScores.txt";
        const string HistoryScoresFileName = "HistoryScores.txt";

        static Grid LetterGrid;

        static public int ButtonWidth;
        static public int ButtonHeight;

        static private LetterProp[,] LetterPropGrid = null;

        static List<Button> ButtonList = new List<Button>();

        static SolidColorBrush SelectedButtonColor = new SolidColorBrush(Colors.Purple);
        static SolidColorBrush GoodWordColor = new SolidColorBrush(Colors.LimeGreen);
        static SolidColorBrush NormalWordColor = new SolidColorBrush(Colors.White);

        static SolidColorBrush BadFortune = new SolidColorBrush(Colors.SandyBrown);
        static SolidColorBrush GoodFortune = new SolidColorBrush(Colors.Silver);
        static SolidColorBrush GreatFortune = new SolidColorBrush(Colors.Gold);

        public const int gridsize = 10;

        private static int level = 1;

        static Random r = new Random();

        public static TextBlock CurrentWord { get; private set; }
        public static int CurrentLevel { get { return level; } private set { } }

        public static double TotalEfficiency { get; internal set; }

        public static int Manna = 0;

        private static SpellInfo NextSpell = null;

        private static int[] Levels = { 0, 25, 60, 100, 160, 230, 310, 400, 500, 650, 850, 1000, 1300, 1600, 2000, 2500, 3000, 4600, 5200, 10000, 20000, 30000  };
        //private static int[] Levels = { 0, 20, 40, 60, 80, 100, 120, 140, 160, 180, 200, 220, 1300, 1600, 2000, 5000, 10000 };
        private static bool levelup = false;

        public static int totalScore;
        public static int HighScoreWordValue = 0;
        public static string HighScoreWord;
        public static string HighScoreWordTally;
        public static int totalwords = 0;
        private static ListBox TryList;
        public static double Efficiency;
        private const int EffWordCount = 3;
        private const int NumberOfTopScores = 20;

        private static TextBlock LevelText;
        private static TextBlock ScoreText;
        private static TextBlock WordScoreText;
        private static TextBlock MannaScoreText;
        private static TextBlock EffText;
        private static ListBox HistoryList;
        public static Popup LetterTipPopup;
        private static TextBlock PopupText;
        private static int FortuneLevelCount;

        internal static void DebugMode()
        {
            Manna = 100;
            level = 20;
        }


        public enum FortuneLevel
        {
            Bad,
            Good,
            Great,
        }

        public static FortuneLevel GetFortune()
        {
            if (Efficiency > 12)
            {
                return FortuneLevel.Great;
            }
            else if (Efficiency >= 10)
            {
                return FortuneLevel.Good;
            }

            return FortuneLevel.Bad;
        }

        public static FortuneLevel GetFortune(int score)
        {
            if (score > 12)
            {
                return FortuneLevel.Great;
            }
            else if (score >= 10)
            {
                return FortuneLevel.Good;
            }

            return FortuneLevel.Bad;
        }

        public async static Task InitLogic()
        {
            await LoadStats();
        }

        public async static void InitializeLetterButtonGrid(Grid _LetterGrid, TextBlock _CurrentWord, ListBox _TryList, TextBlock _MannaScoreText, TextBlock _effText, TextBlock _levelText, TextBlock _scoreText, TextBlock _wordText, ListBox _historyList, Popup _flyout, TextBlock _popuptext, double h)
        {
            LetterGrid = _LetterGrid;
            CurrentWord = _CurrentWord;
            TryList = _TryList;
            MannaScoreText = _MannaScoreText;
            EffText = _effText;
            LevelText = _levelText;
            ScoreText = _scoreText;
            WordScoreText = _wordText;
            HistoryList = _historyList;
            LetterTipPopup = _flyout;
            PopupText = _popuptext;

            double aw = (int)LetterGrid.ActualWidth;
            double ah = (int)LetterGrid.ActualHeight;

            double bs = Math.Min(aw, h);

            LetterGrid.Width = bs;
            LetterGrid.Height = bs;

            ButtonWidth = (int)(bs / gridsize);
            ButtonHeight = (int)(bs / gridsize);

            // create buttons
            LetterPropGrid = new LetterProp[gridsize, gridsize];

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

            LetterProp.InitProbability(level);

            if(WordWarLogic.IsSavedGame())
            {
                bool gameloaded = await LoadGame();
                if(!gameloaded)
                {
                    NewLetterButtonGrid();
                }
            }
            else
            {
                WordWarLogic.SetSavedGame();
                NewLetterButtonGrid();
            }

            UpdateManaScore();
            UpdateStats();

            Spells.UpdateSpellsForLevel(level);
        }

        internal async static Task SaveGame()
        {
            if(LetterPropGrid != null)
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                // Create a simple setting
                for (int i = 0; i < gridsize; i++)
                {
                    for (int j = 0; j < gridsize; j++)
                    {
                        string key = i.ToString() + "_" + j.ToString();
                        localSettings.Values[key + "_letter"] = LetterPropGrid[i, j].letter;
                        localSettings.Values[key + "_type"] = (int)LetterPropGrid[i, j].TileType;
                    }
                }

                localSettings.Values["level"] = level;
                localSettings.Values["manna"] = Manna;
                localSettings.Values["score"] = totalScore;
                localSettings.Values["totalwords"] = totalwords;
                localSettings.Values["best"] = HighScoreWordTally;
                localSettings.Values["eff"] = Efficiency;

                await SaveList<List<WordScoreItem>>(BestWordScores, BestWordScoreFileName);
                await SaveList<List<WordScoreItem>>(BestWordScoresSimple, BestWordScoresSimpleFileName);
                await SaveList<List<WordScoreItem>>(LongestWords, BestLongestWordsFileName);
                //SaveList<List<WordScoreItem>>(AllWords, AllWordScoresFileName);
                await SaveList<List<WordScoreItem>>(FortuneWordScoreHistory, FortuneScoresFileName);
                await SaveList<List<WordScoreItem>>(HistoryWords, HistoryScoresFileName);
            }
        }

        internal async static Task<bool> LoadGame()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            int w = gridsize - 1;
                        
            if(localSettings.Values.Keys.Contains(w.ToString() + "_" + w.ToString() + "_letter"))
            {
                // Create a simple setting
                for (int i = 0; i < gridsize; i++)
                {
                    for (int j = 0; j < gridsize; j++)
                    {
                        string key = i.ToString() + "_" + j.ToString();
                        byte letter = (byte)localSettings.Values[key + "_letter"];
                        LetterProp.TileTypes tt = (LetterProp.TileTypes)localSettings.Values[key + "_type"];
                        LetterPropGrid[i, j] = new LetterProp(letter, tt, i, j);

                        LetterPropGrid[i, j].b.Click += LetterClick;

                        LetterGrid.Children.Add(LetterPropGrid[i, j].b);
                    }
                }

                level = (int)localSettings.Values["level"];
                Manna = (int)localSettings.Values["manna"];
                totalScore = (int)localSettings.Values["score"];
                totalwords = (int)localSettings.Values["totalwords"];
                HighScoreWordTally = (string)localSettings.Values["best"];
                Efficiency = (double)localSettings.Values["eff"];

                UpdateFortune();

                UpdateStats();
                Spells.UpdateSpellsForLevel(level);
                LetterProp.InitProbability(level);

                BestWordScores = await LoadList<List<WordScoreItem>>(BestWordScoreFileName);
                BestWordScoresSimple = await LoadList<List<WordScoreItem>>(BestWordScoresSimpleFileName);
                LongestWords = await LoadList<List<WordScoreItem>>(BestLongestWordsFileName);
                BestGameScores = await LoadList<List<int>>(BestOverallScoresFileName);
                FortuneWordScoreHistory = await LoadList<List<WordScoreItem>>(FortuneScoresFileName);
                HistoryWords = await LoadList<List<WordScoreItem>>(HistoryScoresFileName);

                if (HistoryWords != null)
                {
                    foreach(WordScoreItem wsi in HistoryWords)
                    {
                        HistoryList.Items.Add(wsi.wordscorestring);
                    }
                }
                else
                {
                    HistoryWords = new List<WordScoreItem>();
                }

                if(FortuneWordScoreHistory == null)
                {
                    FortuneWordScoreHistory = new List<WordScoreItem>();
                }

                if (BestWordScores == null)
                {
                    BestWordScores = new List<WordScoreItem>();
                }

                if (BestWordScoresSimple == null)
                {
                    BestWordScoresSimple = new List<WordScoreItem>();
                }

                if (LongestWords == null)
                {
                    LongestWords = new List<WordScoreItem>();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static void UpdateManaScore()
        {
            MannaScoreText.Text = "M: " + Manna.ToString();
        }

        internal static SolidColorBrush GetFortuneColor()
        {
            switch (WordWarLogic.GetFortune())
            {
                case WordWarLogic.FortuneLevel.Bad:
                    return (BadFortune);
                case WordWarLogic.FortuneLevel.Good:
                    return (GoodFortune);
                case WordWarLogic.FortuneLevel.Great:
                    return (GreatFortune);
            }

            return BadFortune;
        }

        internal static SolidColorBrush GetFortuneColor(int score)
        {
            switch (WordWarLogic.GetFortune(score))
            {
                case WordWarLogic.FortuneLevel.Bad:
                    return (BadFortune);
                case WordWarLogic.FortuneLevel.Good:
                    return (GoodFortune);
                case WordWarLogic.FortuneLevel.Great:
                    return (GreatFortune);
            }

            return BadFortune;
        }

        internal static void UpdateFortune()
        {
            SolidColorBrush scb = GetFortuneColor();
            SetGridColor(scb);
            EffText.Foreground = scb;
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
            LetterPropGrid[i, j] = new LetterProp(level, levelup, i, j);

            if (levelup == true)
            {
                levelup = false;
            }

            LetterPropGrid[i, j].b.Click += LetterClick;
            LetterPropGrid[i, j].b.Holding += LetterToolTip;
            LetterPropGrid[i, j].b.RightTapped += LetterRightTapped;

            LetterGrid.Children.Add(LetterPropGrid[i, j].b);
        }

        private static void LetterRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Button b = sender as Button;
            LetterProp lp = b.DataContext as LetterProp;

            Point p = e.GetPosition(LetterGrid);
            LetterTipPopup.VerticalOffset = p.Y - 40;
            LetterTipPopup.HorizontalOffset = p.X - 30;
            //LetterTipPopup. = 
            PopupText.Text = lp.LetterPopup();
            LetterTipPopup.IsOpen = true;
        }

        private static void LetterToolTip(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                Button b = sender as Button;
                LetterProp lp = b.DataContext as LetterProp;

                Point p = e.GetPosition(LetterGrid);
                LetterTipPopup.VerticalOffset = p.Y - 40;
                LetterTipPopup.HorizontalOffset = p.X - 30;
                PopupText.Text = lp.LetterPopup();
                LetterTipPopup.IsOpen = true;
            }
            else if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
            {
                LetterTipPopup.IsOpen = false;
            }
        }

        public static void SetGridColor(SolidColorBrush brush)
        {
            for (int i = 0; i < WordWarLogic.gridsize; i++)
            {
                for (int j = 0; j < WordWarLogic.gridsize; j++)
                {
                    LetterPropGrid[i, j].b.BorderBrush = brush;
                }
            }
        }

        private async static void LetterClick(object sender, RoutedEventArgs e)
        {
            LetterTipPopup.IsOpen = false;

            Button b = sender as Button;

            if (NextSpell != null)
            {
                SpellInfo.SpellOut so = await CastSpell(NextSpell, b.DataContext as LetterProp);

                if(so.si == null && so.worked)
                {
                    if (freeSpell)
                    {
                        Spells.RemoveFoundSpell(NextSpell);
                    }
                    else
                    {
                        ChangeManna(-NextSpell.MannaPoints);
                    }
                }
                NextSpell = so.si;
            }
            else
            {
                if (ButtonList.Count >= 0)
                {
                    string curword = GetCurrentWord().ToLower();
                    // Check if button is adject to the last
                    if (!(IsLetterAdjacentToLastButton(b) && !ButtonList.Contains(b)))
                    {
                        Deselect();
                    }
                    else
                    {
                    }
                    b.Background = SelectedButtonColor;
                    ButtonList.Add(b);

                    // add if for > 3 letters
                    CurrentWord.Text = GetWordTally();

                    // if it's a word, update color to green
                    if (ButtonList.Count > 2 && EngLetterScoring.IsWord(GetCurrentWord()))
                    {
                        CurrentWord.Foreground = GoodWordColor;

                        // if it's a word, remember it.
                        AddToTryList();
                    }
                    else
                    {
                        CurrentWord.Foreground = NormalWordColor;
                    }
                }
            }
        }

        private static void AddToTryList()
        {
            WordScoreItem wsi = new WordScoreItem() { word = GetCurrentWord(), score = WordWarLogic.ScoreWord(), wordscorestring = EngLetterScoring.GetWordTally(ButtonList), simplescore = WordWarLogic.ScoreWordSimple() };
            int indx = TryWordList.FindIndex(f => (f.score < wsi.score));
            if (indx >= 0)
            {
                TryWordList.Insert(indx, wsi);
            }
            else
            {
                TryWordList.Add(wsi);
            }

            TryList.Items.Clear();
            foreach(WordScoreItem wsi_I in TryWordList)
            {
                TryList.Items.Add(wsi_I.word + " " + wsi_I.score.ToString());
            }
        }


        internal static void TurnOver()
        {
            TryList.Items.Clear();
            TryWordList.Clear();
        }

        internal static void Replay()
        {
            totalScore = 0;
            level = 1;
            TotalEfficiency = 0;
            Efficiency = 0;
            HighScoreWordValue = 0;
            HighScoreWord = "";
            totalwords = 0;
            Manna = 0;

            ButtonList.Clear();

            LetterGrid.Children.Clear();

            NewLetterButtonGrid();
        }

        internal static bool CheckNextLevel(int totalScore)
        {
            if (level < Levels.Length - 1 && totalScore >= Levels[level])
            {
                level++;
                levelup = true;

                LevelText.Text = "L: " + CurrentLevel.ToString();

                if (level >= 5)
                {
                    ChangeManna(6);
                }

                Spells.UpdateSpellsForLevel(level);
                LetterProp.InitProbability(level);
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

        public struct ScoreStats
        {
            public int MannaScore;
            public SpellInfo si;
            public int bonus;
        }

        internal static ScoreStats RecordWordScore()
        {
            ScoreStats ss = new ScoreStats();

            int wordTotal = WordWarLogic.ScoreWord();

            HistoryList.Items.Insert(0, GetWordTally());

            totalScore += wordTotal;
            if (wordTotal > WordWarLogic.HighScoreWordValue)
            {
                WordWarLogic.HighScoreWordValue = wordTotal;
                WordWarLogic.HighScoreWord = GetCurrentWord();
                HighScoreWordTally = EngLetterScoring.GetWordTally(ButtonList);
            }

            WordScoreItem wsi = new WordScoreItem() { word = GetCurrentWord(), score = wordTotal, wordscorestring = EngLetterScoring.GetWordTally(ButtonList), simplescore = WordWarLogic.ScoreWordSimple() };

            ss.bonus = EngLetterScoring.LengthBonus(wsi.word);

            FortuneWordScoreHistory.Add(wsi);
            if (FortuneWordScoreHistory.Count > EffWordCount)
            {
                FortuneWordScoreHistory.RemoveAt(0);
            }

            HistoryWords.Add(wsi);

            CheckTopBestWordScores(wsi);
            CheckTopBestWordScoresSimple(wsi);
            CheckTopLongestWordScores(wsi);

            WordWarLogic.totalwords++;

            TotalEfficiency = totalScore / totalwords;
            Efficiency = GetLatestEff();

            if(GetFortune() == FortuneLevel.Great)
            {
                FortuneLevelCount++;
            }
            else
            {
                FortuneLevelCount = 0;
            }

            if (FortuneLevelCount > 4)
            {
                Manna += (FortuneLevelCount - 4);
            }
            ss.MannaScore = ScoreManna();

            // If it's a big or price word, give them a spell based on the word.
            string curword = GetCurrentWord();
            if (wordTotal > 14 || curword.Length >= 8)
            {
                SpellInfo si = null;

                if (wordTotal > 70 || curword.Length > 16)
                {
                    si = Spells.GetSpell(6, level);
                }
                else if (wordTotal > 55 || curword.Length > 15)
                {
                    si = Spells.GetSpell(5, level);
                }
                else if (wordTotal > 45 || curword.Length > 14)
                {
                    si = Spells.GetSpell(4, level);
                } else if(wordTotal > 35 || curword.Length > 13)
                {
                    si = Spells.GetSpell(3, level);
                } else if(wordTotal > 25 || curword.Length > 11)
                {
                    si = Spells.GetSpell(2, level);
                } else if(wordTotal > 17 || curword.Length > 9)
                {
                    si = Spells.GetSpell(1, level);
                } else if(wordTotal > 14 || curword.Length >= 8)
                {
                    si = Spells.GetSpell(0, level);
                }

                ss.si = si;

            }

            UpdateFortune();

            UpdateStats();

            return ss;
        }

        public static bool IsSavedGame()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.Keys.Contains("SavedGame"))
            {
                return true;
            }

            return false;
        }

        public static void SetSavedGame()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["SavedGame"] = 1;
        }

        public static void ResetSavedGame()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.Keys.Contains("SavedGame"))
            {
                localSettings.Values.Remove("SavedGame");
            }
        }


        private static void UpdateStats()
        {
            WordScoreText.Text = "Best: " + HighScoreWordTally;

            ScoreText.Text = totalScore.ToString();

            if (totalwords > 0)
            {
                TotalEfficiency = totalScore / totalwords;
            }
            EffText.Text = "E: " + Efficiency.ToString("#.#") + "(" + TotalEfficiency.ToString("#.##") + ")";

            LevelText.Text = "L: " + level.ToString();

            MannaScoreText.Text = "M: " + Manna.ToString();
        }

        private static void CheckTopLongestWordScores(WordScoreItem wsi)
        {
            int indx = LongestWords.FindIndex(f => (f.word.Length < wsi.word.Length));
            if (indx >= 0)
            {
                LongestWords.Insert(indx, wsi);
            }
            else
            {
                LongestWords.Add(wsi);
            }
            if (LongestWords.Count > NumberOfTopScores)
            {
                LongestWords.RemoveAt(NumberOfTopScores);
            }
        }

        private static void CheckTopBestWordScoresSimple(WordScoreItem wsi)
        {
            int indx = BestWordScoresSimple.FindIndex(f => (f.score < wsi.simplescore));
            if (indx >= 0)
            {
                BestWordScoresSimple.Insert(indx, wsi);
            }
            else
            {
                BestWordScoresSimple.Add(wsi);
            }
            if (BestWordScoresSimple.Count > NumberOfTopScores)
            {
                BestWordScoresSimple.RemoveAt(NumberOfTopScores);
            }
        }

        private static void CheckTopBestWordScores(WordScoreItem wsi)
        {
            int indx = BestWordScores.FindIndex(f => (f.score < wsi.score));
            if (indx >= 0)
            {
                BestWordScores.Insert(indx, wsi);
            }
            else
            {
                BestWordScores.Add(wsi);
            }
            if (BestWordScores.Count > NumberOfTopScores)
            {
                BestWordScores.RemoveAt(NumberOfTopScores);
            }
        }

        private static int ScoreWordSimple()
        {
            return EngLetterScoring.ScoreWordSimple(ButtonList);
        }

        private static double GetLatestEff()
        {
            int wordtotal = 0;
            foreach (WordScoreItem wsi in FortuneWordScoreHistory)
            {
                wordtotal += wsi.score;
            }

            return (double)wordtotal / (double)FortuneWordScoreHistory.Count;
        }

        public static int ScoreManna()
        {
            int addedManna = EngLetterScoring.ScoreManna(ButtonList);
            Manna += addedManna;
            UpdateManaScore();

            return addedManna;            
        }

        private static bool IsLetterAdjacentToLastButton(Button b)
        {
            if (ButtonList.Count <= 0)
            {
                return true;
            }

            LetterProp lp1 = b.DataContext as LetterProp;
            LetterProp lp2 = ButtonList.Last().DataContext as LetterProp;

            if (Math.Abs(lp1.i - lp2.i) <= 1 && Math.Abs(lp1.j - lp2.j) <= 1)
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
            for (int i = gridsize - 1; i >= 0; i--)
            {
                for (int j = gridsize - 1; j >= 0; j--)
                {
                    LetterProp curlp = LetterPropGrid[i, j];
                    if (curlp.IsBurning())
                    {
                        if (curlp.j >= gridsize - 1)
                        {
                            // GameOver
                            return true;
                        }

                        RemoveAndReplaceTile(curlp.i, curlp.j + 1);
                    }
                }
            }

            WordWarAI wwai = new WordWarAI(LetterPropGrid);
            return (!wwai.AnyWords());
        }

        public static void RemoveAndReplaceTile(int i, int j)
        {
            LetterGrid.Children.Remove(LetterPropGrid[i, j].b);

            for (int jp = j; jp > 0; jp--)
            {
                LetterPropGrid[i, jp] = LetterPropGrid[i, jp - 1];
                Grid.SetRow(LetterPropGrid[i, jp].b, jp);
                LetterPropGrid[i, jp].j = jp;
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

        internal static string GetCurrentWord()
        {
            return EngLetterScoring.GetCurrentWord(ButtonList);
        }

        internal async static Task SaveStats()
        {
            // Add the latest high score
            if(BestGameScores == null)
            {
                BestGameScores = new List<int>();
            }

            int indx = BestGameScores.FindIndex(f => (f < totalScore));
            if (indx >= 0)
            {
                BestGameScores.Insert(indx, totalScore);
            }
            else
            {
                BestGameScores.Add(totalScore);
            }
            if (BestGameScores.Count > NumberOfTopScores)
            {
                BestGameScores.RemoveAt(NumberOfTopScores);
            }

            // Save Best Word, Best Game stats
            await SaveList<List<WordScoreItem>>(BestWordScores, BestWordScoreFileName);
            await SaveList<List<WordScoreItem>>(BestWordScoresSimple, BestWordScoresSimpleFileName);
            await SaveList<List<WordScoreItem>>(LongestWords, BestLongestWordsFileName);
            await SaveList<List<int>>(BestGameScores, BestOverallScoresFileName);
        }

        internal static async Task SaveList<T>(T list, string file)
        {
                try
                {
                    StorageFile sampleFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);

                    using(IRandomAccessStream s = await sampleFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                        dcs.WriteObject(s.AsStreamForWrite(), list);
                        await s.FlushAsync();
                    }
                }

                catch (Exception ex)
                {
                    Announcment a = new Announcment(file + " " + ex.Message);
                    await a.ShowAsync();
                }
        }

        internal static async Task LoadStats()
        {
            // Save Best Word, Best Game stats
            BestWordScores = await LoadList<List<WordScoreItem>>(BestWordScoreFileName);
            BestWordScoresSimple = await LoadList<List<WordScoreItem>>(BestWordScoresSimpleFileName);
            LongestWords = await LoadList<List<WordScoreItem>>(BestLongestWordsFileName);
            BestGameScores = await LoadList<List<int>>(BestOverallScoresFileName);

            if(BestWordScores == null)
            {
                BestWordScores = new List<WordScoreItem>();
            }

            if (LongestWords == null)
            {
                LongestWords = new List<WordScoreItem>();
            }
            if (BestWordScoresSimple == null)
            {
                BestWordScoresSimple = new List<WordScoreItem>();
            }
            if (BestGameScores == null)
            {
                BestGameScores = new List<int>();
            }
        }

        internal static async Task<T> LoadList<T>(string file)
        {
          
            StorageFile sampleFile = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
            try
            {
                if (sampleFile != null)
                {
                    T list;
                    using (IInputStream s = await sampleFile.OpenReadAsync())
                    {
                        DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                        list = (T)dcs.ReadObject(s.AsStreamForRead());
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                if(!(ex is System.Xml.XmlException))
                {
                    Announcment a = new Announcment(file + " " + ex.Message);
                    await a.ShowAsync();
                }
            }

            return default(T);
        }


        internal static void FillBestWords(ListBox best_Words)
        {
            best_Words.Items.Clear();

            foreach(WordScoreItem wsi in BestWordScores)
            {
                best_Words.Items.Add(wsi.wordscorestring);               
            }
        }

        internal static void FillBestWordsSimple(ListBox best_Words)
        {
            best_Words.Items.Clear();

            foreach (WordScoreItem wsi in BestWordScoresSimple)
            {
                best_Words.Items.Add(wsi.word + " " + wsi.simplescore);
            }
        }

        internal static void FillTopScores(ListBox highScores)
        {
            highScores.Items.Clear();

            foreach (int score in BestGameScores)
            {
                highScores.Items.Add(score.ToString());
            }
        }

        internal static void FillTopLongWords(ListBox highScores)
        {
            highScores.Items.Clear();

            foreach (WordScoreItem wsi in LongestWords)
            {
                highScores.Items.Add(wsi.word);
            }
        }

        internal async static void ResetScores()
        {
            StorageFile sf = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(BestWordScoreFileName, CreationCollisionOption.ReplaceExisting);
            await sf.DeleteAsync();
            sf = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(BestWordScoresSimpleFileName, CreationCollisionOption.ReplaceExisting);
            await sf.DeleteAsync();
            sf = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(BestLongestWordsFileName, CreationCollisionOption.ReplaceExisting);
            await sf.DeleteAsync();
            sf = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(BestOverallScoresFileName, CreationCollisionOption.ReplaceExisting);
            await sf.DeleteAsync();

            BestGameScores.Clear();
            BestWordScores.Clear();
            BestWordScoresSimple.Clear();
            LongestWords.Clear();

            Efficiency = 0;
            level = 0;
            totalScore = 0;
            Manna = 0;
        }

        internal async static Task EndGame()
        {
            // Remove saved game setings.
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            int w = gridsize - 1;

            if (localSettings.Values.Keys.Contains(w.ToString() + "_" + w.ToString() + "_letter"))
            {
                localSettings.Values.Remove(w.ToString() + "_" + w.ToString() + "_letter");
            }

            FortuneWordScoreHistory.Clear();
            AllWords.Clear();

            StorageFile sf = await ApplicationData.Current.RoamingFolder.CreateFileAsync(FortuneScoresFileName, CreationCollisionOption.ReplaceExisting);
            await sf.DeleteAsync();
            sf = await ApplicationData.Current.RoamingFolder.CreateFileAsync(AllWordScoresFileName, CreationCollisionOption.ReplaceExisting);
            await sf.DeleteAsync();

            Efficiency = 0;
            level = 0;
            totalScore = 0;
            Manna = 0;
        }
    }
}
