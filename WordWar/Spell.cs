using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WordWar
{
    public class SpellInfo
    {
        public enum SpellType
        {
            DestroyLetter,
            DestroyGroup,
            WordHint,
            RandomVowels,
            Burn,
            LetterSwap,
            ChangeToVowel,
            ConvertLetter,
        };

        public SpellType spellType;
        public string FriendlyName;
        public int MannaPoints;
        public int SpellLevel;
        public bool Immediate;
    }

    class Spells
    {
        private const int SpellPerRow = 3;


        static List<SpellInfo> AllSpells = new List<SpellInfo>
        {
            { new SpellInfo { spellType = SpellInfo.SpellType.DestroyLetter, FriendlyName = "Snipe", MannaPoints = 7, SpellLevel = 13, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.DestroyGroup, FriendlyName = "Bomb", MannaPoints = 20, SpellLevel = 12, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ChangeToVowel, FriendlyName = "Vowelize", MannaPoints = 7, SpellLevel = 10, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.WordHint, FriendlyName = "Hint", MannaPoints = 10, SpellLevel = 1, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.Burn, FriendlyName = "Burn" ,MannaPoints = 6, SpellLevel = 6, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.LetterSwap, FriendlyName = "Swap", MannaPoints = 6, SpellLevel = 5, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RandomVowels, FriendlyName = "Vowel Dust", MannaPoints = 8, SpellLevel = 11, Immediate = true }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ConvertLetter, FriendlyName = "Convert", MannaPoints = 15, SpellLevel = 8, Immediate = false }},
        };

        static List<SpellInfo> AvailableSpells = new List<SpellInfo>();

        public static void ListButtons(Grid g, int manna, RoutedEventHandler spellClick)
        {
            int buttonwidth = (int)(g.Width / SpellPerRow);

            RowDefinition[] rows = new RowDefinition[5];
            ColumnDefinition[] columns = new ColumnDefinition[SpellPerRow];

            for (int x = 0; x < columns.Length; x++)
            {
                columns[x] = new ColumnDefinition();
                g.ColumnDefinitions.Add(columns[x]);
            }

            for (int x = 0; x < rows.Length; x++)
            {
                rows[x] = new RowDefinition();
                g.RowDefinitions.Add(rows[x]);
            }

            int i = 0, j = 0;

            foreach (SpellInfo si in AvailableSpells)
            {
                Button b = new Button();
                b.Content = si.FriendlyName;
                b.DataContext = si;
                b.Click += spellClick;
                b.FontSize = 16;
                b.Margin = new Thickness(1);
                b.Padding = new Thickness(1);
                b.Width = 100;

                if (manna < si.MannaPoints)
                {
                    b.IsEnabled = false;
                }

                Grid.SetRow(b, j);
                Grid.SetColumn(b, i);

                g.Children.Add(b);

                i++;
                if (i >= SpellPerRow)
                {
                    i = 0;
                    j++;
                }
            }
        }

        internal static void UpdateSpellsForLevel(int level)
        {
            AvailableSpells.Clear();
            foreach (SpellInfo si in AllSpells)
            {
                if (level >= si.SpellLevel)
                {
                    AvailableSpells.Add(si);
                }
            }
        }

        internal static bool HasSpells()
        {
            return (AvailableSpells.Count > 0);
        }
    }

    partial class WordWarLogic
    {
        private static int LetterSwapStep;
        private static Button LetterSwapFirst;

        private static SpellInfo CastSpell(SpellInfo si, Button b)
        {
            switch (si.spellType)
            {
                case SpellInfo.SpellType.DestroyLetter:
                    SpellDestroyLetter(b);
                    break;
                case SpellInfo.SpellType.DestroyGroup:
                    SpellDestroyLetterGroupSmall(b);
                    break;
                case SpellInfo.SpellType.RandomVowels:
                    CreateRandomVowels(5);
                    break;
                case SpellInfo.SpellType.ChangeToVowel:
                    b.Content = EngLetterScoring.RandomVowel();
                    break;
                case SpellInfo.SpellType.Burn:
                    BurnTile(b);
                    break;
                case SpellInfo.SpellType.LetterSwap:
                    if(LetterSwapStep == 0)
                    {
                        LetterSwapFirst = b;
                        LetterSwapStep = 1;
                        return si;
                    }
                    else
                    {
                        SwapLetters(b, LetterSwapFirst);
                        LetterSwapFirst = null;
                        LetterSwapStep = 0;
                    }
                    break;
                case SpellInfo.SpellType.ConvertLetter:
                    WordWarLogic.ConvertLetterTile(b);
                    break;
            }

            return null;
        }

        private static void ConvertLetterTile(Button b)
        {
            string LetterToConvert = b.Content as string;

            for (int i = gridsize - 1; i >= 0; i--)
            {
                for (int j = gridsize - 1; j >= 0; j--)
                {
                    
                    if((LetterButtons[i, j].b.Content as string) == LetterToConvert)
                    {
                        LetterButtons[i, j].b.Content = EngLetterScoring.GetRandomLetter(false);
                    }
                }
            }
        }

        private async static void SwapLetters(Button ba, Button bb)
        {
            LetterProp lpa = ba.DataContext as LetterProp;
            LetterProp lpb = bb.DataContext as LetterProp;

            if (Math.Abs(lpa.i - lpb.i) - Math.Abs(lpa.j - lpb.j) > 1)
            {
                Announcment a = new Announcment("Swapping only works with adjcent letters, up/down/left/right.");
                await a.ShowAsync();
            }
            else
            {

                int ti = lpa.i;
                lpa.i = lpb.i;
                lpb.i = ti;

                int tj = lpa.j;
                lpa.j = lpb.j;
                lpb.j = tj;

                Grid.SetRow(LetterButtons[lpa.i, lpa.j].b, lpb.j);
                Grid.SetColumn(LetterButtons[lpa.i, lpa.j].b, lpb.i);

                Grid.SetRow(LetterButtons[lpb.i, lpb.j].b, lpa.j);
                Grid.SetColumn(LetterButtons[lpb.i, lpb.j].b, lpa.i);

                LetterButtons[lpa.i, lpa.j] = lpa;
                LetterButtons[lpb.i, lpb.j] = lpb;
            }
        }

        private static void BurnTile(Button b)
        {
            LetterProp lp = b.DataContext as LetterProp;
            lp.ChangeTileTo(b, LetterProp.TileType.Burning);
        }

        private static void CreateRandomVowels(int v)
        {
            int n = v;
            int x = 20;  // Try at most to change 20 letters

            while (n > 0 && x > 0)
            {
                int i = r.Next(gridsize);
                int j = r.Next(gridsize);

                if (EngLetterScoring.IsConsonant((string)LetterButtons[i, j].b.Content))
                {
                    LetterButtons[i, j].b.Content = EngLetterScoring.RandomVowel();
                    n--;
                }
                x--;
            }
        }

        private static void SpellDestroyLetterGroupSmall(Button b)
        {
            LetterProp lp = b.DataContext as LetterProp;

            if (lp.j - 1 >= 0)
            {
                RemoveAndReplaceTile(lp.i, lp.j - 1);
            }
            RemoveAndReplaceTile(lp.i, lp.j);

            if (lp.j + 1 < gridsize)
            {
                RemoveAndReplaceTile(lp.i, lp.j + 1);
            }

            RemoveAndReplaceTile(lp.i, lp.j);

            if (lp.i - 1 >= 0)
            {
                RemoveAndReplaceTile(lp.i - 1, lp.j);
            }

            if (lp.i + 1 < gridsize)
            {
                RemoveAndReplaceTile(lp.i + 1, lp.j);
            }
        }

        private static void SpellDestroyLetter(Button b)
        {
            LetterProp lp = b.DataContext as LetterProp;
            RemoveAndReplaceTile(lp.i, lp.j);
        }

        internal static void ReadySpell(SpellInfo selectedSpell)
        {
            if (selectedSpell.Immediate)
            {
                CastSpell(selectedSpell, null);
                Manna -= selectedSpell.MannaPoints;
            }
            {
                NextSpell = selectedSpell;
            }
        }

        internal static string GetCurrentWord()
        {
            return EngLetterScoring.GetCurrentWord(ButtonList);
        }
    }
}
