using Armoire.Properties;
using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.CriMw;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static Armoire.Program;

namespace Armoire
{
    public class Conflicts
    {
        public Conflicts()
        {
            foreach(string chara in charas)
            {
                chritm_prop_cos.Add(chara.Remove(3), new List<string>());
                chritm_prop_item.Add(chara.Remove(3), new List<string>());
            }
        }
        public List<string> module_tbl = new();
        public List<string> module_tbl_index = new();
        public List<string> customize_tbl = new();
        public List<string> customize_tbl_index = new();
        public Dictionary<string, List<string>> chritm_prop_cos = new();
        public Dictionary<string, List<string>> chritm_prop_item = new();
        public List<string> obj_db = new();
        public List<string> tex_db = new();
        public List<string> tex_db_names = new();
        public List<string> spr_db = new();
        public List<string> spr_db_spr = new();
        public List<string> spr_db_tex = new();
        public List<string> itmFarcs = new();
    }
    public class UsedIdSet //stores spr_db, obj_db, tex_db, module,customize,chritm and relevant indexes that are in use as to avoid conflicts.
    {
        public Dictionary<uint, string> spr_db = new();
        public Dictionary<uint, string> spr_db_spr = new();
        public Dictionary<uint, string> spr_db_tex = new();
        public Dictionary<uint, string> obj_db = new();
        public Dictionary<uint, string> tex_db = new();
        public Dictionary<string, string> tex_db_names = new();
        public Dictionary<int, string> module_tbl = new();
        public Dictionary<int, string> module_tbl_index = new();
        public Dictionary<int, string> customize_item_tbl = new();
        public Dictionary<int, string> customize_item_tbl_index = new();
        public Dictionary<string, Dictionary<int, string>> chritm_prop_cos = new();
        public Dictionary<string, Dictionary<int, string>> chritm_prop_item = new();
        public Dictionary<string, string> itmFarcs = new();
        public Conflicts conflicts = new();

        public void WriteConflictsToFile(string path)
        {
            using(StreamWriter sw = new StreamWriter(path+"\\conflicts.txt"))
            {
                sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_module_id}:");
                foreach(string c in conflicts.module_tbl)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"(Possibly) {Resources.conflict_start} {Resources.conflict_module_sort}:");
                foreach (string c in conflicts.module_tbl_index)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_custom_id}:");
                foreach (string c in conflicts.customize_tbl)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"(Possibly) {Resources.conflict_start} {Resources.conflict_custom_sort}:");
                foreach (string c in conflicts.customize_tbl_index)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"{Resources.conflict_start} Object IDs:");
                foreach (string c in conflicts.obj_db)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_tex_id}:");
                foreach (string c in conflicts.tex_db)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_tex_name}:");
                foreach (string c in conflicts.tex_db_names)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_spr_set}:");
                foreach (string c in conflicts.spr_db)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_spr}:");
                foreach (string c in conflicts.spr_db_spr)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_spr_tex}:");
                foreach (string c in conflicts.spr_db_tex)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
                foreach(string key in conflicts.chritm_prop_cos.Keys)
                {
                    sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_cos_id} ({key}):");
                    foreach (string c in conflicts.chritm_prop_cos[key])
                    {
                        sw.WriteLine(c);
                    }
                    sw.WriteLine("");
                }
                foreach (string key in conflicts.chritm_prop_item.Keys)
                {
                    sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_item_id} ({key}):");
                    foreach (string c in conflicts.chritm_prop_item[key])
                    {
                        sw.WriteLine(c);
                    }
                    sw.WriteLine("");
                }
                sw.WriteLine($"{Resources.conflict_start} {Resources.conflict_farc}:");
                foreach (string c in conflicts.itmFarcs)
                {
                    sw.WriteLine(c);
                }
                sw.WriteLine("");
            }
        }
        public string GetIdAsString(int id)
        {
            string idString = id.ToString();
            while(idString.Length < 3)
            {
                idString = "0" + idString;
            }
            return idString;
        }

        public int GetUnusedId_Regular(int currentId, string chara) //module or customize item ids
        {
            int cur = currentId;
            if(chara == "all" || chara == "ALL")
            {
                while (module_tbl.Keys.Contains(cur) || customize_item_tbl.Keys.Contains(cur) || itmFarcs.Keys.Contains($"cmnitm{GetIdAsString(cur)}.farc") || itmFarcs.Keys.Contains($"spr_cmnitm_thmb{GetIdAsString(cur)}.farc"))
                {
                    cur++;
                }
            }
            else
            {
                while (module_tbl.Keys.Contains(cur) || customize_item_tbl.Keys.Contains(cur) || itmFarcs.Keys.Contains($"{chara.ToLower()}itm{GetIdAsString(cur)}.farc") || itmFarcs.Keys.Contains($"spr_sel_md{GetIdAsString(cur)}cmn.farc"))
                {
                    cur++;
                }
            }
            module_tbl.Add(cur, "Armoire Wizard");
            customize_item_tbl.Add(cur, "Armoire Wizard");
            return cur;
        }
        public uint GetUnusedId_Database(uint currentId, Dictionary<uint, string> list) //obj, spr or tex_db ids
        {
            uint cur = currentId;
            while (list.Keys.Contains(cur))
            {
                cur++;
            }
            return cur;
        }
        public int GetUnusedId_Database(int currentId, Dictionary<int, string> list) //obj, spr or tex_db ids
        {
            int cur = currentId;
            while (list.Keys.Contains(cur))
            {
                cur++;
            }
            return cur;
        }

        public UsedIdSet()
        {
            foreach (string chrMid in charas)
            {
                string chrShort = chrMid.Remove(3);
                chritm_prop_cos.Add(chrShort, new Dictionary<int, string>());
                chritm_prop_item.Add(chrShort, new Dictionary<int, string>());
            }
        }
        public uint GetUnusedID_Maximum(Dictionary<uint, string> list, int maximum)
        {
            Random random = new();
            uint outValue = (uint)random.Next(0, maximum);
            while (list.Keys.Contains(outValue))
            {
                outValue = (uint)random.Next(0, maximum);
            }
            return outValue;
        }
        public int GetUnusedID_Maximum(Dictionary<int, string> list, int maximum)
        {
            Random random = new();
            int outValue = random.Next(0, maximum);
            while (list.Keys.Contains(outValue))
            {
                outValue = random.Next(0, maximum);
            }
            return outValue;
        }

        public int GetUnusedID_CustomizeItem(int currentId)
        {
            List<int> allIds = new();
            foreach(Dictionary<int, string> ids in chritm_prop_item.Values)
            {
                foreach(int id in ids.Keys)
                {
                    allIds.Add(id);
                }
            }
            while (allIds.Contains(currentId))
            {
                currentId++;
            }
            foreach(Dictionary<int, string> ids in chritm_prop_item.Values)
            {
                ids.Add(currentId, "Armoire Wizard"); //add new value
            }
            return currentId;
        }
 
        public void ProcessLocalFiles()
        {
            string nxPref = "rom_switch/rom/";
            string stPref = "rom_steam/rom/";
            string reference = "Mega Mix+";
            CpkArchive dlc = BinaryFile.Load<CpkArchive>(Settings.Default.gamePath + "/diva_dlc00_region.cpk");
            CpkArchive main = BinaryFile.Load<CpkArchive>(Settings.Default.gamePath + "/diva_main.cpk");
            FarcArchive module = BinaryFile.Load<FarcArchive>(dlc.Open("rom_steam_region_dlc/rom/gm_module_tbl.farc", EntryStreamMode.MemoryStream));
            FarcArchive customize = BinaryFile.Load<FarcArchive>(dlc.Open("rom_steam_region_dlc/rom/gm_customize_item_tbl.farc", EntryStreamMode.MemoryStream));
            FarcArchive chritm = BinaryFile.Load<FarcArchive>(main.Open($"{nxPref}chritm_prop.farc", EntryStreamMode.MemoryStream));
            SpriteDatabase sprDb = BinaryFile.Load<SpriteDatabase>(main.Open($"{stPref}2d/spr_db.bin", EntryStreamMode.MemoryStream));
            ObjectDatabase objDb = BinaryFile.Load<ObjectDatabase>(main.Open($"{stPref}objset/obj_db.bin", EntryStreamMode.MemoryStream));
            TextureDatabase texDb = BinaryFile.Load<TextureDatabase>(main.Open($"{stPref}objset/tex_db.bin", EntryStreamMode.MemoryStream));
            main.Dispose();
            dlc.Dispose();
            ObservableCollection<CharacterItemFile> chr = IO.ReadCharaFile(chritm).Result;
            foreach (CharacterItemFile chrItm in chr)
            {
                foreach (CharacterCostumeEntry c in chrItm.costumes)
                {
                    if (!chritm_prop_cos[chrItm.chara.ToUpper()].ContainsKey(c.id))
                    {
                        chritm_prop_cos[chrItm.chara.ToUpper()].Add(c.id, reference);
                    }
                }
                foreach (CharacterItemEntry i in chrItm.items)
                {
                    if (!chritm_prop_item[chrItm.chara.ToUpper()].ContainsKey(i.no))
                    {
                        chritm_prop_item[chrItm.chara.ToUpper()].Add(i.no, reference);
                    }
                }
            }
            foreach (SpriteSetInfo spr in sprDb.SpriteSets)
            {
                if (!spr_db.ContainsKey(spr.Id))
                {
                    spr_db.Add(spr.Id, reference);
                }
                if (!itmFarcs.ContainsKey(spr.FileName))
                {
                    itmFarcs.Add(spr.FileName, reference);
                }
                foreach (SpriteInfo sp in spr.Sprites)
                {
                    if (!spr_db_spr.ContainsKey(sp.Id))
                    {
                        spr_db_spr.Add(sp.Id, reference);
                    }
                }
                foreach (SpriteTextureInfo st in spr.Textures)
                {
                    if (!spr_db_tex.ContainsKey(st.Id))
                    {
                        spr_db_tex.Add(st.Id, reference);
                    }
                }
            }
            foreach (ObjectSetInfo objInfo in objDb.ObjectSets)
            {
                if (!obj_db.ContainsKey(objInfo.Id))
                {
                    obj_db.Add(objInfo.Id, reference);
                }
                if (!itmFarcs.ContainsKey(objInfo.ArchiveFileName))
                {
                    itmFarcs.Add(objInfo.ArchiveFileName, reference);
                }
            }
            foreach (TextureInfo texInfo in texDb.Textures)
            {
                if (!tex_db.ContainsKey(texInfo.Id))
                {
                    tex_db.Add(texInfo.Id, reference);
                }
                if (!tex_db_names.ContainsKey(texInfo.Name))
                {
                    tex_db_names.Add(texInfo.Name, reference);
                }
            }
            foreach (Module m in IO.ReadModuleFile(module).Result)
            {
                if (!module_tbl.ContainsKey(m.id))
                {
                    module_tbl.Add(m.id, reference);
                }
                if (!module_tbl_index.ContainsKey(m.sort_index))
                {
                    module_tbl_index.Add(m.sort_index, reference);
                }
            }
            foreach (CustomizeItem c in IO.ReadCustomFile(customize).Result)
            {
                if (!customize_item_tbl.ContainsKey(c.id))
                {
                    customize_item_tbl.Add(c.id, reference);
                }
                if (!customize_item_tbl_index.ContainsKey(c.sort_index))
                {
                    customize_item_tbl_index.Add(c.sort_index, reference);
                }
            }
        }

        public async Task GetDMAIds()
        {
            string jsonModules = await File.ReadAllTextAsync(Settings.Default.dmaPath + "\\modules.ini");
            string jsonTextures = await File.ReadAllTextAsync(Settings.Default.dmaPath + "\\textures.ini");
            string jsonObjects = await File.ReadAllTextAsync(Settings.Default.dmaPath + "\\objects.ini");
            string jsonCustoms = await File.ReadAllTextAsync(Settings.Default.dmaPath + "\\cstm_items.ini");
            string jsonSprites = await File.ReadAllTextAsync(Settings.Default.dmaPath + "\\sprites.ini");
            string jsonSpriteSets = await File.ReadAllTextAsync(Settings.Default.dmaPath + "\\sprite_sets.ini");
            string jsonPosts = await File.ReadAllTextAsync(Settings.Default.dmaPath + "\\posts.ini");
            //start modules
            try
            {
                JsonArray posts = (JsonArray)JsonNode.Parse(jsonPosts);
                JsonObject job = (JsonObject)JsonNode.Parse(jsonModules);
                Dictionary<int, string> postRef = new()
                {
                    { 0, "[DMA]Mega Mix+" }
                };
                
                var modules = (JsonObject)job["uploaded_modules"];
                var posts2 = (JsonObject)job["posts"];
                foreach (var obj in posts)
                {
                    if (!postRef.ContainsKey((int)obj["id"]))
                    {
                        postRef.Add((int)obj["id"], $"[DMA]{obj["name"]}");
                    }
                }
                foreach (KeyValuePair<string, JsonNode?> obj in posts2)
                {
                    if (!postRef.ContainsKey((int)obj.Value["id"]))
                    {
                        postRef.Add((int)obj.Value["id"], $"[DMA]{obj.Value["name"]}");
                    }
                }
                foreach (var obj in modules)
                {
                    int post;
                    if (obj.Value[0]["post"] == null)
                    {
                        post = 0;
                    }
                    else
                    {
                        post = (int)obj.Value[0]["post"];
                    }
                    int module_id = (int)obj.Value[0]["id"];
                    int cos_id = (int)obj.Value[0]["module"]["cos"]["id"];
                    string chara = (string)obj.Value[0]["module"]["chara"];
                    if (module_tbl.ContainsKey((int)obj.Value[0]["id"]))
                    {
                        conflicts.module_tbl.Add($"{module_id}: {module_tbl[module_id]} and {postRef[post]}");
                    }
                    else
                    {
                        module_tbl.Add(module_id, postRef[post]);
                    }
                    if (chritm_prop_cos[chara].ContainsKey(cos_id))
                    {
                        conflicts.chritm_prop_cos[chara].Add($"{cos_id}: {chritm_prop_cos[chara][cos_id]} and {postRef[post]}");
                    }
                    else
                    {
                        chritm_prop_cos[chara].Add(cos_id, postRef[post]);
                    }
                    foreach (var obj2 in obj.Value[0]["module"]["cos"]["items"].AsArray())
                    {
                        int item_id = (int)obj2["id"];
                        if (chritm_prop_item[chara].ContainsKey(item_id))
                        {
                            conflicts.chritm_prop_item[chara].Add($"{item_id}: {chritm_prop_item[chara][item_id]} and {postRef[post]}");
                        }
                        else
                        {
                            chritm_prop_item[chara].Add(item_id, postRef[post]);
                        }
                        foreach (var obj3 in obj2["objset"] as JsonArray)
                        {
                            string farc = ((string)obj3 + ".farc").ToLower();
                            if (itmFarcs.ContainsKey(farc))
                            {
                                conflicts.itmFarcs.Add($"{farc}: {itmFarcs[farc]} and {postRef[post]}");
                            }
                            else
                            {
                                itmFarcs.Add(farc, postRef[post]);
                            }
                        }
                    }
                }
                foreach (var obj in (JsonObject)job["reserved_modules"])
                {
                    string post = (string)obj.Value["label"];
                    int id = (int)obj.Value["id"];
                    if (module_tbl.ContainsKey(id))
                    {
                        conflicts.module_tbl.Add($"{id}: {module_tbl[id]} and [DMA Reserved] {post}");
                    }
                    else
                    {
                        module_tbl.Add(id, "[DMA Reserved] " + post);
                    }
                }
                //TO-DO reserved costumes
                //end modules
                //start cstm_item
                JsonObject job_c = (JsonObject)JsonNode.Parse(jsonCustoms);
                var customs = (JsonObject)job_c["uploaded_cstm_items"];
                var posts3 = (JsonObject)job_c["posts"];
                foreach (KeyValuePair<string, JsonNode?> obj in posts3)
                {
                    if (!postRef.ContainsKey((int)obj.Value["id"]))
                    {
                        postRef.Add((int)obj.Value["id"], $"[DMA]{obj.Value["name"]}");
                    }
                }
                foreach (var obj in customs)
                {
                    int post;
                    if (obj.Value[0]["post"] == null)
                    {
                        post = 0;
                    }
                    else
                    {
                        post = (int)obj.Value[0]["post"];
                    }
                    int custom_id = (int)obj.Value[0]["id"];
                    if (customize_item_tbl.ContainsKey((int)obj.Value[0]["id"]))
                    {
                        conflicts.customize_tbl.Add($"{custom_id}: {customize_item_tbl[custom_id]} and {postRef[post]}");
                    }
                    else
                    {
                        customize_item_tbl.Add((int)obj.Value[0]["id"], postRef[post]);
                    }
                    int obj_id = (int)obj.Value[0]["cstm_item"]["obj_id"];
                    string chara = ((string)obj.Value[0]["cstm_item"]["chara"]);
                    if (chara == "ALL")
                    {
                        string fileName = "CMNITM" + obj_id + ".farc";
                        foreach (string chara1 in charas)
                        {
                            string chr = chara1.Remove(3);
                            if (chritm_prop_item[chr].ContainsKey(obj_id))
                            {
                                conflicts.chritm_prop_item[chr].Add($"{obj_id}: {chritm_prop_item[chr][obj_id]} and {postRef[post]}");
                            }
                            else
                            {
                                chritm_prop_item[chr].Add(obj_id, postRef[post]);
                            }
                        }
                        if (itmFarcs.ContainsKey(fileName))
                        {
                            conflicts.itmFarcs.Add($"{fileName}: {itmFarcs[fileName]} and {postRef[post]}");

                        }
                        else
                        {
                            itmFarcs.Add(fileName, postRef[post]);
                        }
                    }
                    else
                    {
                        string fileName = chara + "ITM" + obj_id + ".farc";
                        if (chritm_prop_item[chara].ContainsKey(obj_id))
                        {
                            conflicts.chritm_prop_item[chara].Add($"{obj_id}: {chritm_prop_item[chara][obj_id]} and {postRef[post]}");
                        }
                        else
                        {
                            chritm_prop_item[chara].Add(obj_id, postRef[post]);
                        }
                        if (itmFarcs.ContainsKey(fileName))
                        {
                            conflicts.itmFarcs.Add($"{fileName}: {itmFarcs[fileName]} and {postRef[post]}");
                        }
                        else
                        {
                            itmFarcs.Add(fileName, postRef[post]);
                        }
                    }
                }
                foreach (var obj in (JsonObject)job_c["reserved_cstm_items"])
                {
                    int id = (int)obj.Value["id"];
                    string post = "[DMA Reserved] " + (string)obj.Value["label"];
                    if (customize_item_tbl.ContainsKey(id))
                    {
                        conflicts.customize_tbl.Add($"{id}: {customize_item_tbl[id]} and {post}");
                    }
                    else
                    {
                        customize_item_tbl.Add(id, post);
                    }
                }
                //end cstm_item
                //start textures
                JsonObject job_t = (JsonObject)JsonNode.Parse(jsonTextures);
                var textures = (JsonObject)job_t["entries"];
                var posts4 = (JsonObject)job_t["posts"];
                foreach (KeyValuePair<string, JsonNode?> obj in posts4)
                {
                    if (!postRef.ContainsKey((int)obj.Value["id"]))
                    {
                        Console.WriteLine($"{(int)obj.Value["id"]} - {(string)obj.Value["name"]}");
                        postRef.Add((int)obj.Value["id"], $"[DMA]{obj.Value["name"]}");
                    }
                }
                foreach (var obj in textures)
                {
                    int post;
                    if ((int)obj.Value[0]["post_id"] == -1)
                    {
                        post = 0;
                    }
                    else
                    {
                        post = (int)obj.Value[0]["post_id"];
                    }
                    uint id = (uint)obj.Value[0]["id"];
                    string name = (string)obj.Value[0]["name"];
                    if (tex_db.ContainsKey(id))
                    {
                        conflicts.tex_db.Add($"{id}: {tex_db[id]} and {postRef[post]}");
                    }
                    else
                    {
                        tex_db.Add((uint)obj.Value[0]["id"], postRef[post] + " (Texture DB)");
                    }
                    if (tex_db_names.ContainsKey(name))
                    {
                        conflicts.tex_db_names.Add($"{name}: {tex_db_names[name]} and {postRef[post]}");
                    }
                    else
                    {
                        tex_db_names.Add(name, postRef[post]);
                    }
                }
                //end textures
                //start objects
                JsonObject job_o = (JsonObject)JsonNode.Parse(jsonObjects);
                var objects = (JsonObject)job_o["entries"];
                var posts5 = (JsonObject)job_o["posts"];
                foreach (KeyValuePair<string, JsonNode?> obj in posts5)
                {
                    if (!postRef.ContainsKey((int)obj.Value["id"]))
                    {
                        postRef.Add((int)obj.Value["id"], $"[DMA]{obj.Value["name"]}");
                    }
                }
                foreach (var obj in objects)
                {
                    int post;
                    if ((int)obj.Value[0]["post_id"] == -1)
                    {
                        post = 0;
                    }
                    else
                    {
                        post = (int)obj.Value[0]["post_id"];
                    }
                    uint id = (uint)obj.Value[0]["id"];
                    string name = (string)obj.Value[0]["name"] + ".farc";
                    if (obj_db.ContainsKey(id))
                    {
                        conflicts.obj_db.Add($"{id}: {obj_db[id]} and {postRef[post]}");
                    }
                    else
                    {
                        obj_db.Add(id, postRef[post]);
                    }
                    if (itmFarcs.ContainsKey(name))
                    {
                        conflicts.itmFarcs.Add($"{name}: {itmFarcs[name]} and {postRef[post]}");
                    }
                    else
                    {
                        itmFarcs.Add(name, postRef[post]);
                    }
                }
                //end objects
                //start sprites
                JsonObject job_s = (JsonObject)JsonNode.Parse(jsonSprites);
                var sprites = (JsonObject)job_s["entries"];
                var posts6 = (JsonObject)job_s["posts"];
                foreach (KeyValuePair<string, JsonNode?> obj in posts6)
                {
                    if (!postRef.ContainsKey((int)obj.Value["id"]))
                    {
                        postRef.Add((int)obj.Value["id"], $"[DMA]{obj.Value["name"]}");
                    }
                }
                foreach (var obj in sprites)
                {
                    int post;
                    if ((int)obj.Value[0]["post_id"] == -1)
                    {
                        post = 0;
                    }
                    else
                    {
                        post = (int)obj.Value[0]["post_id"];
                    }
                    uint id = (uint)obj.Value[0]["id"];
                    if (spr_db_spr.ContainsKey(id))
                    {
                        conflicts.spr_db_spr.Add($"{id}: {spr_db_spr[id]} and {postRef[post]}");
                    }
                    else
                    {
                        spr_db_spr.Add(id, postRef[post]);
                    }
                }
                //start sprite sets
                JsonObject job_spr = (JsonObject)JsonNode.Parse(jsonSpriteSets);
                var sprSets = (JsonObject)job_spr["entries"];
                var posts7 = (JsonObject)job_spr["posts"];
                foreach (KeyValuePair<string, JsonNode?> obj in posts7)
                {
                    if (!postRef.ContainsKey((int)obj.Value["id"]))
                    {
                        postRef.Add((int)obj.Value["id"], $"[DMA]{obj.Value["name"]}");
                    }
                }
                foreach (var obj in sprSets)
                {
                    int post;
                    if ((int)obj.Value[0]["post_id"] == -1)
                    {
                        post = 0;
                    }
                    else
                    {
                        post = (int)obj.Value[0]["post_id"];
                    }
                    uint id = (uint)obj.Value[0]["id"];
                    string name = ((string)obj.Value[0]["name"] + ".farc").ToLower();
                    if (spr_db.ContainsKey((uint)obj.Value[0]["id"]))
                    {
                        conflicts.spr_db.Add($"{id}: {spr_db[id]} and {postRef[post]}");
                    }
                    else
                    {
                        spr_db.Add(id, postRef[post]);
                    }
                    if(itmFarcs.ContainsKey(name))
                    {
                        conflicts.itmFarcs.Add($"{name}: {itmFarcs[name]} and {postRef[post]}");
                    }
                    else
                    {
                        itmFarcs.Add(name, postRef[post]);
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace(e, true);
                StackFrame stf = st.GetFrame(st.FrameCount - 1);
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { PopupNotification pop = new(e.Message); }));
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => {
                    PopupNotification pop = new("Line number: " + stf.GetFileLineNumber().ToString());
                        }));
                //end sprites
            }
        }

        public string GetCleanPath(string path)
        {
            string half = path.Split("mods")[1];
            return half.Split("\\")[1];
        }

        public async Task GetUsedModIds(bool useDMA)
        {
            if (Directory.Exists(Settings.Default.gamePath + "\\mods"))
            {
                List<string> obj_dbs = Directory.EnumerateFiles(Settings.Default.gamePath + "\\mods", "*obj_db.bin", SearchOption.AllDirectories).ToList();
                List<string> tex_dbs = Directory.EnumerateFiles(Settings.Default.gamePath + "\\mods", "*tex_db.bin", SearchOption.AllDirectories).ToList();
                List<string> spr_dbs = Directory.EnumerateFiles(Settings.Default.gamePath + "\\mods", "*spr_db.bin", SearchOption.AllDirectories).ToList();
                List<string> module_tbls = Directory.EnumerateFiles(Settings.Default.gamePath + "\\mods", "*gm_module_tbl.farc", SearchOption.AllDirectories).ToList();
                List<string> customize_tbls = Directory.EnumerateFiles(Settings.Default.gamePath + "\\mods", "*gm_customize_item_tbl.farc", SearchOption.AllDirectories).ToList();
                List<string> chritm_props = Directory.EnumerateFiles(Settings.Default.gamePath + "\\mods", "*chritm_prop.farc", SearchOption.AllDirectories).ToList();
                foreach (string o_db in obj_dbs)
                {
                    ObjectDatabase db = new();
                    db.Load(o_db);
                    foreach (ObjectSetInfo objSetInfo in db.ObjectSets)
                    {
                        if (!obj_db.ContainsKey(objSetInfo.Id))
                        {
                            obj_db.Add(objSetInfo.Id, GetCleanPath(o_db));
                        }
                        else
                        {
                            conflicts.obj_db.Add($"{objSetInfo.Id}: {obj_db[objSetInfo.Id]} and {GetCleanPath(o_db)}");
                        }
                        if (!itmFarcs.ContainsKey(objSetInfo.ArchiveFileName))
                        {
                            itmFarcs.Add(objSetInfo.ArchiveFileName, GetCleanPath(o_db));
                        }
                        else
                        {
                            conflicts.itmFarcs.Add($"{objSetInfo.ArchiveFileName}: {itmFarcs[objSetInfo.ArchiveFileName]} and {GetCleanPath(o_db)}");
                        }
                    }
                    db.Dispose();
                }
                foreach (string t_db in tex_dbs)
                {
                    TextureDatabase db = new();
                    db.Load(t_db);
                    foreach (TextureInfo texInfo in db.Textures)
                    {
                        if (!tex_db.ContainsKey(texInfo.Id))
                        {
                            tex_db.Add(texInfo.Id, GetCleanPath(t_db));
                        }
                        else
                        {
                            conflicts.obj_db.Add($"{texInfo.Id}: {tex_db[texInfo.Id]} and {GetCleanPath(t_db)}");
                        }
                        if (!tex_db_names.ContainsKey(texInfo.Name))
                        {
                            tex_db_names.Add(texInfo.Name, GetCleanPath(t_db));
                        }
                        else
                        {
                            conflicts.tex_db_names.Add($"{texInfo.Name}: {tex_db_names[texInfo.Name]} and {GetCleanPath(t_db)}");
                        }
                    }
                    db.Dispose();
                }
                foreach (string s_db in spr_dbs)
                {
                    SpriteDatabase db = new();
                    db.Load(s_db);
                    foreach (SpriteSetInfo sprSet in db.SpriteSets)
                    {
                        if (!spr_db.ContainsKey(sprSet.Id))
                        {
                            spr_db.Add(sprSet.Id, GetCleanPath(s_db));
                        }
                        else
                        {
                            conflicts.spr_db.Add($"{sprSet.Id}: {spr_db[sprSet.Id]} and {GetCleanPath(s_db)}");
                        }
                        foreach (SpriteInfo sprInfo in sprSet.Sprites)
                        {
                            if (!spr_db_spr.ContainsKey(sprInfo.Id))
                            {
                                spr_db_spr.Add(sprInfo.Id, GetCleanPath(s_db));
                            }
                            else
                            {
                                conflicts.spr_db_spr.Add($"{sprInfo.Id}: {spr_db_spr[sprInfo.Id]} and {GetCleanPath(s_db)}");
                            }
                        }
                        foreach (SpriteTextureInfo sprTexInfo in sprSet.Textures)
                        {
                            if (!spr_db_tex.ContainsKey(sprTexInfo.Id))
                            {
                                spr_db_tex.Add(sprTexInfo.Id, GetCleanPath(s_db));
                            }
                            else
                            {
                                conflicts.spr_db_tex.Add($"{sprTexInfo.Id}: {spr_db_tex[sprTexInfo.Id]} and {GetCleanPath(s_db)}");
                            }
                        }
                    }
                    db.Dispose();
                }
                foreach (string m_tbl in module_tbls)
                {
                    FarcArchive m_farc = BinaryFile.Load<FarcArchive>(m_tbl);
                    ObservableCollection<Module> modules = await IO.ReadModuleFile(m_farc);
                    foreach (Module m in modules)
                    {
                        if (!module_tbl.ContainsKey(m.id))
                        {
                            module_tbl.Add(m.id, GetCleanPath(m_tbl));
                        }
                        else
                        {
                            conflicts.module_tbl.Add($"{m.id}: {module_tbl[m.id]} and {GetCleanPath(m_tbl)}");
                        }
                        if (!module_tbl_index.ContainsKey(m.sort_index))
                        {
                            module_tbl_index.Add(m.sort_index, GetCleanPath(m_tbl));
                        }
                        else
                        {
                            conflicts.module_tbl_index.Add($"{m.sort_index}: {module_tbl_index[m.sort_index]} and {GetCleanPath(m_tbl)}");
                        }
                    }
                    m_farc.Dispose();
                }
                foreach (string c_tbl in customize_tbls)
                {
                    FarcArchive c_farc = BinaryFile.Load<FarcArchive>(c_tbl);
                    ObservableCollection<CustomizeItem> customizeItems = await IO.ReadCustomFile(c_farc);
                    foreach (CustomizeItem i in customizeItems)
                    {
                        if (!customize_item_tbl.ContainsKey(i.id))
                        {
                            customize_item_tbl.Add(i.id, GetCleanPath(c_tbl));
                        }
                        else
                        {
                            conflicts.customize_tbl.Add($"{i.id}: {customize_item_tbl[i.id]} and {GetCleanPath(c_tbl)}");
                        }
                        if (!customize_item_tbl_index.ContainsKey(i.sort_index))
                        {
                            customize_item_tbl_index.Add(i.sort_index, GetCleanPath(c_tbl));
                        }
                        else
                        {
                            conflicts.customize_tbl_index.Add($"{i.sort_index}: {customize_item_tbl_index[i.sort_index]} and {GetCleanPath(c_tbl)}");
                        }
                    }
                    c_farc.Dispose();
                }
                foreach (string chr_tbl in chritm_props)
                {
                    FarcArchive chr_farc = BinaryFile.Load<FarcArchive>(chr_tbl);
                    ObservableCollection<CharacterItemFile> characterItemFiles = await IO.ReadCharaFile(chr_farc);
                    foreach (CharacterItemFile characterItemEntry in characterItemFiles)
                    {
                        foreach (CharacterItemEntry itemEntry in characterItemEntry.items)
                        {
                            if (!chritm_prop_item[characterItemEntry.chara.ToUpper()].ContainsKey(itemEntry.no))
                            {
                                chritm_prop_item[characterItemEntry.chara.ToUpper()].Add(itemEntry.no, GetCleanPath(chr_tbl));
                            }
                            else
                            {
                                conflicts.chritm_prop_item[characterItemEntry.chara.ToUpper()].Add($"{itemEntry.no}: {chritm_prop_item[characterItemEntry.chara.ToUpper()][itemEntry.no]} and {GetCleanPath(chr_tbl)}");
                            }
                        }
                        foreach (CharacterCostumeEntry cosEntry in characterItemEntry.costumes)
                        {
                            if (!chritm_prop_cos[characterItemEntry.chara.ToUpper()].ContainsKey(cosEntry.id))
                            {
                                chritm_prop_cos[characterItemEntry.chara.ToUpper()].Add(cosEntry.id, GetCleanPath(chr_tbl));
                            }
                            else
                            {
                                conflicts.chritm_prop_cos[characterItemEntry.chara.ToUpper()].Add($"{cosEntry.id}: {chritm_prop_cos[characterItemEntry.chara.ToUpper()][cosEntry.id]} and {GetCleanPath(chr_tbl)}");
                            }
                        }
                    }
                }
            }
            else { throw new Exception("no mods folder?"); }
            if (useDMA)
            {
                if (Settings.Default.dmaPath == null || Settings.Default.lastDmaUpdate.Date != DateTime.Today)
                {
                    await IO.DMAUpdate();
                    await GetDMAIds();
                }
                else
                {
                    await GetDMAIds();
                }
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
        public int id { get; set; } = 1;
        public string name { get; set; }
        public bool ng { get; set; }
        public string shop_price { get; set; }
        public int shop_st_day { get; set; } = 1;
        public int shop_st_month { get; set; } = 1;
        public int shop_st_year { get; set; } = 2009;
        public int sort_index { get; set; }
        public virtual List<string> getEntry()
        {
            List<string> x = new();
            return x;
        }
    }

    public class Module : Entry
    {
        public Attr attr { get; set; } = Attr.Default_CT;
        public string cos { get; set; }
        public override List<string> getEntry()
        {
            List<string> x = new();
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

    public class CustomizeItem : Entry
    {
        public int bind_module { get; set; }
        public int obj_id { get; set; }
        public string parts { get; set; }
        public bool sell_type { get; set; }
        public override List<string> getEntry()
        {
            List<string> x = new();
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
    public class CharacterCostumeEntry
    {
        public string entry { get; set; }
        public int id { get; set; } = 9999;
        public ObservableCollection<int> items { get; set; } = new ObservableCollection<int> { 500, 1, 300 };
        public List<string> getEntry()
        {
            List<string> x = new();
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
    public class DataSetTex
    {
        public string entry { get; set; }
        public string chg { get; set; } = "ENTER NEW TEX NAME";
        public string org { get; set; } = "ENTER OLD TEX NAME";
    }
    public class CharacterItemEntry
    {
        public string entry { get; set; }
        public int attr { get; set; } = 0;
        public int rpk = -1; // N/A
        public string uid { get; set; } = "DUMMY_DIVSKN";
        public ObservableCollection<DataSetTex> dataSetTex { get; set; } = new ObservableCollection<DataSetTex>();
        public int desID { get; set; } = 0;
        public decimal face_depth { get; set; } = 0;
        public int flag = 0;
        public string name { get; set; } = "DUMMY";
        public int no { get; set; } = 9999;
        public List<string> objset { get; set; } = new List<string> { "MIKITM001" };
        public int orgItm { get; set; } = 0;
        public int subID { get; set; } = 0;
        public int type { get; set; } = 0;

        public List<string> GetEntry()
        {
            string a = "item." + entry + ".";
            if(uid == "NULL")
            {
                rpk = 1;
            }
            List<string> x = new()
            {
                a + "attr=" + attr,
                a + "data.obj.0.rpk=" + rpk,
                a + "data.obj.0.uid=" + uid,
                a + "data.obj.length=1"
            };
            int count = 0;
            foreach (DataSetTex tex in dataSetTex)
            {
                tex.entry = count.ToString();
                count++;
            }
            foreach (DataSetTex tex in dataSetTex.OrderBy(o => o.entry))
            {
                x.Add(a + "data.tex." + tex.entry + ".chg=" + tex.chg);
                x.Add(a + "data.tex." + tex.entry + ".org=" + tex.org);
            }
            if (dataSetTex.Count > 0)
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
    public class CharacterItemFile
    {
        public string chara { get; set; }
        public List<CharacterCostumeEntry> costumes { get; set; } = new();
        public List<CharacterItemEntry> items { get; set; } = new();
        public string GetFullName()
        {
            switch (chara)
            {
                case "mik":
                    return Resources.name_miku;
                case "rin":
                    return Resources.name_rin;
                case "len":
                    return Resources.name_len;
                case "luk":
                    return Resources.name_luka;
                case "kai":
                    return Resources.name_kaito;
                case "mei":
                    return Resources.name_meiko;
                case "sak":
                    return Resources.name_sakine;
                case "ner":
                    return Resources.name_neru;
                case "hak":
                    return Resources.name_haku;
                case "tet":
                    return Resources.name_teto;
                default:
                    return "Unknown・不明";
            }
        }
        public string GetShortName()
        {
            switch (chara)
            {
                case "mik":
                    return "MIKU";
                case "rin":
                    return "RIN";
                case "len":
                    return "LEN";
                case "luk":
                    return "LUKA";
                case "kai":
                    return "KAITO";
                case "mei":
                    return "MEIKO";
                case "sak":
                    return "SAKINE";
                case "ner":
                    return "NERU";
                case "hak":
                    return "HAKU";
                case "tet":
                    return "TETO";
                default:
                    return "TETO";
            }
        }
        public string GetFileName()
        {
            string x = chara + "itm_tbl.txt";
            return x;
        }
    }
    public class WizardObject
    {
        public string objectFilePath;
        public ObjectSetInfo objectSet = new();
        public bool isExisting;
        public CharacterItemEntry item = new();
    }
    public class WizardEntry
    {
        public bool isItem;
        public List<WizardObject> objects = new();
        public LocalisedNames localNames = new();
        public bool hairNG;
        public string parts;
        public string name = Resources.cmn_temp;
        public int id = -1;
        public string chara;
        public int sort_index = 999;
        public List<CharacterItemEntry> existingItems = new();
        public Bitmap bitmap = Resources.md_dummy;
    }

    public class LocalisedNames
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

    public class Downloader
    {
        public List<FileDownloadEventArgs> downloads = new();
        public Downloader()
        {
            IsFinished += DownloadFinished;
        }
        public event EventHandler<FileDownloadEventArgs> IsFinished;
        void DownloadFinished(object sender, FileDownloadEventArgs e)
        {
            Debug.WriteLine($"{e.FileName} Downloaded");
            downloads.Remove(e);
            if (downloads.Count == 0)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    PopupNotification popup = new(Resources.notice_download_finished);
                });
            }
        }
        public virtual void OnDownloadFinished(FileDownloadEventArgs e)
        {
            IsFinished?.Invoke(this, e);
        }
        public async Task DownloadFileASync(string url, string path, string fileName)
        {
            FileDownloadEventArgs args = new()
            {
                FileName = fileName,
            };
            downloads.Add(args);
            int index = downloads.IndexOf(args);
            for (int i=1; i<4; i++) //try 3 times before giving up
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(180); //wait at least 3 minutes so slow connections dont die
                        var response = await client.GetAsync(url);
                        using (FileStream fs = new FileStream(path + "/" + fileName, FileMode.OpenOrCreate))
                        {
                            fs.SetLength((int)response.Content.Headers.ContentLength); //MUST set length otherwise it has the potential to dl extra garbage and break
                            await response.Content.CopyToAsync(fs);
                            OnDownloadFinished(args);
                        }
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Unable to download file. (Attempt {i}/3)");
                    Debug.WriteLine(e.Message);
                    downloads.Remove(args);
                    if (i == 3)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            PopupNotification popup = new($"{Resources.notice_download_fail}\n{Resources.notice_download_failed_file} {args.FileName}.\n{Resources.notice_try_again}");
                        });
                        Settings.Default.lastDmaUpdate = DateTime.MinValue;
                    }
                }
            }
        }
    }

    public class FileDownloadEventArgs : EventArgs
    {
        public string FileName { get; set; }
    }
}
