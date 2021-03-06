﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;




namespace Paint
{
    enum DRAW_MODE : int
    {
        PENMODE = 0,        // 펜 모드
        SHAPEMODE = 1,      // 도형 모드
        PAINTMODE = 2,      // 색 채우기 모드
        ERASERMODE = 3,     // 지우개 모드
        EDITMODE = 4        // 그 외 편집 모드
    }

    public partial class Form1 : Form
    {
        int curMode;
        Color curColor = Color.Black;
        int curLineSize = 1;

        Point mouseDownPoint;

        Bitmap pictureBoxBmp;

        private void SetDrawMode(int mode)
        {
            switch (mode)
            {
                case (int)DRAW_MODE.PENMODE:
                    curMode = (int)DRAW_MODE.PENMODE;
                    this.Cursor = LoadCursor(Properties.Resources.PenCursor_small);
                    break;
                case (int)DRAW_MODE.SHAPEMODE:
                    curMode = (int)DRAW_MODE.SHAPEMODE;
                    this.Cursor = LoadCursor(Properties.Resources.ShapesCursor);
                    break;
                case (int)DRAW_MODE.PAINTMODE:
                    curMode = (int)DRAW_MODE.PAINTMODE;
                    this.Cursor = LoadCursor(Properties.Resources.PaintCursor);
                    break;
                case (int)DRAW_MODE.ERASERMODE:
                    curMode = (int)DRAW_MODE.ERASERMODE;
                    this.Cursor = LoadCursor(Properties.Resources.EraserCursor);
                    break;
                case (int)DRAW_MODE.EDITMODE:
                    this.Cursor = Cursors.Default;
                    break;
                default:
                    this.Cursor = Cursors.Default;
                    break;
            }
        }
        
        private Cursor LoadCursor(byte[] cursorFile)
        {
            MemoryStream cursorMemoryStream = new MemoryStream(cursorFile);
            Cursor hand = new Cursor(cursorMemoryStream);
        
            return hand;
        }
        
        private void doFloodFill(Point startPoint, Color preColor)
        {
            try
            {
                Stack<Point> pixels = new Stack<Point>();
                preColor = pictureBoxBmp.GetPixel(startPoint.X, startPoint.Y);
                pixels.Push(startPoint);

                while (pixels.Count > 0)
                {
                    Point i = pixels.Pop();
                    if (i.X < pictureBoxBmp.Width && i.X > 0 && i.Y < pictureBoxBmp.Height && i.Y > 0)
                    {
                        if (pictureBoxBmp.GetPixel(i.X, i.Y) == preColor)
                        {
                            pictureBoxBmp.SetPixel(i.X, i.Y, curColor);
                            pixels.Push(new Point(i.X - 1, i.Y));
                            pixels.Push(new Point(i.X + 1, i.Y));
                            pixels.Push(new Point(i.X, i.Y - 1));
                            pixels.Push(new Point(i.X, i.Y + 1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        public Form1()
        {
            InitializeComponent();
            panel2.BackColor = curColor;
            SetDrawMode((int)DRAW_MODE.PENMODE);
        
            pictureBoxBmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            SetDrawMode((int)DRAW_MODE.PENMODE);
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            SetDrawMode((int)DRAW_MODE.SHAPEMODE);
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            SetDrawMode((int)DRAW_MODE.PAINTMODE);
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            SetDrawMode((int)DRAW_MODE.ERASERMODE);
        }
        
        private void button5_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
        
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                curColor = colorDialog.Color;
                panel2.BackColor = curColor;
            }
        }
        
        private void panel2_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
        
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                curColor = colorDialog.Color;
                panel2.BackColor = curColor;
            }
        }
        
        private void button6_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "paint1.png";
            saveFileDialog.Filter = "PNG File|*.png|Bitmap File|*.bmp|JPEG File|*.jpg";
        
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("이미지가 없습니다!");
                return;
            }
        
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = (FileStream)saveFileDialog.OpenFile();
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        this.pictureBox1.Image.Save(fs, ImageFormat.Png);
                        break;
        
                    case 2:
                        this.pictureBox1.Image.Save(fs, ImageFormat.Bmp);
                        break;
        
                    case 3:
                        this.pictureBox1.Image.Save(fs, ImageFormat.Jpeg);
                        break;
                }
        
                fs.Dispose();
                fs.Close();
            }  
        }
        
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((curMode == (int)DRAW_MODE.PENMODE || curMode == (int)DRAW_MODE.ERASERMODE) && e.Button == MouseButtons.Left)
            {
                Point curPoint = pictureBox1.PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y));
        
                Pen p;
                if (curMode == (int)DRAW_MODE.ERASERMODE)
                    p = new Pen(Color.White);
                else
                    p = new Pen(curColor);
        
                p.Width = curLineSize;
        
                Graphics g = Graphics.FromImage(pictureBoxBmp);
                g.DrawEllipse(p, curPoint.X, curPoint.Y, p.Width, p.Width);
                pictureBox1.Image = pictureBoxBmp;
        
                p.Dispose();
                g.Dispose();
            }
        }
        
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            SetDrawMode(curMode);
        }
        
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            SetDrawMode((int)DRAW_MODE.EDITMODE);
        }
        
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (curMode == (int)DRAW_MODE.SHAPEMODE && e.Button == MouseButtons.Left)
            {
                mouseDownPoint = new Point(e.X, e.Y);
            }
            else if (curMode == (int)DRAW_MODE.PAINTMODE && e.Button == MouseButtons.Left)
            {
                Point startPoint = new Point(e.X, e.Y);
                Color preColor = pictureBoxBmp.GetPixel(startPoint.X, startPoint.Y);
                doFloodFill(startPoint, preColor);
                pictureBox1.Image = pictureBoxBmp;
            }
        }
        
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if(curMode == (int)DRAW_MODE.SHAPEMODE && e.Button == MouseButtons.Left)
            {
                Pen p = new Pen(curColor);
                p.Width = curLineSize;
        
                Point mouseUpPoint = new Point(e.X, e.Y);
                            
                Graphics g = Graphics.FromImage(pictureBoxBmp);
                g.DrawRectangle(p, new Rectangle(mouseDownPoint.X, mouseDownPoint.Y, Math.Abs(mouseUpPoint.X - mouseDownPoint.X), Math.Abs(mouseUpPoint.Y - mouseDownPoint.Y)));
                pictureBox1.Image = pictureBoxBmp;
        
                p.Dispose();
                g.Dispose();
            }
        }
        
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            curLineSize = trackBar1.Value;
        }
    }
}
        