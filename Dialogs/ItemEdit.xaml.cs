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
    /// Interaction logic for CharaCos.xaml
    /// </summary>
    public partial class ItemEdit : Window
    {
        itemEntry itemCxt = null;
        public ItemEdit(itemEntry item)
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
                TextEntry win = new TextEntry(false, "Enter object name");
                if(win.ShowDialog() == true)
                {
                    if (win.Result != "ENTER VALUE HERE")
                    {
                        itemCxt.objset.Add(win.Result);
                        ObjsetBox.Items.Refresh();
                    }
                    else { Program.NotiBox("Enter a value.", "Error"); }
                }
            }
            else
            {
                Program.NotiBox("Only 2 objects may be used.", "Common Sense Error");
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
            if(itemCxt.dataSetTexes.Count == 0)
            {
                itemCxt.dataSetTexes = new System.Collections.ObjectModel.ObservableCollection<dataSetTex>();
            }
            TexListEdit texEdit = new TexListEdit(itemCxt.dataSetTexes);
            texEdit.ShowDialog();
        }
    }
}
