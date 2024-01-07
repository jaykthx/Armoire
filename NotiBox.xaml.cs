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
