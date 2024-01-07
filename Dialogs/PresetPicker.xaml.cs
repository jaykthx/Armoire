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
        public List<string> presets = new List<string> { "Eye Texture Swap", "Contact Lenses", "Hair", "Body", "Hands", "Head Accessory", "Face Accessory", "Chest Accessory", "Back Accessory", "Custom" };
        public PresetPicker(itemEntry item)
        {
            InitializeComponent();
            this.DataContext= item;
            holdName.Text = item.name;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /*
         * attr,subid,desid,rpk,type,orgitm,flag,face_depth
         * Head Acc (1,0,0,-1,0,0,0,0.00)
         * Face Acc (1,4,1,-1,0,0,0,0.00)
         * Chest Acc (1,8,2,-1,0,0,0,0.00)
         * Back Acc (1,16,2,-1,0,0,0,0.00)
         * Hair (1,1,0,-1,0,0,0,0.00)
         * Body (1,10,2,-1,1,0,0,0.00)
         * Hands (1,14,2,-1,0,0,0,0.00)
         * Contact Lenses (1,6,1,-1,0,0,0,0.00)
         * Eye Tex (37,24,0,1,2,0,0,0.00) uid = NULL
         */
    }
}
