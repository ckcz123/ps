using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ps
{
    public partial class Form1 : Form
    {

        private int height;

        public Form1(int height)
        {
            this.height = height;
            InitializeComponent();
        }

        private Bitmap bitmap1, lbitmap1, bitmap2, nBitmap2, copyBitmap;
        private Bitmap picture1, picture2;
        private Graphics graphics1, graphics2;
        string directory1, filename1, directory2, filename2;
        private int lx=-1, ly=-1, rx=-1, ry=-1;
        private Bitmap[] cacheBitmap = new Bitmap[25];

        private string getDirectory()
        {
            string curr = Directory.GetCurrentDirectory();
            foreach (var s in new [] {"project\\images\\", "..\\project\\images\\", "images\\"})
            {
                string temp = Path.GetFullPath(Path.Combine(curr, s));
                if (Directory.Exists(temp)) return temp;
            }
            return curr;
        }

        private bool loadImage(ref string directory, ref string filename, ref Bitmap bitmap)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PNG图片(*.png)|*.png";
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(directory))
            {
                dialog.InitialDirectory = getDirectory();
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
                    Bitmap _bitmap_ = (Bitmap)Image.FromFile(dialog.FileName);
                    Bitmap _bitmap = cloneBitmap(_bitmap_);
                    _bitmap_.Dispose();

                    // 检查纯白和纯黑底色
                    BitmapWrapper bitmapWrapper = new BitmapWrapper(_bitmap);
                    int trans = 0, white = 0, black = 0;
                    for (int i = 0; i < _bitmap.Width; i++)
                    {
                        for (int j = 0; j < _bitmap.Height; j++)
                        {
                            Color color = bitmapWrapper.GetPixel(i, j);
                            if (color.A==0) trans++;
                            if (color.A==255 && color.R==255 && color.G==255 && color.B==255) white++;
                            // if (color.A == 255 && color.R == 0 && color.G == 0 && color.B == 0) black++;
                        }
                    }

                    if (white > black && white > trans * 10)
                    {
                        if (MessageBox.Show("看起来这张图片是以纯白为底色，是否自动调整为透明底色？", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Information) == DialogResult.OK)
                        {
                            for (int i = 0; i < _bitmap.Width; i++)
                            {
                                for (int j = 0; j < _bitmap.Height; j++)
                                {
                                    Color color = bitmapWrapper.GetPixel(i, j);
                                    if (color.A == 255 && color.R == 255 && color.G == 255 && color.B == 255)
                                        bitmapWrapper.SetPixel(i, j, Color.Transparent);
                                }
                            }
                        }
                    }
                    /*
                    if (black > white && black > trans * 10)
                    {
                        if (MessageBox.Show("看起来这张图片是以纯黑为底色，是否自动调整为透明底色？", "提示", MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Information) == DialogResult.OK)
                        {
                            for (int i = 0; i < _bitmap.Width; i++)
                            {
                                for (int j = 0; j < _bitmap.Height; j++)
                                {
                                    Color color = bitmapWrapper.GetPixel(i, j);
                                    if (color.A == 255 && color.R == 0 && color.G == 0 && color.B == 0)
                                        bitmapWrapper.SetPixel(i, j, Color.Transparent);
                                }
                            }
                        }
                    }
                     * */
                    bitmapWrapper.UnWrapper();

                    if (_bitmap.Width % 32 != 0 || _bitmap.Height % height != 0)
                    {
                        if (_bitmap.Width <= 128 && _bitmap.Height <= 4 * height)
                        {
                            if (MessageBox.Show("目标长宽不符合条件，是否自动进行调整？", "加载错误", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Asterisk) == DialogResult.OK)
                            {
                                bitmap = new Bitmap(128, 4 * height);
                                Graphics graphics = Graphics.FromImage(bitmap);

                                int w = _bitmap.Width / 4, h = _bitmap.Height / 4;
                                for (int i = 0; i < 4; i++)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        graphics.DrawImage(_bitmap, new Rectangle(i*32 + (32-w)/2, j*height + (height-h)/2, w, h), i*w, j*h, w, h, GraphicsUnit.Pixel);
                                    }
                                } 
                                graphics.Dispose();
                                _bitmap.Dispose();
                            }
                            else
                            {
                                _bitmap.Dispose();
                                return false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("目标图片宽不是32的倍数，或高不是" + height + "的倍数！", "加载失败", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            _bitmap.Dispose();
                            return false;
                        }
                    }
                    else
                    {
                        bitmap = new Bitmap(_bitmap.Width, _bitmap.Height);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        Util.drawImage(graphics, _bitmap);
                        graphics.Dispose();
                        _bitmap.Dispose();
                    }
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

        private Bitmap setOpacity(Bitmap bitmap)
        {
            Bitmap v = new Bitmap(bitmap.Width, bitmap.Height);
            int value = (int)numericUpDown1.Value;
            if (value < 0) value = 0;
            if (value > 255) value = 255;
            float opacity = value/255f;
            Graphics graphics = Graphics.FromImage(v);
            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = opacity;
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
            graphics.Dispose();
            return v;
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
                for (int i = 0; i < 25; i++)
                {
                    if (cacheBitmap[i] != null)
                        cacheBitmap[i].Dispose();
                    cacheBitmap[i] = null;
                }
                if (nBitmap2 != null)
                {
                    nBitmap2.Dispose();
                    nBitmap2 = null;
                }
                numericUpDown1.Value = 255;
                nBitmap2 = setOpacity(bitmap2);
                picture2 = cloneBitmap(bitmap2);
                graphics2 = Graphics.FromImage(picture2);
                rx = ry = -1;
                pictureBox2.Image = picture2;
                trackBar1.Value = 0;
                trackBar1.Enabled = true;
                new Thread(() =>
                {
                    for (int i = 0; i < 25; i++)
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
                graphics1.DrawRectangle(pen, 32 * lx, height * ly, 32, height);
                pictureBox1.Image = picture1;
            }
            if (rx >= 0 && ry >= 0)
            {
                graphics2.DrawRectangle(pen, 32 * rx, height * ry, 32, height);
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
            Bitmap nBitmap = new Bitmap(bitmap1.Width, bitmap1.Height + height, bitmap1.PixelFormat);
            Graphics graphics = Graphics.FromImage(nBitmap);
            Util.drawImage(graphics, bitmap1);
            graphics.Dispose();
            bitmap1 = cloneBitmap(nBitmap);
            nBitmap.Dispose();
            drawBorder();
            panel1.AutoScrollPosition = new Point(0, bitmap1.Height);
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
            ly = ((MouseEventArgs)e).Y/height;
            drawBorder();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (bitmap2 == null) return;
            lx = ly = -1;
            rx = ((MouseEventArgs)e).X / 32;
            ry = ((MouseEventArgs)e).Y / height;
            drawBorder();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C)
            {
                // 复制
                if (lx >= 0 && ly >= 0)
                {
                    copyBitmap = bitmap1.Clone(new Rectangle(32 * lx, height * ly, 32, height), bitmap1.PixelFormat);
                    pictureBox3.Image = copyBitmap;
                }
                if (rx >= 0 && ry >= 0)
                {
                    copyBitmap = nBitmap2.Clone(new Rectangle(32 * rx, height * ry, 32, height), nBitmap2.PixelFormat);
                    pictureBox3.Image = copyBitmap;
                }
            }
            if (e.KeyCode == Keys.V && copyBitmap!=null)
            {
                // 替换
                if (lx >= 0 && ly >= 0)
                {
                    lbitmap1 = cloneBitmap(bitmap1);
                    Graphics graphics=Graphics.FromImage(bitmap1);
                    graphics.Clip = new Region(new Rectangle(32*lx, height*ly, 32, height));
                    graphics.Clear(Color.Transparent);
                    Util.drawImage(graphics, copyBitmap, 32 * lx, height * ly);
                    graphics.Dispose();
                    drawBorder();
                }
            }
            if (e.KeyCode == Keys.Z)
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

            int hue = (int) (value * 360f / trackBar1.Maximum);

            Bitmap map2 = new Bitmap(map.Width, map.Height, map.PixelFormat);

            BitmapWrapper mapWrapper = new BitmapWrapper(map);
            BitmapWrapper map2Wrapper = new BitmapWrapper(map2);

            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    map2Wrapper.SetPixel(new Point(i,j), Util.addHue(mapWrapper.GetPixel(new Point(i,j)), hue));
                }
            }

            mapWrapper.UnWrapper();
            map2Wrapper.UnWrapper();
            return map2;
        }

        private void redrawImage(object sender, EventArgs e)
        {
            if (bitmap2 == null) return;
            if (cacheBitmap[trackBar1.Value] != null)
            {
                nBitmap2 = setOpacity(cacheBitmap[trackBar1.Value]);
            }
            else
            {
                Bitmap tmp = calBitmap(bitmap2, trackBar1.Value);
                nBitmap2 = setOpacity(tmp);
                tmp.Dispose();
            }
            drawBorder();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox3.Height = height;
            label6.Text = "本工具只支持32x" + height + "像素的操作，敬请谅解。";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (picture1 == null || picture2 == null)
            {
                MessageBox.Show("没有图片！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (bitmap1.Width != 64 && bitmap1.Width != 128)
            {
                MessageBox.Show("要导入的目标只能是怪物或NPC！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (nBitmap2.Width != 128 || nBitmap2.Height != 4 * height)
            {
                MessageBox.Show("只有4x4的图片才能进行批量导入！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lbitmap1 = cloneBitmap(bitmap1);
            Bitmap nBitmap = new Bitmap(bitmap1.Width, bitmap1.Height + 4 * height, bitmap1.PixelFormat);
            Graphics graphics = Graphics.FromImage(nBitmap);
            Util.drawImage(graphics, bitmap1);

            if (bitmap1.Width == 128)
            {
                Util.drawImage(graphics, nBitmap2, 0, bitmap1.Height);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Bitmap first = nBitmap2.Clone(new Rectangle(0, i * height, 32, height), nBitmap2.PixelFormat);
                    Util.drawImage(graphics, first, 0, bitmap1.Height + i * height);

                    BitmapWrapper firstWrapper = new BitmapWrapper(first);

                    Bitmap second = nBitmap2.Clone(new Rectangle(32, i * height, 32, height), nBitmap2.PixelFormat);
                    BitmapWrapper secondWrapper = new BitmapWrapper(second);

                    bool same = true;
                    for (var x = 0; x < 32; x++)
                    {
                        for (var y = 0; y < height; y++)
                        {
                            if (firstWrapper.GetPixel(x, y).ToArgb() != secondWrapper.GetPixel(x, y).ToArgb())
                            {
                                same = false;
                            }
                        }
                    }
                    firstWrapper.UnWrapper();
                    secondWrapper.UnWrapper();

                    if (same)
                    {
                        // 1-3
                        graphics.DrawImage(nBitmap2, new Rectangle(32, bitmap1.Height + i*height, 32, 32),
                            new Rectangle(64, i * height, 32, height), GraphicsUnit.Pixel);
                    }
                    else
                    {
                        Util.drawImage(graphics, second, 32, bitmap1.Height + i * height);
                    }

                    first.Dispose();
                    second.Dispose();
                }
            }

            graphics.Dispose();
            bitmap1 = cloneBitmap(nBitmap);
            nBitmap.Dispose();
            drawBorder();

            panel1.AutoScrollPosition = new Point(0, bitmap1.Height);
            MessageBox.Show("批量导入成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            bitmap1 = new Bitmap(32, height);
            lbitmap1 = null;
            picture1 = cloneBitmap(bitmap1);
            graphics1 = Graphics.FromImage(picture1);
            lx = ly = -1;
            pictureBox1.Image = picture1;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (picture1 == null)
            {
                MessageBox.Show("没有图片！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            lbitmap1 = cloneBitmap(bitmap1);
            Bitmap nBitmap = new Bitmap(bitmap1.Width + 32, bitmap1.Height, bitmap1.PixelFormat);
            Graphics graphics = Graphics.FromImage(nBitmap);
            Util.drawImage(graphics, bitmap1);
            graphics.Dispose();
            bitmap1 = cloneBitmap(nBitmap);
            nBitmap.Dispose();
            drawBorder();
            panel1.AutoScrollPosition = new Point(bitmap1.Width, 0);
        }
    }
}
