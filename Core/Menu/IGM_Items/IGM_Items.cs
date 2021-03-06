﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OpenVIII
{
    public partial class IGM_Items : Menu
    {
        #region Fields

        protected Dictionary<Mode, Func<bool>> InputsDict;

        #endregion Fields

        #region Events

        public event EventHandler<KeyValuePair<byte, FF8String>> ChoiceChangeHandler;

        public event EventHandler RefreshCompletedHandler;

        public event EventHandler<Faces.ID> TargetChangeHandler;

        #endregion Events

        #region Enums

        public enum Mode : byte
        {
            /// <summary>
            /// Select one of the 4 top options to do
            /// </summary>
            TopMenu,

            /// <summary>
            /// Choose an item to use
            /// </summary>
            SelectItem,

            /// <summary>
            /// Choose a character or gf to use item on
            /// </summary>
            UseItemOnTarget,
        }

        public enum SectionName : byte
        {
            TopMenu,
            UseItemGroup,
            Help,
            Title,
        }

        #endregion Enums

        #region Properties

        public IGMData.Pool.Item ItemPool => ((IGMData.Pool.Item)((IGMData.Group.Base)Data[SectionName.UseItemGroup])[1, 0]);

        private IGMDataItem.HelpBox help => (IGMDataItem.HelpBox)Data[SectionName.Help];

        #endregion Properties

        #region Methods

        public static IGM_Items Create() => Create<IGM_Items>();

        public override bool Inputs() => InputsDict[(Mode)GetMode()]();

        public override void Refresh() => Refresh(false);

        public new void Refresh(bool skipmode)
        {
            if (!skipmode)
                SetMode(Mode.SelectItem);
            base.Refresh();
            RefreshCompletedHandler?.Invoke(this, null);
        }

        protected override void Init()
        {
            Size = new Vector2 { X = 840, Y = 630 };
            base.Init();
            List<Task> tasks = new List<Task>
            {
                Task.Run(() => Data.TryAdd(SectionName.Help, new IGMDataItem.HelpBox { Pos = new Rectangle(15, 69, 810, 78), Title = Icons.ID.HELP, Options = Box_Options.Middle})),
                Task.Run(() => {
                    FF8String[] keys = new FF8String[]{
                        Strings.Name.Use, //todo add to Strings.Name
                        Strings.Name.Rearrange,
                        Strings.Name.Sort,
                        Strings.Name.Battle };
                    FF8String[] values = new FF8String[]{
                        Strings.Description.Use, //todo add to Strings.Description
                        Strings.Description.Rearrange,
                        Strings.Description.Sort,
                        Strings.Description.Battle};
                    if(keys.Distinct().Count() == keys.Length && keys.Length == values.Length)
                    Data.TryAdd(SectionName.TopMenu, IGMData_TopMenu.Create((from i in Enumerable.Range(0,keys.Length) select i).ToDictionary(x=>keys[x],x=>values[x])));
                    else Data.TryAdd(SectionName.TopMenu, null);
                }),
                Task.Run(() => Data.TryAdd(SectionName.Title, new IGMDataItem.Box { Data = Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2), Pos = new Rectangle(615, 0, 225, 66)})),
                Task.Run(() => Data.TryAdd(SectionName.UseItemGroup, IGMData.Group.Base.Create(IGMData_Statuses.Create(),IGMData.Pool.Item.Create(),IGMData_TargetPool.Create())))
                
            };
            Task.WaitAll(tasks.ToArray());
            ChoiceChangeHandler = help.TextChangeEvent;
            ItemPool.ItemChangeHandler += help.TextChangeEvent;
            ModeChangeHandler += help.ModeChangeEvent;
            Func<bool> TopMenuInputs = null;
            if (Data[SectionName.TopMenu] != null)
                TopMenuInputs = Data[SectionName.TopMenu].Inputs;
            InputsDict = new Dictionary<Mode, Func<bool>>() {
                {Mode.TopMenu, TopMenuInputs},
                {Mode.SelectItem, UseItemGroup.ITEM[1, 0].Inputs},
                {Mode.UseItemOnTarget, UseItemGroup.ITEM[2, 0].Inputs}
                };
            SetMode(Mode.SelectItem);
        }

        private IGMData.Base UseItemGroup=> (IGMData.Group.Base)Data[SectionName.UseItemGroup];

        #endregion Methods
    }
}