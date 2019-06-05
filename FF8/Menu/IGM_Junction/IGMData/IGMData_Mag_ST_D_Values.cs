﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {

            private class IGMData_Mag_ST_D_Values : IGMData
            {
                public IGMData_Mag_ST_A_D_Slots ST_A_D_Slots { get; }

                //    public new Saves.CharacterData PrevSetting { get; private set; }
                //    public new Saves.CharacterData Setting { get; private set; }
                public IGMData_Mag_ST_D_Values(IGMData_Mag_ST_A_D_Slots mag_ST_A_D_Slots) : base( 14, 5, new IGMDataItem_Box(title: Icons.ID.Status_Defense, pos: new Rectangle(280, 342, 545, 288)), 2, 7)
                {
                    ST_A_D_Slots = mag_ST_A_D_Slots;
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-25, -10);
                    SIZE[i].Y -= 3 * row;
                }

                public override bool Update()
                {
                    if (Memory.State.Characters != null)
                    {
                        byte[] spell = new byte[] {
                            Memory.State.Characters[Character].Stat_J[Kernel_bin.Stat.ST_Def_1],
                            Memory.State.Characters[Character].Stat_J[Kernel_bin.Stat.ST_Def_2],
                            Memory.State.Characters[Character].Stat_J[Kernel_bin.Stat.ST_Def_3],
                            Memory.State.Characters[Character].Stat_J[Kernel_bin.Stat.ST_Def_4]
                        };
                        Dictionary<Kernel_bin.J_Statuses, byte> total = new Dictionary<Kernel_bin.J_Statuses, byte>(8);

                        IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(Kernel_bin.J_Statuses)).Cast<Enum>();
                        foreach (Enum flag in availableFlags.Where(d => !total.ContainsKey((Kernel_bin.J_Statuses)d)))
                            total.Add((Kernel_bin.J_Statuses)flag, 0);
                        for (int i = 0; i < spell.Length; i++)
                            foreach (Enum flag in availableFlags.Where(Kernel_bin.MagicData[spell[i]].Stat_J_def.HasFlag))
                            {
                                total[(Kernel_bin.J_Statuses)flag] += (byte)((Kernel_bin.MagicData[spell[i]].Stat_J_def_val * Memory.State.Characters[Character].Magics[spell[i]]) / 100);
                                if (total[(Kernel_bin.J_Statuses)flag] > 100) total[(Kernel_bin.J_Statuses)flag] = 100;
                            }

                        Enum[] availableFlagsarray = availableFlags.ToArray();
                        for (short pos = 0; pos < Count - 1; pos++)
                        {
                            ITEM[pos, 0] = new IGMDataItem_Icon(Icons.ID.Status_Death + pos, new Rectangle(SIZE[pos + 1].X, SIZE[pos + 1].Y, 0, 0), 10);
                            ITEM[pos, 1] = null;
                            ITEM[pos, 2] = null;
                            ITEM[pos, 3] = new IGMDataItem_Int(total[(Kernel_bin.J_Statuses)availableFlagsarray[pos + 1]], new Rectangle(SIZE[pos + 1].X + SIZE[pos + 1].Width - 80, SIZE[pos + 1].Y, 0, 0), 17, numtype: Icons.NumType.sysFntBig, spaces: 3);
                            ITEM[pos, 4] = new IGMDataItem_String("%", new Rectangle(SIZE[pos + 1].X + SIZE[pos + 1].Width - 20, SIZE[pos + 1].Y, 0, 0));

                            //ITEM[pos, 2] = new IGMDataItem_Icon(Icons.ID.Arrow_Up, new Rectangle(SIZE[pos + 1].X + SIZE[pos + 1].Width - 105, SIZE[pos + 1].Y, 0, 0), 17);

                            //if (PrevSetting == null || PrevSetting.Stat_J[stat] == Setting.Stat_J[stat] || PrevSetting.TotalStat(stat, VisableCharacter) == Setting.TotalStat(stat, VisableCharacter))
                            //{
                            //    ITEM[pos, 4] = null;
                            //}
                            //else if (PrevSetting.TotalStat(stat, VisableCharacter) > Setting.TotalStat(stat, VisableCharacter))
                            //{
                            //    ((IGMDataItem_Icon)ITEM[pos, 0]).Pallet = 5;
                            //    ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Pallet = 5;
                            //    ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Red;
                            //    ((IGMDataItem_Int)ITEM[pos, 2]).Colorid = Font.ColorID.Red;
                            //    if (ITEM[pos, 3] != null)
                            //        ((IGMDataItem_String)ITEM[pos, 3]).Colorid = Font.ColorID.Red;
                            //    ITEM[pos, 4] = new IGMDataItem_Icon(Icons.ID.Arrow_Down, new Rectangle(SIZE[pos].X + 250, SIZE[pos].Y, 0, 0), 16);
                            //}
                            //else
                            //{
                            //    ((IGMDataItem_Icon)ITEM[pos, 0]).Pallet = 6;
                            //    ((IGMDataItem_Icon)ITEM[pos, 0]).Faded_Pallet = 6;
                            //    ((IGMDataItem_String)ITEM[pos, 1]).Colorid = Font.ColorID.Yellow;
                            //    ((IGMDataItem_Int)ITEM[pos, 2]).Colorid = Font.ColorID.Yellow;
                            //    if (ITEM[pos, 3] != null)
                            //        ((IGMDataItem_String)ITEM[pos, 3]).Colorid = Font.ColorID.Yellow;
                            //    ITEM[pos, 4] = new IGMDataItem_Icon(Icons.ID.Arrow_Up, new Rectangle(SIZE[pos].X + 250, SIZE[pos].Y, 0, 0), 17);
                            //}
                        }
                    }
                    return base.Update();
                }
            }
        }
    }
}