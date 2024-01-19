using Armoire.Dialogs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
using static Armoire.MainWindow;

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
                Program.NotiBox("Usted debe abrir un archivo antes de intentar a salvarlo.", "Error");
            }
        }
        private void SaveAsCommandBinding_Executed(object sender, RoutedEventArgs e)
        {
            SaveFile(false);
        }
        private void Open_CustEditor(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Windows.OfType<CustEditor>().Any())
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
            if (!System.Windows.Application.Current.Windows.OfType<MainWindow>().Any())
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                Program.NotiBox("This window is already open.", "Friendly Reminder");
            }
        }
        private void Open_TexEditor(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Windows.OfType<TexEdit>().Any())
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
            if (!System.Windows.Application.Current.Windows.OfType<ObjEditMain>().Any())
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
            chritmFiles.Clear();
            CosDataGrid.ItemsSource = null;
            ItemDataGrid.ItemsSource = null;
            CosDataGrid.Items.Clear();
            ItemDataGrid.Items.Clear();
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Character Item Table files| *chritm_prop.farc|All files (*.*)|*.*" };
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileName != null)
                {
                    Program.charaPath = ofd.FileName;
                    OpenProcess();
                }
                else { return; }
            }
            CharaBox.SelectedIndex = 0;
        }
        private void OpenProcess()
        {
            CosDataGrid.ItemsSource = null;
            ItemDataGrid.ItemsSource = null;
            CosDataGrid.Items.Clear();
            ItemDataGrid.Items.Clear();
            CharaBox.Items.Clear();
            var farc = BinaryFile.Load<FarcArchive>(Program.charaPath);
            chritmFiles = Program.IO.ReadCharaFile(farc);
            foreach (chritmFile x in chritmFiles)
            {
                CharaBox.Items.Add(x.getFullName());
            }
            CosDataGrid.ItemsSource = chritmFiles[0].costumes;
            ItemDataGrid.ItemsSource = chritmFiles[0].items;
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
            if (chritmFiles[CharaBox.SelectedIndex].costumes.Count > 1)
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
            else
            {
                Program.NotiBox("No haga eso.", "Error");
            }
        }
        private void DelItem(object sender, RoutedEventArgs e)
        {
            if (chritmFiles[CharaBox.SelectedIndex].items.Count > 1)
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
            else
            {
                Program.NotiBox("No haga eso.", "Error");
            }
        }

        private void AddCos(object sender, RoutedEventArgs e)
        {
            chritmFiles[CharaBox.SelectedIndex].costumes.Insert(CosDataGrid.SelectedIndex + 1, GetDummy());
            CosDataGrid.Items.Refresh();
        }
        private void AddItem(object sender, RoutedEventArgs e)
        {
            chritmFiles[CharaBox.SelectedIndex].items.Insert(ItemDataGrid.SelectedIndex + 1, GetDummyItem());
            ItemDataGrid.Items.Refresh();
        }
        private cosEntry GetDummy()
        {
            cosEntry dummyCos = new cosEntry();
            List<int> tempItems = new List<int>
            {
                500,
                1,
                300
            };
            dummyCos.items = new ObservableCollection<int>(tempItems);
            dummyCos.id = 999;
            return dummyCos;
        }
        private itemEntry GetDummyItem()
        {
            itemEntry dummy = new itemEntry();
            dummy.attr = 0;
            dummy.desID = 0;
            ObservableCollection<dataSetTex> setTex = new ObservableCollection<dataSetTex>();
            dummy.dataSetTexes = setTex;
            dummy.face_depth = 0;
            dummy.flag = 0;
            dummy.name = "DUMMY";
            dummy.rpk = 0;
            dummy.type = 0;
            dummy.objset = new List<string>
            {
                "MIKITM001"
            };
            dummy.orgItm= 0;
            dummy.subID = 0;
            dummy.uid = "DUMMY_DIVSKN";
            dummy.no = 999;
            return dummy;
        }

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].EndsWith(".farc"))
            {
                Program.charaPath = files[0];
                OpenProcess();
            }
            else { return; }
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
            if(chritmFiles.Count > 0)
            {
                CosDataGrid.ItemsSource = chritmFiles[CharaBox.SelectedIndex].costumes;
                ItemDataGrid.ItemsSource = chritmFiles[CharaBox.SelectedIndex].items;
            }
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
