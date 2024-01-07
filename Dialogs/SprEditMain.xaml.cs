﻿using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for SprEditMain.xaml
    /// </summary>
    public partial class SprEditMain : Window
    {
        public SprEditMain()
        {
            InitializeComponent();
        }
        SpriteDatabase db = new SpriteDatabase();
        string saveLocation = null;
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile();
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save();
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs();
        }

        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Please select a Sprite Database"; //"データベースを選択してください。";
            ofd.Filter = "Sprite Database files|*spr_db.bin"; //"データベースファイル |*_db.bin"; 
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (db.SpriteSets != null)
                {
                    Grid1.ItemsSource = null;
                    Grid1.Items.Clear();
                    db.SpriteSets.Clear();
                }
                saveLocation = ofd.FileName;
                db = BinaryFile.Load<SpriteDatabase>(ofd.FileName);
                //spriteSets = new ObservableCollection<SpriteSetInfo>(db.SpriteSets);
                Grid1.ItemsSource = db.SpriteSets;
            }
            Grid1.DataContext = db.SpriteSets;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // Sprite editor
        {
            SpriteSetInfo spr = ((FrameworkElement)sender).DataContext as SpriteSetInfo;
            SprEditSub win = new SprEditSub(spr, true);
            win.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // Texture editor
        {
            SpriteSetInfo spr = ((FrameworkElement)sender).DataContext as SpriteSetInfo;
            SprEditSub win = new SprEditSub(spr, false);
            win.ShowDialog();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void Save()
        {
            if (saveLocation != null && db.SpriteSets.Count > 0)
            {
                db.Save(saveLocation);
                System.Windows.MessageBox.Show("Saved successfully.");
            }
            else
            {
                System.Windows.MessageBox.Show("Please save your file correctly.");
            }
        }
        private void SaveAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Sprite Database files|*spr_db.bin|All files|*.*";
            sfd.FileName = "mod_spr_db.bin";
            if (db.SpriteSets.Count != 0)
            {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    db.Save(sfd.FileName);
                    System.Windows.MessageBox.Show("Saved successfully.");
                }
                else
                {
                    System.Windows.MessageBox.Show("An error occurred while saving your file" +
                        "\n" + "Please try again.");
                }
            }
            else { System.Windows.MessageBox.Show("Please save your file normally."); }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            SortDescription sort = new SortDescription();
            if (Grid1.Items.SortDescriptions.Count > 0)
            {
                sort = Grid1.Items.SortDescriptions[0];
            }
            foreach (SpriteSetInfo x in Grid1.SelectedItems)
            {
                if (db.SpriteSets.Count > 0)
                {
                    db.SpriteSets.Remove(x);
                }
            }
            Grid1.Items.Refresh();
            Grid1.Items.SortDescriptions.Clear();
            if (sort.PropertyName != null)
            {
                Grid1.Items.SortDescriptions.Add(sort);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            SpriteInfo newSpr = new SpriteInfo();
            newSpr.Id = 0;
            newSpr.Name = "NEW SPR ENTRY";
            newSpr.Index = 0;
            SpriteTextureInfo newTex = new SpriteTextureInfo();
            newTex.Id = 0;
            newTex.Name = "NEW TEX ENTRY";
            newTex.Index = 0;
            SpriteSetInfo newSprInfo = new SpriteSetInfo();
            newSprInfo.FileName = "DUMMY.farc";
            newSprInfo.Name = "NEW SPR ENTRY";
            newSprInfo.Id = 0;
            newSprInfo.Sprites.Add(newSpr);
            newSprInfo.Textures.Add(newTex);
            db.SpriteSets.Add(newSprInfo);
            Grid1.Items.Refresh();
        }
        private void Dupe_Click(object sender, RoutedEventArgs e) //Dupe ONE sprite
        {
            int index = 0;
            List<SpriteSetInfo> spriteColle = new List<SpriteSetInfo>();
            foreach (var x in Grid1.SelectedItems)
            {
                index = Grid1.Items.IndexOf(x);
                spriteColle.Add(spriteDupe(index));
            }
            foreach (SpriteSetInfo spr in spriteColle)
            {
                db.SpriteSets.Insert(index + 1, spr);
                index++;
            }
            Grid1.Items.Refresh();
        }

        private SpriteSetInfo spriteDupe(int index)
        {
            SpriteSetInfo newSprInfo = new SpriteSetInfo();
            newSprInfo.FileName = db.SpriteSets[index].FileName;
            newSprInfo.Name = db.SpriteSets[index].Name;
            newSprInfo.Id = db.SpriteSets[index].Id;
            foreach (SpriteInfo spr in db.SpriteSets[index].Sprites)
            {
                SpriteInfo newSpr = new SpriteInfo();
                newSpr.Id = spr.Id;
                newSpr.Name = spr.Name;
                newSpr.Index = spr.Index;
                newSprInfo.Sprites.Add(newSpr);
            }
            foreach (SpriteTextureInfo tex in db.SpriteSets[index].Textures)
            {
                SpriteTextureInfo newTex = new SpriteTextureInfo();
                newTex.Id = tex.Id;
                newTex.Name = tex.Name;
                newTex.Index = tex.Index;
                newSprInfo.Textures.Add(newTex);
            }
            return newSprInfo;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Program.NotiBox("This is case sensitive." + "\nThis only applies to selected items", "Information");
            TextEntry ti = new TextEntry(false, "Enter the old text");
            TextEntry ti2 = new TextEntry(false, "Enter the new text");
            string detect;
            string number;
            ti.ShowDialog();
            ti2.ShowDialog();
            detect = ti.Result;
            number = ti2.Result;
            foreach (SpriteSetInfo spr in Grid1.SelectedItems)
            {
                if (spr.FileName.Contains(detect))
                {
                    spr.FileName = spr.FileName.Replace(detect, number);
                }
                if (spr.Name.Contains(detect))
                {
                    spr.Name = spr.Name.Replace(detect, number);
                }
                foreach (SpriteInfo sprInfo in spr.Sprites)
                {
                    if (sprInfo.Name.Contains(detect))
                    {
                        sprInfo.Name = sprInfo.Name.Replace(detect, number);
                    }
                }
                Grid1.Items.Refresh();
            }
        }
    }
}
