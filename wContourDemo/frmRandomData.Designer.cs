namespace wContourDemo
{
    partial class frmRandomData
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RB_Discrete = new System.Windows.Forms.RadioButton();
            this.RB_Grid = new System.Windows.Forms.RadioButton();
            this.GB_Grid = new System.Windows.Forms.GroupBox();
            this.TB_Col = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.TB_Row = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.GB_Discrete = new System.Windows.Forms.GroupBox();
            this.TB_Count = new System.Windows.Forms.TextBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.B_Contour = new System.Windows.Forms.Button();
            this.GB_Grid.SuspendLayout();
            this.GB_Discrete.SuspendLayout();
            this.SuspendLayout();
            // 
            // RB_Discrete
            // 
            this.RB_Discrete.AutoSize = true;
            this.RB_Discrete.Location = new System.Drawing.Point(33, 99);
            this.RB_Discrete.Name = "RB_Discrete";
            this.RB_Discrete.Size = new System.Drawing.Size(71, 16);
            this.RB_Discrete.TabIndex = 34;
            this.RB_Discrete.TabStop = true;
            this.RB_Discrete.Text = "Discrete";
            this.RB_Discrete.UseVisualStyleBackColor = true;
            this.RB_Discrete.CheckedChanged += new System.EventHandler(this.RB_Discrete_CheckedChanged);
            // 
            // RB_Grid
            // 
            this.RB_Grid.AutoSize = true;
            this.RB_Grid.Location = new System.Drawing.Point(33, 77);
            this.RB_Grid.Name = "RB_Grid";
            this.RB_Grid.Size = new System.Drawing.Size(47, 16);
            this.RB_Grid.TabIndex = 33;
            this.RB_Grid.TabStop = true;
            this.RB_Grid.Text = "Grid";
            this.RB_Grid.UseVisualStyleBackColor = true;
            this.RB_Grid.CheckedChanged += new System.EventHandler(this.RB_Grid_CheckedChanged);
            // 
            // GB_Grid
            // 
            this.GB_Grid.Controls.Add(this.TB_Col);
            this.GB_Grid.Controls.Add(this.Label2);
            this.GB_Grid.Controls.Add(this.TB_Row);
            this.GB_Grid.Controls.Add(this.Label1);
            this.GB_Grid.Location = new System.Drawing.Point(12, 12);
            this.GB_Grid.Name = "GB_Grid";
            this.GB_Grid.Size = new System.Drawing.Size(178, 54);
            this.GB_Grid.TabIndex = 30;
            this.GB_Grid.TabStop = false;
            this.GB_Grid.Text = "Grid points";
            // 
            // TB_Col
            // 
            this.TB_Col.Location = new System.Drawing.Point(130, 23);
            this.TB_Col.Name = "TB_Col";
            this.TB_Col.Size = new System.Drawing.Size(42, 21);
            this.TB_Col.TabIndex = 3;
            this.TB_Col.Text = "20";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(93, 26);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(29, 12);
            this.Label2.TabIndex = 2;
            this.Label2.Text = "Col:";
            // 
            // TB_Row
            // 
            this.TB_Row.Location = new System.Drawing.Point(43, 23);
            this.TB_Row.Name = "TB_Row";
            this.TB_Row.Size = new System.Drawing.Size(42, 21);
            this.TB_Row.TabIndex = 1;
            this.TB_Row.Text = "20";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(5, 26);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(29, 12);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Row:";
            // 
            // GB_Discrete
            // 
            this.GB_Discrete.Controls.Add(this.TB_Count);
            this.GB_Discrete.Controls.Add(this.Label4);
            this.GB_Discrete.Location = new System.Drawing.Point(195, 12);
            this.GB_Discrete.Name = "GB_Discrete";
            this.GB_Discrete.Size = new System.Drawing.Size(113, 54);
            this.GB_Discrete.TabIndex = 32;
            this.GB_Discrete.TabStop = false;
            this.GB_Discrete.Text = "Discrete points";
            // 
            // TB_Count
            // 
            this.TB_Count.Location = new System.Drawing.Point(62, 22);
            this.TB_Count.Name = "TB_Count";
            this.TB_Count.Size = new System.Drawing.Size(42, 21);
            this.TB_Count.TabIndex = 1;
            this.TB_Count.Text = "50";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(11, 25);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(41, 12);
            this.Label4.TabIndex = 0;
            this.Label4.Text = "Count:";
            // 
            // B_Contour
            // 
            this.B_Contour.ForeColor = System.Drawing.Color.Red;
            this.B_Contour.Location = new System.Drawing.Point(160, 82);
            this.B_Contour.Name = "B_Contour";
            this.B_Contour.Size = new System.Drawing.Size(98, 30);
            this.B_Contour.TabIndex = 31;
            this.B_Contour.Text = "Contour";
            this.B_Contour.UseVisualStyleBackColor = true;
            this.B_Contour.Click += new System.EventHandler(this.B_Contour_Click);
            // 
            // frmRandomData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 134);
            this.Controls.Add(this.RB_Discrete);
            this.Controls.Add(this.RB_Grid);
            this.Controls.Add(this.GB_Grid);
            this.Controls.Add(this.GB_Discrete);
            this.Controls.Add(this.B_Contour);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmRandomData";
            this.Text = "Random Data";
            this.Load += new System.EventHandler(this.frmRandomData_Load);
            this.GB_Grid.ResumeLayout(false);
            this.GB_Grid.PerformLayout();
            this.GB_Discrete.ResumeLayout(false);
            this.GB_Discrete.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.RadioButton RB_Discrete;
        internal System.Windows.Forms.RadioButton RB_Grid;
        internal System.Windows.Forms.GroupBox GB_Grid;
        internal System.Windows.Forms.TextBox TB_Col;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.TextBox TB_Row;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.GroupBox GB_Discrete;
        internal System.Windows.Forms.TextBox TB_Count;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Button B_Contour;
    }
}