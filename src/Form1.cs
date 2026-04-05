using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ATMapEditor
{
    public partial class Form1 : Form
    {
        const int MAPSIZE = 50 * 40;
        const int MAPFILESIZE = 4 * MAPSIZE;
        uint[] cell = new uint[MAPSIZE];
        int pos_old;
        string mapFilePath;
        bool mouseInClick = false;
        int mouseInRect, x1, y1;

        private void MsgErr(string text)
        {
            MessageBox.Show(text, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void MapDraw()
        {
            Bitmap atmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(atmap);

            SolidBrush bmount = new SolidBrush(Color.Brown);
            SolidBrush bsea = new SolidBrush(Color.DarkBlue);
            SolidBrush bforest = new SolidBrush(Color.DarkGreen);
            SolidBrush bshallow = new SolidBrush(Color.DodgerBlue);
            SolidBrush bwood = new SolidBrush(Color.ForestGreen);
            SolidBrush bbay = new SolidBrush(Color.Blue);
            SolidBrush bplain = new SolidBrush(Color.GreenYellow);
            //Rectangle rect = new Rectangle(new Point(0), s8x8);
            int size = 8;
            for (int i = 0; i < MAPSIZE; i++)
            {
                switch (cell[i])
                {
                    case 0:
                        g.FillRectangle(bmount, i % 50 * size, i / 50 * size, size, size);
                        break;
                    case 1:
                        g.FillRectangle(bsea, i % 50 * size, i / 50 * size, size, size);
                        break;
                    case 2:
                        g.FillRectangle(bforest, i % 50 * size, i / 50 * size, size, size);
                        break;
                    case 3:
                        g.FillRectangle(bshallow, i % 50 * size, i / 50 * size, size, size);
                        break;
                    case 4:
                        g.FillRectangle(bwood, i % 50 * size, i / 50 * size, size, size);
                        break;
                    case 5:
                        g.FillRectangle(bbay, i % 50 * size, i / 50 * size, size, size);
                        break;
                    case 6:
                        g.FillRectangle(bplain, i % 50 * size, i / 50 * size, size, size);
                        break;
                }
            }
            bplain.Dispose();
            bshallow.Dispose();
            bsea.Dispose();
            bwood.Dispose();
            bbay.Dispose();
            bforest.Dispose();
            bmount.Dispose();
            g.Dispose();

            pictureBox1.Image = atmap;
        }

        private void MapChange(int pos)
        {
            if (radioButton1.Checked)
                cell[pos] = 0;
            else if (radioButton2.Checked)
                cell[pos] = 1;
            else if (radioButton3.Checked)
                cell[pos] = 2;
            else if (radioButton4.Checked)
                cell[pos] = 3;
            else if (radioButton5.Checked)
                cell[pos] = 4;
            else if (radioButton6.Checked)
                cell[pos] = 5;
            else if (radioButton7.Checked)
                cell[pos] = 6;
        }

        private void MapFileOpen(string openFilePath)
        {
            System.IO.FileStream fp;
            byte[] buf = new byte[4];
            try
            {
                fp = new System.IO.FileStream(openFilePath, System.IO.FileMode.Open);
            }
            catch
            {
                MsgErr("指定したファイルにアクセスできません。");
                return;
            }
            if (fp.Length != MAPFILESIZE)
            {
                MsgErr("開いたファイルはマップデータではありません。");
                fp.Close();
                return;
            }
            for (int i = 0; i < MAPSIZE; i++)
            {
                fp.Read(buf, 0, 4);
                cell[i] = System.BitConverter.ToUInt32(buf, 0);
                if (cell[i] > 6)
                {
                    MsgErr("開いたファイルはマップデータではないか、データが壊れています。");
                    fp.Close();
                    return;
                }
            }
            fp.Close();
            mapFilePath = openFilePath;
            this.Text = System.IO.Path.GetFileName(openFilePath) + " - ATMapEditor";
            MapDraw();
        }

        private void MapFileSave(string saveFilePath)
        {
            System.IO.FileStream fp;
            byte[] buf = new byte[4];
            try
            {
                fp = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);
            }
            catch
            {
                MsgErr("指定したファイルにアクセスできません。");
                return;
            }
            for (int i = 0; i < MAPSIZE; i++)
            {
                if (cell[i] > 6)
                {
                    MsgErr("内部データが壊れています。");
                    fp.Close();
                    return;
                }
                buf = System.BitConverter.GetBytes(cell[i]);
                fp.Write(buf, 0, 4);
            }
            fp.Close();
            mapFilePath = saveFilePath;
            this.Text = System.IO.Path.GetFileName(saveFilePath) + " - ATMapEditor";
        }

        public Form1(string[] args)
        {
            InitializeComponent();
            pos_old = -1;
            mouseInRect = 0;
            if (args.Length == 1)
            {
                if (System.IO.File.Exists(args[0]))
                {
                    MapFileOpen(args[0]);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            MapFileOpen(openFileDialog1.FileName);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mapFilePath == null)
                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                    return;
            MapFileSave(mapFilePath);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = System.IO.Path.GetFileName(mapFilePath);
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            MapFileSave(saveFileDialog1.FileName);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseInClick)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.X >= pictureBox1.Width ||
                        e.Y >= pictureBox1.Height ||
                        e.X < 0 ||
                        e.Y < 0)
                        return;
                    int pos = e.X / 8 + e.Y / 8 * 50;
                    if (pos == pos_old)
                        return;
                    pos_old = pos;

                    MapChange(pos);
                    MapDraw();
                }
                else
                {
                    mouseInClick = false;
                }
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.X >= pictureBox1.Width ||
                    e.Y >= pictureBox1.Height ||
                    e.X < 0 ||
                    e.Y < 0)
                    return;
                if (mouseInRect == 1)
                {
                    x1 = e.X;
                    y1 = e.Y;
                    mouseInRect = 2;
                    buttonRect.Text = "位置(2)...";
                    return;
                }
                else if (mouseInRect == 2)
                {
                    int x2, y2;
                    if (x1 > e.X)
                    {
                        x2 = x1;
                        x1 = e.X;
                    }
                    else
                        x2 = e.X;
                    if (y1 > e.Y)
                    {
                        y2 = y1;
                        y1 = e.Y;
                    }
                    else
                        y2 = e.Y;
                    for (int x = x1; x <= x2; x++)
                    {
                        for (int y = y1; y <= y2; y++)
                        {
                            MapChange(x / 8 + y / 8 * 50);
                        }
                    }
                    MapDraw();
                    mouseInRect = 0;
                    buttonRect.Text = "矩形領域";
                    return;
                }
                mouseInClick = true;
                pictureBox1_MouseMove(sender, e);
            }
        }

        private void buttonRect_Click(object sender, EventArgs e)
        {
            if (mouseInRect != 0)
            {
                mouseInRect = 0;
                buttonRect.Text = "矩形領域";
                return;
            }
            mouseInRect = 1;
            buttonRect.Text = "位置(1)...";
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dropdata = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (dropdata.Length > 1)
                    return;
                if (!System.IO.File.Exists(dropdata[0]))
                    return;
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] dropdata = (string[])e.Data.GetData(DataFormats.FileDrop);
            MapFileOpen(dropdata[0]);
        }

    }
}