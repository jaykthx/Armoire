using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Textures;
using MikuMikuLibrary.Textures.Processing;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for TexturePicker.xaml
    /// </summary>
    public partial class TexturePicker : Window
    {
        public itemEntry temp_item;
        string selectedFileAutoGen = null;
        string selChara;
        List<int> textureList = new();
        public class ListBoxItemImage
        {
            public string Name { get; set; }
            public BitmapImage Image { get; set; }
        }
        List<ListBoxItemImage> listItems = new();
        private void mainProcess(string farc_path)
        {
            temp_item.dataSetTexes = new System.Collections.ObjectModel.ObservableCollection<dataSetTex>();
            FarcArchive farc = BinaryFile.Load<FarcArchive>(farc_path);
            foreach (string fileName in farc.FileNames)
            {
                if (fileName.Contains("_tex"))
                {
                    var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                    TextureSet texset = new();
                    texset.Load(source, true);
                    int count = 0;
                    foreach (Texture tex in texset.Textures)
                    {
                        Bitmap bmp;
                        textureList.Add(count);
                        bmp = TextureDecoder.DecodeToBitmap(tex);
                        BitmapImage bmpImg = Program.ToBitmapImage(bmp);
                        listItems.Add(new ListBoxItemImage() { Name = count.ToString(), Image = bmpImg });
                        count++;
                    }
                    eyebrowCombo.ItemsSource = listItems;
                    eyelashCombo.ItemsSource = listItems;
                    eyeLCombo.ItemsSource = listItems;
                    eyeRCombo.ItemsSource = listItems;
                    faceCombo.ItemsSource = listItems;
                }
            }
        }
        private string[] getStrings(string chara)
        {
            string first = "F_DIVA_" + chara;
            return new string[5] { first+"000_EYEBROW", first+"000_EYELASHES", first+ "000_EYE_LEFT", first+ "000_EYE_RIGHT", first+ "000_FACE" };
        }
        public TexturePicker(itemEntry item, string chara) // other
        {
            InitializeComponent();
            selChara = chara;
            temp_item = item;
            OpenFileDialog ofd = new();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.FileName.EndsWith(".farc"))
                {
                    selectedFileAutoGen = Path.GetFileNameWithoutExtension(ofd.FileName);
                    mainProcess(ofd.FileName);
                }
            }
            else { this.Close(); }
        }
        public TexturePicker(itemEntry item, string file_path, string chara) // wizard moment
        {
            InitializeComponent();
            selChara = chara;
            temp_item = item;
            if (file_path.EndsWith(".farc"))
            {
                selectedFileAutoGen = Path.GetFileNameWithoutExtension(file_path);
                mainProcess(file_path);
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_file_error, Properties.Resources.cmn_error);
                this.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // Apply changes
        {
            string[] orgs = getStrings(selChara);
            string mid = selectedFileAutoGen.ToUpper() + "_AUTO_TEXTURE_";
            if (eyeLCombo.SelectedIndex > -1)
            {
                dataSetTex eye_l = new();
                eye_l.org = orgs[2];
                eye_l.chg = mid + (eyeLCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTexes.Add(eye_l);
            }
            if (eyeRCombo.SelectedIndex > -1)
            {
                dataSetTex eye_r = new();
                eye_r.org = orgs[3];
                eye_r.chg = mid + (eyeRCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTexes.Add(eye_r);
            }
            if (eyelashCombo.SelectedIndex > -1)
            {
                dataSetTex eyelash = new();
                eyelash.org = orgs[1];
                eyelash.chg = mid + (eyelashCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTexes.Add(eyelash);
            }
            if (eyebrowCombo.SelectedIndex > -1)
            {
                dataSetTex eyebrow = new();
                eyebrow.org = orgs[0];
                eyebrow.chg = mid + (eyebrowCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTexes.Add(eyebrow);
            }
            if (faceCombo.SelectedIndex > -1)
            {
                dataSetTex face = new();
                face.org = orgs[4];
                face.chg = mid + (faceCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTexes.Add(face);
            }
            this.Close();
        }
    }
}
