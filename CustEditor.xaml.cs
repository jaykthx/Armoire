using Armoire.Dialogs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Armoire
{
    public partial class CustEditor : Window
    {
        public CustEditor()
        {
            InitializeComponent();
            charaBox.ItemsSource = charas;
            partBox.ItemsSource = partsList;
            DataGrid.ItemsSource = CustItems;
            DataGrid.Items.IsLiveSorting = true;
        }

        public static ObservableCollection<cstm_item> CustItems = new ObservableCollection<cstm_item>();
        public List<string> charas = new List<string> { "MIKU", "RIN", "LEN", "LUKA", "KAITO", "MEIKO", "NERU", "HAKU", "SAKINE", "TETO", "ALL" };
        public List<string> partsList = new List<string> { "KAMI", "FACE", "NECK", "ZUJO", "BACK" };

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].EndsWith(".farc"))
            {
                Program.modulePath = files[0];
                CustItems = Program.readCustomFile(Program.customPath);
                DataGrid.ItemsSource = CustItems;
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

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
        private void SaveFile(bool isQuickSave)
        {
            if (CustItems.Count > 0)
            {
                if (isQuickSave)
                {
                    Program.saveFile<cstm_item>(Program.customPath, CustItems);
                }
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog() { Filter = "FARC files (*.farc)|*.farc" };
                    sfd.FileName = "mod_gm_customize_item_tbl.farc";
                    if (sfd.ShowDialog() == true)
                    {
                        Program.customPath = sfd.FileName;
                        Program.saveFile<cstm_item>(sfd.FileName, CustItems);
                    }
                }
            }
            else { return; }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (Program.customPath != null)
            {
                SaveFile(true);
            }
            else
            {
                Program.NotiBox("You must open a file before attempting to save it.", "Error");
            }
        }
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(false);
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
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Supported files|*.csv;*customize_item_tbl.farc|Customize Item Table files|*customize_item_tbl.farc|Armoire-exported CSV files|*.csv|All files (*.*)|*.*" };
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileName != null && ofd.FileName.EndsWith(".farc"))
                {
                    Program.customPath = ofd.FileName;
                    CustItems = Program.readCustomFile(Program.customPath);
                }
                else if (ofd.FileName != null && ofd.FileName.EndsWith(".csv"))
                {
                    string[] split = ofd.FileName.Split('\\');
                    string newFileNameLocation = ofd.FileName.Remove((ofd.FileName.Length - split[split.Length - 1].Length), split[split.Length - 1].Length);
                    newFileNameLocation = newFileNameLocation + "mod_gm_customize_item_tbl.farc";
                    Program.customPath = newFileNameLocation;
                    CustItems = Program.readCustomFileCSV(ofd.FileName);
                }
                else { return; }
            }
            DataGrid.ItemsSource = CustItems;
        }

        private void DeleteEntry(object sender, RoutedEventArgs e)
        {
            if (CustItems.Count > 1)
            {
                List<cstm_item> sel = new List<cstm_item>();

                foreach (cstm_item x in DataGrid.SelectedItems)
                {
                    sel.Add(x);
                }
                foreach (cstm_item m in sel)
                {
                    CustItems.Remove(m);

                }
                //Modules.RemoveAt(DataGrid.SelectedIndexes);
            }
            else
            {
                Program.NotiBox("An error occured whilst deleting your items.", "Error");
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            CustItems.Insert(DataGrid.SelectedIndex + 1, GetDummy());
        }
        private cstm_item GetDummy()
        {
            cstm_item dummyModule = new cstm_item();
            dummyModule.name = "ダミー";
            dummyModule.chara = "ALL";
            dummyModule.id = 999;
            dummyModule.bind_module = -1;
            dummyModule.parts = "ZUJO";
            dummyModule.ng = false;
            dummyModule.shop_price = "750";
            return dummyModule;
        }
        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFile();
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Program.customPath != null)
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
        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            PreviewWindow prev = new PreviewWindow(false);
            prev.ShowDialog();
        }
        public sealed class cstm_item_map : ClassMap<cstm_item>
        {
            public cstm_item_map()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.entry).Ignore();
            }
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "gm_customize_item_tbl.csv";
            sfd.Filter = "CSV Files|*.csv";
            if (sfd.ShowDialog() == true)
            {
                using (TextWriter writer = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                {
                    var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csv.Context.RegisterClassMap<cstm_item_map>();
                    csv.WriteRecords(CustItems); // where values implements IEnumerable
                }
            }
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
    }
}
