using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics;
using SharpDX.XInput;
using WeScriptWrapper;
using WeScript.SDK.UI;
using WeScript.SDK.UI.Components;
using System.Runtime.InteropServices; //for StructLayout
using System.Security.Permissions;
using WeScript.SDK.Utils;

namespace EscapeFromTarkov
{
    class Program
    {

        public static IntPtr ModuleBase = IntPtr.Zero;
        public static IntPtr processHandle = IntPtr.Zero; //processHandle variable used by OpenProcess (once)
        public static bool gameProcessExists = false; //avoid drawing if the game process is dead, or not existent
        public static bool isWow64Process = false; //we all know the game is 32bit, but anyway...
        public static bool isGameOnTop = false; //we should avoid drawing while the game is not set on top
        public static bool isOverlayOnTop = false; //we might allow drawing visuals, while the user is working with the "menu"
        public static uint PROCESS_ALL_ACCESS = 0x1FFFFF; //hardcoded access right to OpenProcess
        public static Vector2 wndMargins = new Vector2(0, 0); //if the game window is smaller than your desktop resolution, you should avoid drawing outside of it
        public static Vector2 wndSize = new Vector2(0, 0); //get the size of the game window ... to know where to draw 
        public static Vector2 AimTarg2D = new Vector2(0, 0); //for aimbot
        public static Vector3 AimTarg3D = new Vector3(0, 0, 0);
        public static Vector2 GameCenterPos = new Vector2(0, 0); //for crosshair and a
        public static IntPtr gameObjectManager = IntPtr.Zero;
        public static IntPtr GameWorld = IntPtr.Zero;
        public static List<ItemObj> LootItems = new List<ItemObj>();
        public static Dictionary<string, long> ItemsAddressHistory = new Dictionary<string, long>();
        public static DateTime ItemsRefereshDT = DateTime.Now;
        public static Dictionary<string, string> AmmoIdsToNamesDic = new Dictionary<string, string>
        {
            { "5447ac644bdc2d6c208b4567", "Ball 5.56x45"},
            { "54527a984bdc2d4e668b4567", "M855"},
            { "54527ac44bdc2d36668b4567", "M855A1"},
            { "560d5e524bdc2d25448b4571", "7mm 12c"},
            { "560d61e84bdc2da74d8b4571", "SNB"},
            { "560d75f54bdc2da74d8b4573", "SNB 7.62x54 R"},
            { "5649ed104bdc2d3d1c8b458b", "PS 7.62x39"},
            { "5656d7c34bdc2d9d198b4587", "PS"},
            { "5656eb674bdc2d35148b457c", "40mm"},
            { "56d59d3ad2720bdb418b4577", "Pst gzh"},
            { "56dfef82d2720bbd668b4567", "BP"},
            { "56dff026d2720bb8668b4567", "BS"},
            { "56dff061d2720bb5668b4567", "BT"},
            { "56dff0bed2720bb0668b4567", "FMJ"},
            { "56dff216d2720bbd668b4568", "HP"},
            { "56dff2ced2720bb4668b4567", "PP"},
            { "56dff338d2720bbd668b4569", "PRS"},
            { "56dff3afd2720bba668b4567", "PS"},
            { "56dff421d2720b5f5a8b4567", "SP"},
            { "56dff4a2d2720bbd668b456a", "T"},
            { "56dff4ecd2720b5f5a8b4568", "US"},
            { "5735fdcd2459776445391d61", "AKBS"},
            { "5735ff5c245977640e39ba7e", "FMJ43"},
            { "573601b42459776410737435", "LRN"},
            { "573602322459776445391df1", "LRNPC"},
            { "5736026a245977644601dc61", "P gl"},
            { "573603562459776430731618", "Pst gzh"},
            { "573603c924597764442bd9cb", "PT gzh"},
            { "573718ba2459775a75491131", "9 BZT gzh"},
            { "573719762459775a626ccbc1", "9 P gzh"},
            { "573719df2459775a626ccbc2", "PBM"},
            { "57371aab2459775a77142f22", "PMM"},
            { "57371b192459775a9f58a5e0", "PPe gzh"},
            { "57371e4124597760ff7b25f1", "PPT gzh"},
            { "57371eb62459776125652ac1", "PRS gs"},
            { "57371f2b24597761224311f1", "PS gs PPO"},
            { "57371f8d24597761006c6a81", "PSO gzh"},
            { "5737201124597760fc4431f1", "Pst gzh"},
            { "5737207f24597760ff7b25f2", "PSV"},
            { "573720e02459776143012541", "RG028 gzh"},
            { "57372140245977611f70ee91", "SP7 gzh"},
            { "5737218f245977612125ba51", "SP8 gzh"},
            { "573722e82459776104581c21", "BZHT gzh"},
            { "573724b42459776125652ac2", "P gzh"},
            { "5737250c2459776125652acc", "PBM"},
            { "5737256c2459776125652acd", "PMM"},
            { "573725b0245977612125bae2", "Ppe gzh"},
            { "5737260b24597761224311f2", "PPT gzh"},
            { "5737266524597761006c6a8c", "PRS gzh"},
            { "573726d824597765d96be361", "PS gs PPO"},
            { "5737273924597765dd374461", "PSO gzh"},
            { "573727c624597765cc785b5b", "Pst gzh"},
            { "5737280e24597765cc785b5c", "PSV"},
            { "5737287724597765e1625ae2", "RG028 gzh"},
            { "573728cc24597765cc785b5d", "SP7 gzh"},
            { "573728f324597765e5728561", "SP8 gzh"},
            { "5737292724597765e5728562", "BP gs"},
            { "57372a7f24597766fe0de0c1", "BP gs"},
            { "57372ac324597767001bc261", "BP gs"},
            { "57372b832459776701014e41", "BS gs"},
            { "57372bad245977670b7cd242", "BS gs"},
            { "57372bd3245977670b7cd243", "BS gs"},
            { "57372c21245977670937c6c2", "BT gs"},
            { "57372c56245977685e584582", "BT gs"},
            { "57372c89245977685d4159b1", "BT gs"},
            { "57372d1b2459776862260581", "PP gs"},
            { "57372d4c245977685a3da2a1", "PP gs"},
            { "57372db0245977685d4159b2", "PP gs"},
            { "57372deb245977685d4159b3", "PRS gs"},
            { "57372e1924597768553071c1", "PRS gs"},
            { "57372e4a24597768553071c2", "PRS gs"},
            { "57372e73245977685d4159b4", "PS gs"},
            { "57372e94245977685648d3e1", "PS gs"},
            { "57372ebf2459776862260582", "PS gs"},
            { "57372ee1245977685d4159b5", "T gs"},
            { "57372f2824597769a270a191", "T gs"},
            { "57372f5c24597769917c0131", "T gs"},
            { "57372f7d245977699b53e301", "US gs"},
            { "57372fc52459776998772ca1", "US gs"},
            { "5737300424597769942d5a01", "US gs"},
            { "5737330a2459776af32363a1", "FMJ"},
            { "5737339e2459776af261abeb", "HP"},
            { "573733c72459776b0b7b51b0", "SP"},
            { "5739d41224597779c3645501", "Pst Gzh"},
            { "57a0dfb82459774d3078b56c", "SP-5"},
            { "57a0e5022459774d1673f889", "SP-6"},
            { "58820d1224597753c90aeb13", "12x70 Slug"},
            { "58864a4f2459770fcc257101", "PSO gzh"},
            { "5887431f2459777e1612938f", "LPS Gzh"},
            { "58dd3ad986f77403051cba8f", "M80"},
            { "5943d9c186f7745a13413ac9", "Shrapnel"},
            { "5996f6cb86f774678763a6ca", "RGD-5 Shrapnel"},
            { "5996f6d686f77467977ba6cc", "F1 Shrapnel"},
            { "5996f6fc86f7745e585b4de3", "M67 Shrapnel"},
            { "59e0d99486f7744a32234762", "BP"},
            { "59e4cf5286f7741778269d8a", "T45M"},
            { "59e4d24686f7741776641ac7", "US"},
            { "59e4d3d286f774176a36250a", "HP"},
            { "59e6542b86f77411dc52a77a", "FMJ"},
            { "59e655cb86f77411dc52a77b", "EKO"},
            { "59e6658b86f77411d949b250", "Geksa"},
            { "59e68f6f86f7746c9f75e846", "M856"},
            { "59e6906286f7746c9f75e847", "M856A1"},
            { "59e690b686f7746c9f75e848", "M995"},
            { "59e6918f86f7746c9f75e849", "Mk255 Mod0"},
            { "59e6920f86f77411d82aa167", "55 FMJ"},
            { "59e6927d86f77411da468256", "55 HP"},
            { "59e77a2386f7742ee578960a", "7N1"},
            { "5a269f97c4a282000b151807", "SP10"},
            { "5a26abfac4a28232980eabff", "SP11"},
            { "5a26ac06c4a282000c5a90a8", "SP12"},
            { "5a26ac0ec4a28200741e1e18", "SP13"},
            { "5a38ebd9c4a282000d722a5b", "7.5 20c"},
            { "5a3c16fe86f77452b62de32a", "Luger CCI"},
            { "5a6086ea4f39f99cd479502f", "M61"},
            { "5a608bf24f39f98ffc77720e", "M62"},
            { "5ba2678ad4351e44f824b344", "FMJ SX"},
            { "5ba26812d4351e003201fef1", "Action SX"},
            { "5ba26835d4351e0035628ff5", "AP SX"},
            { "5ba26844d4351e00334c9475", "Subsonic SX"},
            { "5c0d56a986f774449d5de529", "RIP"},
            { "5c0d591486f7744c505b416f", "12x70 RIP"},
            { "5c0d5ae286f7741e46554302", "WG"},
            { "5c0d5e4486f77478390952fe", "7N39"},
            { "5c0d668f86f7747ccb7f13b2", "SPP"},
            { "5c0d688c86f77413ae3407b2", "BP"},
            { "5c11279ad174af029d64592b", "Warmage"},
            { "5c1127bdd174af44217ab8b9", "DIPP"},
            { "5c1127d0d174af29be75cf68", "12x70 DIPP"},
            { "5c1260dc86f7746b106e8748", "7N12"},
            { "5c12619186f7743f871c8a32", "7N9"},
            { "5c1262a286f7743f8a69aab2", "7N39"},
            { "5c3df7d588a4501f290594e5", "GT"},
            { "5c925fa22e221601da359b7b", "AP 6.3"},
            { "5cadf6ddae9215051e1c23b2", "PS12"},
            { "5cadf6e5ae921500113bb973", "PS12A"},
            { "5cadf6eeae921500134b2799", "PS12B"},
            { "5cc80f38e4a949001152b560", "SS190"},
            { "5cc80f53e4a949000e1ea4f8", "L191"},
            { "5cc80f67e4a949035e43bbba", "SB193"},
            { "5cc80f79e4a949033c7343b2", "SS198LF"},
            { "5cc80f8fe4a949033b0224a2", "SS197SR"},
            { "5cc86832d7f00c000d3a6e6c", "R37.F"},
            { "5cc86840d7f00c002412c56c", "R37.X"},
            { "5cde8864d7f00c0010373be1", "B-32"},
            { "5d2f2ab648f03550091993ca", "BZT-44M"},
            { "5d6e6772a4b936088465b17c", "5.25 12c"},
            { "5d6e67fba4b9361bc73bc779", "6.5 12c"},
            { "5d6e6806a4b936088465b17e", "8.5 12c"},
            { "5d6e6869a4b9361c140bcfde", "Grizzly"},
            { "5d6e6891a4b9361bd473feea", "P-3 12c"},
            { "5d6e689ca4b9361bc8618956", "P-6u 12c"},
            { "5d6e68a8a4b9360b6c0d54e2", "AP-20"},
            { "5d6e68b3a4b9361bca7e50b5", "CSP"},
            { "5d6e68c4a4b9361b93413f79", ".50 12c"},
            { "5d6e68d1a4b93622fe60e845", "SF"},
            { "5d6e68dea4b9361bcc29e659", "2-Sabot"},
            { "5d6e68e6a4b9361c140bcfe0", "FTX"},
            { "5d6e6911a4b9361bd5780d52", "Flech."},
            { "5d6e695fa4b936359b35d852", "5.6 20c"},
            { "5d6e69b9a4b9361bc8618958", "6.2 20k"},
            { "5d6e69c7a4b9360b6c0d54e4", "7.3 20c"},
            { "5d6e6a05a4b93618084f58d0", "20/70 Slug"},
            { "5d6e6a42a4b9364f07165f52", "P-6u 20c"},
            { "5d6e6a53a4b9361bd473feec", "P-3 20c"},
            { "5d6e6a5fa4b93614ec501745", "Dev."},
            { "5e023cf8186a883be655e54f", "T46M"},
            { "5e023d34e8a400319a28ed44", "7BT1"},
            { "5e023d48186a883be655e551", "7N37"},
            { "5e023e53d4353e3302577c4c", "BPZ FMJ"},
            { "5e023e6e34d52a55c3304f71", "TPZ SP"},
            { "5e023e88277cce2b522ff2b1", "Ultra Nosler"},
            { "5e81f423763d9f754677bf2e", ".45 FMJ"},
            { "5ea2a8e200685063ec28c05a", ".45 RIP"}
        };
        public static Dictionary<string, string> KeysIdsToNamesDic = new Dictionary<string, string>
        {
            { "5448ba0b4bdc2d02308b456c", "Factory"},
            { "5672c92d4bdc2d180f8b4567", "Room 118 Key"},
            { "5780cda02459777b272ede61", "306 Key"},
            { "5780cf692459777de4559321", "315 Key"},
            { "5780cf722459777a5108b9a1", "308 Key"},
            { "5780cf7f2459777de4559322", "Mark.Key"},
            { "5780cf942459777df90dcb72", "Room 214 Key"},
            { "5780cf9e2459777df90dcb73", "Room 218 Key"},
            { "5780cfa52459777dfb276eb1", "Room 220 Key"},
            { "5780d0532459777a5108b9a2", "Customs key"},
            { "5780d0652459777df90dcb74", "Gas station"},
            { "5780d07a2459777de4559324", "Cabin key"},
            { "57a349b2245977762b199ec7", "Key"},
            { "5913611c86f77479e0084092", "Cabin key"},
            { "5913651986f774432f15d132", "Sixpack"},
            { "59136a4486f774447a1ed172", "Gdesk"},
            { "59136e1e86f774432f15d133", "110 Key"},
            { "59136f6f86f774447a1ed173", "Key"},
            { "591382d986f774465a6413a7", "105 Key"},
            { "591383f186f7744a4c5edcf3", "104 Key"},
            { "5913877a86f774432f15d444", "Storage"},
            { "5913915886f774123603c392", "Checkpoint"},
            { "5914578086f774123569ffa4", "108 Key"},
            { "59148c8a86f774197930e983", "Room 204 Key"},
            { "59148f8286f7741b951ea113", "Safe"},
            { "591ae8f986f77406f854be45", "Yotota"},
            { "591afe0186f77431bd616a11", "ZB-014"},
            { "5937ee6486f77408994ba448", "Key"},
            { "5938144586f77473c2087145", "Key"},
            { "5938504186f7740991483f30", "Room 203 Key"},
            { "593858c486f774253a24cb52", "Key"},
            { "5938603e86f77435642354f4", "206 Key"},
            { "59387a4986f77401cc236e62", "114 Key"},
            { "5938994586f774523a425196", "103 Key"},
            { "593962ca86f774068014d9af", "Unk. key"},
            { "593aa4be86f77457f56379f8", "303 Key"},
            { "5a0dc45586f7742f6b0b73e3", "San.104"},
            { "5a0dc95c86f77452440fc675", "San. 112"},
            { "5a0ea64786f7741707720468", "San.107"},
            { "5a0ea69f86f7741cd5406619", "San.108"},
            { "5a0ea79b86f7741d4a35298e", "Storeroom"},
            { "5a0eb38b86f774153b320eb0", "SMW"},
            { "5a0eb6ac86f7743124037a28", "Cottage"},
            { "5a0ec6d286f7742c0b518fb5", "San.205"},
            { "5a0ec70e86f7742c0b518fba", "San.207"},
            { "5a0ee30786f774023b6ee08f", "San.216"},
            { "5a0ee34586f774023b6ee092", "San.220"},
            { "5a0ee37f86f774023657a86f", "San.221"},
            { "5a0ee4b586f7743698200d22", "San.206 "},
            { "5a0ee62286f774369454a7ac", "San.209 "},
            { "5a0ee72c86f77436955d3435", "San.213"},
            { "5a0ee76686f7743698200d5c", "San.216"},
            { "5a0eeb1a86f774688b70aa5c", "San.303"},
            { "5a0eeb8e86f77461257ed71a", "San.309 "},
            { "5a0eebed86f77461230ddb3d", "San.325 "},
            { "5a0eec9686f77402ac5c39f2", "San.310 "},
            { "5a0eecf686f7740350630097", "San. 313 "},
            { "5a0eed4386f77405112912aa", "San.314"},
            { "5a0eedb386f77403506300be", "San.322 "},
            { "5a0eee1486f77402aa773226", "San.328 "},
            { "5a0eff2986f7741fd654e684", "Safe 321 "},
            { "5a0f006986f7741ffd2fe484", "Safe "},
            { "5a0f045e86f7745b0f0d0e42", "Safe "},
            { "5a0f068686f7745b0d4ea242", "Safe "},
            { "5a0f075686f7745bcc42ee12", "Safe "},
            { "5a0f08bc86f77478f33b84c2", "Safe "},
            { "5a0f0f5886f7741c4e32a472", "Safe "},
            { "5a13ee1986f774794d4c14cd", "San.323"},
            { "5a13eebd86f7746fd639aa93", "San.218 "},
            { "5a13ef0686f7746e5a411744", "San.219"},
            { "5a13ef7e86f7741290491063", "San.301 "},
            { "5a13f24186f77410e57c5626", "San.222 "},
            { "5a13f35286f77413ef1436b0", "San.226 "},
            { "5a13f46386f7741dd7384b04", "San.306 "},
            { "5a144bdb86f7741d374bbde0", "San.205 "},
            { "5a144dfd86f77445cb5a0982", "San.203 "},
            { "5a1452ee86f7746f33111763", "San.222 "},
            { "5a145d4786f7744cbb6f4a12", "San.306 "},
            { "5a145d7b86f7744cbb6f4a13", "San.308 "},
            { "5a145ebb86f77458f1796f05", "San.316 "},
            { "5ad5ccd186f774446d5706e9", "OLI Office"},
            { "5ad5cfbd86f7742c825d6104", "Log. Office"},
            { "5ad5d20586f77449be26d877", "OLI Ut."},
            { "5ad5d49886f77455f9731921", "Pow. Ut."},
            { "5ad5d64486f774079b080af8", "Pharmacy"},
            { "5ad5d7d286f77450166e0a89", "KIBA"},
            { "5ad5db3786f7743568421cce", "MES"},
            { "5ad7217186f7746744498875", "OLI"},
            { "5ad7242b86f7740a6a3abd43", "IDEA "},
            { "5ad7247386f7747487619dc3", "Goshan"},
            { "5addaffe86f77470b455f900", "KIBA 2"},
            { "5c1d0c5f86f7744bb2683cf0", "Blue"},
            { "5c1d0d6d86f7744bb2683e1f", "Yellow"},
            { "5c1d0dc586f7744baf2e7b79", "Green"},
            { "5c1d0efb86f7744baf2e7b7b", "Red"},
            { "5c1d0f4986f7744bb01837fa", "Black"},
            { "5c1e2a1e86f77431ea0ea84c", "Lk.MO"},
            { "5c1e2d1f86f77431e9280bee", "Lk.TA(w)"},
            { "5c1e495a86f7743109743dfb", "Violet"},
            { "5c1f79a086f7746ed066fb8f", "Lk.ASR"},
            { "5c94bbff86f7747ee735c08f", "Keycard"},
            { "5d08d21286f774736e7c94c3", "KSH"},
            { "5d80c60f86f77440373c4ece", "RB-BK"},
            { "5d80c62a86f7744036212b3f", "RB-VO"},
            { "5d80c66d86f774405611c7d6", "RB-AO"},
            { "5d80c6c586f77440351beef1", "RB-OB"},
            { "5d80c6fc86f774403a401e3c", "RB-TB"},
            { "5d80c78786f774403a401e3e", "RB-AK"},
            { "5d80c88d86f77440556dbf07", "RB-AM"},
            { "5d80c8f586f77440373c4ed0", "RB-OP"},
            { "5d80c93086f7744036212b41", "RB-MP11"},
            { "5d80c95986f77440351beef3", "RB-MP12"},
            { "5d80ca9086f774403a401e40", "RB-MP21"},
            { "5d80cab086f77440535be201", "RB-MP22"},
            { "5d80cb3886f77440556dbf09", "RB-PSP1"},
            { "5d80cb5686f77440545d1286", "RB-PS81"},
            { "5d80cb8786f774405611c7d9", "RB-PP"},
            { "5d80cbd886f77470855c26c2", "RB-MP13"},
            { "5d80ccac86f77470841ff452", "RB-ORB1"},
            { "5d80ccdd86f77474f7575e02", "RB-ORB2"},
            { "5d80cd1a86f77402aa362f42", "RB-ORB3"},
            { "5d8e0db586f7744450412a42", "RB-KORL"},
            { "5d8e0e0e86f774321140eb56", "RB-KPRL"},
            { "5d8e15b686f774445103b190", "HEPS"},
            { "5d8e3ecc86f774414c78d05e", "RB-GN"},
            { "5d947d3886f774447b415893", "RB-SMP"},
            { "5d947d4e86f774447b415895", "RB-KSM"},
            { "5d95d6be86f77424444eb3a7", "RB-PS82"},
            { "5d95d6fa86f77424484aa5e9", "RB-PSP2"},
            { "5d9f1fa686f774726974a992", "RB-ST"},
            { "5da46e3886f774653b7a83fe", "RB-RS"},
            { "5da5cdcd86f774529238fb9b", "RB-RH"},
            { "5da743f586f7744014504f72", "USEC key"},
            { "5e42c71586f7747f245e1343", "Med.St."},
            { "5e42c81886f7742a01529f57", "#11SR"},
            { "5e42c83786f7742a021fdf3c", "#21WS"},
        };
        public static Dictionary<string, string> InfoIdsToNamesDic = new Dictionary<string, string>
        {
            { "590c37d286f77443be3d7827", "SAS"},
            { "590c392f86f77444754deb29", "SSD"},
            { "590c621186f774138d11ea29", "Flash drive"},
            { "590c639286f774151567fa95", "Manual"},
            { "590c645c86f77412b01304d9", "Diary"},
            { "590c651286f7741e566b6461", "Diary"},
            { "5c12613b86f7743bbe2c3f76", "Intelligence"},
        };
        public static Dictionary<string, string> MedicalIdsToNamesDic = new Dictionary<string, string>
        {
            { "544fb25a4bdc2dfb738b4567", "Bandage"},
            { "544fb3364bdc2d34748b456a", "Splint"},
            { "544fb37f4bdc2dee738b4567", "Painkillers"},
            { "544fb3f34bdc2d03748b456a", "Morphine"},
            { "544fb45d4bdc2dee738b4568", "Salewa"},
            { "5751a25924597722c463c472", "Bandage"},
            { "5751a89d24597722aa0e8db0", "GoldenStar"},
            { "5755356824597772cb798962", "AI-2"},
            { "5755383e24597772cb798966", "Vaseline"},
            { "590c657e86f77412b013051d", "Grizzly"},
            { "590c661e86f7741e566b646a", "Car"},
            { "590c678286f77426c9660122", "IFAK"},
            { "590c695186f7741e566b64a2", "Augmentin"},
            { "5af0454c86f7746bf20992e8", "Alu Splint"},
            { "5af0548586f7743a532b7e99", "Ibuprofen"},
            { "5c0e530286f7747fa1419862", "Propital"},
            { "5c0e531286f7747fa54205c2", "SJ1 TGLabs"},
            { "5c0e531d86f7747fa23f4d42", "SJ6 TGLabs"},
            { "5c0e533786f7747fa23f4d47", "Zagustin"},
            { "5c0e534186f7747fa1419867", "eTG-c"},
            { "5c10c8fd86f7743d7d706df3", "Adrenaline"},
            { "5d02778e86f774203e7dedbe", "CMS"},
            { "5d02797c86f774203f38e30a", "Surv12"},
        };
        public static Dictionary<string, string> ProvisionsIdsToNamesDic = new Dictionary<string, string>
        {
            { "5448fee04bdc2dbc018b4567", "Water"},
            { "5448ff904bdc2d6f028b456e", "Crackers"},
            { "544fb62a4bdc2dfb738b4568", "Pineapp.Jc."},
            { "544fb6cc4bdc2d34748b456e", "Slickers"},
            { "5673de654bdc2d180f8b456d", "Saury"},
            { "5734773724597737fd047c14", "Cond. milk"},
            { "57347d3d245977448f7b7f61", "Croutons"},
            { "57347d5f245977448b40fa81", "Humpback"},
            { "57347d692459774491567cf1", "Peas"},
            { "57347d7224597744596b4e72", "Tushonka"},
            { "57347d8724597744596b4e76", "Squash"},
            { "57347d90245977448f7b7f65", "Oatflakes"},
            { "57347d9c245977448b40fa85", "Herring"},
            { "57347da92459774491567cf5", "Tushonka"},
            { "57505f6224597709a92585a9", "Alyonka"},
            { "575062b524597720a31c09a1", "Green Tea"},
            { "57513f07245977207e26a311", "Apl.jc."},
            { "57513f9324597720a7128161", "Pom.juice"},
            { "57513fcc24597720a31c09a6", "Vita juice"},
            { "5751435d24597720a27126d1", "NRG Drink"},
            { "57514643245977207f2c2d09", "TarCola"},
            { "575146b724597720a27126d5", "Milk"},
            { "5751487e245977207e26a315", "Emelya"},
            { "5751496424597720a27126da", "Hot Rod"},
            { "590c5d4b86f774784e1b9c45", "Lunchbox"},
            { "590c5f0d86f77413997acfab", "MRE"},
            { "59e3577886f774176a362503", "Sugar"},
            { "5bc9b156d4351e00367fbce9", "Mayo"},
            { "5bc9c29cd4351e003562b8a3", "Sprats"},
            { "5c0fa877d174af02a012e1cf", "Aquamari"},
            { "5d1b33a686f7742523398398", "Superwater"},
            { "5d1b376e86f774252519444e", "Moonshine"},
            { "5d403f9186f7743cac3f229b", "Whiskey"},
            { "5d40407c86f774318526545a", "Vodka"},
        };
        public static Dictionary<string, string> QuestIdsToNamesDic = new Dictionary<string, string>
        {
            { "590c62a386f77412b0130255", "Sliderkey"},
            { "590dde5786f77405e71908b2", "Сase"},
            { "5910922b86f7747d96753483", "Сase"},
            { "591092ef86f7747bb8703422", "Docs"},
            { "591093bb86f7747caa7bb2ee", "Letter"},
            { "5937fd0086f7742bf33fc198", "Watch"},
            { "5938188786f77474f723e87f", "Docs 0031"},
            { "5938878586f7741b797c562f", "Docs 0052"},
            { "593965cf86f774087a77e1b6", "Docs 0048"},
            { "5939a00786f7742fe8132936", "Zibbo"},
            { "5939e5a786f77461f11c0098", "Docs 0013"},
            { "5939e9b286f77462a709572c", "Letter"},
            { "593a87af86f774122f54a951", "Reagent"},
            { "5a0448bc86f774736f14efa8", "Sanatorium"},
            { "5a29276886f77435ed1b117c", "HDD"},
            { "5a29284f86f77463ef3db363", "Toughbook"},
            { "5a29357286f77409c705e025", "Flash drive"},
            { "5a294d7c86f7740651337cf9", "SAS disk"},
            { "5a294d8486f774068638cd93", "SAS disk"},
            { "5a6860d886f77411cd3a9e47", "Docs 0060"},
            { "5a687e7886f7740c4a5133fb", "Blood sample"},
            { "5ac620eb86f7743a8e6e0da0", "Package"},
            { "5ae9a0dd86f7742e5f454a05", "Manifests"},
            { "5ae9a18586f7746e381e16a3", "Manifests"},
            { "5ae9a1b886f77404c8537c62", "Manifests"},
            { "5ae9a25386f7746dd946e6d9", "Cargo route"},
            { "5ae9a3f586f7740aab00e4e6", "Book p.1"},
            { "5ae9a4fc86f7746e381e1753", "Book p.2"},
            { "5b43237186f7742f3a4ab252", "Сhemcont"},
            { "5b4c72b386f7745b453af9c0", "Controller"},
            { "5b4c72c686f77462ac37e907", "Controller"},
            { "5b4c72fb86f7745cef1cffc5", "Gyroscope"},
            { "5b4c81a086f77417d26be63f", "Сhemcont"},
            { "5b4c81bd86f77418a75ae159", "Сhemcont"},
            { "5c12301c86f77419522ba7e4", "False FD"},
            { "5d357d6b86f7745b606e3508", "photo album"},
            { "5d3ec50586f774183a607442", "Encr. pack"},
        };
        public static Dictionary<string, string> ModsIdsToNamesDic = new Dictionary<string, string>
        {
            { "5a1eaa87fcdbcb001865f75e", "REAP-IR"},
            { "5c110624d174af029e69734c", "T-7"},
            { "5c0558060db834001b735271", "GPNVG-18"},
            { "5d1b5e94d7ad1a2b865a96b0", "RS-32"},
        };
        public static Dictionary<string, string> BarterIdsToNamesDic = new Dictionary<string, string>
        {
            { "5672cb124bdc2d1a0f8b4568", "AA Bat."},
            { "5672cb304bdc2dc2088b456a", "D Bat."},
            { "5672cb724bdc2dc2088b456b", "GMcount"},
            { "56742c284bdc2d98058b456d", "Crickent"},
            { "56742c2e4bdc2d95058b456d", "Zibbo"},
            { "56742c324bdc2d150f8b456d", "GPhone"},
            { "5733279d245977289b77ec24", "Battery"},
            { "573474f924597738002c6174", "Chainlet"},
            { "5734758f24597738025ee253", "GoldChain"},
            { "573475fb24597737fb1379e1", "Cigarettes"},
            { "573476d324597737da2adc13", "Cigarettes"},
            { "573476f124597737e04bf328", "Cigarettes"},
            { "5734770f24597738025ee254", "Cigarettes"},
            { "5734779624597737e04bf329", "CPU Fan"},
            { "573477e124597737dd42e191", "CPU"},
            { "5734781f24597737e04bf32a", "DVD"},
            { "573478bc24597738002c6175", "Horse"},
            { "5734795124597738002c6176", "Tape"},
            { "57347b8b24597737dd42e192", "Matches"},
            { "57347baf24597738002c6178", "RAM"},
            { "57347c1124597737fb1379e3", "Duct tape"},
            { "57347c2e24597744902c94a1", "PSU"},
            { "57347c5b245977448d35f6e1", "Bolts"},
            { "57347c77245977448d35f6e2", "Screw nut"},
            { "57347c93245977448d35f6e3", "Toothpaste"},
            { "57347ca924597744596b4e71", "Graphics card"},
            { "57347cd0245977445a2d6ff1", "T-Plug"},
            { "577e1c9d2459773cd707c525", "Paper"},
            { "5909e99886f7740c983b9984", "USB-A"},
            { "590a358486f77429692b2790", "RecBatt"},
            { "590a373286f774287540368b", "Dfuel"},
            { "590a386e86f77429692b27ab", "HDD"},
            { "590a391c86f774385a33c404", "Magnet"},
            { "590a3b0486f7743954552bdb", "Сircuit board"},
            { "590a3c0a86f774385a33c450", "Plug"},
            { "590a3cd386f77436f20848cb", "ES Lamp"},
            { "590a3d9c86f774385926e510", "UV Lamp"},
            { "590a3efd86f77437d351a25b", "GasAn"},
            { "590c2b4386f77425357b6123", "Pliers"},
            { "590c2c9c86f774245b1f03f2", "MTape"},
            { "590c2d8786f774245b1f03f3", "Screwdriver"},
            { "590c2e1186f77425357b6124", "Set"},
            { "590c311186f77424d1667482", "Wrench"},
            { "590c31c586f774245e3141b2", "Nails"},
            { "590c346786f77423e50ed342", "Xeno"},
            { "590c35a486f774273531c822", "Shus"},
            { "590c595c86f7747884343ad7", "Filter"},
            { "590c5a7286f7747884343aea", "Gpowder"},
            { "590c5bbd86f774785762df04", "WD-40"},
            { "590c5c9f86f77477c91c36e7", "WD-40"},
            { "590de71386f774347051a052", "Teapot"},
            { "590de7e986f7741b096e5f32", "Vase"},
            { "59e3556c86f7741776641ac2", "Bleach"},
            { "59e358a886f7741776641ac3", "Wiper"},
            { "59e3596386f774176c10a2a2", "Paid"},
            { "59e35abd86f7741778269d82", "Sodium"},
            { "59e35cbb86f7741778269d83", "Hose"},
            { "59e35de086f7741778269d84", "Drill"},
            { "59e35ef086f7741777737012", "Screws"},
            { "59e3606886f77417674759a5", "NaCl"},
            { "59e361e886f774176c10a2a5", "H2O2"},
            { "59e3639286f7741777737013", "Lion"},
            { "59e3647686f774176a362507", "Clock"},
            { "59e3658a86f7741776641ac4", "Cat"},
            { "59e366c186f7741778269d85", "Plex"},
            { "59e36c6f86f774176c10a2a7", "Cord"},
            { "59f32bb586f774757e1e8442", "Dogtag"},
            { "59f32c3b86f77472a31742f0", "Dogtag"},
            { "59faf7ca86f7740dbe19f6c2", "Roler"},
            { "59faf98186f774067b6be103", "Alkali"},
            { "59fafb5d86f774067a6f2084", "Propane"},
            { "59faff1d86f7746c51718c9c", "0.2BTC"},
            { "5af0484c86f7740f02001f7f", "Coffee"},
            { "5af04b6486f774195a3ebb49", "Elite"},
            { "5af04c0b86f774138708f78e", "Controller"},
            { "5af04e0a86f7743a532b79e2", "Gyroscope"},
            { "5af0534a86f7743b6f354284", "Ophthalmoscope"},
            { "5af0561e86f7745f5f3ad6ac", "Powerbank"},
            { "5b4335ba86f7744d2837a264", "Bloodset"},
            { "5b43575a86f77424f443fe62", "Fcond"},
            { "5bc9b355d4351e6d1509862a", "#FireKlean"},
            { "5bc9b720d4351e450201234b", "1GPhone"},
            { "5bc9b9ecd4351e3bac122519", "Beardoil"},
            { "5bc9bc53d4351e00367fbcee", "Rooster"},
            { "5bc9bdb8d4351e003562b8a1", "Badge"},
            { "5bc9be8fd4351e00334cae6e", "Tea"},
            { "5bc9c049d4351e44f824d360", "Book"},
            { "5bc9c377d4351e3bac12251b", "Firesteel"},
            { "5c052e6986f7746b207bc3c9", "Defibrillator"},
            { "5c052f6886f7746b1e3db148", "SG-C10"},
            { "5c052fb986f7746b2101e909", "RFIDR"},
            { "5c05300686f7746dce784e5d", "VPX"},
            { "5c05308086f7746b2101e90b", "Virtex"},
            { "5c0530ee86f774697952d952", "LEDX"},
            { "5c06779c86f77426e00dd782", "Wires"},
            { "5c06782b86f77426df5407d2", "Cap."},
            { "5c12620d86f7743f8b198b72", "Tetriz"},
            { "5c1265fc86f7743f896a21c2", "GPX"},
            { "5c1267ee86f77416ec610f72", "Prokill"},
            { "5c12688486f77426843c7d32", "Paracord"},
            { "5c13cd2486f774072c757944", "Soap"},
            { "5c13cef886f774072e618e82", "TP"},
            { "5d0375ff86f774186372f685", "M.Cable"},
            { "5d0376a486f7747d8050965c", "MCB"},
            { "5d03775b86f774203e7e0c4b", "AESA"},
            { "5d0377ce86f774186372f689", "Iridium"},
            { "5d03784a86f774203e7e0c4d", "MGT"},
            { "5d0378d486f77420421a5ff4", "Filter"},
            { "5d03794386f77420415576f5", "Tank battery"},
            { "5d0379a886f77420407aa271", "OFZ "},
            { "5d1b2f3f86f774252167a52c", "FP-100"},
            { "5d1b2fa286f77425227d1674", "Motor"},
            { "5d1b2ffd86f77425243e8d17", "NIXXOR"},
            { "5d1b304286f774253763a528", "LCD"},
            { "5d1b309586f77425227d1676", "BrokenLCD"},
            { "5d1b313086f77425227d1678", "Relay"},
            { "5d1b317c86f7742523398392", "Hand drill"},
            { "5d1b31ce86f7742523398394", "R-pliers"},
            { "5d1b327086f7742525194449", "Gauge"},
            { "5d1b32c186f774252167a530", "Therm."},
            { "5d1b36a186f7742523398433", "Fuel"},
            { "5d1b371186f774253763a656", "Fuel"},
            { "5d1b385e86f774252167b98a", "Filter"},
            { "5d1b392c86f77425243e98fe", "Bulb"},
            { "5d1b39a386f774252339976f", "Tube"},
            { "5d1b3a5d86f774252167ba22", "Meds"},
            { "5d1b3f2d86f774253763b735", "Syringe"},
            { "5d1c774f86f7746d6620f8db", "Helix"},
            { "5d1c819a86f774771b0acd6c", "W.parts"},
            { "5d235a5986f77443f6329bc6", "Skull"},
            { "5d235b4d86f7742e017bc88a", "GP"},
            { "5d40412b86f7743cb332ac3a", "Shampoo"},
            { "5d40419286f774318526545f", "M. Scissors"},
            { "5d4041f086f7743cac3f22a7", "Ortodontox"},
            { "5d40425986f7743185265461", "Nippers"},
            { "5d4042a986f7743185265463", "L&F Scr."},
            { "5d63d33b86f7746ea9275524", "F Scr."},
            { "5d6fc78386f77449d825f9dc", "Gpowder"},
            { "5d6fc87386f77449db3db94e", "Gpowder"},
            { "5df8a6a186f77412640e2e80", "R.Ball"},
            { "5df8a72c86f77412640e2e83", "S.Ball"},
            { "5df8a77486f77412672a1e3f", "V.Ball"},
            { "5e2aedd986f7746d404f3aa4", "GreenBat "},
            { "5e2aee0a86f774755a234b62", "Cyclon "},
            { "5e2aef7986f7746d3f3c33f5", "Repellent"},
            { "5e2af00086f7746d3f3c33f7", "Cleaner"},
            { "5e2af02c86f7746d420957d4", "Chlorine"},
            { "5e2af22086f7746d3f3c33fa", "Poxeram"},
            { "5e2af29386f7746d4159f077", "KEK"},
            { "5e2af2bc86f7746d3f3c33fc", "Matches"},
            { "5e2af37686f774755a234b65", "SurvL "},
            { "5e2af41e86f774755a234b67", "Cordura"},
            { "5e2af47786f7746d404f3aaa", "Fleece "},
            { "5e2af4a786f7746d3f3c3400", "Ripstop"},
            { "5e2af4d286f7746d4159f07a", "Aramid"},
            { "5e2af51086f7746d3f3c3402", "Fuze"},
            { "5e54f62086f774219b0f1937", "Raven"},
            { "5e54f6af86f7742199090bf3", "Lupo's"},
        };
        public static Dictionary<string, string> GearsIdsToNamesDic = new Dictionary<string, string>
        {
            { "544a11ac4bdc2d470e8b456a", "Alpha Container"},
            { "544a5caa4bdc2d1a388b4568", "AVS"},
            { "544a5cde4bdc2d39388b456b", "MBSS"},
            { "545cdae64bdc2d39198b4568", "Tri-Zip"},
            { "545cdb794bdc2d3a198b456a", "6B43 6A"},
            { "557ff21e4bdc2d89578b4586", "TGlass"},
            { "5645bc214bdc2d363b8b4571", "Kiver-M"},
            { "5645bcc04bdc2d363b8b4572", "ComTac2"},
            { "5648a69d4bdc2ded0b8b457b", "BlackRock"},
            { "5648a7494bdc2d9d488b4583", "PACA"},
            { "567143bf4bdc2d1a0f8b4567", "Сase"},
            { "56e294cdd2720b603a8b4575", "Terraplane 85L"},
            { "56e335e4d2720b6c058b456d", "ScavBP"},
            { "56e33634d2720bd8058b456b", "Duffle"},
            { "56e33680d2720be2748b4576", "T-Bag"},
            { "572b7adb24597762ae139821", "Scav Vest"},
            { "572b7d8524597762b472f9d1", "Cap"},
            { "572b7f1624597762ae139822", "Balaclava"},
            { "572b7fa124597762b472f9d2", "Beanie"},
            { "572b7fa524597762b747ce82", "Mask"},
            { "5783c43d2459774bbe137486", "Wallet"},
            { "5857a8b324597729ab0a0e7d", "Beta Container"},
            { "5857a8bc2459772bad15db29", "Gamma container"},
            { "58ac60eb86f77401897560ff", "Balaclava_dev"},
            { "590c60fc86f77412b13fddcf", "Docs"},
            { "5929a2a086f7744f4b234d43", "6sh112"},
            { "592c2d1a86f7746dbe2af32a", "Alpha"},
            { "59db794186f77448bc595262", "Epsilon"},
            { "59e7635f86f7742cbf2c1095", "3M"},
            { "59e763f286f7742ee57895da", "Pilgrim"},
            { "59e7643b86f7742cbf2c109a", "WTRig"},
            { "59e7708286f7742cbd762753", "Ushanka"},
            { "59e770b986f7742cbd762754", "AFGlass"},
            { "59e770f986f7742cbe3164ef", "Cap"},
            { "59e7711e86f7746cae05fbe1", "Kolpak"},
            { "59e7715586f7742ee5789605", "Resp"},
            { "59e8936686f77467ce798647", "Balaclava_test"},
            { "59ef13ca86f77445fd0e2483", "Pumpkin"},
            { "59fafd4b86f7745ca07e1232", "Keytool"},
            { "59fb016586f7746d0d4b423a", "MCase"},
            { "59fb023c86f7746d0d4b423c", "WCase"},
            { "59fb042886f7746c5005a7b2", "ICase"},
            { "5a154d5cfcdbcb001a3b00da", "Fast MT"},
            { "5a16b672fcdbcb001912fa83", "FVisor"},
            { "5a16b7e1fcdbcb00165aa6c9", "FShield"},
            { "5a16b9fffcdbcb0176308b34", "RAC"},
            { "5a16ba61fcdbcb098008728a", "Mandible"},
            { "5a16badafcdbcb001865f72d", "SArmor"},
            { "5a16bb52fcdbcb001a3b00dc", "SLock"},
            { "5a43943586f77416ad2f06e2", "Hat"},
            { "5a43957686f7742a2c2f11b0", "Hat"},
            { "5a7c4850e899ef00150be885", "6B47"},
            { "5aa2a7e8e5b5b00016327c16", "USEC"},
            { "5aa2b87de5b5b00016327c25", "BEAR"},
            { "5aa2b89be5b5b0001569311f", "Emercom"},
            { "5aa2b8d7e5b5b00014028f4a", "Police"},
            { "5aa2b923e5b5b000137b7589", "RGlass"},
            { "5aa2b986e5b5b00014028f4c", "Dundukk"},
            { "5aa2b9aee5b5b00015693121", "RayBench"},
            { "5aa2b9ede5b5b000137b758b", "CHat"},
            { "5aa2ba19e5b5b00014028f4e", "Fleece"},
            { "5aa2ba46e5b5b000137b758d", "UXPRO"},
            { "5aa2ba71e5b5b000137b758f", "Sordin"},
            { "5aa7cfc0e5b5b00015693143", "6B47"},
            { "5aa7d03ae5b5b00016327db5", "UNTAR"},
            { "5aa7d193e5b5b000171d063f", "SFERA"},
            { "5aa7e276e5b5b000171d0647", "Altyn"},
            { "5aa7e373e5b5b000137b76f0", "FShield"},
            { "5aa7e3abe5b5b000171d064d", "FShield"},
            { "5aa7e454e5b5b0214e506fa2", "ZSh-1-2M"},
            { "5aa7e4a4e5b5b000137b76f2", "ZSh-1-2M"},
            { "5aafbcd986f7745e590fff23", "MedCase"},
            { "5aafbde786f774389d0cbc0f", "AmmoCase"},
            { "5ab8dab586f77441cd04f2a2", "MK3"},
            { "5ab8dced86f774646209ec87", "M2"},
            { "5ab8e4ed86f7742d8e50c7fa", "MF-UN"},
            { "5ab8e79e86f7742d8b372e78", "GZHEL-K"},
            { "5ab8ebf186f7742d8b372e80", "Attack 2"},
            { "5ab8ee7786f7742d8f33f0b9", "ArmyBag"},
            { "5ab8f04f86f774585f4237d8", "Sling"},
            { "5ab8f20c86f7745cdb629fb2", "Shmaska"},
            { "5ab8f39486f7745cd93a1cca", "CF"},
            { "5ab8f4ff86f77431c60d91ba", "Ghost"},
            { "5ab8f85d86f7745cd93a1cf5", "Shemagh"},
            { "5ac4c50d5acfc40019262e87", "Visor"},
            { "5ac8d6885acfc400180ae7b0", "Fast MT Tan"},
            { "5b3f16c486f7747c327f55f7", "Armband "},
            { "5b3f3ade86f7746b6b790d8e", "Armband "},
            { "5b3f3af486f774679e752c1f", "Armband"},
            { "5b3f3b0186f774021a2afef7", "Armband"},
            { "5b3f3b0e86f7746752107cda", "Armband "},
            { "5b40e1525acfc4771e1c6611", "ULACH"},
            { "5b40e2bc5acfc40016388216", "ULACH"},
            { "5b40e3f35acfc40016388218", "ACHHC"},
            { "5b40e4035acfc47a87740943", "ACHHC"},
            { "5b40e5e25acfc4001a599bea", "BEAR"},
            { "5b40e61f5acfc4001a599bec", "USEC"},
            { "5b4325355acfc40019478126", "Shemagh"},
            { "5b4326435acfc433000ed01d", "Mask"},
            { "5b43271c5acfc432ff4dce65", "Bandana"},
            { "5b4327aa5acfc400175496e0", "Panama"},
            { "5b4329075acfc400153b78ff", "Pompon"},
            { "5b4329f05acfc47a86086aa1", "Ronin"},
            { "5b432b2f5acfc4771e1c6622", "Shattered"},
            { "5b432b6c5acfc4001a599bf0", "Skull"},
            { "5b432b965acfc47a8774094e", "GSSh-01"},
            { "5b432be65acfc433000ed01f", "6B34"},
            { "5b432c305acfc40019478128", "GP-5"},
            { "5b432d215acfc4771e1c6624", "LZSh"},
            { "5b432f3d5acfc4704b4a1dfb", "Momex"},
            { "5b44c6ae86f7742d1627baea", "Beta2"},
            { "5b44c8ea86f7742d1627baf1", "Commando "},
            { "5b44cad286f77402a54ae7e5", "Tactec "},
            { "5b44cd8b86f774503d30cba2", "Gen4 Full"},
            { "5b44cf1486f77431723e3d05", "Gen4 Assault"},
            { "5b44d0de86f774503d30cba8", "Gen4 HMK"},
            { "5b44d22286f774172b0c9de8", "Kirasa"},
            { "5b46238386f7741a693bcf9c", "Kiver FS"},
            { "5b6d9ce188a4501afc1b2b25", "T H I C C"},
            { "5b7c710788a4506dec015957", "Lucky Scav junkbox"},
            { "5bd06f5d86f77427101ad47c", "Mask"},
            { "5bd0716d86f774171822ef4b", "Mask"},
            { "5bd071d786f7747e707b93a3", "Mask"},
            { "5bd073a586f7747e6f135799", "Mustache"},
            { "5bd073c986f7747f627e796c", "Kotton"},
            { "5c066ef40db834001966a595", "NVG mask"},
            { "5c06c6a80db834001b735491", "SSh-68"},
            { "5c08f87c0db8340019124324", "SHPM"},
            { "5c0919b50db834001b7ce3b9", "1Sch FShield"},
            { "5c091a4e0db834001d5addc8", "Maska 1Sch"},
            { "5c093ca986f7740a1867ab12", "Kappa"},
            { "5c093db286f7740a1b2617e3", "Holodilnick"},
            { "5c093e3486f77430cb02e593", "Dogtags"},
            { "5c0a840b86f7742ffa4f2482", "T H I C C"},
            { "5c0d2727d174af02a012cf58", "Djeta"},
            { "5c0d32fcd174af02a1659c75", "Proximity"},
            { "5c0e3eb886f7742015526062", "6B5-16"},
            { "5c0e446786f7742013381639", "6B5-15"},
            { "5c0e51be86f774598e797894", "6B13"},
            { "5c0e53c886f7747fa54205c7", "6B13"},
            { "5c0e541586f7747fa54205c9", "6B13M"},
            { "5c0e57ba86f7747fa141986d", "6B23-2"},
            { "5c0e5bab86f77461f55ed1f3", "6B23-1"},
            { "5c0e5edb86f77461f55ed1f7", "Zhuk-3"},
            { "5c0e625a86f7742d77340f62", "Zhuk-6a"},
            { "5c0e655586f774045612eeb2", "Trooper"},
            { "5c0e66e2d174af02a96252f4", "SLAAP"},
            { "5c0e6a1586f77404597b4965", "Belt"},
            { "5c0e722886f7740458316a57", "M1"},
            { "5c0e746986f7741453628fe5", "TV-110"},
            { "5c0e774286f77468413cc5b2", "Blackjack 50"},
            { "5c0e805e86f774683f3dd637", "Paratus"},
            { "5c0e842486f77443a74d2976", "1Sch FShield"},
            { "5c0e874186f7745dc7616606", "Maska 1Sch"},
            { "5c0e9f2c86f77432297fe0a3", "Commando "},
            { "5c127c4486f7745625356c13", "Magbox"},
            { "5c165d832e2216398b5a7e36", "Tactical Sport"},
            { "5c178a942e22164bef5ceca3", "Chops"},
            { "5c1793902e221602b21d3de2", "CP Ears"},
            { "5c17a7ed2e2216152142459c", "Airframe Tan"},
            { "5c1a1cc52e221602b3136e3d", "M Frame"},
            { "5c1a1e3f2e221602b66cc4c2", "Beard"},
            { "5ca20abf86f77418567a43f2", "Triton"},
            { "5ca20d5986f774331e7c9602", "Berkut"},
            { "5ca20ee186f774799474abc2", "Vulkan-5"},
            { "5ca2113f86f7740b2547e1d2", "FShield"},
            { "5ca2151486f774244a3b8d30", "Redut-M"},
            { "5ca21c6986f77479963115a7", "Redut-T5"},
            { "5d235bb686f77443f4331278", "SICC"},
            { "5d5d646386f7742797261fd9", "6B3TM-01M"},
            { "5d5d85c586f774279a21cbdb", "D3CRX"},
            { "5d5d87f786f77427997cfaef", "A18"},
            { "5d5d8ca986f7742798716522", "MRig"},
            { "5d5d940f86f7742797262046", "Mechanism"},
            { "5d5e7d28a4b936645d161203", "TC-2001"},
            { "5d5e9c74a4b9364855191c40", "TC-2002"},
            { "5d5fca1ea4b93635fd598c07", "Crossbow"},
            { "5d6d2e22a4b9361bd5780d05", "Gascan"},
            { "5d6d2ef3a4b93618084f58bd", "Aviator"},
            { "5d6d3716a4b9361bc8618872", "LSHZ-2DTM"},
            { "5d6d3829a4b9361bc8618943", "FShield"},
            { "5d6d3943a4b9360dbc46d0cc", "Cover"},
            { "5d6d3be5a4b9361bc73bc763", "Aventail"},
            { "5d96141523f0ea1b7f2aacab", "'Door Kicker'"},
            { "5df8a2ca86f7740bfe6df777", "6B2"},
            { "5df8a42886f77412640e2e75", "MPPV"},
            { "5df8a4d786f77412672a1e3b", "6SH118"},
            { "5df8a58286f77412631087ed", "Tank helmet"},
            { "5e00c1ad86f774747333222c", "EXFIL"},
            { "5e00cdd986f7747473332240", "FShield"},
            { "5e00cfa786f77469dc6e5685", "TW Ears"},
            { "5e01ef6886f77445f643baa4", "EXFIL"},
            { "5e01f31d86f77465cf261343", "TW Ears"},
            { "5e01f37686f774773c6f6c15", "FShield"},
            { "5e2af55f86f7746d4159f07c", "Grenades"},
            { "5e4abb5086f77406975c9342", "Slick"},
            { "5e4abc1f86f774069619fbaa", "Bank Robber"},
            { "5e4abc6786f77406812bd572", "SFMP"},
            { "5e4abfed86f77406a2713cf7", "Tarzan"},
            { "5e4ac41886f77406a511c9a8", "AACPC"},
            { "5e4bfc1586f774264f7582d3", "TC 800"},
            { "5e4d34ca86f774264f758330", "Razor"},
            { "5e54f76986f7740366043752", "Shroud"},
            { "5e54f79686f7744022011103", "Plague mask"},
            { "5e71f6be86f77429f2683c44", "Rivals"},
            { "5e71f70186f77429ee09f183", "Rivals"},
            { "5e71fad086f77422443d4604", "Rivals"},
            { "5e9dacf986f774054d6b89f4", "Defender-2"},
            { "5e9db13186f7742f845ee9d3", "LBT-1961A"},
            { "5e9dcf5986f7746c417435b3", "Day Pack"},
            { "5ea058e01dbce517f324b3e2", "Heavy Trooper"},
            { "5ea05cf85ad9772e6624305d", "TK Fast MT"},
            { "5ea17ca01412a1425304d1c0", "Bastion"},
            { "5ea18c84ecf1982c7712d9a2", "Bastion"},
        };

        /*public static Dictionary<string, string> ItemsIdsToNamesDic = new Dictionary<string, string>
        {
            { "5c94bbff86f7747ee735c08f", "Keycard"},
            { "5c1e495a86f7743109743dfb", "Violet"},
            { "5c1d0f4986f7744bb01837fa", "Black"},
            { "5c1d0efb86f7744baf2e7b7b", "Red"},
            { "5c1d0dc586f7744baf2e7b79", "Green"},
            { "5c12613b86f7743bbe2c3f76", "Intelligence"},
            { "590c621186f774138d11ea29", "Flash drive"},
            { "590c392f86f77444754deb29", "SSD"},
            { "5e4abb5086f77406975c9342", "Slick"},
            { "5c0530ee86f774697952d952", "LEDX"},
            { "5c05308086f7746b2101e90b", "Virtex"},
            { "5c05300686f7746dce784e5d", "VPX"},
            { "5c052fb986f7746b2101e909", "RFIDR"},
            { "5c052e6986f7746b207bc3c9", "Defibrillator"},
            { "5bc9c377d4351e3bac12251b", "Firesteel"},
            //{ "5bc9b720d4351e450201234b", "1GPhone"},
            { "5b43575a86f77424f443fe62", "Fcond"},
            { "5af0534a86f7743b6f354284", "Ophthalmoscope"},
            { "5734758f24597738025ee253", "GoldChain"},
            { "5e54f6af86f7742199090bf3", "Lupo's"},
            { "5d235a5986f77443f6329bc6", "Skull"},
            { "5d1b327086f7742525194449", "Gauge"},
            { "5d1b32c186f774252167a530", "Therm."},
            { "5d1b2ffd86f77425243e8d17", "NIXXOR"},
            { "57347ca924597744596b4e71", "Graphics card"},
            { "5d08d21286f774736e7c94c3", "KSH"},
            { "5c1d0c5f86f7744bb2683cf0", "Blue"},
            { "5c110624d174af029e69734c", "T-7"},
            { "5a1eaa87fcdbcb001865f75e", "REAP-IR"},
            { "5d0379a886f77420407aa271", "OFZ "},
            { "59faf7ca86f7740dbe19f6c2", "Roler"},
            { "5c12620d86f7743f8b198b72", "Tetriz"},
            { "5c052f6886f7746b1e3db148", "SG-C10"},
            { "59faff1d86f7746c51718c9c", "0.2BTC"},
            { "5d947d4e86f774447b415895", "RB-KSM"},
            { "5d80cbd886f77470855c26c2", "RB-MP13"},
            { "59ef13ca86f77445fd0e2483", "Pumpkin"},
            { "59fafd4b86f7745ca07e1232", "Keytool"},
            { "59fb016586f7746d0d4b423a", "MCase"},
            { "59fb023c86f7746d0d4b423c", "WCase"},
            { "59fb042886f7746c5005a7b2", "ICase"},
            { "5aafbcd986f7745e590fff23", "MedCase"},
            { "5aafbde786f774389d0cbc0f", "AmmoCase"},
            { "5c127c4486f7745625356c13", "Magbox"},
            { "5d235bb686f77443f4331278", "SICC"},
            { "59e3577886f774176a362503", "Sugar"},
            { "5c093e3486f77430cb02e593", "Dogtags"},
            { "590c60fc86f77412b13fddcf", "Docs"},
            { "5c1267ee86f77416ec610f72", "Prokill"},
            { "5d0378d486f77420421a5ff4", "Filter"},
            { "5d1b385e86f774252167b98a", "Filter"},
        };
        */
        public static Menu RootMenu { get; private set; }
        public static Menu VisualsMenu { get; private set; }
        public static Menu ItemsMenu { get; private set; }
        //public static Menu AimbotMenu { get; private set; }

        class Components
        {
            public static readonly MenuKeyBind MainAssemblyToggle = new MenuKeyBind("mainassemblytoggle", "Toggle the whole assembly effect by pressing key:", VirtualKeyCode.Delete, KeybindType.Toggle, true);
            public static class VisualsComponent
            {
                public static readonly MenuBool DrawTheVisuals = new MenuBool("drawthevisuals", "Enable all of the Visuals", true);
                public static readonly MenuColor AlliesColor = new MenuColor("alliescolor", "Allies ESP Color", new SharpDX.Color(0, 255, 0));
                public static readonly MenuBool DrawAlliesEsp = new MenuBool("drawbox", "Draw Allies ESP", true);
                public static readonly MenuColor EnemiesColor = new MenuColor("enemiescolor", "Enemies ESP Color", new SharpDX.Color(255, 0, 0));
                public static readonly MenuColor BotsColor = new MenuColor("botscolor", "Bots ESP Color", new SharpDX.Color(0, 0, 255));
                public static readonly MenuBool DrawBox = new MenuBool("drawbox", "Draw Box ESP", true);
                public static readonly MenuSlider DrawBoxThic = new MenuSlider("boxthickness", "Draw Box Thickness", 0, 0, 10);
                public static readonly MenuBool DrawBoxBorder = new MenuBool("drawboxborder", "Draw Border around Box and Text?", true);
                public static readonly MenuBool DrawBoxHP = new MenuBool("drawboxhp", "Draw Health", true);
                //public static readonly MenuBool DrawBoxAR = new MenuBool("drawboxar", "Draw Armor", true);
                public static readonly MenuSliderBool DrawTextSize = new MenuSliderBool("drawtextsize", "Text Size", false, 14, 4, 72);
                public static readonly MenuBool DrawTextDist = new MenuBool("drawtextdist", "Draw Distance", true);
                public static readonly MenuBool DrawTextName = new MenuBool("drawtextname", "Draw Player Name", true);
                public static readonly MenuBool DrawTotalPlayersCount = new MenuBool("drawtotalplayerscount", "Draw Total Players Count (Scavs Included)", true);
            }

            public static class ItemsVisualsComponent
            {
                public static readonly MenuBool DrawItems = new MenuBool("drawitems", "Enable Drawing items", true);
            }
            //public static class AimbotComponent
            //{
            //    public static readonly MenuBool AimGlobalBool = new MenuBool("enableaim", "Enable Aimbot Features", true);
            //    public static readonly MenuKeyBind AimKey = new MenuKeyBind("aimkey", "Aimbot HotKey (HOLD)", VirtualKeyCode.LeftMouse, KeybindType.Hold, false);
            //    public static readonly MenuList AimType = new MenuList("aimtype", "Aimbot Type", new List<string>() { "Direct Engine ViewAngles", "Real Mouse Movement" }, 0);
            //    public static readonly MenuList AimSpot = new MenuList("aimspot", "Aimbot Spot", new List<string>() { "Aim at their Head", "Aim at their Body" }, 0);
            //    public static readonly MenuBool AimAtEveryone = new MenuBool("aimeveryone", "Aim At Everyone (even teammates)", true);
            //    public static readonly MenuSlider AimSpeed = new MenuSlider("aimspeed", "Aimbot Speed %", 100, 1, 100);
            //    public static readonly MenuBool DrawAimSpot = new MenuBool("drawaimspot", "Draw Aimbot Spot", true);
            //    public static readonly MenuBool DrawAimTarget = new MenuBool("drawaimtarget", "Draw Aimbot Current Target", true);
            //    public static readonly MenuColor AimTargetColor = new MenuColor("aimtargetcolor", "Target Color", new SharpDX.Color(0x1F, 0xBE, 0xD6, 255));
            //    public static readonly MenuBool DrawAimFov = new MenuBool("drawaimfov", "Draw Aimbot FOV Circle", true);
            //    public static readonly MenuColor AimFovColor = new MenuColor("aimfovcolor", "FOV Color", new SharpDX.Color(255, 255, 255, 30));
            //    public static readonly MenuSlider AimFov = new MenuSlider("aimfov", "Aimbot FOV", 150, 4, 1000);
            //}
        }

        public static void InitializeMenu()
        {
            VisualsMenu = new Menu("visualsmenu", "Visuals Menu")
            {
                Components.VisualsComponent.DrawTheVisuals,
                Components.VisualsComponent.AlliesColor,
                Components.VisualsComponent.DrawAlliesEsp.SetToolTip("Don't forget that disabling this feature in a regular dm server will still erase half of the players!"),
                Components.VisualsComponent.EnemiesColor,
                Components.VisualsComponent.BotsColor,
                Components.VisualsComponent.DrawBox,
                Components.VisualsComponent.DrawBoxThic.SetToolTip("Setting thickness to 0 will let the assembly auto-adjust itself depending on model distance"),
                Components.VisualsComponent.DrawBoxBorder.SetToolTip("Drawing borders may take extra performance (FPS) on low-end computers"),
                Components.VisualsComponent.DrawBoxHP,
                //Components.VisualsComponent.DrawBoxAR,                Components.VisualsComponent.DrawTextSize,
                Components.VisualsComponent.DrawTextDist,
                Components.VisualsComponent.DrawTextName,
                Components.VisualsComponent.DrawTotalPlayersCount,
            };

            ItemsMenu = new Menu("itemsMenu", "Items Menu")
            {
                Components.ItemsVisualsComponent.DrawItems
            };

            ItemsMenu.Add(new MenuSeperator("itemmenubarterseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenubarterseparator", "Barter Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenubarterseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenubarterenable", "Draw Barter Items", true));
            ItemsMenu.Add(new MenuSeperator("itemmenubarterseparator3", ""));
            foreach (var anItem in BarterIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, true));

            ItemsMenu.Add(new MenuSeperator("itemmenuinfoseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenuinfoseparator", "Info Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenuinfoseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenuinfoenable", "Draw Info Items", true));
            ItemsMenu.Add(new MenuSeperator("itemmenuinfoseparator3", ""));
            foreach (var anItem in InfoIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, true));

            ItemsMenu.Add(new MenuSeperator("itemmenumedicalseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenumedicalseparator", "Medical Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenumedicalseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenumedicalenable", "Draw Medical Items", false));
            ItemsMenu.Add(new MenuSeperator("itemmenumedicalseparator3", ""));
            foreach (var anItem in MedicalIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, false));

            ItemsMenu.Add(new MenuSeperator("itemmenukeysseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenukeysseparator", "Keys Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenukeysseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenukeysenable", "Draw Keys Items", true));
            ItemsMenu.Add(new MenuSeperator("itemmenukeysseparator3", ""));
            foreach (var anItem in KeysIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, true));

            ItemsMenu.Add(new MenuSeperator("itemmenumodsseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenumodsseparator", "Mods Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenumodsseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenumodsenable", "Draw Mods Items", true));
            ItemsMenu.Add(new MenuSeperator("itemmenumodsseparator3", ""));
            foreach (var anItem in ModsIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, true));

            ItemsMenu.Add(new MenuSeperator("itemmenugearseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenugearseparator", "Gear Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenugearseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenugearenable", "Draw Gear Items", true));
            ItemsMenu.Add(new MenuSeperator("itemmenugearseparator3", ""));
            foreach (var anItem in GearsIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, true));

            ItemsMenu.Add(new MenuSeperator("itemmenuprovisionsseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenuprovisionsseparator", "Provisions Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenuprovisionsseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenuprovisionsenable", "Draw Provisions Items", false));
            ItemsMenu.Add(new MenuSeperator("itemmenuprovisionsseparator3", ""));
            foreach (var anItem in ProvisionsIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, false));

            ItemsMenu.Add(new MenuSeperator("itemmenuquestseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenuquestseparator", "Quests Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenuquestseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenuquestenable", "Draw Quests Items", false));
            ItemsMenu.Add(new MenuSeperator("itemmenuquestseparator3", ""));
            foreach (var anItem in QuestIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, false));

            ItemsMenu.Add(new MenuSeperator("itemmenuammoseparator1", ""));
            ItemsMenu.Add(new MenuSeperator("itemmenuammoseparator", "Ammo Items"));
            ItemsMenu.Add(new MenuSeperator("itemmenuammoseparator2", ""));
            ItemsMenu.Add(new MenuBool("itemsmenuammoenable", "Draw Ammo Items", false));
            ItemsMenu.Add(new MenuSeperator("itemmenuammoseparator3", ""));
            foreach (var anItem in AmmoIdsToNamesDic)
                ItemsMenu.Add(new MenuBool(anItem.Key.ToString(), anItem.Value, false));

            //AimbotMenu = new Menu("aimbotmenu", "Aimbot Menu")
            //{
            //    Components.AimbotComponent.AimGlobalBool,
            //    Components.AimbotComponent.AimKey,
            //    Components.AimbotComponent.AimType,
            //    Components.AimbotComponent.AimSpot,
            //    Components.AimbotComponent.AimAtEveryone,
            //    Components.AimbotComponent.AimSpeed,
            //    Components.AimbotComponent.DrawAimSpot,
            //    Components.AimbotComponent.DrawAimTarget,
            //    Components.AimbotComponent.DrawAimFov,
            //    Components.AimbotComponent.AimFovColor,
            //    Components.AimbotComponent.AimFov,
            //};


            RootMenu = new Menu("escapefromtarkov", "WeScript.app EscapeFromTarkov Assembly", true)
            {
                Components.MainAssemblyToggle.SetToolTip("The magical boolean which completely disables/enables the assembly!"),
                VisualsMenu,
                ItemsMenu,
                //AimbotMenu,
            };
            RootMenu.Attach();
        }

        private static double GetDistance3D(Vector3 myPos, Vector3 enemyPos)
        {
            Vector3 vector = new Vector3(myPos.X - enemyPos.X, myPos.Y - enemyPos.Y, myPos.Z - enemyPos.Z);
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        //Refs:
        //https://github.com/matkuscz/EscapeFromTarkov-External-Cheat
        //https://github.com/Zeziroth/EscapeFromTarkov_External_ESP/tree/master/UnityExtract/UnityExtract

        static void Main(string[] args)
        {
            if (!VIP.IsSubscriber())
            {
                Console.WriteLine("You need to be Subscribed to WSA so you can use the hack.");
                return;
            }

            if (!VIP.IsTopicContentUnlocked("/219-escape-from-tarkov-eft-assembly-esp-and-aimbot-soon/"))
            {
                Console.WriteLine("You need to unlock the cheat by Paying with WSA Points in the topic..");
                return;
            }

            Console.WriteLine("WeScript.app Escape From Tarkov Assembly 1.0 Loaded! - GH");

            if (!Memory.InitDriver(DriverName.xHunter))
            {
                Console.WriteLine("[ERROR] Failed to initialize driver for some reason...");
            }
            InitializeMenu();
            Renderer.OnRenderer += OnRenderer;
            Memory.OnTick += OnTick;
        }


        private static void OnTick(int counter, EventArgs args)
        {
            if (processHandle == IntPtr.Zero) //if we still don't have a handle to the process
            {
                var wndHnd = Memory.FindWindowName("EscapeFromTarkov"); //try finding the window of the process (check if it's spawned and loaded)
                if (wndHnd != IntPtr.Zero) //if it exists
                {
                    var calcPid = Memory.GetPIDFromHWND(wndHnd); //get the PID of that same process
                    if (calcPid > 0) //if we got the PID
                    {
                        processHandle = Memory.ZwOpenProcess(PROCESS_ALL_ACCESS, calcPid); //get full access to the process so we can use it later
                        if (processHandle != IntPtr.Zero)
                        {
                            //if we got access to the game, check if it's x64 bit, this is needed when reading pointers, since their size is 4 for x86 and 8 for x64
                            isWow64Process = Memory.IsProcess64Bit(processHandle);
                            //here you can scan for signatures and stuff, it happens only once on "attach"
                        }
                    }
                }
            }
            else //else we have a handle, lets check if we should close it, or use it
            {
                var wndHnd = Memory.FindWindowName("EscapeFromTarkov");
                if (wndHnd != IntPtr.Zero) //window still exists, so handle should be valid? let's keep using it
                {
                    //the lines of code below execute every 33ms outside of the renderer thread, heavy code can be put here if it's not render dependant
                    gameProcessExists = true;
                    wndMargins = Renderer.GetWindowMargins(wndHnd);
                    wndSize = Renderer.GetWindowSize(wndHnd);
                    isGameOnTop = Renderer.IsGameOnTop(wndHnd);
                    GameCenterPos = new Vector2(wndSize.X / 2 + wndMargins.X, wndSize.Y / 2 + wndMargins.Y); //even if the game is windowed, calculate perfectly it's "center" for aim or crosshair
                    isOverlayOnTop = Overlay.IsOnTop();
                    ModuleBase = Memory.ZwGetModule(processHandle, "UnityPlayer.dll", isWow64Process); //EscapeFromTarkov.exe  UnityPlayer.dll
                    //GameWorld = Memory.ZwReadPointer(processHandle, (IntPtr)0x2581922DB58, isWow64Process);
                    if (gameObjectManager == IntPtr.Zero)
                    {
                        gameObjectManager =
                            Memory.ZwReadPointer(processHandle, (IntPtr)ModuleBase + 0x151A218, isWow64Process);
                        Console.WriteLine("Found GameObject Manager At " + gameObjectManager.ToString("X"));
                    }
                }
                else //else most likely the process is dead, clean up
                {
                    Memory.CloseHandle(processHandle); //close the handle to avoid leaks
                    processHandle = IntPtr.Zero; //set it like this just in case for C# logic
                    gameObjectManager = IntPtr.Zero; //set it like this just in case for C# logic
                    GameWorld = IntPtr.Zero; //set it like this just in case for C# logic
                    ModuleBase = IntPtr.Zero; //set it like this just in case for C# logic
                    gameProcessExists = false;
                }
            }
        }

        private static List<ulong> FindActiveObjects(string objectName, int limit = 1000000)
        {
            var actObj = Memory.ZwReadUInt64(processHandle, (IntPtr)gameObjectManager.ToInt64() + 0x18);
            var fActObj = actObj; // FirstActiveObject

            var ret = new List<ulong>();

            var i = 0;
            do
            {
                i++;
                if (i > limit)
                    break;

                var gObj = Memory.ZwReadUInt64(processHandle, (IntPtr)actObj + 0x10);
                var gObjNamePtr = Memory.ZwReadUInt64(processHandle, (IntPtr)gObj + 0x60);
                var gObjName = Memory.ZwReadString(processHandle, (IntPtr)gObjNamePtr, false, objectName.Length); // Read as UTF-8

                if (string.Equals(gObjName, objectName, StringComparison.CurrentCultureIgnoreCase))
                    ret.Add(gObj);

                actObj = Memory.ZwReadUInt64(processHandle, (IntPtr)actObj + 0x8); // Next Object
            } while (fActObj != actObj);

            return ret;
        }
        public static ulong ReadChain(ulong address, params uint[] ptrChainOffsets)
        {
            var curAddr = address;

            foreach (var offset in ptrChainOffsets)
            {
                curAddr = Memory.ZwReadUInt64(processHandle, (IntPtr)(curAddr + offset));

                if (curAddr == 0 || (IntPtr)curAddr == IntPtr.Zero)
                    return 0;
            }

            return curAddr;
        }
        public static IntPtr ReadChainPtr(long address, bool showEach, params uint[] ptrChainOffsets)
        {
            var curAddr = (IntPtr)address;

            foreach (var offset in ptrChainOffsets)
            {
                curAddr = Memory.ZwReadPointer(processHandle, (IntPtr)(curAddr.ToInt64() + offset), isWow64Process);
                if (showEach)
                    Console.WriteLine("Offset : 0x" + offset.ToString("X") + " Value: " + curAddr.ToInt64().ToString("X"));
                if (curAddr.ToInt64() == 0 || (IntPtr)curAddr == IntPtr.Zero)
                    return IntPtr.Zero;
            }

            return curAddr;
        }
        public static IntPtr GetPtr(IntPtr pointer, int[] offsets, bool debug = false)
        {
            try
            {
                IntPtr pointedto = pointer;
                foreach (int offset in offsets)
                {
                    IntPtr tmpPointed = (IntPtr)(Memory.ZwReadPointer(processHandle, pointedto, isWow64Process)); //Memory.Read<long>((long)pointedto)
                    pointedto = IntPtr.Add(tmpPointed, offset);
                }

                return pointedto;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetPTR : " + ex.Message + ex.StackTrace);
                return IntPtr.Zero;
            }
        }

        //[Obsolete]
        //private unsafe Vector3 GetBonePosition(IntPtr transform)
        //{
        //    IntPtr transform_internal = RPM.GetPtr(transform, new int[] { 0x10 });
        //    if (!RPM.IsValid(transform_internal.ToInt64()))
        //        return new Vector3();

        //    IntPtr pMatrix = RPM.GetPtr(transform_internal, new int[] { 0x38 });
        //    int index = RPM.Read<int>(RPM.GetPtr(transform_internal, new int[] { 0x38 + sizeof(UInt64) }).ToInt64());
        //    if (!RPM.IsValid(pMatrix.ToInt64()))
        //        return new Vector3();

        //    IntPtr matrix_list_base = RPM.GetPtr(pMatrix, new int[] { 0x8 });
        //    if (!RPM.IsValid(matrix_list_base.ToInt64()))
        //        return new Vector3();

        //    IntPtr dependency_index_table_base = RPM.GetPtr(pMatrix, new int[] { 0x10 });
        //    if (!RPM.IsValid(dependency_index_table_base.ToInt64()))
        //        return new Vector3();

        //    IntPtr pMatricesBufPtr = Marshal.AllocHGlobal(sizeof(Matrix34) * index + sizeof(Matrix34)); // sizeof(Matrix34) == 48
        //    void* pMatricesBuf = pMatricesBufPtr.ToPointer();
        //    RPM.ReadBytes(RPM.GetPtr(matrix_list_base, new int[] { 0 }).ToInt64(), pMatricesBuf, sizeof(Matrix34) * index + sizeof(Matrix34));

        //    IntPtr pIndicesBufPtr = Marshal.AllocHGlobal(sizeof(int) * index + sizeof(int));
        //    void* pIndicesBuf = pIndicesBufPtr.ToPointer();
        //    RPM.ReadBytes(RPM.GetPtr(dependency_index_table_base, new int[] { 0 }).ToInt64(), pIndicesBuf, sizeof(int) * index + sizeof(int));

        //    Vector4f result = *(Vector4f*)((UInt64)pMatricesBuf + 0x30 * (UInt64)index);
        //    int index_relation = *(int*)((UInt64)pIndicesBuf + 0x4 * (UInt64)index);

        //    Vector4f xmmword_1410D1340 = new Vector4f(-2.0f, 2.0f, -2.0f, 0.0f);
        //    Vector4f xmmword_1410D1350 = new Vector4f(2.0f, -2.0f, -2.0f, 0.0f);
        //    Vector4f xmmword_1410D1360 = new Vector4f(-2.0f, -2.0f, 2.0f, 0.0f);

        //    while (index_relation >= 0)
        //    {
        //        Matrix34 matrix34 = *(Matrix34*)((UInt64)pMatricesBuf + 0x30 * (UInt64)index_relation);

        //        Vector4f v10 = matrix34.vec2 * result;
        //        Vector4f v11 = (Vector4f)(VectorOperations.Shuffle(matrix34.vec1, (ShuffleSel)(0)));
        //        Vector4f v12 = (Vector4f)(VectorOperations.Shuffle(matrix34.vec1, (ShuffleSel)(85)));
        //        Vector4f v13 = (Vector4f)(VectorOperations.Shuffle(matrix34.vec1, (ShuffleSel)(-114)));
        //        Vector4f v14 = (Vector4f)(VectorOperations.Shuffle(matrix34.vec1, (ShuffleSel)(-37)));
        //        Vector4f v15 = (Vector4f)(VectorOperations.Shuffle(matrix34.vec1, (ShuffleSel)(-86)));
        //        Vector4f v16 = (Vector4f)(VectorOperations.Shuffle(matrix34.vec1, (ShuffleSel)(113)));
        //        result = (((((((v11 * xmmword_1410D1350) * v13) - ((v12 * xmmword_1410D1360) * v14)) * VectorOperations.Shuffle(v10, (ShuffleSel)(-86))) +
        //            ((((v15 * xmmword_1410D1360) * v14) - ((v11 * xmmword_1410D1340) * v16)) * VectorOperations.Shuffle(v10, (ShuffleSel)(85)))) +
        //            (((((v12 * xmmword_1410D1340) * v16) - ((v15 * xmmword_1410D1350) * v13)) * VectorOperations.Shuffle(v10, (ShuffleSel)(0))) + v10)) + matrix34.vec0);

        //        index_relation = *(int*)((UInt64)pIndicesBuf + 0x4 * (UInt64)index_relation);
        //    }

        //    Marshal.FreeHGlobal(pMatricesBufPtr);
        //    Marshal.FreeHGlobal(pIndicesBufPtr);

        //    return new Vector3(result.X, result.Y, result.Z);
        //}
        private static ulong FindCameraByName(string name)
        {
            // UnityPlayer.dll + 0x14BB400
            // Sig: 48 8B 15 ? ? ? ? 48 8B 4A 10   
            //var camManagerPtr = Memory.ZwReadPointer(processHandle, (IntPtr)ModuleBase.ToInt64() + 0x14BB400, isWow64Process);
            var camManagerAddr = Memory.ZwReadPointer(processHandle, (IntPtr)ModuleBase.ToInt64() + 0x14BB400, isWow64Process);
            var cameraArray = Memory.ZwReadPointer(processHandle, (IntPtr)camManagerAddr, isWow64Process);
            var cameraArrayCount = Memory.ZwReadUInt64(processHandle, (IntPtr)camManagerAddr + 0x8 * 2);

            for (uint i = 0; i < cameraArrayCount; i++)
            {
                var cameraEntity = Memory.ZwReadUInt64(processHandle, (IntPtr)(cameraArray.ToInt64() + (i * 8)));
                var isActive = Memory.ZwReadBool(processHandle, (IntPtr)cameraEntity + 0x39);

                if (!isActive)
                    continue;

                var camGameObj = Memory.ZwReadUInt64(processHandle, (IntPtr)cameraEntity + 0x30);
                var strPtr = Memory.ZwReadUInt64(processHandle, (IntPtr)camGameObj + 0x60);
                var camName = Memory.ZwReadString(processHandle, (IntPtr)strPtr, false, name.Length);

                if (camName == name)
                {
                    return camGameObj; // or return cameraEntity for the camera component
                }
            }

            return 0;
        }
        public static ulong gWorld = 0ul;
        public static bool IsInGame;
        public static IntPtr LocalPlayerPtr = IntPtr.Zero;

        private static void OnRenderer(int fps, EventArgs args)
        {
            if (!gameProcessExists) return; //process is dead, don't bother drawing
            if ((!isGameOnTop) && (!isOverlayOnTop)) return; //if game and overlay are not on top, don't draw
            if (!Components.MainAssemblyToggle.Enabled) return; //main menu boolean to toggle the cheat on or off
            if (gameObjectManager == IntPtr.Zero) return;

            ulong pArrayObj = 0ul;
            ulong itemsArrayObj = 0ul;
            int pArrayLen = 0;
            int itemsArrayLen = 0;
            if (!IsInGame || gWorld == 0ul)
            {
                var gWorldObjects = FindActiveObjects("GameWorld", 3000);

                if (gWorldObjects.Count == 0)
                {
                    //Console.WriteLine("Couldn't find any GWorld Objects!");
                    ItemsAddressHistory.Clear();
                    LootItems.Clear();
                    IsInGame = false;
                    return;
                }

                //Console.WriteLine("Found GWorlds : " + gWorldObjects.Count);
                foreach (var gWorldObject in gWorldObjects)
                {
                    gWorld = ReadChain(gWorldObject, 0x30, 0x18, 0x28);

                    // Player List
                    pArrayObj = Memory.ZwReadUInt64(processHandle, (IntPtr)(gWorld + 0x78));

                    itemsArrayObj = Memory.ZwReadUInt64(processHandle, (IntPtr)(gWorld + 0x70));

                    pArrayLen = Memory.ZwReadInt32(processHandle, (IntPtr)(pArrayObj + 0x18)); // List<T>._size
                    itemsArrayLen = Memory.ZwReadInt32(processHandle, (IntPtr)(itemsArrayObj + 0x18)); // List<T>._size

                    if (pArrayLen > 0)
                    {
                        if ((IntPtr)pArrayObj == IntPtr.Zero || pArrayLen > 0x300)
                            continue;

                        Console.WriteLine("Found LocalGameWorld : " + gWorld.ToString("X"));
                        IsInGame = true;
                        break;
                    }
                }
            }
            else
            {
                pArrayObj = Memory.ZwReadUInt64(processHandle, (IntPtr)(gWorld + 0x78)); // -.ClientLocalGameWorld->RegisteredPlayers (Type: List<Player>)

                itemsArrayObj = Memory.ZwReadUInt64(processHandle, (IntPtr)(gWorld + 0x70));

                pArrayLen = Memory.ZwReadInt32(processHandle, (IntPtr)(pArrayObj + 0x18)); // List<T>._size
                itemsArrayLen = Memory.ZwReadInt32(processHandle, (IntPtr)(itemsArrayObj + 0x18)); // List<T>._size

                if (pArrayLen <= 0)
                {
                    ItemsAddressHistory.Clear();
                    LootItems.Clear();
                    IsInGame = false;
                    LocalPlayerPtr = IntPtr.Zero;
                }
            }

            if (!IsInGame)
            {
                ItemsAddressHistory.Clear();
                LootItems.Clear();
                LocalPlayerPtr = IntPtr.Zero;
                return;
            }

            if (Components.VisualsComponent.DrawTotalPlayersCount.Enabled)
                Renderer.DrawText("Total Players (Scavs Included): " + pArrayLen, new Vector2(5, 5), Color.Red);


            var pArrayBase = Memory.ZwReadUInt64(processHandle, (IntPtr)pArrayObj + 0x10) + 0x20; // List<T>._items.ArrayBase

            bool _usingOptics;
            var cameraGameObject = FindCameraByName("BaseOpticCamera(Clone)"); // Aiming down sights (camera optics)
            if (cameraGameObject == 0)
            {
                cameraGameObject = FindCameraByName("FPS Camera");
                _usingOptics = false;
            }
            else _usingOptics = true;

            if (cameraGameObject == 0) return;

            var cameraComponent = ReadChain(cameraGameObject, 0x30, 0x18);

            // Camera position is (you can save a read if you split the chain): 
            //    var cTransform = Memory.ReadChain(cameraGameObject, 0x30, 0x08, 0x38);
            //    var cPosition = Memory.Read<Vector3>(cTransform + 0xB0);

            var viewMatrix = Memory.ZwReadMatrix(processHandle, (IntPtr)cameraComponent + 0xD8);


            if (LocalPlayerPtr == IntPtr.Zero)
            {
                for (uint i = 0; i < pArrayLen; i++)
                {
                    var entity = Memory.ZwReadPointer(processHandle, (IntPtr)(pArrayBase + i * 0x8), isWow64Process);
                    if (Memory.ZwReadPointer(processHandle, (IntPtr)entity.ToInt64() + 0x18, isWow64Process) !=
                        IntPtr.Zero)
                    {
                        LocalPlayerPtr = entity;
                        break;
                    }
                }
            }

            var localpProfile = Memory.ZwReadUInt64(processHandle, (IntPtr)LocalPlayerPtr.ToInt64() + 0x3D0);
            var localpInfo = Memory.ZwReadUInt64(processHandle, (IntPtr)localpProfile + 0x28);
            var localpGroupIdPtr = Memory.ZwReadUInt64(processHandle, (IntPtr)localpInfo + 0x18);
            var localpGroupId = Memory.ZwReadString(processHandle, (IntPtr)localpGroupIdPtr + 0x14, true, 64);

            var localLocation = Memory.ZwReadVector3(processHandle, (IntPtr)ReadChainPtr(LocalPlayerPtr.ToInt64(), false, 0x10, 0x30, 0x30, 0x8, 0x38).ToInt64() + 0xb0);


            if (Components.ItemsVisualsComponent.DrawItems.Enabled)
            {
                var itemsArrayBase = Memory.ZwReadInt64(processHandle, (IntPtr)itemsArrayObj + 0x10) + 0x20;

                var itemsMenuComponentDic =
                    ItemsMenu.Cast<KeyValuePair<string, MenuComponent>>()
                        .ToDictionary(pair => pair.Key, pair => pair.Value);

                if (ItemsRefereshDT <= DateTime.Now)
                    LootItems.Clear();

                if (LootItems.Count == 0)
                {
                    for (int i = 0; i < itemsArrayLen; i++)
                    {
                        var item = Memory.ZwReadPointer(processHandle, (IntPtr)(itemsArrayBase + i * 0x8),
                            isWow64Process);
                        var itemLoc = Memory.ZwReadVector3(processHandle, (IntPtr)item.ToInt64() + 0x28);
                        var itemObj = Memory.ZwReadPointer(processHandle, (IntPtr)item.ToInt64() + 0x18,
                            isWow64Process);
                        var itemObjId = Memory.ZwReadPointer(processHandle, (IntPtr)itemObj.ToInt64() + 0x18,
                            isWow64Process);
                        var itemObjIdStr = Memory.ZwReadString(processHandle, (IntPtr)itemObjId + 0x14, true, 64);
                        var currentAddress = Memory.ZwReadPointer(processHandle, (IntPtr)itemObj.ToInt64() + 0x38,
                            isWow64Process);
                        var itemTemplate = Memory.ZwReadPointer(processHandle, (IntPtr)itemObj.ToInt64() + 0x40,
                            isWow64Process);
                        var itemIdStr = Memory.ZwReadUInt64(processHandle, (IntPtr)itemTemplate.ToInt64() + 0x50);
                        var itemId =
                            Memory.ZwReadString(processHandle, (IntPtr)itemIdStr + 0x14, true, 64); // Read as Unicode
                        bool alreadyLooted = false;
                        if (!ItemsAddressHistory.ContainsKey(itemObjIdStr))
                            ItemsAddressHistory.Add(itemObjIdStr, currentAddress.ToInt64());
                        else
                            alreadyLooted = ItemsAddressHistory[itemObjIdStr] != currentAddress.ToInt64();

                        if (!itemsMenuComponentDic.ContainsKey(itemId) ||
                            !itemsMenuComponentDic[itemId].Enabled)
                            continue;

                        LootItems.Add(new ItemObj
                        {
                            itemId = itemId,
                            pos = itemLoc,
                            AlreadyLooted = alreadyLooted
                        });
                    }

                    ItemsRefereshDT = DateTime.Now.AddMilliseconds(2000);
                }

                foreach (var item in LootItems)
                {
                    string itemName = "";
                    if (KeysIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenukeysenable"].Enabled)
                            continue;
                        itemName = KeysIdsToNamesDic[item.itemId];
                    }
                    else if (InfoIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenuinfoenable"].Enabled)
                            continue;
                        itemName = InfoIdsToNamesDic[item.itemId];
                    }
                    else if (ProvisionsIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenuprovisionsenable"].Enabled)
                            continue;
                        itemName = ProvisionsIdsToNamesDic[item.itemId];
                    }
                    else if (QuestIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenuquestenable"].Enabled)
                            continue;
                        itemName = QuestIdsToNamesDic[item.itemId];
                    }
                    else if (MedicalIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenumedicalenable"].Enabled)
                            continue;
                        itemName = MedicalIdsToNamesDic[item.itemId];
                    }
                    else if (GearsIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenugearenable"].Enabled)
                            continue;
                        itemName = GearsIdsToNamesDic[item.itemId];
                    }
                    else if (ModsIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenumodsenable"].Enabled)
                            continue;
                        itemName = ModsIdsToNamesDic[item.itemId];
                    }
                    else if (BarterIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenubarterenable"].Enabled)
                            continue;
                        itemName = BarterIdsToNamesDic[item.itemId];
                    }
                    else if (AmmoIdsToNamesDic.ContainsKey(item.itemId))
                    {
                        if (!itemsMenuComponentDic["itemsmenuammoenable"].Enabled)
                            continue;
                        itemName = AmmoIdsToNamesDic[item.itemId];
                    }

                    if (item.AlreadyLooted) continue;
                    var distance = GetDistance3D(localLocation, item.pos);
                    var vecOut = new Vector2();
                    if (Renderer.WorldToScreen(item.pos, out vecOut, viewMatrix, wndMargins, wndSize, W2SType.TypeOGL))
                    {
                        Renderer.DrawText(itemName + $"[{distance:0}m]", vecOut, Color.White,
                            12);
                    }
                }
            }

            if (Components.VisualsComponent.DrawTheVisuals.Enabled)
            {
                for (uint i = 0; i < pArrayLen; i++)
                {
                    var entity = Memory.ZwReadPointer(processHandle, (IntPtr)(pArrayBase + i * 0x8), isWow64Process);

                    var pProfile =
                        Memory.ZwReadUInt64(processHandle,
                            (IntPtr)entity.ToInt64() + 0x3D0); // EFT.Player->profile_0x3d8 (Type: EFT.Profile)
                    var pInfo = Memory.ZwReadUInt64(processHandle,
                        (IntPtr)pProfile + 0x28); // EFT.Profile->Info (Type: -.ObfuscatedClass1044)
                    var pGroupIdPtr =
                        Memory.ZwReadUInt64(processHandle,
                            (IntPtr)pInfo + 0x18); // EFT.Profile->Info (Type: -.ObfuscatedClass1044)
                    var pGroupIdStr =
                        Memory.ZwReadString(processHandle, (IntPtr)pGroupIdPtr + 0x14, true,
                            64); // EFT.Profile->Info (Type: -.ObfuscatedClass1044)
                    var pNickname =
                        Memory.ZwReadUInt64(processHandle,
                            (IntPtr)pInfo + 0x10); //-.GClass1044->Nickname (Type: String)
                    var pName = Memory.ZwReadString(processHandle, (IntPtr)pNickname + 0x14, true,
                        64); // Read as Unicode
                    var pHealthController = Memory.ZwReadInt64(processHandle, (IntPtr)entity.ToInt64() + 0x400);

                    float totalHP = 0f;
                    float currentHP = 0f;

                    if (!string.IsNullOrEmpty(localpGroupId) && localpGroupId == pGroupIdStr &&
                        !Components.VisualsComponent.DrawAlliesEsp.Enabled)
                        continue;

                    if (Components.VisualsComponent.DrawBoxHP.Enabled)
                    {
                        var bodyContainer =
                            ReadChainPtr(pHealthController, false, 0x18, 0x18);
                        for (uint offset = 0x30; offset < 0xC0; offset += 0x18)
                        {
                            var hpObject = ReadChainPtr(bodyContainer.ToInt64(), false, offset, 0x10);
                            var currHP = Memory.ZwReadFloat(processHandle, (IntPtr)hpObject.ToInt64() + 0x10);
                            var maxHP = Memory.ZwReadFloat(processHandle, (IntPtr)hpObject.ToInt64() + 0x14);

                            totalHP += maxHP;
                            currentHP += currHP;
                        }
                    }

                    float botsPredictHP = totalHP / 4.4f;

                    if (LocalPlayerPtr == entity)
                        continue;
                    //Console.Write(pName + " - ");
                    var pRegTime =
                        Memory.ZwReadInt32(processHandle,
                            (IntPtr)(pInfo + 0x54)); // -.ObfuscatedClass1044->RegistrationDate (Type: Int32)
                    var isPlayer = pRegTime != 0;

                    var FPSBoxColor = Color.Blue;

                    if (isPlayer)
                    {
                        if (!string.IsNullOrEmpty(localpGroupId) && pGroupIdStr == localpGroupId) //For TeamMates
                            FPSBoxColor = Components.VisualsComponent.AlliesColor.Color;
                        else
                            FPSBoxColor = Components.VisualsComponent.EnemiesColor.Color; //For Enemy Players.
                    }
                    else
                    {
                        if (botsPredictHP >= 150) //For Bosses
                        {
                            FPSBoxColor = Color.Purple;
                        }
                        else if (botsPredictHP >= 137) //For Raiders
                        {
                            FPSBoxColor = Color.HotPink;
                        }
                        else //For Bots!
                        {
                            FPSBoxColor = Components.VisualsComponent.BotsColor.Color;
                        }
                    }

                    string playerNameToDisplay = "";

                    if (isPlayer)
                        playerNameToDisplay = pName;
                    else if (botsPredictHP >= 150) //Boss
                        playerNameToDisplay = "Boss";
                    else if (botsPredictHP >= 137) //Raiders
                        playerNameToDisplay = "Raider";
                    else //Bot..
                        playerNameToDisplay = "Bot";

                    var locationForEnemy = Memory.ZwReadVector3(processHandle,
                        (IntPtr)ReadChainPtr(entity.ToInt64(), false, 0x10, 0x30, 0x30, 0x8, 0x38).ToInt64() + 0xb0);
                    if (locationForEnemy.IsZero)
                        continue;
                    locationForEnemy.Y += 2f;

                    var locationForEnemyFeet = locationForEnemy; //Memory.ZwReadVector3(processHandle, (IntPtr)(entity.ToInt64() + 0x5FC));
                    var distance = GetDistance3D(localLocation, locationForEnemy);
                    Vector2 vecOut;
                    Vector2 vecOutFeet;
                    if (Renderer.WorldToScreen(locationForEnemy, out vecOut, viewMatrix, wndMargins, wndSize, W2SType.TypeOGL))
                    {
                        locationForEnemyFeet.Y -= 2.95f;
                        if (Renderer.WorldToScreen(locationForEnemyFeet, out vecOutFeet, viewMatrix, wndMargins,
                            wndSize, W2SType.TypeOGL))
                        {
                            Renderer.DrawFPSBox(vecOut, vecOutFeet, FPSBoxColor, BoxStance.standing, Components.VisualsComponent.DrawBoxThic.Value, Components.VisualsComponent.DrawBoxBorder.Enabled, Components.VisualsComponent.DrawBox.Enabled,
                                currentHP, totalHP, 0, 0, Components.VisualsComponent.DrawTextSize.Value, !Components.VisualsComponent.DrawTextName.Enabled ? "" : playerNameToDisplay, !Components.VisualsComponent.DrawTextDist.Enabled ? "" : distance.ToString("0") + "m", "", "", "");
                        }
                    }

                    //var bMtx = ReadChain((uint)entity.ToInt64(), 0xA0, 0x28, 0x28, 0x18);

                    //if (bMtx == 0)
                    //    continue;

                    //var bonesList = (IntPtr)ReadChain((uint)entity.ToInt64(), 0xA0, 0x28, 0x28);
                    //var bonesCnt = Memory.ZwReadInt32(processHandle, (IntPtr)bonesList.ToInt64() + 0x18);
                    //Console.WriteLine("Bones Count : " + bonesCnt);
                    //bMtx += 0x20;

                    //for (int j = 0; j < bonesCnt; j++)
                    //{
                    //    var boneObj = Memory.ZwReadPointer(processHandle, (IntPtr)bMtx + j * 0x8, isWow64Process);
                    //    if (boneObj == IntPtr.Zero)
                    //        continue;

                    //    var boneBeforeVec = ReadChain((uint)boneObj.ToInt64(), 0x10, 0x38);

                    //    var boneLoc = Memory.ZwReadVector3(processHandle, (IntPtr)boneBeforeVec + 0xB0);

                    //    if (boneLoc.IsZero)
                    //        continue;

                    //    if (Renderer.WorldToScreen(boneLoc, out vecOutFeet, viewMatrix, wndMargins, wndSize, W2SType.TypeOGL))
                    //    {
                    //        Renderer.DrawCircleFilled(vecOutFeet, 3f, Color.Red);
                    //    }
                    //}
                }
            }
        }
    }

    public class ItemObj
    {
        public Vector3 pos;
        public string itemId;
        public string address;
        public bool AlreadyLooted;
    }
}
