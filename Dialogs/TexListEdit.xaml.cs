using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for TexListEdit.xaml
    /// </summary>
    public partial class TexListEdit : Window
    {
        ObservableCollection<dataSetTex> temp;
        public TexListEdit(ObservableCollection<dataSetTex> textureList)
        {
            InitializeComponent();
            this.DataContext = null;
            texDataGrid.Items.Clear();
            this.DataContext= textureList;
            temp = textureList;
            texDataGrid.ItemsSource= textureList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            dataSetTex tex = new dataSetTex();
            tex.chg = "New Texture Name";
            tex.org = "Original Texture Name";
            temp.Add(tex);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if(texDataGrid.SelectedIndex != -1)
            {
                temp.RemoveAt(texDataGrid.SelectedIndex);
            }
        }
    }
}
