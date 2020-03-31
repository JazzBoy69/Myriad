using System;
using System.IO;
using Myriad.Data;

namespace Myriad.Library
{

    public class ImageElement
    {
        const double landscapeRatio = .70;
        const double portraitRatio = 1.18;
        private string path;
        private double height = -1.0;
        private double width;
        private double widthpercentage = 1;
        private double ratio;
        private bool valid = false;
        public static string pictureDirectory = System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot", "pictures");
        public static string pictureSourceDirectory = "pictures";
        string filename;


        public ImageElement(string p)
        {
            filename = p.Replace("[[", "").Replace("]]", "");
            GetDimensionsFromFile();
        }

        public void GetDimensionsFromFile()
        {
            string path = System.IO.Path.Combine(pictureDirectory, filename);
            this.path = System.IO.Path.Combine(pictureSourceDirectory, filename);
            if (File.Exists(path))
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                height = (double)img.Height;
                width = (double)img.Width;
                ratio = height / width;
                valid = true;
            }
        }

        public double Ratio
        { get { return ratio; } }

        public bool Valid { get { return valid; } }

        public string Pattern
        {
            get
            {
                if (ratio < landscapeRatio) return "l";
                if (ratio > portraitRatio) return "p";
                return "s";
            }
        }

        public double Height { get { return height; } set { height = value; ratio = height / width; } }

        public double Width { get { return width; } set { width = value; ratio = height / width; } }

        public double WidthPercentage { get { return widthpercentage; } set { widthpercentage = value; } }

        public override string ToString()
        {
            return "<img src=\"" + path + "\" width=\"" + string.Format("{0:F4}%", widthpercentage * 100) + "\" class="+Class+" />";
        }


        public string Class
        {
            get
            {
                if (Ratio < landscapeRatio) return "landscape";
                if (Ratio > portraitRatio) return "portrait";
                return "square";
            }
        }

        public string PlainString
        {
            get
            {
                return "<img src=\"" + path + "\" class=\"" + Class + "\" />";
            }
        }
        public string SiblingString
        {
            get
            {
                return "<img src=\"" + path + "\" width=\"" + string.Format("{0:F4}%", widthpercentage * 100) +"\" class=\"" + Class + " sibling\" />";
            }
        }
		public string RightSiblingString
		{
			get
			{
				return "<img src=\"" + path + "\" width=\"" + string.Format("{0:F4}%", widthpercentage * 100) + "\" class=\"" + Class + " rightside sibling\" />";
			}
		}
		public string LeftSiblingString
		{
			get
			{
				return "<img src=\"" + path + "\" width=\"" + string.Format("{0:F4}%", widthpercentage * 100) + "\" class=\"" + Class + " leftside sibling\" />";
			}
		}
		public string OnlyChildString
        {
            get
            {
                return "<img src=\"" + path + "\" class='" + Class + " single' width=" + WidthString + " />";
            }
        }

        public string Path { get { return path; }}
        public static string WidthString
        {
            get
            {
				return "100%";
                /*double x = 960 * ratio;
                if (Ratio < landscapeRatio)
                {
                    if (x > 500) return string.Format("{0:F4}% ", 500 / x * 100);
                    return "100%";
                }

                if (Ratio > portraitRatio)
                {
                    if (x > 600) return string.Format("{0:F4}% ", 600 / x * 100);
                    return "100%";
                }
                if (x>450) return string.Format("{0:F4}% ", 450 / x * 100);
                return "100%";*/
            }
        }
    }
}