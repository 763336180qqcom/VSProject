﻿using DevTest.Common;
using DevTest.Entity;
using System;
using System.Net;
using System.Runtime.Remoting.Messaging;
using DevTest.CN_GetWeather;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Xml;
using System.Collections.Generic;
using DevExpress.Utils;

namespace DevTest
{
    public partial class frm天气 : XtraFormC
    {
        private List<Province> mPro;
        private string[] weatherResult;
        private delegate WeatherInfo mGetWeatherWithPara(string city);
        private delegate void EndGetWeather(WeatherInfo info);
        private WeatherWS mService;
        // [DllImport("user32", EntryPoint = "HideCaret")]
        // private static extern bool HideCaret(IntPtr hWnd);
        public frm天气()
        {
            InitializeComponent();
        }
        private void frm天气_Load(object sender, EventArgs e)
        {
            //me0.MouseDown += me0MouseDown;
        }
        private void frm天气_Shown(object sender, EventArgs ex)
        {
           // XtraFormP.showWait("请稍等", "正在加载天气信息");
            try
            {
                mGetWeatherWithPara d = new mGetWeatherWithPara(getWeatherWithPara);
                if (cmb市.EditValue != null)
                    d.BeginInvoke(cmb市.EditValue.ToString(), new AsyncCallback(dCallBack), this);
                else
                    d.BeginInvoke("", new AsyncCallback(dCallBack), this);
            }
            catch (Exception e)
            {
                XtraMessageBox.Show(e.Message);
            }
        }

        private void me0MouseDown(object sender, MouseEventArgs e)
        {
            //HideCaret((sender as MemoEdit).Handle);
        }
        private void enableC()
        {
            meMain.Enabled = !meMain.Enabled;
            me0.Enabled = !me0.Enabled;
            me1.Enabled = !me1.Enabled;
            me2.Enabled = !me2.Enabled;
            me3.Enabled = !me3.Enabled;
            me4.Enabled = !me4.Enabled;
        }
        private void iniControl(WeatherInfo info)
        {
            imageEdit0A.Properties.InitialImage = Util.getImage("0.gif");
            enableC();
            meMain.Text = string.Format("\t{0}/", info.City);
            meMain.Text += info.Area + "\r\n\t";
            meMain.Text += info.Tm + "\r\n\t";
            meMain.Text += info.ToDay + "\r\n\t";
            meMain.Text += info.Uv + "\r\n\t";
            meMain.Text += info.UvIndex.Replace("。", "。\r\n\t");

            me0.Text = info.Weather5Days[0].ToDay + Environment.NewLine;
            me0.Text += info.Weather5Days[0].T.Replace("/", "~") + Environment.NewLine;
            me0.Text += info.Weather5Days[0].Wind + "\r\n";
            imageEdit0A.EditValue = info.Weather5Days[0].IconA;
            imageEdit0B.EditValue = info.Weather5Days[0].IconB;

            me1.Text = info.Weather5Days[1].ToDay + "\r\n";
            me1.Text += info.Weather5Days[1].T.Replace("/", "~") + "\r\n";
            me1.Text += info.Weather5Days[1].Wind + "\r\n";
            imageEdit1A.EditValue = info.Weather5Days[1].IconA;
            imageEdit1B.EditValue = info.Weather5Days[1].IconB;

            me2.Text = info.Weather5Days[2].ToDay + "\r\n";
            me2.Text += info.Weather5Days[2].T.Replace("/", "~") + "\r\n";
            me2.Text += info.Weather5Days[2].Wind + "\r\n";
            imageEdit2A.EditValue = info.Weather5Days[2].IconA;
            imageEdit2B.EditValue = info.Weather5Days[2].IconB;

            me3.Text = info.Weather5Days[3].ToDay + "\r\n";
            me3.Text += info.Weather5Days[3].T.Replace("/", "~") + "\r\n";
            me3.Text += info.Weather5Days[3].Wind + "\r\n";
            imageEdit3A.EditValue = info.Weather5Days[3].IconA;
            imageEdit3B.EditValue = info.Weather5Days[3].IconB;

            me4.Text = info.Weather5Days[4].ToDay + "\r\n";
            me4.Text += info.Weather5Days[4].T.Replace("/", "~") + "\r\n";
            me4.Text += info.Weather5Days[4].Wind + "\r\n";
            imageEdit4A.EditValue = info.Weather5Days[4].IconA;
            imageEdit4B.EditValue = info.Weather5Days[4].IconB;
            if (mPro.Count > 0)
            {
                cmb省.Properties.Items.Clear();
                foreach (Province p in mPro)
                    cmb省.Properties.Items.Add(p.Name);
            }
            XtraFormP.closeWait();
        }

        private void dCallBack(IAsyncResult result)
        {
            try
            {
                mGetWeatherWithPara d = (result as AsyncResult).AsyncDelegate as mGetWeatherWithPara;
                EndGetWeather endGetDelegate = new EndGetWeather(iniControl);
                WeatherInfo info = d.EndInvoke(result);
                if (info != null)
                    this.Invoke(endGetDelegate, info);
                else
                    throw new Exception(Properties.Resources.strInfoFailed);
            }
            catch (Exception e)
            {
                XtraFormP.closeWait();
                XtraMessageBox.Show(e.Message);
                return;
            }
        }

        private WeatherInfo getWeatherWithPara(string city)
        {
            try
            {
                mService = new WeatherWS();
                if (string.IsNullOrEmpty(city))
                    city = getCity().Replace("市", "").Trim();
                mService.Timeout = 16384;
                weatherResult = mService.getWeather(city, "3573dee3157c41c8ad5fd76feef41cdd");
                WeatherInfo info = new WeatherInfo() { City = weatherResult[0], Area = weatherResult[1], Tm = weatherResult[3], ToDay = weatherResult[4], Uv = weatherResult[5], UvIndex = weatherResult[6] };

                WeatherInfoA a = new WeatherInfoA();
                a.ToDay = weatherResult[7];
                a.T = weatherResult[8];
                a.Wind = weatherResult[9];
                a.IconA = Util.getImage(weatherResult[10]);
                a.IconB = Util.getImage(weatherResult[11]);
                info.Weather5Days.Add(a);

                a = new WeatherInfoA();
                a.ToDay = weatherResult[12];
                a.T = weatherResult[13];
                a.Wind = weatherResult[14];
                a.IconA = Util.getImage(weatherResult[15]);
                a.IconB = Util.getImage(weatherResult[16]);
                info.Weather5Days.Add(a);

                a = new WeatherInfoA();
                a.ToDay = weatherResult[17];
                a.T = weatherResult[18];
                a.Wind = weatherResult[19];
                a.IconA = Util.getImage(weatherResult[20]);
                a.IconB = Util.getImage(weatherResult[21]);
                info.Weather5Days.Add(a);

                a = new WeatherInfoA();
                a.ToDay = weatherResult[22];
                a.T = weatherResult[23];
                a.Wind = weatherResult[24];
                a.IconA = Util.getImage(weatherResult[25]);
                a.IconB = Util.getImage(weatherResult[26]);
                info.Weather5Days.Add(a);

                a = new WeatherInfoA();
                a.ToDay = weatherResult[27];
                a.T = weatherResult[28];
                a.Wind = weatherResult[29];
                a.IconA = Util.getImage(weatherResult[30]);
                a.IconB = Util.getImage(weatherResult[31]);
                info.Weather5Days.Add(a);
                if (mPro == null)
                    ReadXmlNodes();
                return info;
            }
            catch (Exception e)
            {
                XtraMessageBox.Show(e.Message);
                return null;
            }
        }
        private void ReadXmlNodes()
        {
            mPro = new List<Province>();
            string fileName = Application.StartupPath.Replace("\\bin\\Debug", "\\Resources\\" + "Area.xml");
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(fileName);
                XmlNodeList nodes_P = xmlDoc.SelectNodes("//Province");
                foreach (XmlNode node in nodes_P)
                {
                    Province p = new Province();
                    p.Name = node.Attributes["name"].Value;
                    XmlNodeList nodes_C = node.ChildNodes;
                    List<string> cNames = new List<string>();
                    foreach (XmlNode node0 in nodes_C)
                    {
                        string name = node0.InnerText;
                        cNames.Add(name);
                    }
                    p.Childs = cNames;
                    mPro.Add(p);
                }
                if (mPro.Count == 0)
                {
                    throw new Exception("获取位置名称失败！");
                }
            }
            catch (Exception e)
            {
                XtraMessageBox.Show(e.Message);
                return;
            }
        }
        private string getCity()
        {
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            try
            {
                string result = wc.DownloadString("http://ip.chinaz.com/getip.aspx");
                result = result.Substring(0, result.LastIndexOf(" "));
                result = result.Substring(result.LastIndexOf("'") + 1);
                return result;
            }
            catch (Exception e)
            {
                throw new CustomException(e.Message);
            }
            finally
            {
                wc.Dispose();
            }
        }
        
        private void cmb市_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cmb市.EditValue != null)
            {
                enableC();
                XtraFormP.showWait("请稍等", "正在加载天气信息");
                try
                {
                    mGetWeatherWithPara d = new mGetWeatherWithPara(getWeatherWithPara);
                    d.BeginInvoke(cmb市.EditValue.ToString(), new AsyncCallback(dCallBack), this);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message);
                }
            }
        }

        private void cmb省_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cmb省.EditValue != null && mPro.Count > 0)
            {
                for (int i = 0; i < mPro.Count; i++)
                {
                    if (mPro[i].Name.Equals(cmb省.EditValue.ToString()))
                    {
                        cmb市.Properties.Items.Clear();
                        cmb市.Properties.Items.AddRange(mPro[i].Childs);
                        cmb市.SelectedIndex = 0;
                        break;
                    }
                }
            }
        }
    }
}
