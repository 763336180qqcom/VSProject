﻿using DevExpress.Printing.Core.PdfExport.Metafile;
using DevTest.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTest
{
    class DB
    {
        public static string CONNSTR = "";

        public static DataTable getDt(string tableName, List<ConditionItem> items)
        {
            DataTable dt = null;
            if (tableName != null && !CONNSTR.Equals(""))
            {
                StringBuilder sqlStr = new StringBuilder("select * from " + tableName);
                dt = new DataTable();
                try
                {
                    if (items != null && items.Count > 0)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (i == 0)
                            {
                                sqlStr.AppendFormat(" where " + items[i].Name + items[i].Type + "@" + items[i].Name);
                            }
                            else
                            {
                                sqlStr.Append(" and " + items[i].Name + items[i].Type + "@" + items[i].Name);
                            }
                        }
                    }

                    using (SqlConnection conn = new SqlConnection(CONNSTR))
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = sqlStr.ToString();
                        cmd.Connection = conn;
                        conn.Open();
                        if (items != null && items.Count > 0)
                        {
                            for (int i = 0; i < items.Count; i++)
                            {
                                SqlParameter param = new SqlParameter("@" + items[i].Name, items[i].Value);
                                cmd.Parameters.Add(param);
                            }
                        }
                        using (SqlDataAdapter adapter = new SqlDataAdapter())
                        {
                            adapter.SelectCommand = cmd;
                            adapter.Fill(dt);
                        }
                    }
                }
                catch (Exception e)
                {
                    dt = null;
                    throw e;
                }
            }
            return dt;
        }
        public static void 提交合约及费用(string str名称_合约, string str运营商ID, int int捆绑终端, int int允许老号,
            string str店员价, string str最低价, string str毛利奖励, string str现金奖励, DateTime? dte开始时间, DateTime? dte结束时间, int int状态_合约, DataTable dt费用收入)
        {
            if (string.IsNullOrEmpty(str名称_合约))
                throw new CustomException("合约名称不能为空");
            if (string.IsNullOrEmpty(str运营商ID))
                throw new CustomException("运营商ID不能为空");
            if (string.IsNullOrEmpty(str店员价))
                throw new CustomException("店员价不能为空");
            if (!dte开始时间.HasValue)
                throw new CustomException("开始时间不能为空");
            else
                dte开始时间 = dte开始时间.Value.Date;
            if (!Util.IsDecimal(str店员价))
                throw new CustomException("店员价格式错误");
            if (!Util.IsInt(str运营商ID))
                throw new CustomException("运营商ID格式错误");
            if (dte结束时间.HasValue && DateTime.Compare(Convert.ToDateTime(dte结束时间), Convert.ToDateTime(dte开始时间)) < 0)
                throw new CustomException("结束时间不能早于开始时间");
            if (dte结束时间.HasValue)
                dte结束时间 = dte结束时间.Value.Date;
            SqlConnection con = new SqlConnection(CONNSTR);
            SqlTransaction tran = null;
            try
            {
                con.Open();
                tran = con.BeginTransaction();
                SqlCommand cmd = null;
                string sql校验重复 = "SELECT TOP 1 fID FROM t合约业务 (updlock) WHERE 名称=@str名称 AND 状态=1";
                cmd = new SqlCommand(sql校验重复, con, tran);
                cmd.Parameters.AddWithValue("@str名称", str名称_合约);
                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                ad.Fill(dt);
                if (dt.Rows.Count > 0)
                    throw new CustomException("名称与已存在的有效合约重复");
                dt.Dispose();
                string sql添加合约 = "INSERT INTO t合约业务(名称,运营商ID,捆绑终端,允许老号,店员价,最低价,毛利奖励,现金奖励,开始时间,结束时间,状态) VALUES(@名称,@运营商ID,@捆绑终端,@允许老号,@店员价,@最低价,@毛利奖励,@现金奖励,@开始时间,@结束时间,@状态)";
                cmd = new SqlCommand(sql添加合约, con, tran);
                cmd.Parameters.AddWithValue("@名称", str名称_合约);
                cmd.Parameters.AddWithValue("@运营商ID", str运营商ID);
                cmd.Parameters.AddWithValue("@捆绑终端", int捆绑终端);
                cmd.Parameters.AddWithValue("@允许老号", int允许老号);
                cmd.Parameters.AddWithValue("@店员价", str店员价);
                if (string.IsNullOrEmpty(str最低价))
                    cmd.Parameters.AddWithValue("@最低价", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@最低价", str最低价);
                if (string.IsNullOrEmpty(str毛利奖励))
                    cmd.Parameters.AddWithValue("@毛利奖励", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@毛利奖励", str毛利奖励);
                if (string.IsNullOrEmpty(str现金奖励))
                    cmd.Parameters.AddWithValue("@现金奖励", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@现金奖励", str现金奖励);
                cmd.Parameters.AddWithValue("@开始时间", dte开始时间);
                if (dte结束时间.HasValue)
                    cmd.Parameters.AddWithValue("@结束时间", dte结束时间);
                else
                    cmd.Parameters.AddWithValue("@结束时间", DBNull.Value);
                cmd.Parameters.AddWithValue("@状态", int状态_合约);
                cmd.ExecuteNonQuery();
                #region
                if (dt费用收入 != null)
                {
                    string sql获取fID = "SELECT IDENT_CURRENT('t合约业务')";
                    cmd = new SqlCommand(sql获取fID, con, tran);
                    SqlDataAdapter ad1 = new SqlDataAdapter(cmd);
                    DataTable dt1 = new DataTable();
                    ad1.Fill(dt1);
                    string strfPID = dt1.Rows[0][0].ToString();
                    for (int i = 0; i < dt费用收入.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dt费用收入.Rows[i]["项目名称"].ToString()))
                        {
                            throw new CustomException("第" + (i + 1) + "行名称不能为空");
                        }
                        if (string.IsNullOrEmpty(dt费用收入.Rows[i]["类别"].ToString()))
                        {
                            throw new CustomException("选择类别");
                        }
                        if (!Util.IsDecimal(dt费用收入.Rows[i]["金额"].ToString()))
                        {
                            throw new CustomException("第" + (i + 1) + "行金额格式错误");
                        }
                        int int类别 = 0;
                        if (dt费用收入.Rows[i]["类别"].Equals("收入"))
                            int类别 = 1;
                        else
                            int类别 = -1;
                        if (int类别 == 0)
                            throw new CustomException("第" + (i + 1) + "行类别格式错误");
                        int int状态_费用收入 = 0;
                        if (dt费用收入.Rows[i]["状态"].Equals("有效"))
                            int状态_费用收入 = 1;
                        else
                            int状态_费用收入 = -1;
                        if (int状态_费用收入 == 0)
                            throw new CustomException("第" + (i + 1) + "行状态格式错误");
                        string str名称_费用收入 = dt费用收入.Rows[i]["项目名称"].ToString();
                        string str金额 = dt费用收入.Rows[i]["金额"].ToString();
                        string str期数 = dt费用收入.Rows[i]["期数"].ToString();
                        string sql添加费用收入 = "INSERT INTO DevTest.dbo.t费用收入 " +
                            "VALUES(@strfPID,@intType,@str名称,@str金额,@str期数,@str状态)";
                        cmd = new SqlCommand(sql添加费用收入, con, tran);
                        cmd.Parameters.AddWithValue("@strfPID", strfPID);
                        cmd.Parameters.AddWithValue("@intType", int类别);
                        cmd.Parameters.AddWithValue("@str名称", str名称_费用收入);
                        cmd.Parameters.AddWithValue("@str金额", str金额);
                        cmd.Parameters.AddWithValue("@str期数", str期数);
                        cmd.Parameters.AddWithValue("@str状态", int状态_费用收入);
                        cmd.ExecuteNonQuery();
                    }
                    #endregion
                }
                tran.Commit();
            }
            catch (CustomException ex)
            {
                tran.Rollback();
                throw ex;
            }
            catch (Exception)
            {

                tran.Rollback();
                throw new CustomException("操作失败！");
            }
            finally
            {
                con.Close();
            }
        }
        public static void 提交合约及费用(Hashtable para_HY, List<Hashtable> para_FY)
        {
            //名称 ,
            //运营商ID ,
            //捆绑终端 ,
            //允许老号 ,
            //店员价 ,
            //最低价 ,
            //毛利奖励 ,
            //现金奖励 ,
            //开始时间 ,
            //结束时间 ,
            //状态
            if (string.IsNullOrEmpty((string)para_HY["名称"]))
                throw new CustomException("输入正确的合约名称！");
            if ((int)para_HY["运营商ID"] <= 0)
                throw new CustomException("输入正确的运营商ID！");
            if ((decimal)para_HY["店员价"] < 0)
                throw new CustomException("店员价不能为负数！");
            if ((decimal)para_HY["最低价"] < 0)
                throw new CustomException("最低不能为负数！");
            if ((decimal)para_HY["毛利奖励"] < 0)
                throw new CustomException("毛利奖励不能为负数！");
            if ((decimal)para_HY["现金奖励"] < 0)
                throw new CustomException("现金奖励不能为负数！");
            for (int i = 0; i < para_FY.Count; i++)
            {
                if (string.IsNullOrEmpty((string)para_FY[i]["名称!"]))
                {
                    if ((int)para_FY[i]["type"] == 1)
                        throw new CustomException("输入正确的收入名称!");
                    if ((int)para_FY[i]["type"] == -1)
                        throw new CustomException("输入正确的费用名称!");
                }
                if ((decimal)para_FY[i]["金额"] < 0)
                    throw new CustomException("金额必须为正数！");
                if ((int)para_FY[i]["type"] == -1 && (int)para_FY[i]["期数"] != 1)
                    throw new CustomException("费用项目期数必须为1!");
                if ((int)para_FY[i]["type"] == 1 && (int)para_FY[i]["期数"] <= 0)
                    throw new CustomException("收入项目期数必须为正数!");
            }
            SqlConnection con = new SqlConnection(CONNSTR);
            SqlTransaction tran = null;
            try
            {
                con.Open();
                tran = con.BeginTransaction();
                int fPID = 0;
                string inValue_HY = "INSERT INTO t合约业务 VALUES(@名称,@运营商ID,@捆绑终端,@允许老号,@店员价,@最低价,@毛利奖励,@现金奖励,@开始时间,@结束时间,@状态)";
                SqlCommand cmd = new SqlCommand(inValue_HY, con, tran);
                cmd.Parameters.AddWithValue("@名称", para_HY["名称"]);
                cmd.Parameters.AddWithValue("@运营商ID", para_HY["运营商ID"]);
                cmd.Parameters.AddWithValue("@捆绑终端", para_HY["捆绑终端"]);
                cmd.Parameters.AddWithValue("@允许老号", para_HY["允许老号"]);
                cmd.Parameters.AddWithValue("@店员价", para_HY["店员价"]);
                cmd.Parameters.AddWithValue("@最低价", para_HY["最低价"]);
                cmd.Parameters.AddWithValue("@毛利奖励", para_HY["毛利奖励"]);
                cmd.Parameters.AddWithValue("@现金奖励", para_HY["现金奖励"]);
                cmd.Parameters.AddWithValue("@开始时间", para_HY["开始时间"]);
                cmd.Parameters.AddWithValue("@结束时间", para_HY["结束时间"]);
                cmd.Parameters.AddWithValue("@状态", para_HY["状态"]);
                cmd.ExecuteNonQuery();
                string selectID = "SELECT TOP 1 fID FROM t合约业务 ORDER BY fID DESC";
                cmd = new SqlCommand(selectID, con, tran);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                if (dt.Rows.Count != 1)
                    throw new CustomException("插入数据失败！");
                fPID = (int)dt.Rows[0][0];
                for (int i = 0; i < para_FY.Count; i++)
                {
                    string inValue_FY = "INSERT INTO t费用收入 VALUES(@fPID,@type,@名称,@金额,@期数,@状态)";
                    cmd = new SqlCommand(inValue_FY, con, tran);
                    cmd.Parameters.AddWithValue("@fPID", fPID);
                    cmd.Parameters.AddWithValue("@type", para_FY[i]["type"]);
                    cmd.Parameters.AddWithValue("@名称", para_FY[i]["名称"]);
                    cmd.Parameters.AddWithValue("@金额", para_FY[i]["金额"]);
                    cmd.Parameters.AddWithValue("@期数", para_FY[i]["期数"]);
                    cmd.Parameters.AddWithValue("@状态", para_FY[i]["状态"]);
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        break;
                        throw new CustomException("插入数据失败！");
                    }
                }
                tran.Commit();
            }
            catch (CustomException ex)
            {
                tran.Rollback();
                throw new CustomException(ex.Message);
            }
            catch (Exception)
            {
                tran.Rollback();
                throw new Exception("操作失败！");
            }
        }

        public static DataTable Ini合约表结构()
        {
            string strGetDt = "SELECT * FROM v合约业务 WHERE fID=@fID";
            SqlConnection con = new SqlConnection(CONNSTR);
            con.Open();
            SqlCommand cmd = new SqlCommand(strGetDt, con);
            cmd.Parameters.AddWithValue("@fID", -1);
            SqlDataAdapter at = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            at.Fill(dt);
            return dt;
        }
        public static DataTable Ini收入费用表结构()
        {
            string strGetDt = "SELECT * FROM v费用收入 WHERE 业务ID=@业务ID";
            SqlConnection con = new SqlConnection(CONNSTR);
            con.Open();
            SqlCommand cmd = new SqlCommand(strGetDt, con);
            cmd.Parameters.AddWithValue("@业务ID", -1);
            SqlDataAdapter at = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            at.Fill(dt);
            dt.Clear();
            return dt;
        }
        public static void frm合约业务_Update(string strfID, string str名称, DateTime? dte开始时间, DateTime? dte结束时间)
        {
            if (string.IsNullOrEmpty(str名称))
                throw new CustomException("名称必填");
            if (!dte开始时间.HasValue)
                throw new CustomException("开始时间必选");
            else
                dte开始时间 = dte开始时间.Value.Date;
            if (dte结束时间.HasValue)
                dte结束时间 = dte结束时间.Value.Date;
            if (dte结束时间.HasValue && DateTime.Compare(Convert.ToDateTime(dte结束时间), Convert.ToDateTime(dte开始时间)) < 0)
                throw new CustomException("结束时间不能早于开始时间");
            SqlConnection con = new SqlConnection(CONNSTR);
            SqlTransaction tran = null;
            try
            {
                con.Open();
                tran = con.BeginTransaction();
                string sqlCheck = "SELECT * FROM t合约业务(UPDLOCK) WHERE fID<>@strfID AND 状态=1 AND 名称 =@str名称";
                SqlCommand cmd = new SqlCommand(sqlCheck, con, tran);
                cmd.Parameters.AddWithValue("@strfID", strfID);
                cmd.Parameters.AddWithValue("@str名称", str名称);
                SqlDataAdapter adt = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adt.Fill(dt);
                if (dt.Rows.Count > 0)
                    throw new CustomException("不能与已有的有效合约名称重复");
                string sqlUpdate = " UPDATE t合约业务 SET 名称=@str名称,开始时间=@dte开始时间, 结束时间 = @dte结束时间";
                sqlUpdate += " WHERE fID=@strfID";
                cmd = new SqlCommand(sqlUpdate, con, tran);
                cmd.Parameters.AddWithValue("@strfID", strfID);
                cmd.Parameters.AddWithValue("@str名称", str名称);
                cmd.Parameters.AddWithValue("@dte开始时间", dte开始时间);
                if (dte结束时间.HasValue)
                    cmd.Parameters.AddWithValue("@dte结束时间", dte结束时间);
                else
                    cmd.Parameters.AddWithValue("@dte结束时间", DBNull.Value);
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
            catch (CustomException ex)
            {
                tran.Rollback();
                throw new CustomException(ex.Message);
            }
            catch (Exception)
            {
                tran.Rollback();
                throw new Exception("操作失败！");
            }
            finally
            {
                con.Close();
            }
        }
        public static DataTable sRefreshHY(ref string lastStamp)
        {
            if (!Util.IsInt(lastStamp))
                throw new CustomException("传入的时间戳格式错误");
            using (SqlConnection con = new SqlConnection(CONNSTR))
            {
                con.Open();
                SqlCommand cmd = null;
                string getStamp = "SELECT MAX(int时间戳) FROM v合约业务(NOLOCK)";
                cmd = new SqlCommand(getStamp, con);
                SqlDataAdapter adt = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adt.Fill(dt);
                if (!(dt.Rows.Count > 0) || dt.Rows[0][0] == null)
                    throw new CustomException("获取最新时间戳出错");
                if (Convert.ToInt32(lastStamp) < Convert.ToInt32(dt.Rows[0][0]))
                {
                    lastStamp = dt.Rows[0][0].ToString();
                    string getDt = "SELECT [fID],[名称],[运营商ID],[捆绑终端],[允许老号],[店员价],[最低价],[毛利奖励]," +
                        "[现金奖励],[开始时间],[结束时间],[状态] FROM v合约业务(NOLOCK)";
                    cmd = new SqlCommand(getDt, con);
                    adt = new SqlDataAdapter(cmd);
                    dt = new DataTable();
                    adt.Fill(dt);
                    return dt;
                }
            }
            return null;
        }

        public static DataTable getNames(string[] flags)
        {
            using (SqlConnection con = new SqlConnection(CONNSTR))
            {
                con.Open();
                switch (flags[0])
                {
                    case "P":
                        SqlCommand cmd = new SqlCommand("SELECT DISTINCT 省中文  FROM t城市列表(nolock) WHERE 中文=所属上级市中文 ORDER BY 省中文 DESC", con);
                        SqlDataAdapter adt = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adt.Fill(dt);
                        if (dt.Rows.Count > 0)
                            return dt;
                        else
                            throw new CustomException("没有找到省级名称"); ;
                    case "C":
                        SqlCommand cmdC = new SqlCommand("SELECT 所属上级市中文 FROM t城市列表(nolock) WHERE 省中文=@省中文 AND 中文=所属上级市中文", con);
                        cmdC.Parameters.AddWithValue("@省中文", flags[1]);
                        SqlDataAdapter adtC = new SqlDataAdapter(cmdC);
                        DataTable dtC = new DataTable();
                        adtC.Fill(dtC);
                        if (dtC.Rows.Count > 0)
                            return dtC;
                        else
                            throw new CustomException("没有找到对应的下级市名称");
                    case "D":
                        SqlCommand cmdD = new SqlCommand("SELECT 中文 FROM t城市列表 WHERE 所属上级市中文=@市中文", con);
                        cmdD.Parameters.AddWithValue("@市中文", flags[1]);
                        SqlDataAdapter adtD = new SqlDataAdapter(cmdD);
                        DataTable dtD = new DataTable();
                        adtD.Fill(dtD);
                        if (dtD.Rows.Count > 0)
                            return dtD;
                        else
                            throw new CustomException("没有找到对应的下级县区名称");
                }
            }
            return null;
        }

    }
}
