﻿using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ButtonPanel;
using DevExpress.XtraPrinting.Preview;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DevTest
{
    public partial class frm主界面 : XtraForm
    {
        public static string sSelectedTabName;
        public static string sCurrentFromName;
        public static string sText;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

        private DevExpress.XtraEditors.ButtonsPanelControl.GroupBoxButton gpBoxBtn_Hide;


        public frm主界面()
        {
            sSelectedTabName = "";
            sCurrentFromName = "";
            sText = "";
            InitializeComponent();
        }

        private string GetText(IntPtr hWnd)
        {
            StringBuilder result = new StringBuilder(128);
            GetWindowText(hWnd, result, result.Capacity);
            return result.ToString();
        }

        private void frm主界面_Load(object sender, EventArgs e)
        {
            gpBoxBtn_Hide = new DevExpress.XtraEditors.ButtonsPanelControl.GroupBoxButton("隐藏", null);
            gpBoxBtn_Hide.Tag = "hide";
            this.gc_Left.CustomHeaderButtons.AddRange(new IBaseButton[] { gpBoxBtn_Hide });
            sText = this.Text;
            StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            navBar合约业务_LinkClicked(null, null);
        }
        private void midPageEvent(object sender, EventArgs e)
        {
            if (((DevExpress.XtraTabbedMdi.XtraTabbedMdiManager)sender).SelectedPage != null)
                sSelectedTabName = ((DevExpress.XtraTabbedMdi.XtraTabbedMdiManager)sender).SelectedPage.MdiChild.Name;
        }
        private void frm主界面_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("点击'否'最小化到系统托盘,点击'是'退出应用.", "确定当前应用吗?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    myNotify.Dispose();
                    Dispose();
                    Application.Exit();
                }
                else
                {
                    myNotify.Visible = true;
                    e.Cancel = true;
                    Hide();
                }
            }
        }

        private void navBar合约业务_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {

            LoadFrm(new frm合约业务());
        }

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            myNotify.Dispose();
            Dispose();
            Application.Exit();
        }

        private void toolStripMenuItem_Show_Click(object sender, EventArgs e)
        {

            Show();
            WindowState = FormWindowState.Maximized;
            Activate();
        }

        private void gcLeft_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            switch (e.Button.Properties.Tag.ToString())
            {
                case "hide":
                    nbc_Left.Dock = (nbc_Left.Dock == DockStyle.Left ? DockStyle.Fill : DockStyle.Left);
                    switch (e.Button.Properties.Caption)
                    {
                        case "固定":
                            nbc_Left.Width = 169;
                            gc_Left.Width = 198;
                            pc_Left.Width = 196;
                            e.Button.Properties.Caption = "隐藏";
                            break;
                        case "隐藏":
                            nbc_Left.Width = 0;
                            gc_Left.Width = 29;
                            pc_Left.Width = 27;
                            e.Button.Properties.Caption = "固定";
                            break;
                    }
                    break;
            }
        }

        private void myNotify_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Maximized;
            Activate();
        }

        private void navBar新增合约_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            LoadFrm(new frm添加合约());
        }

        private void navBarDateEdit_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            LoadFrm(new frmDateEdit());
        }

        private PrintPreviewFormEx pv = null;
        private void navBarReports_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            frmXRP xrp = new frmXRP();
            pv = new ReportPrintTool(xrp).PreviewForm;
            if (!Util.childs.Contains(pv.Name))
            {
                pv.FormClosed += new FormClosedEventHandler(ptClosed);
                pv.MdiParent = this;
                pv.Text = "打印预览";
                Util.childs.Add(pv.Name);
                xrp.ShowPreview();
            }
        }
        private void ptClosed(object sender, FormClosedEventArgs e)
        {
            if (Util.childs.Contains(pv.Name))
                Util.childs.Remove(pv.Name);
        }
        private void navBarSGRID_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            LoadFrm(new frmSG());
        }

        private void timer_getWindowText_Tick(object sender, EventArgs e)
        {
            sCurrentFromName = GetText(GetForegroundWindow());
        }

        private void navBarWeather_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            LoadFrm(new frm天气());
        }
        private void LoadFrm(XtraForm f)
        {
            if (!(Util.childs.Contains(f.Name)))
            {
                Util.childs.Add(f.Name);
                f.MdiParent = this;
                f.Show();
            }
        }
    }
}
