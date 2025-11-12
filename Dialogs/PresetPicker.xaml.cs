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
        public WizardObject curWizObj;
        public string charaSel;
        public CharacterItemEntry itemCurrent;
        public PresetPicker(WizardObject wizObj, string chara, bool isModule)
        {
            InitializeComponent();
            curWizObj = wizObj;
            charaSel = chara;
            itemCurrent = wizObj.item;
            holdName.Text = wizObj.item.name;
            if (!isModule)
            {
                eyeButton.Visibility = Visibility.Collapsed;
                hairButton.Visibility = Visibility.Collapsed;
                bodyButton.Visibility = Visibility.Collapsed;
                handButton.Visibility = Visibility.Collapsed;
                contactButton.Visibility = Visibility.Collapsed;
            }
            CheckIsPreset();
        }

        private CharacterItemEntry applyPreset(Program.ItemPreset preset)
        {
            if (preset.subid == 24)
            {
                    TexturePicker tp = new(curWizObj, charaSel);
                    tp.ShowDialog();
                    itemCurrent = tp.temp_item;
            }
            curWizObj.item.attr = preset.attr;
            curWizObj.item.desID = preset.desid;
            curWizObj.item.subID = preset.subid;
            curWizObj.item.flag = preset.flag;
            curWizObj.item.rpk = preset.rpk;
            curWizObj.item.orgItm = preset.orgitm;
            curWizObj.item.face_depth = preset.face_depth;
            curWizObj.item.type = preset.type;
            return curWizObj.item;
        }

        private void CheckIsPreset()
        {
            SolidColorBrush sel = new(Color.FromRgb(80, 200, 100));
            switch (curWizObj.item.subID)
            {
                case 24:
                    eyeButton.Background = sel;
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
            applyPreset(Program.itemPresets[0]);
            this.Close();
        }
        private void contact_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[1]);
            this.Close();
        }
        private void hair_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[2]);
            this.Close();
        }
        private void body_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[3]);
            this.Close();
        }
        private void hand_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[4]);
            this.Close();
        }
        private void headAcc_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[5]);
            this.Close();
        }
        private void faceAcc_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[6]);
            this.Close();
        }
        private void chestAcc_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[7]);
            this.Close();
        }
        private void backAcc_Click(object sender, RoutedEventArgs e)
        {
            applyPreset(Program.itemPresets[8]);
            this.Close();
        }
    }
}
