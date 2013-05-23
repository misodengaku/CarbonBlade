using AsyncOAuth;
using Codeplex.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Sambhala
{
    public partial class Form1 : Form
    {
        const string consumerKey = "SFC0AFRZ7p22OeISr42dA", consumerSecret = "bmAz5tzvMGgPYUTw8BhPlsdpQ0Wplgra268Jwk0NQ", fileName = "asahara.xml";
        //string accessTokenKey, accessTokenSecret;
        AccessToken accessToken = null;
        Salvation setting;
        //string user = "";
        int count = 0;
        //IDisposable hStream;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Salvation));
                FileStream fs = new FileStream(fileName, System.IO.FileMode.Open);
                setting = (Salvation)serializer.Deserialize(fs);
                fs.Close();
                accessToken = new AccessToken(setting.Key, setting.Secret);
                toolStripStatusLabel1.Text = "認証済みです";
                ReloadList();
            }
            catch
            {
                setting = new Salvation();
                string[] row_1 = { "救済は？", "成功する" };
                listView1.Items.Add(new ListViewItem(row_1));
            }


        }

        #region ﾒｿｯﾖ

        private async void Post(string text, string inReplyTo = "")
        {
            var client = new TwitterClient(consumerKey, consumerSecret, accessToken);

            for (var i = count; i > 0; i--)
                text += "‍";

            var result = await client.PostUpdate(text);
            while (result.Contains("Status is a duplicate"))
            {
                text += "‍";
                result = await client.PostUpdate(text);
            }

            count++;
            if (count >= 10)
                count = 0;
        }

        private void SaveSetting(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Salvation));
            FileStream fs = new FileStream(fileName, System.IO.FileMode.OpenOrCreate);
            serializer.Serialize(fs, obj);
            fs.Close();
        }

        private void ReloadList()
        {
            listView1.Items.Clear();
            foreach (var item in setting.Words)
            {
                string[] row = { item.MatchWord, item.PostWord };
                listView1.Items.Add(new ListViewItem(row));
            }
        }

        #endregion

        #region ｲﾍﾞﾝﾖﾊﾝﾖﾔ

        private async void connectUserStreamButton_Click(object sender, EventArgs e)
        {
            var client = new TwitterClient(consumerKey, consumerSecret, accessToken);
            await client.GetStream(async x =>
            {
                if (x == "")
                    return;
                var serializer = new DataContractJsonSerializer(typeof(TweetData));
                object obj;
                try
                {
                    if (x.Contains("{\"delete\":"))
                    {
                        Post("ツイ消しを見た");
                    }
                    else
                    {
                        obj = serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(x)));
                        var item = (TweetData)obj;
                        if (item.Text != null)
                        {
                            if (item.RetweetedStatus != null)
                            {
                                //item.isRetweetStatus = true;
                                return;
                            }
                            //MessageBox.Show(item.Text);
                            foreach (var word in setting.Words)
                            {
                                if (item.Text.Contains(word.MatchWord))
                                {
                                    var s = await client.Favorite(item.ID);
                                    Post(word.PostWord);
                                }
                            }
                        }
                    }
                }
                catch { }
            });
            #region 残骸
            // subscribe async stream
            /*var cancel = client.GetStream()
                .Skip(1)
                .Subscribe(async x =>
                {
                    var json = DynamicJson.Parse(x);
                    if (json.text())
                    {
                        if (json.text.Contains("救済は"))
                        {
                            var text = "成功する";
                            var result = await client.PostUpdate(text);
                            while (result.Contains("Status is a duplicate"))
                            {
                                text += "‍";
                                result = await client.PostUpdate(text);
                            }
                        }
                    }

                });*/

            #endregion

            return;
            //cancel.Dispose(); // キャンセルはDisposeで行う
        }

        private async void authMenuItem_Click(object sender, EventArgs e)
        {
            var url = await TwitterClient.GetAuthorizeUrl(consumerKey, consumerSecret);
            
            Process.Start(url);

            MessageBox.Show("ブラウザで認証後、メニューバー上のテキストボックスにPINコードを入力し、「認証」をクリックしてください");

            return;
        }

        private async void pinInputMenuItem_Click(object sender, EventArgs e)
        {
            if (pinBox.Text == "")
            {
                MessageBox.Show("PINコードを入力してください");
                return;
            }

            var pinCode = pinBox.Text;
            var accessTokenResponse = await TwitterClient.GetAccessToken(consumerKey, consumerSecret, pinCode);
            accessToken = new AccessToken(accessTokenResponse.Key, accessTokenResponse.Secret);
            setting.Key = accessToken.Key;
            setting.Secret = accessToken.Secret;
            try
            {
                SaveSetting(setting);
                toolStripStatusLabel1.Text = "認証済みです";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return;
        }

        private async void carbonPostButton_Click(object sender, EventArgs e)
        {
            var client = new TwitterClient(consumerKey, consumerSecret, accessToken);
            string text = "かつて瞑想修行中に麻原の妻である松本知子から「アーナンダは眠っている」としつこく注意されたが、「私は起きている」と反論すると、麻原からカーボン製の竹刀で打擲され、一週間ほど立つのにも苦労した経験があったことを公判中に語った。";

            var result = await client.PostUpdate(text);
            while (result.Contains("Status is a duplicate"))
            {
                text += "‍";
                result = await client.PostUpdate(text);
            }

        }

        private void addSalvationWord_Click(object sender, EventArgs e)
        {
            if (matchWord.Text != "" && ReplyWord.Text != "")
            {
                setting.Words.Add(new Word(matchWord.Text, ReplyWord.Text));
                SaveSetting(setting);
                ReloadList();
            }

        }

        private void deleteSalvationWord_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices[0] == -1)
                return;
            listView1.Items.RemoveAt(listView1.SelectedIndices[0]);
            setting.Words.RemoveAt(listView1.SelectedIndices[0]);
            SaveSetting(setting);
        }

        #endregion
    }
}
