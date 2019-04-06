using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace wContourDemo
{
    public partial class frmRandomData : Form
    {
        public frmRandomData()
        {
            InitializeComponent();
        }

        private void frmRandomData_Load(object sender, EventArgs e)
        {
            this.RB_Discrete.Checked = true;
        }

        private void RB_Grid_CheckedChanged(object sender, EventArgs e)
        {
            this.GB_Grid.Enabled = true;
            this.GB_Discrete.Enabled = false;
        }

        private void RB_Discrete_CheckedChanged(object sender, EventArgs e)
        {
            this.GB_Discrete.Enabled = true;
        }

        private void B_Contour_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            frmMain.pCurrenWin.ClearObjects();
            int rows = int.Parse(TB_Row.Text);
            int cols = int.Parse(TB_Col.Text);
            if (this.RB_Grid.Checked)
            {
                frmMain.pCurrenWin.CreateGridData(rows, cols);
            }
            else
            {
                frmMain.pCurrenWin.CreateDiscreteData(int.Parse(TB_Count.Text));
                frmMain.pCurrenWin.InterpolateData(rows, cols);
            }
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
            frmMain.pCurrenWin.SetContourValues(values);
            frmMain.pCurrenWin.TracingContourLines();            
            frmMain.pCurrenWin.SmoothLines();
            frmMain.pCurrenWin.GetEcllipseClipping();
            frmMain.pCurrenWin.ClipLines();
            frmMain.pCurrenWin.TracingPolygons();
            frmMain.pCurrenWin.ClipPolygons();            

            frmMain.pCurrenWin.GB_DrawSet.Enabled = true;
            frmMain.pCurrenWin.CB_DiscreteData.Checked = false;
            frmMain.pCurrenWin.CB_GridData.Checked = false;
            frmMain.pCurrenWin.CB_BorderLines.Checked = false;
            frmMain.pCurrenWin.CB_ContourLine.Checked = false;
            frmMain.pCurrenWin.CB_ContourPolygon.Checked = true;
            frmMain.pCurrenWin._dFormat = "0";

            frmMain.pCurrenWin.SetCoordinate(-10, frmMain.pCurrenWin.PictureBox1.Width, -10, frmMain.pCurrenWin.PictureBox1.Height);
            frmMain.pCurrenWin.CreateLegend();
            frmMain.pCurrenWin.PictureBox1.Refresh();

            this.Cursor = Cursors.Default;
        }
    }
}
