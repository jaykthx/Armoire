using System;
using System.Diagnostics;
using System.Windows;

namespace Armoire
{
    public partial class App : Application
    {
        public static bool isEgg = false;
        App()
        {
        }
        public ResourceDictionary ThemeDictionary // thank you stackoverflow ppl for this
        {
            get { return Resources.MergedDictionaries[0]; }
        }

        public void ChangeTheme(Uri uri)
        {
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/Theme.xaml", UriKind.Relative) });
        }
        public void ChangeAlignment(HorizontalAlignment align)
        {
            this.Resources["HorizAlign"] = align;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            foreach(var arg in e.Args)
            {
                if(arg == "egg" || arg == "-egg")
                {
                    isEgg = true;

                }
            }
        }
    }
}
