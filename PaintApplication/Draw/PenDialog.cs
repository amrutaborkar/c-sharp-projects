using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Draw
{
	public partial class PenDialog : Form
	{
		Color penColor;
		int penWidth;

		public PenDialog()
		{
			InitializeComponent();
 		}

		private void button2_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			penWidth = (int)numericUpDown1.Value;
		}

        public int PenWidth
        {
            set { penWidth = value; numericUpDown1.Value = (decimal)value; } 
            get { return penWidth; }
        }

        public Color PenColor
        {
            set { penColor = value; }
            get { return penColor; }
        }
    }
}