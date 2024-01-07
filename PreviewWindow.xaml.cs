using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.CriMw;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Sprites;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {

        public string imagePath = null;
        public bool moduleFlag;
        public PreviewWindow(bool isModule)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select your game directory.";
            fbd.SelectedPath = Properties.Settings.Default.gamePath;
            moduleFlag = isModule;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imagePath= fbd.SelectedPath;
                Properties.Settings.Default.gamePath = fbd.SelectedPath;
                Properties.Settings.Default.Save();
                NotiBox noti = new NotiBox("This process might take a short while. Please be patient.\nA new window will open once the process is complete.", "Notice");
                noti.Show();
                if (!moduleFlag)
                {
                    custExp.Visibility = Visibility.Visible;
                }
                populateNames();
                noti.Close();
            }
            else
            {
                Program.NotiBox("You have opted not to display images.\nNo module preview images will be shown.", "Notice");
                if (!moduleFlag)
                {
                    custExp.Visibility = Visibility.Visible;
                }
                populateNames();
            }
        }

        private string getIdString(int id)
        {
            string final = id.ToString();
            while(final.Length < 3)
            {
                StringBuilder sb = new StringBuilder(final);
                sb.Insert(0, "0");
                final = sb.ToString();
            }
            return final;
        }

        private void populateNames()
        {
            mikuPanel.Children.Clear();
            rinPanel.Children.Clear();
            lenPanel.Children.Clear();
            lukaPanel.Children.Clear();
            kaitoPanel.Children.Clear();
            meikoPanel.Children.Clear();
            neruPanel.Children.Clear();
            sakinePanel.Children.Clear();
            hakuPanel.Children.Clear();
            tetoPanel.Children.Clear();
            BitmapImage img = new BitmapImage();
            List<string> dirs = new List<string>();
            CpkArchive cpk = new CpkArchive();
            if (imagePath != null)
            {
                dirs = new List<string>(Directory.EnumerateDirectories(imagePath + "\\mods", "2d", SearchOption.AllDirectories));
                cpk = BinaryFile.Load<CpkArchive>(imagePath + "\\diva_main.cpk");
            }
            if (moduleFlag)
            {
                foreach (module x in MainWindow.Modules.OrderBy(x => x.sort_index))
                {
                    bool isFound = false;
                    if (isFound) { continue; }
                    if (dirs != null)
                    {
                        string fileSearch = "spr_sel_md" + getIdString(x.id) + "cmn";
                        img = new BitmapImage();
                        foreach (string dir in dirs)
                        {
                            if (isFound)
                            {
                                break;
                            }
                            foreach (string file in Directory.EnumerateFiles(dir, fileSearch + ".farc"))
                            {
                                FarcArchive farc = BinaryFile.Load<FarcArchive>(file);
                                EntryStream source = farc.Open(fileSearch + ".bin", EntryStreamMode.MemoryStream);
                                SpriteSet sprite = BinaryFile.Load<SpriteSet>(source);
                                Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
                                img = ToBitmapImage(cropSprite);
                                cropSprite.Dispose();
                                sprite.Dispose();
                                source.Dispose();
                                farc.Dispose();
                                isFound = true;
                                break;
                            }
                        }
                        foreach (string file in cpk.FileNames)
                        {
                            if (isFound)
                            {
                                break;
                            }
                            if (file == "rom_switch/rom/2d/" + fileSearch + ".farc")
                            {
                                FarcArchive farc = BinaryFile.Load<FarcArchive>(cpk.Open(file, EntryStreamMode.MemoryStream));
                                EntryStream source = farc.Open(fileSearch + ".bin", EntryStreamMode.MemoryStream);
                                SpriteSet sprite = BinaryFile.Load<SpriteSet>(source);
                                Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
                                img = ToBitmapImage(cropSprite);
                                cropSprite.Dispose();
                                sprite.Dispose();
                                source.Dispose();
                                farc.Dispose();
                                isFound = true;
                                break;
                            }
                        }
                    }
                    EntryDisplay chara = new EntryDisplay(img, x.name, x.sort_index, x.shop_price, x.attr);
                    switch (x.chara)
                    {
                        case "MIKU":
                            mikuPanel.Children.Add(chara);
                            break;
                        case "RIN":
                            rinPanel.Children.Add(chara);
                            break;
                        case "LEN":
                            lenPanel.Children.Add(chara);
                            break;
                        case "LUKA":
                            lukaPanel.Children.Add(chara);
                            break;
                        case "KAITO":
                            kaitoPanel.Children.Add(chara);
                            break;
                        case "MEIKO":
                            meikoPanel.Children.Add(chara);
                            break;
                        case "SAKINE":
                            sakinePanel.Children.Add(chara);
                            break;
                        case "NERU":
                            neruPanel.Children.Add(chara);
                            break;
                        case "HAKU":
                            hakuPanel.Children.Add(chara);
                            break;
                        case "TETO":
                            tetoPanel.Children.Add(chara);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                foreach (cstm_item x in CustEditor.CustItems.OrderBy(x => x.sort_index))
                {
                    bool isFound = false;
                    if (isFound) { continue; }
                    if (dirs != null)
                    {
                        string fileSearch = "spr_cmnitm_thmb" + getIdString(x.id); //spr_cmnitm_thmb1210
                        img = new BitmapImage();
                        foreach (string dir in dirs)
                        {
                            if (isFound)
                            {
                                break;
                            }
                            foreach (string file in Directory.EnumerateFiles(dir, fileSearch + ".farc"))
                            {
                                FarcArchive farc = BinaryFile.Load<FarcArchive>(file);
                                EntryStream source = farc.Open(fileSearch + ".bin", EntryStreamMode.MemoryStream);
                                SpriteSet sprite = BinaryFile.Load<SpriteSet>(source);
                                Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
                                img = ToBitmapImage(cropSprite);
                                cropSprite.Dispose();
                                sprite.Dispose();
                                source.Dispose();
                                farc.Dispose();
                                isFound = true;
                                break;
                            }
                        }
                        foreach (string file in cpk.FileNames)
                        {
                            if (isFound)
                            {
                                break;
                            }
                            if (file == "rom_switch/rom/2d/" + fileSearch + ".farc")
                            {
                                FarcArchive farc = BinaryFile.Load<FarcArchive>(cpk.Open(file, EntryStreamMode.MemoryStream));
                                EntryStream source = farc.Open(fileSearch + ".bin", EntryStreamMode.MemoryStream);
                                SpriteSet sprite = BinaryFile.Load<SpriteSet>(source);
                                Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
                                img = ToBitmapImage(cropSprite);
                                cropSprite.Dispose();
                                sprite.Dispose();
                                source.Dispose();
                                farc.Dispose();
                                isFound = true;
                                break;
                            }
                        }
                    }
                    EntryDisplay chara = new EntryDisplay(img, x.name, x.sort_index, x.shop_price, x.bind_module);
                    switch (x.chara)
                    {
                        case "MIKU":
                            mikuPanel.Children.Add(chara);
                            break;
                        case "RIN":
                            rinPanel.Children.Add(chara);
                            break;
                        case "LEN":
                            lenPanel.Children.Add(chara);
                            break;
                        case "LUKA":
                            lukaPanel.Children.Add(chara);
                            break;
                        case "KAITO":
                            kaitoPanel.Children.Add(chara);
                            break;
                        case "MEIKO":
                            meikoPanel.Children.Add(chara);
                            break;
                        case "SAKINE":
                            sakinePanel.Children.Add(chara);
                            break;
                        case "NERU":
                            neruPanel.Children.Add(chara);
                            break;
                        case "HAKU":
                            hakuPanel.Children.Add(chara);
                            break;
                        case "TETO":
                            tetoPanel.Children.Add(chara);
                            break;
                        case "ALL":
                            custPanel.Children.Add(chara);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public BitmapImage ToBitmapImage(Bitmap bitmap) // Credit to the uploader of this code on StackOverflow who i forgot to note :(
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }
}
