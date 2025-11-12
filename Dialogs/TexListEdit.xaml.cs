using System.Collections.ObjectModel;
using System.Windows;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for TexListEdit.xaml
    /// </summary>
    public partial class TexListEdit : Window
    {
        ObservableCollection<DataSetTex> temp;
        public TexListEdit(ObservableCollection<DataSetTex> textureList)
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
            DataSetTex tex = new();
            tex.chg = "New Texture Name(新)";
            tex.org = "Original Texture Name(元)";
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
