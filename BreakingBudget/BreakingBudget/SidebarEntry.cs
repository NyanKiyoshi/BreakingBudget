﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kerido.Controls;

namespace BreakingBudget
{
    class SidebarEntry
    {
        private static byte[] DEFAULT_ICON = new byte[] { 0xEE, 0x97, 0x90 };

        public SidebarEntry(MultiPanePage target, string text)
        {
            this.Text = text;
            this.Target = target;
        }

        public SidebarEntry(MultiPanePage target, byte[] icon, string text) : this(target, text)
        {
            this.Icon = icon;
        }

        // A UTF8 char array representing a Google Material icon
        private byte[] _icon;
        public byte[] Icon {
            get
            {
                return (this._icon != null) ? this._icon : SidebarEntry.DEFAULT_ICON;
            }
            set
            {
                this._icon = value;
            }
        }

        // The entry's name
        public string Text { get; set; }

        // The entry's target
        public MultiPanePage Target { get; set; }

        // The children nodes.
        public SidebarEntry[] children { get; set; }
    }
}
