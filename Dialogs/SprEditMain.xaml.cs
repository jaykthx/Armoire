﻿using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

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
        SpriteDatabase db = new();
        string saveLocation = null;
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
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
            OpenFileDialog ofd = new()
            {
                Title = "Please select a Sprite Database",
                Filter = "Sprite Database files|*spr_db.bin",
                Multiselect = true
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Open(ofd.FileNames);
            }
        }
        private void Open(string[] files)
        {
            if (db.SpriteSets != null)
            {
                saveLocation = files[0];
                Grid1.ItemsSource = null;
                Grid1.Items.Clear();
                db.SpriteSets.Clear();
            }
            foreach (string file in files)
            {
                if (file.EndsWith("spr_db.bin"))
                {
                    SpriteDatabase temp_db = new();
                    temp_db = BinaryFile.Load<SpriteDatabase>(file);
                    foreach (SpriteSetInfo sprsetinfo in temp_db.SpriteSets)
                    {
                        db.SpriteSets.Add(sprsetinfo);
                    }
                }
            }
            Grid1.ItemsSource = db.SpriteSets;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void Spr_Click(object sender, RoutedEventArgs e) // Sprite editor
        {
            SpriteSetInfo spr = ((FrameworkElement)sender).DataContext as SpriteSetInfo;
            SprEditSub win = new(spr, true);
            win.ShowDialog();
        }

        private void Tex_Click(object sender, RoutedEventArgs e) // Texture editor
        {
            SpriteSetInfo spr = ((FrameworkElement)sender).DataContext as SpriteSetInfo;
            SprEditSub win = new(spr, false);
            win.ShowDialog();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void Save()
        {
            if (saveLocation != null && db.SpriteSets.Count > 0)
            {
                db.Save(saveLocation);
                Program.NotiBox(Properties.Resources.exp_6, Properties.Resources.window_notice);
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_generic, Properties.Resources.cmn_error);
            }
        }
        private void SaveAs()
        {
            SaveFileDialog sfd = new();
            sfd.Filter = "Sprite Database files|*spr_db.bin|All files|*.*";
            sfd.FileName = "mod_spr_db.bin";
            if (db.SpriteSets.Count != 0)
            {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    db.Save(sfd.FileName);
                    Program.NotiBox(Properties.Resources.exp_6, Properties.Resources.window_notice);
                }
                else
                {
                    Program.NotiBox(Properties.Resources.warn_error_save, Properties.Resources.cmn_error);
                }
            }
            else { Program.NotiBox(Properties.Resources.warn_generic, Properties.Resources.cmn_error); }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            SortDescription sort = new();
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
            SpriteInfo newSpr = new();
            newSpr.Id = 0;
            newSpr.Name = "NEW SPR ENTRY";
            newSpr.Index = 0;
            SpriteTextureInfo newTex = new();
            newTex.Id = 0;
            newTex.Name = "NEW TEX ENTRY";
            newTex.Index = 0;
            SpriteSetInfo newSprInfo = new();
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
            List<SpriteSetInfo> spriteColle = new();
            foreach (var x in Grid1.SelectedItems)
            {
                TextEntry tex = new TextEntry(true, "Please enter increment for ID value.");
                if (tex.ShowDialog() == true && tex.Result.Length > 0)
                {
                    index = db.SpriteSets.IndexOf(x as SpriteSetInfo);
                    spriteColle.Add(spriteDupe(index, uint.Parse(tex.Result)));
                    foreach (SpriteSetInfo spr in spriteColle)
                    {
                        db.SpriteSets.Insert(index + 1, spr);
                        index++;
                    }
                }
                else
                {
                    spriteColle.Add(spriteDupe(index, 0));
                    foreach (SpriteSetInfo spr in spriteColle)
                    {
                        db.SpriteSets.Insert(index + 1, spr);
                        index++;
                    }
                }
            }
            Grid1.ItemsSource = db.SpriteSets;
            SortDescription sort = Grid1.Items.SortDescriptions[0];
            Grid1.Items.SortDescriptions.Add(sort);
            Grid1.Items.SortDescriptions.RemoveAt(0);
            Grid1.Items.Refresh();
        }

        private SpriteSetInfo spriteDupe(int index, uint increment)
        {
            SpriteSetInfo newSprInfo = new();
            newSprInfo.FileName = db.SpriteSets[index].FileName;
            newSprInfo.Name = db.SpriteSets[index].Name + "_DUPE";
            newSprInfo.Id = db.SpriteSets[index].Id + increment;
            foreach (SpriteInfo spr in db.SpriteSets[index].Sprites)
            {
                SpriteInfo newSpr = new();
                newSpr.Id = spr.Id + increment;
                newSpr.Name = spr.Name;
                newSpr.Index = spr.Index;
                newSprInfo.Sprites.Add(newSpr);
            }
            foreach (SpriteTextureInfo tex in db.SpriteSets[index].Textures)
            {
                SpriteTextureInfo newTex = new();
                newTex.Id = tex.Id + increment;
                newTex.Name = tex.Name;
                newTex.Index = tex.Index;
                newSprInfo.Textures.Add(newTex);
            }
            return newSprInfo;
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            Program.NotiBox(Properties.Resources.warn_case_sensitive, Properties.Resources.window_notice);
            TextEntry ti = new(false, Properties.Resources.replace_old);
            TextEntry ti2 = new(false, Properties.Resources.replace_new);
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
        private void DataGrid_Drop(object sender, System.Windows.DragEventArgs e) // Loads drag and dropped items
        {
            string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            Open(files);
        }
    }
}
