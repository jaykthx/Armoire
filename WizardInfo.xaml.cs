using Armoire.Dialogs;
using Armoire.Properties;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Armoire
{
    public partial class WizardInfo : UserControl
    {
        public WizardEntry wizMod = new();
        public WizardInfo()
        {
            InitializeComponent();
            CharacterBox.ItemsSource = Program.charas;
            CharacterBox.SelectedIndex = 0;
            wizMod.chara = Program.charas[0];
            if (!Settings.Default.ftCustomise)
            {
                HairTabFT.Visibility = Visibility.Collapsed;
                ModuleTabFT.Visibility = Visibility.Collapsed;
            }
            Program.Wizard.SetModuleImage(Properties.Resources.md_dummy, moduleImageMM);
            Program.Wizard.SetModuleImage(Properties.Resources.md_dummy, moduleImageFT);
            Program.Wizard.SetModuleImage(Properties.Resources.md_dummy, hairImageMM);
            Program.Wizard.SetModuleImage(Properties.Resources.md_dummy, hairImageFT);
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            PresetPicker picker = new(wizMod.objects[0], "ALL", false);
            picker.ShowDialog();
            wizMod.objects[0].item = picker.itemCurrent;
            switch (wizMod.objects[0].item.subID)
            {
                case 0:
                    wizMod.parts = "ZUJO";
                    break;
                case 4:
                    wizMod.parts = "FACE";
                    break;
                case 8:
                    wizMod.parts = "NECK";
                    break;
                case 16:
                    wizMod.parts = "BACK";
                    break;
            }
        }
            
        private void AddButton_Click(object sender, RoutedEventArgs e) //add
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "FARC files|*.farc";
            ofd.Title = Properties.Resources.exp_1;
            try
            {
                if (ofd.ShowDialog() == true)
                {
                    foreach (string filePath in ofd.FileNames)
                    {
                        WizItem itm = new(false);
                        itm.curObj.objectFilePath = filePath;
                        itm.fileName.Text = Path.GetFileName(filePath);
                        ModuleItemPanel.Children.Add(itm);
                    }
                }
            }
            catch (Exception ex)
            {
                PopupNotification pop = new(ex.Message);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) // modules only
        {
            ModuleItemPanel.Children.Clear();
        }

        private void NameButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(false, "Enter Item Name");
            if (tex.Result != Properties.Resources.cmn_enter_value && !string.IsNullOrWhiteSpace(tex.Result))
            {
                NameText.Text = Properties.Resources.cmn_name + ": " + tex.Result;
                wizMod.name = tex.Result;
            }
            else { return; }
        }

        private void IdButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(true, "Enter ID Value");
            if (tex.Result != Properties.Resources.cmn_enter_value && !string.IsNullOrWhiteSpace(tex.Result) && !tex.Result.Contains(' '))
            {
                wizMod.id = int.Parse(tex.Result);
                IdText.Text = Properties.Resources.cmn_id + ": " + wizMod.id;
            }
            else { return; }
        }

        private void IndexButton_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(true, "Enter Sorting Index Value");
            if (tex.Result != Properties.Resources.cmn_enter_value && !string.IsNullOrWhiteSpace(tex.Result) && !tex.Result.Contains(' '))
            {
                wizMod.sort_index = int.Parse(tex.Result);
                IndexText.Text = Properties.Resources.cmn_index + ": " + tex.Result;
            }
            else { return; }
        }


        private void CharacterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CharacterBox.SelectedIndex > -1)
            {
                wizMod.chara = Program.charas[CharacterBox.SelectedIndex];
            }
        }
        private void PartsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CharacterBox.SelectedIndex > -1)
            {
                wizMod.chara = Program.charas[CharacterBox.SelectedIndex];
            }
        }

        public void finalizeModule() //add all objects so the program doesnt shit itself
        {
            if(TypeSelectionBox.SelectedIndex == 0)
            {
                wizMod.objects.Clear();
                wizMod.hairNG = (bool)HairSwapCheckBox.IsChecked;
                foreach (WizItem x in ModuleItemPanel.Children)
                {
                    if (x.curObj.item != null && x.curObj.objectFilePath != null)
                    {
                        wizMod.objects.Add(x.curObj);
                    }
                }
            }
            else
            {
                wizMod.isItem = true;
            }
        }

        private void SelectModuleImageMMButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new() { Filter = "PNG Files|*.png" };
            if (ofd.ShowDialog() == true)
            {
                Bitmap pngImage = new(ofd.FileName);
                if (pngImage.Width == 512 && pngImage.Height == 512)
                {
                    Program.Wizard.SetModuleImage(pngImage, moduleImageMM);
                    wizMod.img_mm = pngImage;
                }
                else
                {
                    Program.NotiBox(Properties.Resources.exp_3, Properties.Resources.cmn_error);
                }
            }
        }
        private void SelectModuleImageFTButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new() { Filter = "PNG Files|*.png" };
            if (ofd.ShowDialog() == true)
            {
                Bitmap pngImage = new(ofd.FileName);
                if (pngImage.Width == 512 && pngImage.Height == 512)
                {
                    Program.Wizard.SetModuleImage(pngImage, moduleImageFT);
                    wizMod.img_ft = pngImage;
                }
                else
                {
                    Program.NotiBox(Properties.Resources.exp_3, Properties.Resources.cmn_error);
                }
            }
        }
        private void SelectHairImageMMButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new() { Filter = "PNG Files|*.png" };
            if (ofd.ShowDialog() == true)
            {
                Bitmap pngImage = new(ofd.FileName);
                if (pngImage.Width == 512 && pngImage.Height == 512)
                {
                    Program.Wizard.SetModuleImage(pngImage, hairImageMM);
                    wizMod.img_h_mm = pngImage;
                }
                else
                {
                    Program.NotiBox(Properties.Resources.exp_3, Properties.Resources.cmn_error);
                }
            }
        }
        private void SelectHairImageFTButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new() { Filter = "PNG Files|*.png" };
            if (ofd.ShowDialog() == true)
            {
                Bitmap pngImage = new(ofd.FileName);
                if (pngImage.Width == 512 && pngImage.Height == 512)
                {
                    Program.Wizard.SetModuleImage(pngImage, hairImageFT);
                    wizMod.img_h_ft = pngImage;
                }
                else
                {
                    Program.NotiBox(Properties.Resources.exp_3, Properties.Resources.cmn_error);
                }
            }
        }

        private void AddExistingButton_Click(object sender, RoutedEventArgs e) // add existing item (module only)
        {
            ItemPicker itmp = new(wizMod.chara);
            itmp.ShowDialog();
            foreach(CharacterItemEntry x in itmp.selectedItems)
            {
                WizItem itm = new(true);
                itm.curObj.objectFilePath = null;
                wizMod.existingItems.Add(x);
                itm.fileName.Text = x.name;
                ModuleItemPanel.Children.Add(itm);
            }
        }

        private void DeleteSelfButton_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        private void TypeSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ModuleItemEditPanel != null)
            {
                if (TypeSelectionBox.SelectedIndex == 0)
                {
                    wizMod.objects.Clear();
                    wizMod.isItem = false;
                    CustomizeItemEditPanel.Visibility = Visibility.Collapsed;
                    ModuleItemPanel.Visibility = Visibility.Visible;
                    CharacterBox.Visibility = Visibility.Visible;
                    CharacterText.Visibility = Visibility.Visible;
                    ItemPanelText.Visibility = Visibility.Visible;
                    HairSwapCheckBox.Visibility = Visibility.Visible;
                    HairSwapText.Visibility = Visibility.Visible;
                    ModuleItemEditPanel.Visibility = Visibility.Visible;
                    ItemFileName.Text = string.Empty;
                }
                else
                {
                    wizMod.objects.Clear();
                    wizMod.isItem = true;
                    wizMod.chara = "ALL";
                    ModuleItemEditPanel.Visibility = Visibility.Collapsed;
                    ModuleItemPanel.Visibility = Visibility.Collapsed;
                    CharacterBox.Visibility = Visibility.Collapsed;
                    CharacterText.Visibility = Visibility.Collapsed;
                    ItemPanelText.Visibility = Visibility.Collapsed;
                    HairSwapCheckBox.Visibility = Visibility.Collapsed;
                    HairSwapText.Visibility = Visibility.Collapsed;
                    CustomizeItemEditPanel.Visibility = Visibility.Visible;
                    ModuleItemPanel.Children.Clear();
                }
            }
        }

        private void SelectItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "FARC Files (*.farc)|*.farc",
                Title = "Please select your model's .farc file"
            };
            if (ofd.ShowDialog() == true)
            {
                WizardObject wizObj = new()
                {
                    objectFilePath = ofd.FileName
                };
                wizMod.objects.Clear();
                wizMod.objects.Add(wizObj);
                ItemFileName.Text = Path.GetFileNameWithoutExtension(ofd.FileName);
            }
        }

        private void HairSwapCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void PreviewImage_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
