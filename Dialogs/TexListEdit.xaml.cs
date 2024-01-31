using System.Collections.ObjectModel;
using System.Windows;

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
