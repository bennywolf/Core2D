﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class PdfRenderer : Test2d.IRenderer
    {
        public bool DrawPoints { get; set; }
        public double Zoom { get; set; }

        private Func<double, double> ScaleToPage;

        public void Create(string path, Test2d.Container container)
        {
            using (var doc = new PdfDocument())
            {
                Add(doc, container);
                doc.Save(path);
            }
        }

        public void Create(string path, IEnumerable<Test2d.Container> containers)
        {
            using (var doc = new PdfDocument())
            {
                foreach (var page in containers)
                {
                    Add(doc, page);
                }
                doc.Save(path);
            }
        }

        private void Add(PdfDocument doc, Test2d.Container container)
        {
            // create A4 page with landscape orientation
            PdfPage page = doc.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Landscape;

            using (XGraphics gfx = XGraphics.FromPdfPage(page))
            {
                // calculate x and y page scale factors
                double scaleX = page.Width.Value / container.Width;
                double scaleY = page.Height.Value / container.Height;
                double scale = Math.Min(scaleX, scaleY);

                // set scaling function
                ScaleToPage = (value) => value * scale;

                // draw block contents to pdf graphics
                Render(gfx, container);
            }
        }

        private void Render(object gfx, Test2d.Container container)
        {
            foreach (var layer in container.Layers)
            {
                if (layer.IsVisible)
                {
                    Draw(gfx, layer);
                }
            }
        }

        public void ClearCache()
        {
        }

        private XColor ToXColor(Test2d.ArgbColor color)
        {
            return XColor.FromArgb(
                color.A,
                color.R,
                color.G,
                color.B);
        }

        private XPen ToXPen(Test2d.ShapeStyle style)
        {
            return new XPen(
                ToXColor(style.Stroke),
                ScaleToPage(style.Thickness))
            {
                LineCap = XLineCap.Flat
            };
        }

        private XSolidBrush ToXSolidBrush(Test2d.ArgbColor color)
        {
            return new XSolidBrush(ToXColor(color));
        }

        private System.Windows.Rect CreateRect(Test2d.XPoint tl, Test2d.XPoint br, double dx, double dy)
        {
            double tlx = Math.Min(tl.X, br.X);
            double tly = Math.Min(tl.Y, br.Y);
            double brx = Math.Max(tl.X, br.X);
            double bry = Math.Max(tl.Y, br.Y);
            return new System.Windows.Rect(
                new System.Windows.Point(tlx + dx, tly + dy),
                new System.Windows.Point(brx + dx, bry + dy));
        }

        public void Draw(object gfx, Test2d.Layer layer)
        {
            foreach (var shape in layer.Shapes)
            {
                shape.Draw(gfx, this, 0, 0);
            }
        }

        public void Draw(object gfx, Test2d.XLine line, double dx, double dy)
        {
            (gfx as XGraphics).DrawLine(
                ToXPen(line.Style),
                ScaleToPage(line.Start.X + dx),
                ScaleToPage(line.Start.Y + dy),
                ScaleToPage(line.End.X + dx),
                ScaleToPage(line.End.Y + dy));
        }

        public void Draw(object gfx, Test2d.XRectangle rectangle, double dx, double dy)
        {
            var rect = CreateRect(
                rectangle.TopLeft,
                rectangle.BottomRight,
                dx, dy);

            if (rectangle.IsFilled)
            {
                (gfx as XGraphics).DrawRectangle(
                    ToXPen(rectangle.Style),
                    ToXSolidBrush(rectangle.Style.Fill),
                    ScaleToPage(rect.X),
                    ScaleToPage(rect.Y),
                    ScaleToPage(rect.Width),
                    ScaleToPage(rect.Height));
            }
            else
            {
                (gfx as XGraphics).DrawRectangle(
                    ToXPen(rectangle.Style),
                    ScaleToPage(rect.X),
                    ScaleToPage(rect.Y),
                    ScaleToPage(rect.Width),
                    ScaleToPage(rect.Height));
            }
        }

        public void Draw(object gfx, Test2d.XEllipse ellipse, double dx, double dy)
        {
            var rect = CreateRect(
                ellipse.TopLeft,
                ellipse.BottomRight,
                dx, dy);

            if (ellipse.IsFilled)
            {
                (gfx as XGraphics).DrawEllipse(
                    ToXPen(ellipse.Style),
                    ToXSolidBrush(ellipse.Style.Fill),
                    ScaleToPage(rect.X),
                    ScaleToPage(rect.Y),
                    ScaleToPage(rect.Width),
                    ScaleToPage(rect.Height));
            }
            else
            {
                (gfx as XGraphics).DrawEllipse(
                    ToXPen(ellipse.Style),
                    ScaleToPage(rect.X),
                    ScaleToPage(rect.Y),
                    ScaleToPage(rect.Width),
                    ScaleToPage(rect.Height));
            }
        }

        public void Draw(object gfx, Test2d.XArc arc, double dx, double dy)
        {
            var a = PdfArc.FromXArc(arc, dx, dy);

            (gfx as XGraphics).DrawArc(
                ToXPen(arc.Style),
                ScaleToPage(a.X),
                ScaleToPage(a.Y),
                ScaleToPage(a.Width),
                ScaleToPage(a.Height),
                a.StartAngle,
                a.SweepAngle);
        }

        public void Draw(object gfx, Test2d.XBezier bezier, double dx, double dy)
        {
            (gfx as XGraphics).DrawBezier(
                ToXPen(bezier.Style),
                ScaleToPage(bezier.Point1.X),
                ScaleToPage(bezier.Point1.Y),
                ScaleToPage(bezier.Point2.X),
                ScaleToPage(bezier.Point2.Y),
                ScaleToPage(bezier.Point3.X),
                ScaleToPage(bezier.Point3.Y),
                ScaleToPage(bezier.Point4.X),
                ScaleToPage(bezier.Point4.Y));
        }

        public void Draw(object gfx, Test2d.XQBezier qbezier, double dx, double dy)
        {
            double x1 = qbezier.Point1.X;
            double y1 = qbezier.Point1.Y;
            double x2 = qbezier.Point1.X + (2.0 * (qbezier.Point2.X - qbezier.Point1.X)) / 3.0;
            double y2 = qbezier.Point1.Y + (2.0 * (qbezier.Point2.Y - qbezier.Point1.Y)) / 3.0;
            double x3 = x2 + (qbezier.Point3.X - qbezier.Point1.X) / 3.0;
            double y3 = y2 + (qbezier.Point3.Y - qbezier.Point1.Y) / 3.0;
            double x4 = qbezier.Point3.X;
            double y4 = qbezier.Point3.Y;

            (gfx as XGraphics).DrawBezier(
                ToXPen(qbezier.Style),
                ScaleToPage(x1 + dx), ScaleToPage(y1 + dy),
                ScaleToPage(x2 + dx), ScaleToPage(y2 + dy),
                ScaleToPage(x3 + dx), ScaleToPage(y3 + dy),
                ScaleToPage(x4 + dx), ScaleToPage(y4 + dy));
        }

        public void Draw(object gfx, Test2d.XText text, double dx, double dy)
        {
            XPdfFontOptions options = new XPdfFontOptions(
                PdfFontEncoding.Unicode,
                PdfFontEmbedding.Always);

            XFont font = new XFont(
                text.Style.FontName,
                ScaleToPage(text.Style.FontSize),
                XFontStyle.Regular,
                options);

            var rect = CreateRect(
                text.TopLeft,
                text.BottomRight,
                dx, dy);

            XRect srect = new XRect(
                ScaleToPage(rect.X),
                ScaleToPage(rect.Y),
                ScaleToPage(rect.Width),
                ScaleToPage(rect.Height));

            XStringFormat format = new XStringFormat();
            switch (text.Style.TextHAlignment)
            {
                case Test2d.TextHAlignment.Left:
                    format.Alignment = XStringAlignment.Near;
                    break;
                case Test2d.TextHAlignment.Center:
                    format.Alignment = XStringAlignment.Center;
                    break;
                case Test2d.TextHAlignment.Right:
                    format.Alignment = XStringAlignment.Far;
                    break;
            }

            switch (text.Style.TextVAlignment)
            {
                case Test2d.TextVAlignment.Top:
                    format.LineAlignment = XLineAlignment.Near;
                    break;
                case Test2d.TextVAlignment.Center:
                    format.LineAlignment = XLineAlignment.Center;
                    break;
                case Test2d.TextVAlignment.Bottom:
                    format.LineAlignment = XLineAlignment.Far;
                    break;
            }

            if (text.IsFilled)
            {
                (gfx as XGraphics).DrawRectangle(ToXSolidBrush(text.Style.Fill), srect);
            }

            (gfx as XGraphics).DrawString(
                text.Text,
                font,
                ToXSolidBrush(text.Style.Stroke),
                srect,
                format);
        }
    }
}
