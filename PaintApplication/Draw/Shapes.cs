using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;


namespace Draw
{
	public enum ShapeType { Line, Rect, FreeLine };

    public abstract class Shape
	{
		public Pen penFinal;
		public static Pen penTemp;
		public abstract void Draw(Graphics g, Pen pen);
        public abstract void mouseMove(Graphics g, MouseEventArgs e);
        public abstract void writeBinary(BinaryWriter bw);
        public abstract void readBinary(BinaryReader br);
        public abstract void writeText(StreamWriter sw);
        public abstract void readText(StreamReader sr);

        public Shape(Pen pen)
        {
            this.penFinal = pen;
        }

        public Shape()
        {
			penFinal = penTemp;
        }

        public static Shape CreateShape(ShapeType type, Point pt, Pen pen)
        {
            switch (type)
            {
				case ShapeType.Line:
                    return new Line(pt, pen);
                case ShapeType.Rect:
                    return new Rect(pt, pen);
                case ShapeType.FreeLine:
                    return new FreeLine(pt, pen);
                default:
                    return null;
            }
        }

        public static Shape CreateShape(ShapeType type)
        {
            switch (type)
            {
                case ShapeType.Line:
                    return new Line();
                case ShapeType.Rect:
                    return new Rect();
                case ShapeType.FreeLine:
                    return new FreeLine();
                default:
                    return null;
            }
        }

        public String getHexString(Color color)
        {
            String hexValue = "";
            hexValue = color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return hexValue;
        }

        public Point retrievePointFromStream(StreamReader sr)
        {
            var pointString1 = sr.ReadLine().Split(' ');
            return new Point(
                              int.Parse(pointString1[0]),
                              int.Parse(pointString1[1]));

        }

        public void setPenProperties(StreamReader sr){
            String colorLine = sr.ReadLine();
            int alphaVal = int.Parse(colorLine.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int redVal = int.Parse(colorLine.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int greenVal = int.Parse(colorLine.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            int blueVal = int.Parse(colorLine.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            this.penFinal.Color = Color.FromArgb(alphaVal, redVal, greenVal, blueVal);
            this.penFinal.Width = Convert.ToInt32(sr.ReadLine());
    }

	}  // End Shape class

    public class Line : Shape
	{
        protected Point pt1, pt2;

        public Point Pt1
        {
            get { return pt1; }
            set { pt1 = value; }
        }

        public Point Pt2
        {
            get { return pt2; }
            set { pt2 = value; }
        }
        
        public Line(Point p1, Point p2, Pen pen) : base(pen)
		{
			pt1 = p1;
			pt2 = p2;
		}

		public Line()
		{
			pt1 = Point.Empty;
			pt2 = Point.Empty;
			penFinal = penTemp;
		}

		public Line(Point pt, Pen pen)
		{
            pt1 = pt2 = pt;
			this.penFinal = pen;
		}

		public override void Draw(Graphics g, Pen pen)
		{
			g.DrawLine(pen, pt1, pt2);
		}

		public override void mouseMove(Graphics g, MouseEventArgs e)
		{
			pt2 = e.Location;
			Draw(g, penTemp);
		}

        public override void writeBinary(BinaryWriter bw)
        {
            bw.Write(base.GetType().Name);
            bw.Write(this.Pt1.X.ToString()); bw.Write(this.Pt1.Y.ToString());
            bw.Write(this.Pt2.X.ToString()); bw.Write(this.Pt2.Y.ToString());
            bw.Write(this.penFinal.Color.ToArgb());
            bw.Write(this.penFinal.Width);
        }

        public override void readBinary(BinaryReader br)
        {
 
            pt1 = new Point(
                              int.Parse( br.ReadString()),
                              int.Parse( br.ReadString()));
            pt2 = new Point(
                              int.Parse( br.ReadString()),
                              int.Parse( br.ReadString()));
            
        }

        public override void writeText(StreamWriter sw)
        {
            sw.WriteLine(base.GetType().Name);
            sw.Write(this.Pt1.X); sw.WriteLine(" "+this.Pt1.Y);
            sw.Write(this.Pt2.X); sw.WriteLine(" " + this.Pt2.Y);
            sw.WriteLine(this.getHexString(this.penFinal.Color));
            sw.WriteLine(this.penFinal.Width);
        }

        public override void readText(StreamReader sr)
        {
            //Reading Point
            pt1=retrievePointFromStream(sr); 
            pt2 = retrievePointFromStream(sr);
        }

    } // End line class

    public class Rect : Line
    {
        public Rect(Point p1, Point p2, Pen pen)
            : base(p1, p2, pen)
        { }

		public Rect()
			: base()
		{ }

		public Rect(Point pt, Pen pen)
			: base(pt, pen)
		{ }

        public override void Draw(Graphics g, Pen pen)
        {
			int x = Math.Min(pt1.X, pt2.X);
			int y = Math.Min(pt1.Y, pt2.Y);
            int width = Math.Abs(pt2.X - pt1.X);
            int height = Math.Abs(pt2.Y - pt1.Y);
            Rectangle rect = new Rectangle(x, y, width, height);
            g.DrawRectangle(pen, rect);
        }

		public override void mouseMove(Graphics g, MouseEventArgs e)
		{
			pt2 = e.Location;
			Draw(g, penTemp);
		}

        public override void writeBinary(BinaryWriter bw)
        {
            bw.Write(base.GetType().Name);
            bw.Write(this.Pt1.ToString());
            bw.Write(this.Pt2.ToString());
            bw.Write(this.penFinal.Color.ToArgb());
            bw.Write(this.penFinal.Width);
        }

        public override void readBinary(BinaryReader br)
        {
            String pointString1 = br.ReadString();
            var coordinates = Regex.Replace(pointString1, @"[\{\}a-zA-Z=]", "").Split(',');

            pt1 = new Point(
                              int.Parse(coordinates[0]),
                              int.Parse(coordinates[1]));
            String pointString2 = br.ReadString();
            coordinates = Regex.Replace(pointString2, @"[\{\}a-zA-Z=]", "").Split(',');

            pt2 = new Point(
                              int.Parse(coordinates[0]),
                              int.Parse(coordinates[1]));
            
            
        }

        public override void writeText(StreamWriter sw)
        {
            sw.WriteLine(base.GetType().Name);
            sw.Write(this.Pt1.X); sw.WriteLine(" " + this.Pt1.Y);
            sw.Write(this.Pt2.X); sw.WriteLine(" " + this.Pt2.Y);
            sw.WriteLine(this.getHexString(this.penFinal.Color));
            sw.WriteLine(this.penFinal.Width);
        }

        public override void readText(StreamReader sr)
        {
            //Reading Point
            pt1 = retrievePointFromStream(sr);
            pt2 = retrievePointFromStream(sr); 
        }

    } // End Rect class

    public class FreeLine : Shape
    {
        protected List<Point> freeList;

        public List<Point> FreeList
        {
            get { return freeList; }
            set { freeList = value; }
        }

        public FreeLine(Point pt, Pen pen)
            : base(pen)
        {
            freeList = new List<Point>();
            freeList.Add(pt);
        }

        public FreeLine(Pen pen)
            : base(pen)
        {
            freeList = new List<Point>();
        }

        public FreeLine()
			: base()
        {
            freeList = new List<Point>();
        }

        public override void Draw(Graphics g, Pen pen)
        {
            Point[] ptArray = freeList.ToArray();
            g.DrawLines(pen, ptArray);
        }

		public override void mouseMove(Graphics g, MouseEventArgs e)
		{
			freeList.Add(e.Location);
			Draw(g, penTemp);
        }

        public override void writeBinary(BinaryWriter bw)
        {
            bw.Write(base.GetType().Name);
            foreach (Point pt in freeList) {
                bw.Write(pt.ToString());
            }
            bw.Write("End");
            bw.Write(this.penFinal.Color.ToArgb());
            bw.Write(this.penFinal.Width);
        }

        public override void readBinary(BinaryReader br)
        {
            String s=br.ReadString();
            while (!s.Equals("End"))
            {
               
                var coordinates = Regex.Replace(s, @"[\{\}a-zA-Z=]", "").Split(',');

                Point pt = new Point(
                                  int.Parse(coordinates[0]),
                                  int.Parse(coordinates[1]));
                freeList.Add(pt);
                s = br.ReadString();
             }
            
        }

        public override void writeText(StreamWriter sw)
        {
            sw.WriteLine(base.GetType().Name);
            foreach (Point pt in this.freeList) {
                sw.WriteLine(pt);
            }
            sw.WriteLine("End");
            
            sw.WriteLine(this.getHexString(this.penFinal.Color));
            sw.WriteLine(this.penFinal.Width);
           
        }

        public override void readText(StreamReader sr)
        {
            String s = sr.ReadLine();
            while (!s.Equals("End"))
            {

                var coordinates = Regex.Replace(s, @"[\{\}a-zA-Z=]", "").Split(',');

                Point pt = new Point(
                                  int.Parse(coordinates[0]),
                                  int.Parse(coordinates[1]));
                freeList.Add(pt);
                s = sr.ReadLine();
            }

        }

    } // End FreeLine class
}
