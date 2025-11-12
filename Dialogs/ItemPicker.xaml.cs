using Armoire.Properties;
using Microsoft.Win32;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.CriMw;
using MikuMikuLibrary.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for ItemPicker.xaml
    /// </summary>
    public partial class ItemPicker : Window
    {
        public List<CharacterItemEntry> selectedItems = new List<CharacterItemEntry>();
        public string selChara;
        public ItemPicker(string chara)
        {
            InitializeComponent();
            selChara = chara;
            if(Settings.Default.gamePath == null || !Settings.Default.gamePath.Contains("Project DIVA"))
            {
                OpenFolderDialog ofd = new();
                if(ofd.ShowDialog() == true)
                {
                    if(ofd.FolderName.Contains("Hatsune Miku Project DIVA Mega Mix Plus"))
                    {
                        Settings.Default.gamePath = ofd.FolderName;
                        Settings.Default.Save();
                        itemsBox.ItemsSource = Task.Run(() => getExistingItems(Settings.Default.gamePath)).Result;
                    }
                }
            }
            else
            {
                itemsBox.ItemsSource = Task.Run(() => getExistingItems(Settings.Default.gamePath)).Result;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(CharacterItemEntry x in itemsBox.SelectedItems)
            {
                selectedItems.Add(x);
            }
            this.Close();
        }

        private async Task<List<CharacterItemEntry>> getExistingItems(string gamePath)
        {
            CpkArchive cpk = new CpkArchive();
            List<CharacterItemEntry> items = new();
            cpk.Load(gamePath+"/diva_main.cpk");
            FarcArchive cust = BinaryFile.Load<FarcArchive>(cpk.Open("rom_switch/rom/chritm_prop.farc", EntryStreamMode.MemoryStream));
            ObservableCollection<CharacterItemFile> charas = await Program.IO.ReadCharaFile(cust);
            foreach(CharacterItemFile x in charas)
            {
                Debug.WriteLine($"Short name: {x.GetShortName()} selChara: {selChara}");
                if (x.GetShortName() == selChara)
                {
                    Debug.WriteLine("Name matched!");
                    foreach (CharacterItemEntry i in x.items)
                    {
                        items.Add(i);
                    }
                }
            }
            return items;
        }
    }
}
