using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WordWar
{
    public sealed partial class SpellChooser : ContentDialog
    {
        int totalManna = 0;
        public SpellInfo SelectedSpell = null;
        public bool freespell = false;

        public SpellChooser(int _totalManna)
        {
            this.InitializeComponent();

            totalManna = _totalManna;

            Spells.ListButtons(SpellCastGrid, SpellFoundGrid, _totalManna, SpellClick, FoundSpellClick);
        }

        private void FoundSpellClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            SpellInfo si = b.DataContext as SpellInfo;

            SelectedSpell = si;
            this.Hide();

            freespell = true;
        }

        private void SpellClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            SpellInfo si = b.DataContext as SpellInfo;

            if (si.MannaPoints <= totalManna)
            {
                SelectedSpell = si;
                this.Hide();
            }

            freespell = false;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
