using System;
using System.Collections.Generic;
using System.IO;
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

namespace DeadByDaylight
{
    class Program
    {
        public static IntPtr processHandle = IntPtr.Zero; //processHandle variable used by OpenProcess (once)
        public static bool gameProcessExists = false; //avoid drawing if the game process is dead, or not existent
        public static bool isWow64Process = false; //we all know the game is 32bit, but anyway...
        public static bool isGameOnTop = false; //we should avoid drawing while the game is not set on top
        public static bool isOverlayOnTop = false; //we might allow drawing visuals, while the user is working with the "menu"
        public static uint PROCESS_ALL_ACCESS = 0x1FFFFF; //hardcoded access right to OpenProcess (even EAC strips some of the access flags)
        public static Vector2 wndMargins = new Vector2(0, 0); //if the game window is smaller than your desktop resolution, you should avoid drawing outside of it
        public static Vector2 wndSize = new Vector2(0, 0); //get the size of the game window ... to know where to draw
        public static IntPtr GameBase = IntPtr.Zero;
        public static IntPtr GameSize = IntPtr.Zero;
        public static DateTime LastSpacePressedDT = DateTime.Now;
        public static IntPtr GWorldPtr = IntPtr.Zero;
        public static IntPtr GNamesPtr = IntPtr.Zero;

        public static Vector3 FMinimalViewInfo_Location = new Vector3(0, 0, 0);
        public static Vector3 FMinimalViewInfo_Rotation = new Vector3(0, 0, 0);
        public static float FMinimalViewInfo_FOV = 0;

        public static uint survivorID = 0;
        public static uint killerID = 0;
        public static uint escapeID = 0;
        public static uint hatchID = 0;
        public static uint generatorID = 0;


        public static Menu RootMenu { get; private set; }
        public static Menu VisualsMenu { get; private set; }
        public static Menu MiscMenu { get; private set; }

        class Components
        {
            public static readonly MenuKeyBind MainAssemblyToggle = new MenuKeyBind("mainassemblytoggle", "Toggle the whole assembly effect by pressing key:", VirtualKeyCode.Delete, KeybindType.Toggle, true);
            public static class VisualsComponent
            {
                public static readonly MenuBool DrawTheVisuals = new MenuBool("drawthevisuals", "Enable all of the Visuals", true);
                public static readonly MenuColor SurvColor = new MenuColor("srvcolor", "Survivors Color", new SharpDX.Color(0, 255, 0, 60));
                public static readonly MenuBool DrawSurvivorBox = new MenuBool("srvbox", "Draw Survivors Box", true);
                public static readonly MenuColor KillerColor = new MenuColor("kilcolor", "Killers Color", new SharpDX.Color(255, 0, 0, 100));
                public static readonly MenuBool DrawKillerBox = new MenuBool("drawbox", "Draw Box ESP", true);
                public static readonly MenuSlider DrawBoxThic = new MenuSlider("boxthickness", "Draw Box Thickness", 0, 0, 10);
                public static readonly MenuBool DrawBoxBorder = new MenuBool("drawboxborder", "Draw Border around Box and Text?", true);
                public static readonly MenuBool DrawMiscInfo = new MenuBool("drawmiscinfos", "Draw hatch and escape positions", true);
                public static readonly MenuColor MiscColor = new MenuColor("misccolor", "Draw Text Color", new SharpDX.Color(255, 255, 255, 100));
                public static readonly MenuBool DrawGenerators = new MenuBool("drawgenerators", "Draw Generators positions", true);
                //public static readonly MenuSlider OffsetGuesser = new MenuSlider("ofsgues", "Guess the offset", 10, 1, 250);
            }

            public static class MiscComponent
            {
                public static readonly MenuBool AutoSkillCheck = new MenuBool("autoskillcheck", "Auto Skill Check (+Bonus)", true);
            }
        }

        public static void InitializeMenu()
        {
            VisualsMenu = new Menu("visualsmenu", "Visuals Menu")
            {
                Components.VisualsComponent.DrawTheVisuals,
                Components.VisualsComponent.SurvColor,
                Components.VisualsComponent.DrawSurvivorBox,
                Components.VisualsComponent.KillerColor,
                Components.VisualsComponent.DrawKillerBox,
                Components.VisualsComponent.DrawBoxThic.SetToolTip("Setting thickness to 0 will let the assembly auto-adjust itself depending on model distance"),
                Components.VisualsComponent.DrawBoxBorder.SetToolTip("Drawing borders may take extra performance (FPS) on low-end computers"),
                Components.VisualsComponent.DrawMiscInfo,
                Components.VisualsComponent.MiscColor,
                Components.VisualsComponent.DrawGenerators,
                //Components.VisualsComponent.OffsetGuesser,
            };

            MiscMenu = new Menu("miscmenu", "Misc Menu")
            {
                Components.MiscComponent.AutoSkillCheck
            };


            RootMenu = new Menu("dbdexample", "WeScript.app DeadByDaylight Example Assembly", true)
            {
                Components.MainAssemblyToggle.SetToolTip("The magical boolean which completely disables/enables the assembly!"),
                VisualsMenu,
                MiscMenu,
            };
            RootMenu.Attach();
        }


        public static string GetNameFromID(uint ID) //really bad implementation - probably needs fixing, plus it's better to use it as a dumper once at startup and cache ids
        {
            if (processHandle != IntPtr.Zero)
            {
                if (GameBase != IntPtr.Zero)
                {
                    var GNamesAddress = Memory.ZwReadPointer(processHandle, GNamesPtr, isWow64Process); //48 8B 05 ? ? ? ? 48 85 C0 75 5F
                    if (GNamesAddress != IntPtr.Zero)
                    {
                        UInt64 ChunkIndex = ID / 0x4000;
                        UInt64 WithinChunkIndex = ID % 0x4000;
                        var fNamePtr = Memory.ZwReadPointer(processHandle, (IntPtr)(GNamesAddress.ToInt64() + (long)ChunkIndex * 0x8), isWow64Process);
                        if (fNamePtr != IntPtr.Zero)
                        {
                            var fName = Memory.ZwReadPointer(processHandle, (IntPtr)(fNamePtr.ToInt64() + 0x8 * (long)WithinChunkIndex), isWow64Process);
                            if (fName != IntPtr.Zero)
                            {
                                var name = Memory.ZwReadString(processHandle, (IntPtr)fName.ToInt64() + 0xC, false, 64);
                                if (name.Length > 0) return name;
                            }
                        }
                    }
                }
            }
            return "NULL";
        }

        static void Main(string[] args)
        {
            Console.WriteLine("WeScript.app experimental DBD assembly for patch 4.0.2 with Driver bypass");
            InitializeMenu();
            if (!Memory.InitDriver(DriverName.frost_64))
            {
                Console.WriteLine("[ERROR] Failed to initialize driver for some reason...");
            }
            Renderer.OnRenderer += OnRenderer;
            Memory.OnTick += OnTick;
        }
        public static double dims = 0.01905f;
        private static double GetDistance3D(Vector3 myPos, Vector3 enemyPos)
        {
            Vector3 vector = new Vector3(myPos.X - enemyPos.X, myPos.Y - enemyPos.Y, myPos.Z - enemyPos.Z);
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z) * dims;
        }
        private static void OnTick(int counter, EventArgs args)
        {
            if (processHandle == IntPtr.Zero) //if we still don't have a handle to the process
            {
                var wndHnd = Memory.FindWindowName("DeadByDaylight  "); //why the devs added spaces after the name?!
                if (wndHnd != IntPtr.Zero) //if it exists
                {
                    //Console.WriteLine("weheree");
                    var calcPid = Memory.GetPIDFromHWND(wndHnd); //get the PID of that same process
                    if (calcPid > 0) //if we got the PID
                    {
                        processHandle = Memory.ZwOpenProcess(PROCESS_ALL_ACCESS, calcPid); //the driver will get a stripped handle, but doesn't matter, it's still OK
                        if (processHandle != IntPtr.Zero)
                        {
                            //if we got access to the game, check if it's x64 bit, this is needed when reading pointers, since their size is 4 for x86 and 8 for x64
                            isWow64Process = Memory.IsProcess64Bit(processHandle); //we know DBD is 64 bit but anyway...
                        }
                        else
                        {
                            Console.WriteLine("failed to get handle");
                        }
                    }
                }
            }
            else //else we have a handle, lets check if we should close it, or use it
            {
                var wndHnd = Memory.FindWindowName("DeadByDaylight  "); //why the devs added spaces after the name?!
                if (wndHnd != IntPtr.Zero) //window still exists, so handle should be valid? let's keep using it
                {
                    //the lines of code below execute every 33ms outside of the renderer thread, heavy code can be put here if it's not render dependant
                    gameProcessExists = true;
                    wndMargins = Renderer.GetWindowMargins(wndHnd);
                    wndSize = Renderer.GetWindowSize(wndHnd);
                    isGameOnTop = Renderer.IsGameOnTop(wndHnd);
                    isOverlayOnTop = Overlay.IsOnTop();

                    if (GameBase == IntPtr.Zero) //do we have access to Gamebase address?
                    {
                        GameBase = Memory.ZwGetModule(processHandle, null, isWow64Process); //if not, find it
                        //Console.WriteLine($"GameBase: {GameBase.ToString("X")}");
                        Console.WriteLine("Got GAMEBASE of DBD!");
                    }
                    else
                    {
                        if (GameSize == IntPtr.Zero)
                        {
                            GameSize = Memory.ZwGetModuleSize(processHandle, null, isWow64Process);
                            //Console.WriteLine($"GameSize: {GameSize.ToString("X")}");
                        }
                        else
                        {
                            if (GWorldPtr == IntPtr.Zero)
                            {
                                GWorldPtr = Memory.ZwFindSignature(processHandle, GameBase, GameSize, "48 8B 1D ? ? ? ? 48 85 DB 74 3B 41", 0x3);
                                Console.WriteLine($"GWorldPtr: {GWorldPtr.ToString("X")}");
                            }
                            if (GNamesPtr == IntPtr.Zero)
                            {
                                GNamesPtr = Memory.ZwFindSignature(processHandle, GameBase, GameSize, "48 8B 05 ? ? ? ? 48 85 C0 75 5F", 0x3);
                                Console.WriteLine($"GNamesPtr: {GNamesPtr.ToString("X")}");
                            }
                        }
                    }

                }
                else //else most likely the process is dead, clean up
                {
                    Memory.CloseHandle(processHandle); //close the handle to avoid leaks
                    processHandle = IntPtr.Zero; //set it like this just in case for C# logic
                    gameProcessExists = false;
                    //clear your offsets, modules
                    GameBase = IntPtr.Zero;
                    GameSize = IntPtr.Zero;

                    GWorldPtr = IntPtr.Zero;
                    GNamesPtr = IntPtr.Zero;

                }
            }
        }

        private static void OnRenderer(int fps, EventArgs args)
        {
            if (!gameProcessExists) return; //process is dead, don't bother drawing
            if ((!isGameOnTop) && (!isOverlayOnTop)) return; //if game and overlay are not on top, don't draw
            if (!Components.MainAssemblyToggle.Enabled) return; //main menu boolean to toggle the cheat on or off

            //Renderer.DrawText($"min: {(151000 + (Components.VisualsComponent.OffsetGuesser.Value * 10)).ToString()} max: {(151100 + (Components.VisualsComponent.OffsetGuesser.Value * 10)).ToString()}", new Vector2(300, 300));

            //Matrix viewProj = new Matrix();
            var myPos = new Vector3();
            var USkillCheck = IntPtr.Zero;
            var UWorld =
                Memory.ZwReadPointer(processHandle, GWorldPtr,
                    isWow64Process); //48 8B 1D ?? ?? ?? ?? 48 85 DB 74 3B 41 || mov rbx,[DeadByDaylight-Win64-Shipping.exe+5A29158]
            if (UWorld != IntPtr.Zero)
            {
                var UGameInstance =
                    Memory.ZwReadPointer(processHandle, (IntPtr) UWorld.ToInt64() + 0x170, isWow64Process);
                if (UGameInstance != IntPtr.Zero)
                {
                    var localPlayerArray = Memory.ZwReadPointer(processHandle, (IntPtr) UGameInstance.ToInt64() + 0x40,
                        isWow64Process);
                    if (localPlayerArray != IntPtr.Zero)
                    {
                        var ULocalPlayer = Memory.ZwReadPointer(processHandle, localPlayerArray, isWow64Process);
                        if (ULocalPlayer != IntPtr.Zero)
                        {
                            var ULocalPlayerControler = Memory.ZwReadPointer(processHandle,
                                (IntPtr) ULocalPlayer.ToInt64() + 0x0038, isWow64Process);
                            if (ULocalPlayerControler != IntPtr.Zero)
                            {
                                var ULocalPlayerPawn = Memory.ZwReadPointer(processHandle,
                                    (IntPtr) ULocalPlayerControler.ToInt64() + 0x0378, isWow64Process);
                                if (ULocalPlayerPawn != IntPtr.Zero)
                                {
                                    var UInteractionHandler = Memory.ZwReadPointer(processHandle,
                                        (IntPtr) ULocalPlayerPawn.ToInt64() + 0x0BF0, isWow64Process);

                                    if (UInteractionHandler != IntPtr.Zero)
                                    {
                                        USkillCheck = Memory.ZwReadPointer(processHandle,
                                            (IntPtr) UInteractionHandler.ToInt64() + 0x0278, isWow64Process);
                                    }

                                    var ULocalRoot = Memory.ZwReadPointer(processHandle,
                                        (IntPtr) ULocalPlayerPawn.ToInt64() + 0x0168, isWow64Process);
                                    if (ULocalRoot != IntPtr.Zero)
                                    {
                                        myPos = Memory.ZwReadVector3(processHandle,
                                            (IntPtr) ULocalRoot.ToInt64() + 0x017C);
                                    }
                                }

                                var APlayerCameraManager = Memory.ZwReadPointer(processHandle,
                                    (IntPtr) ULocalPlayerControler.ToInt64() + 0x3E0, isWow64Process);
                                if (APlayerCameraManager != IntPtr.Zero)
                                {
                                    FMinimalViewInfo_Location = Memory.ZwReadVector3(processHandle,
                                        (IntPtr) APlayerCameraManager.ToInt64() + 0x1A30 + 0x0010);
                                    FMinimalViewInfo_Rotation = Memory.ZwReadVector3(processHandle,
                                        (IntPtr) APlayerCameraManager.ToInt64() + 0x1A30 + 0x001C);
                                    FMinimalViewInfo_FOV = Memory.ZwReadFloat(processHandle,
                                        (IntPtr) APlayerCameraManager.ToInt64() + 0x1A30 + 0x0028);
                                }
                            }

                            //var CameraPtr = Memory.ZwReadPointer(processHandle, (IntPtr)ULocalPlayer.ToInt64() + 0xB8, isWow64Process);
                            //if (CameraPtr != IntPtr.Zero)
                            //{
                            //    viewProj = Memory.ZwReadMatrix(processHandle, (IntPtr)(CameraPtr.ToInt64() + 0x1FC));
                            //}
                        }
                    }
                }

                if (Components.MiscComponent.AutoSkillCheck.Enabled)
                {
                    if (USkillCheck != IntPtr.Zero)
                    {
                        var isDisplayed = Memory.ZwReadBool(processHandle,
                            (IntPtr) USkillCheck.ToInt64() + 0x0308);
                        if (isDisplayed && LastSpacePressedDT.AddMilliseconds(200) <
                            DateTime.Now)
                        {
                            var currentProgress = Memory.ZwReadFloat(processHandle,
                                (IntPtr) USkillCheck.ToInt64() + 0x02A0);
                            var startSuccessZone = Memory.ZwReadFloat(processHandle,
                                (IntPtr) USkillCheck.ToInt64() + 0x0270);

                            if (currentProgress > startSuccessZone)
                            {
                                LastSpacePressedDT = DateTime.Now;
                                Input.KeyPress(VirtualKeyCode.Space);
                            }
                        }
                    }
                }

                var ULevel = Memory.ZwReadPointer(processHandle, (IntPtr) UWorld.ToInt64() + 0x38, isWow64Process);
                if (ULevel != IntPtr.Zero)
                {
                    var AActors = Memory.ZwReadPointer(processHandle, (IntPtr) ULevel.ToInt64() + 0xA0, isWow64Process);
                    var ActorCnt = Memory.ZwReadUInt32(processHandle, (IntPtr) ULevel.ToInt64() + 0xA8);
                    if ((AActors != IntPtr.Zero) && (ActorCnt > 0))
                    {
                        for (uint i = 0; i <= ActorCnt; i++)
                        {
                            var AActor = Memory.ZwReadPointer(processHandle, (IntPtr) (AActors.ToInt64() + i * 8),
                                isWow64Process);
                            if (AActor != IntPtr.Zero)
                            {
                                var USceneComponent = Memory.ZwReadPointer(processHandle,
                                    (IntPtr) AActor.ToInt64() + 0x168, isWow64Process);
                                if (USceneComponent != IntPtr.Zero)
                                {
                                    var tempVec = Memory.ZwReadVector3(processHandle,
                                        (IntPtr) USceneComponent.ToInt64() + 0x160);
                                    var AActorID = Memory.ZwReadUInt32(processHandle,
                                        (IntPtr) AActor.ToInt64() + 0x18);

                                    if ((AActorID > 0) && (AActorID < 200000))
                                    {
                                        if ((survivorID == 0) || (killerID == 0) || (escapeID == 0) ||
                                            (hatchID == 0) || (generatorID == 0))
                                        {

                                            var retname = GetNameFromID(AActorID);
                                            if (retname.Contains("BP_CamperInteractable_"))
                                            {
                                                survivorID = AActorID;
                                            }

                                            if (retname.Contains("SlasherInteractable_"))
                                            {
                                                killerID = AActorID;
                                            }

                                            if (retname.Contains("BP_Escape01"))
                                            {
                                                escapeID = AActorID;
                                            }

                                            if (retname.Contains("BP_Hatch"))
                                            {
                                                hatchID = AActorID;
                                            }

                                            if (retname.StartsWith("Generator"))
                                                generatorID = AActorID;
                                        }

                                        //the check below is a ghetto way to "guess" the ID of players and killers using a slider in the menu
                                        //Vector2 vScreen_d3d11 = new Vector2(0, 0);
                                        //if ((AActorID >= 160000 + (Components.VisualsComponent.OffsetGuesser.Value * 10)) && (AActorID <= 160100 + (Components.VisualsComponent.OffsetGuesser.Value * 10)))
                                        //{
                                        //    if (Renderer.WorldToScreen(tempVec, out vScreen_d3d11, viewProj, wndMargins, wndSize, W2SType.TypeD3D11))
                                        //    {
                                        //        //Renderer.DrawText($"ID: {AActorID.ToString()}", vScreen_d3d11, new Color(255, 255, 255), 12, TextAlignment.centered, false);
                                        //        var gnm = GetNameFromID(AActorID);
                                        //        if (gnm != "NULL")
                                        //        {
                                        //            Renderer.DrawText($"ID: {AActorID.ToString()} {gnm}" , vScreen_d3d11, Color.White, 12, TextAlignment.centered, false);
                                        //        }
                                        //    }
                                        //}
                                        //var gnm = GetNameFromID(AActorID);
                                        //{
                                        //    if ((gnm.Contains("BP_Hatch")) || (gnm.Contains("Generator")) || (gnm.Contains("BP_Escape01")))
                                        //    {
                                        //        Vector2 vScreen_d3d11 = new Vector2(0, 0);
                                        //        if (Renderer.WorldToScreen(tempVec, out vScreen_d3d11, viewProj, wndMargins, wndSize, W2SType.TypeD3D11))
                                        //        {
                                        //            Renderer.DrawText(gnm, vScreen_d3d11, Color.White, 12, TextAlignment.centered, false);
                                        //        }
                                        //    }
                                        //}
                                    }

                                    if (Components.VisualsComponent.DrawTheVisuals.Enabled
                                    ) //this should have been placed earlier?
                                    {
                                        int dist = (int) (GetDistance3D(myPos, tempVec));
                                        if (AActorID == survivorID)
                                        {
                                            Vector2 vScreen_h3ad = new Vector2(0, 0);
                                            Vector2 vScreen_f33t = new Vector2(0, 0);
                                            if (Renderer.WorldToScreenUE4(
                                                new Vector3(tempVec.X, tempVec.Y, tempVec.Z + 60.0f),
                                                out vScreen_h3ad, FMinimalViewInfo_Location, FMinimalViewInfo_Rotation,
                                                FMinimalViewInfo_FOV, wndMargins, wndSize))
                                            {
                                                Renderer.WorldToScreenUE4(
                                                    new Vector3(tempVec.X, tempVec.Y, tempVec.Z - 130.0f),
                                                    out vScreen_f33t, FMinimalViewInfo_Location,
                                                    FMinimalViewInfo_Rotation, FMinimalViewInfo_FOV, wndMargins,
                                                    wndSize);
                                                if (Components.VisualsComponent.DrawSurvivorBox.Enabled)
                                                {
                                                    Renderer.DrawFPSBox(vScreen_h3ad, vScreen_f33t,
                                                        Components.VisualsComponent.SurvColor.Color,
                                                        BoxStance.standing,
                                                        Components.VisualsComponent.DrawBoxThic.Value,
                                                        Components.VisualsComponent.DrawBoxBorder.Enabled);
                                                    Renderer.DrawText("SURVIVOR [" + dist + "m]", vScreen_f33t.X,
                                                        vScreen_f33t.Y + 5,
                                                        Components.VisualsComponent.SurvColor.Color, 12,
                                                        TextAlignment.centered, false);
                                                }
                                            }
                                        }

                                        if (AActorID == killerID)
                                        {
                                            Vector2 vScreen_h3ad = new Vector2(0, 0);
                                            Vector2 vScreen_f33t = new Vector2(0, 0);
                                            if (Renderer.WorldToScreenUE4(
                                                new Vector3(tempVec.X, tempVec.Y, tempVec.Z + 80.0f),
                                                out vScreen_h3ad, FMinimalViewInfo_Location, FMinimalViewInfo_Rotation,
                                                FMinimalViewInfo_FOV, wndMargins, wndSize))
                                            {
                                                Renderer.WorldToScreenUE4(
                                                    new Vector3(tempVec.X, tempVec.Y, tempVec.Z - 150.0f),
                                                    out vScreen_f33t, FMinimalViewInfo_Location,
                                                    FMinimalViewInfo_Rotation, FMinimalViewInfo_FOV, wndMargins,
                                                    wndSize);
                                                if (Components.VisualsComponent.DrawKillerBox.Enabled)
                                                {
                                                    Renderer.DrawFPSBox(vScreen_h3ad, vScreen_f33t,
                                                        Components.VisualsComponent.KillerColor.Color,
                                                        BoxStance.standing,
                                                        Components.VisualsComponent.DrawBoxThic.Value,
                                                        Components.VisualsComponent.DrawBoxBorder.Enabled);
                                                    Renderer.DrawText("KILLER [" + dist + "m]", vScreen_f33t.X,
                                                        vScreen_f33t.Y + 5,
                                                        Components.VisualsComponent.KillerColor.Color, 12,
                                                        TextAlignment.centered, false);
                                                }
                                            }
                                        }

                                        if (Components.VisualsComponent.DrawMiscInfo.Enabled)
                                        {
                                            if (AActorID == escapeID)
                                            {
                                                Vector2 vScreen_d3d11 = new Vector2(0, 0);
                                                if (Renderer.WorldToScreenUE4(tempVec, out vScreen_d3d11,
                                                    FMinimalViewInfo_Location, FMinimalViewInfo_Rotation,
                                                    FMinimalViewInfo_FOV,
                                                    wndMargins, wndSize))
                                                {
                                                    Renderer.DrawText("ESCAPE [" + dist + "m]", vScreen_d3d11,
                                                        Components.VisualsComponent.MiscColor.Color, 12,
                                                        TextAlignment.centered, false);
                                                }
                                            }
                                            else if (AActorID == hatchID)
                                            {
                                                Vector2 vScreen_d3d11 = new Vector2(0, 0);
                                                if (Renderer.WorldToScreenUE4(tempVec, out vScreen_d3d11,
                                                    FMinimalViewInfo_Location, FMinimalViewInfo_Rotation,
                                                    FMinimalViewInfo_FOV,
                                                    wndMargins, wndSize))
                                                {
                                                    Renderer.DrawText("HATCH [" + dist + "m]", vScreen_d3d11,
                                                        Components.VisualsComponent.MiscColor.Color, 12,
                                                        TextAlignment.centered, false);
                                                }
                                            }
                                        }

                                        if (Components.VisualsComponent.DrawGenerators.Enabled)
                                        {
                                            if (AActorID != generatorID)
                                                continue;
                                            var isRepaired =
                                                Memory.ZwReadBool(processHandle, (IntPtr) AActor.ToInt64() + 0x03D1);
                                            var isBlocked = Memory.ZwReadBool(processHandle,
                                                (IntPtr) AActor.ToInt64() + 0x0494);

                                            var currentProgressPercent =
                                                Memory.ZwReadFloat(processHandle, (IntPtr) AActor.ToInt64() + 0x03E0) *
                                                100;
                                            Color selectedColor;
                                            if (isBlocked)
                                                selectedColor = Color.Red;
                                            else if (isRepaired)
                                                selectedColor = Color.Green;
                                            else
                                                selectedColor = Color.Yellow;
                                            Vector2 vScreen_d3d11 = new Vector2(0, 0);
                                            if (Renderer.WorldToScreenUE4(tempVec, out vScreen_d3d11,
                                                FMinimalViewInfo_Location, FMinimalViewInfo_Rotation,
                                                FMinimalViewInfo_FOV,
                                                wndMargins, wndSize))
                                            {
                                                Renderer.DrawText(
                                                    $"Generator [{dist}m] ({currentProgressPercent:##0}%)",
                                                    vScreen_d3d11,
                                                    selectedColor, 15, TextAlignment.centered, false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    }
}
