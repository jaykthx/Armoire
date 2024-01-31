using Armoire.Dialogs;
using CsvHelper;
using CsvHelper.Configuration;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Armoire
{
    public partial class MainWindow : Window
    {
        public static ObservableCollection<module> Modules = new ObservableCollection<module>();
        public MainWindow()
        {
            InitializeComponent();
            charaBox.ItemsSource = Program.charas;
            attrBox.ItemsSource = Enum.GetValues(typeof(Attr)).Cast<Attr>();
            DataGrid.ItemsSource = Modules;
            DataGrid.Items.IsLiveSorting = true;
        }
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile();
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Program.modulePath != null)
            {
                SaveFile(true);
            }
            else
            {
                Program.NotiBox("You must open a file before attempting to save it.", "Error");
            }
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile(false);
        }
        private void Open_CustEditor(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Windows.OfType<CustEditor>().Any())
            {
                CustEditor win = new CustEditor();
                win.Show();
            }
            else
            {
                Program.NotiBox("This window is already open.", "Friendly Reminder");
            }
        }
        private void Open_CharaEditor(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Windows.OfType<CharaEditor>().Any())
            {
                CharaEditor win = new CharaEditor();
                win.Show();
            }
            else
            {
                Program.NotiBox("This window is already open.", "Friendly Reminder");
            }
        }
        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Supported files|*.csv;*_module_tbl.farc|Module Table Files|*_module_tbl.farc|Armoire-exported CSV files|*.csv|All files (*.*)|*.*" };
            ofd.Multiselect= true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Modules.Clear();
                ObservableCollection<module> tempModules = new ObservableCollection<module>();
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
                    else if (file.EndsWith(".csv"))
                    {
                        string[] split = file.Split('\\');
                        Program.modulePath = ofd.FileNames[0];
                        string newFileNameLocation = ofd.FileNames[0].Remove((ofd.FileName.Length - split[split.Length - 1].Length), split[split.Length - 1].Length);
                        newFileNameLocation += "mod_gm_module_tbl.farc";
                        tempModules = Program.IO.ReadModuleFileCSV(file);
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
            DataGrid.ItemsSource = Modules;
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
                    SaveFileDialog sfd = new SaveFileDialog() { Filter = "FARC files (*.farc)|*.farc" };
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
                List<module> sel = new List<module>();
                foreach (module x in DataGrid.SelectedItems)
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
                Program.NotiBox("An error occured whilst deleting your items.", "Error");
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) // Add at position
        {
            Modules.Insert(DataGrid.SelectedIndex + 1, GetDummy());
        }
        private void AddButton2_Click(object sender, RoutedEventArgs e) // Add to end of list
        {
            Modules.Add(GetDummy());
        }
        private module GetDummy() // Dummy module that can be added to the list
        {
            module dummyModule = new module();
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
                DataGrid.ItemsSource = Modules;
            }
            else if (files[0].EndsWith(".csv"))
            {
                Modules = Program.IO.ReadModuleFileCSV(files[0]);
                DataGrid.ItemsSource = Modules;
                string[] split = files[0].Split('\\');
                string newFileNameLocation = files[0].Remove((files[0].Length - split[split.Length - 1].Length), split[split.Length - 1].Length);
                newFileNameLocation += "mod_gm_module_tbl.farc";
                Program.modulePath = newFileNameLocation;
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

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "gm_module_tbl.csv";
            sfd.Filter = "CSV Files|*.csv";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (TextWriter writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                {
                    var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csv.Context.RegisterClassMap<moduleMap>();
                    csv.WriteRecords(Modules); // where values implements IEnumerable
                }
            }
        }
        public sealed class moduleMap : ClassMap<module>
        {
            public moduleMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.entry).Ignore();
            }
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            PreviewWindow prev = new PreviewWindow(true);
            prev.ShowDialog();
        }
        private void Open_SprEditor(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Windows.OfType<SprEditMain>().Any())
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
        private void Wizard_Click(object sender, RoutedEventArgs e)
        {
            ModuleWizard wiz = new ModuleWizard();
            wiz.Show();
        }
        private void Test_Click(object sender, RoutedEventArgs e)
        {

            // Testing DB Cleaner
            OpenFileDialog ofd = new OpenFileDialog();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            SaveFileDialog sfd = new SaveFileDialog();
            fbd.Description = "Pick the mod folder.";
            List<string> objsetFarcs = new List<string>();
            List<uint> textureIDs = new List<uint>();
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
                ObjectDatabase new_obj_db = new ObjectDatabase();
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
                TextureDatabase new_tex_db = new TextureDatabase();
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
    }
}
