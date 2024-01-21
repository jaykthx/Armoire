using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for PresetPicker.xaml
    /// </summary>
    public partial class PresetPicker : Window
    {
        public List<string> presets = new List<string> { "Eye Texture Swap", "Contact Lenses", "Hair", "Body", "Hands", "Head Accessory", "Face Accessory", "Chest Accessory", "Back Accessory"};
        public itemEntry itemCurrent;
        public PresetPicker(itemEntry item)
        {
            InitializeComponent();
            Program.CreatePresetList();
            presetBox.ItemsSource = presets;
            presetBox.SelectedIndex = 0;
            itemCurrent = item;
            holdName.Text = item.name;
            presetBox.Focus();
        }

        private itemEntry applyPreset(Program.ItemPreset preset, itemEntry item)
        {
            if(preset.subid == 6 || preset.subid == 24)
            {
                Program.NotiBox("You have selected a preset related to texture swapping." +
                    "\nYou will need to add the texture swaps within the Item Edit window of the Character Item Editor, as the program cannot do this automatically." +
                    "\nIf you do not do this, the texture swaps will not be applied and the item will not appear in-game.", "Notice");
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

        // attr,subid,desid,rpk,type,orgitm,flag,face_depth

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(presetBox.SelectedIndex > -1)
            {
                applyPreset(Program.itemPresets[presetBox.SelectedIndex], itemCurrent);
            }
            this.Close();
        }
    }
}
