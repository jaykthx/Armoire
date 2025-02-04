using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for PresetPicker.xaml
    /// </summary>
    public partial class PresetPicker : Window
    {
        public itemEntry itemCurrent;
        readonly string setChara;
        readonly string currentFarcPath;
        readonly bool wizCheck;
        public PresetPicker(itemEntry item, string chara, bool isModule, bool isWizard)
        {
            InitializeComponent();
            wizCheck = isWizard;
            setChara = chara;
            itemCurrent = item;
            holdName.Text = item.name;
            if (!isModule)
            {
                eyeButton.Visibility = Visibility.Collapsed;
                hairButton.Visibility = Visibility.Collapsed;
                bodyButton.Visibility = Visibility.Collapsed;
                handButton.Visibility = Visibility.Collapsed;
                contactButton.Visibility = Visibility.Collapsed;
                headButton.Visibility = Visibility.Collapsed;
            }
            CheckIsPreset(item);
        }

        public PresetPicker(itemEntry item, string farcPath, string chara, bool isModule, bool isWizard)
        {
            InitializeComponent();
            wizCheck = isWizard;
            setChara = chara;
            itemCurrent = item;
            holdName.Text = item.name;
            if (!isModule)
            {
                eyeButton.Visibility = Visibility.Collapsed;
                hairButton.Visibility = Visibility.Collapsed;
                bodyButton.Visibility = Visibility.Collapsed;
                handButton.Visibility = Visibility.Collapsed;
                contactButton.Visibility = Visibility.Collapsed;
                headButton.Visibility = Visibility.Collapsed;
            }
            CheckIsPreset(item);
            currentFarcPath = farcPath;
        }

        private itemEntry applyPreset(Program.ItemPreset preset, itemEntry item)
        {
            if ((preset.subid == 6 || (preset.subid == 24 & preset.attr != 2085)) && wizCheck)
            {
                if (currentFarcPath != null)
                {
                    TexturePicker tp = new(item, currentFarcPath, setChara);
                    tp.ShowDialog();
                    itemCurrent = tp.temp_item;
                }
                else
                {
                    TexturePicker tp = new(item, setChara);
                    tp.ShowDialog();
                    itemCurrent = tp.temp_item;
                }
            }
            item.attr = preset.attr;
            item.desID = preset.desid;
            item.subID = preset.subid;
            item.flag = preset.flag;
            item.rpk = preset.rpk;
            item.orgItm = preset.orgitm;
            item.face_depth = preset.face_depth;
            item.type = preset.type;
            return item;
        }

        private void CheckIsPreset(itemEntry item)
        {
            SolidColorBrush sel = new(Color.FromRgb(80, 200, 100));
            switch (item.subID)
            {
                case 24:
                    if (item.attr == 2085)
                    {
                        headButton.Background = sel;
                    }
                    else
                    {
                        eyeButton.Background = sel;
                    }
                    break;
                case 6:
                    contactButton.Background = sel;
                    break;
                case 1:
                    hairButton.Background = sel;
                    break;
                case 10:
                    bodyButton.Background = sel;
                    break;
                case 14:
                    handButton.Background = sel;
                    break;
                case 0:
                    headAccButton.Background = sel;
                    break;
                case 4:
                    faceAccButton.Background = sel;
                    break;
                case 8:
                    chestAccButton.Background = sel;
                    break;
                case 16:
                    backAccButton.Background = sel;
                    break;
                default:
                    break;
            }
        }

        private void eye_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[0], itemCurrent);
            this.Close();
        }
        private void contact_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[1], itemCurrent);
            this.Close();
        }
        private void hair_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[2], itemCurrent);
            this.Close();
        }
        private void body_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[3], itemCurrent);
            this.Close();
        }
        private void hand_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[4], itemCurrent);
            this.Close();
        }
        private void headAcc_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[5], itemCurrent);
            this.Close();
        }
        private void faceAcc_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[6], itemCurrent);
            this.Close();
        }
        private void chestAcc_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[7], itemCurrent);
            this.Close();
        }
        private void backAcc_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[8], itemCurrent);
            this.Close();
        }
        private void head_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[9], itemCurrent);
            this.Close();
        }
    }
}
