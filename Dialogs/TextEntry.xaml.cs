using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for TextEntry.xaml
    /// </summary>
    public partial class TextEntry : Window
    {
        bool isNumeric = false;
        public TextEntry(bool isNumericOnly, string topTitle)
        {
            InitializeComponent();
            if (!isNumericOnly)
            {
                texBox.Visibility = Visibility.Visible;
                texBox.SelectAll();
                texBox.Focus();
            }
            else
            {
                isNumeric= true;
                numBox.Visibility = Visibility.Visible;
                numBox.SelectAll();
                numBox.Focus();
            }
            TopTitle.Text = topTitle;
        }

        public string Result
        {
            get { if (isNumeric) { return numBox.Text; }
                else { return texBox.Text; }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CheckNumbers(object sender, TextCompositionEventArgs e) // Taken from stackoverflow
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
