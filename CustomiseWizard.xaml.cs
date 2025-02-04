using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Objects;
using MikuMikuLibrary.Databases;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Armoire.Dialogs;
using System.Windows.Forms;
using Object = MikuMikuLibrary.Objects.Object;

namespace Armoire
{
    public partial class CustomiseWizard : Window
    {
        ObservableCollection<cstm_item> tempCustoms = new(); // final customise item table to be exported
        TextureDatabase tex_db = new(); // final tex_db to be exported
        ObjectDatabase obj_db = new(); // final obj_db to be exported
        SpriteDatabase spr_db = new(); // final spr_db to be exported
        ObservableCollection<chritmFile> list = new(); // list of all chritms to be exported (chritm x 10 = 10 charas)
        usedIDs finalUsedIDs = new();
        public string exportFolder;

        public CustomiseWizard()
        {
            InitializeComponent();
            PopulateChritms();
        }

        private void PopulateChritms()
        {
            foreach (string x in Program.charas)
            {
                chritmFile file = new()
                {
                    items = new List<itemEntry>(),
                    costumes = new List<cosEntry>(),
                    chara = x.Remove(3, x.Length - 3).ToLower()
                };
                list.Add(file);
            }
        }
        private void Add_Click(object sender, RoutedEventArgs e) 
        {
            CustomInfo cusInfo = new();
            itemHost.Children.Add(cusInfo);
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                itemHost.Children.RemoveAt(itemHost.Children.Count - 1);
            }
            catch
            {
            }
        }
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            finalUsedIDs = new usedIDs();
            if (itemHost.Children.Count > 0)
            {
                FolderBrowserDialog fbd = new()
                {
                    Description = "Please select your game directory.",
                };
                if (Path.Exists(Properties.Settings.Default.gamePath))
                {
                    fbd.SelectedPath = Properties.Settings.Default.gamePath;
                }
                else
                {
                    Properties.Settings.Default.gamePath = "";
                }
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Program.GetExistingIDs(fbd.SelectedPath, finalUsedIDs);
                    Properties.Settings.Default.gamePath = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                    TextEntry textEntry = new(false, "Enter MOD Folder Name");
                    textEntry.ShowDialog();
                    if (textEntry.Result.Length > 0)
                    {
                        exportFolder = Program.Wizard.ProcessDirectories(textEntry.Result);
                        // do the actual making
                        tempCustoms.Clear();
                        tex_db = new TextureDatabase();
                        obj_db = new ObjectDatabase();
                        spr_db = new SpriteDatabase();
                        list.Clear();
                        PopulateChritms();
                        bool proceed = true;
                        foreach (CustomInfo info in itemHost.Children)
                        {
                            if (info.wizCus.parts != null && info.wizCus.name != null && info.wizCus.obj.objectFilePath != null)
                            {
                                AddToCharaItemTable(info);
                                AddToCustomiseTable(info.wizCus, info.wizCus.obj.item.no); // add to cstm table
                                Program.CreateModLocalisation(exportFolder + "/lang2/", info.wizCus.name, info.wizCus.id, false);
                                Program.GenerateSprite(info.wizCus.bitmap, info.wizCus.id, exportFolder + "2d", true);
                            }
                            else
                            {
                                Program.NotiBox(Properties.Resources.warn_wizard_problem, Properties.Resources.cmn_error);
                                proceed = false;
                                Program.IO.DeleteDirectory(exportFolder);
                                return;
                            }
                        }
                        if (proceed)
                        {
                            Program.IO.SaveFile(exportFolder + "/mod_gm_customize_item_tbl.farc", tempCustoms);
                            if (spr_db.SpriteSets.Count > 0)
                            {
                                spr_db.Save(exportFolder + "/2d/mod_spr_db.bin");
                            }
                            obj_db.Save(exportFolder + "/objset/mod_obj_db.bin");
                            tex_db.Save(exportFolder + "/objset/mod_tex_db.bin");
                            Program.IO.SaveChr(exportFolder + "/mod_chritm_prop.farc", list);
                        }
                    }
                    else
                    {
                        Program.NotiBox(Properties.Resources.warn_no_name, Properties.Resources.cmn_error);
                        return;
                    }
                }
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_no_items, Properties.Resources.cmn_error);
            }
        }
        private void AddToCustomiseTable(wizCustom wizCus, int item_no)
        {
            cstm_item temp = new()
            {
                bind_module = -1,
                id = wizCus.id,
                name = wizCus.name,
                parts = wizCus.parts,
                chara = "ALL",
                obj_id = item_no,
                ng = true,
                shop_price = "300",
                shop_st_day = 1,
                shop_st_month = 1,
                shop_st_year = 2009
            };
            if (Program.Databases.CheckID(finalUsedIDs.customize_item_tbl, wizCus.id) == false)
            {
                finalUsedIDs.customize_item_tbl.Add(temp.id);
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_used_0 + Properties.Resources.cmn_id + Properties.Resources.warn_used_1, Properties.Resources.window_notice);
            }

            if (Program.Databases.CheckID(finalUsedIDs.customize_item_tbl_index, wizCus.sort_index) == false)
            {
                finalUsedIDs.customize_item_tbl_index.Add(wizCus.sort_index);
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_used_0 + Properties.Resources.cmn_index + Properties.Resources.warn_used_1, Properties.Resources.window_notice);
            }
            temp.sort_index = wizCus.sort_index;
            Program.Databases.AddToSpriteDatabase(spr_db, wizCus.id, true, finalUsedIDs.spr_db);
            tempCustoms.Add(temp);
        }
        private ObjectSetInfo CreateObjInfo(wizObj wiz)
        {
            ObjectSetInfo objSetInfo = new();
            var farc = BinaryFile.Load<FarcArchive>(wiz.objectFilePath);
            var newFarc = new FarcArchive();
            foreach (string fileName in farc)
            {
                if (fileName.EndsWith("_obj.bin"))
                {
                    string mainName = fileName.Remove(fileName.Length - 8, 8);
                    objSetInfo.Name = mainName.ToUpper();
                    objSetInfo.ArchiveFileName = mainName + ".farc";
                    objSetInfo.FileName = fileName;
                    objSetInfo.TextureFileName = mainName + "_tex.bin";
                    var stream = new MemoryStream();
                    var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                    var objset = new ObjectSet();
                    objset.Load(source);
                    Program.Wizard.ConvertToTriangleStrips(objset);
                    foreach (Object obj in objset.Objects)
                    {
                        ObjectInfo objInfo = new()
                        {
                            Name = obj.Name,
                            Id = obj.Id
                        };
                        objSetInfo.Objects.Add(objInfo);
                    }
                    Program.Wizard.ProcessTextures(objset, mainName, finalUsedIDs, tex_db);
                    objset.Save(stream, true);
                    farc.Add(objSetInfo.FileName, stream, false, ConflictPolicy.Replace);
                    farc.Save(exportFolder + "/objset/" + objSetInfo.ArchiveFileName);
                }
            }
            farc.Dispose();
            newFarc.Dispose();
            return objSetInfo;
        }

        private void AddToCharaItemTable(CustomInfo cusInfo)
        {
            List<ObjectSetInfo> entries = new();
            ObjectSetInfo objset = CreateObjInfo(cusInfo.wizCus.obj);
            entries.Add(objset);
            cusInfo.wizCus.obj.objectSet = objset;
            objset.Id = Program.Databases.GetUnusedID(finalUsedIDs.obj_db, 19999);
            finalUsedIDs.obj_db.Add(objset.Id);
            obj_db.ObjectSets.Add(objset);
            cusInfo.wizCus.obj.item.name = cusInfo.wizCus.obj.objectSet.Name + " " + Program.Wizard.ReturnSubIDString(cusInfo.wizCus.obj.item.subID);
            cusInfo.wizCus.obj.item.uid = cusInfo.wizCus.obj.objectSet.Objects[0].Name;
            cusInfo.wizCus.obj.item.objset = new List<string>
                {
                    cusInfo.wizCus.obj.objectSet.Name
                };
            int itm_num = Program.Wizard.GetItemNumber(cusInfo.wizCus.obj.objectFilePath, "cmnitm", finalUsedIDs);
            Finish(itm_num, cusInfo);
        }

        private void Finish(int itm_num, CustomInfo cusInfo)
        {
            int count = 0;
            foreach (var chara in Program.charas)
            {
                finalUsedIDs.chritm_prop_item[Program.Databases.GetChritmName(chara)].Add(itm_num);
                list[count].items.Add(cusInfo.wizCus.obj.item);
                count++;
            }
        }
    }
}

