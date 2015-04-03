using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Draw
{
	public partial class Form1 : Form
	{
		Point pt1;
        Color penColor = Color.FromArgb(255, 255, 0, 0);
        int penWidth = 10;
        List<Shape> shapeList = new List<Shape>();
        ShapeType currentShape = ShapeType.Line;
        Shape shape;
        bool dataModified = false;
        string currentFile;
		bool mouseDown = false;

		public Form1()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
            Shape.penTemp = new Pen(Color.Black);
		}

		private void Form1_MouseDown(object sender, MouseEventArgs e)
		{
			pt1 = e.Location;
			Pen pen = new Pen(penColor, penWidth);
            shape = Shape.CreateShape(currentShape, pt1, pen);
			mouseDown = true;
		}

		private void Form1_MouseMove(object sender, MouseEventArgs e)
		{
			if (!mouseDown)
				return;	// Don't respond if mouseDown flag not set

			Graphics g = this.CreateGraphics();
			if (e.Button == MouseButtons.Left)
			{
				Invalidate();
				Update();
                shape.mouseMove(g, e);
			}
		}

		private void Form1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				return;	// Don't respond to right mouse button up
			if (!mouseDown)
				return;	// Don't respond if mouseDown flag not set

			Graphics g = this.CreateGraphics();
            shape.mouseMove(g, e);
            shapeList.Add(shape);
			Invalidate();
            dataModified = true;
			mouseDown = false;
        }

		private void Form1_Paint(object sender, PaintEventArgs e)
		{
            foreach (Shape shape in shapeList)
                shape.Draw(e.Graphics, shape.penFinal);
        }

		private void penWidthMenuItem_Click(object sender, EventArgs e)
		{
			PenDialog dlg = new PenDialog();
            dlg.PenWidth = penWidth;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                penWidth = dlg.PenWidth;
            }
		}

        private void lineMenuItem_Click(object sender, EventArgs e)
        {
            currentShape = ShapeType.Line;
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentShape = ShapeType.Rect;
        }

        private void freeLineMenuItem_Click(object sender, EventArgs e)
        {
            currentShape = ShapeType.FreeLine;
        }

        private void printShapesMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\nAll Shapes");
            foreach (Shape shape in shapeList)
                Console.WriteLine(shape);
        }

        private void penColorMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = penColor;
            if (dlg.ShowDialog() == DialogResult.OK)
                penColor = dlg.Color;

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Text files (*.txt)|*.txt| Binary files (*.bin)|*.bin |   All files (*.*)|*.*";
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            DialogResult dr = dlg.ShowDialog();
            if (dr == DialogResult.Cancel)
                return;
               saveFile(dlg.FileName);
           
        }

        private void saveFile(String fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            FileInfo info = new FileInfo(fileName);
            currentFile = fileName;
            if (info.Extension.Equals(".txt"))
            {
                StreamWriter sw = new StreamWriter(fs);
                foreach (Shape s in shapeList)
                {
                    s.writeText(sw);
                }
                sw.Close();
            }
            else if (info.Extension.Equals(".bin"))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                foreach (Shape s in shapeList)
                {
                    s.writeBinary(bw);
                }
                bw.Close();
            }
            dataModified = false;
          
            fs.Close();

        }

        private void clearForm()
        {
            penColor = Color.FromArgb(255, 255, 0, 0);
            penWidth = 10;
            shapeList.Clear();
            currentShape = ShapeType.Line;
            dataModified = false;
            mouseDown = false;

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                //call saveAs
                saveAsToolStripMenuItem_Click(sender, e);

            }
            else
            {
                saveFile(currentFile);
            }
        } 

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModified)
            {
                //ask if you want to save
                DialogResult result = showSaveChanges();
                switch (result)
                {
                    case DialogResult.Yes://call save  break;
                        saveToolStripMenuItem_Click(sender, e); break;
                    case DialogResult.No: currentFile = null;
                       break;
                    case DialogResult.Cancel: return;
                }
            }
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Text files (*.txt)|*.txt| Binary files (*.bin)|*.bin |   All files (*.*)|*.*";
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            DialogResult dr = dlg.ShowDialog();
            if (dr == DialogResult.Cancel)
                return;

            clearForm();
            this.CreateGraphics().Clear(Form1.ActiveForm.BackColor);
           
            if (File.Exists(dlg.FileName))
            {
                //clear graphics
                this.CreateGraphics().Clear(Form1.ActiveForm.BackColor);
                FileStream fs = new FileStream(dlg.FileName, FileMode.Open);
                currentFile = dlg.FileName;
                FileInfo info = new FileInfo(dlg.FileName);
                if (info.Extension.Equals(".txt"))
                {
                    StreamReader sr = new StreamReader(fs);
                    String s = sr.ReadLine();
                    while (s != null)
                    {
                        Shape shape1 = Shape.CreateShape((ShapeType)Enum.Parse(typeof(ShapeType), s, false));
                        shape1.readText(sr);

                        String colorLine = sr.ReadLine();
                        int alphaVal = int.Parse(colorLine.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        int redVal = int.Parse(colorLine.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        int greenVal = int.Parse(colorLine.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                        int blueVal = int.Parse(colorLine.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                        Color color = Color.FromArgb(alphaVal, redVal, greenVal, blueVal);
                        float width = Convert.ToInt32(sr.ReadLine());
                        Pen penFinal = new Pen(color, width);
                        shape1.penFinal = penFinal;
                        shape1.Draw(this.CreateGraphics(), shape1.penFinal);
                        shapeList.Add(shape1);
                        s = sr.ReadLine();
                        
                    }
                    sr.Close();
                }
                else if (info.Extension.Equals(".bin"))
                {
                    BinaryReader br = new BinaryReader(fs);
                    String s = br.ReadString();
                    while (s != null)
                    {
                        Shape shape1 = Shape.CreateShape((ShapeType)Enum.Parse(typeof(ShapeType), s, false));
                        shape1.readBinary(br);
                        int blue = Convert.ToInt32(br.ReadByte());
                        int green = Convert.ToInt32(br.ReadByte());
                        int red = Convert.ToInt32(br.ReadByte());
                        int alpha = Convert.ToInt32(br.ReadByte());
                        Color color  = Color.FromArgb(alpha, red, green, blue);
                        float width = br.ReadSingle();
                        Pen penFinal = new Pen(color, width);
                        shape1.penFinal = penFinal;
                        
                        shapeList.Add(shape1);
                        shape1.Draw(this.CreateGraphics(), shape1.penFinal);
                        if (br.PeekChar() != -1)
                        {
                            s = br.ReadString();
                        }
                        else
                        {
                            break;
                        }
                    }
                    br.Close();
                }
            }
        }

        private DialogResult showSaveChanges()
        {
            string messageBoxText = "Do you want to save changes?";
            string caption = "Image Builder";
            MessageBoxButtons button = MessageBoxButtons.YesNoCancel;
            MessageBoxIcon icon = MessageBoxIcon.Warning;
            return MessageBox.Show(messageBoxText, caption, button, icon);
        }
       
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModified)
            {
                //ask if you want to save
                DialogResult result = showSaveChanges();
                switch (result)
                {
                    case DialogResult.Yes://call save  break;
                        saveToolStripMenuItem_Click(sender, e); break;
                    case DialogResult.No: currentFile = null;
                        clearForm();
                        this.CreateGraphics().Clear(Form1.ActiveForm.BackColor); break;
                    case DialogResult.Cancel: return;
                }
            }
            
                clearForm();
                this.CreateGraphics().Clear(Form1.ActiveForm.BackColor);
            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModified)
            {
                //ask if you want to save
                DialogResult result = showSaveChanges();
                switch (result)
                {
                    case DialogResult.Yes://call save  break;
                        saveToolStripMenuItem_Click(sender, e); break;
                    case DialogResult.No: 
                        break;
                    case DialogResult.Cancel: return;
                }
            }
            Application.Exit();
        } 

	}  // end class Form1   
            
}