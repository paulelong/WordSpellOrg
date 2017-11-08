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
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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

            CurrentWord.Text = "";
            WordScore.Text = "New Game";

            TryList.Items.Clear();
            HistoryList.Items.Clear();
            Spells.RestFoundSpells();
        }

        public static Task BeginAsync(Storyboard storyboard)
        {
            System.Threading.Tasks.TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler<object> onComplete = null;
                onComplete = (s, e) =>
                {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };
                storyboard.Completed += onComplete;
                storyboard.Begin();
            }
            return tcs.Task;
        }

        private async void SubmitWord_Click(object sender, RoutedEventArgs e)
        {
            WordWarLogic.LetterTipPopup.IsOpen = false;

            string s = WordWarLogic.GetCurrentWord().ToLower();

            if (EngLetterScoring.IsWord(s))
            {
                //ScoreFlash.Visibility = Visibility.Visible;
                ScoreFlash.Foreground = WordWarLogic.GetFortuneColor(WordWarLogic.ScoreWord());

                ScoreFlash.Text = s;
                await BeginAsync(ScoreMotionSmall);

                ScoreFlash.Text = WordWarLogic.ScoreWord().ToString();
                await BeginAsync(ScoreMotion);

                WordWarLogic.ScoreStats ss = WordWarLogic.RecordWordScore();
                if(ss.MannaScore > 0)
                {
                    ScoreFlash.Foreground = WordWarLogic.GetFortuneColor(WordWarLogic.ScoreWord());
                    ScoreFlash.Text = "Manna +" + ss.MannaScore;
                    await BeginAsync(ScoreMotionSmall);
                }

                if (ss.bonus > 0)
                {
                    ScoreFlash.Foreground = WordWarLogic.GetFortuneColor(WordWarLogic.ScoreWord());
                    ScoreFlash.Text = "Bonus +" + ss.bonus;
                    await BeginAsync(ScoreMotionSmall);
                }

                if (ss.si != null)
                {
                    Announcment a = new Announcment("Nice word, you've earned a " + ss.si.FriendlyName + " spell.");
                    await a.ShowAsync();
                }

                WordWarLogic.TurnOver();

                if(WordWarLogic.CheckNextLevel(WordWarLogic.totalScore))
                {
                    string levelmsg = "Welcom to Level " + WordWarLogic.CurrentLevel.ToString() + "\n\n";
                    if (Spells.HasSpells())
                    {
                        levelmsg += "You have new spells";
                    }
                    Announcment a = new Announcment(levelmsg);
                    await a.ShowAsync();
                }

                WordWarLogic.RemoveWordAndReplaceTiles();

                WordWarLogic.Deselect();

                bool gameOver = WordWarLogic.ProcessLetters();
                if(gameOver)
                {
                    Announcment a = new Announcment("Game Over");
                    await a.ShowAsync();

                    await WordWarLogic.SaveStats();
                    await WordWarLogic.EndGame();
                    WordWarLogic.Resume = false;

                    this.Frame.GoBack();

                    WordWarLogic.ResetSavedGame();
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
                WordWarLogic.ReadySpell(sc.SelectedSpell, sc.freespell);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            double aw = LetterGrid.ActualWidth;
            WordWarLogic.InitializeLetterButtonGrid(LetterGrid, CurrentWord, TryList, Manna, Eff, Level, Score, WordScore, HistoryList, LetterTip, PopupText, BackgroundGrid.ActualHeight - Lower.ActualHeight);
            
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WordWarLogic.ResizeButtonGrid(BackgroundGrid.ActualWidth, BackgroundGrid.ActualHeight - Lower.ActualHeight);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //await WordWarLogic.SaveGame();
        }
    }
}
