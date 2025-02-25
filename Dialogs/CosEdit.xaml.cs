﻿using System.Windows;

namespace Armoire.Dialogs
{
    /// <summary>
    /// Interaction logic for CharaItem.xaml
    /// </summary>
    public partial class CosEdit : Window
    {
        cosEntry cosCxt;
        public CosEdit(cosEntry cos)
        {
            InitializeComponent();
            this.DataContext = cos;
            cosCxt = cos;
            idBox.Text = "ID: " + cos.id;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            TextEntry win = new(true, "Enter item number");
            if (win.ShowDialog() == true)
            {
                if (win.Result != Properties.Resources.cmn_enter_value && !string.IsNullOrEmpty(win.Result))
                {
                    cosCxt.items.Add(int.Parse(win.Result));
                    itemList.Items.Refresh();
                }
                else { Program.NotiBox(Properties.Resources.warn_enter_value, Properties.Resources.cmn_error); }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (itemList.SelectedIndex != -1 && cosCxt.items.Count > 1)
            {
                cosCxt.items.RemoveAt(itemList.SelectedIndex);
            }
            else if(cosCxt.items.Count == 1)
            {
                Program.NotiBox(Properties.Resources.warn_costume_empty, Properties.Resources.cmn_error);
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_item_not_sel, Properties.Resources.cmn_error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextEntry tex = new(true, Properties.Resources.cmn_enter_value);
            tex.ShowDialog();
            if(!string.IsNullOrEmpty(tex.Result))
            {
                cosCxt.id = int.Parse(tex.Result);
                idBox.Text = "ID: " + cosCxt.id;
            }
        }
    }
}
