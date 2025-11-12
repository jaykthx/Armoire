using Microsoft.Win32;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using System;
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
        ObjectDatabase db = new();
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
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
        private void Object_Click(object sender, RoutedEventArgs e) // Object sub-editor
        {
            ObjectSetInfo obj = ((FrameworkElement)sender).DataContext as ObjectSetInfo;
            ObjEditSub win = new(obj);
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
            SortDescription sort = new();
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
            ObjectSetInfo objSet = new()
            {
                Id = 0,
                Name = "NEW OBJECT SET ENTRY",
                TextureFileName = "dummy_tex.bin",
                FileName = "dummy_obj.bin",
                ArchiveFileName = "dummy.farc"
            };
            ObjectInfo newObj = new()
            {
                Id = 0,
                Name = "NEW OBJECT ENTRY"
            };
            objSet.Objects.Add(newObj);
            db.ObjectSets.Add(objSet);
            Grid1.Items.Refresh();
        }

        private void Dupe_Click(object sender, RoutedEventArgs e) //Dupe ONE sprite
        {
            int index = 0;
            List<ObjectSetInfo> objColle = new();
            TextEntry tex = new TextEntry(true, Properties.Resources.warn_increment);
            if (tex.ShowDialog() == true && tex.Result.Length > 0)
            {
                foreach (var x in Grid1.SelectedItems)
                {
                    index = db.ObjectSets.IndexOf(x as ObjectSetInfo);
                    objColle.Add(ObjDupe(index, uint.Parse(tex.Result)));
                }
            }
            else
            {
                foreach (var x in Grid1.SelectedItems)
                {
                    index = db.ObjectSets.IndexOf(x as ObjectSetInfo);
                    objColle.Add(ObjDupe(index, 0));
                }
            }
            foreach (ObjectSetInfo obj in objColle)
            {
                db.ObjectSets.Insert(index + 1, obj);
                index++;
            }
            Grid1.ItemsSource = db.ObjectSets;
            if (Grid1.Items.SortDescriptions.Count > 0)
            {
                SortDescription sort = Grid1.Items.SortDescriptions[0];
                Grid1.Items.SortDescriptions.Add(sort);
                Grid1.Items.SortDescriptions.RemoveAt(0);
            }
            Grid1.Items.Refresh();
        }

        private ObjectSetInfo ObjDupe(int index, uint increment) // return ObjectSetInfo
        {
            ObjectSetInfo newObjInfo = new()
            {
                FileName = db.ObjectSets[index].FileName,
                ArchiveFileName = db.ObjectSets[index].ArchiveFileName,
                TextureFileName = db.ObjectSets[index].TextureFileName,
                Name = db.ObjectSets[index].Name + "_DUPE",
                Id = db.ObjectSets[index].Id + increment
            };
            foreach (ObjectInfo obj in db.ObjectSets[index].Objects)
            {
                ObjectInfo newObj = new()
                {
                    Id = obj.Id + increment,
                    Name = obj.Name
                };
                newObjInfo.Objects.Add(newObj);
            }
            return newObjInfo;
        }
        private void OpenFile()
        {
            OpenFileDialog ofd = new()
            {
                Title = "Please select your Object Database", //"データベースを選択してください。";
                Filter = "Object Database files|*obj_db.bin", //"データベースファイル |*_db.bin"; 
                Multiselect = true,
            };
            try
            {
                if (ofd.ShowDialog() == true)
                {
                    Open(ofd.FileNames);
                }
            }

            catch (Exception e)
            {
                PopupNotification pop = new(e.Message);
            }
        }

        private void Open(string[] files)
        {
            if (db.ObjectSets != null)
            {
                saveLocation = files[0];
                Grid1.ItemsSource = null;
                Grid1.Items.Clear();
                db.ObjectSets.Clear();
            }
            foreach(string file in files)
            {
                if (file.EndsWith("obj_db.bin"))
                {
                    ObjectDatabase temp_db = new();
                    temp_db = BinaryFile.Load<ObjectDatabase>(file);
                    foreach (ObjectSetInfo objsetinfo in temp_db.ObjectSets)
                    {
                        db.ObjectSets.Add(objsetinfo);
                    }
                }
            }
            Grid1.ItemsSource = db.ObjectSets;
        }
        private void Save()
        {
            if (saveLocation != null && db.ObjectSets.Count > 0)
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
            SaveFileDialog sfd = new()
            {
                Filter = "Object Database files|*obj_db.bin",
                FileName = "mod_obj_db.bin"
            };
            if (db.ObjectSets.Count != 0)
            {
                if (sfd.ShowDialog() == true)
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
        private void DataGrid_Drop(object sender, System.Windows.DragEventArgs e) // Loads drag and dropped items
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Open(files);
        }
    }
}
