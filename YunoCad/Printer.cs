using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MC = Informatix.MGDS.Cad;

namespace Yuno.Cad
{
    public class Printer
    {
        public static Printer Instance { get; } = new Printer();
        Printer() { }

        public string this[string option]
        {
            get
            {
                MC.GetPrinterSetup(option, out string value);
                return value;
            }
            set
            {
                MC.PrinterSetup(option, value);
            }
        }

        public string Color
        {
            get { return this["COLOR"]; }
            set { this["COLOR"] = value; }
        }

        public string Copies
        {
            get { return this["COPIES"]; }
            set { this["COPIES"] = value; }
        }

        public string Orientation
        {
            get { return this["ORIENTATION"]; }
            set { this["ORIENTATION"] = value; }
        }

        public string PaperSize
        {
            get { return this["PAPERSIZE"]; }
            set { this["PAPERSIZE"] = value; }
        }

        public string PortName
        {
            get { return this["PORTNAME"]; }
        }

        public string PrinterName
        {
            get { return this["PRINTERNAME"]; }
            set { this["PRINTERNAME"] = value; }
        }

        public string PrintQuality
        {
            get { return this["PRINTQUALITY"]; }
            set { this["PRINTQUALITY"] = value; }
        }

        public string UnthickenedWidthColor
        {
            get { return this["UNTHICKENED_WIDTH_COLOR"]; }
            set { this["UNTHICKENED_WIDTH_COLOR"] = value; }
        }

        public string UnthickenedWidthMono
        {
            get { return this["UNTHICKENED_WIDTH_MONO"]; }
            set { this["UNTHICKENED_WIDTH_MONO"] = value; }
        }

        // document?
        public PaperSize PlotPaperSize
        {
            get
            {
                PaperSize ps;
                MC.GetPlotPaperSize(out ps.Width, out ps.Height);
                return ps;
            }
        }

        public void ResetToDefault()
        {
            MC.PlotResetToDefault();
        }

        System.Windows.Forms.DialogResult Setup()
        {
            return MC.PlotSetup();
        }

        // document? view?
        public void PlotView(double scaleFactor = 0)
        {
            MC.PlotView(scaleFactor);
        }
        public void PlotEx(string options)
        {
            MC.PlotViewEx(options);
        }
        
        // document?
        public System.Windows.Forms.DialogResult PlotWnd()
        {
            return MC.PlotWnd();
        }

        // document? view?
        public string Print(string options)
        {
            return MC.Print(options);
        }
    }

    public struct PaperSize
    {
        public double Width;
        public double Height;
    }
}
