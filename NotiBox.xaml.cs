using System.Windows;

namespace Armoire
{
    /// <summary>
    /// Interaction logic for NotiBox.xaml
    /// </summary>
    public partial class NotiBox : Window
    {
        public NotiBox(string textValue, string title)
        {
            InitializeComponent();
            textBox.Text = textValue;
            titleBox.Text = title;
            okButton.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
