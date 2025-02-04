using MikuMikuLibrary.Databases;
using System.Windows;
using System.Windows.Input;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for SprEditSub.xaml
    /// </summary>
    public partial class SprEditSub : Window
    {
        public SpriteSetInfo spriteSet = null;
        public bool spriteMode = false;
        public SprEditSub(SpriteSetInfo spriteSetInfo, bool isSprite)
        {
            InitializeComponent();
            this.DataContext = spriteSetInfo;
            spriteSet = spriteSetInfo;
            if (isSprite)
            {
                spriteMode = true;
                Grid123.ItemsSource = spriteSet.Sprites;
                AddMenu.Header = "Add Sprite";
                DelMenu.Header = "Delete Sprite";
            }
            else
            {
                Grid123.ItemsSource = spriteSet.Textures;
                AddMenu.Header = "Add Texture";
                DelMenu.Header = "Delete Texture";
            }
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
            if (spriteMode)
            {
                SpriteInfo sprite = new();
                sprite.Index = 0;
                sprite.Name = "NEW SPR ENTRY";
                sprite.Id = 0;
                spriteSet.Sprites.Add(sprite);
                //Grid123.ItemsSource = spriteSet.Sprites;
                //Grid123.Items.Refresh();
            }
            else
            {
                SpriteTextureInfo texture = new();
                texture.Index = 0;
                texture.Name = "NEW TEX ENTRY";
                texture.Id = 0;
                spriteSet.Textures.Add(texture);
                //Grid123.ItemsSource = spriteSet.Textures;
                //Grid123.Items.Refresh();
            }
            Grid123.Items.Refresh();
        }
        private void MenuItem2_Click(object sender, RoutedEventArgs e) //Delete
        {
            if (spriteMode && spriteSet.Sprites.Count > 0)
            {
                foreach (var x in Grid123.SelectedItems)
                {
                    if (Grid123.Items.Count > 0)
                    {
                        spriteSet.Sprites.RemoveAt(Grid123.Items.IndexOf(x));
                    }
                }
                Grid123.ItemsSource = spriteSet.Sprites;
            }
            else if (!spriteMode && spriteSet.Textures.Count > 1)
            {
                foreach (var x2 in Grid123.SelectedItems)
                {
                    if (Grid123.Items.Count > 0)
                    {
                        spriteSet.Textures.RemoveAt(Grid123.Items.IndexOf(x2));
                    }
                    else
                    {
                        Program.NotiBox(Properties.Resources.warn_delete, Properties.Resources.window_notice);
                    }
                }
                Grid123.ItemsSource = spriteSet.Textures;
            }
            else
            {
                return;
            }
            Grid123.Items.Refresh();
        }
    }
}
