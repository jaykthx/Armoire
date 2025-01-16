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
using MikuMikuLibrary.Materials;
using System.Windows.Documents;
using System.Diagnostics;

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
        string exportFolder;

        public CustomiseWizard()
        {
            InitializeComponent();
            PopulateChritms();
        }

        private void PopulateChritms() //OK
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
        private void Add_Click(object sender, RoutedEventArgs e) //OK
        {
            CustomInfo cusInfo = new();
            itemHost.Children.Add(cusInfo);
        }
        private void Remove_Click(object sender, RoutedEventArgs e) // OK
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
                    SelectedPath = Properties.Settings.Default.gamePath
                };
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Program.GetExistingIDs(fbd.SelectedPath, finalUsedIDs);
                    Properties.Settings.Default.gamePath = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                    TextEntry textEntry = new(false, "Enter MOD Folder Name");
                    textEntry.ShowDialog();
                    if (textEntry.Result.Length > 0)
                    {
                        exportFolder = Properties.Settings.Default.gamePath + "/mods/" + textEntry.Result + "/rom/";
                        Directory.CreateDirectory(exportFolder);
                        Directory.CreateDirectory(exportFolder + "2d");
                        Directory.CreateDirectory(exportFolder + "objset");
                        Directory.CreateDirectory(exportFolder + "lang2");
                        Program.CreateModConfig(exportFolder.Remove(exportFolder.Length - 5, 5), textEntry.Result);
                        // do the actual making
                        tempCustoms.Clear();
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
                                Program.NotiBox("Please check your items, something is not set correctly.", Properties.Resources.cmn_error);
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
                        Program.NotiBox("You need to give the mod a name.\nNot proceeding.", Properties.Resources.cmn_error);
                        return;
                    }
                }
            }
            else
            {
                Program.NotiBox("There are no items in the wizard.", Properties.Resources.cmn_error);
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
                Program.NotiBox("This ID is already being used and the resulting item will cause compatibility issues.", Properties.Resources.window_notice);
            }

            if (Program.Databases.CheckID(finalUsedIDs.customize_item_tbl_index, wizCus.sort_index) == false)
            {
                finalUsedIDs.customize_item_tbl_index.Add(wizCus.sort_index);
            }
            else
            {
                Program.NotiBox("This sorting index is already being used, the resulting customize item will have sorting issues.", "Warning");
            }
            temp.sort_index = wizCus.sort_index;
            Program.Databases.AddToSpriteDatabase(spr_db, wizCus.id, true, finalUsedIDs.spr_db);
            tempCustoms.Add(temp);
        }
        private int GetItemNumber(string item_file_name, string chara)
        {
            string name = Path.GetFileNameWithoutExtension(item_file_name);
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
                    ChoiceWindow choice = new("The item number of the object you selected is already being used with this character.\n" +
                        "Would you like to use a unused, randomly generated one?", "No", "Yes");
                    choice.ShowDialog();
                    if (choice.isRightClicked)
                    {
                        return Program.Databases.GetUnusedID(finalUsedIDs.chritm_prop_item[Program.Databases.GetChritmName(chara)]);
                    }
                    else
                    {
                        return int.Parse(final[1]);
                    }
                }
            }
            else
            {
                Program.NotiBox("This file doesn't contain 'itm', you will have to adjust the ID manually.", Properties.Resources.cmn_error);
                return 5555;
            }
        }
        private void AddToCharaItemTable(CustomInfo cusInfo)
        {
            List<wizObjEntry> entries = new();
            wizObjEntry objEntry = new();
            var farc = BinaryFile.Load<FarcArchive>(cusInfo.wizCus.obj.objectFilePath);
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
                    Dictionary<uint, uint> texIDs = new();
                    for (int i = 0; i < objset.TextureIds.Count; i++)
                    {
                        if (texIDs.ContainsKey(objset.TextureIds[i]))
                        {
                            objset.TextureIds[i] = texIDs[objset.TextureIds[i]];
                        }
                        else
                        {
                            texIDs.Add(objset.TextureIds[i], Program.Databases.GetUnusedID(finalUsedIDs.tex_db, 9999999));
                            objset.TextureIds[i] = texIDs[objset.TextureIds[i]];
                            finalUsedIDs.tex_db.Add(objset.TextureIds[i]);
                        }

                    }
                    foreach (MikuMikuLibrary.Objects.Object obj in objset.Objects)
                    {
                        foreach (Material mat in obj.Materials)
                        {
                            foreach (MaterialTexture matTex in mat.MaterialTextures)
                            {
                                if (texIDs.ContainsKey(matTex.TextureId))
                                {
                                    matTex.TextureId = texIDs[matTex.TextureId];
                                }
                            }
                        }
                    }
                    int texCount = 0;
                    foreach (uint id in objset.TextureIds)
                    {
                        TextureInfo tex = new()
                        {
                            Id = id,
                            Name = mainName.ToUpper() + "_AUTO_TEXTURE_" + texCount
                        };
                        texCount++;
                        tex_db.Textures.Add(tex);
                    }
                    objset.Save(stream, true);
                    cusInfo.wizCus.obj.objEntry = objEntry;
                    entries.Add(objEntry);
                    farc.Add(fileName, stream, false, ConflictPolicy.Replace);
                    farc.IsCompressed = true;
                    farc.Save(exportFolder + "/objset/" + Path.GetFileName(cusInfo.wizCus.obj.objectFilePath));
                }
                farc.Dispose();
            }
            ObjectSetInfo objSetInfo = new();
            ObjectInfo objInfo = new();
            objSetInfo.Id = Program.Databases.GetUnusedID(finalUsedIDs.obj_db, 19999);
            finalUsedIDs.obj_db.Add(objSetInfo.Id);
            objSetInfo.Name = cusInfo.wizCus.obj.objEntry.name;
            objSetInfo.TextureFileName = cusInfo.wizCus.obj.objEntry.textureFileName;
            objSetInfo.ArchiveFileName = cusInfo.wizCus.obj.objEntry.archiveFileName;
            objSetInfo.FileName = cusInfo.wizCus.obj.objEntry.fileName;
            objInfo.Id = cusInfo.wizCus.obj.objEntry.objId;
            objInfo.Name = cusInfo.wizCus.obj.objEntry.objName;
            objSetInfo.Objects.Add(objInfo);
            obj_db.ObjectSets.Add(objSetInfo);
            cusInfo.wizCus.obj.item.name = cusInfo.wizCus.obj.objEntry.name + "_AUTO-GEN_"+cusInfo.wizCus.parts;
            cusInfo.wizCus.obj.item.uid = cusInfo.wizCus.obj.objEntry.objName;
            cusInfo.wizCus.obj.item.objset = new List<string>
                {
                    cusInfo.wizCus.obj.objEntry.name
                };
            cusInfo.wizCus.obj.item.dataSetTexes = new ObservableCollection<dataSetTex>();
            int itm_num = GetItemNumber(cusInfo.wizCus.obj.objectFilePath, "cmnitm");
            cusInfo.wizCus.obj.item.no = itm_num;
            Test(itm_num, cusInfo);
        }
        private void Test(int itm_num, CustomInfo cusInfo)
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

