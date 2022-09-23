using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GIF2DOT_GIF
{
    class MyimageConverter
    {
        /// <summary>
        /// 黒のみ抽出
        /// </summary>
        /// <param name="bmpSource"></param>
        /// <returns></returns>
        public static void ImageConvert32bitToLine(Bitmap bmpSource, double dblBrightness, int intRed, int intGreen, int intBlue)
        {
            Color colorTransparent = Color.FromArgb(255, 255-intRed, 255-intGreen, 255-intBlue);
            Color colorLine = Color.FromArgb(0, intRed, intGreen, intBlue);
            for (int row = 0; row < bmpSource.Height; ++row)
            {
                for (int col = 0; col < bmpSource.Width; ++col)
                {
                    Color color1 = bmpSource.GetPixel(col, row);

                    if (color1.A == 255 && color1.R == 0 && color1.G == 0 && color1.B == 0)
                    {
                        bmpSource.SetPixel(col, row, colorTransparent);
                    }
                    else if (color1.GetBrightness() < dblBrightness)
                    {
                        bmpSource.SetPixel(col, row, colorLine);
                    }
                    else
                    {
                        bmpSource.SetPixel(col, row, colorTransparent);
                    }
                }
            }
        }

        /// <summary>
        /// 指定した画像からグレースケール画像を作成する
        /// </summary>
        /// <param name="img">基の画像</param>
        /// <returns>作成されたグレースケール画像</returns>
        public static Bitmap CreateGrayscaleImage(Bitmap img)
        {
            //グレースケールの描画先となるImageオブジェクトを作成
            Bitmap newImg = new Bitmap(img.Width, img.Height);
            //newImgのGraphicsオブジェクトを取得
            Graphics g = Graphics.FromImage(newImg);

            //ColorMatrixオブジェクトの作成
            //グレースケールに変換するための行列を指定する
            System.Drawing.Imaging.ColorMatrix cm =
                new System.Drawing.Imaging.ColorMatrix(
                    new float[][]{
                    new float[]{0.3086f, 0.3086f, 0.3086f, 0 ,0},
                    new float[]{0.6094f, 0.6094f, 0.6094f, 0, 0},
                    new float[]{0.0820f, 0.0820f, 0.0820f, 0, 0},
                    new float[]{0, 0, 0, 1, 0},
                    new float[]{0, 0, 0, 0, 1}
                });
            //ImageAttributesオブジェクトの作成
            System.Drawing.Imaging.ImageAttributes ia =
                new System.Drawing.Imaging.ImageAttributes();
            //ColorMatrixを設定する
            ia.SetColorMatrix(cm);

            //ImageAttributesを使用してグレースケールを描画
            g.DrawImage(img,
                new Rectangle(0, 0, img.Width, img.Height),
                0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

            //リソースを解放する
            g.Dispose();

            return newImg;
        }

        /// <summary>
        /// 画像ドット変換
        /// </summary>
        /// <param name="bmpSource"></param>
        /// <returns></returns>
        public static void ImageConvert32bitToDot(Bitmap bmpSource, int intRowBlock, int intColBlock)
        {
            Color colorTransparent = Color.FromArgb(255, 0, 0, 0);
            for (int row = 0; row < bmpSource.Height; ++row)
            {
                for (int col = 0; col < bmpSource.Width; ++col)
                {
                    Color color1 = bmpSource.GetPixel(col, row);
                    if (color1.A == 0 && color1.R == 0 && color1.G == 0 && color1.B == 0)
                    {
                        bmpSource.SetPixel(col, row, colorTransparent);
                    }
                }
            }

            for (int row = 0; row < bmpSource.Height; ++row)
            {
                for (int col = 0; col < bmpSource.Width; ++col)
                {
                    Color color1 = bmpSource.GetPixel(col, row);
                    bool bolIsColorTransparent = true;
                    for (int row2 = row; row2 < row + intRowBlock && row2 < bmpSource.Height; ++row2)
                    {
                        for (int col2 = col; col2 < col + intColBlock && col2 < bmpSource.Width; ++col2)
                        {
                            Color color5 = bmpSource.GetPixel(col2, row2);
                            if (color5.A == colorTransparent.A
                                && color5.R == colorTransparent.R
                                && color5.G == colorTransparent.G
                                && color5.B == colorTransparent.B)
                            {
                                ;
                            }
                            else
                            {
                                if (bolIsColorTransparent)
                                {
                                    bolIsColorTransparent = false;
                                    color1 = color5;
                                    break;
                                }
                            }
                        }
                        if (bolIsColorTransparent == false)
                        {
                            break;
                        }
                    }

                    if (bolIsColorTransparent)
                    {

                    }
                    else
                    {
                        for (int row2 = row; row2 < row + intRowBlock && row2 < bmpSource.Height; ++row2)
                        {
                            for (int col2 = col; col2 < col + intColBlock && col2 < bmpSource.Width; ++col2)
                            {
                                bmpSource.SetPixel(col2, row2, color1);
                            }
                        }
                    }
                    col += intColBlock - 1;
                }
                row += intRowBlock - 1;
            }
        }

        /// <summary>
        /// 32bpp→8bpp変換
        /// </summary>
        /// <param name="bmpSource">32bpp画像</param>
        /// <returns>8bpp画像</returns>
        public static Bitmap ImageConvert32bitTo8bit(Bitmap bmpSource)
        {
            Bitmap bmp8bit = new Bitmap(bmpSource.Width, bmpSource.Height, PixelFormat.Format8bppIndexed);

            BitmapData bmpData8bit = bmp8bit.LockBits(
                new Rectangle(0, 0, bmp8bit.Width, bmp8bit.Height),
                ImageLockMode.WriteOnly,
                bmp8bit.PixelFormat
            );

            BitmapData bmpData32bit = bmpSource.LockBits(
                new Rectangle(0, 0, bmpSource.Width, bmpSource.Height),
                ImageLockMode.ReadOnly,
                bmpSource.PixelFormat
            );

            byte[] imgBuf8bit = new byte[bmpData8bit.Stride * bmp8bit.Height];
            byte[] imgBuf32bit = new byte[bmpData32bit.Stride * bmpSource.Height];

            Marshal.Copy(bmpData32bit.Scan0, imgBuf32bit, 0, imgBuf32bit.Length);

            var colors = new Dictionary<Color, byte>();
            byte colorIndex = 0;

            for (int y = 0; y < bmpSource.Height; y++)
            {
                for (int x = 0; x < bmpSource.Width; x++)
                {
                    int index32bit = y * bmpData32bit.Stride + x * 4;
                    int index8bit = y * bmpData8bit.Stride + x;
                    byte b = imgBuf32bit[index32bit];
                    byte g = imgBuf32bit[index32bit + 1];
                    byte r = imgBuf32bit[index32bit + 2];

                    // 32bitから8bitへ
                    var color = Color.FromArgb(r, g, b);
                    if (colors.TryGetValue(color, out byte i))
                    {
                        imgBuf8bit[index8bit] = i;
                    }
                    else
                    {
                        imgBuf8bit[index8bit] = colorIndex;
                        colors.Add(color, colorIndex);
                        colorIndex++;
                    }
                }
            }
            Marshal.Copy(imgBuf8bit, 0, bmpData8bit.Scan0, imgBuf8bit.Length);
            bmpSource.UnlockBits(bmpData32bit);
            bmp8bit.UnlockBits(bmpData8bit);

            var pal = bmp8bit.Palette;
            foreach (var item in colors)
                pal.Entries[item.Value] = item.Key;
            bmp8bit.Palette = pal;

            return bmp8bit;
        }

        /// <summary>
        /// 指定された画像から1bppのイメージを作成する
        /// </summary>
        /// <param name="img">基になる画像</param>
        /// <returns>1bppに変換されたイメージ</returns>
        public static Bitmap Create1bppImage(Bitmap img, double dblBrightness)
        {
            //1bppイメージを作成する
            Bitmap newImg = new Bitmap(img.Width, img.Height,
                PixelFormat.Format1bppIndexed);

            //Bitmapをロックする
            BitmapData bitmapData = newImg.LockBits(
                new Rectangle(0, 0, newImg.Width, newImg.Height),
                ImageLockMode.WriteOnly, newImg.PixelFormat);

            //新しい画像のピクセルデータを作成する
            byte[] pixels = new byte[bitmapData.Stride * bitmapData.Height];
            for (int y = 0; y < bitmapData.Height; y++)
            {
                for (int x = 0; x < bitmapData.Width; x++)
                {
                    //明るさが指定以上の時は白くする
                    if (dblBrightness <= img.GetPixel(x, y).GetBrightness())
                    {
                        //ピクセルデータの位置
                        int pos = (x >> 3) + bitmapData.Stride * y;
                        //白くする
                        pixels[pos] |= (byte)(0x80 >> (x & 0x7));
                    }
                }
            }
            //作成したピクセルデータをコピーする
            IntPtr ptr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);

            //ロックを解除する
            newImg.UnlockBits(bitmapData);

            return newImg;
        }

    }
}
