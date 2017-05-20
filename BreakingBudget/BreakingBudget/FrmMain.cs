﻿using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using MetroFramework;
using Kerido.Controls;

namespace BreakingBudget
{
    public partial class FrmMain : MetroForm
    {
        private readonly SidebarEntry[] SidebarEntries;

        public readonly string BaseName;
        public readonly MultiPanePage DefaultPage;

        public FrmMain()
        {
            InitializeComponent();

            // set the base name (used later to rename the form)
            this.BaseName = this.Text;

            // set the default page and switch to it
            this.DefaultPage = this.Meow;
            this.SwitchPanel(this.DefaultPage);

            this.Font = new Font("Arial", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
            this.SidebarEntries = new SidebarEntry[] 
            {
                new SidebarEntry(this.Meow, new byte[] { 0xEE, 0xA1, 0xA8 }, "Meow"),
                new SidebarEntry(this.OwO, new byte[] { 0xEE, 0xA1, 0xA9 }, "OwO"),
                new SidebarEntry(this.OmO, new byte[] { 0xEE, 0x90, 0xA0 }, "OmO")
            };

            AutoCompleter.ImplementCompleter(textBox1, 2);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            GenerateSidebar();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (
                MetroMessageBox.Show(this,
                    "Êtes-vous sûr de vouloir quitter ?",
                    "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question
                ) != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }
}
