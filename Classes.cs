using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Armoire
{
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
}
