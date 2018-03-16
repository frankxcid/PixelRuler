using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace PixelRuler
{
    public partial class frmMain : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private bool IAmVertical = false;
        private bool doItOnce = false;
        private Rectangle rectLeft;
        private Rectangle rectRight;
        private Rectangle rectUp;
        private Rectangle rectDown;
        private Rectangle rectCenter;
        private Point MousePos;
        private direction resizeDirection;
        private Point resizingStart;
        private int minWidth = 150;
        private int minHeight = 150;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_MouseDown(object sender, MouseEventArgs e)
        {
            resizeDirection = direction.none;
            if (e.Button != MouseButtons.Left) { return; }
            
            resizingStart = PointToScreen(new Point(e.X, e.Y));

            if (rectLeft.Contains(MousePos))
            {
                resizeDirection = direction.left;
            }
            else if (rectRight.Contains(MousePos))
            {
                resizeDirection = direction.right;
            }
            else if (rectUp.Contains(MousePos))
            {
                resizeDirection = direction.up;
            }
            else if (rectDown.Contains(MousePos))
            {
                resizeDirection = direction.down;
            }
            else
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                executeresize(); //redraw in case off screen erase
            }
            
        }
        
        private void executeresize()
        {
            int wd = this.Size.Width;
            int ht = this.Size.Height;
            int controlWidth = btnAspect.Width + btnUp.Width;
            int controlHeight = btnAspect.Height + btnLeft.Height;
            //set the aspect control in center
            btnAspect.Image = (IAmVertical ? iLstAspect.Images[1] : iLstAspect.Images[0]);
            Point buttonPoint = new Point();
            buttonPoint.X = (IAmVertical ? ((int)Math.Floor(wd / 2D) - (int)Math.Floor(controlWidth / 2D)) : 50 - (int)Math.Floor(controlWidth/2D));
            buttonPoint.Y = (IAmVertical ? 50 - (int)Math.Floor(controlHeight / 2D) + btnLeft.Height : ((int)Math.Floor(ht / 2D) - (int)Math.Floor(controlHeight / 2D)) + btnLeft.Height);
            btnAspect.Location = buttonPoint;
            btnLeft.Location = new Point(btnAspect.Location.X, btnAspect.Location.Y - btnLeft.Height);
            btnRight.Location = new Point(btnLeft.Location.X + btnLeft.Width, btnLeft.Location.Y);
            btnUp.Location = new Point(btnAspect.Location.X + btnAspect.Width, btnAspect.Location.Y);
            btnDown.Location = new Point(btnUp.Location.X, btnUp.Location.Y + btnUp.Height);
            btnLeft.Text = char.ConvertFromUtf32(0xEF);
            btnRight.Text = char.ConvertFromUtf32(0xF0);
            btnUp.Text = char.ConvertFromUtf32(0xF1);
            btnDown.Text = char.ConvertFromUtf32(0xF2);
            btnClose.Location = new Point(wd - 40, (IAmVertical ? 20 : 40));

            ttAll.SetToolTip(btnAspect, (IAmVertical ? "Click to make ruler horizontal." : "Click to make ruler vertical"));
            ttAll.SetToolTip(btnClose, "Close the ruler");

            int pos = 0;
            int dim = (IAmVertical ? ht : wd);
            tickType tt;
            Graphics g = this.CreateGraphics();

            Pen pen = new Pen(Color.Black);
            g.Clear(Color.Wheat);
            while (pos < dim)
            {
                if (pos > 0)
                {
                    if (pos % 100 == 0)
                    {
                        tt = tickType.century;
                    }
                    else if (pos % 50 == 0)
                    {
                        tt = tickType.quint;
                    }
                    else if (pos % 10 == 0)
                    {
                        tt = tickType.decade;
                    }
                    else
                    {
                        tt = tickType.fifth;
                    }
                    drawTick(pos, tt);
                }
                pos += 5;
            }
            //set rectangle handles
            int handleThickness = 4;
            int handleHt = 50;
            rectLeft = new Rectangle(0, (ht - handleHt) / 2, handleThickness, handleHt);
            rectRight = new Rectangle(wd - handleThickness - 1, (ht - handleHt) / 2, handleThickness, handleHt);
            rectUp = new Rectangle((wd - handleHt) / 2, 0, handleHt, handleThickness);
            rectDown = new Rectangle((wd - handleHt) / 2, ht - handleThickness - 1, handleHt, handleThickness);            
            if (IAmVertical)
            {
                g.DrawRectangle(Pens.Black, rectUp);
                g.DrawRectangle(Pens.Black, rectDown);
                rectCenter = new Rectangle(0, 100, wd, ht - 100 - handleThickness);
            }
            else
            {
                g.DrawRectangle(Pens.Black, rectLeft);
                g.DrawRectangle(Pens.Black, rectRight);
                rectCenter = new Rectangle(100, 0, wd - 100 - handleThickness, ht);
            }
            g.DrawRectangle(Pens.Transparent, rectCenter);

            g.Dispose();
        }
        private enum tickType
        {
            century,
            decade,
            quint,
            fifth
        }
        private void drawTick(int pos, tickType _tickType)
        {
            int wd = this.Size.Width;
            int ht = this.Size.Height;
            //Rulers: 
            //  horizontal: ruler1 = top, ruler2 = bottom
            //  vertical: ruler1 = right, ruler2 = left
            int lineLenght = 0;
            Font ft = new Font("Arial", 20, GraphicsUnit.Pixel);
            switch (_tickType)
            {
                case tickType.century:
                    lineLenght = 15;
                    ft = new Font("Arial", 20, GraphicsUnit.Pixel);
                    break;
                case tickType.quint:
                    lineLenght = 12;
                    ft = new Font("Arial", 12, GraphicsUnit.Pixel);
                    break;
                case tickType.decade:
                    lineLenght = 5;
                    ft = new Font("Arial", 8, GraphicsUnit.Pixel);
                    break;
                default:
                    lineLenght = 3;
                    break;
            }
            Graphics g = this.CreateGraphics();
            Rectangle ruler1 = new Rectangle();
            Rectangle ruler2 = new Rectangle();
            Point textPoint1 = new Point();
            Point textPoint2 = new Point();
            int adjPost = pos;
            if (adjPost > 100 && adjPost % 100 != 0)
            {
                adjPost -= (adjPost - (adjPost % 100));
            }
            SizeF stringSize = g.MeasureString(adjPost.ToString("#"), ft);
            if (IAmVertical)
            {
                ruler1 = new Rectangle(wd - lineLenght, pos, lineLenght, 0);
                ruler2 = new Rectangle(0, pos, lineLenght, 0);
                textPoint1 = new Point((int)(wd - lineLenght - stringSize.Width - 2), (int)Math.Floor(pos - (stringSize.Height / 2)));
                textPoint2 = new Point((int)(lineLenght + 2), (int)Math.Floor(pos - (stringSize.Height / 2)));
            }
            else
            {
                ruler1 = new Rectangle(pos, 0, 0, lineLenght);
                ruler2 = new Rectangle(pos, ht - lineLenght, 0, lineLenght);
                textPoint1 = new Point((int)Math.Floor(pos - (stringSize.Width / 2)), (int)(lineLenght + 2));
                textPoint2 = new Point((int)Math.Floor(pos - (stringSize.Width / 2)), (int)(ht - lineLenght - 2 - stringSize.Height));
            }
            Pen pen = new Pen(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            g.DrawLine(pen, ruler1.X, ruler1.Y, ruler1.X + ruler1.Width, ruler1.Y + ruler1.Height);
            g.DrawLine(pen, ruler2.X, ruler2.Y, ruler2.X + ruler2.Width, ruler2.Y + ruler2.Height);
            if (_tickType != tickType.fifth)
            {
                g.DrawString(adjPost.ToString("#"), ft, drawBrush, textPoint1);
                g.DrawString(adjPost.ToString("#"), ft, drawBrush, textPoint2);
            }
            g.Dispose();
        }
        
        private void frmMain_Paint(object sender, PaintEventArgs e)
        {
            if (!doItOnce)
            {
                executeresize();
                doItOnce = true;
            }
        }

        private void btnAspect_Click(object sender, EventArgs e)
        {
            IAmVertical = !IAmVertical;
            int currentWD = this.Width;
            int currentHt = this.Height;
            this.Width = currentHt;
            this.Height = currentWD;
            executeresize();
        }
        private enum direction
        {
            up,
            down,
            left,
            right,
            none
        };
        private void move(direction dir)
        {

            int currentX = this.Location.X;
            int currentY = this.Location.Y;
            switch (dir)
            {
                case direction.down:
                    currentY += 1;
                    break;
                case direction.left:
                    currentX -= 1;
                    break;
                case direction.right:
                    currentX += 1;
                    break;
                case direction.up:
                    currentY -= 1;
                    break;
            }
            this.Location = new Point(currentX, currentY);
        }
        private void btnLeft_Click(object sender, EventArgs e)
        {
            move(direction.left);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            move(direction.right);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            move(direction.up);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            move(direction.down);
        }

        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
            Point resizingEnding = PointToScreen(new Point(e.X, e.Y));
            Rectangle rectForm = Bounds;
            if (Capture)
            {
                //this means that the form has capture the mouse as in resizing, not during move
                if (resizeDirection != direction.none)
                {
                    switch (resizeDirection)
                    {
                        case direction.up:
                            Bounds = new Rectangle(rectForm.X, rectForm.Y + resizingEnding.Y - resizingStart.Y, rectForm.Width, rectForm.Height - resizingEnding.Y + resizingStart.Y);
                            break;
                        case direction.right:
                            Bounds = new Rectangle(rectForm.X, rectForm.Y, rectForm.Width + resizingEnding.X - resizingStart.X, rectForm.Height);
                            break;
                        case direction.down:
                            Bounds = new Rectangle(rectForm.X, rectForm.Y, rectForm.Width, rectForm.Height + resizingEnding.Y - resizingStart.Y);
                            break;
                        case direction.left:
                            Bounds = new Rectangle(rectForm.X + resizingEnding.X - resizingStart.X, rectForm.Y, rectForm.Width - resizingEnding.X + resizingStart.X, rectForm.Height);
                            break;
                    }
                    resizingStart = resizingEnding;
                    doItOnce = false;
                    Invalidate();
                }
            }
            else
            {
                MousePos = new Point(e.X, e.Y);
                Cursor = Cursors.Default;
                if (rectCenter.Contains(MousePos))
                {
                    Cursor = Cursors.SizeAll;
                }
                if (rectUp.Contains(MousePos) || rectDown.Contains(MousePos))
                {
                    Cursor = Cursors.SizeNS;
                }
                if (rectLeft.Contains(MousePos) || rectRight.Contains(MousePos))
                {
                    Cursor = Cursors.SizeWE;
                }
            }
        }

        private void frmMain_MouseUp(object sender, MouseEventArgs e)
        {
            executeresize();
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (Height < minHeight)
            {
                Capture = false;
                Height = minHeight;
            }
            if (Width < minWidth)
            {
                Capture = false;
                Width = minWidth;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
