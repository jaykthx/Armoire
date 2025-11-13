using Armoire.Dialogs;
using Armoire.Properties;
using Microsoft.Win32;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Armoire
{
    public partial class MainWindow : Window
    {
        
        int secretCount = 0;
        int secretCount2 = 0;
        readonly string[] availableThemes = ["Default", "Light", "Blue", "Pink", "Violet", "PSP", "Miku", "Rin", "Len", "Luka", "Kaito", "Meiko", "Real"];
        //module
        public static ObservableCollection<Module> Modules = new();
        //cust
        public static ObservableCollection<CustomizeItem> CustItems = new();
        //chara
        public static ObservableCollection<CharacterItemFile> chritmFiles = new();
        public enum LanguageSetting
        {
            en = 0,
            ja = 1,
        }


        public List<string> charas_Customize = new() { "MIKU", "RIN", "LEN", "LUKA", "KAITO", "MEIKO", "NERU", "HAKU", "SAKINE", "TETO", "ALL" };
        public List<string> partsList = new() { "KAMI", "ZUJO", "FACE", "NECK", "BACK" };
        private readonly string[] languages = ["English", "日本語"];
        private bool _isModuleUnsaved;
        public bool isModuleUnsaved
        {
            get { return _isModuleUnsaved; }
            set
            {
                _isModuleUnsaved = value;
                if(value == true)
                {
                    moduleTab.Header = Properties.Resources.tab_module + "*";
                }
                else
                {
                    moduleTab.Header = Properties.Resources.tab_module;
                }
            }
        }
        private bool _isCustomizeUnsaved;
        public bool isCustomizeUnsaved
        {
            get { return _isCustomizeUnsaved; }
            set
            {
                _isCustomizeUnsaved = value;
                if (value == true)
                {
                    customTab.Header = Properties.Resources.tab_custom + "*";
                }
                else
                {
                    customTab.Header = Properties.Resources.tab_custom;
                }
            }
        }
        private bool _isCharacterUnsaved;
        public bool isCharacterUnsaved
        {
            get { return _isCharacterUnsaved; }
            set
            {
                _isCharacterUnsaved = value;
                if (value == true)
                {
                    charaTab.Header = Properties.Resources.tab_chritm + "*";
                }
                else
                {
                    charaTab.Header = Properties.Resources.tab_chritm;
                }
            }
        }

        void ContextMenuFix()
        {
            var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            Action setAlignmentValue = () => {
                if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null) menuDropAlignmentField.SetValue(null, false);
            };
            setAlignmentValue();
            SystemParameters.StaticPropertyChanged += (sender, e) => { setAlignmentValue(); };
        }
        public MainWindow()
        {
            ContextMenuFix();
            Settings.Default.Reload();
            var selLang = (LanguageSetting)Settings.Default.languageSetting;
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(selLang.ToString());
            InitializeComponent();
            this.Title = $"Armoire ({Program.versionDate.Date.ToShortDateString()})";
            SetAlignment(Settings.Default.alignmentSetting);
            AutoCheckComboBox.SelectedIndex = Convert.ToInt32(Settings.Default.autoCheckSetting);
            if (Settings.Default.autoCheckSetting)
            {
                Program.DownloadUpdate();
            }
            charaBox_Modules.ItemsSource = Program.charas;
            attrBox.ItemsSource = Enum.GetValues(typeof(Attr)).Cast<Attr>();
            DataGrid_Modules.ItemsSource = Modules;
            if (App.isEgg)
            {
                LenThemeButton.Visibility = Visibility.Collapsed;
                if(Settings.Default.themeSetting == "Len")
                {
                    Settings.Default.themeSetting = "Default";
                    ChangeTheme("Default");
                }
            }
            DataGrid_Modules.Items.IsLiveSorting = true;
            charaBox_Customize.ItemsSource = charas_Customize;
            partBox.ItemsSource = partsList;
            DataGrid_Customize.ItemsSource = CustItems;
            DataGrid_Customize.Items.IsLiveSorting = true;
            ItemDataGrid.Items.IsLiveSorting = true;
            LanguageComboBox.ItemsSource = languages;
            LanguageComboBox.SelectedIndex = Settings.Default.languageSetting;
            Debug.WriteLine(selLang);
            if (availableThemes.Contains(Settings.Default.themeSetting))
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
            switch (TabControl1.SelectedIndex)
            {
                case 0:
                    SaveFile(true);
                    isModuleUnsaved = false;
                    break;
                case 1:
                    SaveFile_Customize(true);
                    isCustomizeUnsaved = false;
                    break;
                case 2:
                    SaveFile_Chara(true);
                    isCharacterUnsaved = false;
                    break;
                default: // this should not happen :)
                    break;
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
                isCharacterUnsaved = false;
            }
        }
        private void OpenFile_Module()
        {
            OpenFileDialog ofd = new() { Filter = "Supported files|*_module_tbl.farc|Module Table Files|*_module_tbl.farc|All files (*.*)|*.*" };
            ofd.Multiselect = true;
            try
            {
                if (ofd.ShowDialog() == true)
                {
                    isModuleUnsaved = false;
                    Modules.Clear();
                    ObservableCollection<Module> tempModules = new();
                    foreach (string file in ofd.FileNames)
                    {
                        if (file.EndsWith(".farc"))
                        {
                            Program.modulePath = ofd.FileNames[0];
                            var farc = BinaryFile.Load<FarcArchive>(file);
                            tempModules = Task.Run(() => Program.IO.ReadModuleFile(farc)).Result;
                            List<Module> modules = tempModules.ToList();
                            foreach (Module m in modules)
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
            }
            catch (Exception e)
            {
                PopupNotification pop = new(e.Message);
            }
            DataGrid_Modules.ItemsSource = Modules;
        }
        private void SaveFile(bool isQuickSave)
        {
            try
            {
                if (Modules.Count > 0)
                {
                    if (isQuickSave && !string.IsNullOrEmpty(Program.modulePath))
                    {
                        Program.IO.SaveFile<Module>(Program.modulePath, Modules);
                    }
                    else
                    {
                        SaveFileDialog sfd = new() { Filter = "FARC files (*.farc)|*.farc" };
                        sfd.FileName = "mod_gm_module_tbl.farc";
                        if (sfd.ShowDialog() == true)
                        {
                            Program.modulePath = sfd.FileName;
                            Program.IO.SaveFile<Module>(sfd.FileName, Modules);
                        }
                    }
                }
                else { PopupNotification pop = new(Properties.Resources.warn_error_save); }
            }
            catch (Exception e)
            {
                PopupNotification popup = new(e.Message);
            }
        }

        private void DeleteEntry(object sender, RoutedEventArgs e) // Deletes all *selected* entries
        {
            if (Modules.Count > 1)
            {
                List<Module> sel = new List<Module>();
                foreach (object item in DataGrid_Modules.SelectedItems)
                {
                    sel.Add(item as Module);
                }
                foreach (Module m in sel)
                {
                    Modules.Remove(m);
                }
            }
            else
            {
                PopupNotification popup = new(Properties.Resources.warn_delete);
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
        private Module GetDummy_Module() // Dummy module that can be added to the list
        {
            Module dummyModule = new();
            dummyModule.name = "ダミー";
            dummyModule.chara = "MIKU";
            dummyModule.id = 999;
            dummyModule.cos = "COS_001";
            dummyModule.ng = false;
            dummyModule.shop_price = "750";
            return dummyModule;
        }

        private async void DataGrid_Drop(object sender, DragEventArgs e) // Loads drag and dropped items
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Modules.Clear();
            isModuleUnsaved = false;
            if (files[0].EndsWith("gm_module_tbl.farc"))
            {
                Program.modulePath = files[0];
                var farc = BinaryFile.Load<FarcArchive>(Program.modulePath);
                Modules = await Program.IO.ReadModuleFile(farc);
                DataGrid_Modules.ItemsSource = Modules;
            }
            else { PopupNotification pop = new(Properties.Resources.warn_generic); }
        }
        private void Open_SprEditor(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<SprEditMain>().Any())
            {
                SprEditMain spr = new();
                spr.Show();
            }
            else
            {
                PopupNotification pop = new(Properties.Resources.warn_window);
            }
        }
        private void Open_TexEditor(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<TexEdit>().Any())
            {
                TexEdit texEditor = new();
                texEditor.Show();
            }
            else
            {
                PopupNotification pop = new(Properties.Resources.warn_window);
            }
        }
        private void Open_ObjEditor(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<ObjEditMain>().Any())
            {
                ObjEditMain objEditor = new();
                objEditor.Show();
            }
            else
            {
                PopupNotification pop = new(Properties.Resources.warn_window);
            }
        }

        private async void DataGrid_Drop_Customize(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            CustItems.Clear();
            isCustomizeUnsaved = false;
            if (files[0].EndsWith("gm_customize_item_tbl.farc"))
            {
                Program.customPath = files[0];
                var farc = BinaryFile.Load<FarcArchive>(Program.customPath);
                CustItems = await Program.IO.ReadCustomFile(farc);
                DataGrid_Customize.ItemsSource = CustItems;
            }
            else { PopupNotification pop = new(Properties.Resources.warn_generic); }
        }
        private void OpenFile_Customize()
        {
            OpenFileDialog ofd = new() { Filter = "Supported files|*customize_item_tbl.farc|Customize Item Table files|*customize_item_tbl.farc|All files (*.*)|*.*" };
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                isCustomizeUnsaved = false;
                CustItems.Clear();
                ObservableCollection<CustomizeItem> tempCustoms = new();
                foreach (string file in ofd.FileNames)
                {
                    try
                    {
                        if (file.EndsWith(".farc"))
                        {
                            Program.customPath = ofd.FileNames[0];
                            var farc = BinaryFile.Load<FarcArchive>(file);
                            tempCustoms = Task.Run(() => Program.IO.ReadCustomFile(farc)).Result;
                            List<CustomizeItem> customs = tempCustoms.ToList();
                            foreach (CustomizeItem c in tempCustoms)
                            {
                                CustItems.Add(c);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        PopupNotification pop = new(e.Message);
                    }
                }
            }
            DataGrid_Customize.ItemsSource = CustItems;
        }
        private void SaveFile_Customize(bool isQuickSave)
        {
            if (CustItems.Count > 0)
            {
                if (isQuickSave && !string.IsNullOrEmpty(Program.customPath))
                {
                    Program.IO.SaveFile<CustomizeItem>(Program.customPath, CustItems);
                }
                else
                {
                    SaveFileDialog sfd = new() { Filter = "FARC files (*.farc)|*.farc" };
                    sfd.FileName = "mod_gm_customize_item_tbl.farc";
                    if (sfd.ShowDialog() == true)
                    {
                        Program.customPath = sfd.FileName;
                        Program.IO.SaveFile<CustomizeItem>(sfd.FileName, CustItems);
                    }
                }
            }
            else { PopupNotification pop = new(Properties.Resources.warn_error_save); }
        }
        private void DeleteEntry_Customize(object sender, RoutedEventArgs e)
        {
            if (CustItems.Count > 1)
            {
                List<CustomizeItem> sel = new();
                foreach (CustomizeItem x in DataGrid_Customize.SelectedItems)
                {
                    sel.Add(x);
                }
                foreach (CustomizeItem m in sel)
                {
                    CustItems.Remove(m);

                }
            }
            else
            {
                PopupNotification pop = new(Properties.Resources.warn_delete);
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
        private CustomizeItem GetDummy_Customize()
        {
            CustomizeItem dummyModule = new();
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
            SaveFile_Customize(true);
        }
        private void SaveAs_Click_Customize(object sender, RoutedEventArgs e)
        {
            SaveFile_Customize(false);
        }
        private void OpenFile_Chara()
        {
            OpenFileDialog ofd = new() { Filter = "Character Item Table files| *chritm_prop.farc|All files (*.*)|*.*", Multiselect = true };
            try
            {
                if (ofd.ShowDialog() == true)
                {
                    Program.charaPath = ofd.FileNames[0];
                    isCharacterUnsaved = false;
                    ClearProcess_Chara();
                    chritmFiles = Task.Run(() => Program.IO.OpenCharacterItemFile(ofd.FileNames)).Result;
                    foreach (CharacterItemFile x in chritmFiles)
                    {
                        CharaBox.Items.Add(x.GetFullName());
                    }
                    CharaBox.SelectedIndex = 0;
                }
            }
            catch (Exception e)
            {
                PopupNotification pop = new(e.Message);
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

        

        private void SaveFile_Chara(bool isQuickSave)
        {
            if (chritmFiles.Count > 0)
            {
                if (isQuickSave && !string.IsNullOrEmpty(Program.charaPath))
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
                    if (sfd.ShowDialog() == true)
                    {
                        Program.charaPath = sfd.FileName;
                        Program.IO.SaveChr(Program.charaPath, chritmFiles);
                    }
                }
            }
            else
            {
                PopupNotification pop = new(Properties.Resources.warn_error_save);
            }
        }

        private void DelCos(object sender, RoutedEventArgs e)
        {
            if (chritmFiles.Count > 0 && chritmFiles[CharaBox.SelectedIndex] != null)
            {
                try
                {
                    chritmFiles[CharaBox.SelectedIndex].costumes.Remove(CosListBox.SelectedItem as CharacterCostumeEntry);
                    CosListBox.Items.Refresh();
                    isCharacterUnsaved = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Program.NotiBox(Properties.Resources.warn_generic, Properties.Resources.cmn_error);
                }
            }
        }
        private void DelItem(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CharacterItemEntry> sel = new();
                foreach (CharacterItemEntry x in ItemDataGrid.SelectedItems)
                {
                    sel.Add(x);
                }
                foreach (CharacterItemEntry m in sel)
                {
                    chritmFiles[CharaBox.SelectedIndex].items.Remove(m);
                }
                ItemDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                PopupNotification pop = new(ex.Message);
            }
        }

        private void AddCos(object sender, RoutedEventArgs e)
        {
            if (chritmFiles.Count > 0)
            {
                chritmFiles[CharaBox.SelectedIndex].costumes.Add(new CharacterCostumeEntry());
                CosListBox.Items.Refresh();
                isCharacterUnsaved = true;
            }
        }
        private void EditCos(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chritmFiles.Count > 0 && CharaBox.SelectedIndex > -1)
                {
                    int index = chritmFiles[CharaBox.SelectedIndex].costumes.IndexOf(CosListBox.SelectedItem as CharacterCostumeEntry);
                    CharacterCostumeEntry cos = chritmFiles[CharaBox.SelectedIndex].costumes[index];
                    CosEdit cosEdit = new(cos);
                    cosEdit.ShowDialog();
                    CosListBox.Items.Refresh();
                    isCharacterUnsaved = true;
                }
            }
            catch (Exception ex)
            {
                PopupNotification pop = new(ex.Message);
            }
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            if (chritmFiles.Count > 0)
            {
                chritmFiles[CharaBox.SelectedIndex].items.Insert(ItemDataGrid.SelectedIndex + 1, new CharacterItemEntry());
                ItemDataGrid.Items.Refresh();
                isCharacterUnsaved = true;
            }
        }
        private void DataGrid_Drop_Chara(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            isCharacterUnsaved = false;
            ClearProcess_Chara();
            if (files[0].EndsWith("chritm_prop.farc"))
            {
                chritmFiles = Task.Run(() => Program.IO.OpenCharacterItemFile(files)).Result;
                foreach (CharacterItemFile x in chritmFiles)
                {
                    CharaBox.Items.Add(x.GetFullName());
                }
                CharaBox.SelectedIndex = 0;
            }
        }

        private void CharaBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chritmFiles != null && chritmFiles.Count > 0)
            {
                CosListBox.ItemsSource = chritmFiles[CharaBox.SelectedIndex].costumes;
                ItemDataGrid.ItemsSource = chritmFiles[CharaBox.SelectedIndex].items;
            }
            else
            {
                //throw new Exception("CharaBox Selection Changed error");
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            CharacterItemEntry item = ((FrameworkElement)sender).DataContext as CharacterItemEntry;
            if (chritmFiles.Count > 0 && item != null)
            {
                ItemEdit itemEdit = new(item);
                itemEdit.ShowDialog();
                isCharacterUnsaved = true;
            }
        }

        private void List_Click(object sender, RoutedEventArgs e)
        {
            CharacterCostumeEntry cos = ((FrameworkElement)sender).DataContext as CharacterCostumeEntry;
            if (chritmFiles.Count > 0 && cos != null)
            {
                CosEdit cosEdit = new(cos);
                cosEdit.ShowDialog();
                isCharacterUnsaved = true;
            }
        }

        private void ChangeTheme(string theme)
        {
            Settings.Default.themeSetting = theme;
            Settings.Default.Save();
            var app = (App)Application.Current;
            app.ChangeTheme(new Uri(("/Themes/" + theme + ".xaml"), UriKind.Relative));
        }

        private void Pink_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Pink");
            secretCount2 = 0;
            secretCount++;
            if (secretCount >= 10)
            {
                ChangeTheme("PSP");
            }
        }
        private void Blue_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Blue");
            secretCount2 = 0;
            secretCount = 0;
        }
        private void Light_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Light");
            secretCount2 = 0;
            secretCount = 0;
        }
        private void Default_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Default");
            secretCount = 0;
            secretCount2++;
            if(secretCount2 >= 10)
            {
                ChangeTheme("Real");
            }
        }
        private void Violet_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Violet");
            secretCount2 = 0;
            secretCount = 0;
        }

        private void Close_Modules(object sender, RoutedEventArgs e)　//Close
        {
            Modules.Clear();
            isModuleUnsaved = false;
            Program.modulePath = null;
        }
        private void Close_Customs(object sender, RoutedEventArgs e)　//Close
        {
            CustItems.Clear();
            isCustomizeUnsaved = false;
            Program.customPath = null;
        }
        private void Close_Character(object sender, RoutedEventArgs e)　//Close
        {
            ItemDataGrid.ItemsSource = null;
            CosListBox.ItemsSource = null;
            isCharacterUnsaved = false;
            Program.charaPath = null;
        }

        private void DMAUpdate_Click(object sender, RoutedEventArgs e) //DMA Update
        {
            if (Settings.Default.lastDmaUpdate.Date == DateTime.Today.Date)
            {
                ChoiceWindow choice = new(Properties.Resources.dma_update_already, Properties.Resources.cmn_no, Properties.Resources.cmn_yes);
                if (choice.isRightClicked)
                {
                    Program.IO.DMAUpdate();
                    LockControls();
                }
            }
            else
            {
                Program.IO.DMAUpdate();
                LockControls();
            }
        }

        private void LockControls()
        {
            Task.Run(() =>
            {
                while (Program.downloader.downloads.Count > 0)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        SelfUpdateButton.IsEnabled = false;
                        DmaDownloadButton.IsEnabled = false;
                    });
                    
                }
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    SelfUpdateButton.IsEnabled = true;
                    DmaDownloadButton.IsEnabled = true;
                });
            });
        }
        
        private async void DMACheck_Click(object sender, RoutedEventArgs e) //DMA Check
        {
            ChoiceWindow choice = new($"{Properties.Resources.dma_check_1}\n{Properties.Resources.dma_check_2}\n{Properties.Resources.cmn_sure}", Properties.Resources.cmn_no, Properties.Resources.cmn_yes);
            if (choice.isRightClicked)
            {
                UsedIdSet usedId = new();
                await Task.Run(() => usedId.ProcessLocalFiles());
                await usedId.GetUsedModIds(true);
                OpenFolderDialog ofd = new();
                while (string.IsNullOrEmpty(ofd.FolderName))
                {
                    ofd.ShowDialog();
                }
                usedId.WriteConflictsToFile(ofd.FolderName);
                PopupNotification popup = new("Conflicts saved to: " + ofd.FolderName);
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isModuleUnsaved ||isCustomizeUnsaved ||isCharacterUnsaved)
            {
                ChoiceWindow choice = new(Properties.Resources.notice_unsaved, Properties.Resources.cmn_no, Properties.Resources.cmn_yes);
                if (choice.isRightClicked)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void DataGrid_Modules_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isModuleUnsaved = true;
        }

        private void DataGrid_Customize_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isCustomizeUnsaved = true;
        }

        private void ItemDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isCharacterUnsaved = true;
        }

        private void CosListBox_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ClearProcess_Chara();
            isCharacterUnsaved = false;
            if (files[0].EndsWith("chritm_prop.farc"))
            {
                chritmFiles = Task.Run(() => Program.IO.OpenCharacterItemFile(files)).Result;
                foreach (CharacterItemFile x in chritmFiles)
                {
                    CharaBox.Items.Add(x.GetFullName());
                }
                CharaBox.SelectedIndex = 0;
            }
        }

        private void WizardAddButton_Click(object sender, RoutedEventArgs e)
        {
            WizardInfo modInfo = new();
            itemHost.Children.Add(modInfo);
        }

        private async void WizardCreateButton_Click(object sender, RoutedEventArgs e)
        {
            //Task.Run(() => Create());
            if(itemHost.Children.Count > 0)
            {
                while (string.IsNullOrEmpty(Settings.Default.gamePath) || !Settings.Default.gamePath.EndsWith("Hatsune Miku Project DIVA Mega Mix Plus"))
                {
                    OpenFolderDialog ofd = new()
                    {
                        Title = "Please select your game directory. (Hatsune Miku Project DIVA Mega Mix Plus)"
                    };
                    if (ofd.ShowDialog() == true && ofd.FolderName.EndsWith("Hatsune Miku Project DIVA Mega Mix Plus"))
                    {
                        Settings.Default.gamePath = ofd.FolderName;
                        Settings.Default.Save();
                    }
                }
                TextEntry tex = new(false, "Enter Mod Title");
                while (string.IsNullOrWhiteSpace(tex.Result))
                {
                    tex.ShowDialog();
                    if (string.IsNullOrWhiteSpace(tex.Result))
                    {
                        PopupNotification pop = new("Please enter a valid name.");
                    }
                }
                ChoiceWindow choice = new("Would you like to use DMA to increase compatibility with existing mods?", Properties.Resources.cmn_no, Properties.Resources.cmn_yes);
                await WizardCode.WizardCreate(itemHost, choice.isRightClicked, tex.Result);
            }
            else
            {
                PopupNotification pop = new(Properties.Resources.warn_no_items);
            }
        }
        private void WizardRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ChoiceWindow choice = new(Properties.Resources.warn_delete_all, Properties.Resources.cmn_no, Properties.Resources.cmn_yes);
            if (choice.isRightClicked)
            {
                itemHost.Children.Clear();
                PopupNotification popup = new("All current Wizard items have been deleted.");
            }
        }

        private void SelfUpdate_Click(object sender, RoutedEventArgs e)
        {
            Program.DownloadUpdate();
            LockControls();
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(LanguageComboBox.SelectedIndex > -1 && LanguageComboBox.SelectedIndex != Settings.Default.languageSetting)
            {
                switch (LanguageComboBox.SelectedIndex)
                {
                    case 0:
                        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
                        Settings.Default.languageSetting = LanguageComboBox.SelectedIndex;
                        Settings.Default.Save();
                        foreach (Window win in Application.Current.Windows)
                        {
                            if (win != this)
                            {
                                win.Close();
                            }
                        }
                        MainWindow main = new();
                        main.Show();
                        this.Close();
                        break;
                    case 1:
                        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja");
                        Settings.Default.languageSetting = LanguageComboBox.SelectedIndex;
                        Settings.Default.Save();
                        foreach (Window win in Application.Current.Windows)
                        {
                            if (win != this)
                            {
                                win.Close();
                            }
                        }
                        MainWindow main2 = new();
                        main2.Show();
                        this.Close();
                        break;
                    default:
                        break;
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Program.NotiBox("" + new Hyperlink(), "About");
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) //https://stackoverflow.com/questions/10238694/example-using-hyperlink-in-wpf tysm
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Miku_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Miku");
            secretCount2 = 0;
            secretCount = 0;
        }
        private void Rin_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Rin");
            secretCount2 = 0;
            secretCount = 0;
        }
        private void Len_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Len");
            secretCount2 = 0;
            secretCount = 0;
        }
        private void Luka_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Luka");
            secretCount2 = 0;
            secretCount = 0;
        }
        private void Kaito_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Kaito");
            secretCount2 = 0;
            secretCount = 0;
        }
        private void Meiko_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme("Meiko");
            secretCount2 = 0;
            secretCount = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e) //dma path
        {
            OpenFolderDialog ofd = new();
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FolderName.Contains("Hatsune Miku Project DIVA Mega Mix Plus"))
                {
                    Settings.Default.gamePath = ofd.FolderName;
                    Settings.Default.Save();
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) //game path
        {
            OpenFolderDialog ofd = new();
            if (ofd.ShowDialog() == true)
            {
                Settings.Default.dmaPath = ofd.FolderName;
                Settings.Default.Save();
            }
        }

        private void AlignmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetAlignment(AlignmentComboBox.SelectedIndex);
        }

        private void SetAlignment(int align)
        {
            var app = (App)Application.Current;
            switch (align)
            {
                case 0:
                    Debug.WriteLine("honestly");
                    app.ChangeAlignment(HorizontalAlignment.Center);
                    break;
                case 1:
                    app.ChangeAlignment(HorizontalAlignment.Left);
                    break;
                case 2:
                    app.ChangeAlignment(HorizontalAlignment.Right);
                    break;
                default:
                    break;
            }
            Settings.Default.alignmentSetting = align;
            Settings.Default.Save();
            if(AlignmentComboBox.SelectedIndex != align)
            {
                AlignmentComboBox.SelectedIndex = align;
            }
        }

        private void AutoCheckComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(AutoCheckComboBox.SelectedIndex > -1)
            {
                Settings.Default.autoCheckSetting = Convert.ToBoolean(AutoCheckComboBox.SelectedIndex);
                Settings.Default.Save();
            }
        }
    }
}
