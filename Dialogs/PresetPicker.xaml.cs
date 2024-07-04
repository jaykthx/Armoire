using System.Collections.Generic;
using System.Windows;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for PresetPicker.xaml
    /// </summary>
    public partial class PresetPicker : Window
    {
        public List<string> presets = new List<string> { "Eye Texture Swap", "Contact Lenses", "Hair", "Body", "Hands", "Head Accessory", "Face Accessory", "Chest Accessory", "Back Accessory"};
        public itemEntry itemCurrent;
        string currentFarcPath;
        public PresetPicker(itemEntry item)
        {
            InitializeComponent();
            Program.CreatePresetList();
            presetBox.ItemsSource = presets;
            presetBox.SelectedIndex = 0;
            itemCurrent = item;
            holdName.Text = item.name;
            CheckIsPreset(item);
            presetBox.Focus();
        }

        public PresetPicker(itemEntry item, string farcPath)
        {
            InitializeComponent();
            Program.CreatePresetList();
            presetBox.ItemsSource = presets;
            presetBox.SelectedIndex = 0;
            itemCurrent = item;
            holdName.Text = item.name;
            CheckIsPreset(item);
            presetBox.Focus();
            currentFarcPath = farcPath;
        }

        private itemEntry applyPreset(Program.ItemPreset preset, itemEntry item)
        {
            if (preset.subid == 6 || preset.subid == 24)
            {
                Program.NotiBox("You have selected a preset related to texture swapping." +
                    "\nYou will need to add the texture swaps within the Item Edit window of the Character Item Editor, as the program cannot do this automatically." +
                    "\nIf you do not do this, the texture swaps will not be applied and the item will not appear in-game.", Properties.Resources.window_notice);
                if (currentFarcPath != null)
                {
                    TexturePicker tp = new TexturePicker(item, currentFarcPath);
                    tp.ShowDialog();
                    itemCurrent = tp.temp_item;
                }
                else
                {
                    TexturePicker tp = new TexturePicker(item);
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
            int count = 0;
            foreach(var preset in Program.itemPresets)
            {
                if(item.subID == preset.subid)
                {
                    presetBox.SelectedIndex = count;
                }
                count++;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(presetBox.SelectedIndex > -1)
            {
                applyPreset(Program.itemPresets[presetBox.SelectedIndex], itemCurrent);
            }
            this.Close();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
