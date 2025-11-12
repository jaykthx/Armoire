using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Armoire
{
    /// <summary>
    /// Interaction logic for PopupNotification.xaml
    /// </summary>
    public partial class PopupNotification : Window
    {
        public PopupNotification(string notificationText)
        {
            InitializeComponent();
            notifText.Text = notificationText;
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = SystemParameters.MaximizedPrimaryScreenHeight - this.Height;
            IEnumerator enumer = App.Current.Windows.GetEnumerator();
            List<Window> list = new();
            foreach(Window x in App.Current.Windows)
            {
                if (x.GetType() == typeof(PopupNotification) && x != this)
                {
                    list.Add(x);
                }
            }
            if(list.Count > 0)
            {
                if ((list.Last().Top - this.Height) < this.Height)
                {
                    this.Left = list.Last().Left - this.Width;
                    this.Top = SystemParameters.MaximizedPrimaryScreenHeight - this.Height;
                }
                else
                {
                    this.Left = list.Last().Left;
                    this.Top = list.Last().Top - this.Height;
                }
            }
            this.Show();
            if (Application.Current.Windows.OfType<MainWindow>().Any())
            {
                Application.Current.Windows.OfType<MainWindow>().First().Focus();
            }
        }

        private void DoubleAnimationUsingKeyFrames_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
