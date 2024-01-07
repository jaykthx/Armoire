using Microsoft.Win32;
using MikuMikuLibrary.Databases;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
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
                MessageBox.Show("Saved successfully.");
            }
            else
            {
                MessageBox.Show("An error occurred while saving your file" +
                        "\n" + "Please try again.");
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
                    System.Windows.MessageBox.Show("Saved successfully.");
                }
                else
                {
                    System.Windows.MessageBox.Show("An error occurred while saving your file" +
                        "\n" + "Please try again.");
                }
            }
            else { MessageBox.Show("Please save your file normally."); }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is case sensitive." + "\nThis only applies to selected items");
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
    }
}
