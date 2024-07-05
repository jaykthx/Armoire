using Microsoft.Win32;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for TexEdit.xaml
    /// </summary>
    public partial class TexEdit : Window
    {
        public TexEdit()
        {
            InitializeComponent();
        }
        TextureDatabase db = new TextureDatabase();
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
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            SortDescription sort = new SortDescription();
            if (Grid1.Items.SortDescriptions.Count > 0)
            {
                sort = Grid1.Items.SortDescriptions[0];
            }
            foreach (TextureInfo x in Grid1.SelectedItems)
            {
                if (db.Textures.Count > 0)
                {
                    db.Textures.Remove(x);
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
            TextureInfo texInfo = new TextureInfo
            {
                Id = 0,
                Name = "NEW TEXTURE ENTRY"
            };
            db.Textures.Add(texInfo);
            Grid1.Items.Refresh();
        }
        private void Dupe_Click(object sender, RoutedEventArgs e) //Dupe ONE item
        {
            List<ObjectSetInfo> objColle = new List<ObjectSetInfo>();
            foreach (var x in Grid1.SelectedItems)
            {
                objColle.Add(TexDupe(Grid1.Items.IndexOf(x)));
            }
            Grid1.Items.Refresh();
        }

        private ObjectSetInfo TexDupe(int index) // return ObjectSetInfo
        {
            ObjectSetInfo newObjInfo = new ObjectSetInfo
            {
                Name = db.Textures[index].Name,
                Id = db.Textures[index].Id
            };
            return newObjInfo;
        }
        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Please select your Texture Database", //"データベースを選択してください。";
                Filter = "Texture Database files|*tex_db.bin", //"データベースファイル |*_db.bin"; 
            };
            if (ofd.ShowDialog() == true)
            {
                if (db.Textures != null)
                {
                    saveLocation = ofd.FileName;
                    Grid1.ItemsSource = null;
                    Grid1.Items.Clear();
                    db.Textures.Clear();
                }
                db = BinaryFile.Load<TextureDatabase>(ofd.FileName);
                Grid1.ItemsSource = db.Textures;
            }
        }

        private void Save()
        {
            if (saveLocation != null && db.Textures.Count > 0)
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
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Texture Database files|*tex_db.bin",
                FileName = "mod_tex_db.bin"
            };
            if (db.Textures.Count != 0)
            {
                if (sfd.ShowDialog() == true)
                {
                    db.Save(sfd.FileName);
                    Program.NotiBox(Properties.Resources.exp_6, Properties.Resources.window_notice);
                }
                else
                {
                    Program.NotiBox("An error occurred while saving your file.\nPlease try again.", Properties.Resources.cmn_error);
                }
            }
            else { Program.NotiBox(Properties.Resources.warn_generic, Properties.Resources.cmn_error); }
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            Program.NotiBox("This is case sensitive." + "\nThis only applies to selected items", Properties.Resources.window_notice);
            TextEntry ti = new TextEntry(false, "Enter the old text");
            TextEntry ti2 = new TextEntry(false, "Enter the new text");
            string detect;
            string number;
            ti.ShowDialog();
            ti2.ShowDialog();
            detect = ti.Result;
            number = ti2.Result;
            foreach (TextureInfo texInfo in Grid1.SelectedItems)
            {
                if (texInfo.Name.Contains(detect))
                {
                    texInfo.Name = texInfo.Name.Replace(detect, number);
                }
            }
            Grid1.Items.Refresh();
        }
        private void DataGrid_Drop(object sender, System.Windows.DragEventArgs e) // Loads drag and dropped items
        {
            string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (db.Textures != null)
            {
                saveLocation = files[0];
                Grid1.ItemsSource = null;
                Grid1.Items.Clear();
                db.Textures.Clear();
            }
            if (files[0].EndsWith("tex_db.bin"))
            {
                db = BinaryFile.Load<TextureDatabase>(files[0]);
                Grid1.ItemsSource = db.Textures;
            }
        }
    }
}
