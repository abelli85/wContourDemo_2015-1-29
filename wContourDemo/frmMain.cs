using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using wContour;

namespace wContourDemo
{
    public partial class frmMain : Form
    {        

        public static frmMain pCurrenWin = null;

        double[,] _gridData = null;
        double[,] _discreteData = null;
        double[] _X = null;
        double[] _Y = null;
        double[] _CValues = null;
        Color[] _colors = null;

        List<List<PointD>> _mapLines = new List<List<PointD>>();
        List<Border> _borders = new List<Border>();
        List<PolyLine> _contourLines = new List<PolyLine>();
        List<PolyLine> _clipContourLines = new List<PolyLine>();
        List<Polygon> _contourPolygons = new List<Polygon>();
        List<Polygon> _clipContourPolygons = new List<Polygon>();
        List<wContour.Legend.lPolygon> _legendPolygons = new List<Legend.lPolygon>();
        List<PolyLine> _streamLines = new List<PolyLine>();
        
        double _undefData = -9999.0;
        List<List<PointD>> _clipLines = new List<List<PointD>>();
        //List<PointD> _clipPList = new List<PointD>();                   
        Color _startColor = default(Color);
        Color _endColor = default(Color);
        private int _highlightIdx = 0;

        private double _minX = 0;
        private double _minY = 0;
        private double _maxX = 0;
        private double _maxY = 0;
        private double _scaleX = 1.0;
        private double _scaleY = 1.0;

        public string _dFormat = "0";
        
        public frmMain()
        {
            InitializeComponent();

            pCurrenWin = this;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.PictureBox1.BackColor = Color.White;
            GB_DrawSet.Enabled = false;
            //this.Lab_StartColor.BackColor = Color.Black;
            //this.Lab_EndColor.BackColor = Color.White;
            _startColor = this.Lab_StartColor.BackColor;
            _endColor = this.Lab_EndColor.BackColor;
            NUD_Highlight.Enabled = false;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            double pX = 0.0;
            double pY = 0.0;
            ToCoordinate(e.X, e.Y, ref pX, ref pY);
            this.TSSL_Coord.Text = "X=" + pX.ToString("0.00") + "; Y=" + pY.ToString("0.00");
        }

        public void ReadSuferGridData(string aFile)
        {
            //Read file
            StreamReader sr = new StreamReader(aFile);
            int LastNonEmpty, i;
            string aLine;
            string[] dataArray;
            List<string> dataList = new List<string>();

            aLine = sr.ReadLine();
            for (i = 1; i <= 4; i++)
                aLine = aLine + " " + sr.ReadLine();

            dataArray = aLine.Split();
            LastNonEmpty = -1;
            dataList.Clear();
            for (i = 0; i < dataArray.Length; i++)
            {
                if (dataArray[i] != string.Empty)
                {
                    LastNonEmpty++;
                    dataList.Add(dataArray[i]);
                }
            }

            int XNum = int.Parse(dataList[1]);
            int YNum = int.Parse(dataList[2]);
            double XMin = double.Parse(dataList[3]);
            double XMax = double.Parse(dataList[4]);
            double YMin = double.Parse(dataList[5]);
            double YMax = double.Parse(dataList[6]);            

            double XDelt = (XMax - XMin) / (XNum - 1);
            double YDelt = (YMax - YMin) / (YNum - 1);
            _X = new double[XNum];
            for (i = 0; i < XNum; i++)
            {
                _X[i] = XMin + i * XDelt;
            }
            _Y = new double[YNum];
            for (i = 0; i < YNum; i++)
            {
                _Y[i] = YMin + i * YDelt;
            }

            //Read grid data
            _gridData = new double[YNum, XNum];
            aLine = sr.ReadLine();
            int ii, jj;
            int d = 0;
            while (aLine != null)
            {
                if (aLine.Trim() == "")
                {
                    aLine = sr.ReadLine();
                    continue;
                }
                dataArray = aLine.Split();
                LastNonEmpty = -1;
                dataList.Clear();
                for (i = 0; i < dataArray.Length; i++)
                {
                    if (dataArray[i] != string.Empty)
                    {
                        LastNonEmpty++;
                        dataList.Add(dataArray[i]);
                    }
                }

                for (i = 0; i < dataList.Count; i++)
                {
                    ii = d / XNum;
                    jj = d % XNum;
                    _gridData[ii, jj] = double.Parse(dataList[i]);
                    d += 1;
                }

                aLine = sr.ReadLine();
            }

            sr.Close();   
        }

        public void CreateGridData(int rows, int cols)
        {
            int i = 0;
            int j = 0;           
            double[,] dataArray = null;
            double XDelt = 0;
            double YDelt = 0;

            //---- Generate X and Y coordinates            
            _X = new double[cols];
            _Y = new double[rows];            

            XDelt = this.PictureBox1.Width / cols;
            YDelt = this.PictureBox1.Height / rows;
            for (i = 0; i <= cols - 1; i++)
            {
                _X[i] = i * XDelt;
            }
            for (i = 0; i <= rows - 1; i++)
            {
                _Y[i] = i * YDelt;
            }

            //---- Generate random data between 10 to 100
            Random random = new Random();
            dataArray = new double[rows, cols];
            for (i = 0; i <= rows - 1; i++)
            {
                for (j = 0; j <= cols - 1; j++)
                {
                    dataArray[i, j] = random.Next(10, 100);
                }
            }            

            _gridData = dataArray;
        }

        public void CreateDiscreteData(int dataNum)
        {
            int i = 0;            
            double[,] S = null;            

            //---- Generate discrete points
            Random random = new Random();
            S = new double[3, dataNum];
            //---- x,y,value
            for (i = 0; i <= dataNum - 1; i++)
            {
                S[0, i] = random.Next(0, this.PictureBox1.Width);
                S[1, i] = random.Next(0, this.PictureBox1.Height);
                S[2, i] = random.Next(10, 100);
            }            

            _discreteData = S;
        }

        public void InterpolateData(int rows, int cols)
        {                      
            double[,] dataArray = null;
            double XDelt = 0;
            double YDelt = 0;

            //---- Generate Grid Coordinate           
            double Xlb = 0;
            double Ylb = 0;
            double Xrt = 0;
            double Yrt = 0;            

            Xlb = 0;
            Ylb = 0;
            Xrt = this.PictureBox1.Width;
            Yrt = this.PictureBox1.Height;
            XDelt = this.PictureBox1.Width / cols;
            YDelt = this.PictureBox1.Height / rows;
            
            Interpolate.CreateGridXY_Num(Xlb, Ylb, Xrt, Yrt, cols, rows, ref _X, ref _Y);

            dataArray = new double[rows, cols];
            dataArray = Interpolate.Interpolation_IDW_Neighbor(_discreteData, _X, _Y, 8, _undefData);
            //dataArray = Interpolate.Interpolation_IDW_Radius(_discreteData, _X, _Y, 4, 100, _undefData);

            _gridData = dataArray;
        }

        public void SetContourValues(double[] values)
        {
            _CValues = values;
        }

        public void TracingContourLines()
        {
            //---- Contour values
            int nc = _CValues.Length;

            //---- Colors
            _colors = CreateColors(_startColor, _endColor, nc + 1);

            double XDelt = 0;
            double YDelt = 0;
            XDelt = _X[1] - _X[0];
            YDelt = _Y[1] - _Y[0];            
            int[,] S1 = new int[1, 1];
            _borders = Contour.TracingBorders(_gridData, _X, _Y, ref S1, _undefData);
            _contourLines = Contour.TracingContourLines(_gridData, _X, _Y, nc, _CValues, _undefData, _borders, S1);
        }

        private Color[] CreateColors(Color sColor, Color eColor, int cNum)
        {
            Color[] colors = new Color[cNum];
            int sR = 0;
            int sG = 0;
            int sB = 0;
            int eR = 0;
            int eG = 0;
            int eB = 0;
            int rStep = 0;
            int gStep = 0;
            int bStep = 0;
            int i = 0;

            sR = sColor.R;
            sG = sColor.G;
            sB = sColor.B;
            eR = eColor.R;
            eG = eColor.G;
            eB = eColor.B;
            rStep = Convert.ToInt32((eR - sR) / cNum);
            gStep = Convert.ToInt32((eG - sG) / cNum);
            bStep = Convert.ToInt32((eB - sB) / cNum);
            for (i = 0; i <= colors.Length - 1; i++)
            {
                colors[i] = Color.FromArgb(sR + i * rStep, sG + i * gStep, sB + i * bStep);
            }

            return colors;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (ChB_AntiAlias.Checked)
                g.SmoothingMode = SmoothingMode.AntiAlias;

            PaintGraphics(g);
            //PaintGraphics_1(g);
        }

        private void PaintGraphics(Graphics g)
        {
            int i = 0;
            int j = 0;
            wContour.PolyLine aline = default(wContour.PolyLine);
            List<PointD> newPList = new List<PointD>();
            double aValue = 0;
            Color aColor = default(Color);
            Pen aPen = default(Pen);
            wContour.PointD aPoint = default(wContour.PointD);
            Point[] Points = null;
            int sX = 0;
            int sY = 0;

            //Draw contour polygons
            if (CB_ContourPolygon.Checked)
            {
                List<Polygon> drawPolygons = _contourPolygons;
                if (CB_Clipped.Checked)
                    drawPolygons = _clipContourPolygons;

                for (i = 0; i < drawPolygons.Count; i++)
                {
                    DrawPolygon(g, drawPolygons[i], false);
                }
                if (ChB_Highlight.Checked)
                {
                    if (_highlightIdx < drawPolygons.Count)
                        DrawPolygon(g, drawPolygons[_highlightIdx], true);
                }
            }

            //Draw contour lines
            if (CB_ContourLine.Checked)
            {
                List<PolyLine> drawLines = _contourLines;
                if (CB_Clipped.Checked)
                    drawLines = _clipContourLines;

                for (i = 0; i <= drawLines.Count - 1; i++)
                {
                    aline = drawLines[i];
                    aValue = aline.Value;
                    aColor = _colors[Array.IndexOf(_CValues, aValue)];
                    newPList = aline.PointList;

                    Points = new Point[newPList.Count];
                    for (j = 0; j <= newPList.Count - 1; j++)
                    {
                        aPoint = (wContour.PointD)newPList[j];
                        ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                        Points[j] = new Point(sX, sY);
                    }
                    aPen = new Pen(Color.Black, 2.0F);
                    aPen.Color = aColor;
                    g.DrawLines(aPen, Points);
                }
            }

            //Draw border lines
            if (CB_BorderLines.Checked)
            {
                for (i = 0; i < _borders.Count; i++)
                {
                    Border aBorder = _borders[i];
                    for (j = 0; j < aBorder.LineNum; j++)
                    {
                        BorderLine bLine = aBorder.LineList[j];
                        Points = new Point[bLine.pointList.Count];
                        for (int k = 0; k < bLine.pointList.Count; k++)
                        {
                            aPoint = bLine.pointList[k];
                            ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                            Points[k] = new Point(sX, sY);
                        }
                        g.DrawLines(Pens.Purple, Points);
                        //if (i == 3)
                        //{
                        //    Pen bPen = new Pen(Color.Blue);
                        //    bPen.Width = 2;
                        //    g.DrawLines(bPen, Points);
                        //}
                        //if (i == 3)
                        //    g.DrawString(i.ToString(), new Font("Arial", 8), new SolidBrush(Color.Red), Points[0].X, Points[0].Y);
                    }
                }
            }

            //Draw clip line
            if (CB_Clipped.Checked && _clipLines.Count > 0)
            {
                foreach (List<PointD> cLine in _clipLines)
                {
                    Points = new Point[cLine.Count];
                    for (i = 0; i < cLine.Count; i++)
                    {
                        aPoint = cLine[i];
                        ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                        Points[i] = new Point(sX, sY);
                    }
                    g.DrawLines(Pens.Purple, Points);
                }
            }

            //Draw data points
            Font drawFont = new Font("Arial", 8);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            if (CB_DiscreteData.Checked && _discreteData != null)
            {
                for (i = 0; i < _discreteData.GetLength(1); i++)
                {
                    ToScreen(_discreteData[0, i], _discreteData[1, i], ref sX, ref sY);
                    g.DrawEllipse(Pens.Red, sX, sY, 1.5f, 1.5f);
                    if (_discreteData[2, i] >= 0.1)
                        g.DrawString(_discreteData[2, i].ToString(_dFormat), drawFont, drawBrush, sX, sY);
                }
            }
            if (CB_GridData.Checked && _gridData != null)
            {
                for (i = 0; i < _gridData.GetLength(0); i++)
                {
                    for (j = 0; j < _gridData.GetLength(1); j++)
                    {
                        ToScreen(_X[j], _Y[i], ref sX, ref sY);
                        if (!DoubleEquals(_gridData[i, j], _undefData))
                        {
                            g.DrawEllipse(Pens.Red, sX, sY, 1.5f, 1.5f);
                            //if (_gridData[i, j] >= 0.1)
                                g.DrawString(_gridData[i, j].ToString(_dFormat), drawFont, drawBrush, sX, sY);
                            //g.DrawString(i.ToString() + "," + j.ToString(), drawFont, drawBrush, sX, sY);
                        }
                        else
                            g.DrawEllipse(Pens.Gray, sX, sY, 1.5f, 1.5f);
                    }
                }
            }

            //Draw stream lines
            if (_streamLines.Count > 0)
            {
                for (i = 0; i < _streamLines.Count; i++)
                {
                    aline = _streamLines[i];
                    aValue = aline.Value;
                    newPList = aline.PointList;

                    PointF[] fPoints = new PointF[newPList.Count];
                    float fX = 0, fY = 0;
                    for (j = 0; j < newPList.Count; j++)
                    {
                        aPoint = (wContour.PointD)newPList[j];
                        ToScreen(aPoint.X, aPoint.Y, ref fX, ref fY);
                        fPoints[j] = new PointF(fX, fY);
                    }
                    aPen = new Pen(Color.Blue);
                    g.DrawLines(aPen, fPoints);

                    int len = 12;
                    for (j = 0; j < fPoints.Length; j++)
                    {
                        if (j > 0 && j < fPoints.Length - 2 && j % len == 0)
                        {
                            //Draw arraw
                            PointF aP = fPoints[j];
                            PointF bPoint = fPoints[j + 1];

                            DrawArrowYaq(g, aP, bPoint);

                            //double U = bPoint.X - aP.X;
                            //double V = bPoint.Y - aP.Y;
                            //double angle = Math.Atan((V) / (U)) * 180 / Math.PI;
                            //angle = angle + 90;
                            //if (U < 0)
                            //    angle = angle + 180;

                            //if (angle >= 360)
                            //    angle = angle - 360;

                            //PointF[] apoints = new PointF[3];
                            //PointF eP1 = new PointF();
                            //double aSize = 8;
                            //eP1.X = (int)(aP.X - aSize * Math.Sin((angle + 20.0) * Math.PI / 180));
                            //eP1.Y = (int)(aP.Y + aSize * Math.Cos((angle + 20.0) * Math.PI / 180));
                            //apoints[0] = eP1;
                            //apoints[1] = aP;
                            ////g.DrawLine(aPen, aP, eP1);

                            //PointF eP2 = new PointF();
                            //eP2.X = (int)(aP.X - aSize * Math.Sin((angle - 20.0) * Math.PI / 180));
                            //eP2.Y = (int)(aP.Y + aSize * Math.Cos((angle - 20.0) * Math.PI / 180));
                            ////g.DrawLine(aPen, aP, eP1);
                            //apoints[2] = eP2;
                            //g.DrawLines(aPen, apoints);
                        }
                    }
                }
            }

            //Draw map lines
            if (_mapLines.Count > 0)
            {
                for (i = 0; i < _mapLines.Count; i++)
                {
                    newPList = _mapLines[i];

                    Points = new Point[newPList.Count];
                    for (j = 0; j <= newPList.Count - 1; j++)
                    {
                        aPoint = newPList[j];
                        ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                        Points[j] = new Point(sX, sY);
                    }
                    aPen = new Pen(Color.Black);
                    aPen.Color = Color.Black;
                    g.DrawLines(aPen, Points);
                    //g.DrawString("0", new Font("Arial", 8), new SolidBrush(Color.Black), Points[0]);
                }
            }

            //---- Draw legend
            if (CB_ContourPolygon.Checked)
            {
                if (_legendPolygons.Count > 0)
                {
                    wContour.Legend.lPolygon aLPolygon = default(wContour.Legend.lPolygon);
                    SolidBrush aBrush = new SolidBrush(Color.Black);
                    for (i = 0; i < _legendPolygons.Count; i++)
                    {
                        aLPolygon = _legendPolygons[i];
                        aValue = aLPolygon.value;
                        if (aLPolygon.isFirst)
                        {
                            aColor = _colors[0];
                        }
                        else
                        {
                            aColor = _colors[Array.IndexOf(_CValues, aValue) + 1];
                        }
                        newPList = aLPolygon.pointList;

                        Points = new Point[newPList.Count];
                        for (j = 0; j <= newPList.Count - 1; j++)
                        {
                            aPoint = newPList[j];
                            ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                            Points[j] = new Point(sX, sY);
                        }
                        aBrush.Color = aColor;
                        g.FillPolygon(aBrush, Points);
                        g.DrawPolygon(Pens.Black, Points);

                        Point sPoint = Points[2];
                        if (i < _legendPolygons.Count - 1)
                        {
                            g.DrawString(_CValues[i].ToString("0.0"), drawFont, Brushes.Black, sPoint.X - 10, sPoint.Y - 15);
                        }
                    }
                }
            }
        }

        public static float getAngleYaq(double uValue, double vValue)
        {
            float angle = (float)(Math.Atan(vValue / uValue) * 180 / Math.PI);
            if (uValue < 0)
            {
                angle -= 180;
            }
            return angle;
        }

        private float DrawArrowYaq(Graphics GR, PointF p0, PointF p1)
        {


            double dX = p0.X - p1.X;
            double dY = p0.Y - p1.Y;
            double X = p0.X - (dX / 2);
            double Y = p0.Y - (dY / 2);
            float Angle = getAngleYaq(dX, dY);

            RectangleF R = new RectangleF((float)X - 4, (float)Y - 4, 8, 8);

            // Rectangle.r
            //   GR.DrawLine(Pens.Black, SC.TraceFrom, SC.TraceTo);
            //   GR.FillEllipse(Brushes.Red, R);
            //   return;

            float xMid = (float)X;
            float yMid = (float)Y;
            //R = new RectangleF(xMid - 4, yMid - 3, 8, 6);
            PointF[] pt = new PointF[5];
            pt[0] = new PointF(R.X, R.Y);
            pt[1] = new PointF(R.Right, R.Top + (R.Height / 2));
            pt[2] = new PointF(R.Left, R.Bottom);
            pt[3] = new PointF(R.Left + R.Width / 2, pt[1].Y);
            pt[4] = pt[0];

            //      g.DrawLine(Pens.Red, 0, yMid, this.Width, yMid);
            //      g.DrawLine(Pens.Red, xMid, 0, xMid, this.Height);
            //the central point of the rotation
            Matrix Mx = GR.Transform;
            GR.TranslateTransform(xMid, yMid);
            //rotation procedure
            // GR.RotateTransform(Angle + 90);

            Angle = 180 - Angle;
            GR.RotateTransform(-Angle);

            GR.TranslateTransform(-xMid, -yMid);



            GR.FillPolygon(Brushes.Black, pt);
            GR.ResetTransform();
            return Angle;
            //      GR.Transform = Mx;


        }

        private void DrawPolygon(Graphics g, Polygon aPolygon, bool isHighlight)
        {
            int j;
            PolyLine aline = aPolygon.OutLine;
            double aValue = aPolygon.LowValue;
            Color aColor = _colors[Array.IndexOf(_CValues, aValue) + 1];
            if (isHighlight)
                aColor = Color.Green;
            else
            {
                if (!aPolygon.IsHighCenter)
                {
                    for (j = 0; j <= _colors.Length - 1; j++)
                    {
                        if (aColor == _colors[j])
                        {
                            aColor = _colors[j - 1];
                        }
                    }
                }
            }
            List<PointD> newPList = aline.PointList;
            //if (!Contour.IsClockwise(newPList))
            //    newPList.Reverse();

            Point[] Points = new Point[newPList.Count];
            int sX = 0, sY = 0;
            for (j = 0; j <= newPList.Count - 1; j++)
            {
                PointD aPoint = newPList[j];
                ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                Points[j] = new Point(sX, sY);
            }

            GraphicsPath bPath = new GraphicsPath();
            GraphicsPath aPath = new GraphicsPath();
            aPath.AddPolygon(Points);
            bPath.AddLines(Points);
            //Region aRegion = new Region(aPath);
            if (aPolygon.HasHoles)
            {
                for (int h = 0; h < aPolygon.HoleLines.Count; h++)
                {
                    newPList = aPolygon.HoleLines[h].PointList;
                    //if (Contour.IsClockwise(newPList))
                    //    newPList.Reverse();
                    Points = new Point[newPList.Count];
                    for (j = 0; j <= newPList.Count - 1; j++)
                    {
                        PointD aPoint = newPList[j];
                        ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                        Points[j] = new Point(sX, sY);
                    }
                    //aPath = new GraphicsPath();
                    aPath.AddPolygon(Points);
                    GraphicsPath cPath = new GraphicsPath();
                    cPath.AddLines(Points);
                    bPath.AddPath(cPath, false);
                    //aRegion.Xor(aPath);
                }
            }

            Pen aPen = new Pen(Color.Black);
            aPen.Color = aColor;
            SolidBrush aBrush = new SolidBrush(Color.Black);
            aBrush.Color = aColor;
            g.FillPath(aBrush, aPath);
            g.DrawPath(Pens.Black, bPath);
        }

        private void PaintGraphics_1(Graphics g)
        {
            int i = 0;
            int j = 0;
            wContour.PolyLine aline = default(wContour.PolyLine);
            List<PointD> newPList = new List<PointD>();
            double aValue = 0;
            Color aColor = default(Color);
            Pen aPen = default(Pen);
            wContour.PointD aPoint = default(wContour.PointD);
            Point[] Points = null;
            int sX = 0;
            int sY = 0;

            //Draw contour polygons
            if (CB_ContourPolygon.Checked)
            {
                List<Polygon> drawPolygons = _contourPolygons;
                if (CB_Clipped.Checked)
                    drawPolygons = _clipContourPolygons;

                Color aBackcolor = Color.White;
                SolidBrush aBrush = default(SolidBrush);
                wContour.Polygon aPolygon = default(wContour.Polygon);
                Region selRegion = new Region();
                //GraphicsPath selPath = new GraphicsPath();
                for (i = 0; i <= drawPolygons.Count - 1; i++)
                {
                    aPolygon = drawPolygons[i];
                    aline = aPolygon.OutLine;
                    aValue = aPolygon.LowValue;
                    aColor = _colors[Array.IndexOf(_CValues, aValue) + 1];
                    if (!aPolygon.IsHighCenter)
                    {
                        for (j = 0; j <= _colors.Length - 1; j++)
                        {
                            if (aColor == _colors[j])
                            {
                                aColor = _colors[j - 1];
                            }
                        }
                    }
                    newPList = aline.PointList;
                    if (!Contour.IsClockwise(newPList))
                        newPList.Reverse();

                    Points = new Point[newPList.Count];
                    for (j = 0; j <= newPList.Count - 1; j++)
                    {
                        aPoint = newPList[j];
                        ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                        Points[j] = new Point(sX, sY);
                    }

                    GraphicsPath bPath = new GraphicsPath();
                    GraphicsPath aPath = new GraphicsPath();
                    aPath.AddPolygon(Points);
                    bPath.AddLines(Points);
                    Region aRegion = new Region(aPath);
                    if (aPolygon.HasHoles)
                    {
                        for (int h = 0; h < aPolygon.HoleLines.Count; h++)
                        {
                            newPList = aPolygon.HoleLines[h].PointList;
                            if (Contour.IsClockwise(newPList))
                                newPList.Reverse();
                            Points = new Point[newPList.Count];
                            for (j = 0; j <= newPList.Count - 1; j++)
                            {
                                aPoint = newPList[j];
                                ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                                Points[j] = new Point(sX, sY);
                            }
                            aPath = new GraphicsPath();
                            aPath.AddPolygon(Points);
                            GraphicsPath cPath = new GraphicsPath();
                            cPath.AddLines(Points);
                            bPath.AddPath(cPath, false);
                            aRegion.Xor(aPath);
                        }
                    }

                    aPen = new Pen(Color.Black);
                    aPen.Color = aColor;
                    aBrush = new SolidBrush(Color.Black);
                    aBrush.Color = aColor;
                    if (i == _highlightIdx)
                    {
                        //selPath = aPath;
                        selRegion = aRegion;
                    }

                    g.FillRegion(aBrush, aRegion);
                    //g.FillPath(aBrush, aPath);
                    //g.DrawPath(Pens.Gray, bPath);
                    g.DrawPath(Pens.Black, bPath);
                    //g.FillPolygon(aBrush, Points);
                    //g.DrawLines(Pens.Gray, Points);
                }
                if (ChB_Highlight.Checked)
                {
                    aBrush.Color = Color.Green;
                    g.FillRegion(aBrush, selRegion);
                    //g.FillPath(aBrush, selPath);
                }
            }

            //Draw contour lines
            if (CB_ContourLine.Checked)
            {
                List<PolyLine> drawLines = _contourLines;
                if (CB_Clipped.Checked)
                    drawLines = _clipContourLines;

                for (i = 0; i <= drawLines.Count - 1; i++)
                {
                    aline = drawLines[i];
                    aValue = aline.Value;
                    aColor = _colors[Array.IndexOf(_CValues, aValue)];
                    newPList = aline.PointList;

                    Points = new Point[newPList.Count];
                    for (j = 0; j <= newPList.Count - 1; j++)
                    {
                        aPoint = (wContour.PointD)newPList[j];
                        ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                        Points[j] = new Point(sX, sY);
                    }
                    aPen = new Pen(Color.Black);
                    aPen.Color = aColor;
                    g.DrawLines(aPen, Points);
                }
            }

            //Draw border lines
            if (CB_BorderLines.Checked)
            {
                for (i = 0; i < _borders.Count; i++)
                {
                    Border aBorder = _borders[i];
                    for (j = 0; j < aBorder.LineNum; j++)
                    {
                        BorderLine bLine = aBorder.LineList[j];
                        Points = new Point[bLine.pointList.Count];
                        for (int k = 0; k < bLine.pointList.Count; k++)
                        {
                            aPoint = bLine.pointList[k];
                            ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                            Points[k] = new Point(sX, sY);
                        }
                        g.DrawLines(Pens.Purple, Points);
                        //g.DrawString(i.ToString(), new Font("Arial", 8), new SolidBrush(Color.Red), Points[0].X, Points[0].Y);
                    }
                }
            }

            //Draw clip line
            if (CB_Clipped.Checked && _clipLines.Count > 0)
            {
                foreach (List<PointD> cLine in _clipLines)
                {
                    Points = new Point[cLine.Count];
                    for (i = 0; i < cLine.Count; i++)
                    {
                        aPoint = cLine[i];
                        ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                        Points[i] = new Point(sX, sY);
                    }
                    g.DrawLines(Pens.Purple, Points);
                }
            }

            //Draw data points
            Font drawFont = new Font("Arial", 8);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            if (CB_DiscreteData.Checked && _discreteData != null)
            {
                for (i = 0; i < _discreteData.GetLength(1); i++)
                {
                    ToScreen(_discreteData[0, i], _discreteData[1, i], ref sX, ref sY);
                    g.DrawEllipse(Pens.Red, sX, sY, 1.5f, 1.5f);
                    //if (_discreteData[2, i] >= 0.1)
                    //    g.DrawString(_discreteData[2, i].ToString(_dFormat), drawFont, drawBrush, sX, sY);
                }
            }
            if (CB_GridData.Checked && _gridData != null)
            {
                for (i = 0; i < _gridData.GetLength(0); i++)
                {
                    for (j = 0; j < _gridData.GetLength(1); j++)
                    {
                        ToScreen(_X[j], _Y[i], ref sX, ref sY);
                        if (!DoubleEquals(_gridData[i, j], _undefData))
                        {
                            g.DrawEllipse(Pens.Red, sX, sY, 1.5f, 1.5f);
                            //if (_gridData[i, j] >= 0.1)
                            //    g.DrawString(_gridData[i, j].ToString(_dFormat), drawFont, drawBrush, sX, sY);
                            //g.DrawString(i.ToString() + "," + j.ToString(), drawFont, drawBrush, sX, sY);
                        }
                        else
                            g.DrawEllipse(Pens.Gray, sX, sY, 1.5f, 1.5f);
                    }
                }
            }

            //Draw stream lines
            if (_streamLines.Count > 0)
            {
                for (i = 0; i < _streamLines.Count; i++)
                {
                    aline = _streamLines[i];
                    aValue = aline.Value;
                    newPList = aline.PointList;

                    PointF[] fPoints = new PointF[newPList.Count];
                    float fX = 0, fY = 0;
                    for (j = 0; j < newPList.Count; j++)
                    {
                        aPoint = (wContour.PointD)newPList[j];
                        ToScreen(aPoint.X, aPoint.Y, ref fX, ref fY);
                        fPoints[j] = new PointF(fX, fY);
                    }
                    aPen = new Pen(Color.Blue);
                    g.DrawLines(aPen, fPoints);

                    int len = 12;
                    for (j = 0; j < fPoints.Length; j++)
                    {
                        if (j > 0 && j < fPoints.Length - 2 && j % len == 0)
                        {
                            //Draw arraw
                            PointF aP = fPoints[j];
                            PointF bPoint = fPoints[j + 1];
                            double U = bPoint.X - aP.X;
                            double V = bPoint.Y - aP.Y;
                            double angle = Math.Atan((V) / (U)) * 180 / Math.PI;
                            angle = angle + 90;
                            if (U < 0)
                                angle = angle + 180;

                            if (angle >= 360)
                                angle = angle - 360;

                            PointF eP1 = new PointF();
                            double aSize = 8;
                            eP1.X = (int)(aP.X - aSize * Math.Sin((angle + 20.0) * Math.PI / 180));
                            eP1.Y = (int)(aP.Y + aSize * Math.Cos((angle + 20.0) * Math.PI / 180));
                            g.DrawLine(aPen, aP, eP1);

                            eP1.X = (int)(aP.X - aSize * Math.Sin((angle - 20.0) * Math.PI / 180));
                            eP1.Y = (int)(aP.Y + aSize * Math.Cos((angle - 20.0) * Math.PI / 180));
                            g.DrawLine(aPen, aP, eP1);
                        }
                    }
                }
            }

            //Draw map lines
            if (_mapLines.Count > 0)
            {
                for (i = 0; i < _mapLines.Count; i++)
                {
                    newPList = _mapLines[i];

                    Points = new Point[newPList.Count];
                    for (j = 0; j <= newPList.Count - 1; j++)
                    {
                        aPoint = newPList[j];
                        ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                        Points[j] = new Point(sX, sY);
                    }
                    aPen = new Pen(Color.Black);
                    aPen.Color = Color.Black;
                    g.DrawLines(aPen, Points);
                    //g.DrawString("0", new Font("Arial", 8), new SolidBrush(Color.Black), Points[0]);
                }
            }

            //---- Draw legend
            if (CB_ContourPolygon.Checked)
            {
                if (_legendPolygons.Count > 0)
                {
                    wContour.Legend.lPolygon aLPolygon = default(wContour.Legend.lPolygon);
                    SolidBrush aBrush = new SolidBrush(Color.Black);
                    for (i = 0; i < _legendPolygons.Count; i++)
                    {
                        aLPolygon = _legendPolygons[i];
                        aValue = aLPolygon.value;
                        if (aLPolygon.isFirst)
                        {
                            aColor = _colors[0];
                        }
                        else
                        {
                            aColor = _colors[Array.IndexOf(_CValues, aValue) + 1];
                        }
                        newPList = aLPolygon.pointList;

                        Points = new Point[newPList.Count];
                        for (j = 0; j <= newPList.Count - 1; j++)
                        {
                            aPoint = newPList[j];
                            ToScreen(aPoint.X, aPoint.Y, ref sX, ref sY);
                            Points[j] = new Point(sX, sY);
                        }
                        aBrush.Color = aColor;
                        g.FillPolygon(aBrush, Points);
                        g.DrawPolygon(Pens.Black, Points);

                        Point sPoint = Points[2];
                        if (i < _legendPolygons.Count - 1)
                        {
                            g.DrawString(_CValues[i].ToString("0.0"), drawFont, Brushes.Black, sPoint.X - 10, sPoint.Y - 15);
                        }
                    }
                }
            }
        }

        public void ClearObjects()
        {
            _discreteData = null;
            _gridData = null;
            _borders = new List<Border>();
            _contourLines = new List<PolyLine>();
            _contourPolygons = new List<Polygon>();
            _clipLines = new List<List<PointD>>();            
            _clipContourLines = new List<PolyLine>();
            _clipContourPolygons = new List<Polygon>();
            _mapLines = new List<List<PointD>>();
            _legendPolygons = new List<Legend.lPolygon>();
            _streamLines = new List<PolyLine>();
        }

        public void SetCoordinate(double minX, double maxX, double minY, double maxY)
        {
            _minX = minX;
            _maxX = maxX;
            _minY = minY;
            _maxY = maxY;            
            _scaleX = (this.PictureBox1.Width - 10) / (_maxX - _minX);
            _scaleY = (this.PictureBox1.Height - 10) / (_maxY - _minY);
            this.PictureBox1.Refresh();
        }

        private void ToScreen(double pX, double pY, ref int sX, ref int sY)
        {
            sX = (int)((pX - _minX) * _scaleX);
            sY = (int)((_maxY - pY) * _scaleY);
        }

        private void ToScreen(double pX, double pY, ref float sX, ref float sY)
        {
            sX = (float)((pX - _minX) * _scaleX);
            sY = (float)((_maxY - pY) * _scaleY);
        }

        private void ToCoordinate(int sX, int sY, ref double pX, ref double pY)
        {
            pX = sX / _scaleX + _minX;
            pY = _maxY - sY / _scaleY;
        }

        public void SmoothLines()
        {
            _contourLines = Contour.SmoothLines(_contourLines);
        }  

        public void SmoothLines(float step)
        {
            _contourLines = Contour.SmoothLines(_contourLines, step);
        }        

        public void GetEcllipseClipping()
        {
            _clipLines = new List<List<PointD>>();

            //---- Generate border with ellipse
            double x0 = 0;
            double y0 = 0;
            double a = 0;
            double b = 0;
            double c = 0;
            bool ifX = false;
            x0 = this.PictureBox1.Width / 2;
            y0 = this.PictureBox1.Height / 2;
            double dist = 0;
            dist = 100;
            a = x0 - dist;
            b = y0 - dist / 2;
            if (a > b)
            {
                ifX = true;
            }
            else
            {
                ifX = false;
                c = a;
                a = b;
                b = c;
            }

            int i = 0;
            int n = 0;
            n = 100;
            double nx = 0;
            double x1 = 0;
            double y1 = 0;
            double ytemp = 0;
            List<PointD> pList = new List<PointD>();
            List<PointD> pList1 = new List<PointD>();
            wContour.PointD aPoint;
            nx = (x0 * 2 - dist * 2) / n;
            for (i = 1; i <= n; i++)
            {
                x1 = dist + nx / 2 + (i - 1) * nx;                
                if (ifX)
                {
                    ytemp = Math.Sqrt((1 - Math.Pow((x1 - x0), 2) / Math.Pow(a, 2)) * Math.Pow(b, 2));
                    y1 = y0 + ytemp;
                    aPoint = new PointD();
                    aPoint.X = x1;
                    aPoint.Y = y1;
                    pList.Add(aPoint);
                    aPoint = new PointD();
                    aPoint.X = x1;
                    y1 = y0 - ytemp;
                    aPoint.Y = y1;
                    pList1.Add(aPoint);
                }
                else
                {
                    ytemp = Math.Sqrt((1 - Math.Pow((x1 - x0), 2) / Math.Pow(b, 2)) * Math.Pow(a, 2));
                    y1 = y0 + ytemp;
                    aPoint = new PointD();
                    aPoint.X = x1;
                    aPoint.Y = y1;
                    pList1.Add(aPoint);
                    aPoint = new PointD();
                    aPoint.X = x1;
                    y1 = y0 - ytemp;
                    aPoint.Y = y1;
                    pList1.Add(aPoint);
                }
            }

            aPoint = new PointD();
            if (ifX)
            {                
                aPoint.X = x0 - a;
            }
            else
            {
                aPoint.X = x0 - b;
            }
            aPoint.Y = y0;
            List<PointD> cLine = new List<PointD>();
            cLine.Add(aPoint);
            for (i = 0; i <= pList.Count - 1; i++)
            {
                cLine.Add(pList[i]);
            }
            aPoint = new PointD();
            aPoint.Y = y0;
            if (ifX)
            {
                aPoint.X = x0 + a;
            }
            else
            {
                aPoint.X = x0 + b;
            }
            cLine.Add(aPoint);
            for (i = pList1.Count - 1; i >= 0; i += -1)
            {
                cLine.Add(pList1[i]);
            }
            cLine.Add(cLine[0]);
            _clipLines.Add(cLine);
        }

        public void ClipLines()
        {
            _clipContourLines = new List<PolyLine>();
            foreach (List<PointD> cLine in _clipLines)
                _clipContourLines.AddRange(Contour.ClipPolylines(_contourLines, cLine));
        }

        public void ClipPolygons()
        {
            _clipContourPolygons = new List<Polygon>();
            //for (int i = 0; i < _clipLines.Count; i++)
            //    _clipContourPolygons.AddRange(Contour.ClipPolygons(_contourPolygons, _clipLines[i]));

            //_clipContourPolygons.AddRange(Contour.ClipPolygons(_contourPolygons, _clipLines[20]));

            foreach (List<PointD> cLine in _clipLines)
                _clipContourPolygons.AddRange(Contour.ClipPolygons(_contourPolygons, cLine));
        }        

        public void TracingPolygons()
        {           
            _contourPolygons = Contour.TracingPolygons(_gridData, _contourLines, _borders, _CValues);            
        }

        public void ReadMapFile_WMP(string aFile)
        {
            StreamReader sr = new StreamReader(aFile);
            string aLine;
            string shapeType;
            string[] dataArray;
            int shapeNum;
            int i, pNum;            
            PointD aPoint;                                  

            //Read shape type
            shapeType = sr.ReadLine().Trim();
            //Read shape number
            shapeNum = Convert.ToInt32(sr.ReadLine());
            _clipLines = new List<List<PointD>>();
            switch (shapeType)
            {                                
                case "Polygon":
                    for (int s = 0; s < shapeNum; s++)
                    {
                        pNum = Convert.ToInt32(sr.ReadLine());
                        List<PointD> cLine = new List<PointD>();
                        for (i = 0; i < pNum; i++)
                        {
                            aLine = sr.ReadLine();
                            dataArray = aLine.Split(',');
                            aPoint = new PointD();
                            aPoint.X = Convert.ToDouble(dataArray[0]);
                            aPoint.Y = Convert.ToDouble(dataArray[1]);
                            cLine.Add(aPoint);
                        }
                        _clipLines.Add(cLine);
                    }                       
                    break;
                default:
                    MessageBox.Show("Shape type is invalid!" + Environment.NewLine +
                        shapeType, "Error");                    
                    break;
            }

            sr.Close();            
        }

        public void CreateLegend()
        {
            wContour.Legend aLegend = new wContour.Legend();
            wContour.PointD aPoint = new PointD();

            double width = _maxX - _minX;
            aPoint.X = _minX + width / 4;
            aPoint.Y = _minY + width / 100;
            wContour.Legend.legendPara lPara = new Legend.legendPara();
            lPara.startPoint = aPoint;
            lPara.isTriangle = true;
            lPara.isVertical = false;
            lPara.length = width / 2;
            lPara.width = width / 100;
            lPara.contourValues = _CValues;

            _legendPolygons = Legend.CreateLegend(lPara);
        }

        private void Lab_StartColor_Click(object sender, EventArgs e)
        {
            ColorDialog aDlg = new ColorDialog();
            aDlg.Color = this.Lab_StartColor.BackColor;
            if (aDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Lab_StartColor.BackColor = aDlg.Color;
                _startColor = aDlg.Color;
                _colors = CreateColors(_startColor, _endColor, _CValues.Length + 1);
                this.PictureBox1.Refresh();
            }
        }

        private void Lab_EndColor_Click(object sender, EventArgs e)
        {
            ColorDialog aDlg = new ColorDialog();
            aDlg.Color = this.Lab_EndColor.BackColor;
            if (aDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Lab_EndColor.BackColor = aDlg.Color;
                _endColor = aDlg.Color;
                _colors = CreateColors(_startColor, _endColor, _CValues.Length + 1);
                this.PictureBox1.Refresh();
            }
        }

        private void B_About_Click(object sender, EventArgs e)
        {
            frmAbout aFrm = new frmAbout();
            aFrm.ShowDialog();
        }              

        private void OutputSurferGridData()
        {
            SaveFileDialog aDlg = new SaveFileDialog();
            aDlg.Filter = "Surfer Grid (*.grd)|*.grd";
            if (aDlg.ShowDialog() == DialogResult.OK)
            {
                double min = 0, max = 0;
                GetMinMaxValues(ref min, ref max);

                StreamWriter sw = new StreamWriter(aDlg.FileName);
                sw.WriteLine("DSAA");
                sw.WriteLine(_X.Length.ToString() + " " + _Y.Length.ToString());
                sw.WriteLine(_X[0].ToString() + " " + _X[_X.Length - 1].ToString());
                sw.WriteLine(_Y[0].ToString() + " " + _Y[_Y.Length - 1].ToString());
                sw.WriteLine(min.ToString() + " " + max.ToString());
                double value;
                string aLine = string.Empty;
                for (int i = 0; i < _Y.Length; i++)
                {
                    for (int j = 0; j < _X.Length; j++)
                    {
                        //if (_gridData[i, j] == _undefData)
                        //    value = 1.70141e+038;
                        //else
                        //    value = _gridData[i, j];

                        value = _gridData[i, j];

                        if (j == 0)
                            aLine = value.ToString();
                        else
                            aLine = aLine + " " + value.ToString();
                    }
                    sw.WriteLine(aLine);
                }

                sw.Close();
            }
        }

        private void GetMinMaxValues(ref double min, ref double max)
        {
            int dNum = 0;
            for (int i = 0; i < _gridData.GetLength(0); i++)
            {
                for (int j = 0; j < _gridData.GetLength(1); j++)
                {
                    if (_gridData[i, j] == _undefData)
                        continue;

                    if (dNum == 0)
                    {
                        min = _gridData[i, j];
                        max = min;
                    }
                    else
                    {
                        if (min > _gridData[i, j])
                            min = _gridData[i, j];
                        else if (max < _gridData[i, j])
                            max = _gridData[i, j];
                    }
                    dNum += 1;
                }
            }
        }

        private void TSMI_RandomData_Click(object sender, EventArgs e)
        {
            frmRandomData aFrm = new frmRandomData();
            aFrm.Show(this);
        }

        private void TSMI_About_Click(object sender, EventArgs e)
        {
            frmAbout aFrm = new frmAbout();
            aFrm.ShowDialog();
        }

        private void CB_DiscreteData_CheckedChanged(object sender, EventArgs e)
        {
            this.PictureBox1.Refresh();
        }

        private void CB_GridData_CheckedChanged(object sender, EventArgs e)
        {
            this.PictureBox1.Refresh();
        }

        private void CB_BorderLines_CheckedChanged(object sender, EventArgs e)
        {
            this.PictureBox1.Refresh();
        }

        private void CB_ContourLine_CheckedChanged(object sender, EventArgs e)
        {
            this.PictureBox1.Refresh();
        }

        private void CB_ContourPolygon_CheckedChanged(object sender, EventArgs e)
        {
            this.PictureBox1.Refresh();
        }

        private void TSMI_IDWNeighbour_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            _dFormat = "0.0";
            string mapFile = Path.Combine(Application.StartupPath, "China.wmp");
            ReadMapFile_WMP(mapFile);
            _mapLines = new List<List<PointD>>(_clipLines);
            string dataFile = Path.Combine(Application.StartupPath, "Temp_2010101420.csv");
            ReadCVSData(dataFile);

            //Interpolation
            int rows = 80;
            int cols = 80;
            Interpolate.CreateGridXY_Num(60, -20, 140, 60, rows, cols, ref _X, ref _Y);
            _gridData = new double[rows, cols];
            _gridData = Interpolate.Interpolation_IDW_Neighbor(_discreteData, _X, _Y, 8, _undefData);

            //Contour
            double[] values = new double[] {-5,0,5,10,15,20,25,30,35,40};
            SetContourValues(values);
            TracingContourLines();
            SmoothLines();
            ClipLines();
            TracingPolygons();
            ClipPolygons();            

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = true;

            SetCoordinate(70, 140, 14, 55);
            CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }        

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            _dFormat = "0.0";
            string mapFile = Path.Combine(Application.StartupPath, "China1.wmp");
            ReadMapFile_WMP(mapFile);
            _mapLines = new List<List<PointD>>(_clipLines);  
            string dataFile = Path.Combine(Application.StartupPath, "testGrid.dat");
            ReadSuferGridData(dataFile);

            //Contour
            //double[] values = new double[] { 0.1, 1, 2, 5, 10, 20, 25, 50, 100 };
            double[] values = new double[] { 1, 2 };
            SetContourValues(values);
            TracingContourLines();
            SmoothLines();
            ClipLines();
            TracingPolygons();
            ClipPolygons();

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = true;

            SetCoordinate(70, 140, 14, 55);
            CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }

        private void TB_Hightlight_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(NUD_Highlight.Text, out _highlightIdx))
                this.PictureBox1.Refresh();
        }

        private void CB_Clipped_CheckedChanged(object sender, EventArgs e)
        {
            this.PictureBox1.Refresh();
        }

        private void PictureBox1_Resize(object sender, EventArgs e)
        {
            _scaleX = (this.PictureBox1.Width - 10) / (_maxX - _minX);
            _scaleY = (this.PictureBox1.Height - 10) / (_maxY - _minY);
            this.PictureBox1.Refresh();
        }

        private void ReadCVSData(string aFile)
        {
            //Read data
            StreamReader sr = new StreamReader(aFile, System.Text.Encoding.Default);
            string[] dataArray, fieldArray;
            List<string[]> dataList = new List<string[]>();
            string aLine = sr.ReadLine();    //Title
            fieldArray = aLine.Split(',');                                    
            aLine = sr.ReadLine();    //First line              
            while (aLine != null)
            {
                dataArray = aLine.Split(',');
                if (dataArray.Length < 3)
                {
                    aLine = sr.ReadLine();
                    continue;
                }

                dataList.Add(dataArray);

                aLine = sr.ReadLine();
            }

            sr.Close();

            _discreteData = new double[3, dataList.Count];
            for (int i = 0; i < dataList.Count; i++)
            {
                _discreteData[0, i] = double.Parse(dataList[i][1]);
                _discreteData[1, i] = double.Parse(dataList[i][2]);
                _discreteData[2, i] = double.Parse(dataList[i][3]);
            }
        }

        private void ReadDatFile(string aFile)
        {
            //Read data
            StreamReader sr = new StreamReader(aFile, System.Text.Encoding.Default);
            string[] dataArray;
            List<string[]> dataList = new List<string[]>();            
            string aLine = sr.ReadLine();    //First line              
            while (aLine != null)
            {
                dataArray = aLine.Split(',');
                if (dataArray.Length < 3)
                {
                    aLine = sr.ReadLine();
                    continue;
                }

                dataList.Add(dataArray);

                aLine = sr.ReadLine();
            }

            sr.Close();

            _discreteData = new double[3, dataList.Count];
            for (int i = 0; i < dataList.Count; i++)
            {
                _discreteData[0, i] = double.Parse(dataList[i][0]);
                _discreteData[1, i] = double.Parse(dataList[i][1]);
                _discreteData[2, i] = double.Parse(dataList[i][2]);
            }
        }

        private void TSMI_IDWRadius_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            _dFormat = "0.0";
            string mapFile = Path.Combine(Application.StartupPath, "China.wmp");
            ReadMapFile_WMP(mapFile);
            _mapLines = new List<List<PointD>>(_clipLines);
            string dataFile = Path.Combine(Application.StartupPath, "Prec_2010101420.csv");
            ReadCVSData(dataFile);

            //Interpolation
            int rows = 160;
            int cols = 160;
            Interpolate.CreateGridXY_Num(60, -20, 140, 60, rows, cols, ref _X, ref _Y);
            _gridData = new double[rows, cols];
            _gridData = Interpolate.Interpolation_IDW_Radius(_discreteData, _X, _Y, 4, 2, _undefData);            

            //Contour
            double[] values = new double[] { 0.1, 1, 2, 5, 10, 20, 25, 50, 100 };
            SetContourValues(values);
            TracingContourLines();
            SmoothLines();
            ClipLines();
            TracingPolygons();
            ClipPolygons();            

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = true;

            SetCoordinate(70, 140, 14, 55);
            CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }

        private static bool DoubleEquals(double a, double b)
        {
            if (Math.Abs(a - b) < 0.000001)
                return true;
            else
                return false;
        }

        private void TSMI_Cressman_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            _dFormat = "0.0";
            string mapFile = Path.Combine(Application.StartupPath, "China.wmp");
            ReadMapFile_WMP(mapFile);
            _mapLines = new List<List<PointD>>(_clipLines);
            string dataFile = Path.Combine(Application.StartupPath, "Prec_2010101420.csv");
            ReadCVSData(dataFile);

            //Interpolation
            int rows = 80;
            int cols = 140;
            //Interpolate.CreateGridXY_Num(60, -20, 140, 60, rows, cols, ref _X, ref _Y);
            Interpolate.CreateGridXY_Num(70, 15, 140, 55, rows, cols, ref _X, ref _Y);
            _gridData = new double[rows, cols];
            List<double> radList = new List<double>(new double[] { 10, 8, 6, 4, 2 });
            _gridData = Interpolate.Cressman(_discreteData, _X, _Y, _undefData, radList);            

            //Contour
            double[] values = new double[] { 0.1, 1, 2, 5, 10, 20, 25, 50, 100 };
            SetContourValues(values);
            TracingContourLines();
            SmoothLines();
            ClipLines();
            TracingPolygons();
            ClipPolygons();

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = true;

            SetCoordinate(69.9, 140.1, 14.9, 55.1);
            //SetCoordinate(70, 140, 14, 55);
            CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;            
        }

        private void TSMI_UndefineData_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();                       
            string dataFile = Application.StartupPath + "\\test2Grid.dat";
            ReadSuferGridData(dataFile);

            //Contour
            double[] values = new double[] {
                20,
                30,
                40,
                50,
                60,
                70,
                80,
                90
            };
            SetContourValues(values);
            TracingContourLines();
            SmoothLines();
            GetEcllipseClipping();
            ClipLines();
            TracingPolygons();
            ClipPolygons();

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = true;

            SetCoordinate(0,_X[_X.Length - 1], 0, _Y[_Y.Length - 1]);
            CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }

        private void TSMI_Streamline_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            _dFormat = "0.0";
            string mapFile = Application.StartupPath + "\\China.wmp";
            ReadMapFile_WMP(mapFile);
            _mapLines = new List<List<PointD>>(_clipLines);
            string dataFile = Application.StartupPath + "\\uwnd.grd";
            ReadSuferGridData(dataFile);
            double[,] UData = _gridData;
            dataFile = Application.StartupPath + "\\vwnd.grd";
            ReadSuferGridData(dataFile);
            double[,] VData = _gridData;
            
            //Streamline
            _streamLines = Contour.TracingStreamline(UData, VData, _X, _Y, _undefData, 4);

            GB_DrawSet.Enabled = false;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = false;

            SetCoordinate(70, 140, 14, 55);            

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }

        private void ChB_AntiAlias_CheckedChanged(object sender, EventArgs e)
        {
            PictureBox1.Refresh();
        }

        private void NUD_Highlight_ValueChanged(object sender, EventArgs e)
        {
            _highlightIdx = (int)NUD_Highlight.Value;
            PictureBox1.Refresh();
        }

        private void ChB_Highlight_CheckedChanged(object sender, EventArgs e)
        {
            NUD_Highlight.Enabled = ChB_Highlight.Checked;
            PictureBox1.Refresh();
        }

        private void TSMI_Help_Click(object sender, EventArgs e)
        {
            string aFile = Application.StartupPath + "\\wContour.chm";
            if (File.Exists(aFile))
            {
                System.Diagnostics.Process.Start(aFile);
            }
        }

        private void B_OutputData_Click_1(object sender, EventArgs e)
        {
            OutputSurferGridData();
        }

        private void TSMI_Test_Click(object sender, EventArgs e)
        {
            //test();
            //test1();
            //test3();
            test4();
        }

        private void test1()
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            _dFormat = "0.0";
            string dataFile = Application.StartupPath + "\\test2.grd";
            ReadSuferGridData(dataFile);

            //Contour
            double[] values = new double[] {
                20,
                30,
                40,
                50,
                60,
                70,
                80,
                90
            };
            SetContourValues(values);
            this.TracingContourLines();
            this.SmoothLines();
            this.GetEcllipseClipping();
            this.ClipLines();
            this.TracingPolygons();
            this.ClipPolygons();

            this.GB_DrawSet.Enabled = true;
            this.CB_DiscreteData.Checked = false;
            this.CB_GridData.Checked = false;
            this.CB_BorderLines.Checked = false;
            this.CB_ContourLine.Checked = false;
            this.CB_ContourPolygon.Checked = true;
            this._dFormat = "0";

            this.SetCoordinate(-10, this.PictureBox1.Width, -10, this.PictureBox1.Height);
            this.CreateLegend();
            this.PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }

        private void test()
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            string dataFile = Application.StartupPath + "\\rain.grd";
            ReadSuferGridData(dataFile);

            //Contour
            double[] values = new double[] {
                0,
                3,
                6,
                9,
                12,
                15,
                80,
                90
            };
            _undefData = 1.70141E+38;
            SetContourValues(values);
            TracingContourLines();
            SmoothLines();
            GetEcllipseClipping();
            ClipLines();
            TracingPolygons();
            ClipPolygons();

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = true;

            SetCoordinate(0, _X[_X.Length - 1], 0, _Y[_Y.Length - 1]);
            CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }

        private void test3()
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            string dataFile = "D:\\Temp\\wContourAndInterpolationTest\\TestData\\PolarizationTilt.grd";
            ReadSuferGridData(dataFile);

            //Contour
            double[] levels = new double[] {
                -90,
                -72,
                -54,
                -36,
                -18,
                0,
                18,
                36,
                54,
                72,
                90
            };
            _CValues = levels;
            _colors = CreateColors(_startColor, _endColor, levels.Length + 1);
            double undefData = -9999.0;

            Int32[,] S1 = new Int32[1, 1];
            //Find contours
            List<wContour.Border> borders = wContour.Contour.TracingBorders(_gridData, _X, _Y, ref S1, undefData);
            List<PolyLine> contourLines = wContour.Contour.TracingContourLines(_gridData, _X, _Y, levels.Length, levels, undefData, borders, S1);
            List<wContour.Polygon> contourPolygons = wContour.Contour.TracingPolygons(_gridData, contourLines, borders, levels);
            this._contourLines = contourLines;
            this._contourPolygons = contourPolygons;

            //SetContourValues(values);
            //TracingContourLines();
            //SmoothLines();
            //GetEcllipseClipping();
            //ClipLines();
            //TracingPolygons();
            //ClipPolygons();

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = true;

            SetCoordinate(_X[0], _X[_X.Length - 1], _Y[0], _Y[_Y.Length - 1]);
            CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }

        private void test2()
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            string dataFile = @"F:\Temp\MODIS\test.grd";
            ReadSuferGridData(dataFile);

            //Contour
            double[] values = new double[] {
                0.8
            };
            _undefData = -9999;
            SetContourValues(values);
            TracingContourLines();
            //SmoothLines();
            //GetEcllipseClipping();
            //ClipLines();
            //TracingPolygons();
            //ClipPolygons();

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = true;
            CB_ContourLine.Checked = false;
            //CB_ContourPolygon.Checked = true;

            SetCoordinate(_X[0] - 5, _X[_X.Length - 1], _Y[0] + 5, _Y[_Y.Length - 1]);
            //CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }

        private void test4()
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            ClearObjects();
            _dFormat = "0.0";
            string dataFile = "D:\\Temp\\test\\2015-01-03_15h11m29s_owcurves.dat";
            ReadDatFile(dataFile);

            //Interpolation
            int rows = 30;
            int cols = 26;
            int i, j;
            _X = new double[cols];
            _Y = new double[rows];
            _gridData = new double[rows, cols];
            for (i = 0; i < cols; i++)
            {
                _X[i] = _discreteData[0, 0] + i * 80;
            }

            for (i = 0; i < rows; i++)
            {
                _Y[i] = _discreteData[1, 0] + i * 80;
            }

            for (j = 0; j < rows; j++)
            {
                for (i = 0; i < cols; i++)
                {
                    _gridData[j, i] = _discreteData[2, j * cols + i];
                }
            }

            //Contour
            double[] values = new double[] { 0, 1, 3, 5, 10, 20, 40 };
            SetContourValues(values);
            TracingContourLines();
            //SmoothLines(0.05f);
            SmoothLines(0.1f);
            ClipLines();
            TracingPolygons();
            ClipPolygons();

            GB_DrawSet.Enabled = true;
            CB_DiscreteData.Checked = false;
            CB_GridData.Checked = false;
            CB_BorderLines.Checked = false;
            CB_ContourLine.Checked = false;
            CB_ContourPolygon.Checked = true;

            SetCoordinate(_X[0] - 10, _X[cols - 1], _Y[0], _Y[rows - 1] + 10);
            CreateLegend();

            PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }
    }
}
