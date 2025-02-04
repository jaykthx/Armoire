using System;
using System.Windows;

namespace Armoire
{
    public partial class App : Application
    {
        App()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja-JP");
        }
        public ResourceDictionary ThemeDictionary // thank you stackoverflow ppl for this
        {
            // You could probably get it via its name with some query logic as well.
            get { return Resources.MergedDictionaries[0]; }
        }

        public void ChangeTheme(Uri uri)
        {
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/Theme.xaml", UriKind.Relative) });
        }
    }
}
