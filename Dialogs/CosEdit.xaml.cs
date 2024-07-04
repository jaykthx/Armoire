using System.Windows;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for CharaItem.xaml
    /// </summary>
    public partial class CosEdit : Window
    {
        cosEntry cosCxt;
        public CosEdit(cosEntry cos)
        {
            InitializeComponent();
            this.DataContext = cos;
            cosCxt = cos;
            idBox.Text = "ID: " + cos.id;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            TextEntry win = new TextEntry(true, "Enter item number");
            if (win.ShowDialog() == true)
            {
                if (win.Result != "ENTER VALUE HERE")
                {
                    cosCxt.items.Add(int.Parse(win.Result));
                    itemList.Items.Refresh();
                }
                else { Program.NotiBox("Enter a value.", Properties.Resources.cmn_error); }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (itemList.SelectedIndex != -1 && cosCxt.items.Count > 1)
            {
                cosCxt.items.RemoveAt(itemList.SelectedIndex);
            }
            else if(cosCxt.items.Count == 1)
            {
                Program.NotiBox("Costumes must not be empty.", Properties.Resources.cmn_error);
            }
            else
            {
                Program.NotiBox("You must select an item first.", Properties.Resources.cmn_error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new TextEntry(true, Properties.Resources.cmn_enter_value);
            tex.ShowDialog();
            cosCxt.id = int.Parse(tex.Result);
            idBox.Text = "ID: " + cosCxt.id;
        }
    }
}
