﻿using System;
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
using System.Threading.Tasks;
using Fiddler;

namespace KanColleDbPost
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
        }

        public enum UrlType
        {
            SHIP2,
            DECK,
            DECK_PORT,
            BASIC,
            CREATESHIP,
            GETSHIP,
            CREATEITEM,
            START,
            NEXT,
            BATTLE,
            BATTLE_MIDNIGHT,
            BATTLE_SP_MIDNIGHT,
            BATTLE_NIGHT_TO_DAY,
            BATTLERESULT,
            PRACTICE_BATTLE,
            PRACTICE_BATTLERESULT,
            MASTER_FURNITURE,
            MASTER_MAPAREA,
            MASTER_MAPCELL,
            MASTER_MAPINFO,
            MASTER_MISSION,
            MASTER_SHIP,
            MASTER_SLOTITEM,
            MASTER_STYPE,
            MASTER_USEITEM,
        };

        public Dictionary<UrlType, string> urls = new Dictionary<UrlType, string>()
        {
            { UrlType.SHIP2,                    "api_get_member/ship2"                },
            { UrlType.DECK,                     "api_get_member/deck"                 },
            { UrlType.DECK_PORT,                "api_get_member/deck_port"            },
            { UrlType.BASIC,                    "api_get_member/basic"                },
            { UrlType.CREATESHIP,               "api_req_kousyou/createship"          },
            { UrlType.GETSHIP,                  "api_req_kousyou/getship"             },
            { UrlType.CREATEITEM,               "api_req_kousyou/createitem"          },
            { UrlType.START,                    "api_req_map/start"                   },
            { UrlType.NEXT,                     "api_req_map/next"                    },
            { UrlType.BATTLE,                   "api_req_sortie/battle"               },
            { UrlType.BATTLE_MIDNIGHT,          "api_req_battle_midnight/battle"      },
            { UrlType.BATTLE_SP_MIDNIGHT,       "api_req_battle_midnight/sp_midnight" },
            { UrlType.BATTLE_NIGHT_TO_DAY,      "api_req_sortie/night_to_day"         },
            { UrlType.BATTLERESULT,             "api_req_sortie/battleresult"         },
            { UrlType.PRACTICE_BATTLE,          "api_req_practice/battle"             },
            { UrlType.PRACTICE_BATTLERESULT,    "api_req_practice/battle_result"      },
            //{ UrlType.MASTER_FURNITURE,         "api_get_master/furniture"            },
            //{ UrlType.MASTER_MAPAREA,           "api_get_master/maparea"              },
            //{ UrlType.MASTER_MAPCELL,           "api_get_master/mapcell"              },
            //{ UrlType.MASTER_MAPINFO,           "api_get_master/mapinfo"              },
            //{ UrlType.MASTER_MISSION,           "api_get_master/mission"              },
            //{ UrlType.MASTER_SHIP,              "api_get_master/ship"                 },
            //{ UrlType.MASTER_SLOTITEM,          "api_get_master/slotitem"             },
            //{ UrlType.MASTER_STYPE,             "api_get_master/stype"                },
            //{ UrlType.MASTER_USEITEM,           "api_get_master/useitem"              },
        };
        
        void FiddlerApplication_AfterSessionComplete(Fiddler.Session oSession)
        {
            Task.Factory.StartNew(() =>
            {
                string url = oSession.fullUrl;
                if (url.IndexOf("/kcsapi/") <= 0)
                {
                    if (checkBox1.Checked)
                    {
                        AppendText(url + "\n");
                    }
                    return;
                }

                foreach (KeyValuePair<UrlType, string> kvp in urls)
                {
                    if (url.IndexOf(kvp.Value) > 0)
                    {
                        string responseBody = oSession.GetResponseBodyAsString();
                        responseBody.Replace("svdata=", "");

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
            });
        }

        private string PostServer(Session oSession)
        {
            string token = textBox2.Text;                   // TODO: ユーザー毎のトークンを設定
            string agent = "********************";          // TODO: アプリ毎のトークンを設定
            string url = oSession.fullUrl;
            string requestBody = oSession.GetRequestBodyAsString();
            string responseBody = oSession.GetResponseBodyAsString();
            responseBody.Replace("svdata=", "");

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
                return oSession.responseCode + ": " + response;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse error = (HttpWebResponse)ex.Response;
                    return error.ResponseUri + " " + oSession.responseCode + ": " + error.StatusDescription;
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
            if( button1.Text == "開始" )
            {
                FiddlerApplication.Startup(8877, true, false);
                AppendText("----- Capture start\n");
                button1.Text = "停止";
            }
            else
            {
                AppendText("----- Capture stop\n");
                FiddlerApplication.Shutdown();
                button1.Text = "開始";
            }
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
            global::KanColleDbPost.Properties.Settings.Default.Save();
        }
    }
}
