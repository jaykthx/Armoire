using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Objects;
using MikuMikuLibrary.Databases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Armoire.Dialogs;
using System.Windows.Forms;
using System.Linq;
using Object = MikuMikuLibrary.Objects.Object;

namespace Armoire
{
    public partial class ModuleWizard : Window
    {
        ObservableCollection<module> tempModules = new(); // final module table to be exported
        ObservableCollection<cstm_item> tempCustoms = new(); // final customise item table to be exported
        TextureDatabase tex_db = new(); // final tex_db to be exported
        ObjectDatabase obj_db = new(); // final obj_db to be exported
        SpriteDatabase spr_db = new(); // final spr_db to be exported
        ObservableCollection<chritmFile> list = new(); // list of all chritms to be exported (chritm x 10 = 10 charas)
        usedIDs finalUsedIDs = new();
        string exportFolder;

        public ModuleWizard()
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
            ModuleInfo modInfo = new();
            moduleHost.Children.Add(modInfo);
        }
        
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            finalUsedIDs = new usedIDs();
            if (moduleHost.Children.Count > 0)
            {
                FolderBrowserDialog fbd = new()
                {
                    Description = "Please select your game directory.",
                    SelectedPath = Properties.Settings.Default.themeSetting
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Program.GetExistingIDs(fbd.SelectedPath, finalUsedIDs);
                    Properties.Settings.Default.themeSetting = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                    TextEntry textEntry = new(false, "Enter MOD Folder Name");
                    textEntry.ShowDialog();
                    if (textEntry.Result.Length > 0)
                    {
                        string exportFolder = Program.Wizard.ProcessDirectories(textEntry.Result);
                        // do the actual making
                        tempModules.Clear();
                        tex_db = new TextureDatabase();
                        obj_db = new ObjectDatabase();
                        spr_db = new SpriteDatabase();
                        list.Clear();
                        PopulateChritms();
                        bool proceed = true;
                        foreach (ModuleInfo info in moduleHost.Children)
                        {
                            info.finalizeModule();
                            if (info.wizMod.chara != null && info.wizMod.name != null && info.wizMod.objects.Count > 0)
                            {
                                AddToModuleTable(info.wizMod);
                                Program.CreateModLocalisation(exportFolder + "/lang2/", info.wizMod.name, info.wizMod.id, true);
                                Program.GenerateSprite(info.wizMod.bitmap, info.wizMod.id, exportFolder + "2d", false);
                                foreach (wizObj obj in info.wizMod.objects)
                                {
                                    if (obj.item.subID == 1)
                                    {
                                        info.wizMod.localNames.cn = info.wizMod.name + info.wizMod.localNames.cn;
                                        info.wizMod.localNames.en = info.wizMod.name + info.wizMod.localNames.en;
                                        info.wizMod.localNames.kr = info.wizMod.name + info.wizMod.localNames.kr;
                                        info.wizMod.localNames.fr = info.wizMod.name + info.wizMod.localNames.fr;
                                        info.wizMod.localNames.sp += info.wizMod.name;
                                        info.wizMod.localNames.ge += info.wizMod.name;
                                        info.wizMod.localNames.it += info.wizMod.name;
                                        info.wizMod.localNames.tw += info.wizMod.name;
                                        Program.CreateModLocalisation(exportFolder + "/lang2/", info.wizMod.localNames, info.wizMod.id);
                                        Program.GenerateSprite(info.wizMod.bitmap, info.wizMod.id, exportFolder + "2d", true);
                                    }
                                }
                                AddToCharaItemTable(info);
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
                            Program.IO.SaveFile(exportFolder + "/mod_gm_module_tbl.farc", tempModules);
                            if (tempCustoms.Count > 0)
                            {
                                Program.IO.SaveFile(exportFolder + "/mod_gm_customize_item_tbl.farc", tempCustoms);
                            }
                            if(spr_db.SpriteSets.Count > 0)
                            {
                                spr_db.Save(exportFolder + "/2d/mod_spr_db.bin");
                            }
                            obj_db.Save(exportFolder + "/objset/mod_obj_db.bin");
                            tex_db.Save(exportFolder + "/objset/mod_tex_db.bin");
                            Program.IO.SaveChr(exportFolder + "/mod_chritm_prop.farc", list);
                        }
                    }

                }
                else
                {
                    Program.NotiBox(Properties.Resources.warn_no_name, Properties.Resources.cmn_error);
                    return;
                }
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_no_items, Properties.Resources.cmn_error);
            }
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                moduleHost.Children.RemoveAt(moduleHost.Children.Count - 1);
            }
            catch
            {
            }
        }

        private void AddToModuleTable(wizModule wizMod)
        {
            module temp = new();
            if (wizMod.hairNG)
            {
                temp.attr = Attr.NoSwap_CT;
            }
            else
            {
                temp.attr = Attr.Default_CT;
            }
            temp.chara = wizMod.chara;
            if (Program.Databases.CheckID(finalUsedIDs.module_tbl, wizMod.id) == false)
            {
                finalUsedIDs.module_tbl.Add(temp.id);
            }
            else
            {
                ChoiceWindow win = new(Properties.Resources.warn_used_0 + Properties.Resources.cmn_item_nofull + Properties.Resources.warn_used_1 + "\n" +
                    Properties.Resources.warn_used_offer, Properties.Resources.cmn_no, Properties.Resources.cmn_yes);
                win.ShowDialog();
                if (win.isRightClicked)
                {
                    wizMod.id = Program.Databases.GetUnusedID(finalUsedIDs.module_tbl, finalUsedIDs.customize_item_tbl);
                    finalUsedIDs.module_tbl.Add(wizMod.id);
                }
            }
            temp.id = wizMod.id;
            temp.cos = "COS_" + Program.Databases.GetIDString((temp.id + 1).ToString());
            temp.name = wizMod.name;
            if (Program.Databases.CheckID(finalUsedIDs.module_tbl_index, wizMod.sort_index) == false)
            {
                finalUsedIDs.module_tbl_index.Add(wizMod.sort_index);
            }
            else
            {
                ChoiceWindow win = new(Properties.Resources.warn_used_0 + Properties.Resources.cmn_index + Properties.Resources.warn_used_1 + "\n" +
                    Properties.Resources.warn_used_offer, Properties.Resources.cmn_item_no, Properties.Resources.cmn_yes);
                win.ShowDialog();
                if (win.isRightClicked)
                {
                    wizMod.sort_index = Program.Databases.GetUnusedID(finalUsedIDs.module_tbl_index);
                    finalUsedIDs.module_tbl_index.Add(wizMod.sort_index);
                }
            }
            temp.sort_index = wizMod.sort_index;
            temp.shop_price = "900";
            temp.shop_st_day = 1;
            temp.shop_st_month = 1;
            temp.shop_st_year = 2009;
            Program.Databases.AddToSpriteDatabase(spr_db, wizMod.id, false, finalUsedIDs.spr_db);
            tempModules.Add(temp);
        }
        private void AddToCustomiseTable(wizModule wizMod, int item_no)
        {
            cstm_item temp = new()
            {
                bind_module = wizMod.id,
                id = wizMod.id,
                name = wizMod.name + "ヘア",
                parts = "KAMI",
                chara = wizMod.chara,
                obj_id = item_no,
                ng = true,
                shop_price = "2",
                shop_st_day = 1,
                shop_st_month = 1,
                shop_st_year = 2009
            };
            if (Program.Databases.CheckID(finalUsedIDs.customize_item_tbl, wizMod.id) == false)
            {
                finalUsedIDs.customize_item_tbl.Add(temp.id);
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_used_0 + Properties.Resources.cmn_id + Properties.Resources.warn_used_1, Properties.Resources.window_notice);
            }
            
            if (Program.Databases.CheckID(finalUsedIDs.customize_item_tbl_index, wizMod.sort_index) == false)
            {
                finalUsedIDs.customize_item_tbl_index.Add(wizMod.sort_index);
            }
            else
            {
                Program.NotiBox(Properties.Resources.warn_used_0 + Properties.Resources.cmn_index + Properties.Resources.warn_used_1, Properties.Resources.window_notice);
            }
            temp.sort_index = wizMod.sort_index;
            Program.Databases.AddToSpriteDatabase(spr_db, wizMod.id, true, finalUsedIDs.spr_db);
            tempCustoms.Add(temp);
        }

        private ObjectSetInfo CreateObjInfo(wizObj wiz, string chara)
        {
            ObjectSetInfo objSetInfo = new();
            var farc = BinaryFile.Load<FarcArchive>(wiz.objectFilePath);
            string subName = chara.ToUpper() + "ITM" + Program.Databases.GetUnusedID(finalUsedIDs.chritm_prop_item[Program.Databases.GetChritmName(chara)]);
            var newFarc = new FarcArchive();
            foreach (string fileName in farc)
            {
                if (fileName.EndsWith("_obj.bin"))
                {
                    string mainName = fileName.Remove(fileName.Length - 8, 8);
                    if (wiz.item.attr != 2085)
                    {
                        objSetInfo.Name = mainName.ToUpper();
                    }
                    else
                    {
                        objSetInfo.Name = subName;
                    }
                    objSetInfo.ArchiveFileName = mainName + ".farc";
                    objSetInfo.FileName = fileName;
                    objSetInfo.TextureFileName = mainName + "_tex.bin";
                    var stream = new MemoryStream();
                    var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                    var objset = new ObjectSet();
                    objset.Load(source);
                    Program.Wizard.ConvertToTriangleStrips(objset);
                    List<Object> headObj = new List<Object>();
                    int hCount = 0;
                    foreach (Object obj in objset.Objects)
                    {
                        if (wiz.item.attr != 2085)
                        {
                            ObjectInfo objInfo = new();
                            objInfo.Name = obj.Name;
                            objInfo.Id = obj.Id;
                            objSetInfo.Objects.Add(objInfo);
                        }
                        else
                        {
                            
                            headObj.Add(obj);
                        }
                    }
                    foreach(Object hObj in headObj)
                    {
                        ObjectInfo objInfo = new();
                        if (hObj.Name.Contains("_head_0") || hObj.Name.Contains("_HEAD_0"))
                        {
                            objInfo.Name = subName + $"_ATAM_HEAD_0{hCount}_SP__DIVSKN";
                            hCount++;
                        }
                        else
                        {
                            objInfo.Name = subName + $"_ATAM_HEAD_999__DIVSKN";
                        }
                        objInfo.Id = hObj.Id;
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

        private void AddToCharaItemTable(ModuleInfo modInfo)
        {
            List<ObjectSetInfo> entries = new(); // this is where we're storing the ObjSetInfo classes we create below so they can be used for chritm_tbl generating ig?
            foreach (wizObj wiz in modInfo.wizMod.objects)
            {
                ObjectSetInfo objset = CreateObjInfo(wiz, Program.Databases.GetChritmName(modInfo.charBox.Text.ToUpper()));
                entries.Add(objset);
                wiz.objectSet = objset;
            }
            foreach (ObjectSetInfo o in entries)
            {
                o.Id = Program.Databases.GetUnusedID(finalUsedIDs.obj_db, 19999);
                finalUsedIDs.obj_db.Add(o.Id);
                obj_db.ObjectSets.Add(o);
            }
            cosEntry cos = new()
            {
                id = modInfo.wizMod.id,
                items = new()
            };
            bool containsHands = false;
            foreach (wizObj o in modInfo.wizMod.objects)
            {
                if (o.item.subID == 14)
                {
                    containsHands = true;
                }
            }
            if (!containsHands)
            {
                if (modInfo.wizMod.chara != "SAKINE")
                {
                    cos.items.Add(301);
                }
                else
                {
                    cos.items.Add(306);
                }
            }
            foreach (wizObj x in modInfo.wizMod.objects)
            {
                x.item.name = x.objectSet.Name + " " + Program.Wizard.ReturnSubIDString(x.item.subID);
                x.item.uid = x.objectSet.Objects[0].Name;
                x.item.objset = new List<string>
                {
                    x.objectSet.Name
                };
                if (x.item.attr == 2085)
                {
                    x.item.no = int.Parse(x.objectSet.Name.Split(new string[] { "ITM" }, StringSplitOptions.None).LastOrDefault());
                }
                else
                {
                    int itm_num = Program.Wizard.GetItemNumber(x.objectFilePath, modInfo.wizMod.chara, finalUsedIDs);
                    x.item.no = itm_num;
                }
                list[modInfo.charBox.SelectedIndex].items.Add(x.item);
                cos.items.Add(x.item.no);
                if (x.item.subID == 1 && !modInfo.wizMod.hairNG)
                {
                    AddToCustomiseTable(modInfo.wizMod, x.item.no);
                }
            }
            list[modInfo.charBox.SelectedIndex].costumes.Add(cos);
        }
    }
}
