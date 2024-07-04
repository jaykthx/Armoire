using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Textures;
using MikuMikuLibrary.Textures.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for TexturePicker.xaml
    /// </summary>
    public partial class TexturePicker : Window
    {
        string[] short_charas = new string[] { "MIK", "RIN", "LEN", "LUK", "KAI", "MEI", "HAK", "NER", "SAK", "TET" };
        public itemEntry temp_item;
        string selectedFileAutoGen = null;
        List<int> textureList = new List<int>();
        public class ListBoxItemImage
        {
            public string Name { get; set; }
            public BitmapImage Image { get; set; }
        }
        List<ListBoxItemImage> listItems = new List<ListBoxItemImage>();
        private void mainProcess(string farc_path)
        {
            FarcArchive farc = BinaryFile.Load<FarcArchive>(farc_path);
            foreach (string fileName in farc.FileNames)
            {
                if (fileName.Contains("_tex"))
                {
                    var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                    TextureSet texset = new TextureSet();
                    texset.Load(source);
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
                    imageListBox.ItemsSource = listItems;
                    eyebrowCombo.ItemsSource = textureList;
                    eyelashCombo.ItemsSource = textureList;
                    eyeLCombo.ItemsSource = textureList;
                    eyeRCombo.ItemsSource = textureList;
                    faceCombo.ItemsSource = textureList;
                }
            }
        }
        private string[] getStrings(string chara)
        {
            string first = "F_DIVA_" + chara;
            return new string[5] { first+"000_EYEBROW", first+"000_EYELASHES", first+ "000_EYE_LEFT", first+ "000_EYE_RIGHT", first+ "000_FACE" };
        }
        public TexturePicker(itemEntry item) // other
        {
            InitializeComponent();
            charaBox.ItemsSource = short_charas;
            temp_item = item;
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.FileName.EndsWith(".farc"))
                {
                    mainProcess(ofd.FileName);
                }
            }
        }
        public TexturePicker(itemEntry item, string file_path) // wizard moment
        {
            InitializeComponent();
            charaBox.ItemsSource = short_charas;
            temp_item = item;
            if (file_path.EndsWith(".farc"))
            {
                selectedFileAutoGen = Path.GetFileNameWithoutExtension(file_path);
                mainProcess(file_path);
            }
            else
            {
                Program.NotiBox("Something is wrong with the file you selected for this item.\nIt doesn't look like a .farc format file.", Properties.Resources.cmn_error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // Apply changes
        {
            string[] orgs = getStrings(charaBox.SelectedValue as string);
            string mid = selectedFileAutoGen.ToUpper() + "_AUTO_TEXTURE_";
            if (eyeLCombo.SelectedIndex > -1)
            {
                dataSetTex eye_l = new dataSetTex();
                eye_l.org = orgs[2];
                eye_l.chg = mid + eyeLCombo.SelectedValue as string;
                temp_item.dataSetTexes.Add(eye_l);
            }
            if (eyeRCombo.SelectedIndex > -1)
            {
                dataSetTex eye_r = new dataSetTex();
                eye_r.org = orgs[3];
                eye_r.chg = mid + eyeRCombo.SelectedValue as string;
                temp_item.dataSetTexes.Add(eye_r);
            }
            if (eyelashCombo.SelectedIndex > -1)
            {
                dataSetTex eyelash = new dataSetTex();
                eyelash.org = orgs[1];
                eyelash.chg = mid + eyelashCombo.SelectedValue as string;
                temp_item.dataSetTexes.Add(eyelash);
            }
            if (eyebrowCombo.SelectedIndex > -1)
            {
                dataSetTex eyebrow = new dataSetTex();
                eyebrow.org = orgs[0];
                eyebrow.chg = mid + eyebrowCombo.SelectedValue as string;
                temp_item.dataSetTexes.Add(eyebrow);
            }
            if (faceCombo.SelectedIndex > -1)
            {
                dataSetTex face = new dataSetTex();
                face.org = orgs[4];
                face.chg = mid + faceCombo.SelectedValue as string;
                temp_item.dataSetTexes.Add(face);
            }
            this.Close();
        }
    }
}
