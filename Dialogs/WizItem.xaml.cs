using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for WizItem.xaml
    /// </summary>
    public partial class WizItem : UserControl
    {
        public wizObj curObj = new wizObj();
        public WizItem()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FARC files|*.farc";
            ofd.Title = "Select your model's farc file.";
            if(ofd.ShowDialog() == true)
            {
                curObj.objectFilePath = ofd.FileName;
                fileName.Text = System.IO.Path.GetFileName(ofd.FileName);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(curObj.item == null)
            {
                curObj.item = new itemEntry();
            }
            curObj.item.name = "TEMP ITEM";
            PresetPicker picker = new PresetPicker(curObj.item);
            picker.ShowDialog();
            curObj.item = picker.itemCurrent;
        }
    }
}
