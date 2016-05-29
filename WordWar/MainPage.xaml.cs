using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WordWar
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Random r = new Random();

        SolidColorBrush NormalButtonColor = new SolidColorBrush(ColorHelper.FromArgb(255, 187, 187, 187));
        SolidColorBrush SelectedButtonColor = new SolidColorBrush(ColorHelper.FromArgb(255, 200, 130, 130));


        public MainPage()
        {
            this.InitializeComponent();

            LetterProp.InitLetterPropertyList();

            CurrentWord.Text = "";
            WordScore.Text = "New Game";
            Score.Text = "0";
            Level.Text = "L: 1";
            Eff.Text = "E: 0";
            Manna.Text = "M: 0";

            TryList.Items.Clear();
            HistoryList.Items.Clear();


            EngLetterScoring.LoadDictionary();

            WordWarLogic.InitializeLetterButtonGrid(LetterGrid, CurrentWord, TryList);
        }

        private async void SubmitWord_Click(object sender, RoutedEventArgs e)
        {

            string s = WordWarLogic.GetCurrentWord().ToLower();

            if (EngLetterScoring.DictionaryLookup.Contains(s))
            {
                WordWarLogic.RecordWordScore();

                WordScore.Text = "Best: " + WordWarLogic.HighScoreWordTally;

                Score.Text = WordWarLogic.totalScore.ToString();
                HistoryList.Items.Insert(0, WordWarLogic.GetWordTally());
                TryList.Items.Clear();

                if(WordWarLogic.CheckNextLevel(WordWarLogic.totalScore))
                {
                    string levelmsg = "Welcom to Level " + WordWarLogic.CurrentLevel.ToString() + "\n\n";
                    if (Spells.HasSpells())
                    {
                        levelmsg += "You have new spells";
                    }
                    Announcment a = new Announcment(levelmsg);
                    await a.ShowAsync();

                    Level.Text = "L: " + WordWarLogic.CurrentLevel.ToString();
                }

                Manna.Text = "M: " + WordWarLogic.Manna;

                Eff.Text = "E: " + WordWarLogic.Efficiency.ToString();

                WordWarLogic.RemoveWordAndReplaceTiles();

                WordWarLogic.Deselect();

                bool gameOver = WordWarLogic.ProcessLetters();
                if(gameOver)
                {
                    Announcment a = new Announcment("Game Over");
                    await a.ShowAsync();

                    WordWarLogic.Replay();
                    CurrentWord.Text = "";
                    WordScore.Text = "New Game";
                    Score.Text = "0";
                    Level.Text = "L: 1";
                    Eff.Text = "E: 0";
                    Manna.Text = "M: 0";

                    TryList.Items.Clear();
                    HistoryList.Items.Clear();
                }
            }
            else
            {
                WordWarLogic.Deselect();

                CurrentWord.Text = "Not a known word.  Try again";
            }
        }

        private async void Spell_Click(object sender, RoutedEventArgs e)
        {
            SpellChooser sc = new SpellChooser(WordWarLogic.Manna);

            await sc.ShowAsync();

            if(sc.SelectedSpell !=  null)
            {
                WordWarLogic.Manna -= sc.SelectedSpell.MannaPoints;

                WordWarLogic.ReadySpell(sc.SelectedSpell);
            }
        }
    }
}
