﻿using Armoire.Dialogs;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Armoire
{
    /// <summary>
    /// Interaction logic for CustomInfo.xaml
    /// </summary>
    public partial class CustomInfo : System.Windows.Controls.UserControl
    {
        public wizCustom wizCus = new();
        public List<string> partsList = new() { "ZUJO", "FACE", "NECK",  "BACK" };
        public CustomInfo()
        {
            InitializeComponent();
            Program.Wizard.SetModuleImage(Properties.Resources.md_dummy, moduleImage);
            partsBox.ItemsSource = partsList;
            partsBox.SelectedIndex = 0;
        }

        private void nameButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(false, "Enter Item Name");
            tex.ShowDialog();
            if (tex.Result != Properties.Resources.cmn_enter_value && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result))
            {
                nameText.Text = Properties.Resources.cmn_name + ": " + tex.Result;
                wizCus.name = tex.Result;
            }
            else { return; }
        }

        private void idButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(true, "Enter Item ID Value");
            tex.ShowDialog();
            if (tex.Result != Properties.Resources.cmn_enter_value && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result) && !tex.Result.Contains(' '))
            {
                wizCus.id = int.Parse(tex.Result);
                idText.Text = Properties.Resources.cmn_id + ": " + wizCus.id;
            }
            else { return; }
        }

        private void indexButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(true, "Enter Item Sorting Index Value");
            tex.ShowDialog();
            if (tex.Result != Properties.Resources.cmn_enter_value && tex.Result.Length > 0 && !string.IsNullOrWhiteSpace(tex.Result) && !tex.Result.Contains(' '))
            {
                wizCus.sort_index = int.Parse(tex.Result);
                indexText.Text = Properties.Resources.cmn_index + ": " + tex.Result;
            }
            else { return; }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.Filter = "PNG Files|*.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap pngImage = new(ofd.FileName);
                if (pngImage.Width == 512 && pngImage.Height == 512)
                {
                    moduleImage.Source = Program.GetImage(pngImage);
                    wizCus.bitmap = pngImage;
                }
                else
                {
                    Program.NotiBox(Properties.Resources.exp_3, Properties.Resources.cmn_error);
                }
            }
        }

        private void partsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (partsBox.SelectedIndex > -1)
            {
                wizCus.parts = partsBox.SelectedValue as string;
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FARC files|*.farc";
            ofd.Title = Properties.Resources.exp_1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fileName.Text = System.IO.Path.GetFileName(ofd.FileName).ToString();
                wizCus.obj.objectFilePath = ofd.FileName;
            }
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            wizCus.obj.item.name = Properties.Resources.cmn_temp;
            PresetPicker picker = new(wizCus.obj.item, wizCus.obj.objectFilePath, "ALL", false, true);
            picker.ShowDialog();
            wizCus.obj.item = picker.itemCurrent;
        }
    }
}
