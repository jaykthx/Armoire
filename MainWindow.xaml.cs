using Armoire.Dialogs;
using Armoire.Properties;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Armoire
{
    public partial class MainWindow : Window
    {
        int secretCount = 0;
        //module
        public static ObservableCollection<module> Modules = new();
        //cust
        public static ObservableCollection<cstm_item> CustItems = new();
        //chara
        public static ObservableCollection<chritmFile> chritmFiles = new();

        public List<string> charas_Customize = new() { "MIKU", "RIN", "LEN", "LUKA", "KAITO", "MEIKO", "NERU", "HAKU", "SAKINE", "TETO", "ALL" };
        public List<string> partsList = new() { "KAMI", "FACE", "NECK", "ZUJO", "BACK" };
        public MainWindow()
        {
            InitializeComponent();
            charaBox_Modules.ItemsSource = Program.charas;
            attrBox.ItemsSource = Enum.GetValues(typeof(Attr)).Cast<Attr>();
            DataGrid_Modules.ItemsSource = Modules;
            DataGrid_Modules.Items.IsLiveSorting = true;
            charaBox_Customize.ItemsSource = charas_Customize;
            partBox.ItemsSource = partsList;
            DataGrid_Customize.ItemsSource = CustItems;
            DataGrid_Customize.Items.IsLiveSorting = true;
            ItemDataGrid.Items.IsLiveSorting = true;
            if (Settings.Default.themeSetting.Length > 2)
            {
                ChangeTheme(Settings.Default.themeSetting);
            }
        }
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(TabControl1.SelectedIndex == 0)
            {
                OpenFile_Module();
            }
            else if(TabControl1.SelectedIndex == 1)
            {
                OpenFile_Customize();
            }
            else
            {
                OpenFile_Chara();
            }
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (TabControl1.SelectedIndex == 0)
            {
                if (Program.modulePath != null)
                {
                    SaveFile(true);
                }
                else
                {
                    Program.NotiBox(Properties.Resources.warn_flop_save, Properties.Resources.cmn_error);
                }
            }
            else if (TabControl1.SelectedIndex == 1)
            {
                if (Program.customPath != null)
                {
                    SaveFile_Customize(true);
                }
                else
                {
                    Program.NotiBox(Properties.Resources.warn_flop_save, Properties.Resources.cmn_error);
                }
            }
            else
            {
                if (Program.charaPath != null)
                {
                    SaveFile_Chara(true);
                }
                else
                {
                    Program.NotiBox(Properties.Resources.warn_flop_save, Properties.Resources.cmn_error);
                }
            }
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (TabControl1.SelectedIndex == 0)
            {
                SaveFile(false);
            }
            else if (TabControl1.SelectedIndex == 1)
            {
                SaveFile_Customize(false);
            }
            else
            {
                SaveFile_Chara(false);
            }
        }
        private void OpenFile_Module()
        {
            OpenFileDialog ofd = new() { Filter = "Supported files|*_module_tbl.farc|Module Table Files|*_module_tbl.farc|All files (*.*)|*.*" };
            ofd.Multiselect= true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Modules.Clear();
                ObservableCollection<module> tempModules = new();
                foreach (string file in ofd.FileNames)
                {
                    if (file.EndsWith(".farc"))
                    {
                        Program.modulePath = ofd.FileNames[0];
                        var farc = BinaryFile.Load<FarcArchive>(file);
                        tempModules = Program.IO.ReadModuleFile(farc);
                        List<module> modules = tempModules.ToList();
                        foreach (module m in modules)
                        {
                            Modules.Add(m);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            DataGrid_Modules.ItemsSource = Modules;
        }
        private void SaveFile(bool isQuickSave)
        {
            if (Modules.Count > 0)
            {
                if (isQuickSave)
                {
                    Program.IO.SaveFile<module>(Program.modulePath, Modules);
                }
                else
                {
                    SaveFileDialog sfd = new() { Filter = "FARC files (*.farc)|*.farc" };
                    sfd.FileName = "mod_gm_module_tbl.farc";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Program.modulePath = sfd.FileName;
                        Program.IO.SaveFile<module>(sfd.FileName, Modules);
                    }
                }
            }
            else { return; }
        }

        private void DeleteEntry(object sender, RoutedEventArgs e) // Deletes all *selected* entries
        {
            if (Modules.Count > 1)
            {
                List<module> sel = new();
                foreach (module x in DataGrid_Modules.SelectedItems)
                {
                    sel.Add(x);
                }
                foreach (module m in sel)
                {
                    Modules.Remove(m);

                }
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_delete, Properties.Resources.cmn_error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) // Add at position
        {
            Modules.Insert(DataGrid_Modules.SelectedIndex + 1, GetDummy_Module());
        }
        private void AddButton2_Click(object sender, RoutedEventArgs e) // Add to end of list
        {
            Modules.Add(GetDummy_Module());
        }
        private module GetDummy_Module() // Dummy module that can be added to the list
        {
            module dummyModule = new();
            dummyModule.name = "ダミー";
            dummyModule.chara = "MIKU";
            dummyModule.id = 999;
            dummyModule.cos = "COS_001";
            dummyModule.ng = false;
            dummyModule.shop_price = "750";
            return dummyModule;
        }

        private void DataGrid_Drop(object sender, System.Windows.DragEventArgs e) // Loads drag and dropped items
        {
            string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (files[0].EndsWith(".farc"))
            {
                Program.modulePath = files[0];
                var farc = BinaryFile.Load<FarcArchive>(Program.modulePath);
                Modules = Program.IO.ReadModuleFile(farc);
                DataGrid_Modules.ItemsSource = Modules;
            }
            else { return; }
        }
        private void Open_SprEditor(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Windows.OfType<SprEditMain>().Any())
            {
                SprEditMain spr = new();
                spr.Show();
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_window, Properties.Resources.window_notice);
            }
        }
        private void Open_TexEditor(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Windows.OfType<TexEdit>().Any())
            {
                TexEdit texEditor = new();
                texEditor.Show();
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_window, Properties.Resources.window_notice);
            }
        }
        private void Open_ObjEditor(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Windows.OfType<ObjEditMain>().Any())
            {
                ObjEditMain objEditor = new();
                objEditor.Show();
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_window, Properties.Resources.window_notice);
            }
        }
        private void Wizard_Click(object sender, RoutedEventArgs e)
        {
            ModuleWizard wiz = new();
            wiz.Show();
        }
        private void Wizard_Click_Customize(object sender, RoutedEventArgs e)
        {
            CustomiseWizard wiz = new();
            wiz.Show();
        }
        private void Test_Click(object sender, RoutedEventArgs e)
        {
            // Testing DB Cleaner - DISABLED for now!!
            OpenFileDialog ofd = new();
            FolderBrowserDialog fbd = new();
            SaveFileDialog sfd = new();
            fbd.Description = "Pick the mod folder.";
            List<string> objsetFarcs = new();
            List<uint> textureIDs = new();
            if(fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach(string s in Directory.EnumerateFiles(fbd.SelectedPath, "*.farc", SearchOption.AllDirectories))
                {
                    if (s.Contains("objset"))
                    {
                        objsetFarcs.Add(Path.GetFileName(s));
                        var farc = BinaryFile.Load<FarcArchive>(s);
                        foreach (string fileName in farc)
                        {
                            if (fileName.EndsWith("_obj.bin"))
                            {
                                string mainName = fileName.Remove(fileName.Length - 8, 8);
                                var stream = new MemoryStream();
                                var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                                var objset = new ObjectSet();
                                objset.Load(source);
                                foreach (uint id in objset.TextureIds)
                                {
                                    textureIDs.Add(id);
                                }
                            }
                            farc.Dispose();
                        }
                    }
                }
            }
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) //pick obj_db
            {
                ObjectDatabase obj_db = BinaryFile.Load<ObjectDatabase>(ofd.FileName);
                ObjectDatabase new_obj_db = new();
                foreach(ObjectSetInfo o in obj_db.ObjectSets)
                {
                    if (objsetFarcs.Contains(o.ArchiveFileName))
                    {
                        new_obj_db.ObjectSets.Add(o);
                    }
                }
                sfd.FileName = "clean_obj_db.bin";
                sfd.Title = "Please save your new Object Database file.";
                if(sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    new_obj_db.Save(sfd.FileName);
                }
            }
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) //pick tex_db
            {
                TextureDatabase tex_db = BinaryFile.Load<TextureDatabase>(ofd.FileName);
                TextureDatabase new_tex_db = new();
                tex_db.Load(ofd.FileName);
                foreach (TextureInfo o in tex_db.Textures)
                {
                    if (textureIDs.Contains(o.Id))
                    {
                        new_tex_db.Textures.Add(o);
                    }
                }
                sfd.FileName = "clean_tex_db.bin";
                sfd.Title = "Please save your new Texture Database file.";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    new_tex_db.Save(sfd.FileName);
                }
            }
        }

        //cust functions
        private void DataGrid_Drop_Customize(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].EndsWith(".farc"))
            {
                Program.customPath = files[0];
                var farc = BinaryFile.Load<FarcArchive>(Program.customPath);
                CustItems = Program.IO.ReadCustomFile(farc);
                DataGrid_Customize.ItemsSource = CustItems;
            }
            else { return; }
        }
        private void OpenFile_Customize()
        {
            OpenFileDialog ofd = new() { Filter = "Supported files|*customize_item_tbl.farc|Customize Item Table files|*customize_item_tbl.farc|All files (*.*)|*.*" };
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CustItems.Clear();
                ObservableCollection<cstm_item> tempCustoms = new();
                foreach (string file in ofd.FileNames)
                {
                    if (file.EndsWith(".farc"))
                    {
                        Program.customPath = ofd.FileNames[0];
                        var farc = BinaryFile.Load<FarcArchive>(file);
                        tempCustoms = Program.IO.ReadCustomFile(farc);
                        List<cstm_item> customs = tempCustoms.ToList();
                        foreach (cstm_item c in tempCustoms)
                        {
                            CustItems.Add(c);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            DataGrid_Customize.ItemsSource = CustItems;
        }
        private void SaveFile_Customize(bool isQuickSave)
        {
            if (CustItems.Count > 0)
            {
                if (isQuickSave)
                {
                    Program.IO.SaveFile<cstm_item>(Program.customPath, CustItems);
                }
                else
                {
                    SaveFileDialog sfd = new() { Filter = "FARC files (*.farc)|*.farc" };
                    sfd.FileName = "mod_gm_customize_item_tbl.farc";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Program.customPath = sfd.FileName;
                        Program.IO.SaveFile<cstm_item>(sfd.FileName, CustItems);
                    }
                }
            }
            else { return; }
        }
        private void DeleteEntry_Customize(object sender, RoutedEventArgs e)
        {
            if (CustItems.Count > 1)
            {
                List<cstm_item> sel = new();
                foreach (cstm_item x in DataGrid_Customize.SelectedItems)
                {
                    sel.Add(x);
                }
                foreach (cstm_item m in sel)
                {
                    CustItems.Remove(m);

                }
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_delete, Properties.Resources.cmn_error);
            }
        }
        private void AddButton_Click_Customize(object sender, RoutedEventArgs e)
        {
            CustItems.Insert(DataGrid_Customize.SelectedIndex + 1, GetDummy_Customize());
        }
        private void AddButton2_Click_Customize(object sender, RoutedEventArgs e)
        {
            CustItems.Add(GetDummy_Customize());
        }
        private cstm_item GetDummy_Customize()
        {
            cstm_item dummyModule = new();
            dummyModule.name = "ダミー";
            dummyModule.chara = "ALL";
            dummyModule.id = 999;
            dummyModule.bind_module = -1;
            dummyModule.parts = "ZUJO";
            dummyModule.ng = false;
            dummyModule.shop_price = "300";
            return dummyModule;
        }

        private void Open_Click_Customize(object sender, RoutedEventArgs e)
        {
            OpenFile_Customize();
        }

        private void Save_Click_Customize(object sender, RoutedEventArgs e)
        {
            if (Program.customPath != null)
            {
                SaveFile_Customize(true);
            }
            else
            {
                Program.NotiBox(Properties.Resources.exp_4, Properties.Resources.cmn_error);
            }
        }
        private void SaveAs_Click_Customize(object sender, RoutedEventArgs e)
        {
            SaveFile_Customize(false);
        }
        private void OpenFile_Chara()
        {
            OpenFileDialog ofd = new() { Filter = "Character Item Table files| *chritm_prop.farc|All files (*.*)|*.*", Multiselect = true };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Program.charaPath = ofd.FileNames[0];
                OpenProcessNew(ofd.FileNames);
            }
        }
        private void ClearProcess_Chara()
        {
            chritmFiles.Clear();
            CosListBox.ItemsSource = null;
            ItemDataGrid.ItemsSource = null;
            CosListBox.Items.Clear();
            ItemDataGrid.Items.Clear();
            CharaBox.Items.Clear();
        }

        private void OpenProcessNew(string[] fileNames)
        {
            ClearProcess_Chara();
            ObservableCollection<chritmFile> fullTemp = new();
            foreach (string file in fileNames)
            {
                var farc = BinaryFile.Load<FarcArchive>(file);
                if (fullTemp.Count == 0)
                {
                    ObservableCollection<chritmFile> temp = new(Program.IO.ReadCharaFile(farc));
                    foreach (chritmFile chr in temp)
                    {
                        fullTemp.Add(chr);
                    }
                }
                else
                {
                    ObservableCollection<chritmFile> temp = new(Program.IO.ReadCharaFile(farc));
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
            CosListBox.ItemsSource = chritmFiles[0].costumes;
            ItemDataGrid.ItemsSource = chritmFiles[0].items;
            CharaBox.SelectedIndex = 0;
        }

        private void SaveFile_Chara(bool isQuickSave)
        {
            if (chritmFiles.Count > 0)
            {
                if (isQuickSave)
                {
                    Program.IO.SaveChr(Program.charaPath, chritmFiles);
                }
                else
                {
                    SaveFileDialog sfd = new()
                    {
                        Filter = "FARC files (*.farc)|*.farc",
                        FileName = "mod_chritm_prop.farc"
                    };
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                chritmFiles[CharaBox.SelectedIndex].costumes.Remove(CosListBox.SelectedItem as cosEntry);
                CosListBox.Items.Refresh();
            }
            catch
            {
                Program.NotiBox(Properties.Resources.warn_generic, Properties.Resources.cmn_error);
            }
        }
        private void DelItem(object sender, RoutedEventArgs e)
        {
            try
            {
                List<itemEntry> sel = new();
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
                Program.NotiBox(Properties.Resources.warn_generic, Properties.Resources.cmn_error);
            }
        }

        private void AddCos(object sender, RoutedEventArgs e)
        {
            if (chritmFiles.Count > 0)
            {
                    chritmFiles[CharaBox.SelectedIndex].costumes.Add(new cosEntry());
                    CosListBox.Items.Refresh();
            }
        }
        private void EditCos(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chritmFiles.Count > 0 && CharaBox.SelectedIndex > -1)
                {
                    int index = chritmFiles[CharaBox.SelectedIndex].costumes.IndexOf(CosListBox.SelectedItem as cosEntry);
                    cosEntry cos = chritmFiles[CharaBox.SelectedIndex].costumes[index];
                    CosEdit cosEdit = new(cos);
                    cosEdit.ShowDialog();
                    CosListBox.Items.Refresh();
                }
            }
            catch (Exception)
            {
                Program.NotiBox(Properties.Resources.warn_generic, Armoire.Properties.Resources.cmn_error);
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
        private void DataGrid_Drop_Chara(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            OpenProcessNew(files);
        }

        private void CharaBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (chritmFiles.Count > 0)
                {
                    CosListBox.ItemsSource = chritmFiles[CharaBox.SelectedIndex].costumes;
                    ItemDataGrid.ItemsSource = chritmFiles[CharaBox.SelectedIndex].items;
                }
            }
            catch { Program.NotiBox(Properties.Resources.warn_generic, Properties.Resources.cmn_error); }

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            itemEntry item = ((FrameworkElement)sender).DataContext as itemEntry;
            if (chritmFiles.Count > 0 && item != null)
            {
                ItemEdit itemEdit = new(item);
                itemEdit.ShowDialog();
            }
        }

        private void List_Click(object sender, RoutedEventArgs e)
        {
            cosEntry cos = ((FrameworkElement)sender).DataContext as cosEntry;
            if (chritmFiles.Count > 0 && cos != null)
            {
                CosEdit cosEdit = new(cos);
                cosEdit.ShowDialog();
            }
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            itemEntry item = ((FrameworkElement)sender).DataContext as itemEntry;
            if (chritmFiles.Count > 0 && item != null)
            {
                PresetPicker ppcker = new(item, chritmFiles[CharaBox.SelectedIndex].chara.ToUpper(), true, false);
                ppcker.ShowDialog();
            }
        }

        private void ChangeTheme(string theme)
        {
            Settings.Default.themeSetting = theme;
            Settings.Default.Save();
            var app = (App)System.Windows.Application.Current;
            app.ChangeTheme(new Uri(("/Themes/" + theme + ".xaml"), UriKind.Relative));
        }

        private void Pink_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Pink");
            secretCount++;
            if(secretCount >= 10)
            {
                ChangeTheme("PSP");
            }
        }
        private void Blue_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Blue");
            secretCount = 0;
        }
        private void Dark_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Dark");
            secretCount = 0;
        }
        private void Light_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Light");
            secretCount = 0;
        }
        private void Default_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Default");
            secretCount = 0;
        }
        private void Vampire_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Vampire");
            secretCount = 0;
        }
    }
}
