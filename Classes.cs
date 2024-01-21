using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.CriMw;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Armoire
{
    public class usedIDs
    {
        public List<uint> spr_db = new List<uint>();
        public List<uint> obj_db = new List<uint>();
        public List<uint> tex_db = new List<uint>();
        public List<int> module_tbl = new List<int>();
        public List<int> module_tbl_index = new List<int>();
        public List<int> customize_item_tbl = new List<int>();
        public List<int> customize_item_tbl_index = new List<int>();
        public Dictionary<string, List<int>> chritm_prop_cos = new Dictionary<string, List<int>>();
        public Dictionary<string, List<int>> chritm_prop_item = new Dictionary<string, List<int>>();
        public void get_used_ids (SpriteDatabase s_db)
        {
            foreach (SpriteSetInfo spr in s_db.SpriteSets)
            {
                spr_db.Add(spr.Id);
                foreach (SpriteInfo s in spr.Sprites)
                {
                    spr_db.Add(s.Id);
                }
                foreach (SpriteTextureInfo t in spr.Textures)
                {
                    spr_db.Add(t.Id);
                }
            }
        }
        public void get_used_ids(ObjectDatabase o_db)
        {
            foreach (ObjectSetInfo obj in o_db.ObjectSets)
            {
                obj_db.Add(obj.Id);
            }
        }

        public void get_used_ids(TextureDatabase t_db)
        {
            foreach (TextureInfo t in t_db.Textures)
            {
                tex_db.Add(t.Id);
            }
        }
        /// <summary>
        /// Get the used IDs for your farc archive table file. (mode: 0 = Modules, 1 = Customize Items, 2 = Character Items (chritm_prop))
        /// </summary>
        /// <param name="farc"></param>
        /// <param name="mode"></param>
        public void get_used_ids(FarcArchive farc, int mode) // mode: 0 - module 1 - customize 2 - chritm
        {
            if(mode == 0) // works
            {
                ObservableCollection<module> m_tbl = Program.IO.ReadModuleFile(farc);
                foreach (module m in m_tbl)
                {
                    if (!module_tbl.Contains(m.id))
                    {
                        module_tbl.Add(m.id);
                    }
                    if (!module_tbl_index.Contains(m.sort_index))
                    {
                        module_tbl_index.Add(m.sort_index);
                    }
                }
            }
            else if(mode == 1) // will work if used
            {
                ObservableCollection<cstm_item> c_tbl = Program.IO.ReadCustomFile(farc);
                foreach (cstm_item c in c_tbl)
                {
                    if (!customize_item_tbl.Contains(c.id))
                    {
                        customize_item_tbl.Add(c.id);
                    }
                    if (!customize_item_tbl.Contains(c.sort_index))
                    {
                        customize_item_tbl.Add(c.sort_index);
                    }
                }
            }
            else if(mode == 2) // works
            {
                ObservableCollection<chritmFile> chritms = Program.IO.ReadCharaFile(farc);
                foreach (chritmFile chrFile in chritms)
                {
                    List<int> costumes = new List<int>();
                    List<int> items = new List<int>();
                    foreach (cosEntry cos in chrFile.costumes)
                    {
                        costumes.Add(cos.id);
                    }
                    foreach (itemEntry item in chrFile.items)
                    {
                        items.Add(item.no);
                    }
                    if (chritm_prop_cos.ContainsKey(chrFile.chara))
                    {
                        foreach(int cos in costumes)
                        {
                            chritm_prop_cos[chrFile.chara].Add(cos);
                        }
                        foreach (int item in items)
                        {
                            chritm_prop_item[chrFile.chara].Add(item);
                        }
                    }
                    else
                    {
                        chritm_prop_cos.Add(chrFile.chara, costumes);
                        chritm_prop_item.Add(chrFile.chara, items);
                    }
                    
                }
            }
            else
            {
                Program.NotiBox("Don't play with me right now.", "You messed up");
            }
        }
    }

    public enum Attr
    {
        Default_N = 0,
        Default_FS = 4,
        Default_CT = 8,
        Default_PL = 12,
        Swimsuit_N = 1,
        Swimsuit_FS = 5,
        Swimsuit_CT = 9,
        Swimsuit_PL = 13,
        NoSwap_N = 2,
        NoSwap_FS = 6,
        NoSwap_CT = 10,
        NoSwap_PL = 14,
        TShirt = 16
    }
    public class Entry
    {
        public string entry { get; set; }
        public string chara { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public bool ng { get; set; }
        public string shop_price { get; set; }
        public int shop_st_day { get; set; } //honestly who would bother but ill leave it anyway
        public int shop_st_month { get; set; }
        public int shop_st_year { get; set; }
        public int sort_index { get; set; }
        public virtual List<string> getEntry()
        {
            List<string> x = new List<string>();
            return x;
        }
    }

    [Serializable]
    public class module : Entry
    {
        public Attr attr { get; set; }
        public string cos { get; set; }
        public override List<string> getEntry()
        {
            List<string> x = new List<string>();
            string a = "module." + entry + ".";
            x.Add(a + "attr=" + (int)attr);
            x.Add(a + "chara=" + chara);
            x.Add(a + "cos=" + cos);
            x.Add(a + "id=" + id);
            x.Add(a + "name=" + name);
            x.Add(a + "ng=" + Convert.ToInt32(ng));
            x.Add(a + "shop_ed_day=9");
            x.Add(a + "shop_ed_month=9");
            x.Add(a + "shop_ed_year=2999");
            x.Add(a + "shop_price=" + shop_price);
            x.Add(a + "shop_st_day=" + shop_st_day);
            x.Add(a + "shop_st_month=" + shop_st_month);
            x.Add(a + "shop_st_year=" + shop_st_year);
            x.Add(a + "sort_index=" + sort_index);
            return x;
        }
    }

    [Serializable]
    public class cstm_item : Entry
    {
        public int bind_module { get; set; }
        public int obj_id { get; set; }
        public string parts { get; set; }
        public bool sell_type { get; set; }
        public override List<string> getEntry()
        {
            List<string> x = new List<string>();
            string a = "cstm_item." + entry + ".";
            x.Add(a + "bind_module=" + bind_module);
            x.Add(a + "chara=" + chara);
            x.Add(a + "id=" + id);
            x.Add(a + "name=" + name);
            x.Add(a + "ng=" + Convert.ToInt32(ng));
            x.Add(a + "obj_id=" + obj_id);
            x.Add(a + "parts=" + parts);
            x.Add(a + "sell_type=" + Convert.ToInt32(sell_type));
            x.Add(a + "shop_ed_day=9");
            x.Add(a + "shop_ed_month=9");
            x.Add(a + "shop_ed_year=2999");
            x.Add(a + "shop_price=" + shop_price);
            x.Add(a + "shop_st_day=" + shop_st_day);
            x.Add(a + "shop_st_month=" + shop_st_month);
            x.Add(a + "shop_st_year=" + shop_st_year);
            x.Add(a + "sort_index=" + sort_index);
            return x;
        }
    }
    [Serializable]
    public class cosEntry
    {
        public string entry { get; set; }
        public int id { get; set; }
        public ObservableCollection<int> items { get; set; }
        public List<string> getEntry()
        {
            List<string> x = new List<string>();
            string a = "cos." + entry + ".";
            x.Add("cos." + entry + ".id=" + id);
            int count = 0;
            foreach (int i in items)
            {
                x.Add(a + "item." + count + "=" + i);
                count++;
            }
            x.Add(a + "item.length=" + count);
            return x;
        }
    }
    [Serializable]
    public class dataSetTex
    {
        public string entry { get; set; }
        public string chg { get; set; }
        public string org { get; set; }
    }
    [Serializable]
    public class itemEntry
    {
        public string entry { get; set; }
        public int attr { get; set; }  //Editable
        public int rpk = -1; // N/A
        public string uid { get; set; } //Table
        public ObservableCollection<dataSetTex> dataSetTexes { get; set; } //Editable - Window
        public int desID { get; set; } //Editable
        public decimal face_depth { get; set; } //Editable (Combobox?)
        public int flag = 0; //Marks tex swaps, not needed?
        public string name { get; set; } // Table
        public int no { get; set; } // Table
        public List<string> objset { get; set; } //Editable - Window
        public int orgItm { get; set; } //Editable
        public int subID { get; set; }  //Editable
        public int type { get; set; } //Editable

        public List<string> getEntry()
        {
            string a = "item." + entry + ".";
            if(uid == "NULL")
            {
                rpk = 1;
            }
            List<string> x = new List<string>();
            x.Add(a + "attr=" + attr);
            x.Add(a + "data.obj.0.rpk=" + rpk);
            x.Add(a + "data.obj.0.uid=" + uid);
            x.Add(a + "data.obj.length=1");
            int count = 0;
            foreach (dataSetTex tex in dataSetTexes)
            {
                tex.entry = count.ToString();
                count++;
            }
            foreach (dataSetTex tex in dataSetTexes.OrderBy(o => o.entry))
            {
                x.Add(a + "data.tex." + tex.entry + ".chg=" + tex.chg);
                x.Add(a + "data.tex." + tex.entry + ".org=" + tex.org);
            }
            if (dataSetTexes.Count > 0)
            {
                x.Add(a + "data.tex.length=" + count);
            }
            x.Add(a + "des_id=" + desID);
            x.Add(a + "exclusion=0");
            x.Add(a + "face_depth=" + face_depth);
            x.Add(a + "flag=" + flag);
            x.Add(a + "name=" + name);
            x.Add(a + "no=" + no);
            if (objset.Count > 1)
            {
                x.Add(a + "objset.0=" + objset[0]);
                x.Add(a + "objset.1=" + objset[1]);
                x.Add(a + "objset.length=2");
            }
            else
            {
                x.Add(a + "objset.0=" + objset[0]);
                x.Add(a + "objset.length=1");
            }
            x.Add(a + "org_itm=" + orgItm);
            x.Add(a + "point=0");
            x.Add(a + "sub_id=" + subID);
            x.Add(a + "type=" + type);
            return x;
        }
    }
    public class chritmFile
    {
        public string chara { get; set; }
        public List<cosEntry> costumes { get; set; }
        public List<itemEntry> items { get; set; }
        public string getFullName()
        {
            switch (chara)
            {
                case "mik":
                    return "Hatsune Miku";
                case "rin":
                    return "Kagamine Rin";
                case "len":
                    return "Kagamine Len";
                case "luk":
                    return "Megurine Luka";
                case "kai":
                    return "Kaito";
                case "mei":
                    return "Meiko";
                case "sak":
                    return "Sakine Meiko";
                case "ner":
                    return "Akita Neru";
                case "hak":
                    return "Yowane Haku";
                case "tet":
                    return "Kasane Teto";
                default:
                    return chara;
            }
        }
        public string getFileName()
        {
            string x = chara + "itm_tbl.txt";
            return x;
        }
    }
    public class wizObj
    {
        public string objectFilePath; // file path
        public wizObjEntry objEntry; // add after assigning for better follow-up
        public itemEntry item;
    }
    public class wizModule
    {
        public List<wizObj> objects = new List<wizObj>();
        public localisedNames localNames = new localisedNames();
        public bool hairNG;
        public string name;
        public int id = -1; //default to a number
        public string chara;
        public int sort_index = 999;
        public Bitmap bitmap = Armoire.Properties.Resources.md_dummy;
        public string image_path = Directory.GetCurrentDirectory() + "/Resources/md_dummy.png";
    }

    public class localisedNames
    {
        public string cn = "发型";
        public string en = " Hair";
        public string fr = " - Coupe";
        public string ge = "Frisur: ";
        public string it = "Capelli ";
        public string kr = " 헤어";
        public string sp = "Pelo ";
        public string tw = "髮型:";
    }
    public class wizObjEntry
    {
        public string name;
        public string fileName; //obj
        public string textureFileName;//tex
        public string archiveFileName;//farc
        public uint id;//obj_db id
        public string objName; //divskn
        public uint objId; //divskn id
    }
}
