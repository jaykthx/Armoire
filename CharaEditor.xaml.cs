using Armoire.Dialogs;
using Microsoft.Win32;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Armoire
{
    public partial class CharaEditor : Window
    {
        public static ObservableCollection<chritmFile> chritmFiles = new ObservableCollection<chritmFile>();
        public CharaEditor()
        {
            InitializeComponent();
            CosDataGrid.Items.IsLiveSorting = true;
            ItemDataGrid.Items.IsLiveSorting = true;
        }
        private void OpenCommandBinding_Executed(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
        private void SaveCommandBinding_Executed(object sender, RoutedEventArgs e)
        {
            if (Program.charaPath != null)
            {
                SaveFile(true);
            }
            else
            {
                Program.NotiBox("You must open a file before trying to save it.", "Error");
            }
        }
        private void SaveAsCommandBinding_Executed(object sender, RoutedEventArgs e)
        {
            SaveFile(false);
        }
        private void Open_CustEditor(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<CustEditor>().Any())
            {
                CustEditor custEditor = new CustEditor();
                custEditor.Show();
            }
            else
            {
                Program.NotiBox("This window is already open.", "Friendly Reminder");
            }
            
        }
        private void Open_MainEditor(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                Program.NotiBox("This window is already open.", "Friendly Reminder");
            }
        }
        private void Open_SprEditor(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<SprEditMain>().Any())
            {
                SprEditMain spr = new SprEditMain();
                spr.Show();
            }
            else
            {
                Program.NotiBox("This window is already open.", "Friendly Reminder");
            }
        }
        private void Open_TexEditor(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<TexEdit>().Any())
            {
                TexEdit texEditor = new TexEdit();
                texEditor.Show();
            }
            else
            {
                Program.NotiBox("This window is already open.", "Friendly Reminder");
            }
        }
        private void Open_ObjEditor(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<ObjEditMain>().Any())
            {
                ObjEditMain objEditor = new ObjEditMain();
                objEditor.Show();
            }
            else
            {
                Program.NotiBox("This window is already open.", "Friendly Reminder");
            }
        }
        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Character Item Table files| *chritm_prop.farc|All files (*.*)|*.*", Multiselect = true };
            if (ofd.ShowDialog() == true)
            {
                Program.charaPath = ofd.FileNames[0];
                OpenProcessNew(ofd.FileNames);
            }
        }
        private void ClearProcess()
        {
            chritmFiles.Clear();
            CosDataGrid.ItemsSource = null;
            ItemDataGrid.ItemsSource = null;
            CosDataGrid.Items.Clear();
            ItemDataGrid.Items.Clear();
            CharaBox.Items.Clear();
        }

        private void OpenProcessNew(string[] fileNames)
        {
            ClearProcess();
            ObservableCollection<chritmFile> fullTemp = new ObservableCollection<chritmFile>();
            foreach (string file in fileNames)
            {
                var farc = BinaryFile.Load<FarcArchive>(file);
                if (fullTemp.Count == 0)
                {
                    ObservableCollection<chritmFile> temp = new ObservableCollection<chritmFile>(Program.IO.ReadCharaFile(farc));
                    foreach(chritmFile chr in temp)
                    {
                        fullTemp.Add(chr);
                    }
                }
                else
                {
                    ObservableCollection<chritmFile> temp = new ObservableCollection<chritmFile>(Program.IO.ReadCharaFile(farc));
                    foreach (chritmFile tempchr in temp)
                    {
                        foreach (chritmFile chr in fullTemp)
                        {
                            if (tempchr.chara == chr.chara)
                            {
                                foreach (cosEntry cos in tempchr.costumes)
                                {
                                    chr.costumes.Add(cos);
                                }
                                foreach (itemEntry item in tempchr.items)
                                {
                                    chr.items.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            chritmFiles = fullTemp;
            foreach (chritmFile x in chritmFiles)
            {
                CharaBox.Items.Add(x.getFullName());
            }
            CosDataGrid.ItemsSource = chritmFiles[0].costumes;
            ItemDataGrid.ItemsSource = chritmFiles[0].items;
            CharaBox.SelectedIndex = 0;
        }

        private void SaveFile(bool isQuickSave)
        {
            if (chritmFiles.Count > 0)
            {
                if (isQuickSave)
                {
                    Program.IO.SaveChr(Program.charaPath, chritmFiles);
                }
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog
                    {
                        Filter = "FARC files (*.farc)|*.farc",
                        FileName = "mod_chritm_prop.farc"
                    };
                    if (sfd.ShowDialog() == true)
                    {
                        Program.charaPath = sfd.FileName;
                        Program.IO.SaveChr(Program.charaPath, chritmFiles);
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void DelCos(object sender, RoutedEventArgs e)
        {
            try
            {
                List<cosEntry> sel = new List<cosEntry>();
                foreach (cosEntry x in CosDataGrid.SelectedItems)
                {
                    sel.Add(x);
                }
                foreach (cosEntry m in sel)
                {
                    chritmFiles[CharaBox.SelectedIndex].costumes.Remove(m);
                }
                CosDataGrid.Items.Refresh();
            }
            catch
            {
                Program.NotiBox("No haga eso.", "Error");
            }
        }
        private void DelItem(object sender, RoutedEventArgs e)
        {
            try
            {
                List<itemEntry> sel = new List<itemEntry>();
                foreach (itemEntry x in ItemDataGrid.SelectedItems)
                {
                    sel.Add(x);
                }
                foreach (itemEntry m in sel)
                {
                    chritmFiles[CharaBox.SelectedIndex].items.Remove(m);
                }
                ItemDataGrid.Items.Refresh();
            }
            catch
            {
                Program.NotiBox("No haga eso.", "Error");
            }
        }

        private void AddCos(object sender, RoutedEventArgs e)
        {
            if (chritmFiles.Count > 0)
            {
                chritmFiles[CharaBox.SelectedIndex].costumes.Insert(CosDataGrid.SelectedIndex + 1, new cosEntry());
                CosDataGrid.Items.Refresh();
            }
        }
        private void AddItem(object sender, RoutedEventArgs e)
        {
            if (chritmFiles.Count > 0)
            {
                chritmFiles[CharaBox.SelectedIndex].items.Insert(ItemDataGrid.SelectedIndex + 1, new itemEntry());
                ItemDataGrid.Items.Refresh();
            }
        }
        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            OpenProcessNew(files);
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CharaBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (chritmFiles.Count > 0)
                {
                    CosDataGrid.ItemsSource = chritmFiles[CharaBox.SelectedIndex].costumes;
                    ItemDataGrid.ItemsSource = chritmFiles[CharaBox.SelectedIndex].items;
                }
            }
            catch { Program.NotiBox("An error occurred.", "Error"); }
            
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            itemEntry item = ((FrameworkElement)sender).DataContext as itemEntry;
            if (chritmFiles.Count > 0 && item != null)
            {
                ItemEdit itemEdit = new ItemEdit(item);
                itemEdit.ShowDialog();
            }
        }

        private void List_Click(object sender, RoutedEventArgs e)
        {
            cosEntry cos = ((FrameworkElement)sender).DataContext as cosEntry;
            if (chritmFiles.Count > 0 && cos != null)
            {
                CosEdit cosEdit = new CosEdit(cos);
                cosEdit.ShowDialog();
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            itemEntry item = ((FrameworkElement)sender).DataContext as itemEntry;
            if(chritmFiles.Count > 0 && item != null)
            {
                PresetPicker ppcker = new PresetPicker(item);
                ppcker.ShowDialog();
            }
        }
    }
}
