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
    /// Interaction logic for ObjEditMain.xaml
    /// </summary>
    public partial class ObjEditMain : Window
    {
        ObjectDatabase db = new ObjectDatabase();
        string saveLocation = null;
        public ObjEditMain()
        {
            InitializeComponent();
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e) // Sprite editor
        {
            ObjectSetInfo obj = ((FrameworkElement)sender).DataContext as ObjectSetInfo;
            ObjEditSub win = new ObjEditSub(obj);
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

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            SortDescription sort = new SortDescription();
            if (Grid1.Items.SortDescriptions.Count > 0)
            {
                sort = Grid1.Items.SortDescriptions[0];
            }
            foreach (ObjectSetInfo x in Grid1.SelectedItems)
            {
                if (db.ObjectSets.Count > 0)
                {
                    db.ObjectSets.Remove(x);
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
            ObjectSetInfo objSet = new ObjectSetInfo();
            objSet.Id = 0;
            objSet.Name = "NEW OBJECT SET ENTRY";
            objSet.TextureFileName = "dummy_tex.bin";
            objSet.FileName = "dummy_obj.bin";
            objSet.ArchiveFileName = "dummy.farc";
            ObjectInfo newObj = new ObjectInfo();
            newObj.Id = 0;
            newObj.Name = "NEW OBJECT ENTRY";
            objSet.Objects.Add(newObj);
            db.ObjectSets.Add(objSet);
            Grid1.Items.Refresh();
        }
        private void Dupe_Click(object sender, RoutedEventArgs e) //Dupe ONE item
        {
            int index = 0;
            List<ObjectSetInfo> objColle = new List<ObjectSetInfo>();
            foreach (var x in Grid1.SelectedItems)
            {
                index = Grid1.Items.IndexOf(x);
                objColle.Add(objDupe(index));
            }
            foreach (ObjectSetInfo obj in objColle)
            {
                db.ObjectSets.Insert(index + 1, obj);
                index++;
            }
            Grid1.Items.Refresh();
        }

        private ObjectSetInfo objDupe(int index) // return ObjectSetInfo
        {
            ObjectSetInfo newObjInfo = new ObjectSetInfo();
            newObjInfo.FileName = db.ObjectSets[index].FileName;
            newObjInfo.ArchiveFileName = db.ObjectSets[index].ArchiveFileName;
            newObjInfo.TextureFileName = db.ObjectSets[index].TextureFileName;
            newObjInfo.Name = db.ObjectSets[index].Name;
            newObjInfo.Id = db.ObjectSets[index].Id;
            foreach (ObjectInfo obj in db.ObjectSets[index].Objects)
            {
                ObjectInfo newObj = new ObjectInfo();
                newObj.Id = obj.Id;
                newObj.Name = obj.Name;
                newObjInfo.Objects.Add(newObj);
            }
            return newObjInfo;
        }
        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Please select your Object Database", //"データベースを選択してください。";
                Filter = "Object Database files|*obj_db.bin", //"データベースファイル |*_db.bin"; 
            };
            if (ofd.ShowDialog() == true)
            {
                if (db.ObjectSets != null)
                {
                    saveLocation = ofd.FileName;
                    Grid1.ItemsSource = null;
                    Grid1.Items.Clear();
                    db.ObjectSets.Clear();
                }
                db = BinaryFile.Load<ObjectDatabase>(ofd.FileName);
                Grid1.ItemsSource = db.ObjectSets;
            }
        }
        private void Save()
        {
            if (saveLocation != null && db.ObjectSets.Count > 0)
            {
                db.Save(saveLocation);
                MessageBox.Show("Saved successfully.");
            }
            else
            {
                MessageBox.Show("Please save your file normally.");
            }
        }
        private void SaveAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Object Database files|*obj_db.bin";
            sfd.FileName = "mod_obj_db.bin";
            if (db.ObjectSets.Count != 0)
            {
                if (sfd.ShowDialog() == true)
                {
                    db.Save(sfd.FileName);
                    MessageBox.Show("Saved successfully.");
                }
                else
                {
                    MessageBox.Show("An error occurred while saving your file" +
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
            foreach (ObjectSetInfo objSet in Grid1.SelectedItems)
            {
                if (objSet.ArchiveFileName.Contains(detect))
                {
                    objSet.ArchiveFileName = objSet.ArchiveFileName.Replace(detect, number);
                }
                if (objSet.TextureFileName.Contains(detect))
                {
                    objSet.TextureFileName = objSet.TextureFileName.Replace(detect, number);
                }
                if (objSet.FileName.Contains(detect))
                {
                    objSet.FileName = objSet.FileName.Replace(detect, number);
                }
                if (objSet.Name.Contains(detect))
                {
                    objSet.Name = objSet.Name.Replace(detect, number);
                }
                foreach (ObjectInfo objInfo in objSet.Objects)
                {
                    if (objInfo.Name.Contains(detect))
                    {
                        objInfo.Name = objInfo.Name.Replace(detect, number);
                    }
                }
            }
            Grid1.Items.Refresh();
        }
    }
}
