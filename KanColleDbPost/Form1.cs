using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Web;
using Fiddler;

namespace KanColleDb
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Fiddler.FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
        }

        void FiddlerApplication_AfterSessionComplete(Fiddler.Session oSession)
        {
            string[] keys = new string[]
            {
		        "api_req_kousyou/createship",
		        "api_req_kousyou/getship",
		        "api_req_kousyou/createitem",
		        "api_get_member/basic",
		        "api_get_member/deck",
		        "api_get_member/deck_port",
                "api_get_member/ship2",
		        "api_req_map/start",
		        "api_req_map/next",
		        "api_req_sortie/battle",
		        "api_req_battle_midnight/battle",
		        "api_req_sortie/battleresult",
		        "api_req_practice/battle",
		        "api_req_practice/battle_result"
            };
            string url = oSession.fullUrl;
            if( url.IndexOf("/kcsapi/") <= 0 )
            {
                if (checkBox1.Checked)
                {
                    AppendText(url + "\n");
                }
                return;
            }
            foreach (string key in keys)
            {
                if (url.IndexOf(key) > 0)
                {
                    string str = "Post server from " + url + "\n";
                    AppendText(str);

                    string res = PostServer(oSession);
                    str = "Post response : " + res + "\n";
                    AppendText(str);
                    return;
                }
            }
            if (checkBox1.Checked)
            {
                AppendText(url + "\n");
                }
        }

        private string PostServer(Fiddler.Session oSession)
        {
            string token = textBox2.Text;                   // TODO: ユーザー毎のトークンを設定
            string agent = "********************";          // TODO: アプリ毎のトークンを設定
            string url = oSession.fullUrl;
            string requestBody = oSession.GetRequestBodyAsString();
            string responseBody = oSession.GetResponseBodyAsString();

            try
            {
                WebRequest req = WebRequest.Create("http://api.kancolle-db.net/1/");
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                System.Text.Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                string postdata =
                      "token=" + HttpUtility.UrlEncode(token) + "&"
                    + "agent=" + HttpUtility.UrlEncode(agent) + "&"
                    + "url=" + HttpUtility.UrlEncode(url) + "&"
                    + "requestbody=" + HttpUtility.UrlEncode(requestBody) + "&"
                    + "responsebody=" + HttpUtility.UrlEncode(responseBody);
                byte[] postDataBytes = System.Text.Encoding.ASCII.GetBytes(postdata);
                req.ContentLength = postDataBytes.Length;

                Stream reqStream = req.GetRequestStream();
                reqStream.Write(postDataBytes, 0, postDataBytes.Length);
                reqStream.Close();

                WebResponse res = req.GetResponse();
                HttpWebResponse httpRes = (HttpWebResponse)res;
                Stream resStream = res.GetResponseStream();
                StreamReader sr = new StreamReader(resStream, enc);
                string response = sr.ReadToEnd();
                sr.Close();
                return httpRes.StatusCode + ": " + response;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse error = (HttpWebResponse)ex.Response;
                    return error.ResponseUri + " " + error.StatusCode + ": " + error.StatusDescription;
                }
                return ex.Message;
            }
        }
        
        /// <summary>
        /// キャプチャ開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Fiddler.FiddlerApplication.Startup(8877, true, false);
            AppendText("----- Capture start\n");
        }

        /// <summary>
        /// キャプチャ終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            AppendText("----- Capture stop\n");
            Fiddler.FiddlerApplication.Shutdown();
        }

        // Windowsフォームコントロールに対して非同期な呼び出しを行うためのデリゲート
        delegate void SetTextCallback(string text);

        private void AppendText(string text)
        {
            // 呼び出し元のコントロールのスレッドが異なるか確認をする
            if (this.textBox1.InvokeRequired)
            {
                // 同一メソッドへのコールバックを作成する
                SetTextCallback delegateMethod = new SetTextCallback(AppendText);

                // コントロールの親のInvoke()メソッドを呼び出すことで、呼び出し元の
                // コントロールのスレッドでこのメソッドを実行する
                this.Invoke(delegateMethod, new object[] { text });
            }
            else
            {
                // コントロールを直接呼び出す
                this.textBox1.AppendText(text);
                this.textBox1.SelectionStart = textBox1.Text.Length;
                this.textBox1.ScrollToCaret();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Fiddlerのシャットダウン
            Fiddler.FiddlerApplication.Shutdown();
        }
    }
}
