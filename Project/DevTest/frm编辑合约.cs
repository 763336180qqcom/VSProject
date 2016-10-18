﻿using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevTest
{
    public partial class frm编辑合约 : XtraForm
    {
        public frm编辑合约()
        {
            InitializeComponent();
        }
        public static string str名称="";
        public static DateTime? dte开始=DateTime.Today;
        public static DateTime? dte结束=null;
        public static string fID = "-1";
        private void frm编辑合约_Load(object sender, EventArgs e)
        {
           
            if (!string.IsNullOrEmpty(str名称))
                txt名称.Text = str名称;
            if (dte开始.HasValue)
                dte开始时间.EditValue = dte开始;
            else
                dte开始时间.EditValue = DateTime.Today;
            if (dte结束.HasValue)
                dte结束时间.EditValue = dte结束;
            else
                dte结束时间.EditValue = null;
        }

        private void btn保存_Click(object sender, EventArgs e)
        {
            str名称 = txt名称.Text.Trim();
            dte开始 = (DateTime?)dte开始时间.EditValue;
            dte结束 = (DateTime?)dte结束时间.EditValue;
            try
            {
                DB.frm合约业务_Update(fID, str名称, dte开始, dte结束);
                TipForm.getInstance().showShort(800);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
