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
            nameHold.Text = "Costume: " + cos.id;
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
                else { Program.NotiBox("Enter a value.", "Error"); }
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
                Program.NotiBox("Costumes must not be empty.", "Error");
            }
            else
            {
                Program.NotiBox("You must select an item first.", "Error");
            }
        }
    }
}
