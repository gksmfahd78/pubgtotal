#define DEBUG

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using Tesseract;
using System.Threading;

namespace pubg_total
{
    public partial class main : Form
    {
        private static int PLATFORM = 0;
        private static string jsonURL = "https://yhs.kr/api/PUBG/";

        private int _refX = 0;
        private int _refY = 0;
        private int _imgW = 0;
        private int _imgH = 0;

        private Bitmap bitmap;

        private List<string> nicknames = new List<string>();

        private string textBoxStr;

        public main()
        {
            InitializeComponent();
        }

        private void main_Load(object sender, EventArgs e)
        {
            infiniteScreenCaptrue();
        }

        private Mat bitmap2Mat(Bitmap bitmap)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
        }

        private Bitmap mat2Bitmap(Mat mat)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        }

        private void ImgCapture(int refX = 0, int refY = 0, int imgW = 0, int imgH = 0)
        {
            _refX = refX;
            _refY = refY;
            _imgW = imgW;
            _imgH = imgH;
        }

        private void searchPlayer(string nickname)
        {
            string playerID = getPlayerID(nickname);

            if (playerID != null)
            {

                if (nicknames.Count == 0)
                {
                    nicknames.Add(nickname);
                    textBoxStr += nickname + "\n\n" + getPlayerNormalTotal(playerID) + "\n\n\n" + getPlayerRankedTotal(playerID) + "\n\n\n";
                    textBox.AppendText(textBoxStr);
                }

                else
                {
                    for (int i = 0; i < nicknames.Count; i++)
                    {
                        if (nickname != nicknames[i])
                        {
                            if (i == nicknames.Count - 1)
                            {
                                nicknames.Add(nickname);
                                textBoxStr += nickname + "\n\n" + getPlayerNormalTotal(playerID) + "\n\n\n" + getPlayerRankedTotal(playerID) + "\n\n\n";
                                textBox.AppendText(textBoxStr);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            else
            {
                textBoxStr += nickname + " total : \n\n NAN";
                textBox.AppendText(textBoxStr);
            }
        }

        private void infiniteScreenCaptrue()
        {
#if DEBUG
            searchPlayer("QR_LEE");
#else
            ImgCapture(1700, 120, 220, 60);

            for(; ; )
            {
                System.Drawing.Size mSize = new System.Drawing.Size(_imgW, _imgH);
                Bitmap bitmap = new Bitmap(_imgW, _imgH);
                Graphics graphics = Graphics.FromImage(bitmap);
                Mat dst = new Mat();

                try
                {
                    graphics.CopyFromScreen(_refX, _refY, 0, 0, mSize);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }

                Cv2.Resize(bitmap2Mat(bitmap), dst, new OpenCvSharp.Size(320, 120));
                //Cv2.ImShow("test", bitmap2Mat(bitmap));
                Cv2.WaitKey(10);
                Pix pix = PixConverter.ToPix(mat2Bitmap(dst));
                var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
                var result = engine.Process(pix);
                //Console.WriteLine(result.GetText());
                searchPlayer(result.GetText().Trim());

                Thread.Sleep(1000);
            }
#endif
        }

        private string getPlayerID(string nickname)
        {
            using (WebClient wc = new WebClient())
            {
                string param = "?platform=" + PLATFORM + "&nickname=" + nickname;
                try {
                    string json = wc.DownloadString(jsonURL + "player/" + param);
                    JObject jObject = JObject.Parse(json);
                    string id = jObject["id"].ToString();
                    return id;
                }
                catch (WebException we) 
                {
                    return null;
                }

            }
        }

        private string getPlayerNormalTotal(string id)
        {
            using (WebClient wc = new WebClient())
            {
                string param = "?id=" + id;
                string json = wc.DownloadString(jsonURL + "normal/" + param);
                try
                {
                    string result = "";
                    float damageDealt, roundsPlayed;
                    JObject jObject = JObject.Parse(json);
                    result += "[NORMAL]\n";
                    result += "solo round played : " + jObject["gameMode"]["solo"]["roundsPlayed"].ToString() + "\n";
                    result += "solo kd : " + jObject["gameMode"]["solo"]["KD_point"].ToString() + "\n";
                    damageDealt = (float.Parse(jObject["gameMode"]["solo"]["damageDealt"].ToString()));
                    roundsPlayed = (float.Parse(jObject["gameMode"]["solo"]["roundsPlayed"].ToString()));
                    result += "solo damge dealt : " + damageDealt / roundsPlayed + "\n";
                    result += "duo round played : " + jObject["gameMode"]["duo"]["roundsPlayed"].ToString() + "\n";
                    result += "duo kd : " + jObject["gameMode"]["duo"]["KD_point"].ToString() + "\n";
                    damageDealt = (float.Parse(jObject["gameMode"]["duo"]["damageDealt"].ToString()));
                    roundsPlayed = (float.Parse(jObject["gameMode"]["duo"]["roundsPlayed"].ToString()));
                    result += "duo damge dealt : " + damageDealt / roundsPlayed + "\n";
                    result += "squad round played : " + jObject["gameMode"]["squad"]["roundsPlayed"].ToString() + "\n";
                    result += "squad kd : " + jObject["gameMode"]["squad"]["KD_point"].ToString() + "\n";
                    damageDealt = (float.Parse(jObject["gameMode"]["squad"]["damageDealt"].ToString()));
                    roundsPlayed = (float.Parse(jObject["gameMode"]["squad"]["roundsPlayed"].ToString()));
                    result += "squad damge dealt : " + damageDealt / roundsPlayed;

                    return result;
                }

                catch (WebException we)
                {
                    return null;
                }
            }
        }

        private string getPlayerRankedTotal(string id)
        {
            using (WebClient wc = new WebClient())
            {
                string param = "?id=" + id;
                string json = wc.DownloadString(jsonURL + "ranked/" + param);
                try
                {
                    string result = "";
                    float damageDealt, roundsPlayed;
                    JObject jObject = JObject.Parse(json);
                    result += "[Ranked]\n";
                    result += "solo current ranked : " + jObject["gameMode"]["solo"]["currentRank"]["tier"].ToString() + " "
                            + " " + jObject["gameMode"]["squad"]["currentRank"]["subTier"].ToString() + "\n" ;
                    result += "solo kd : " + jObject["gameMode"]["solo"]["KD_point"].ToString() + "\n";
                    damageDealt = (float.Parse(jObject["gameMode"]["solo"]["damageDealt"].ToString()));
                    roundsPlayed = (float.Parse(jObject["gameMode"]["squad"]["roundsPlayed"].ToString()));
                    result += "solo damge dealt : " + damageDealt/roundsPlayed + "\n";
                    result += "squad current ranked : " + jObject["gameMode"]["squad"]["currentRank"]["tier"].ToString() 
                            + " " + jObject["gameMode"]["squad"]["currentRank"]["subTier"].ToString() + "\n";
                    result += "squad kd : " + jObject["gameMode"]["squad"]["KD_point"].ToString() + "\n";
                    damageDealt = (float.Parse(jObject["gameMode"]["squad"]["damageDealt"].ToString()));
                    roundsPlayed = (float.Parse(jObject["gameMode"]["squad"]["roundsPlayed"].ToString()));
                    result += "squad damge dealt : " + damageDealt/roundsPlayed;

                    return result;
                }

                catch (WebException we)
                {
                    return null;
                }
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
