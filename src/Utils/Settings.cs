﻿//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using linerider.Audio;
using linerider.UI;
using linerider.Utils;
using System.Windows.Forms;
using System.Drawing;

namespace linerider
{
    static class Settings
    {
        public enum BezierMode
        {
            Direct = 0,
            Trace = 1
        }
        public static class Recording
        {
            public static bool ShowTools = false;
            public static bool ShowFps = true;
            public static bool ShowPpf = true;
            public static bool ShowHitTest = false;
            public static bool EnableColorTriggers = true;
            public static bool ResIndZoom = true; //Use resolution-independent zoom based on window size when recording
        }
        public static class Local
        {
            public static bool RecordingMode;
            public static float MaxZoom
            {
                get
                {
                    return Settings.SuperZoom ? Constants.MaxSuperZoom : Constants.MaxZoom;
                }
            }
            public static bool TrackOverlay = false;
            public static bool TrackOverlayFixed = false;
            public static int TrackOverlayFixedFrame = 0;
            public static int TrackOverlayOffset = -1;
        }
        public static class Editor
        {
            public static bool HitTest;
            public static bool SnapNewLines;
            public static bool SnapMoveLine;
            public static bool SnapToGrid;
            public static bool ForceXySnap;
            public static float XySnapDegrees;
            public static bool MomentumVectors;
            public static bool RenderGravityWells;
            public static bool DrawContactPoints;
            public static bool LifeLockNoOrange;
            public static bool LifeLockNoFakie;
            public static bool ShowLineLength;
            public static bool ShowLineAngle;
            public static bool ShowLineID;
        }
        public static class Lines
        {
            public static Color DefaultLine;
            public static Color DefaultNightLine;
            public static Color AccelerationLine;
            public static Color SceneryLine;
            public static Color StandardLine;
        }
        public static class Bezier
        {
            public static int Resolution;
            public static int NodeSize;
            public static int Mode;
        }
        public static int PlaybackZoomType;
        public static float PlaybackZoomValue;
        public static float Volume;
        public static bool SuperZoom;
        public static bool WhiteBG;
        public static bool NightMode;
        public static bool SmoothCamera;
        public static bool PredictiveCamera;
        public static bool RoundLegacyCamera;
        public static bool SmoothPlayback;
        public static bool CheckForUpdates;
        public static bool Record1080p;
        public static bool RecordSmooth;
        public static bool RecordMusic;
        public static int RecordingWidth;
        public static int RecordingHeight;
        public static int ScreenshotWidth;
        public static int ScreenshotHeight;
        public static float ScrollSensitivity;
        public static int SettingsPane;
        public static bool MuteAudio;
        public static bool PreviewMode;
        public static int SlowmoSpeed;
        public static float DefaultPlayback;
        public static bool DrawCollisionGrid; //Draw the grid used in line collision detection
        public static bool DrawAGWs; //Draw the normally invisible line extensions used to smooth curve collisions
        public static bool DrawFloatGrid; //Draw the exponential grid of floating-point 'regions' (used for angled kramuals)
        public static bool DrawCamera; //Draw the camera's area

        public static float ZoomMultiplier; //A constant multiplier for the zoom

        //LRTran settings
        public static String SelectedScarf; //What custom scarf is selected
        public static int ScarfSegments; //How many scarf segments on restart
        public static String SelectedBoshSkin; //What bosh skin is selected
        public static bool customScarfOnPng; //To replace colors in the png for a custom scarf
        public static bool discordActivityEnabled; //If the discord activity should be run, dll is still needed
        public static String discordActivity1; //what activities are displayed
        public static String discordActivity2; //what activities are displayed
        public static String discordActivity3; //what activities are displayed
        public static String discordActivity4; //what activities are displayed
        public static String largeImageKey; //What image discord uses
        public static bool showChangelog; //Show the changelog
        public static int multiScarfAmount; //How many scarves the rider has
        public static int multiScarfSegments; //How many segments a multi scarf has
        public static int autosaveChanges; //Changes when autosave starts
        public static int autosaveMinutes; //Amount of minues per autosave
        public static int mainWindowWidth; //Main window Width
        public static int mainWindowHeight; //Main window height
        public static String DefaultSaveFormat; //What the save menu auto picks 
        public static String DefaultAutosaveFormat; //What the autosave format is
        public static String DefaultQuicksaveFormat; //What the autosave format is
        public static String DefaultCrashBackupFormat; //Format crash backups are saved to

        // RatherBeLunar Addon Settings
        public static bool velocityReferenceFrameAnimation = true;
        public static bool recededLinesAsScenery;
        public static bool forwardLinesAsScenery;
        public static float animationRelativeVelX;
        public static float animationRelativeVelY;

        public static bool ColorPlayback;
        public static bool OnionSkinning;
        public static int PastOnionSkins;
        public static int FutureOnionSkins;
        public static string LastSelectedTrack = "";
        public static Dictionary<Hotkey, KeyConflicts> KeybindConflicts = new Dictionary<Hotkey, KeyConflicts>();
        public static Dictionary<Hotkey, List<Keybinding>> Keybinds = new Dictionary<Hotkey, List<Keybinding>>();
        private static Dictionary<Hotkey, List<Keybinding>> DefaultKeybinds = new Dictionary<Hotkey, List<Keybinding>>();

        //Malizma Addon Settings
        public static bool InvisibleRider;

        // if true, recordings start on the frame that is currently being edited (_game.Track.offset from ExportWindow)
        public static bool currentFrame;

        static Settings()
        {
            RestoreDefaultSettings();
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                KeybindConflicts.Add(hk, KeyConflicts.General);
                Keybinds.Add(hk, new List<Keybinding>());
            }
            //conflicts, for keybinds that depend on a state, so keybinds 
            //outside of its state can be set as long
            //as its dependant state (general) doesnt have a keybind set
            KeybindConflicts[Hotkey.PlaybackZoom] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackUnzoom] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackSpeedUp] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackSpeedDown] = KeyConflicts.Playback;

            KeybindConflicts[Hotkey.LineToolFlipLine] = KeyConflicts.LineTool;

            KeybindConflicts[Hotkey.ToolXYSnap] = KeyConflicts.Tool;
            KeybindConflicts[Hotkey.ToolToggleSnap] = KeyConflicts.Tool;
            KeybindConflicts[Hotkey.EditorCancelTool] = KeyConflicts.Tool;

            KeybindConflicts[Hotkey.ToolLengthLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolAngleLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolAxisLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolPerpendicularAxisLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolLifeLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolLengthLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolCopy] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolCut] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolPaste] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolDelete] = KeyConflicts.SelectTool;

            KeybindConflicts[Hotkey.PlayButtonIgnoreFlag] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.EditorCancelTool] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolAddSelection] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolToggleSelection] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolScaleAspectRatio] = KeyConflicts.HardCoded;
            SetupDefaultKeybinds();
        }
        public static void RestoreDefaultSettings()
        {
            Editor.HitTest = false;
            Editor.SnapNewLines = true;
            Editor.SnapMoveLine = true;
            Editor.SnapToGrid = false;
            Editor.ForceXySnap = false;
            Editor.XySnapDegrees = 15;
            Editor.MomentumVectors = false;
            Editor.RenderGravityWells = false;
            Editor.DrawContactPoints = false;
            Editor.LifeLockNoOrange = false;
            Editor.LifeLockNoFakie = false;
            Editor.ShowLineLength = true;
            Editor.ShowLineAngle = true;
            Editor.ShowLineID = false;
            Lines.DefaultLine = Constants.DefaultLineColor;
            Lines.DefaultNightLine = Constants.DefaultNightLineColor;
            Lines.AccelerationLine = Constants.RedLineColor;
            Lines.SceneryLine = Constants.SceneryLineColor;
            Lines.StandardLine = Constants.BlueLineColor;
            Bezier.Resolution = 30;
            Bezier.NodeSize = 15;
            Bezier.Mode = (int) BezierMode.Direct;
            PlaybackZoomType = 0;
            PlaybackZoomValue = 4;
            Volume = 100;
            SuperZoom = false;
            WhiteBG = false;
            NightMode = false;
            SmoothCamera = true;
            PredictiveCamera = false;
            RoundLegacyCamera = true;
            SmoothPlayback = true;
            CheckForUpdates = true;
            Record1080p = false;
            RecordSmooth = true;
            RecordMusic = true;
            RecordingWidth = 1280;
            RecordingHeight = 720;
            ScreenshotWidth = 1280;
            ScreenshotHeight = 720;
            ScrollSensitivity = 1;
            SettingsPane = 0;
            MuteAudio = false;
            PreviewMode = false;
            SlowmoSpeed = 2;
            DefaultPlayback = 1f;
            ColorPlayback = false;
            OnionSkinning = false;
            PastOnionSkins = 10;
            FutureOnionSkins = 20;
            ScarfSegments = 5;
            SelectedScarf = "*default*";
            SelectedBoshSkin = "*default*";
            customScarfOnPng = false;
            discordActivityEnabled = false;
            discordActivity1 = "none";
            discordActivity2 = "none";
            discordActivity3 = "none";
            discordActivity4 = "none";
            largeImageKey = "lrl";
            showChangelog = true;
            multiScarfAmount = 1;
            multiScarfSegments = 5;
            autosaveChanges = 50;
            autosaveMinutes = 5;
            mainWindowWidth = 1280;
            mainWindowHeight = 720;
            DefaultSaveFormat = ".trk";
            DefaultAutosaveFormat = ".trk";
            DefaultQuicksaveFormat = ".trk";
            DefaultCrashBackupFormat = ".trk";
            DrawCollisionGrid = false;
            DrawAGWs = false;
            DrawFloatGrid = false;
            DrawCamera = false;
            ZoomMultiplier = 1.0f;
            InvisibleRider = false;
        }
        public static void ResetKeybindings()
        {
            foreach (var kb in Keybinds)
            {
                kb.Value.Clear();
            }
            LoadDefaultKeybindings();
        }
        private static void SetupDefaultKeybinds()
        {
            SetupAddonDefaultKeybinds();

            SetupDefaultKeybind(Hotkey.EditorPencilTool, new Keybinding(Key.Q));
            SetupDefaultKeybind(Hotkey.EditorLineTool, new Keybinding(Key.W));
            SetupDefaultKeybind(Hotkey.EditorEraserTool, new Keybinding(Key.E));
            SetupDefaultKeybind(Hotkey.EditorSelectTool, new Keybinding(Key.R));
            SetupDefaultKeybind(Hotkey.EditorPanTool, new Keybinding(Key.T));
            SetupDefaultKeybind(Hotkey.EditorToolColor1, new Keybinding(Key.Number1));
            SetupDefaultKeybind(Hotkey.EditorToolColor2, new Keybinding(Key.Number2));
            SetupDefaultKeybind(Hotkey.EditorToolColor3, new Keybinding(Key.Number3));

            SetupDefaultKeybind(Hotkey.EditorCycleToolSetting, new Keybinding(Key.Tab));
            SetupDefaultKeybind(Hotkey.EditorMoveStart, new Keybinding(Key.D));

            SetupDefaultKeybind(Hotkey.EditorRemoveLatestLine, new Keybinding(Key.BackSpace));
            SetupDefaultKeybind(Hotkey.EditorFocusStart, new Keybinding(Key.Home));
            SetupDefaultKeybind(Hotkey.EditorFocusLastLine, new Keybinding(Key.End));
            SetupDefaultKeybind(Hotkey.EditorFocusRider, new Keybinding(Key.F1));
            SetupDefaultKeybind(Hotkey.EditorFocusFlag, new Keybinding(Key.F2));
            SetupDefaultKeybind(Hotkey.ToolLifeLock, new Keybinding(KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.ToolAngleLock, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolAxisLock, new Keybinding(KeyModifiers.Control | KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolPerpendicularAxisLock, new Keybinding(Key.X, KeyModifiers.Control | KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolLengthLock, new Keybinding(Key.L));
            SetupDefaultKeybind(Hotkey.ToolXYSnap, new Keybinding(Key.X));
            SetupDefaultKeybind(Hotkey.ToolToggleSnap, new Keybinding(Key.S));
            SetupDefaultKeybind(Hotkey.ToolSelectBothJoints, new Keybinding(KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.LineToolFlipLine, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.EditorUndo, new Keybinding(Key.Z, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.EditorRedo,
                new Keybinding(Key.Y, KeyModifiers.Control),
                new Keybinding(Key.Z, KeyModifiers.Control | KeyModifiers.Shift));

            SetupDefaultKeybind(Hotkey.PlaybackStartIgnoreFlag, new Keybinding(Key.Y, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackStartGhostFlag, new Keybinding(Key.I, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackStartSlowmo, new Keybinding(Key.Y, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackFlag, new Keybinding(Key.I));
            SetupDefaultKeybind(Hotkey.PlaybackStart, new Keybinding(Key.Y));
            SetupDefaultKeybind(Hotkey.PlaybackStop, new Keybinding(Key.U));
            SetupDefaultKeybind(Hotkey.PlaybackSlowmo, new Keybinding(Key.M));
            SetupDefaultKeybind(Hotkey.PlaybackZoom, new Keybinding(Key.Z));
            SetupDefaultKeybind(Hotkey.PlaybackUnzoom, new Keybinding(Key.X));

            SetupDefaultKeybind(Hotkey.PlaybackSpeedUp,
                new Keybinding(Key.Plus),
                new Keybinding(Key.KeypadPlus));

            SetupDefaultKeybind(Hotkey.PlaybackSpeedDown,
                new Keybinding(Key.Minus),
                new Keybinding(Key.KeypadMinus));

            SetupDefaultKeybind(Hotkey.PlaybackFrameNext, new Keybinding(Key.Right));
            SetupDefaultKeybind(Hotkey.PlaybackFramePrev, new Keybinding(Key.Left));
            SetupDefaultKeybind(Hotkey.PlaybackForward, new Keybinding(Key.Right, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackBackward, new Keybinding(Key.Left, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackIterationNext, new Keybinding(Key.Right, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackIterationPrev, new Keybinding(Key.Left, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackTogglePause, new Keybinding(Key.Space));

            SetupDefaultKeybind(Hotkey.PreferencesWindow,
                new Keybinding(Key.P, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.GameMenuWindow, new Keybinding(Key.Escape));
            SetupDefaultKeybind(Hotkey.TrackPropertiesWindow, new Keybinding(Key.T, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.PreferenceAllCheckboxSettings, new Keybinding(Key.O, KeyModifiers.Shift | KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.InvisibleRider, new Keybinding(Key.I, KeyModifiers.Shift | KeyModifiers.Alt));

            SetupDefaultKeybind(Hotkey.PreferenceOnionSkinning, new Keybinding(Key.O, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.LoadWindow, new Keybinding(Key.O));
            SetupDefaultKeybind(Hotkey.Quicksave, new Keybinding(Key.S, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.PlayButtonIgnoreFlag, new Keybinding(KeyModifiers.Alt));

            SetupDefaultKeybind(Hotkey.EditorQuickPan, new Keybinding(Key.Space, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.EditorDragCanvas, new Keybinding(MouseButton.Middle));

            SetupDefaultKeybind(Hotkey.EditorCancelTool, new Keybinding(Key.Escape));
            SetupDefaultKeybind(Hotkey.PlayButtonIgnoreFlag, new Keybinding(KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackResetCamera, new Keybinding(Key.N));
            SetupDefaultKeybind(Hotkey.ToolCopy, new Keybinding(Key.C, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolCut, new Keybinding(Key.X, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolPaste, new Keybinding(Key.V, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolDelete, new Keybinding(Key.Delete));
            SetupDefaultKeybind(Hotkey.ToolAddSelection, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolToggleSelection, new Keybinding(KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.ToolScaleAspectRatio, new Keybinding(KeyModifiers.Shift));

            SetupDefaultKeybind(Hotkey.ToolToggleOverlay, new Keybinding(Key.V));

            SetupDefaultKeybind(Hotkey.TriggerMenuWindow, new Keybinding(Key.P));
            SetupDefaultKeybind(Hotkey.SaveAsWindow, new Keybinding(Key.S, KeyModifiers.Control | KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.LineGeneratorWindow, new Keybinding(Key.G));
            SetupDefaultKeybind(Hotkey.DrawDebugCamera, new Keybinding(Key.Period));
            SetupDefaultKeybind(Hotkey.DrawDebugGrid, new Keybinding(Key.Comma));
        }
        private static void SetupAddonDefaultKeybinds()
        {
            SetupDefaultKeybind(Hotkey.MagicAnimateAdvanceFrame, new Keybinding(Key.Keypad0));
            SetupDefaultKeybind(Hotkey.MagicAnimateRecedeFrame, new Keybinding(Key.Keypad1));
            SetupDefaultKeybind(Hotkey.MagicAnimateRecedeMultiFrame, new Keybinding(Key.Keypad2));

            SetupDefaultKeybind(Hotkey.LineGeneratorWindow, new Keybinding(Key.G));
        }
        private static void SetupDefaultKeybind(Hotkey hotkey, Keybinding keybinding, Keybinding secondary = null)
        {
            if (keybinding.IsEmpty)
                return;
            DefaultKeybinds[hotkey] = new List<Keybinding>();
            DefaultKeybinds[hotkey].Add(keybinding);
            if (secondary != null)
            {
                DefaultKeybinds[hotkey].Add(secondary);
            }
        }
        private static void LoadDefaultKeybindings()
        {
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                LoadDefaultKeybind(hk);
            }
        }
        public static List<Keybinding> GetHotkeyDefault(Hotkey hotkey)
        {
            if (!DefaultKeybinds.ContainsKey(hotkey))
                return null;
            return DefaultKeybinds[hotkey];
        }
        private static void LoadDefaultKeybind(Hotkey hotkey)
        {
            if (DefaultKeybinds.ContainsKey(hotkey))
            {
                var defaults = DefaultKeybinds[hotkey];
                if (defaults == null || defaults.Count == 0)
                    return;
                var list = Keybinds[hotkey];
                if (list.Count == 0)
                    CreateKeybind(hotkey, defaults[0]);
                if (defaults.Count > 1)
                {
                    var secondary = defaults[1];
                    if (secondary != null && list.Count == 1 && list[0].IsBindingEqual(defaults[0]))
                        CreateKeybind(hotkey, secondary);
                }
            }
        }
        private static void CreateKeybind(Hotkey hotkey, Keybinding keybinding)
        {
            var conflict = CheckConflicts(keybinding, hotkey);
            if (keybinding.IsEmpty || conflict != Hotkey.None)
                return;
            Keybinds[hotkey].Add(keybinding);
        }
        public static Hotkey CheckConflicts(Keybinding keybinding, Hotkey hotkey)
        {
            if (!keybinding.IsEmpty)
            {
                var inputconflicts = Settings.KeybindConflicts[hotkey];
                if (inputconflicts == KeyConflicts.HardCoded)
                    return Hotkey.None;
                foreach (var keybinds in Settings.Keybinds)
                {
                    var hk = keybinds.Key;
                    var conflicts = Settings.KeybindConflicts[hk];
                    //if the conflicts is equal to or below inputconflicts
                    //then we can compare for conflict
                    //if conflicts is above inputconflicts, ignore
                    //also, if theyre both hardcoded they cannot conflict.
                    if (inputconflicts.HasFlag(conflicts))
                    {
                        foreach (var keybind in keybinds.Value)
                        {
                            if (keybind.IsBindingEqual(keybinding) &&
                                !(inputconflicts == KeyConflicts.HardCoded &&
                                  inputconflicts == conflicts))
                                return hk;
                        }
                    }
                }
            }
            return Hotkey.None;
        }
        public static void Load()
        {
            string[] lines = null;
            try
            {
                if (!File.Exists(Program.UserDirectory + "settings-LRT.conf"))
                {
                    Save();
                }
                lines = File.ReadAllLines(Program.UserDirectory + "settings-LRT.conf");
            }
            catch
            {
            }
            LoadInt(GetSetting(lines, nameof(PlaybackZoomType)), ref PlaybackZoomType);
            LoadFloat(GetSetting(lines, nameof(PlaybackZoomValue)), ref PlaybackZoomValue);
            LoadFloat(GetSetting(lines, nameof(Volume)), ref Volume);
            LoadFloat(GetSetting(lines, nameof(ScrollSensitivity)), ref ScrollSensitivity);
            LoadBool(GetSetting(lines, nameof(SuperZoom)), ref SuperZoom);
            LoadBool(GetSetting(lines, nameof(WhiteBG)), ref WhiteBG);
            LoadBool(GetSetting(lines, nameof(NightMode)), ref NightMode);
            LoadBool(GetSetting(lines, nameof(SmoothCamera)), ref SmoothCamera);
            LoadBool(GetSetting(lines, nameof(PredictiveCamera)), ref PredictiveCamera);
            LoadBool(GetSetting(lines, nameof(CheckForUpdates)), ref CheckForUpdates);
            LoadBool(GetSetting(lines, nameof(SmoothPlayback)), ref SmoothPlayback);
            LoadBool(GetSetting(lines, nameof(RoundLegacyCamera)), ref RoundLegacyCamera);
            LoadBool(GetSetting(lines, nameof(Record1080p)), ref Record1080p);
            LoadBool(GetSetting(lines, nameof(RecordSmooth)), ref RecordSmooth);
            LoadBool(GetSetting(lines, nameof(RecordMusic)), ref RecordMusic);
            LoadInt(GetSetting(lines, nameof(RecordingWidth)), ref RecordingWidth);
            LoadInt(GetSetting(lines, nameof(RecordingHeight)), ref RecordingHeight);
            LoadInt(GetSetting(lines, nameof(ScreenshotWidth)), ref ScreenshotWidth);
            LoadInt(GetSetting(lines, nameof(ScreenshotHeight)), ref ScreenshotHeight);
            LoadBool(GetSetting(lines, nameof(Editor.LifeLockNoFakie)), ref Editor.LifeLockNoFakie);
            LoadBool(GetSetting(lines, nameof(Editor.LifeLockNoOrange)), ref Editor.LifeLockNoOrange);
            LoadInt(GetSetting(lines, nameof(SettingsPane)), ref SettingsPane);
            LoadBool(GetSetting(lines, nameof(MuteAudio)), ref MuteAudio);
            LoadBool(GetSetting(lines, nameof(Editor.HitTest)), ref Editor.HitTest);
            LoadBool(GetSetting(lines, nameof(Editor.SnapNewLines)), ref Editor.SnapNewLines);
            LoadBool(GetSetting(lines, nameof(Editor.SnapMoveLine)), ref Editor.SnapMoveLine);
            LoadBool(GetSetting(lines, nameof(Editor.SnapToGrid)), ref Editor.SnapToGrid);
            LoadBool(GetSetting(lines, nameof(Editor.ForceXySnap)), ref Editor.ForceXySnap);
            LoadFloat(GetSetting(lines, nameof(Editor.XySnapDegrees)), ref Editor.XySnapDegrees);
            LoadBool(GetSetting(lines, nameof(Editor.MomentumVectors)), ref Editor.MomentumVectors);
            LoadBool(GetSetting(lines, nameof(Editor.RenderGravityWells)), ref Editor.RenderGravityWells);
            LoadBool(GetSetting(lines, nameof(Editor.DrawContactPoints)), ref Editor.DrawContactPoints);
            LoadBool(GetSetting(lines, nameof(PreviewMode)), ref PreviewMode);
            LoadInt(GetSetting(lines, nameof(SlowmoSpeed)), ref SlowmoSpeed);
            LoadFloat(GetSetting(lines, nameof(DefaultPlayback)), ref DefaultPlayback);
            LoadBool(GetSetting(lines, nameof(ColorPlayback)), ref ColorPlayback);
            LoadBool(GetSetting(lines, nameof(OnionSkinning)), ref OnionSkinning);
            LoadInt(GetSetting(lines, nameof(PastOnionSkins)), ref PastOnionSkins);
            LoadInt(GetSetting(lines, nameof(FutureOnionSkins)), ref FutureOnionSkins);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineLength)), ref Editor.ShowLineLength);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineAngle)), ref Editor.ShowLineAngle);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineID)), ref Editor.ShowLineID);
            SelectedScarf = GetSetting(lines, nameof(SelectedScarf));
            LoadInt(GetSetting(lines, nameof(ScarfSegments)), ref ScarfSegments);
            SelectedBoshSkin = GetSetting(lines, nameof(SelectedBoshSkin));
            LoadBool(GetSetting(lines, nameof(customScarfOnPng)), ref customScarfOnPng);
            LoadBool(GetSetting(lines, nameof(discordActivityEnabled)), ref discordActivityEnabled);
            discordActivity1 = GetSetting(lines, nameof(discordActivity1));
            discordActivity2 = GetSetting(lines, nameof(discordActivity2));
            discordActivity3 = GetSetting(lines, nameof(discordActivity3));
            discordActivity4 = GetSetting(lines, nameof(discordActivity4));
            largeImageKey = GetSetting(lines, nameof(largeImageKey));
            LoadBool(GetSetting(lines, nameof(showChangelog)), ref showChangelog);
            LoadInt(GetSetting(lines, nameof(multiScarfSegments)), ref multiScarfSegments);
            LoadInt(GetSetting(lines, nameof(multiScarfAmount)), ref multiScarfAmount);
            LoadInt(GetSetting(lines, nameof(autosaveMinutes)), ref autosaveMinutes);
            LoadInt(GetSetting(lines, nameof(autosaveChanges)), ref autosaveChanges);
            LoadInt(GetSetting(lines, nameof(mainWindowWidth)), ref mainWindowWidth);
            LoadInt(GetSetting(lines, nameof(mainWindowHeight)), ref mainWindowHeight);
            DefaultSaveFormat = GetSetting(lines, nameof(DefaultSaveFormat));
            DefaultAutosaveFormat = GetSetting(lines, nameof(DefaultAutosaveFormat));
            DefaultQuicksaveFormat = GetSetting(lines, nameof(DefaultQuicksaveFormat));
            DefaultCrashBackupFormat = GetSetting(lines, nameof(DefaultCrashBackupFormat));
            LoadBool(GetSetting(lines, nameof(DrawCollisionGrid)), ref DrawCollisionGrid);
            LoadBool(GetSetting(lines, nameof(DrawAGWs)), ref DrawAGWs);
            LoadBool(GetSetting(lines, nameof(DrawFloatGrid)), ref DrawFloatGrid);
            LoadBool(GetSetting(lines, nameof(DrawCamera)), ref DrawCamera);
            LoadFloat(GetSetting(lines, nameof(ZoomMultiplier)), ref ZoomMultiplier);
            LoadColor(GetSetting(lines, nameof(Lines.DefaultLine)), ref Lines.DefaultLine);
            LoadColor(GetSetting(lines, nameof(Lines.DefaultNightLine)), ref Lines.DefaultNightLine);
            LoadColor(GetSetting(lines, nameof(Lines.AccelerationLine)), ref Lines.AccelerationLine);
            LoadColor(GetSetting(lines, nameof(Lines.SceneryLine)), ref Lines.SceneryLine);
            LoadColor(GetSetting(lines, nameof(Lines.StandardLine)), ref Lines.StandardLine);
            LoadInt(GetSetting(lines, nameof(Bezier.Resolution)), ref Bezier.Resolution);
            LoadInt(GetSetting(lines, nameof(Bezier.NodeSize)), ref Bezier.NodeSize);
            LoadInt(GetSetting(lines, nameof(Bezier.Mode)), ref Bezier.Mode);
            LoadBool(GetSetting(lines, nameof(InvisibleRider)), ref InvisibleRider);
            if (multiScarfSegments == 0) { multiScarfSegments++; }
            if (ScarfSegments == 0) { ScarfSegments++; }
            LoadAddonSettings(lines);


            var lasttrack = GetSetting(lines, nameof(LastSelectedTrack));
            if (File.Exists(lasttrack) && lasttrack.StartsWith(Constants.TracksDirectory))
            {
                LastSelectedTrack = lasttrack;
            }
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                LoadKeybinding(lines, hk);
            }

            Volume = MathHelper.Clamp(Settings.Volume, 0, 100);
            LoadDefaultKeybindings();
        }
        public static void LoadAddonSettings(string[] lines)
        {
            LoadBool(GetSetting(lines, nameof(velocityReferenceFrameAnimation)), ref velocityReferenceFrameAnimation);
            LoadBool(GetSetting(lines, nameof(forwardLinesAsScenery)), ref forwardLinesAsScenery);
            LoadBool(GetSetting(lines, nameof(recededLinesAsScenery)), ref recededLinesAsScenery);
            LoadFloat(GetSetting(lines, nameof(animationRelativeVelX)), ref animationRelativeVelX);
            LoadFloat(GetSetting(lines, nameof(animationRelativeVelY)), ref animationRelativeVelY);
        }

        public static void Save()
        {
            string config = MakeSetting(nameof(LastSelectedTrack), LastSelectedTrack);
            config += "\r\n" + MakeSetting(nameof(Volume), Volume.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SuperZoom), SuperZoom.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(WhiteBG), WhiteBG.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(NightMode), NightMode.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SmoothCamera), SmoothCamera.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PredictiveCamera), PredictiveCamera.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(CheckForUpdates), CheckForUpdates.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SmoothPlayback), SmoothPlayback.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PlaybackZoomType), PlaybackZoomType.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PlaybackZoomValue), PlaybackZoomValue.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(RoundLegacyCamera), RoundLegacyCamera.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Record1080p), Record1080p.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(RecordSmooth), RecordSmooth.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(RecordMusic), RecordMusic.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(RecordingWidth), RecordingWidth.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(RecordingHeight), RecordingHeight.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ScreenshotWidth), ScreenshotWidth.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ScreenshotHeight), ScreenshotHeight.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ScrollSensitivity), ScrollSensitivity.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.LifeLockNoFakie), Editor.LifeLockNoFakie.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.LifeLockNoOrange), Editor.LifeLockNoOrange.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SettingsPane), SettingsPane.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(MuteAudio), MuteAudio.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.HitTest), Editor.HitTest.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.SnapNewLines), Editor.SnapNewLines.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.SnapMoveLine), Editor.SnapMoveLine.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.SnapToGrid), Editor.SnapToGrid.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.ForceXySnap), Editor.ForceXySnap.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.XySnapDegrees), Editor.XySnapDegrees.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.MomentumVectors), Editor.MomentumVectors.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.RenderGravityWells), Editor.RenderGravityWells.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.DrawContactPoints), Editor.DrawContactPoints.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PreviewMode), PreviewMode.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SlowmoSpeed), SlowmoSpeed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(DefaultPlayback), DefaultPlayback.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ColorPlayback), ColorPlayback.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(OnionSkinning), OnionSkinning.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PastOnionSkins), PastOnionSkins.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(FutureOnionSkins), FutureOnionSkins.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.ShowLineAngle), Editor.ShowLineAngle.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.ShowLineLength), Editor.ShowLineLength.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.ShowLineID), Editor.ShowLineID.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SelectedScarf), SelectedScarf);
            config += "\r\n" + MakeSetting(nameof(ScarfSegments), ScarfSegments.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SelectedBoshSkin), SelectedBoshSkin);
            config += "\r\n" + MakeSetting(nameof(customScarfOnPng), customScarfOnPng.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(discordActivityEnabled), discordActivityEnabled.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(discordActivity1), discordActivity1);
            config += "\r\n" + MakeSetting(nameof(discordActivity2), discordActivity2);
            config += "\r\n" + MakeSetting(nameof(discordActivity3), discordActivity3);
            config += "\r\n" + MakeSetting(nameof(discordActivity4), discordActivity4);
            config += "\r\n" + MakeSetting(nameof(largeImageKey), largeImageKey);
            config += "\r\n" + MakeSetting(nameof(showChangelog), showChangelog.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(multiScarfSegments), multiScarfSegments.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(multiScarfAmount), multiScarfAmount.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(autosaveChanges), autosaveChanges.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(autosaveMinutes), autosaveMinutes.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(mainWindowWidth), mainWindowWidth.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(mainWindowHeight), mainWindowHeight.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(DefaultSaveFormat), DefaultSaveFormat);
            config += "\r\n" + MakeSetting(nameof(DefaultAutosaveFormat), DefaultAutosaveFormat);
            config += "\r\n" + MakeSetting(nameof(DefaultQuicksaveFormat), DefaultQuicksaveFormat);
            config += "\r\n" + MakeSetting(nameof(DefaultCrashBackupFormat), DefaultCrashBackupFormat);
            config += "\r\n" + MakeSetting(nameof(DrawCollisionGrid), DrawCollisionGrid.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(DrawAGWs), DrawAGWs.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(DrawFloatGrid), DrawFloatGrid.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(DrawCamera), DrawCamera.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ZoomMultiplier), ZoomMultiplier.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Lines.DefaultLine), SaveColor(Lines.DefaultLine));
            config += "\r\n" + MakeSetting(nameof(Lines.DefaultNightLine), SaveColor(Lines.DefaultNightLine));
            config += "\r\n" + MakeSetting(nameof(Lines.AccelerationLine), SaveColor(Lines.AccelerationLine));
            config += "\r\n" + MakeSetting(nameof(Lines.SceneryLine), SaveColor(Lines.SceneryLine));
            config += "\r\n" + MakeSetting(nameof(Lines.StandardLine), SaveColor(Lines.StandardLine));
            config += "\r\n" + MakeSetting(nameof(Bezier.Resolution), Bezier.Resolution.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Bezier.NodeSize), Bezier.NodeSize.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Bezier.Mode), Bezier.Mode.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(InvisibleRider), InvisibleRider.ToString(Program.Culture));
            config = SaveAddonSettings(config);
            foreach (var binds in Keybinds)
            {
                foreach (var bind in binds.Value)
                {
                    if (KeybindConflicts[binds.Key] == KeyConflicts.HardCoded)
                        continue;
                    if (!bind.IsEmpty)
                    {
                        string keybind = "";
                        if (bind.UsesModifiers)
                            keybind += bind.Modifiers.ToString();
                        if (bind.UsesKeys)
                        {
                            if (keybind.Length > 0)
                                keybind += "+";
                            keybind += bind.Key.ToString();
                        }
                        if (bind.UsesMouse)
                        {
                            if (keybind.Length > 0)
                                keybind += "+";
                            keybind += bind.MouseButton.ToString();
                        }
                        config += "\r\n" +
                            MakeSetting(binds.Key.ToString(), $"[{keybind}]");
                    }
                }
            }
            try
            {
                File.WriteAllText(Program.UserDirectory + "settings-LRT.conf", config);
            }
            catch { }
        }
        private static string SaveAddonSettings(string config)
        {
            config += "\r\n" + MakeSetting(nameof(velocityReferenceFrameAnimation), velocityReferenceFrameAnimation.ToString());
            config += "\r\n" + MakeSetting(nameof(forwardLinesAsScenery), forwardLinesAsScenery.ToString());
            config += "\r\n" + MakeSetting(nameof(recededLinesAsScenery), recededLinesAsScenery.ToString());
            config += "\r\n" + MakeSetting(nameof(animationRelativeVelX), animationRelativeVelX.ToString());
            config += "\r\n" + MakeSetting(nameof(animationRelativeVelY), animationRelativeVelY.ToString());
            return config;
        }
        private static void LoadKeybinding(string[] config, Hotkey hotkey)
        {
            if (KeybindConflicts[hotkey] == KeyConflicts.HardCoded)
                return;
            int line = 0;
            var hotkeyname = hotkey.ToString();
            var setting = GetSetting(config, hotkeyname, ref line);
            if (setting != null)
                Keybinds[hotkey] = new List<Keybinding>();
            while (setting != null)
            {
                line++;
                var items = setting.Trim(' ', '\t', '[', ']').Split('+');
                Keybinding ret = new Keybinding();
                foreach (var item in items)
                {
                    if (!ret.UsesModifiers &&
                        Enum.TryParse<KeyModifiers>(item, true, out var modifiers))
                    {
                        ret.Modifiers = modifiers;
                    }
                    else if (!ret.UsesKeys &&
                        Enum.TryParse<Key>(item, true, out Key key))
                    {
                        ret.Key = key;
                    }
                    else if (!ret.UsesMouse &&
                        Enum.TryParse<MouseButton>(item, true, out var mouse))
                    {
                        ret.MouseButton = mouse;
                    }
                }

                try
                {
                    if (!ret.IsEmpty)
                        CreateKeybind(hotkey, ret);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"An error occured loading the hotkey {hotkey}\n{e}");
                }
                setting = GetSetting(config, hotkeyname, ref line);
            }

        }
        private static string GetSetting(string[] config, string name)
        {
            int start = 0;
            return GetSetting(config, name, ref start);
        }
        private static string GetSetting(string[] config, string name, ref int start)
        {
            for (int i = start; i < config.Length; i++)
            {
                var idx = config[i].IndexOf("=");
                if (idx != -1 && idx + 1 < config[i].Length && config[i].Substring(0, idx) == name)//split[0] == name && split.Length > 1)
                {

                    var split = config[i].Substring(idx + 1);
                    start = i;
                    return split;
                }
            }
            return null;
        }
        private static string MakeSetting(string name, string value)
        {
            return name + "=" + value;
        }
        private static void LoadInt(string setting, ref int var)
        {
            int val;
            if (int.TryParse(setting, System.Globalization.NumberStyles.Integer, Program.Culture, out val))
                var = val;
        }
        private static void LoadFloat(string setting, ref float var)
        {
            float val;
            if (float.TryParse(setting, System.Globalization.NumberStyles.Float, Program.Culture, out val))
                var = val;
        }
        private static void LoadBool(string setting, ref bool var)
        {
            bool val;
            if (bool.TryParse(setting, out val))
                var = val;
        }
        private static void LoadColor(string setting, ref Color var)
        {
            if (setting != null)
            {
                int[] vals = setting.Split(',').Select(int.Parse).ToArray();
                var = Color.FromArgb(vals[0], vals[1], vals[2]);
            }
        }

        private static string SaveColor(Color color)
        {
            int[] colorValues = { color.R, color.G, color.B };
            return String.Join(",", colorValues);
        }
    }
}