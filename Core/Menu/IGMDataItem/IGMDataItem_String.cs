﻿using Microsoft.Xna.Framework;

namespace OpenVIII
{
    #region Classes

    public class IGMDataItem_String : IGMDataItem
    {
        private byte _palette;

        public FF8String Data { get; set; }
        public Font.ColorID Colorid { get; set; }
        public Font.ColorID Faded_Colorid { get; set; }
        public float Blink_Adjustment { get; set; }
        public Icons.ID? Icon { get; set; }
        public byte Palette
        {
            get => _palette; set => _palette = (byte)(value < Memory.Icons.PaletteCount ? value : 2);
        }
        public byte Faded_Palette { get; set; }

        public bool Blink => Palette != Faded_Palette || Colorid != Faded_Colorid;
        public IGMDataItem_String(FF8String data, Rectangle? pos = null, Font.ColorID? color = null, Font.ColorID? faded_color = null, float blink_adjustment = 1f) : base(pos)
        {
            Data = data;
            Colorid = color ?? Font.ColorID.White;
            Faded_Colorid = faded_color ?? Colorid;
            Blink_Adjustment = blink_adjustment;
        }

        public IGMDataItem_String(Icons.ID? icon, byte? palette, FF8String data, Rectangle? pos = null, Font.ColorID? color = null, byte? faded_palette = null, Font.ColorID? faded_color = null, float blink_adjustment = 1f) : this(data, pos, color, faded_color, blink_adjustment)
        {
            Icon = icon;
            Palette = palette ?? 2;
            Faded_Palette = faded_palette ?? Palette;
        }

        public override void Draw()
        {
            if (Enabled)
            {
                Rectangle r = Pos;
                if (Icon != null && Icon != Icons.ID.None)
                {
                    Rectangle r2 = r;
                    r2.Size = Point.Zero;
                    Memory.Icons.Draw(Icon, Palette, r2, new Vector2(Scale.X), Fade);

                    if (Blink)
                        Memory.Icons.Draw(Icon, Faded_Palette, r2, new Vector2(Scale.X), Fade * Blink_Amount * Blink_Adjustment);
                    r.Offset(Memory.Icons.GetEntryGroup(Icon).Width * Scale.X, 0);
                }
                Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade, color: Colorid);
                if(Blink)
                    Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade * Blink_Amount * Blink_Adjustment, color: Faded_Colorid);
            }
        }
    }

    #endregion Classes
}