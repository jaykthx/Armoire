using Armoire.Dialogs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Armoire
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
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
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileName != null && ofd.FileName.EndsWith(".farc"))
                {
                    Program.modulePath = ofd.FileName;
                    var farc = BinaryFile.Load<FarcArchive>(Program.modulePath);
                    Modules = Program.IO.ReadModuleFile(farc);
                }
                else if(ofd.FileName != null && ofd.FileName.EndsWith(".csv"))
                {
                    string[] split = ofd.FileName.Split('\\');
                    string newFileNameLocation = ofd.FileName.Remove((ofd.FileName.Length - split[split.Length-1].Length),split[split.Length-1].Length);
                    newFileNameLocation += "mod_gm_module_tbl.farc";
                    Program.modulePath = newFileNameLocation;
                    Modules = Program.IO.ReadModuleFileCSV(ofd.FileName);
                }
                else { return; }
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
                    if (sfd.ShowDialog() == true)
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

        private void DataGrid_Drop(object sender, DragEventArgs e) // Loads drag and dropped items
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
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
            if (sfd.ShowDialog() == true)
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
        private void Wizard_Click(object sender, RoutedEventArgs e)
        {
            ModuleWizard wiz = new ModuleWizard();
            wiz.ShowDialog();
        }
    }
}
