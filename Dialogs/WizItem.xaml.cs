using System.Windows;
using System.Windows.Controls;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for WizItem.xaml
    /// </summary>
    public partial class WizItem : UserControl
    {
        public WizardObject curObj = new();
        public WizItem(bool isExisting)
        {
            InitializeComponent();
            if (isExisting)
            {
                itemTypeButton.Visibility = Visibility.Hidden;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(curObj.item == null)
            {
                curObj.item = new CharacterItemEntry();
            }
            curObj.item.name = Properties.Resources.cmn_temp;
            PresetPicker picker = new(curObj, (((WizardInfo)((Grid)((StackPanel)this.Parent).Parent).Parent).CharacterBox.SelectedValue as string).ToUpper(), true);
            picker.ShowDialog();
            curObj.item = picker.itemCurrent;
        }
    }
}
