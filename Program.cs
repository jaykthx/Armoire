using Armoire.Dialogs;
using CsvHelper;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.CriMw;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Sprites;
using MikuMikuLibrary.Textures;
using MikuMikuLibrary.Textures.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using static Armoire.MainWindow;

namespace Armoire
{
    public class Program
    {
        public static string modulePath;
        public static string customPath;
        public static string charaPath;
        public static List<string> charas = new List<string> { "MIKU", "RIN", "LEN", "LUKA", "KAITO", "MEIKO", "NERU", "HAKU", "SAKINE", "TETO" };

        public class IO
        {
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
            public static ObservableCollection<chritmFile> ReadCharaFile(FarcArchive farc)
            {
                ObservableCollection<chritmFile> temp = new ObservableCollection<chritmFile>();

                foreach (var fileName in farc)
                {
                    if (fileName.EndsWith("_tbl.txt"))
                    {
                        var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                        string name = fileName.Remove(3);
                        StreamReader sr = new StreamReader(source);
                        string content = sr.ReadToEnd();
                        string[] vs = content.Split(new[] { '\n', '\r' });
                        temp.Add(ReadList(vs, name));
                    }
                    else
                    {
                        NotiBox(Properties.Resources.exp_5 + fileName.ToString(), Properties.Resources.cmn_error);
                    }
                }
                farc.Dispose();
                return temp;
            }

            public static chritmFile ReadList(string[] content, string name)
            {
                List<cosEntry> tempCosList = new List<cosEntry>();
                List<itemEntry> tempItemList = new List<itemEntry>();
                ObservableCollection<dataSetTex> temp = new ObservableCollection<dataSetTex>();
                cosEntry readEntry = new cosEntry();
                List<int> tempItems = new List<int>();
                itemEntry itemEntry = new itemEntry();
                dataSetTex setTex = new dataSetTex();
                List<string> list = new List<string>();
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
                            readEntry = new cosEntry();
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
                            setTex = new dataSetTex();
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
                            itemEntry.dataSetTexes = temp;
                            temp = new ObservableCollection<dataSetTex>();
                            tempItemList.Add(itemEntry);
                            list = new List<string>();
                            itemEntry = new itemEntry();
                        }
                    }
                }
                chritmFile x = new chritmFile
                {
                    chara = name,
                    costumes = tempCosList.OrderBy(o => o.id).ToList(),
                    items = tempItemList.OrderBy(o => o.no).ToList()
                };
                return x;
            }
            public static ObservableCollection<module> ReadModuleFileCSV(string csvPath)
            {
                ObservableCollection<module> temp = new ObservableCollection<module>();
                using (var reader = new StreamReader(csvPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<module>();
                    csv.Context.RegisterClassMap<moduleMap>();
                    foreach (module x in records)
                    {
                        temp.Add(x);
                    }
                }
                temp = new ObservableCollection<module>(temp.OrderBy(x => x.id).ToList());
                return temp;
            }
            public static ObservableCollection<cstm_item> ReadCustomFileCSV(string csvPath)
            {
                ObservableCollection<cstm_item> temp = new ObservableCollection<cstm_item>();
                using (var reader = new StreamReader(csvPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<cstm_item>();
                    csv.Context.RegisterClassMap<MainWindow.cstm_item_map>();
                    foreach (cstm_item x in records)
                    {
                        temp.Add(x);
                    }
                }
                temp = new ObservableCollection<cstm_item>(temp.OrderBy(x => x.id).ToList());
                return temp;
            }
            public static ObservableCollection<module> ReadModuleFile(FarcArchive farc)
            {
                ObservableCollection<module> temp = new ObservableCollection<module>();
                string[] containString;
                foreach (var fileName in farc)
                {
                    if (fileName.EndsWith(".bin"))
                    {
                        var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                        using (var sr = new StreamReader(source, Encoding.UTF8))
                        {
                            string content = sr.ReadToEnd();
                            var readEntry = new module();
                            foreach (string fileLine in content.Split(new[] { '\n', '\r' }))
                            {
                                if (fileLine.Contains("module."))
                                {
                                    containString = fileLine.Split('=');
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
                                            temp.Add(readEntry);
                                            readEntry = new module();
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                farc.Dispose();
                temp = new ObservableCollection<module>(temp.OrderBy(x => x.id).ToList());
                return temp;
            }

            public static ObservableCollection<cstm_item> ReadCustomFile(FarcArchive farc)
            {
                ObservableCollection<cstm_item> temp = new ObservableCollection<cstm_item>();
                string[] containString;
                foreach (var fileName in farc)
                {
                    if (fileName.EndsWith(".bin"))
                    {
                        var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                        using (var sr = new StreamReader(source, Encoding.UTF8))
                        {
                            string content = sr.ReadToEnd();
                            var readEntry = new cstm_item();
                            int[] dates = { 0, 0, 0 };
                            foreach (string fileLine in content.Split(new[] { '\n', '\r' }))
                            {
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
                                            readEntry = new cstm_item();
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                farc.Dispose();
                temp = new ObservableCollection<cstm_item>(temp.OrderBy(x => x.id).ToList());
                return temp;
            }

            public static void SaveFile<T>(string path, ObservableCollection<T> list) where T : Entry
            {
                if (path.Length > 0)
                {
                    int count = 0;
                    var farc = new FarcArchive();
                    MemoryStream outputSource = new MemoryStream();
                    using (StreamWriter tw = new StreamWriter(outputSource))
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
                        if (typeof(T) == typeof(module))
                        {
                            tw.WriteLine(nameof(module) + ".data_list.length=" + list.Count);
                            farc.Add("gm_module_id.bin", outputSource, true, ConflictPolicy.Replace);
                        }
                        if (typeof(T) == typeof(cstm_item))
                        {
                            tw.WriteLine(nameof(cstm_item) + ".data_list.length=" + list.Count);
                            farc.Add("gm_customize_item_id.bin", outputSource, true, ConflictPolicy.Replace);
                        }
                        farc.IsCompressed = true;
                        farc.Save(path);
                        tw.Close();
                    }
                    farc.Dispose();
                }
                else { NotiBox(Properties.Resources.exp_4, Properties.Resources.cmn_error); }
            }
            public static void SaveChr(string path, ObservableCollection<chritmFile> list)
            {
                if (path.Length > 0)
                {
                    var farc = new FarcArchive();
                    foreach (chritmFile x in list)
                    {
                        if (x.items.Count > 0)
                        {
                            MemoryStream outputSource = new MemoryStream();
                            using (StreamWriter tw = new StreamWriter(outputSource))
                            {
                                tw.AutoFlush = true;
                                int count = 0;
                                if (x.costumes.Count > 0)
                                {
                                    foreach (cosEntry entry in x.costumes)
                                    {
                                        entry.entry = count.ToString();
                                        count++;
                                    }

                                    foreach (cosEntry c in x.costumes.OrderBy(o => o.entry))
                                    {
                                        foreach (string w in c.getEntry())
                                        {
                                            tw.WriteLine(w);
                                        }
                                    }
                                }
                                tw.WriteLine("cos.length=" + x.costumes.Count);
                                count = 0;
                                foreach (itemEntry entry in x.items)
                                {
                                    entry.entry = count.ToString();
                                    count++;
                                }
                                foreach (itemEntry i in x.items.OrderBy(o => o.entry))
                                {
                                    foreach (string w in i.getEntry())
                                    {
                                        tw.WriteLine(w);
                                    }
                                }
                                tw.WriteLine("item.length=" + x.items.Count);
                                farc.Add(x.getFileName(), outputSource, true, ConflictPolicy.Replace);
                                farc.IsCompressed = true;
                                farc.Save(path);
                                tw.Close();
                            }
                        }
                    }
                    farc.Dispose();
                }
                else { NotiBox(Properties.Resources.exp_4, Properties.Resources.cmn_error); }
            }
        }
        public static void NotiBox(string value, string title)
        {
            NotiBox noti = new NotiBox(value, title);
            noti.ShowDialog();
        }
        public static bool ChoiceWindow(string main_text, string left_text, string right_text)
        {
            ChoiceWindow win = new ChoiceWindow(main_text, left_text, right_text);
            win.ShowDialog();
            return win.isRightClicked;
        }

        public class Databases
        {
            public static string GetIDString(string id)
            {
                StringBuilder sb = new StringBuilder(id);
                while (sb.Length < 3)
                {
                    sb.Insert(0, "0");
                }
                return sb.ToString();
            }
            public static int GetUnusedID(List<int> used) // no limit version
            {
                Random rnd = new Random();
                int final = rnd.Next();
                while (used.Contains(final))
                {
                    final = rnd.Next();
                }
                return final;
            }
            public static int GetUnusedID(List<int> used1, List<int> used2) // 2 list (int) version
            {
                Random rnd = new Random();
                int final = rnd.Next();
                while (used1.Contains(final) && used2.Contains(final))
                {
                    final = rnd.Next();
                }
                return final;
            }
            public static uint GetUnusedID(List<uint> used) // no limit version
            {
                Random rnd = new Random();
                int final = rnd.Next();
                while (used.Contains((uint)final))
                {
                    final = rnd.Next();
                }
                return (uint)final;
            }
            public static uint GetUnusedID(List<uint> used, int maximum) // obj_db, tex_db, spr_db
            {
                Random rnd = new Random();
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
            public static void AddToSpriteDatabase(SpriteDatabase spr_db, int id, bool isCustomise, List<uint> used_spr_IDs)
            {
                SpriteSetInfo sprSetInfo = new SpriteSetInfo
                {
                    Id = GetUnusedID(used_spr_IDs)
                };
                used_spr_IDs.Add(sprSetInfo.Id);
                SpriteInfo sprInfo = new SpriteInfo
                {
                    Id = GetUnusedID(used_spr_IDs)
                };
                used_spr_IDs.Add(sprInfo.Id);
                SpriteTextureInfo sprTexInfo = new SpriteTextureInfo
                {
                    Id = GetUnusedID(used_spr_IDs),
                    Name = "MERGE_BC5COMP_0"
                };
                used_spr_IDs.Add(sprTexInfo.Id);
                if (isCustomise)
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
                        return "mik";
                }
            }
        }
        public static Sprite GetSprite(bool isCustomise)
        {
            Sprite spr = new Sprite
            {
                Width = 408,
                Height = 494, // official width + height
                TextureIndex = 0
            };
            if (isCustomise)
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
        public static void GenerateSprite(Bitmap bmp, int id, string outputFolder, bool isCustomise)
        {
            Sprite spr = GetSprite(isCustomise);
            Bitmap newBitmap = new Bitmap(bmp);
            newBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            MikuMikuLibrary.Textures.Processing.TextureEncoderCore tex = new MikuMikuLibrary.Textures.Processing.TextureEncoderCore();
            Texture text = tex.EncodeFromBitmap(newBitmap, TextureFormat.DXT5, true);
            text.Name = "MERGE_BC5COMP_0";
            SpriteSet sprSet = new SpriteSet();
            sprSet.Sprites.Add(spr);
            sprSet.TextureSet.Textures.Add(text);
            FarcArchive farc = new FarcArchive();
            var stream = new MemoryStream();
            sprSet.Save(stream, true);
            string fileName = "spr_";
            if (isCustomise)
            {
                fileName = fileName + "cmnitm_thmb" + id;
            }
            else
            {
                fileName = fileName + "sel_md" + id + "cmn";
            }
            farc.Add(fileName + ".bin", stream, false, ConflictPolicy.Replace);
            farc.Save(outputFolder + "/" + fileName + ".farc");
            farc.Dispose();
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

        public static List<ItemPreset> itemPresets = new List<ItemPreset>();
        static readonly ItemPreset eyetex = new ItemPreset(37, 24, 0, 1, 2, 0, 0, (decimal)0.00);
        static readonly ItemPreset lenses = new ItemPreset(1, 6, 1, -1, 0, 0, 0, (decimal)0.00);
        static readonly ItemPreset hair = new ItemPreset(1, 1, 0, -1, 0, 0, 0, (decimal)0.00);
        static readonly ItemPreset body = new ItemPreset(1, 10, 2, -1, 1, 0, 0, (decimal)0.00);
        static readonly ItemPreset hands = new ItemPreset(1, 14, 2, -1, 0, 0, 0, (decimal)0.00);
        static readonly ItemPreset headacc = new ItemPreset(1, 0, 0, -1, 0, 0, 0, (decimal)0.00);
        static readonly ItemPreset faceacc = new ItemPreset(1, 4, 1, -1, 0, 0, 0, (decimal)0.00);
        static readonly ItemPreset chestacc = new ItemPreset(1, 8, 2, -1, 0, 0, 0, (decimal)0.00);
        static readonly ItemPreset backacc = new ItemPreset(1, 16, 2, -1, 0, 0, 0, (decimal)0.00);
        public static void CreatePresetList()
        {
            itemPresets = new List<ItemPreset>
            {
                eyetex,
                lenses,
                hair,
                body,
                hands,
                headacc,
                faceacc,
                chestacc,
                backacc
            };
        }
        public static BitmapImage ToBitmapImage(Bitmap bitmap) // Credit to the uploader of this code LawMan on StackOverflow
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
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

        public static void CreateModLocalisation(string FolderPath, string Name, int ItemID, bool isModule)
        {
            if (isModule)
            {
                using (TextWriter tw = new StreamWriter(FolderPath + "/mod_str_array.toml", true))
                {
                    tw.WriteLine("cn.module." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("en.module." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("fr.module." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("ge.module." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("it.module." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("kr.module." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("sp.module." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("tw.module." + ItemID + " = " + "\"" + Name + "\"");
                }
            }
            else
            {
                using (TextWriter tw = new StreamWriter(FolderPath + "/mod_str_array.toml", true))
                {
                    tw.WriteLine("cn.customize." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("en.customize." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("fr.customize." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("ge.customize." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("it.customize." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("kr.customize." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("sp.customize." + ItemID + " = " + "\"" + Name + "\"");
                    tw.WriteLine("tw.customize." + ItemID + " = " + "\"" + Name + "\"");
                }
            }
        }
        public static void CreateModLocalisation(string FolderPath, localisedNames LocalNames, int ItemID)
        {
            using (TextWriter tw = new StreamWriter(FolderPath + "/mod_str_array.toml", true))
            {
                tw.WriteLine("cn.customize." + ItemID + " = " + "\"" + LocalNames.cn + "\"");
                tw.WriteLine("en.customize." + ItemID + " = " + "\"" + LocalNames.en + "\"");
                tw.WriteLine("fr.customize." + ItemID + " = " + "\"" + LocalNames.fr + "\"");
                tw.WriteLine("ge.customize." + ItemID + " = " + "\"" + LocalNames.ge + "\"");
                tw.WriteLine("it.customize." + ItemID + " = " + "\"" + LocalNames.it + "\"");
                tw.WriteLine("kr.customize." + ItemID + " = " + "\"" + LocalNames.kr + "\"");
                tw.WriteLine("sp.customize." + ItemID + " = " + "\"" + LocalNames.sp + "\"");
                tw.WriteLine("tw.customize." + ItemID + " = " + "\"" + LocalNames.tw + "\"");
            }
        }

        public static void GetExistingIDs(string gameDirectory, usedIDs usedID)
        {
            if (gameDirectory != null)
            {
                List<string> obj_dbs;
                List<string> tex_dbs;
                List<string> spr_dbs;
                List<string> module_tbls;
                List<string> customise_tbls;
                List<string> chritm_props;
                CpkArchive cpk = new CpkArchive();
                string currentFile = "";
                try
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
                            if (file.Contains("eden") || file.Contains("Eden") || file.Contains("EDEN"))
                            {
                                Program.NotiBox("Eden Project mod folder detected.\nThis file will not be read as it is known to cause issues with Armoire and other mods.", Properties.Resources.cmn_error);
                                continue;
                            }
                            else
                            {
                                currentFile = file;
                                FarcArchive farc = BinaryFile.Load<FarcArchive>(file);
                                usedID.get_used_ids(farc, 0);
                            }
                        }
                        foreach (string file in customise_tbls)
                        {
                            if (file.Contains("eden") || file.Contains("Eden") || file.Contains("EDEN"))
                            {
                                Program.NotiBox("Eden Project mod folder detected.\nThis file will not be read as it is known to cause issues with Armoire and other mods.", Properties.Resources.cmn_error);
                                continue;
                            }
                            else
                            {
                                currentFile = file;
                                FarcArchive farc = BinaryFile.Load<FarcArchive>(file);
                                usedID.get_used_ids(farc, 1);
                            }
                        }
                        foreach (string file in chritm_props)
                        {
                            if (file.Contains("eden") || file.Contains("Eden") || file.Contains("EDEN"))
                            {
                                Program.NotiBox("Eden Project mod folder detected.\nThis file will not be read as it is known to cause issues with Armoire and other mods.", Properties.Resources.cmn_error);
                                continue;
                            }
                            else
                            {
                                currentFile = file;
                                FarcArchive farc = BinaryFile.Load<FarcArchive>(file);
                                usedID.get_used_ids(farc, 2);
                            }
                        }
                        foreach (string file in obj_dbs)
                        {
                            currentFile = file;
                            ObjectDatabase obj_db = BinaryFile.Load<ObjectDatabase>(file);
                            usedID.get_used_ids(obj_db);
                        }
                        foreach (string file in tex_dbs)
                        {
                            currentFile = file;
                            TextureDatabase tex_db = BinaryFile.Load<TextureDatabase>(file);
                            usedID.get_used_ids(tex_db);
                        }
                        foreach (string file in spr_dbs)
                        {
                            currentFile = file;
                            SpriteDatabase spr_db = BinaryFile.Load<SpriteDatabase>(file);
                            usedID.get_used_ids(spr_db);
                        }
                    }
                    if (File.Exists((gameDirectory + "\\diva_dlc00_region.cpk")))
                    {
                        currentFile = "diva_dlc00_region";
                        cpk = BinaryFile.Load<CpkArchive>(gameDirectory + "\\diva_dlc00_region.cpk");
                    }
                    else if (File.Exists((gameDirectory + "\\diva_main_region.cpk")))
                    {
                        currentFile = "diva_main_region";
                        cpk = BinaryFile.Load<CpkArchive>(gameDirectory + "\\diva_main_region.cpk");
                    }
                    else if (File.Exists((gameDirectory + "\\diva_main.cpk")))
                    {
                        currentFile = "diva_main";
                        cpk = BinaryFile.Load<CpkArchive>(gameDirectory + "\\diva_main.cpk");
                    }
                    else
                    {
                        Program.NotiBox("This is not a valid Project DIVA Mega Mix+ directory.", Properties.Resources.cmn_error);
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
                catch { Program.NotiBox("An error occurred while reading the files in your game directory.\nError in: " + currentFile, Properties.Resources.cmn_error); }
            }
        }
        public static void CreateModConfig(string FolderPath, string ModName) //Simple
        {
            using (TextWriter tw = new StreamWriter(FolderPath + "/config.toml"))
            {
                tw.WriteLine("enabled = true");
                tw.WriteLine("name = \"" + ModName + "\"");
                tw.WriteLine("author = \"" + "Armoire" + "\"");
                tw.WriteLine("description = \"A mod created using Armoire.\"");
                tw.WriteLine("include = [\".\"]");
            }
        }
        public static BitmapImage GetImage(Bitmap pngFile)
        {
            Sprite spr = Program.GetSprite(false);
            SpriteSet sprite = new SpriteSet();
            sprite.Sprites.Add(spr);
            Bitmap newBitmap = new Bitmap(pngFile);
            newBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            TextureEncoderCore tex = new TextureEncoderCore();
            Texture text = tex.EncodeFromBitmap(newBitmap, TextureFormat.DXT5, true);
            sprite.TextureSet.Textures.Add(text);
            Bitmap cropSprite = SpriteCropper.Crop(sprite.Sprites[0], sprite);
            BitmapImage img = Program.ToBitmapImage(cropSprite);
            return img;
        }
    }
}