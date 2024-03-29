﻿using Microsoft.Win32;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

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
        private void Object_Click(object sender, RoutedEventArgs e) // Object sub-editor
        {
            ObjectSetInfo obj = ((FrameworkElement)sender).DataContext as ObjectSetInfo;
            ObjEditSub win = new ObjEditSub(obj);
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
            ObjectSetInfo objSet = new ObjectSetInfo
            {
                Id = 0,
                Name = "NEW OBJECT SET ENTRY",
                TextureFileName = "dummy_tex.bin",
                FileName = "dummy_obj.bin",
                ArchiveFileName = "dummy.farc"
            };
            ObjectInfo newObj = new ObjectInfo
            {
                Id = 0,
                Name = "NEW OBJECT ENTRY"
            };
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
                objColle.Add(ObjDupe(index));
            }
            foreach (ObjectSetInfo obj in objColle)
            {
                db.ObjectSets.Insert(index + 1, obj);
                index++;
            }
            Grid1.Items.Refresh();
        }

        private ObjectSetInfo ObjDupe(int index) // return ObjectSetInfo
        {
            ObjectSetInfo newObjInfo = new ObjectSetInfo
            {
                FileName = db.ObjectSets[index].FileName,
                ArchiveFileName = db.ObjectSets[index].ArchiveFileName,
                TextureFileName = db.ObjectSets[index].TextureFileName,
                Name = db.ObjectSets[index].Name,
                Id = db.ObjectSets[index].Id
            };
            foreach (ObjectInfo obj in db.ObjectSets[index].Objects)
            {
                ObjectInfo newObj = new ObjectInfo
                {
                    Id = obj.Id,
                    Name = obj.Name
                };
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
                Program.NotiBox("Saved successfully.", "Notice");
            }
            else
            {
                Program.NotiBox("Please save your file correctly.", "Error");
            }
        }
        private void SaveAs()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Object Database files|*obj_db.bin",
                FileName = "mod_obj_db.bin"
            };
            if (db.ObjectSets.Count != 0)
            {
                if (sfd.ShowDialog() == true)
                {
                    db.Save(sfd.FileName);
                    Program.NotiBox("Saved successfully.", "Notice");
                }
                else
                {
                    Program.NotiBox("An error occurred while saving your file.\nPlease try again.", "Error");
                }
            }
            else { Program.NotiBox("Please save your file correctly.", "Error"); }
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
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
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
