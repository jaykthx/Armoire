using CsvHelper;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using static Armoire.MainWindow;

namespace Armoire
{
    public class Program
    {
        public static string modulePath;
        public static string customPath;
        public static string charaPath;
        public static List<string> charas = new List<string> { "MIKU", "RIN", "LEN", "LUKA", "KAITO", "MEIKO", "NERU", "HAKU", "SAKINE", "TETO" };

        public static void NotiBox(string value, string title)
        {
            NotiBox noti = new NotiBox(value, title);
            noti.ShowDialog();
        }

        public static ObservableCollection<chritmFile> readCharaFile(string charaPath)
        {
            ObservableCollection<chritmFile> temp = new ObservableCollection<chritmFile>();
            var farc = BinaryFile.Load<FarcArchive>(charaPath);
            foreach (var fileName in farc)
            {
                if (fileName.EndsWith("_tbl.txt"))
                {
                    var source = farc.Open(fileName, EntryStreamMode.MemoryStream);
                    string name = fileName.Remove(3);
                    StreamReader sr = new StreamReader(source);
                    string content = sr.ReadToEnd();
                    string[] vs = content.Split(new[] { '\n', '\r' });
                    temp.Add(readList(vs, name));
                }
                else
                {
                    NotiBox("La p*ta maldita: " + fileName.ToString(), "¿En serio, crees que soy idiota?");
                }
            }
            farc.Dispose();
            return temp;
        }

        public static chritmFile readList(string[] content, string name)
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
                if (fileLine.Contains("cos.") && fileLine.Contains(".id="))
                {
                    readEntry.id = int.Parse(split[1]);
                }
                if (fileLine.Contains("cos.") && fileLine.Contains("item.") && !fileLine.Contains("length"))
                {
                    tempItems.Add(int.Parse(split[1]));
                }
                if (fileLine.Contains("cos.") && fileLine.Contains("item.length"))
                {
                    readEntry.items = new ObservableCollection<int>(tempItems);
                    tempCosList.Add(readEntry);
                    readEntry = new cosEntry();
                    tempItems = new List<int>();
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".attr="))
                {
                    itemEntry.attr = Int32.Parse(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".data.obj.0.rpk="))
                {
                    itemEntry.rpk = Int32.Parse(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".data.obj.0.uid="))
                {
                    itemEntry.uid = split[1];
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".chg="))
                {
                    setTex.chg = split[1];
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".org="))
                {
                    setTex.org = split[1];
                    itemEntry.flag = 4;
                    temp.Add(setTex);
                    setTex = new dataSetTex();
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".des_id="))
                {
                    itemEntry.desID = int.Parse(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".face_depth="))
                {
                    itemEntry.face_depth = decimal.Parse(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".flag="))
                {
                    itemEntry.flag = Int32.Parse(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".name="))
                {
                    itemEntry.name = split[1];
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".no="))
                {
                    itemEntry.no = int.Parse(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".objset") && !fileLine.Contains("length"))
                {
                    list.Add(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".org_itm="))
                {
                    itemEntry.orgItm = int.Parse(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".sub_id="))
                {
                    itemEntry.subID = int.Parse(split[1]);
                }
                if (fileLine.Contains("item.") && fileLine.Contains(".type="))
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
            chritmFile x = new chritmFile();
            x.chara = name;
            x.costumes = tempCosList.OrderBy(o => o.id).ToList();
            x.items = tempItemList.OrderBy(o => o.no).ToList();
            return x;
        }
        public static ObservableCollection<module> readModuleFileCSV(string csvPath)
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
        public static ObservableCollection<cstm_item> readCustomFileCSV(string csvPath)
        {
            ObservableCollection<cstm_item> temp = new ObservableCollection<cstm_item>();
            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<cstm_item>();
                csv.Context.RegisterClassMap<CustEditor.cstm_item_map>();
                foreach (cstm_item x in records)
                {
                    temp.Add(x);
                }
            }
            temp = new ObservableCollection<cstm_item>(temp.OrderBy(x => x.id).ToList());
            return temp;
        }
        public static ObservableCollection<module> readModuleFile(string modulePath)
        {
            ObservableCollection<module> temp = new ObservableCollection<module>();
            string[] containString;
            var farc = BinaryFile.Load<FarcArchive>(modulePath);
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

        public static ObservableCollection<cstm_item> readCustomFile(string customPath)
        {
            ObservableCollection<cstm_item> temp = new ObservableCollection<cstm_item>();
            string[] containString;
            var farc = BinaryFile.Load<FarcArchive>(customPath);
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

        public static void saveFile<T>(string path, ObservableCollection<T> list) where T : Entry
        {
            if (path.Length > 0)
            {
                int count = 0;
                var farc = new FarcArchive();
                MemoryStream outputSource = new MemoryStream();
                using (StreamWriter tw = new StreamWriter(outputSource))
                {
                    tw.AutoFlush = true;
                    foreach(T entry in list)
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
            else { Program.NotiBox("That didn't work. Try opening a table first.", "Error"); }
        }
        public static void saveChr(string path, ObservableCollection<chritmFile> list)
        {
            if (path.Length > 0)
            {
                var farc = new FarcArchive();
                foreach (chritmFile x in list)
                {
                    MemoryStream outputSource = new MemoryStream();
                    using (StreamWriter tw = new StreamWriter(outputSource))
                    {
                        tw.AutoFlush = true;
                        int count = 0;
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
                farc.Dispose();
            }
            else { Program.NotiBox("That didn't work. Try opening a table first.", "Error"); }
        }
    }
}
