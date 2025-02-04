using System.Windows;
using System.Windows.Controls;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for WizItem.xaml
    /// </summary>
    public partial class WizItem : UserControl
    {
        public wizObj curObj = new();
        public ModuleInfo parentModInfo;
        public WizItem()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(curObj.item == null)
            {
                curObj.item = new itemEntry();
            }
            curObj.item.name = Armoire.Properties.Resources.cmn_temp;
            PresetPicker picker = new(curObj.item, curObj.objectFilePath, Program.Databases.GetChritmName(parentModInfo.charBox.SelectedValue as string).ToUpper(), true, true);
            picker.ShowDialog();
            curObj.item = picker.itemCurrent;
        }
    }
}
