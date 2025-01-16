using Armoire.Dialogs;
using MikuMikuLibrary.Sprites;
using MikuMikuLibrary.Textures;
using MikuMikuLibrary.Textures.Processing;
using MikuMikuLibrary.Textures.Processing.Interfaces;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Armoire
{
    /// <summary>
    /// Interaction logic for ModuleInfo.xaml
    /// </summary>
    public partial class ModuleInfo : System.Windows.Controls.UserControl
    {
        public wizModule wizMod = new();
        public ModuleInfo()
        {
            InitializeComponent();
            charBox.ItemsSource = Program.charas;
            charBox.SelectedIndex = 0;
            setModuleImage(Properties.Resources.md_dummy);
        }

        private void Button_Click(object sender, RoutedEventArgs e) //add
        {
            WizItem itm = new();
            itemPanel.Children.Add(itm);
            itm.parentModInfo = this;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                itemPanel.Children.RemoveAt(itemPanel.Children.Count - 1);
            }
            catch
            {
            }
        }

        private void nameButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(false, "Enter Module Name");
            tex.ShowDialog();
            if (tex.Result != Properties.Resources.cmn_enter_value && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result))
            {
                nameText.Text = Properties.Resources.cmn_name + ": " + tex.Result;
                wizMod.name = tex.Result;
            }
            else { return; }
        }

        private void idButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(true, "Enter Module ID Value");
            tex.ShowDialog();
            if (tex.Result != Properties.Resources.cmn_enter_value && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result) && !tex.Result.Contains(' '))
            {
                wizMod.id = int.Parse(tex.Result);
                idText.Text = Properties.Resources.cmn_id + ": " + wizMod.id;
            }
            else { return; }
        }

        private void indexButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(true, "Enter Module Sorting Index Value");
            tex.ShowDialog();
            if (tex.Result != Properties.Resources.cmn_enter_value && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result) && !tex.Result.Contains(' '))
            {
                wizMod.sort_index = int.Parse(tex.Result);
                indexText.Text = Properties.Resources.cmn_index + ": " + tex.Result;
            }
            else { return; }
        }


        private void charBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(charBox.SelectedIndex > -1)
            {
                wizMod.chara = Program.charas[charBox.SelectedIndex];
            }
        }

        public void finalizeModule() //add all objects so the program doesnt shit itself
        {
            wizMod.objects.Clear();
            wizMod.hairNG = (bool)hairCheck.IsChecked;
            foreach(WizItem x in itemPanel.Children)
            {
                if(x.curObj.item != null && x.curObj.objectFilePath != null)
                wizMod.objects.Add(x.curObj);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new();
            ofd.Filter = "PNG Files|*.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap pngImage = new(ofd.FileName);
                if (pngImage.Width == 512 && pngImage.Height == 512)
                {
                    setModuleImage(pngImage);
                    wizMod.bitmap = pngImage;
                }
                else
                {
                    Program.NotiBox(Properties.Resources.exp_3, Properties.Resources.cmn_error);
                }
            }
        }
        private void setModuleImage(Bitmap pngFile)
        {
            Sprite spr = Program.GetSprite(false);
            SpriteSet sprite = new();
            sprite.Sprites.Add(spr);
            Bitmap newBitmap = new(pngFile);
            newBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            Texture text = TextureEncoder.EncodeFromBitmap(newBitmap, MikuMikuLibrary.Textures.TextureFormat.DXT5, true);
            //MikuMikuLibrary.Textures.Texture text = MikuMikuLibrary.Native.TextureEncoder.EncodeFromBitmap(newBitmap, MikuMikuLibrary.Textures.TextureFormat.DXT5, true);
            sprite.TextureSet.Textures.Add(text);
            Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
            BitmapImage img = Program.ToBitmapImage(cropSprite);
            moduleImage.Source = img;
        }
    }
}
