using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ps
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Bitmap bitmap1, lbitmap1, bitmap2, nBitmap2, copyBitmap;
        private Bitmap picture1, picture2;
        private Graphics graphics1, graphics2;
        string directory1, filename1, directory2, filename2;
        private int lx=-1, ly=-1, rx=-1, ry=-1;
        private Bitmap[] cacheBitmap = new Bitmap[13];

        private bool loadImage(ref string directory, ref string filename, ref Bitmap bitmap)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PNG图片(*.png)|*.png";
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(directory))
            {
                dialog.InitialDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                dialog.InitialDirectory = directory;
                dialog.FileName = filename;
            }
            dialog.AddExtension = true;
            dialog.DefaultExt = ".png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap _bitmap = (Bitmap)Image.FromFile(dialog.FileName);
                    if (_bitmap.Width % 32 != 0 || _bitmap.Height % 32 != 0)
                    {
                        MessageBox.Show("目标图片的宽或高不是32的倍数！", "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _bitmap.Dispose();
                        return false;
                    }
                    // bitmap = _bitmap;
                    bitmap = new Bitmap(_bitmap.Width, _bitmap.Height);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.DrawImage(_bitmap, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), 0, 0, _bitmap.Width, _bitmap.Height, GraphicsUnit.Pixel);
                    graphics.Dispose();
                    _bitmap.Dispose();
                    directory = Path.GetDirectoryName(dialog.FileName);
                    filename = Path.GetFileName(dialog.FileName);
                    return true;
                }
                catch (Exception)
                {
                    MessageBox.Show("加载失败！", "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return false;
        }

        private Bitmap cloneBitmap(Bitmap bitmap)
        {
            return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loadImage(ref directory1, ref filename1, ref bitmap1))
            {
                lbitmap1 = null;
                picture1 = cloneBitmap(bitmap1);
                graphics1 = Graphics.FromImage(picture1);
                lx = ly = -1;
                pictureBox1.Image = picture1;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (loadImage(ref directory2, ref filename2, ref bitmap2))
            {
                for (int i = 0; i < 13; i++)
                {
                    if (cacheBitmap[i] != null)
                        cacheBitmap[i].Dispose();
                    cacheBitmap[i] = null;
                }
                nBitmap2 = cloneBitmap(bitmap2);
                picture2 = cloneBitmap(bitmap2);
                graphics2 = Graphics.FromImage(picture2);
                rx = ry = -1;
                pictureBox2.Image = picture2;
                trackBar1.Value = 0;
                trackBar1.Enabled = true;
                new Thread(() =>
                {
                    for (int i = 0; i < 13; i++)
                        cacheBitmap[i] = calBitmap(bitmap2, i);
                }).Start();
            }
        }

        private void drawBorder()
        {
            resetImage();
            Pen pen = new Pen(Color.Crimson, 3);
            pen.Alignment = PenAlignment.Inset;
            if (lx >= 0 && ly >= 0)
            {
                graphics1.DrawRectangle(pen, 32 * lx, 32 * ly, 32, 32);
                pictureBox1.Image = picture1;
            }
            if (rx >= 0 && ry >= 0)
            {
                graphics2.DrawRectangle(pen, 32 * rx, 32 * ry, 32, 32);
                pictureBox2.Image = picture2;
            }
            pen.Dispose();
        }

        private void resetImage(int n=0)
        {
            if (bitmap1 != null)
            {
                picture1 = cloneBitmap(bitmap1);
                graphics1 = Graphics.FromImage(picture1);
                pictureBox1.Image = picture1;
            }
            if (nBitmap2 != null)
            {
                picture2 = cloneBitmap(nBitmap2);
                graphics2 = Graphics.FromImage(picture2);
                pictureBox2.Image = picture2;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (picture1 == null)
            {
                MessageBox.Show("没有图片！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            lbitmap1 = cloneBitmap(bitmap1);
            Bitmap nBitmap = new Bitmap(bitmap1.Width, bitmap1.Height + 32, bitmap1.PixelFormat);
            Graphics graphics = Graphics.FromImage(nBitmap);
            graphics.DrawImage(bitmap1, 0, 0);
            graphics.Dispose();
            bitmap1 = cloneBitmap(nBitmap);
            drawBorder();
        }

        private bool saveImage(ref string directory, ref string filename, Bitmap bitmap)
        {
            if (bitmap == null)
            {
                MessageBox.Show("没有图片！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            SaveFileDialog dialog=new SaveFileDialog();
            dialog.InitialDirectory = directory;
            dialog.FileName = filename;
            dialog.Filter = "PNG图片(*.png)|*.png";
            dialog.AddExtension = true;
            dialog.DefaultExt = ".png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bitmap.Save(dialog.FileName);
                    directory = Path.GetDirectoryName(dialog.FileName);
                    filename = Path.GetFileName(dialog.FileName);
                    MessageBox.Show("图片已保存至" + dialog.FileName, "保存成功！", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return true;
                }
                catch (Exception)
                {
                    MessageBox.Show("保存失败！", "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
               
            }
            return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveImage(ref directory1, ref filename1, bitmap1))
            {
                lbitmap1 = null;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (bitmap1 == null) return;
            rx = ry = -1;
            lx = ((MouseEventArgs)e).X/32;
            ly = ((MouseEventArgs)e).Y/32;
            drawBorder();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (bitmap2 == null) return;
            lx = ly = -1;
            rx = ((MouseEventArgs)e).X / 32;
            ry = ((MouseEventArgs)e).Y / 32;
            drawBorder();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)
            {
                // 复制
                if (lx >= 0 && ly >= 0)
                {
                    copyBitmap = bitmap1.Clone(new Rectangle(32 * lx, 32 * ly, 32, 32), bitmap1.PixelFormat);
                    pictureBox3.Image = copyBitmap;
                }
                if (rx >= 0 && ry >= 0)
                {
                    copyBitmap = nBitmap2.Clone(new Rectangle(32 * rx, 32 * ry, 32, 32), nBitmap2.PixelFormat);
                    pictureBox3.Image = copyBitmap;
                }
            }
            if (e.KeyCode == Keys.V && e.Control && copyBitmap!=null)
            {
                // 替换
                if (lx >= 0 && ly >= 0)
                {
                    lbitmap1 = cloneBitmap(bitmap1);
                    Graphics graphics=Graphics.FromImage(bitmap1);
                    graphics.Clip = new Region(new Rectangle(32*lx, 32*ly, 32, 32));
                    graphics.Clear(Color.Transparent);
                    graphics.DrawImage(copyBitmap, 32 * lx, 32 * ly);
                    graphics.Dispose();
                    drawBorder();
                }
            }
            if (e.KeyCode == Keys.Z && e.Control)
            {
                if (lbitmap1 != null)
                {
                    bitmap1 = lbitmap1;
                    lbitmap1 = null;
                }
                drawBorder();
            }
        }

        private Bitmap calBitmap(Bitmap map, int value)
        {
            Bitmap map2 = new Bitmap(map.Width, map.Height, map.PixelFormat);

            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    Color color = map.GetPixel(i, j);
                    int a = color.A;
                    float h = color.GetHue();
                    float s = color.GetSaturation();
                    float b = color.GetBrightness();

                    h += value * 360f / trackBar1.Maximum;
                    if (h >= 360) h -= 360;

                    float fMax, fMid, fMin;
                    int iSextant, iMax, iMid, iMin;

                    if (0.5 < b)
                    {
                        fMax = b - (b * s) + s;
                        fMin = b + (b * s) - s;
                    }
                    else
                    {
                        fMax = b + (b * s);
                        fMin = b - (b * s);
                    }

                    iSextant = (int)Math.Floor(h / 60f);
                    if (300f <= h)
                    {
                        h -= 360f;
                    }
                    h /= 60f;
                    h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
                    if (0 == iSextant % 2)
                    {
                        fMid = h * (fMax - fMin) + fMin;
                    }
                    else
                    {
                        fMid = fMin - h * (fMax - fMin);
                    }

                    iMax = Convert.ToInt32(fMax * 255);
                    iMid = Convert.ToInt32(fMid * 255);
                    iMin = Convert.ToInt32(fMin * 255);

                    Color nColor;
                    switch (iSextant)
                    {
                        case 1:
                            nColor = Color.FromArgb(a, iMid, iMax, iMin);
                            break;
                        case 2:
                            nColor = Color.FromArgb(a, iMin, iMax, iMid);
                            break;
                        case 3:
                            nColor = Color.FromArgb(a, iMin, iMid, iMax);
                            break;
                        case 4:
                            nColor = Color.FromArgb(a, iMid, iMin, iMax);
                            break;
                        case 5:
                            nColor = Color.FromArgb(a, iMax, iMin, iMid);
                            break;
                        default:
                            nColor = Color.FromArgb(a, iMax, iMid, iMin);
                            break;
                    }
                    map2.SetPixel(i, j, nColor);
                }
            }
            return map2;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (cacheBitmap[trackBar1.Value] != null)
            {
                nBitmap2 = cacheBitmap[trackBar1.Value];
            }
            else nBitmap2 = calBitmap(bitmap2, trackBar1.Value);
            drawBorder();
        }
    }
}
