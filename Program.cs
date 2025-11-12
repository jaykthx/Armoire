using Armoire.Dialogs;
using Armoire.Properties;
using Microsoft.Win32;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Sprites;
using MikuMikuLibrary.Textures;
using MikuMikuLibrary.Textures.Processing;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Armoire
{
    public class Program
    {
        public static readonly DateTime versionDate = new DateTime(2025,11,12);
        public static string modulePath;
        public static Downloader downloader = new();
        public static string customPath;
        public static string charaPath;
        public readonly static List<string> charas = ["MIKU", "RIN", "LEN", "LUKA", "KAITO", "MEIKO", "NERU", "HAKU", "SAKINE", "TETO"];
        public static DateTime dmaLastUpdate
        {
            get
            {
                return Settings.Default.lastDmaUpdate;
            }
            set
            {
                Settings.Default.lastDmaUpdate = value;
                Settings.Default.Save();
            }
        }

        public class Wizard
        {
            public static void SetModuleImage(Bitmap pngFile, System.Windows.Controls.Image image)
            {
                Sprite spr = GetSprite(false);
                SpriteSet sprite = new();
                sprite.Sprites.Add(spr);
                Bitmap newBitmap = new(pngFile);
                newBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                Texture text = TextureEncoder.EncodeFromBitmap(newBitmap, TextureFormat.DXT5, true);
                sprite.TextureSet.Textures.Add(text);
                Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
                BitmapImage img = ToBitmapImage(cropSprite);
                image.Source = img;
            }
            /*public static string ProcessDirectories(string folderName)
            {
                string finalFolder = Settings.Default.gamePath + "/mods/" + folderName + "/rom/";
                Directory.CreateDirectory(finalFolder);
                Directory.CreateDirectory(finalFolder + "2d");
                Directory.CreateDirectory(finalFolder + "objset");
                Directory.CreateDirectory(finalFolder + "lang2");
                if (File.Exists(finalFolder + "/lang2/mod_str_array.toml"))
                {
                    File.Delete(finalFolder + "/lang2/mod_str_array.toml");
                }
                return finalFolder;
            }
            public static string ReturnSubIDString(int subID)
            {
                switch (subID)
                {
                    case 0:
                        return Resources.item_headacc;
                    case 4:
                        return Resources.item_faceacc;
                    case 8:
                        return Resources.item_chestacc;
                    case 16:
                        return Resources.item_backacc;
                    case 1:
                        return Resources.item_hair;
                    case 10:
                        return Resources.item_body;
                    case 14:
                        return Resources.item_hands;
                    case 6:
                        return Resources.item_contact;
                    case 24:
                        return Resources.item_head;
                    default:
                        return Resources.item_default;
                }
            }
            

            public static void AddToModuleTable(WizardEntry wizMod, UsedIdSet used, ObservableCollection<Module> modules, SpriteDatabase spr_db)
            {
                Module temp = new();
                if (wizMod.hairNG)
                {
                    temp.attr = Attr.NoSwap_CT;
                }
                else
                {
                    temp.attr = Attr.Default_CT;
                }
                temp.chara = wizMod.chara;
                if (Program.Databases.CheckID(used.module_tbl, wizMod.id) == false)
                {
                    used.module_tbl.Add(temp.id);
                }
                else
                {
                    ChoiceWindow win = new(Properties.Resources.warn_used_0 + Properties.Resources.cmn_item_nofull + Properties.Resources.warn_used_1 + "\n" +
                        Properties.Resources.warn_used_offer, Properties.Resources.cmn_no, Properties.Resources.cmn_yes);
                    win.ShowDialog();
                    if (win.isRightClicked)
                    {
                        wizMod.id = Program.Databases.GetUnusedID(used.module_tbl, used.customize_item_tbl);
                        used.module_tbl.Add(wizMod.id);
                    }
                }
                temp.id = wizMod.id;
                temp.cos = "COS_" + Program.Databases.GetIDString((temp.id + 1).ToString());
                temp.name = wizMod.name;
                if (Program.Databases.CheckID(used.module_tbl_index, wizMod.sort_index) == false)
                {
                    used.module_tbl_index.Add(wizMod.sort_index);
                }
                else
                {
                    ChoiceWindow win = new(Properties.Resources.warn_used_0 + Properties.Resources.cmn_index + Properties.Resources.warn_used_1 + "\n" +
                        Properties.Resources.warn_used_offer, Properties.Resources.cmn_item_no, Properties.Resources.cmn_yes);
                    win.ShowDialog();
                    if (win.isRightClicked)
                    {
                        wizMod.sort_index = Program.Databases.GetUnusedID(used.module_tbl_index);
                        used.module_tbl_index.Add(wizMod.sort_index);
                    }
                }
                temp.sort_index = wizMod.sort_index;
                temp.shop_price = "900";
                temp.shop_st_day = 1;
                temp.shop_st_month = 1;
                temp.shop_st_year = 2009;
                Program.Databases.AddToSpriteDatabase(spr_db, wizMod.id, false, used.spr_db);
                modules.Add(temp);
            }
            public static void AddItemToCustomizeTable(WizardEntry wizMod, int item_no, UsedIdSet used)
            {
                CustomizeItem temp;
                if (wizMod.isItem)
                {
                    temp = new()
                    {
                        bind_module = -1,
                        id = wizMod.id,
                        name = wizMod.name,
                        parts = wizMod.parts,
                        chara = "ALL",
                        obj_id = item_no,
                        ng = false,
                        shop_price = "300",
                        shop_st_day = 1,
                        shop_st_month = 1,
                        shop_st_year = 2009
                    };
                }
                else
                {
                    temp = new()
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
                }
                if (Program.Databases.CheckID(used.customize_item_tbl, wizMod.id) == false)
                {
                    used.customize_item_tbl.Add(temp.id);
                }
                else
                {
                    Program.NotiBox(Properties.Resources.warn_used_0 + Properties.Resources.cmn_id + Properties.Resources.warn_used_1, Properties.Resources.window_notice);
                }

                if (Program.Databases.CheckID(used.customize_item_tbl_index, wizMod.sort_index) == false)
                {
                    used.customize_item_tbl_index.Add(wizMod.sort_index);
                }
                else
                {
                    Program.NotiBox(Properties.Resources.warn_used_0 + Properties.Resources.cmn_index + Properties.Resources.warn_used_1, Properties.Resources.window_notice);
                }
                temp.sort_index = wizMod.sort_index;
                Program.Databases.AddToSpriteDatabase(spr_db, wizMod.id, true, used.spr_db);
                used.Add(temp);
            }

            public static ObjectSetInfo CreateObjInfo(WizardObject wiz, string chara, UsedIdSet used, string exportFolder)
            {
                ObjectSetInfo objSetInfo = new();
                var farc = BinaryFile.Load<FarcArchive>(wiz.objectFilePath);
                string subName = chara.ToUpper() + "ITM" + Program.Databases.GetUnusedID(used.chritm_prop_item[Program.Databases.GetChritmName(chara)]);
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
                        foreach (Object hObj in headObj)
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
                        Program.Wizard.ProcessTextures(objset, mainName, used, tex_db);
                        objset.Save(stream, true);
                        farc.Add(objSetInfo.FileName, stream, false, ConflictPolicy.Replace);
                        farc.Save(exportFolder + "/objset/" + objSetInfo.ArchiveFileName);
                    }
                }
                farc.Dispose();
                newFarc.Dispose();
                return objSetInfo;
            }

            public static void AddToCharaItemTable(ModuleInfo modInfo)
            {
                List<ObjectSetInfo> entries = new(); // this is where we're storing the ObjSetInfo classes we create below so they can be used for chritm_tbl generating ig?
                if (!modInfo.wizMod.isItem)
                {
                    foreach (WizardObject wiz in modInfo.wizMod.objects)
                    {
                        wiz.objectSet = CreateObjInfo(wiz, Program.Databases.GetChritmName(modInfo.CharacterBox.Text.ToUpper()));
                        entries.Add(wiz.objectSet);
                    }
                }
                else
                {
                    modInfo.wizMod.objects[0].objectSet = CreateObjInfo(modInfo.wizMod.objects[0], "CMN");
                    entries.Add(modInfo.wizMod.objects[0].objectSet);
                }
                foreach (ObjectSetInfo o in entries)
                {
                    o.Id = Program.Databases.GetUnusedID(finalUsedIDs.obj_db, 19999);
                    finalUsedIDs.obj_db.Add(o.Id);
                    obj_db.ObjectSets.Add(o);
                }
                if (!modInfo.wizMod.isItem)
                {
                    CharacterCostumeEntry cos = new()
                    {
                        id = modInfo.wizMod.id,
                        items = new()
                    };
                    bool containsHands = false;
                    foreach (CharacterItemEntry x in modInfo.wizMod.existingItems)
                    {
                        cos.items.Add(x.no);
                        if (x.subID == 14)
                        {
                            containsHands = true;
                        }
                    }
                    foreach (WizardObject o in modInfo.wizMod.objects)
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
                    foreach (WizardObject x in modInfo.wizMod.objects)
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
                        list[modInfo.CharacterBox.SelectedIndex].items.Add(x.item);
                        cos.items.Add(x.item.no);
                        if (x.item.subID == 1 && !modInfo.wizMod.hairNG)
                        {
                            AddItemToCustomizeTable(modInfo.wizMod, x.item.no);
                        }
                    }
                    list[modInfo.CharacterBox.SelectedIndex].costumes.Add(cos);
                }
                else
                {
                    modInfo.wizMod.objects[0].item.name = modInfo.wizMod.objects[0].objectSet.Name + " " + Program.Wizard.ReturnSubIDString(modInfo.wizMod.objects[0].item.subID);
                    modInfo.wizMod.objects[0].item.uid = modInfo.wizMod.objects[0].objectSet.Objects[0].Name;
                    for (int x = 0; x < Program.charas.Count; x++)
                    {
                        int itm_num = Program.Wizard.GetItemNumber(modInfo.wizMod.objects[0].objectFilePath, Program.charas[x], );
                        modInfo.wizMod.objects[0].item.no = itm_num;
                        list[x].items.Add(modInfo.wizMod.objects[0].item);
                    }
                    AddItemToCustomizeTable(modInfo.wizMod, modInfo.wizMod.objects[0].item.no);
                }
            }
            }*/
        }

        public class IO
        {
            public static async Task DMAUpdate()
            {
                string dma = "https://divamodarchive.com";
                while (!Directory.Exists(Settings.Default.dmaPath))
                {
                    OpenFolderDialog ofd = new();
                    if (ofd.ShowDialog() == true)
                    {
                        Settings.Default.dmaPath = ofd.FolderName;
                        Directory.CreateDirectory(Settings.Default.dmaPath);
                        Settings.Default.Save();
                    }
                }
                var dl1 = downloader.DownloadFileASync($"{dma}/api/v1/ids/all_modules", Settings.Default.dmaPath, "modules.ini");
                var dl2 = downloader.DownloadFileASync($"{dma}/api/v1/ids/all_textures", Settings.Default.dmaPath, "textures.ini");
                var dl3 = downloader.DownloadFileASync($"{dma}/api/v1/ids/all_objsets", Settings.Default.dmaPath, "objects.ini");
                var dl4 = downloader.DownloadFileASync($"{dma}/api/v1/ids/all_cstm_items", Settings.Default.dmaPath, "cstm_items.ini");
                var dl5 = downloader.DownloadFileASync($"{dma}/api/v1/ids/all_sprite_sets", Settings.Default.dmaPath, "sprite_sets.ini");
                var dl6 = downloader.DownloadFileASync($"{dma}/api/v1/ids/all_sprites", Settings.Default.dmaPath, "sprites.ini");
                var dl7 = downloader.DownloadFileASync($"{dma}/api/v1/posts?limit="+UInt32.MaxValue, Settings.Default.dmaPath, "posts.ini");
                await Task.WhenAll(dl1, dl2, dl3, dl4, dl5, dl6, dl7);
                Program.dmaLastUpdate = DateTime.Now;
            }
            public static async Task<ObservableCollection<CharacterItemFile>> OpenCharacterItemFile(string[] fileNames)
            {
                ObservableCollection<CharacterItemFile> fullTemp = new();
                foreach (string file in fileNames)
                {
                    if (file.EndsWith("chritm_prop.farc"))
                    {
                        var farc = BinaryFile.Load<FarcArchive>(file);
                        if (fullTemp.Count == 0)
                        {
                            ObservableCollection<CharacterItemFile> temp = new(await Program.IO.ReadCharaFile(farc));
                            foreach (CharacterItemFile chr in temp)
                            {
                                fullTemp.Add(chr);
                            }
                        }
                        else
                        {
                            ObservableCollection<CharacterItemFile> temp = new(await Program.IO.ReadCharaFile(farc));
                            foreach (CharacterItemFile tempchr in temp)
                            {
                                foreach (CharacterItemFile chr in fullTemp)
                                {
                                    if (tempchr.chara == chr.chara)
                                    {
                                        foreach (CharacterCostumeEntry cos in tempchr.costumes)
                                        {
                                            chr.costumes.Add(cos);
                                        }
                                        foreach (CharacterItemEntry item in tempchr.items)
                                        {
                                            chr.items.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                charaPath = fileNames[0];
                return fullTemp;
            }
            public static void DeleteDirectory(string target_dir) // Credit - Jeremy Edwards on stackoverflow
            {
                string[] files = Directory.GetFiles(target_dir);
                string[] dirs = Directory.GetDirectories(target_dir);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }
                Directory.Delete(target_dir, false);
            }
            public async static Task<ObservableCollection<CharacterItemFile>> ReadCharaFile(FarcArchive farc)
            {
                ObservableCollection<CharacterItemFile> temp = new();
                Dictionary<string, string> files = new();
                foreach (var fileName in farc)
                {
                    if (fileName.EndsWith("_tbl.txt"))
                    {
                        string name = fileName.Remove(3);
                        var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                        using (StreamReader sr = new(source))
                        {
                            files.Add(name, await sr.ReadToEndAsync());
                        }
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate {
                            PopupNotification popup = new(Resources.exp_5 + fileName.ToString());
                        });
                    }
                }
                farc.Dispose();
                foreach (var x in files.Keys)
                {
                    string[] vs = files[x].Split(['\n', '\r']);
                    temp.Add(ReadList(vs, x));
                }
                return temp;
            }

            public static CharacterItemFile ReadList(string[] content, string name)
            {
                List<CharacterCostumeEntry> tempCosList = new();
                List<CharacterItemEntry> tempItemList = new();
                ObservableCollection<DataSetTex> temp = new();
                CharacterCostumeEntry readEntry = new();
                List<int> tempItems = new();
                CharacterItemEntry itemEntry = new();
                DataSetTex setTex = new();
                List<string> list = new();
                foreach (string fileLine in content)
                {
                    string[] split = fileLine.Split('=');
                    if (fileLine.Contains("cos."))
                    {
                        if (fileLine.Contains(".id="))
                        {
                            readEntry.id = int.Parse(split[1]);
                        }
                        if (fileLine.Contains("item.") && !fileLine.Contains("length"))
                        {
                            tempItems.Add(int.Parse(split[1]));
                        }
                        if (fileLine.Contains("item.length"))
                        {
                            readEntry.items = new ObservableCollection<int>(tempItems);
                            tempCosList.Add(readEntry);
                            readEntry = new CharacterCostumeEntry();
                            tempItems = new List<int>();
                        }
                    }
                    if (fileLine.Contains("item.") && !fileLine.Contains("cos."))
                    {
                        if (fileLine.Contains(".attr="))
                        {
                            itemEntry.attr = Int32.Parse(split[1]);
                        }
                        if (fileLine.Contains(".data.obj.0.rpk="))
                        {
                            itemEntry.rpk = Int32.Parse(split[1]);
                        }
                        if (fileLine.Contains(".data.obj.0.uid="))
                        {
                            itemEntry.uid = split[1];
                        }
                        if (fileLine.Contains(".chg="))
                        {
                            setTex.chg = split[1];
                        }
                        if (fileLine.Contains(".org="))
                        {
                            setTex.org = split[1];
                            itemEntry.flag = 4;
                            temp.Add(setTex);
                            setTex = new DataSetTex();
                        }
                        if (fileLine.Contains(".des_id="))
                        {
                            itemEntry.desID = int.Parse(split[1]);
                        }
                        if (fileLine.Contains(".face_depth="))
                        {
                            itemEntry.face_depth = decimal.Parse(split[1]);
                        }
                        if (fileLine.Contains(".flag="))
                        {
                            itemEntry.flag = Int32.Parse(split[1]);
                        }
                        if (fileLine.Contains(".name="))
                        {
                            itemEntry.name = split[1];
                        }
                        if (fileLine.Contains(".no="))
                        {
                            itemEntry.no = int.Parse(split[1]);
                        }
                        if (fileLine.Contains(".objset") && !fileLine.Contains("length"))
                        {
                            list.Add(split[1]);
                        }
                        if (fileLine.Contains(".org_itm="))
                        {
                            itemEntry.orgItm = int.Parse(split[1]);
                        }
                        if (fileLine.Contains(".sub_id="))
                        {
                            itemEntry.subID = int.Parse(split[1]);
                        }
                        if (fileLine.Contains(".type="))
                        {
                            itemEntry.objset = list;
                            itemEntry.type = int.Parse(split[1]);
                            itemEntry.dataSetTex = temp;
                            temp = new ObservableCollection<DataSetTex>();
                            tempItemList.Add(itemEntry);
                            list = new List<string>();
                            itemEntry = new CharacterItemEntry();
                        }
                    }
                }
                CharacterItemFile x = new()
                {
                    chara = name,
                    costumes = tempCosList.OrderBy(o => o.id).ToList(),
                    items = tempItemList.OrderBy(o => o.no).ToList()
                };
                return x;
            }

            public async static Task<ObservableCollection<Module>> ReadModuleFile(FarcArchive farc)
            {
                var source = farc.Open("gm_module_id.bin", EntryStreamMode.MemoryStream);
                ObservableCollection<Module> list = new();
                string content;
                using (var sr = new StreamReader(source, Encoding.UTF8))
                {
                    content = await sr.ReadToEndAsync();
                }
                farc.Dispose();
                var readEntry = new Module();
                foreach (string line in content.Split(['\n', '\r']))
                {
                    if (line.Contains("module."))
                    {
                        string[] containString = line.Split('=');
                        switch (containString[0])
                        {
                            case string a when a.Contains(".attr"):
                                Enum.TryParse(containString[1], out Attr value);
                                readEntry.attr = value;
                                break;
                            case string a when a.Contains(".chara"):
                                readEntry.chara = containString[1];
                                break;
                            case string a when a.Contains(".cos"):
                                readEntry.cos = containString[1];
                                break;
                            case string a when a.Contains(".id"):
                                readEntry.id = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains(".name"):
                                readEntry.name = containString[1];
                                break;
                            case string a when a.Contains(".ng"):
                                readEntry.ng = Convert.ToBoolean(Int32.Parse(containString[1]));
                                break;
                            case string a when a.Contains("shop_price"):
                                readEntry.shop_price = containString[1];
                                break;
                            case string a when a.Contains("shop_st_day"):
                                readEntry.shop_st_day = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains("shop_st_month"):
                                readEntry.shop_st_month = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains("shop_st_year"):
                                readEntry.shop_st_year = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains("sort_index"):
                                readEntry.sort_index = Int32.Parse(containString[1]);
                                list.Add(readEntry);
                                readEntry = new();
                                break;
                        }
                    }
                }
                return new ObservableCollection<Module>(list.OrderBy(x => x.id).ToList());
            }

            public async static Task<ObservableCollection<CustomizeItem>> ReadCustomFile(FarcArchive farc)
            {
                ObservableCollection<CustomizeItem> temp = new();
                var source = farc.Open("gm_customize_item_id.bin", EntryStreamMode.MemoryStream);
                string content;
                using (var sr = new StreamReader(source, Encoding.UTF8))
                {
                    content = await sr.ReadToEndAsync();
                }
                farc.Dispose();
                var readEntry = new CustomizeItem();
                foreach (string fileLine in content.Split(['\n', '\r']))
                {
                    string[] containString;
                    if (fileLine.Contains("cstm_item."))
                    {
                        containString = fileLine.Split('=');
                        switch (containString[0])
                        {
                            case string a when a.Contains(".bind_module"):
                                readEntry.bind_module = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains(".chara"):
                                readEntry.chara = containString[1];
                                break;
                            case string a when a.Contains(".id"):
                                readEntry.id = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains(".name"):
                                readEntry.name = containString[1];
                                break;
                            case string a when a.Contains(".ng"):
                                int store = int.Parse(containString[1]);
                                readEntry.ng = Convert.ToBoolean(store);
                                break;
                            case string a when a.Contains(".obj_id"):
                                readEntry.obj_id = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains(".sell_type"):
                                int boolVal2 = int.Parse(containString[1]);
                                readEntry.sell_type = Convert.ToBoolean(boolVal2);
                                break;
                            case string a when a.Contains(".parts"):
                                readEntry.parts = containString[1];
                                break;
                            case string a when a.Contains("shop_price"):
                                readEntry.shop_price = containString[1];
                                break;
                            case string a when a.Contains("shop_st_day"):
                                readEntry.shop_st_day = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains("shop_st_month"):
                                readEntry.shop_st_month = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains("shop_st_year"):
                                readEntry.shop_st_year = Int32.Parse(containString[1]);
                                break;
                            case string a when a.Contains("sort_index"):
                                readEntry.sort_index = Int32.Parse(containString[1]);
                                temp.Add(readEntry);
                                readEntry = new CustomizeItem();
                                break;
                        }
                    }
                }
                return new ObservableCollection<CustomizeItem>(temp.OrderBy(x => x.id).ToList()); ;
            }

            public static void SaveFile<T>(string path, ObservableCollection<T> list) where T : Entry
            {
                if (path.Length > 0)
                {
                    int count = 0;
                    var farc = new FarcArchive();
                    MemoryStream outputSource = new();
                    using (StreamWriter tw = new(outputSource))
                    {
                        tw.AutoFlush = true;
                        foreach (T entry in list)
                        {
                            entry.entry = count.ToString();
                            count++;
                        }
                        foreach (T entry in list.OrderBy(x => x.entry)) //order by entry or the file does not save correctly!! (incorrect saving causes bugs in the game!!)
                        {
                            foreach (string x in entry.getEntry())
                            {
                                tw.WriteLine(x);
                            }
                        }
                        if (typeof(T) == typeof(Module))
                        {
                            tw.WriteLine(nameof(Module).ToLower() + ".data_list.length=" + list.Count);
                            farc.Add("gm_module_id.bin", outputSource, true, ConflictPolicy.Replace);
                        }
                        if (typeof(T) == typeof(CustomizeItem))
                        {
                            tw.WriteLine("cstm_item" + ".data_list.length=" + list.Count);
                            farc.Add("gm_customize_item_id.bin", outputSource, true, ConflictPolicy.Replace);
                        }
                        farc.IsCompressed = true;
                        farc.Save(path);
                        tw.Close();
                    }
                    farc.Dispose();
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        PopupNotification popup = new("Saved successfully.");
                    });
                }
                else { NotiBox(Resources.exp_4, Resources.cmn_error); }
            }
            public static void SaveChr(string path, ObservableCollection<CharacterItemFile> list)
            {
                if (path.Length > 0)
                {
                    var farc = new FarcArchive();
                    foreach (CharacterItemFile x in list)
                    {
                        x.chara = x.chara.ToLower();
                        MemoryStream outputSource = new();
                        using (StreamWriter tw = new(outputSource))
                        {
                            tw.AutoFlush = true;
                            int count = 0;
                            if (x.costumes.Count > 0)
                            {
                                foreach (CharacterCostumeEntry entry in x.costumes)
                                {
                                    entry.entry = count.ToString();
                                    count++;
                                }

                                foreach (CharacterCostumeEntry c in x.costumes.OrderBy(o => o.entry))
                                {
                                    foreach (string w in c.getEntry())
                                    {
                                        tw.WriteLine(w);
                                    }
                                }
                            }
                            tw.WriteLine("cos.length=" + x.costumes.Count);
                            count = 0;
                            foreach (CharacterItemEntry entry in x.items)
                            {
                                entry.entry = count.ToString();
                                count++;
                            }
                            foreach (CharacterItemEntry i in x.items.OrderBy(o => o.entry))
                            {
                                foreach (string w in i.GetEntry())
                                {
                                    tw.WriteLine(w);
                                }
                            }
                            tw.WriteLine("item.length=" + x.items.Count);
                            if(!(x.items.Count == 0 && x.costumes.Count == 0))
                            {
                                farc.Add(x.GetFileName(), outputSource, true, ConflictPolicy.Replace);
                            }
                            farc.IsCompressed = true;
                            farc.Save(path);
                            tw.Close();
                        }
                    }
                    farc.Dispose();
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        PopupNotification popup = new("Saved successfully.");
                    });
                }
                else { NotiBox(Resources.exp_4, Resources.cmn_error); }
            }
        }
        /// <summary>
        /// Opens a notification window with a main body of text and a title.
        /// </summary>
        public static void NotiBox(string text, string title)
        {
            NotiBox noti = new(text, title);
            noti.ShowDialog();
        }

        public class Databases
        {
            public static string GetIDString(string id)
            {
                StringBuilder sb = new(id);
                while (sb.Length < 3)
                {
                    sb.Insert(0, "0");
                }
                return sb.ToString();
            }
            public static int GetUnusedID(List<int> used) // no limit version
            {
                Random rnd = new();
                int final = rnd.Next();
                while (used.Contains(final))
                {
                    final = rnd.Next();
                }
                return final;
            }
            public static int GetUnusedID(List<int> used1, List<int> used2) // 2 list (int) version
            {
                Random rnd = new();
                int final = rnd.Next();
                while (used1.Contains(final) && used2.Contains(final))
                {
                    final = rnd.Next();
                }
                return final;
            }
            public static uint GetUnusedID(List<uint> used) // no limit version
            {
                Random rnd = new();
                int final = rnd.Next();
                while (used.Contains((uint)final))
                {
                    final = rnd.Next();
                }
                return (uint)final;
            }
            public static uint GetUnusedID(List<uint> used, int maximum) // obj_db, tex_db, spr_db
            {
                Random rnd = new();
                int final = rnd.Next(maximum);
                while (used.Contains((uint)final))
                {
                    final = rnd.Next(maximum);
                }
                return (uint)final;
            }
            public static bool CheckID(List<int> used, int check) // all the OTHER shit
            {
                if (used.Contains(check))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static void AddToSpriteDatabase(SpriteDatabase spr_db, int id, bool isCustomize, List<uint> used_spr_IDs)
            {
                SpriteSetInfo sprSetInfo = new()
                {
                    Id = GetUnusedID(used_spr_IDs)
                };
                used_spr_IDs.Add(sprSetInfo.Id);
                SpriteInfo sprInfo = new()
                {
                    Id = GetUnusedID(used_spr_IDs)
                };
                used_spr_IDs.Add(sprInfo.Id);
                SpriteTextureInfo sprTexInfo = new()
                {
                    Id = GetUnusedID(used_spr_IDs),
                    Name = "MERGE_BC5COMP_0"
                };
                used_spr_IDs.Add(sprTexInfo.Id);
                if (isCustomize)
                {
                    sprSetInfo.Name = "SPR_CMNITM_THMB" + GetIDString(id.ToString()) + "";
                    sprSetInfo.FileName = "spr_cmnitm_thmb" + GetIDString(id.ToString()) + ".bin";
                    sprInfo.Name = "SPR_CMNITM_THMB" + GetIDString(id.ToString()) + "_ITM_IMG";

                }
                else
                {
                    sprSetInfo.Name = "SPR_SEL_MD" + GetIDString(id.ToString()) + "CMN";
                    sprSetInfo.FileName = "spr_sel_md" + GetIDString(id.ToString()) + "cmn.bin";
                    sprInfo.Name = "SPR_SEL_MD" + GetIDString(id.ToString()) + "CMN_MD_IMG";
                }
                sprSetInfo.Sprites.Add(sprInfo);
                sprSetInfo.Textures.Add(sprTexInfo);
                spr_db.SpriteSets.Add(sprSetInfo);
            }
            public static string GetChritmName(string x)
            {
                switch (x)
                {
                    case "MIKU":
                        return "mik";
                    case "RIN":
                        return "rin";
                    case "LEN":
                        return "len";
                    case "LUKA":
                        return "luk";
                    case "KAITO":
                        return "kai";
                    case "MEIKO":
                        return "mei";
                    case "SAKINE":
                        return "sak";
                    case "NERU":
                        return "ner";
                    case "HAKU":
                        return "hak";
                    case "TETO":
                        return "tet";
                    default:
                        return "tet";
                }
            }
        }
        public static Sprite GetSprite(bool isCustomize)
        {
            Sprite spr = new()
            {
                Width = 408,
                Height = 494, // official width + height
                TextureIndex = 0
            };
            if (isCustomize)
            {
                spr.Name = "ITM_IMG";
            }
            else
            {
                spr.Name = "MD_IMG";
            }
            spr.X = 2;
            spr.Y = 2;
            spr.ResolutionMode = ResolutionMode.HDTV1080;
            return spr;
        }
        
        public class ItemPreset
        {
            public int attr;
            public int subid;
            public int desid;
            public int rpk;
            public int type;
            public int orgitm;
            public int flag;
            public decimal face_depth;

            public ItemPreset(int v1, int v2, int v3, int v4, int v5, int v6, int v7, decimal v8)
            {
                attr = v1;
                subid = v2;
                desid = v3;
                rpk = v4;
                type = v5;
                orgitm = v6;
                flag = v7;
                face_depth = v8;
            }
        }

        public readonly static List<ItemPreset> itemPresets =
        [
            new(37, 24, 0, 1, 2, 0, 0, (decimal)0.00), // eyetex
            new(1, 6, 1, -1, 0, 0, 0, (decimal)0.00), // lenses
            new(1, 1, 0, -1, 0, 0, 0, (decimal)0.00), // hair
            new(1, 10, 2, -1, 1, 0, 0, (decimal)0.00), // body
            new(1, 14, 2, -1, 0, 0, 0, (decimal)0.00), // hands
            new(1, 0, 0, -1, 0, 0, 0, (decimal)0.00), // head accessory
            new(1, 4, 1, -1, 0, 0, 0, (decimal)0.00), // face accessory
            new(1, 8, 2, -1, 0, 0, 0, (decimal)0.00), // chest accessory
            new(1, 16, 2, -1, 0, 0, 0, (decimal)0.00), // back accessory
            new(2085, 24, 0, 1, 1, 0, 0, (decimal)0.00) // head
        ];

        public static BitmapImage ToBitmapImage(Bitmap bitmap) // Credit to the uploader of this code LawMan on StackOverflow
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public static BitmapImage GetImage(Bitmap pngFile)
        {
            Sprite spr = GetSprite(false);
            SpriteSet sprite = new();
            sprite.Sprites.Add(spr);
            Bitmap newBitmap = new(pngFile);
            newBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            Texture text = MikuMikuLibrary.Native.TextureEncoder.EncodeFromBitmap(newBitmap, TextureFormat.DXT5, true);
            sprite.TextureSet.Textures.Add(text);
            Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
            BitmapImage img = ToBitmapImage(cropSprite);
            return img;
        }

        public async static Task GetLocalUsedInformation(bool useDMA)
        {
            if (Settings.Default.gamePath == null)
            {
                OpenFolderDialog ofd = new();
                if (ofd.ShowDialog() == true && ofd.FolderName.EndsWith("Hatsune Miku Project DIVA Mega Mix Plus"))
                {
                    Settings.Default.gamePath = ofd.FolderName;
                }
            }
            if ((!File.Exists(Settings.Default.gamePath + "/diva_main.cpk") || !File.Exists(Settings.Default.gamePath + "/diva_dlc00_region.cpk")) && !useDMA)
            {
                NotiBox("Please make sure you have diva_main.cpk (base game) and diva_dlc00_region.cpk (region dlc) in your game folder.\nOtherwise, use DMA compatibility information.", "Error");
                throw new Exception("Could not locate diva_main.cpk or diva_dlc00_region.cpk.");
            }
            else
            {
                UsedIdSet usedIds = new();
                await Task.Run(usedIds.ProcessLocalFiles);
                await Task.Run(() => usedIds.GetUsedModIds(useDMA));
            }
        }

        public void CheckGamePath()
        {
            while (Settings.Default.gamePath == null || !Settings.Default.gamePath.Contains("Hatsune Miku Project DIVA Mega Mix Plus") || !Directory.Exists(Settings.Default.gamePath))
            {
                OpenFolderDialog ofd = new()
                {
                    Title = "Please select your game directory.",
                    RootDirectory = Settings.Default.gamePath
                };
                if(ofd.ShowDialog() == true)
                {
                    Settings.Default.gamePath = ofd.FolderName;
                    Settings.Default.Save();
                }
            }
        }
        public static async Task DownloadUpdate()
        {
            MemoryStream json = new();
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage request = new(new HttpMethod("GET"), "https://api.github.com/repos/jaykthx/Armoire/releases/latest");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Armoire");
                using (request)
                {
                    var response = httpClient.SendAsync(request);
                    using (json)
                    {
                        json.SetLength((int)response.Result.Content.Headers.ContentLength);
                        await response.Result.Content.CopyToAsync(json);
                    }
                }
            }
            string jsonFile = Encoding.UTF8.GetString(json.ToArray());
            JsonObject job = (JsonObject)JsonNode.Parse(jsonFile);
            var assets = (JsonObject)job["assets"][0];
            DateTime updateTime = (DateTime)assets["updated_at"];
            if (versionDate < updateTime)
            {
                ChoiceWindow choice = new($"{Resources.update_new_found} ({(string)job["name"]})\n{Resources.update_new_found_2}\n\nChangelog\n{(string)job["body"]}", Resources.cmn_no, Resources.cmn_yes);
                if (choice.isRightClicked)
                {
                    await downloader.DownloadFileASync((string)assets["browser_download_url"], AppDomain.CurrentDomain.BaseDirectory, "update.rar");
                    SevenZipBase.SetLibraryPath(AppDomain.CurrentDomain.BaseDirectory + "\\7z.dll");
                    var extractor = new SevenZipExtractor("update.rar");
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\update");
                    await extractor.ExtractArchiveAsync(AppDomain.CurrentDomain.BaseDirectory + "\\update");
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\update.rar");
                    Program.LaunchUpdater();
                }
            }
            else
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    PopupNotification pop = new(Resources.notice_update_newest);
                });
            }
        }

        public static async void LaunchUpdater()
        {
            App.Current.Dispatcher.Invoke((Action)delegate {
            PopupNotification pop = new($"{Resources.notice_update}\n{Resources.notice_update2}");
            });
            await Task.Run(() =>
            {
                Thread.Sleep(2500);
            });
            ProcessStartInfo start = new ProcessStartInfo();
            start.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            start.FileName = "updater.exe";
            start.WindowStyle = ProcessWindowStyle.Minimized;
            Process.Start(start);
            App.Current.Shutdown();
        }
    }
}