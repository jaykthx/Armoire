using Armoire.Properties;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Materials;
using MikuMikuLibrary.Objects;
using MikuMikuLibrary.Sprites;
using MikuMikuLibrary.Textures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Armoire
{
    public class WizardCode
    {
        public static string modFolder;
        public UsedIdSet inUseIds = new();

        public static Dictionary<string, CharacterItemFile> CreateCharacterItemFile()
        {
            Dictionary<string, CharacterItemFile> characterItemFiles = new();
            foreach (string chara in Program.charas)
            {
                CharacterItemFile characterItemFile = new CharacterItemFile();
                characterItemFile.chara = chara.Remove(3);
                characterItemFiles.Add(chara.Remove(3), characterItemFile);
            }
            return characterItemFiles;
        }

        public static CustomizeItem CreateCustomiseItem(WizardEntry wizMod, UsedIdSet usedId, int exportId, int itemNo)
        {
            CustomizeItem cust = new();
            if (wizMod.isItem)
            {
                cust = new()
                {
                    bind_module = -1,
                    chara = "ALL",
                    id = exportId,
                    name = wizMod.name,
                    obj_id = itemNo,
                    parts = wizMod.parts,
                    shop_price = "300",
                    sort_index = usedId.GetUnusedId_Database(wizMod.sort_index, usedId.customize_item_tbl_index),
                    shop_st_day = DateTime.Today.Day,
                    shop_st_month = DateTime.Today.Month,
                    shop_st_year = DateTime.Today.Year
                };

            }
            else
            {
                cust = new()
                {
                    bind_module = exportId,
                    chara = wizMod.chara,
                    parts = "KAMI",
                    id = usedId.GetUnusedID_Maximum(usedId.customize_item_tbl, 1000000),
                    name = wizMod.name,
                    obj_id = itemNo,
                    sell_type = false,
                    shop_price = "2",
                    sort_index = usedId.GetUnusedId_Database(wizMod.sort_index, usedId.customize_item_tbl_index),
                    shop_st_day = DateTime.Today.Day,
                    shop_st_month = DateTime.Today.Month,
                    shop_st_year = DateTime.Today.Year
                };
            }
            return cust;
        }

        public static SpriteSetInfo ExportSprite(Bitmap bmp, int id, string outputFolder, bool isCustomize, UsedIdSet usedIdSet)
        {
            SpriteSetInfo dbEntry = new();
            SpriteSet sprSet = new();
            SpriteInfo sprInfo = new();
            SpriteTextureInfo sprTexInfo = new();
            Sprite spr = Program.GetSprite(isCustomize);
            Bitmap newBitmap = new(bmp);
            newBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            Texture text = MikuMikuLibrary.Native.TextureEncoder.EncodeFromBitmap(newBitmap, TextureFormat.DXT5, true);
            FarcArchive farc = new();
            var stream = new MemoryStream();
            string fileName = "spr_";
            if (isCustomize)
            {
                fileName = fileName + "cmnitm_thmb" + id;
                dbEntry.Name = fileName.ToUpper();
            }
            else
            {
                fileName = fileName + "sel_md" + id + "cmn";
                dbEntry.Name = fileName.ToUpper();
            }
            dbEntry.FileName = fileName + ".bin";
            dbEntry.Id = usedIdSet.GetUnusedID_Maximum(usedIdSet.spr_db, 9999999);
            usedIdSet.spr_db.Add(dbEntry.Id, "Armoire Wizard");
            text.Name = "MERGE_BC5COMP_0";
            sprTexInfo.Name = $"SPRTEX_{fileName.ToUpper()}_MERGE_BC5COMP_0";
            sprTexInfo.Id = usedIdSet.GetUnusedID_Maximum(usedIdSet.spr_db, 9999999);
            usedIdSet.spr_db.Add(sprTexInfo.Id, "Armoire Wizard");
            sprInfo.Name = fileName.ToUpper() + "_" + spr.Name;
            sprInfo.Id = usedIdSet.GetUnusedID_Maximum(usedIdSet.spr_db, 9999999);
            usedIdSet.spr_db.Add(sprInfo.Id, "Armoire Wizard");

            //finalize
            sprSet.TextureSet.Textures.Add(text);
            sprSet.Sprites.Add(spr);
            dbEntry.Textures.Add(sprTexInfo);
            dbEntry.Sprites.Add(sprInfo);
            //save
            sprSet.Save(stream, true);
            farc.IsCompressed = true;
            farc.Add(fileName + ".bin", stream, false, ConflictPolicy.Replace);
            farc.Save(outputFolder + "/" + fileName + ".farc");
            farc.Dispose();
            return dbEntry;
        }
        public static Module CreateModule(WizardEntry wizMod, UsedIdSet usedId, int exportId, int costumeId)
        {
            Module module = new()
            {
                cos = "COS_" + (costumeId + 1).ToString(),
                id = exportId,
                name = wizMod.name,
                chara = wizMod.chara,
                shop_price = "900",
                sort_index = usedId.GetUnusedId_Database(wizMod.sort_index, usedId.customize_item_tbl_index),
                shop_st_day = DateTime.Today.Day,
                shop_st_month = DateTime.Today.Month,
                shop_st_year = DateTime.Today.Year
            };
            return module;
        }

        /// <summary>
        /// Creates the relevant sub-directories for a mod created with the Wizard: "2d", "objset" and "lang2". Returns 'true' if successful.
        /// </summary>
        public static string CreateSubDirectories(string path, string modTitle) // OK
        {
            modFolder = $"{path}\\mods\\{modTitle}";
            if (!Directory.Exists($"{path}\\mods"))
            {
                Directory.CreateDirectory($"{path}\\mods");
            }
            if (Directory.Exists(modFolder))
            {
                Directory.Delete(modFolder, true);
            }
            Directory.CreateDirectory(modFolder);
            Directory.CreateDirectory($"{modFolder}\\rom\\");
            Directory.CreateDirectory($"{modFolder}\\rom\\2d\\");
            Directory.CreateDirectory($"{modFolder}\\rom\\lang2\\");
            Directory.CreateDirectory($"{modFolder}\\rom\\objset\\");
            CreateModConfig(modFolder, modTitle);
            return $"{modFolder}\\rom\\";
        }
        public static void CreateModConfig(string FolderPath, string ModName) //Simple
        {
            using (TextWriter tw = new StreamWriter(FolderPath + "/config.toml"))
            {
                tw.WriteLine($"enabled = true\nname = \"{ModName}\"\nauthor = \"Armoire\"");
                tw.WriteLine("description = \"A mod created using Armoire.\"");
                tw.WriteLine("include = [\".\"]");
            }
        }
        public static string[] ReturnHairNames(string moduleName)
        {
            string[] strings =
            [
                moduleName + "ヘア", //jp
                moduleName + " Hair", //en
                moduleName + "发型", //cn
                moduleName + " - Coupe", //fr
                "Frisur: " + moduleName, //ge
                "Capelli " + moduleName, //it
                moduleName + " 헤어", //kr
                "Pelo " + moduleName, //sp
                "髮型：" + moduleName, //tw
            ];
            return strings;
        }

        public static void WriteLangFile(string modFolderPath, ObservableCollection<Module> modules, ObservableCollection<CustomizeItem> customs)
        {
            using (StreamWriter sw = new(File.Create(modFolderPath + "lang2\\mod_str_array.toml")))
            {
                foreach (Module m in modules)
                {
                    sw.WriteLine($"module.{m.id}=\"{m.name}\"");
                    sw.WriteLine($"module.cn.{m.id}=\"{m.name}\"");
                    sw.WriteLine($"module.fr.{m.id}=\"{m.name}\"");
                    sw.WriteLine($"module.ge.{m.id}=\"{m.name}\"");
                    sw.WriteLine($"module.it.{m.id}=\"{m.name}\"");
                    sw.WriteLine($"module.kr.{m.id}=\"{m.name}\"");
                    sw.WriteLine($"module.sp.{m.id}=\"{m.name}\"");
                    sw.WriteLine($"module.tw.{m.id}=\"{m.name}\"");
                }
                foreach (CustomizeItem custom in customs)
                {
                    if (custom.parts == "KAMI")
                    {
                        string[] names = ReturnHairNames(custom.name);
                        custom.name = names[0];
                        sw.WriteLine($"customize.{custom.id}=\"{names[1]}\"");
                        sw.WriteLine($"customize.cn.{custom.id}=\"{names[2]}\"");
                        sw.WriteLine($"customize.fr.{custom.id}=\"{names[3]}\"");
                        sw.WriteLine($"customize.ge.{custom.id}=\"{names[4]}\"");
                        sw.WriteLine($"customize.it.{custom.id}=\"{names[5]}\"");
                        sw.WriteLine($"customize.kr.{custom.id}=\"{names[6]}\"");
                        sw.WriteLine($"customize.sp.{custom.id}=\"{names[7]}\"");
                        sw.WriteLine($"customize.tw.{custom.id}=\"{names[8]}\"");
                    }
                    else
                    {
                        sw.WriteLine($"customize.{custom.id}=\"{custom.name}\"");
                        sw.WriteLine($"customize.cn.{custom.id}=\"{custom.name}\"");
                        sw.WriteLine($"customize.fr.{custom.id}=\"{custom.name}\"");
                        sw.WriteLine($"customize.ge.{custom.id}=\"{custom.name}\"");
                        sw.WriteLine($"customize.it.{custom.id}=\"{custom.name}\"");
                        sw.WriteLine($"customize.kr.{custom.id}=\"{custom.name}\"");
                        sw.WriteLine($"customize.sp.{custom.id}=\"{custom.name}\"");
                        sw.WriteLine($"customize.tw.{custom.id}=\"{custom.name}\"");
                    }
                }
            }
        }
        /// <summary>
        /// GenerateObjectDatabase creates the obj_db.bin and tex_db.bin, alongside relevant Object and Texture Database entries for all WizardSubObjects in a given WizardObject and renames the farcs.
        /// </summary>
        public static void GenerateObjsetDatabases(WizardObject wizObj, UsedIdSet used, TextureDatabase tex_db, ObjectDatabase obj_db, string modFolderPath, string fileName)
        {
            FarcArchive farc = BinaryFile.Load<FarcArchive>(wizObj.objectFilePath);
            FarcArchive newFarc = new FarcArchive();
            Dictionary<uint, uint> textureIds = new();
            MemoryStream obj_s = new();
            MemoryStream tex_s = new();
            foreach (string file in farc)
            {
                if (file.EndsWith("_obj.bin"))
                {
                    Debug.WriteLine("obj");
                    var stream_o = farc.Open(file, EntryStreamMode.MemoryStream);
                    ObjectSet obj = BinaryFile.Load<ObjectSet>(stream_o);
                    obj.Objects[0].Id = 0;
                    obj.Objects[0].Name = $"{fileName}_skin";
                    foreach (var ob in obj.Objects)
                    {
                        foreach (var mesh in ob.Meshes) // might as well leave this in from kei's batchtristripper - thank you bestie for this part!!
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
                        foreach (Material m in ob.Materials)
                        {
                            foreach (MaterialTexture mt in m.MaterialTextures)
                            {
                                if (!textureIds.ContainsKey(mt.TextureId))
                                {
                                    uint newId = used.GetUnusedId_Database(mt.TextureId, used.tex_db);
                                    textureIds.Add(mt.TextureId, newId);
                                    mt.TextureId = newId;
                                    used.tex_db.Add(newId, "Armoire Wizard");
                                }
                                else
                                {
                                    mt.TextureId = textureIds[mt.TextureId];
                                }
                            }
                        }
                    }
                    for (int i = 0; i < obj.TextureIds.Count; i++)
                    {
                        if (textureIds.ContainsKey(obj.TextureIds[i]))
                        {
                            obj.TextureIds[i] = textureIds[obj.TextureIds[i]];
                            tex_db.Textures.Add(new()
                            {
                                Name = $"{fileName.ToUpper()}_AUTO_TEXTURE_{i}",
                                Id = obj.TextureIds[i],
                            });

                        }
                        else
                        {
                            uint newId = used.GetUnusedId_Database(obj.TextureIds[i], used.tex_db);
                            textureIds.Add(obj.TextureIds[i], newId);
                            obj.TextureIds[i] = newId;
                            used.tex_db.Add(newId, "Armoire Wizard");
                        }
                    }
                    ObjectSetInfo objSet = new();
                    objSet.Id = used.GetUnusedID_Maximum(used.obj_db, 19999);
                    objSet.Name = fileName.ToUpper();
                    objSet.ArchiveFileName = fileName + ".farc";
                    objSet.FileName = fileName + "_obj.bin";
                    objSet.TextureFileName = fileName + "_tex.bin";
                    foreach(var ob in obj.Objects)
                    {
                        objSet.Objects.Add(new ObjectInfo()
                        {
                            Id = ob.Id,
                            Name = ob.Name.ToUpper()
                        });
                    }
                    obj_db.ObjectSets.Add(objSet);
                    obj.Save(obj_s, true);
                    newFarc.Add($"{fileName}_obj.bin", obj_s, true, ConflictPolicy.Replace);
                    Debug.WriteLine("obj ok");
                }
                if (file.EndsWith("_tex.bin"))
                {
                    var stream_t = farc.Open(file, EntryStreamMode.MemoryStream);
                    newFarc.Add($"{fileName}_tex.bin", stream_t, true, ConflictPolicy.Replace);
                    Debug.WriteLine("tex ok");
                }
            }
            //farc.Dispose();
            newFarc.Save($"{modFolderPath}objset\\{fileName}.farc");
            newFarc.Dispose();
        }

        public static async Task WizardCreate(StackPanel itemHost, bool useDMA, string modName)
        {
            ObservableCollection<Module> exportModules = new();
            ObservableCollection<CustomizeItem> exportCustoms = new();
            SpriteDatabase spr_db = new();
            Dictionary<string, CharacterItemFile> exportCharacters = CreateCharacterItemFile();
            string modFolderPath = CreateSubDirectories(Settings.Default.gamePath, modName);
            UsedIdSet usedId = new();
            await Task.Run(() => usedId.ProcessLocalFiles());
            await usedId.GetUsedModIds(useDMA);
            if (itemHost.Children.Count > 0) //check if entries are actually there
            {
                ObjectDatabase obj_db = new();
                TextureDatabase tex_db = new(); //create databases before processing each entry so that they are combined into one mod.
                foreach (WizardInfo wizInfo in itemHost.Children) //each module/customize item
                {
                    CharacterCostumeEntry costume = new();
                    costume.items.Clear();
                    // objset+texset generating
                    int exportId = usedId.GetUnusedId_Regular(wizInfo.wizMod.id, wizInfo.wizMod.chara);
                    wizInfo.finalizeModule();
                    if (!wizInfo.wizMod.isItem)
                    {
                        costume.id = usedId.GetUnusedID_Maximum(usedId.chritm_prop_cos[wizInfo.wizMod.chara.Remove(3)], 1000000);
                    }
                    foreach (WizardObject wizObj in wizInfo.wizMod.objects)
                    {
                        if (!wizObj.isExisting)
                        {
                            string fileName = $"{wizInfo.wizMod.chara.Remove(3).ToLower()}itm{exportId}_{wizObj.item.subID}";
                            if (wizInfo.wizMod.isItem)
                            {
                                fileName = $"cmnitm{exportId}_{wizObj.item.subID}";
                            }
                            if (wizObj.item.subID != 24)
                            {
                                wizObj.item.uid = $"{fileName}_skin";
                            }
                            else
                            {
                                wizObj.item.uid = "NULL";
                            }
                            wizObj.item.name = fileName;
                            wizObj.item.objset = new() { fileName.ToUpper() };
                            foreach (DataSetTex tex in wizObj.item.dataSetTex)
                            {
                                if (tex.chg.Contains("ARMOIREREPLACE"))
                                {
                                    tex.chg = tex.chg.Replace("ARMOIREREPLACE", fileName.ToUpper());
                                }
                            }
                            if (wizInfo.wizMod.isItem)
                            {
                                wizObj.item.no = usedId.GetUnusedID_CustomizeItem(wizInfo.wizMod.id);
                                foreach (CharacterItemFile charaFile in exportCharacters.Values)
                                {
                                    charaFile.items.Add(wizObj.item);
                                }
                                CustomizeItem cust = CreateCustomiseItem(wizInfo.wizMod, usedId, exportId, wizObj.item.no);
                                exportCustoms.Add(cust);
                            }
                            else
                            {
                                wizObj.item.no = usedId.GetUnusedID_Maximum(usedId.chritm_prop_item[wizInfo.wizMod.chara.Remove(3)], 1000000);
                                exportCharacters[wizInfo.wizMod.chara.Remove(3)].items.Add(wizObj.item);
                                costume.items.Add(wizObj.item.no);
                                if (wizObj.item.subID == 1)
                                {
                                    CustomizeItem cust = CreateCustomiseItem(wizInfo.wizMod, usedId, exportId, wizObj.item.no);
                                    exportCustoms.Add(cust);
                                }
                            }
                            GenerateObjsetDatabases(wizObj, usedId, tex_db, obj_db, modFolderPath, fileName);
                        }
                    }
                    if (!wizInfo.wizMod.isItem)
                    {
                        foreach (CharacterItemEntry existingItem in wizInfo.wizMod.existingItems)
                        {
                            costume.items.Add(existingItem.no);
                            Debug.WriteLine("Added to cos");
                        }
                        exportCharacters[wizInfo.wizMod.chara.Remove(3)].costumes.Add(costume);
                        Debug.WriteLine("Added cos to exportCharacters");
                        Module module = CreateModule(wizInfo.wizMod, usedId, exportId, costume.id);
                        if (wizInfo.wizMod.hairNG)
                        {
                            module.attr = Attr.NoSwap_CT;
                        }
                        exportModules.Add(module);
                    }
                    //sprite
                    spr_db.SpriteSets.Add(ExportSprite(wizInfo.wizMod.bitmap, exportId, modFolderPath + "2d\\", wizInfo.wizMod.isItem, usedId));
                }
                if (exportModules.Count > 0)
                {
                    Program.IO.SaveFile<Module>(modFolderPath + "mod_gm_module_tbl.farc", exportModules);
                }
                WriteLangFile(modFolderPath, exportModules, exportCustoms);
                if (exportCustoms.Count > 0)
                {
                    Program.IO.SaveFile<CustomizeItem>(modFolderPath + "mod_gm_customize_item_tbl.farc", exportCustoms);
                }
                ObservableCollection<CharacterItemFile> exportChara = new();
                foreach (CharacterItemFile chritmFile in exportCharacters.Values)
                {
                    exportChara.Add(chritmFile);
                }
                Program.IO.SaveChr($"{modFolderPath}mod_chritm_prop.farc", exportChara);
                Debug.WriteLine("Saved chritm");
                spr_db.Save($"{modFolderPath}2d\\mod_spr_db.bin");
                if (obj_db.ObjectSets.Count > 0)
                {
                    obj_db.Save($"{modFolderPath}objset\\mod_obj_db.bin");
                }
                if (tex_db.Textures.Count > 0)
                {
                    tex_db.Save($"{modFolderPath}objset\\mod_tex_db.bin");
                }
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    PopupNotification popup = new("The Wizard has finished creating your items.");
                });
            }
        }
    }
}
