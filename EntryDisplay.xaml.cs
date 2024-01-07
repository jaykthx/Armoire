using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Armoire
{
    /// <summary>
    /// Interaction logic for EntryDisplay.xaml
    /// </summary>
    public partial class EntryDisplay : UserControl
    {
        public EntryDisplay(BitmapImage image, string main, int sub, string price, Attr attr)
        {
            var fsSource = new Uri("/Resources/item_fs.png", UriKind.Relative);
            var ctSource = new Uri("/Resources/item_ct.png", UriKind.Relative);
            var nSource = new Uri("/Resources/item_n.png", UriKind.Relative);
            InitializeComponent();
            switch (attr)
            {
                case Attr.Default_FS:
                    highlightBox.Source = new BitmapImage(fsSource);
                    break;
                case Attr.NoSwap_FS:
                    highlightBox.Source = new BitmapImage(fsSource);
                    break;
                case Attr.Swimsuit_FS:
                    highlightBox.Source = new BitmapImage(fsSource);
                    break;
                case Attr.Default_CT:
                    highlightBox.Source = new BitmapImage(ctSource);
                    break;
                case Attr.NoSwap_CT:
                    highlightBox.Source = new BitmapImage(ctSource);
                    break;
                case Attr.Swimsuit_CT:
                    highlightBox.Source = new BitmapImage(ctSource);
                    break;
                case Attr.Default_N:
                    break;
                case Attr.NoSwap_N:
                    break;
                case Attr.Swimsuit_N:
                    break;
                case Attr.Default_PL:
                    highlightBox.Source = new BitmapImage(nSource);
                    break;
                case Attr.NoSwap_PL:
                    highlightBox.Source = new BitmapImage(nSource);
                    break;
                case Attr.Swimsuit_PL:
                    highlightBox.Source = new BitmapImage(nSource);
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
