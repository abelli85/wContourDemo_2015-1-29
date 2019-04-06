using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Telerik.Windows.Controls;
using wContour;

namespace ContourSLDemo
{
    public partial class MainPage : System.Windows.Controls.UserControl
    {

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
        List<Legend.lPolygon> _legendPolygons = new List<Legend.lPolygon>();
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

        public MainPage()
        {
            InitializeComponent();
        }

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {
            for (var i = LayoutRoot.Children.Count - 1; i >= 0; i--)
            {
                if (!(LayoutRoot.Children[i] is RadButton))
                {
                    LayoutRoot.Children.Remove(LayoutRoot.Children[i]);
                }
            }

            CreateDiscreteData(30);
            InterpolateData(50, 50);

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
            CreateLegend();

            Debug.WriteLine($"contour lines: {_clipContourLines.Count}.");

            _clipContourLines.ForEach(pline =>
            {
                var head = pline.PointList.First();

                var path = new System.Windows.Shapes.Path();
                var pg = new PathGeometry();
                var pf = new PathFigure()
                {
                    StartPoint = new Point(head.X, head.Y),
                    IsClosed = false,
                };

                var pseg = new PolyLineSegment();
                pline.PointList.ForEach(pd =>
                {
                    pseg.Points.Add(new Point(pd.X, pd.Y));
                });

                Debug.WriteLine($"{pline.Type}/{pline.Value}({pline.BorderIdx}): {pline.PointList.Count}.");
                pf.Segments.Add(pseg);

                pg.Figures.Add(pf);
                path.Data = pg;
                path.Stroke = new SolidColorBrush(Colors.Blue);
                path.StrokeThickness = 2;

                LayoutRoot.Children.Add(path);

                // text
                var tseg = new System.Windows.Controls.TextBlock();
                tseg.Text = $"{pline.Value}";
                tseg.Margin = new Thickness(head.X, head.Y, 0, 0);
                LayoutRoot.Children.Add(tseg);
            });

            for (int i = 0; i < 30; i++)
            {
                var pt = new System.Windows.Controls.TextBlock();
                pt.Text = "+" + _discreteData[2, i].ToString("N0");
                pt.Margin = new Thickness(_discreteData[0, i], _discreteData[1, i], 0, 0);
                pt.Foreground = new SolidColorBrush(Colors.Red);

                LayoutRoot.Children.Add(pt);
            }
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
                S[0, i] = random.Next(0, (int)this.ActualWidth);
                S[1, i] = random.Next(0, (int)this.ActualHeight);
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
            Xrt = (int)this.ActualWidth;
            Yrt = (int)this.ActualHeight;
            XDelt = (int)this.ActualWidth / cols;
            YDelt = (int)this.ActualHeight / rows;

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
                colors[i] = Color.FromArgb(128, (byte)(sR + i * rStep), (byte)(sG + i * gStep), (byte)(sB + i * bStep));
            }

            return colors;
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
            _scaleX = (this.ActualWidth - 10) / (_maxX - _minX);
            _scaleY = (this.ActualHeight - 10) / (_maxY - _minY);
            this.UpdateLayout();
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
            _contourLines = Contour.SmoothLines(_contourLines);
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
            x0 = this.ActualWidth / 2;
            y0 = this.ActualHeight / 2;
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
            PointD aPoint;
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

        public void CreateLegend()
        {
            Legend aLegend = new Legend();
            PointD aPoint = new PointD();

            double width = _maxX - _minX;
            aPoint.X = _minX + width / 4;
            aPoint.Y = _minY + width / 100;
            Legend.legendPara lPara = new Legend.legendPara();
            lPara.startPoint = aPoint;
            lPara.isTriangle = true;
            lPara.isVertical = false;
            lPara.length = width / 2;
            lPara.width = width / 100;
            lPara.contourValues = _CValues;

            _legendPolygons = Legend.CreateLegend(lPara);
        }
    }
}
