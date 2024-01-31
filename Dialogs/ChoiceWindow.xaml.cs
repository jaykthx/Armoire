using System.Windows;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for ChoiceWindow.xaml
    /// </summary>
    public partial class ChoiceWindow : Window
    {
        public bool isRightClicked;
        public ChoiceWindow(string MainText, string LeftText, string RightText)
        {
            InitializeComponent();
            RightButton.Content = RightText;
            LeftButton.Content = LeftText;
            PromptInfo.Text = MainText;
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            isRightClicked = true;
            this.Close();
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            isRightClicked = false;
            this.Close();
        }
    }
}
