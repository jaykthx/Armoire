using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Armoire
{
    /// <summary>
    /// Interaction logic for EntryDisplay.xaml
    /// </summary>
    public partial class EntryDisplay : UserControl
    {
        public EntryDisplay(BitmapImage image, string main, int sub, string price, Attr attr)
        {
            BitmapImage fsSource = Program.ToBitmapImage(Properties.Resources.item_fs);
            BitmapImage ctSource = Program.ToBitmapImage(Properties.Resources.item_ct);
            BitmapImage nSource = Program.ToBitmapImage(Properties.Resources.item_n);
            InitializeComponent();
            switch (attr)
            {
                case Attr.Default_FS:
                    highlightBox.Source = fsSource;
                    break;
                case Attr.NoSwap_FS:
                    highlightBox.Source = fsSource;
                    break;
                case Attr.Swimsuit_FS:
                    highlightBox.Source = fsSource;
                    break;
                case Attr.Default_CT:
                    highlightBox.Source = ctSource;
                    break;
                case Attr.NoSwap_CT:
                    highlightBox.Source = ctSource;
                    break;
                case Attr.Swimsuit_CT:
                    highlightBox.Source = ctSource;
                    break;
                case Attr.Default_N:
                    break;
                case Attr.NoSwap_N:
                    break;
                case Attr.Swimsuit_N:
                    break;
                case Attr.Default_PL:
                    highlightBox.Source = nSource;
                    break;
                case Attr.NoSwap_PL:
                    highlightBox.Source = nSource;
                    break;
                case Attr.Swimsuit_PL:
                    highlightBox.Source = nSource;
                    break;
                default:
                    break;
            }
            imageBox.Source = image;
            mainText.Text = main;
            subText.Text = sub.ToString() + " - " + price+"vp";
        }
        public EntryDisplay(BitmapImage image, string main, int sub, string price, int bind_module)
        {
            InitializeComponent();
            imageBox.Source = image;
            mainText.Text = main;
            if(bind_module == -1)
            {
                subText.Text = sub.ToString() + " - " + price + "vp";
            }
            else
            {
                subText.Text = sub.ToString() + " - " + price + "vp" + " - Bound: " + bind_module.ToString();
            }
        }
    }
}
