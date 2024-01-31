using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

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
