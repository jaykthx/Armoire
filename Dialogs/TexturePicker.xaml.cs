using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Textures;
using MikuMikuLibrary.Textures.Processing;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for TexturePicker.xaml
    /// </summary>
    public partial class TexturePicker : Window
    {
        public CharacterItemEntry temp_item;
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
            temp_item.dataSetTex = new System.Collections.ObjectModel.ObservableCollection<DataSetTex>();
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
            string first = "F_DIVA_" + chara.Remove(3);
            return new string[5] { first+"000_EYEBROW", first+"000_EYELASHES", first+ "000_EYE_LEFT", first+ "000_EYE_RIGHT", first+ "000_FACE" };
        }

        public TexturePicker(WizardObject wizObj, string chara) // wizard moment
        {
            InitializeComponent();
            selChara = chara;
            temp_item = wizObj.item;
            if (wizObj.objectFilePath.EndsWith(".farc"))
            {
                mainProcess(wizObj.objectFilePath);
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
            string mid = "ARMOIREREPLACE_AUTO_TEXTURE_";
            if (eyeLCombo.SelectedIndex > -1)
            {
                DataSetTex eye_l = new();
                eye_l.org = orgs[2];
                eye_l.chg = mid + (eyeLCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTex.Add(eye_l);
            }
            if (eyeRCombo.SelectedIndex > -1)
            {
                DataSetTex eye_r = new();
                eye_r.org = orgs[3];
                eye_r.chg = mid + (eyeRCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTex.Add(eye_r);
            }
            if (eyelashCombo.SelectedIndex > -1)
            {
                DataSetTex eyelash = new();
                eyelash.org = orgs[1];
                eyelash.chg = mid + (eyelashCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTex.Add(eyelash);
            }
            if (eyebrowCombo.SelectedIndex > -1)
            {
                DataSetTex eyebrow = new();
                eyebrow.org = orgs[0];
                eyebrow.chg = mid + (eyebrowCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTex.Add(eyebrow);
            }
            if (faceCombo.SelectedIndex > -1)
            {
                DataSetTex face = new();
                face.org = orgs[4];
                face.chg = mid + (faceCombo.SelectedValue as ListBoxItemImage).Name;
                temp_item.dataSetTex.Add(face);
            }
            this.Close();
        }
    }
}
