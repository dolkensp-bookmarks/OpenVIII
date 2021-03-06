﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    //issues found.
    //558 //color looks off on glow. with purple around it.
    //267 //text showing with white background.
    //132 missing lava.
    //pupu states that there is are 2 widths of the texture for Type1 and Type2
    //we are only using 1 so might be reading the wrong pixels somewhere.
    public static partial class Module
    {
        #region Fields
        public static Background Background;
        public static WalkMesh WalkMesh;
        private static EventEngine eventEngine;
        private static Field_mods mod = 0;

        //private static Texture2D tex;
        //private static Texture2D texOverlap;
        private static IServices services;
        private static INF inf;
        private static TDW tdw;
        private static MSK msk;

        #endregion Fields

        #region Enums

        [Flags]
        public enum _Toggles : byte
        {
            DumpingData = 0x1,
            ClassicSpriteBatch = 0x2,
            Quad = 0x4,
            WalkMesh = 0x8,
            Deswizzle = 0x10,
            Perspective = 0x20,
            Menu = 0x40,
        }

        public enum Field_mods
        {
            INIT,
            DEBUGRENDER,
            DISABLED,
            NOJSM
        };
        
        public static _Toggles Flip(this _Toggles flagged, _Toggles flag)
            => flagged ^= flag;
        #endregion Enums

        #region Properties

        public static Cameras Cameras { get; private set; }
        public static FieldMenu FieldMenu { get; private set; }
        public static _Toggles Toggles { get; set; } = _Toggles.Quad | _Toggles.Menu;
        public static Field_mods Mod { get => mod; }

        #endregion Properties

        #region Methods

        public static void Draw()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            switch (mod)
            {
                case Field_mods.INIT:
                    break; //null
                default:
                    DrawDebug();
                    break;

                case Field_mods.DISABLED:
                    FieldMenu.Draw();
                    break;
            }
        }

        public static string GetFieldName()
        {
            string fieldname = Memory.FieldHolder.fields[Memory.FieldHolder.FieldID].ToLower();
            if (string.IsNullOrWhiteSpace(fieldname))
                fieldname = $"unk{Memory.FieldHolder.FieldID}";
            return fieldname;
        }

        public static string GetFolder(string fieldname = null,string subfolder = "")
        {
            if (string.IsNullOrWhiteSpace(fieldname))
                fieldname = GetFieldName();
            string folder = Path.Combine(Path.GetTempPath(), "Fields", fieldname.Substring(0, 2), fieldname,subfolder);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static void ResetField()
        {
            Memory.SuppressDraw = true;
            mod = Field_mods.INIT;
        }

        public static void Update()
        {
            // lets you move through all the feilds just holding left or right. it will just loop when it runs out.
            //if (false && Input2.DelayedButton(FF8TextTagKey.Left))
            //{
            //    init_debugger_Audio.PlaySound(0);
            //    if (Memory.FieldHolder.FieldID > 0)
            //        Memory.FieldHolder.FieldID--;
            //    else
            //        Memory.FieldHolder.FieldID = checked((ushort)(Memory.FieldHolder.fields.Length - 1));
            //    ResetField();
            //}
            //else if (false && Input2.DelayedButton(FF8TextTagKey.Right))
            //{
            //    init_debugger_Audio.PlaySound(0);
            //    if (Memory.FieldHolder.FieldID < checked((ushort)(Memory.FieldHolder.fields.Length - 1)))
            //        Memory.FieldHolder.FieldID++;
            //    else
            //        Memory.FieldHolder.FieldID = 0;
            //    ResetField();
            //}
            if (Input2.DelayedButton(Keys.D0))
                Toggles = Toggles.Flip(_Toggles.Menu);
            else
            {
                switch (mod)
                {
                    case Field_mods.INIT:
                        Init();
                        break;

                    case Field_mods.DEBUGRENDER:
                        UpdateScript();
                        Background.Update();
                        if (Toggles.HasFlag(_Toggles.Menu))
                            FieldMenu.Update();
                        break; //await events here
                    case Field_mods.NOJSM://no scripts but has background.
                        Background.Update();
                        if (Toggles.HasFlag(_Toggles.Menu))
                            FieldMenu.Update();
                        break; //await events here
                    case Field_mods.DISABLED:
                        FieldMenu.Update();
                        break;
                }
            }
        }

        private static void DrawDebug()
        {

            Background.Draw();

            //Memory.SpriteBatchStartAlpha();
            //Memory.font.RenderBasicText($"FieldID: {GetFieldID()} - {GetFieldName().ToUpper()}" +
            //    $"\n4-Bit: {Background.Is4Bit}" +
            //    $"\nadd: {Background.IsAddBlendMode}" +
            //    $"\n1/2 add: {Background.IsHalfBlendMode}" +
            //    $"\n1/4 add: {Background.IsQuarterBlendMode}" +
            //    $"\nsubtract: {Background.IsSubtractBlendMode}", new Point(20, 20), new Vector2(3f));
            //Memory.SpriteBatchEnd();
            if(Toggles.HasFlag(_Toggles.Menu))
            FieldMenu.Draw();
        }

        private static void DrawEntities() => throw new NotImplementedException();

        private static ushort GetFieldID() => Memory.FieldHolder.FieldID;

        private static void Init()
        {
            Memory.SuppressDraw = true;
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_FIELD);
            string[] test = aw.GetListOfFiles();
            FieldMenu = FieldMenu.Create();
            //TODO fix endless look on FieldID 50.
            if (Memory.FieldHolder.FieldID >= (Memory.FieldHolder.fields?.Length ?? 0)||
                Memory.FieldHolder.FieldID < 0)
                goto end;
            string fieldArchiveName = test.FirstOrDefault(x => x.IndexOf(Memory.FieldHolder.GetString(), StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(fieldArchiveName))
            {
                Debug.WriteLine($"FileNotFound :: {Memory.FieldHolder.FieldID} - {Memory.FieldHolder.GetString().ToUpper()}");
                mod = Field_mods.DISABLED;
                goto end;
            }

            ArchiveBase fieldArchive = aw.GetArchive(fieldArchiveName);
            string[] filelist = fieldArchive.GetListOfFiles();
            string findstr(string s) =>
                filelist.FirstOrDefault(x => x.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);

            byte[] getfile(string s)
            {
                s = findstr(s);
                if (!string.IsNullOrWhiteSpace(s))
                    return fieldArchive.GetBinaryFile(s);
                else
                    return null;
            }
            string s_jsm = findstr(".jsm");
            string s_sy = findstr(".sy");
            if ((Background = Background.Load(getfile(".mim"), getfile(".map"))) == null)
            {
                mod = Field_mods.DISABLED;
                goto end;
            }
            Cameras = Cameras.Load(getfile(".ca"));
            WalkMesh = WalkMesh.Load(getfile(".id"));
            //let's start with scripts
            List<Scripts.Jsm.GameObject> jsmObjects;

            if (!string.IsNullOrWhiteSpace(s_jsm))
            {
                try
                {
                    jsmObjects = Scripts.Jsm.File.Read(fieldArchive.GetBinaryFile(s_jsm));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    mod = Field_mods.NOJSM;
                    goto end;
                }

                Sym.GameObjects symObjects;
                if (!string.IsNullOrWhiteSpace(s_sy))
                {
                    symObjects = Sym.Reader.FromBytes(fieldArchive.GetBinaryFile(s_sy));
                }
                else
                {
                    Debug.WriteLine($"FileNotFound :: {Memory.FieldHolder.GetString().ToUpper()}.sy");
                    mod = Field_mods.NOJSM;
                    goto end;
                }
                services = Initializer.GetServices();
                eventEngine = ServiceId.Field[services].Engine;
                eventEngine.Reset();
                for (int objIndex = 0; objIndex < jsmObjects.Count; objIndex++)
                {
                    Scripts.Jsm.GameObject obj = jsmObjects[objIndex];
                    FieldObject fieldObject = new FieldObject(obj.Id, symObjects.GetObjectOrDefault(objIndex).Name);

                    foreach (Scripts.Jsm.GameScript script in obj.Scripts)
                        fieldObject.Scripts.Add(script.ScriptId, script.Segment.GetExecuter());

                    eventEngine.RegisterObject(fieldObject);
                }
            }
            else
            {
                mod = Field_mods.NOJSM;
                goto end;
            }

            //byte[] mchb = getfile(".mch");//Field character models
            //byte[] oneb = getfile(".one");//Field character models container
            //byte[] msdb = getfile(".msd");//dialogs
            byte[] infb = getfile(".inf");//gateways
            inf = INF.Load(infb);
            //byte[] idb = getfile(".id");//walkmesh
            //byte[] cab = getfile(".ca");//camera
            byte[] tdwb = getfile(".tdw");//extra font
            tdw = new TDW(tdwb);
            byte[] mskb = getfile(".msk");//movie cam
            msk = new MSK(mskb);
            //byte[] ratb = getfile(".rat");//battle on field
            //byte[] pmdb = getfile(".pmd");//particle info
            //byte[] sfxb = getfile(".sfx");//sound effects
            
            mod++;
            end:
            FieldMenu.Refresh();
            return;
        }
        private static void UpdateScript()
        {
            //We do not know every instruction and it's not possible for now to play field with unknown instruction
            //eventEngine.Update(services);
        }

        #endregion Methods

        ///// <summary>
        ///// Blend the colors depending on tile.blendmode
        ///// </summary>
        ///// <param name="finalImageColor"></param>
        ///// <param name="color"></param>
        ///// <param name="tile"></param>
        ///// <returns>Color</returns>
        ///// <see cref="http://www.raphnet.net/electronique/psx_adaptor/Playstation.txt"/>
        ///// <seealso cref="http://www.psxdev.net/forum/viewtopic.php?t=953"/>
        ///// <seealso cref="//http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_MAP"/>
        //private static Color BlendColors(ref Color finalImageColor, Color color, Tile tile)
        //{
        //    //• Semi Transparency
        //    //When semi transparency is set for a pixel, the GPU first reads the pixel it wants to write to, and then calculates
        //    //the color it will write from the 2 pixels according to the semi - transparency mode selected.Processing speed is lower
        //    //in this mode because additional reading and calculating are necessary.There are 4 semi - transparency modes in the
        //    //GPU.
        //    //B = the pixel read from the image in the frame buffer, F = the half transparent pixel
        //    //• 1.0 x B + 0.5 x F
        //    //• 1.0 x B + 1.0 x F
        //    //• 1.0 x B - 1.0 x F
        //    //• 1.0 x B + 0.25 x F
        //    //color must not be black
        //    Color baseColor = finalImageColor;
        //    //BlendState blendmode1 = new BlendState
        //    //{
        //    //    ColorSourceBlend = Blend.SourceColor,
        //    //    ColorDestinationBlend = Blend.DestinationColor,
        //    //    ColorBlendFunction = BlendFunction.Add
        //    //};
        //    //BlendState blendmode2 = new BlendState
        //    //{
        //    //    ColorSourceBlend = Blend.SourceColor,
        //    //    ColorDestinationBlend = Blend.DestinationColor,
        //    //    ColorBlendFunction = BlendFunction.Subtract
        //    //};
        //    //BlendState blendmode3 = new BlendState
        //    //{
        //    //    BlendFactor =
        //    //    ColorSourceBlend = Blend.SourceColor,
        //    //    ColorDestinationBlend = Blend.DestinationColor,
        //    //    ColorBlendFunction = BlendFunction.Subtract
        //    //};

        //    switch (tile.BlendMode)
        //    {
        //        case BlendMode.halfadd:
        //            return finalImageColor = blend0(baseColor, color);

        //        case BlendMode.add:
        //            return finalImageColor = blend1(baseColor, color);

        //        case BlendMode.subtract:
        //            return finalImageColor = blend2(baseColor, color);

        //        case BlendMode.quarteradd:
        //            //break;
        //            return finalImageColor = blend3(baseColor, color);
        //    }
        //    throw new Exception($"Blendtype is {tile.BlendMode}: There are only 4 blend modes, 0-3, 4+ are drawn directly.");
        //}
    }

    internal class MSK
    {
        private byte[] mskb;

        public MSK(byte[] mskb) => this.mskb = mskb;
    }
}