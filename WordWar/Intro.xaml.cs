using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WordWar
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Intro : Page
    {
        private bool loaded = false;

        public Intro()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void StartGme_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
                await WordWarLogic.InitLogic();

                LoadStuff ld = new LoadStuff();
                ContentDialogResult r = await ld.ShowAsync();

                loaded = true;
            }

            WordWarLogic.FillBestWords(Best_Words);
            WordWarLogic.FillTopScores(HighScores);
            WordWarLogic.FillBestWordsSimple(BestWordSimple);
            WordWarLogic.FillTopLongWords(LongestWord);

            if (WordWarLogic.IsSavedGame())
            {
                this.Frame.Navigate(typeof(MainPage), null);
            }

            //return true;
        }

        private void DebugGame_Click(object sender, RoutedEventArgs e)
        {
            WordWarLogic.DebugMode();
            this.Frame.Navigate(typeof(MainPage), null);
        }

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            WordWarLogic.ResetScores();

            await WordWarLogic.InitLogic();

            WordWarLogic.FillBestWords(Best_Words);
            WordWarLogic.FillTopScores(HighScores);
            WordWarLogic.FillBestWordsSimple(BestWordSimple);
            WordWarLogic.FillTopLongWords(LongestWord);

            //return true;
        }

        private void HighScores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
