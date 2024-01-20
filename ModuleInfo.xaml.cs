using Armoire.Dialogs;
using Microsoft.Win32;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Sprites;
using MikuMikuLibrary.Textures.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Armoire
{
    /// <summary>
    /// Interaction logic for ModuleInfo.xaml
    /// </summary>
    public partial class ModuleInfo : System.Windows.Controls.UserControl
    {
        public wizModule wizMod = new wizModule();
        public ModuleInfo()
        {
            InitializeComponent();
            charBox.ItemsSource = Program.charas;
            charBox.SelectedIndex = 0;
            moduleImage.Source = new BitmapImage(new Uri(wizMod.image_path));
        }

        private void Button_Click(object sender, RoutedEventArgs e) //add
        {
            WizItem itm = new WizItem();
            itemPanel.Children.Add(itm);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(itemPanel.Children.Count > 0)
            {
                itemPanel.Children.RemoveAt(itemPanel.Children.Count-1);
            }
        }

        private void nameButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new TextEntry(false, "Enter Module Name");
            tex.ShowDialog();
            if (tex.Result != "ENTER VALUE HERE" && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result))
            {
                nameText.Text = "Name: " + tex.Result;
                wizMod.name = tex.Result;
            }
            else { return; }
        }

        private void idButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new TextEntry(true, "Enter Module ID Value");
            tex.ShowDialog();
            if (tex.Result != "ENTER VALUE HERE" && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result) && !tex.Result.Contains(' '))
            {
                wizMod.id = int.Parse(tex.Result);
                idText.Text = "ID: " + wizMod.id;
            }
            else { return; }
        }

        private void indexButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new TextEntry(true, "Enter Module Sorting Index Value");
            tex.ShowDialog();
            if (tex.Result != "ENTER VALUE HERE" && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result) && !tex.Result.Contains(' '))
            {
                wizMod.sort_index = int.Parse(tex.Result);
                indexText.Text = "Sorting Index: " + tex.Result;
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
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "PNG Files|*.png";
            
                if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap pngImage = new Bitmap(ofd.FileName);
                if (pngImage.Width == 512 && pngImage.Height == 512)
                {
                    setModuleImage(ofd.FileName);
                    wizMod.image_path = ofd.FileName;
                }
                else
                {
                    Program.NotiBox("Your image must be a 512px x 512px PNG format file.", "Error");
                }
            }
        }
        private void setModuleImage(string pngFile)
        {
            Sprite spr = Program.GetSprite(false);
            SpriteSet sprite = new SpriteSet();
            sprite.Sprites.Add(spr);
            Bitmap pngImage = new Bitmap(pngFile);
            pngImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            MikuMikuLibrary.Textures.Processing.TextureEncoderCore tex = new TextureEncoderCore();
            MikuMikuLibrary.Textures.Texture text = tex.EncodeFromBitmap(pngImage, MikuMikuLibrary.Textures.TextureFormat.DXT5, true);
            sprite.TextureSet.Textures.Add(text);
            Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
            BitmapImage img = Program.ToBitmapImage(cropSprite);
            moduleImage.Source = img;
        }
    }
}
