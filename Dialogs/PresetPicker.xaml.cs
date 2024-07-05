using System.Collections.Generic;
using System.Windows;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for PresetPicker.xaml
    /// </summary>
    public partial class PresetPicker : Window
    {
        public List<string> presets = new List<string> { "Eye Texture Swap", "Contact Lenses", "Hair", "Body", "Hands", "Head Accessory", "Face Accessory", "Chest Accessory", "Back Accessory" };
        public List<string> presets_item = new List<string> { "Head Accessory", "Face Accessory", "Chest Accessory", "Back Accessory" };
        public itemEntry itemCurrent;
        string setChara;
        string currentFarcPath;
        public PresetPicker(itemEntry item, string chara, bool isModule)
        {
            InitializeComponent();
            setChara = chara;
            Program.CreatePresetList();
            if (isModule)
            {
                presetBox.ItemsSource = presets;
            }
            else
            {
                presetBox.ItemsSource = presets_item;
            }
            itemCurrent = item;
            holdName.Text = item.name;
            CheckIsPreset(item);
            presetBox.SelectedIndex = 0;
            presetBox.Focus();
        }

        public PresetPicker(itemEntry item, string farcPath, string chara, bool isModule)
        {
            InitializeComponent();
            setChara = chara;
            Program.CreatePresetList();
            if (isModule)
            {
                presetBox.ItemsSource = presets;
            }
            else
            {
                presetBox.ItemsSource = presets_item;
            }
            itemCurrent = item;
            holdName.Text = item.name;
            CheckIsPreset(item);
            presetBox.Focus();
            presetBox.SelectedIndex = 0;
            currentFarcPath = farcPath;
        }

        private itemEntry applyPreset(Program.ItemPreset preset, itemEntry item)
        {
            if (preset.subid == 6 || preset.subid == 24)
            {
                if (currentFarcPath != null)
                {
                    TexturePicker tp = new TexturePicker(item, currentFarcPath, setChara);
                    tp.ShowDialog();
                    itemCurrent = tp.temp_item;
                }
                else
                {
                    TexturePicker tp = new TexturePicker(item, setChara);
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
