using MikuMikuLibrary.Databases;
using System.Windows;
using System.Windows.Input;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for ObjEditSub.xaml
    /// </summary>
    public partial class ObjEditSub : Window
    {
        ObjectSetInfo thisObj;
        public ObjEditSub(ObjectSetInfo obj)
        {
            InitializeComponent();
            thisObj = obj;
            Grid123.ItemsSource = thisObj.Objects;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) //Add
        {
            ObjectInfo objectInfo = new();
            objectInfo.Name = "NEW OBJ ENTRY";
            objectInfo.Id = 0;
            thisObj.Objects.Add(objectInfo);
            //Grid123.ItemsSource = spriteSet.Sprites;
            Grid123.Items.Refresh();
        }
        private void MenuItem2_Click(object sender, RoutedEventArgs e) //Delete
        {
            if (thisObj.Objects.Count > 0)
            {
                foreach (var x in Grid123.SelectedItems)
                {
                    if (Grid123.Items.Count > 0)
                    {
                        thisObj.Objects.RemoveAt(Grid123.Items.IndexOf(x));
                    }
                }
                Grid123.ItemsSource = thisObj.Objects;
            }
            else
            {
                return;
            }
            Grid123.Items.Refresh();
        }
    }
}
