﻿using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Battle Commands
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Battle-commands"/>
        public class Battle_Commands
        {

            public const int id = 0;
            public const int count = 39;
            private const int altlimitid = 18;

            public int ID { get; private set; }
            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public override string ToString() => Name;

            //public byte[] OffsetName;    //0x0000	2 bytes Offset to ability name

            //public byte[] OffsetDesc;    //0x0002	2 bytes Offset to ability description
            /// <summary>
            /// Ability data ID
            /// </summary>
            public byte Ability { get; private set; }           //0x0004	1 byte Ability data ID
            /// <summary>
            /// Unknown Flags
            /// </summary>
            public Debug_battleDat.Information.UnkFlag Flags { get; private set; }             //0x0005	1 byte Unknown Flags
            /// <summary>
            /// Target
            /// </summary>
            public Target Target { get; private set; }            //0x0006	1 byte Target
            /// <summary>
            /// Unknown / Unused
            /// </summary>
            public byte Unknown { get; private set; }           //0x0007	1 byte Unknown / Unused

            public void Read(BinaryReader br, int i)
            {
                ID = i;
                if (ID == 17) //No Mercy
                {
                    Name = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 0); //Fire Cross
                    Description = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 1);
                }
                else if (ID == 18) //Sorcery
                {
                    Name = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 2); //Ice Strike
                    Description = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 3);
                }
                else if (ID == 20) //Limit #1
                {
                    Name = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 4); //Desperado
                    Description = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 5);
                }
                else if (ID == 21) //Limit #2
                {
                    Name = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 6); //Blood Pain
                    Description = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 7);
                }
                else if (ID == 22) //Limit #3
                {
                    Name = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 8); //Massive Anchor
                    Description = Memory.Strings.Read(Strings.FileID.KERNEL, altlimitid, 9);
                }
                else
                {
                    Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                    Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                }

                
                br.BaseStream.Seek(4, SeekOrigin.Current);
                Ability = br.ReadByte();
                Flags = (Debug_battleDat.Information.UnkFlag)br.ReadByte();
                Target = (Target)br.ReadByte();
                Unknown = br.ReadByte();
            }

            public static List<Battle_Commands> Read(BinaryReader br)
            {
                var ret = new List<Battle_Commands>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Battle_Commands();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}

