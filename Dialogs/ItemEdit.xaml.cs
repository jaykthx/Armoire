using System.Windows;
using System.Windows.Controls;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for CharaCos.xaml
    /// </summary>
    public partial class ItemEdit : Window
    {
        CharacterItemEntry itemCxt = null;
        public ItemEdit(CharacterItemEntry item)
        {
            InitializeComponent();
            this.DataContext = item;
            ObjsetBox.ItemsSource = item.objset;
            ObjsetBox.SelectedIndex= 0;
            itemCxt = item;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if(ObjsetBox.Items.Count < 2)
            {
                TextEntry win = new(false, Properties.Resources.warn_enter_value);
                if(win.ShowDialog() == true)
                {
                    if (win.Result != "ENTER VALUE HERE")
                    {
                        itemCxt.objset.Add(win.Result);
                        ObjsetBox.Items.Refresh();
                    }
                    else { Program.NotiBox(Properties.Resources.warn_enter_value, Properties.Resources.cmn_error); }
                }
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_object_limit, Properties.Resources.window_notice);
            }
        }
        private void Del_Click(object sender, RoutedEventArgs e)
        {
            if (ObjsetBox.Items.Count > 1 && ObjsetBox.SelectedIndex != -1)
            {
                itemCxt.objset.RemoveAt(ObjsetBox.SelectedIndex);
                ObjsetBox.Items.Refresh();
            }
            else if (ObjsetBox.SelectedIndex == -1)
            {
                return;
            }
            else
            {
                itemCxt.objset[ObjsetBox.SelectedIndex] = "NULL";
                ObjsetBox.Items.Refresh();
            }
        }
        private void Tex_Click(object sender, RoutedEventArgs e)
        {
            if(itemCxt.dataSetTex.Count == 0)
            {
                itemCxt.dataSetTex = new System.Collections.ObjectModel.ObservableCollection<DataSetTex>();
            }
            TexListEdit texEdit = new(itemCxt.dataSetTex);
            texEdit.ShowDialog();
        }
        private void textChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            var box = e.Source as TextBox;
            box.CaretIndex = box.Text.Length;
        }
    }
}
