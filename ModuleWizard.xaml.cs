using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Objects;
using MikuMikuLibrary.Databases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Armoire.Dialogs;
using System.Windows.Forms;
using MikuMikuLibrary.Archives.CriMw;
using System.Reflection;
using MikuMikuLibrary.Textures;
using System.Runtime.InteropServices.ComTypes;
using MikuMikuLibrary.Materials;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Armoire
{
    public partial class ModuleWizard : Window
    {
        ObservableCollection<module> tempModules = new ObservableCollection<module>(); // final module table to be exported
        ObservableCollection<cstm_item> tempCustoms = new ObservableCollection<cstm_item>(); // final customise item table to be exported
        TextureDatabase tex_db = new TextureDatabase(); // final tex_db to be exported
        ObjectDatabase obj_db = new ObjectDatabase(); // final obj_db to be exported
        SpriteDatabase spr_db = new SpriteDatabase(); // final spr_db to be exported
        ObservableCollection<chritmFile> list = new ObservableCollection<chritmFile>(); // list of all chritms to be exported (chritm x 10 = 10 charas)
        usedIDs finalUsedIDs = new usedIDs();
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
                chritmFile file = new chritmFile
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
            ModuleInfo modInfo = new ModuleInfo();
            moduleHost.Children.Add(modInfo);
        }

        private void GetExistingIDs(string gameDirectory, usedIDs usedID)
        {
            List<string> obj_dbs;
            List<string> tex_dbs;
            List<string> spr_dbs;
            List<string> module_tbls;
            List<string> customise_tbls;
            List<string> chritm_props;
            CpkArchive cpk = new CpkArchive();
            if (gameDirectory != null)
            {
                if (Directory.Exists(gameDirectory + "\\mods"))
                {
                    obj_dbs = new List<string>(Directory.EnumerateFiles(gameDirectory + "\\mods", "*obj_db.bin", SearchOption.AllDirectories));
                    tex_dbs = new List<string>(Directory.EnumerateFiles(gameDirectory + "\\mods", "*tex_db.bin", SearchOption.AllDirectories));
                    spr_dbs = new List<string>(Directory.EnumerateFiles(gameDirectory + "\\mods", "*spr_db.bin", SearchOption.AllDirectories));
                    module_tbls = new List<string>(Directory.EnumerateFiles(gameDirectory + "\\mods", "*gm_module_tbl.farc", SearchOption.AllDirectories));
                    customise_tbls = new List<string>(Directory.EnumerateFiles(gameDirectory + "\\mods", "*gm_customize_item_tbl.farc", SearchOption.AllDirectories));
                    chritm_props = new List<string>(Directory.EnumerateFiles(gameDirectory + "\\mods", "*chritm_prop.farc", SearchOption.AllDirectories));
                    foreach (string file in module_tbls)
                    {
                        FarcArchive farc = BinaryFile.Load<FarcArchive>(file);
                        usedID.get_used_ids(farc, 0);
                    }
                    foreach (string file in customise_tbls)
                    {
                        FarcArchive farc = BinaryFile.Load<FarcArchive>(file);
                        usedID.get_used_ids(farc, 1);
                    }
                    foreach (string file in chritm_props)
                    {
                        FarcArchive farc = BinaryFile.Load<FarcArchive>(file);
                        usedID.get_used_ids(farc, 2);
                    }
                    foreach (string file in obj_dbs)
                    {
                        ObjectDatabase obj_db = BinaryFile.Load<ObjectDatabase>(file);
                        usedID.get_used_ids(obj_db);
                    }
                    foreach (string file in tex_dbs)
                    {
                        TextureDatabase tex_db = BinaryFile.Load<TextureDatabase>(file);
                        usedID.get_used_ids(tex_db);
                    }
                    foreach (string file in spr_dbs)
                    {
                        SpriteDatabase spr_db = BinaryFile.Load<SpriteDatabase>(file);
                        usedID.get_used_ids(spr_db);
                    }
                }
                if (File.Exists((gameDirectory + "\\diva_dlc00_region.cpk")))
                {
                    cpk = BinaryFile.Load<CpkArchive>(gameDirectory + "\\diva_dlc00_region.cpk");
                }
                else if (File.Exists((gameDirectory + "\\diva_main_region.cpk")))
                {
                    cpk = BinaryFile.Load<CpkArchive>(gameDirectory + "\\diva_main_region.cpk");
                }
                else if (File.Exists((gameDirectory + "\\diva_main.cpk")))
                {
                    cpk = BinaryFile.Load<CpkArchive>(gameDirectory + "\\diva_main.cpk");
                }
                else
                {
                    Program.NotiBox("This is not a valid Project DIVA Mega Mix+ directory.", "Error");
                }
                foreach (string file in cpk.FileNames)
                {
                    switch (file)
                    {
                        case string s when s.Contains("gm_module_tbl.farc"):
                            FarcArchive farc = BinaryFile.Load<FarcArchive>(cpk.Open(file, EntryStreamMode.MemoryStream));
                            usedID.get_used_ids(farc, 0);
                            break;
                        case string s when s.Contains("gm_customize_item_tbl.farc"):
                            FarcArchive farc2 = BinaryFile.Load<FarcArchive>(cpk.Open(file, EntryStreamMode.MemoryStream));
                            usedID.get_used_ids(farc2, 1);
                            break;
                        case string s when s.Contains("chritm_prop.farc"):
                            FarcArchive farc3 = BinaryFile.Load<FarcArchive>(cpk.Open(file, EntryStreamMode.MemoryStream));
                            usedID.get_used_ids(farc3, 2);
                            break;
                        case string s when s.Contains("*obj_db.farc"):
                            ObjectDatabase obj_db = BinaryFile.Load<ObjectDatabase>(cpk.Open(file, EntryStreamMode.MemoryStream));
                            usedID.get_used_ids(obj_db);
                            break;
                        case string s when s.Contains("*tex_db.farc"):
                            TextureDatabase tex_db = BinaryFile.Load<TextureDatabase>(cpk.Open(file, EntryStreamMode.MemoryStream));
                            usedID.get_used_ids(tex_db);
                            break;
                        case string s when s.Contains("*spr_db.farc"):
                            SpriteDatabase spr_db = BinaryFile.Load<SpriteDatabase>(cpk.Open(file, EntryStreamMode.MemoryStream));
                            usedID.get_used_ids(spr_db);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void CreateModConfig(string FolderPath, string ModName)
        {
            using (TextWriter tw = new StreamWriter(FolderPath + "/config.toml"))
            {
                tw.WriteLine("enabled = true");
                tw.WriteLine("name = \"" + ModName + "\"");
                tw.WriteLine("description = \"A mod created using Armoire.\"");
                tw.WriteLine("include = [\".\"]");
            }
        }
        
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            finalUsedIDs = new usedIDs();
            if (moduleHost.Children.Count > 0)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog
                {
                    Description = "Please select your game directory.",
                    SelectedPath = Properties.Settings.Default.gamePath
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    GetExistingIDs(fbd.SelectedPath, finalUsedIDs);
                    Properties.Settings.Default.gamePath = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                    TextEntry textEntry = new TextEntry(false, "Enter MOD Folder Name");
                    textEntry.ShowDialog();
                    if (textEntry.Result.Length > 0)
                    {
                        exportFolder = Properties.Settings.Default.gamePath + "/mods/" + textEntry.Result + "/rom/";
                        Directory.CreateDirectory(exportFolder);
                        Directory.CreateDirectory(exportFolder + "2d");
                        Directory.CreateDirectory(exportFolder + "objset");
                        Directory.CreateDirectory(exportFolder + "lang2");
                        CreateModConfig(exportFolder.Remove(exportFolder.Length - 5, 5), textEntry.Result);
                        // do the actual making
                        tempModules.Clear();
                        tex_db = new TextureDatabase();
                        obj_db = new ObjectDatabase();
                        spr_db = new SpriteDatabase();
                        list.Clear();
                        PopulateChritms();
                        bool proceed = true;
                        // see if lang toml exists and delete it if it does!
                        if (File.Exists(exportFolder + "/lang2/mod_str_array.toml"))
                        {
                            File.Delete(exportFolder + "/lang2/mod_str_array.toml");
                        }
                        foreach (ModuleInfo info in moduleHost.Children)
                        {
                            info.finalizeModule();
                            if (info.wizMod.chara != null && info.wizMod.name != null && info.wizMod.objects.Count > 0)
                            {
                                AddToModuleTable(info.wizMod);
                                Program.CreateModLocalisation(exportFolder + "/lang2/", info.wizMod.name, info.wizMod.id);
                                if (info.wizMod.image_path != null)
                                {
                                    Program.GenerateSprite(info.wizMod.image_path, Program.Databases.GetIDString(info.wizMod.id.ToString()), exportFolder + "2d", false);
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
                                            Program.GenerateSprite(info.wizMod.image_path, Program.Databases.GetIDString(info.wizMod.id.ToString()), exportFolder + "2d", true);
                                        }
                                    }
                                }
                                else
                                {
                                    Program.GenerateSprite("../../Resources/md_dummy.png", Program.Databases.GetIDString(info.wizMod.id.ToString()), exportFolder + "2d", false);
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
                                            Program.GenerateSprite("../../Resources/md_dummy.png", Program.Databases.GetIDString(info.wizMod.id.ToString()), exportFolder + "2d", true);
                                        }
                                    }
                                }
                                AddToCharaItemTable(info);
                            }
                            else
                            {
                                Program.NotiBox("Please check your modules, something is not set correctly.", "Error");
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
                    Program.NotiBox("You need to give the mod a name.\nNot proceeding.", "Error");
                    return;
                }
            }
            else
            {
                Program.NotiBox("There are no modules in the wizard.", "Error");
            }
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            moduleHost.Children.RemoveAt(moduleHost.Children.Count - 1);
        }

        private void AddToModuleTable(wizModule wizMod)
        {
            module temp = new module();
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
                Program.NotiBox("This ID is already being used and the resulting module will cause compatibility issues.", "Notice");
                ChoiceWindow win = new ChoiceWindow("This ID is already being used and the resulting module will cause compatibility issues." +
                    "\nWould you like to use a random, unused ID instead?", "No", "Yes");
                win.ShowDialog();
                if (win.isRightClicked)
                {
                    wizMod.id = Program.Databases.GetUnusedID(finalUsedIDs.module_tbl);
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
                Program.NotiBox("This sortiing index is already being used, the resulting module will have sorting issues.", "Warning");
                ChoiceWindow win = new ChoiceWindow("This sorting index is already being used and the resulting module will cause minor issues." +
                    "\nWould you like to use a random, unused sorting index instead?", "No", "Yes");
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
            Program.Databases.AddToSpriteDatabase(spr_db, wizMod, false, finalUsedIDs.spr_db);
            tempModules.Add(temp);
        }
        private void AddToCustomiseTable(wizModule wizMod, int item_no)
        {
            cstm_item temp = new cstm_item
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
                Program.NotiBox("This ID is already being used and the resulting module will cause compatibility issues.", "Notice");
            }
            
            if (Program.Databases.CheckID(finalUsedIDs.customize_item_tbl_index, wizMod.sort_index) == false)
            {
                finalUsedIDs.customize_item_tbl_index.Add(wizMod.sort_index);
            }
            else
            {
                Program.NotiBox("This sortiing index is already being used, the resulting customize item will have sorting issues.", "Warning");
            }
            temp.sort_index = wizMod.sort_index;
            Program.Databases.AddToSpriteDatabase(spr_db, wizMod, true, finalUsedIDs.spr_db);
            tempCustoms.Add(temp);
        }

        private void AddToCharaItemTable(ModuleInfo modInfo)
        {
            List<wizObjEntry> entries = new List<wizObjEntry>();
            foreach (wizObj x in modInfo.wizMod.objects)
            {
                wizObjEntry objEntry = new wizObjEntry();
                /*if (File.Exists(exportFolder + "/objset/" + System.IO.Path.GetFileName(x.objectFilePath)))
                {
                    Program.NotiBox("The object files already exist in this folder and can't be overwritten.", "Notice"); // could do with a yes or no box for this part
                }
                else
                {
                    File.Copy(x.objectFilePath, exportFolder + "/objset/" + System.IO.Path.GetFileName(x.objectFilePath), true);
                }*/
                var farc = BinaryFile.Load<FarcArchive>(x.objectFilePath);
                foreach (string fileName in farc)
                {
                    if (fileName.EndsWith("_obj.bin"))
                    {
                        string mainName = fileName.Remove(fileName.Length - 8, 8);
                        objEntry.fileName = fileName;
                        objEntry.name = (mainName.ToUpper());
                        objEntry.archiveFileName = mainName + ".farc";
                        objEntry.textureFileName = mainName + "_tex.bin";
                        var stream = new MemoryStream();
                        var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                        var objset = new ObjectSet();
                        objset.Load(source);
                        foreach (var obj in objset.Objects)
                        {
                            objEntry.objName = obj.Name;
                            objEntry.objId = obj.Id;
                            foreach (var mesh in obj.Meshes) // might as well leave this in from kei's batchtristripper - thank you bestie for this part
                            {
                                foreach (var subMesh in mesh.SubMeshes)
                                {
                                    if (subMesh.PrimitiveType != PrimitiveType.Triangles)
                                        continue;

                                    var triangleStrip = MikuMikuLibrary.Objects.Processing.Stripifier.Stripify(subMesh.Indices);
                                    if (triangleStrip == null)
                                        continue;

                                    subMesh.PrimitiveType = PrimitiveType.TriangleStrip;
                                    subMesh.Indices = triangleStrip;
                                }
                            }
                        }
                        Dictionary<uint, uint> texIDs = new Dictionary<uint, uint>();
                        for (int i = 0; i < objset.TextureIds.Count; i++)
                        {
                            texIDs.Add(objset.TextureIds[i], Program.Databases.GetUnusedID(finalUsedIDs.tex_db));
                            objset.TextureIds[i] = texIDs[objset.TextureIds[i]];
                            finalUsedIDs.tex_db.Add(objset.TextureIds[i]);
                        }
                        for (int i = 0; i < objset.Objects[0].Materials.Count; i++)
                        {
                            foreach(MaterialTexture mat in objset.Objects[0].Materials[i].MaterialTextures)
                            {
                                if(texIDs.ContainsKey(mat.TextureId))
                                {
                                    mat.TextureId = texIDs[mat.TextureId];
                                }
                            }
                        }
                        int texCount = 0;
                        foreach (uint id in objset.TextureIds)
                        {
                            TextureInfo tex = new TextureInfo
                            {
                                Id = id,
                                Name = mainName.ToUpper() + "_AUTO_TEXTURE_" + texCount
                            };
                            texCount++;
                            tex_db.Textures.Add(tex);
                        }
                        objset.Save(stream, true);
                        x.objEntry = objEntry;
                        entries.Add(objEntry);
                        farc.Add(fileName, stream, false, ConflictPolicy.Replace);
                        farc.IsCompressed = true;
                        farc.Save(exportFolder + "/objset/" + System.IO.Path.GetFileName(x.objectFilePath));
                    }
                    farc.Dispose();
                }
            }
            foreach (wizObjEntry o in entries)
            {
                ObjectSetInfo objSetInfo = new ObjectSetInfo();
                ObjectInfo objInfo = new ObjectInfo();
                objSetInfo.Id = Program.Databases.GetUnusedID(finalUsedIDs.obj_db, 19999);
                finalUsedIDs.obj_db.Add(objSetInfo.Id);
                objSetInfo.Name = o.name;
                objSetInfo.TextureFileName = o.textureFileName;
                objSetInfo.ArchiveFileName = o.archiveFileName;
                objSetInfo.FileName = o.fileName;
                objInfo.Id = o.objId;
                objInfo.Name = o.objName;
                objSetInfo.Objects.Add(objInfo);
                obj_db.ObjectSets.Add(objSetInfo);
            }
            cosEntry cos = new cosEntry
            {
                id = modInfo.wizMod.id,
                items = new ObservableCollection<int>()
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
                x.item.name = x.objEntry.name + "_AUTO-GEN";
                x.item.uid = x.objEntry.objName;
                x.item.objset = new List<string>
                {
                    x.objEntry.name
                };
                x.item.dataSetTexes = new ObservableCollection<dataSetTex>();
                int itm_num = GetItemNumber(x.objectFilePath, modInfo.wizMod.chara);
                if (!Program.Databases.CheckID(finalUsedIDs.chritm_prop_item[Program.Databases.GetChritmName(modInfo.wizMod.chara)], itm_num))
                {
                    finalUsedIDs.chritm_prop_item[Program.Databases.GetChritmName(modInfo.wizMod.chara)].Add(x.item.no);
                }
                x.item.no = itm_num;
                finalUsedIDs.chritm_prop_item[Program.Databases.GetChritmName(modInfo.wizMod.chara)].Add(x.item.no);
                list[modInfo.charBox.SelectedIndex].items.Add(x.item);
                cos.items.Add(x.item.no);
                if (x.item.subID == 1 && !modInfo.wizMod.hairNG)
                {
                    AddToCustomiseTable(modInfo.wizMod, x.item.no);
                }
            }
            list[modInfo.charBox.SelectedIndex].costumes.Add(cos);
        }

        private int GetItemNumber(string item_file_name, string chara)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(item_file_name);
            if (name.Contains("itm"))
            {
                string[] final = name.Split(new string[] { "itm" }, StringSplitOptions.None);
                if (!Program.Databases.CheckID(finalUsedIDs.chritm_prop_item[Program.Databases.GetChritmName(chara)], int.Parse(final[1])))
                {
                    return int.Parse(final[1]);
                }
                else
                {
                    /*if(Program.ChoiceWindow("The item number of the object you selected is already being used with this character.\nYou are going to encounter problems if you do not change it.\nWould you like to change it to the next available ID?", "No", "Yes") == true)
                    {
                        // make code to change mikitm number and resave
                    }*/
                    return int.Parse(final[1]);
                }
            }
            else
            {
                Program.NotiBox("This file doesn't contain 'itm', you will have to adjust the ID manually.", "Error");
                return 5555;
            }
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
