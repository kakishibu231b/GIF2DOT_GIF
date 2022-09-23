using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace GIF2DOT_GIF
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            System.Diagnostics.FileVersionInfo ver =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                   System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Text = this.Text + " " + ver.FileVersion;
        }

        /// <summary>
        /// 参照
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.FileName = "";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string[] strFileNames = openFileDialog1.FileNames;
                    set_textBox1(strFileNames);
                }
            }
            catch
            {

            }
            finally
            {

            }
        }

        /// <summary>
        /// ファイルドラッグアンドドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// ファイルドラッグアンドドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] strFileNames;
            strFileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            set_textBox1(strFileNames);
        }

        /// <summary>
        /// ファイル名設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void set_textBox1(string[] strFileNames)
        {
            string strTargetFileName = "";

            try
            {
                foreach (string strFileName in strFileNames)
                {
                    string strExtension = Path.GetExtension(strFileName);
                    strExtension = strExtension.ToUpper();
                    if (strExtension != ".GIF")
                    {
                        throw new InvalidOperationException("E0001");
                    }

                    if (!File.Exists(strFileName))
                    {
                        strTargetFileName = strFileName;
                        throw new InvalidOperationException("E0002");
                    }
                }

                string strTextBoxText = String.Join("\r\n", strFileNames);
                textBox1.Text = strTextBoxText;

                if (checkBox1.Checked)
                {
                    backgroundWorker1.RunWorkerAsync(strFileNames);
                    convertButton.Enabled = false;
                }
            }
            catch (InvalidOperationException exep)
            {
                this.Activate();

                switch (exep.Message)
                {
                    case "E0001":
                        MessageBox.Show(this, Properties.Resources.E00001);
                        break;
                    case "E0002":
                        MessageBox.Show(this, Properties.Resources.E00002 + strTargetFileName);
                        break;
                    default:
                        break;
                }
            }
            finally
            {

            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.CompareTo("") == 0)
            {
                convertButton.Enabled = false;
            }
            else
            {
                convertButton.Enabled = true;
            }
        }

        /// <summary>
        /// [変換]ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void convertButton_Click(object sender, EventArgs e)
        {
            string strTargetFileName = "";

            try
            {
                string strTextBoxText = textBox1.Text;
                string[] strSeparator = { "\r\n" };
                string[] strFileNames;
                strFileNames = strTextBoxText.Split(strSeparator, StringSplitOptions.None);
                foreach (string strFileNameIn in strFileNames)
                {
                    if (!File.Exists(strFileNameIn))
                    {
                        strTargetFileName = strFileNameIn;
                        throw new InvalidOperationException("E0002");
                    }
                }
                backgroundWorker1.RunWorkerAsync(strFileNames);
                convertButton.Enabled = false;
            }
            catch (InvalidOperationException exep)
            {
                switch (exep.Message)
                {
                    case "E0002":
                        MessageBox.Show(this, Properties.Resources.E00002 + strTargetFileName);
                        break;
                    default:
                        break;
                }
            }
            finally
            {

            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] strFileNames  = (string[])e.Argument;
            foreach (string strFileNameIn in strFileNames)
            {
                string strFileNameOut = Path.GetDirectoryName(strFileNameIn);
                strFileNameOut += "\\";
                strFileNameOut += Path.GetFileNameWithoutExtension(strFileNameIn);
                strFileNameOut += "_";
                strFileNameOut += numericUpDown1.Value.ToString();
                strFileNameOut += "x";
                strFileNameOut += numericUpDown1.Value.ToString();
                strFileNameOut += ".gif";

                saveFile(strFileNameIn, strFileNameOut);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (textBox1.Text.CompareTo("") == 0)
            {
                convertButton.Enabled = false;
            }
            else
            {
                convertButton.Enabled = true;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        /// <summary>
        /// 変換
        /// </summary>
        /// <param name="strFileNameIn"></param>
        /// <param name="strFileNameOut"></param>
        public void saveFile(string strFileNameIn, string strFileNameOut)
        {
            ImageConverter imageConverter = new ImageConverter();

            string strFileName = strFileNameIn;
            Image imgInputImage = null;
            imgInputImage = Image.FromFile(strFileName);

            var bmps = new List<MyGifEncorder.BitmapAndDelayTime>();

            FrameDimension fdimInputImage = new FrameDimension(imgInputImage.FrameDimensionsList[0]);
            int intFrameCount = imgInputImage.GetFrameCount(fdimInputImage);

            int intRowBlock = (int)numericUpDown1.Value;
            int intColBlock = (int)numericUpDown2.Value;

            for (int ii = 0; ii < intFrameCount; ++ii)
            {
                imgInputImage.SelectActiveFrame(fdimInputImage, ii);

                System.Drawing.Bitmap bitmap1 = new System.Drawing.Bitmap(imgInputImage);
                System.Drawing.Bitmap bitmap2 = To32bppArg(bitmap1);

                MyimageConverter.ImageConvert32bitToDot(bitmap2, intRowBlock, intColBlock);

                if (radioButton4.Checked)
                {
                    MyimageConverter.ImageConvert32bitToLine(bitmap2, (double)numericUpDown4.Value, (int)numericUpDown5.Value, (int)numericUpDown6.Value, (int)numericUpDown7.Value);
                }
                else if (radioButton2.Checked)
                {
                    bitmap2 = MyimageConverter.CreateGrayscaleImage(bitmap2);
                }

                Bitmap bitmap3 = MyimageConverter.ImageConvert32bitTo8bit(bitmap2);

                if (radioButton1.Checked)
                {
                    ;
                }
                else if (radioButton3.Checked)
                {
                    bitmap3 = MyimageConverter.Create1bppImage(bitmap3, (double)numericUpDown4.Value);
                }

                bmps.Add(new MyGifEncorder.BitmapAndDelayTime(bitmap3, (ushort)numericUpDown3.Value));
            }

            // 生成
            MyGifEncorder.SaveAnimatedGif(strFileNameOut, bmps, 0);
        }

        /// <summary>
        /// 複数の画像をGIFアニメーションとして保存する
        /// </summary>
        /// <param name="savePath">保存先のファイルのパス</param>
        /// <param name="imageFiles">GIFに追加する画像ファイルのパス</param>
        public static void CreateAnimatedGif(string savePath, string[] imageFiles)
        {
            //GifBitmapEncoderを作成する
            GifBitmapEncoder encoder = new GifBitmapEncoder();

            foreach (string f in imageFiles)
            {
                //画像ファイルからBitmapFrameを作成する
                Uri uri = new Uri(f, UriKind.RelativeOrAbsolute);
                BitmapFrame bmpFrame = BitmapFrame.Create(uri);

                //フレームに追加する
                encoder.Frames.Add(bmpFrame);
            }

            //書き込むファイルを開く
            FileStream outputFileStrm = new FileStream(savePath,
                FileMode.Create, FileAccess.Write, FileShare.None);
            //保存する
            encoder.Save(outputFileStrm);
            //閉じる
            outputFileStrm.Close();
        }

        static Bitmap To32bppArg(Bitmap bmp)
        {
            if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
            {
                return bmp;
            }
            else
            {
                var bmp2 = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);

                using (var g = Graphics.FromImage(bmp2))
                {
                    g.PageUnit = GraphicsUnit.Pixel;
                    g.DrawImageUnscaled(bmp, 0, 0);
                };
                return bmp2;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(numericUpDown1.Value + 10 > numericUpDown1.Maximum)
            {
                numericUpDown1.Value = numericUpDown1.Maximum;
            }
            else
            {
                numericUpDown1.Value += 10;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (numericUpDown1.Value - 10 < numericUpDown1.Minimum)
            {
                numericUpDown1.Value = numericUpDown1.Minimum;
            }
            else
            {
                numericUpDown1.Value -= 10;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (numericUpDown2.Value + 10 > numericUpDown2.Maximum)
            {
                numericUpDown2.Value = numericUpDown2.Maximum;
            }
            else
            {
                numericUpDown2.Value += 10;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (numericUpDown2.Value - 10 < numericUpDown2.Minimum)
            {
                numericUpDown2.Value = numericUpDown2.Minimum;
            }
            else
            {
                numericUpDown2.Value -= 10;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (numericUpDown3.Value + 10 > numericUpDown3.Maximum)
            {
                numericUpDown3.Value = numericUpDown3.Maximum;
            }
            else
            {
                numericUpDown3.Value += 10;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (numericUpDown3.Value - 10 < numericUpDown3.Minimum)
            {
                numericUpDown3.Value = numericUpDown3.Minimum;
            }
            else
            {
                numericUpDown3.Value -= 10;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = Color.FromArgb(0, (int)numericUpDown5.Value, (int)numericUpDown6.Value, (int)numericUpDown7.Value);

            if ( colorDialog1.ShowDialog() == DialogResult.OK )
            {
                numericUpDown5.Value = colorDialog1.Color.R;
                numericUpDown6.Value = colorDialog1.Color.G;
                numericUpDown7.Value = colorDialog1.Color.B;
            }
        }
    }
}
