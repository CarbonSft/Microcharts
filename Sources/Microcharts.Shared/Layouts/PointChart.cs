// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Point.png)
    /// 
    /// Point chart.
    /// </summary>
    public class PointChart : Chart
    {
        #region Properties

        public float PointSize { get; set; } = 14;

        public SKPoint SelectedPoint { get; set; } = SKPoint.Empty;

        public PointMode PointMode { get; set; } = PointMode.Circle;

        public byte PointAreaAlpha { get; set; } = 100;

        /// <summary>
        /// Returns points of chart
        /// </summary>
        public IEnumerable<SKPoint> Points { get; protected set; }

        private float ValueRange => MaxValue - MinValue;

        #endregion

        #region Methods

        public float CalculateYOrigin(float itemHeight, float headerHeight)
        {
            if (MaxValue <= 0)
            {
                return headerHeight;
            } 

            if (MinValue > 0)
            {
                return headerHeight + itemHeight;
            }

            return headerHeight + ((MaxValue / ValueRange) * itemHeight);
        }

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            Points = new List<SKPoint>();
            foreach (var serie in Series)
            {
                var entries = serie as Entry[] ?? serie.ToArray();
                var valueLabelSizes = MeasureValueLabels(entries);
                var footerHeight = CalculateFooterHeight(entries, valueLabelSizes);
                var headerHeight = CalculateHeaderHeight(valueLabelSizes);
                var itemSize = CalculateItemSize(entries, width, height, footerHeight, headerHeight);
                var origin = CalculateYOrigin(itemSize.Height, headerHeight);

                var points = CalculatePoints(itemSize, origin, headerHeight, entries);

                DrawPoints(entries, canvas, points, SKPoint.Empty);
                DrawFooter(entries, canvas, points, itemSize, height, footerHeight);
                DrawValueLabel(entries, canvas, points, itemSize, height, valueLabelSizes);

                Points = Points.Concat(points.ToArray());
            }

        }

        public void SelectNearestPoint(SKPoint tapLocation)
        {
            var point = Points.First(x =>
                SKPoint.Distance(x, tapLocation) == Points.Min(y => SKPoint.Distance(y, tapLocation)));
            SelectedPoint = point;
        }

        protected SKSize CalculateItemSize(IEnumerable<Entry> serie, int width, int height, float footerHeight, float headerHeight)
        {
            var total = serie.Count();
            var w = (width - ((total + 1) * Margin)) / total;
            var h = height - Margin - footerHeight - headerHeight;
            return new SKSize(w, h);
        }

        protected SKPoint[] CalculatePoints(SKSize itemSize, float origin, float headerHeight, IEnumerable<Entry> serie)
        {
            var result = new List<SKPoint>();

            var entries = serie as Entry[] ?? serie.ToArray();
            for (int i = 0; i < entries.Count(); i++)
            {
                var entry = entries.ElementAt(i);

                var x = Margin + (itemSize.Width / 2) + (i * (itemSize.Width + Margin));
                var y = headerHeight + (((MaxValue - entry.Value) / ValueRange) * itemSize.Height);
                var point = new SKPoint(x, y);
                result.Add(point);
            }

            return result.ToArray();
        }

        protected void DrawFooter(IEnumerable<Entry> serie, SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight)
        {
            DrawLabels(serie, canvas, points, itemSize, height, footerHeight);
        }

        protected void DrawLabels(IEnumerable<Entry> serie, SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight)
        {
            var entries = serie as Entry[] ?? serie.ToArray();
            for (int i = 0; i < entries.Count(); i++)
            {
                var entry = entries.ElementAt(i);
                var point = points[i];

                if (!string.IsNullOrEmpty(entry.Label))
                {
                    using (var paint = new SKPaint())
                    {
                        paint.TextSize = LabelTextSize;
                        paint.IsAntialias = true;
                        paint.Color = entry.TextColor;
                        paint.IsStroke = false;

                        var bounds = new SKRect();
                        var text = entry.Label;
                        paint.MeasureText(text, ref bounds);

                        if (bounds.Width > itemSize.Width)
                        {
                            text = text.Substring(0, Math.Min(3, text.Length));
                            paint.MeasureText(text, ref bounds);
                        }

                        if (bounds.Width > itemSize.Width)
                        {
                            text = text.Substring(0, Math.Min(1, text.Length));
                            paint.MeasureText(text, ref bounds);
                        }

                        canvas.DrawText(text, point.X - (bounds.Width / 2), height - (Margin + (LabelTextSize / 2)), paint);
                    }
                }
            }
        }

        protected void DrawPoints(IEnumerable<Entry> serie, SKCanvas canvas, SKPoint[] points, SKPoint selectedPoint)
        {
            if (points.Length > 0 && PointMode != PointMode.None)
            {
                var entries = serie as Entry[] ?? serie.ToArray();
                for (int i = 0; i < points.Length; i++)
                {                    
                    var entry = entries.ElementAt(i);
                    var point = points[i];
                    var size = PointSize;
                    if (SKPoint.Distance(point, selectedPoint) < 0.1)
                    {
                        size = PointSize + 20;
                    }
                    canvas.DrawPoint(point, entry.Color, size, PointMode);
                }
            }
        }

        protected void DrawValueLabel(IEnumerable<Entry> serie, SKCanvas canvas, SKPoint[] points, SKSize itemSize, float height, SKRect[] valueLabelSizes)
        {
            if (points.Length > 0)
            {
                var entries = serie as Entry[] ?? serie.ToArray();
                for (int i = 0; i < points.Length; i++)
                {                    
                    var entry = entries.ElementAt(i);
                    var point = points[i];

                    if (!string.IsNullOrEmpty(entry.ValueLabel))
                    {
                        using (new SKAutoCanvasRestore(canvas))
                        {
                            using (var paint = new SKPaint())
                            {
                                paint.TextSize = LabelTextSize;
                                paint.FakeBoldText = true;
                                paint.IsAntialias = true;
                                paint.Color = entry.Color;
                                paint.IsStroke = false;

                                var bounds = new SKRect();
                                var text = entry.ValueLabel;
                                paint.MeasureText(text, ref bounds);

                                canvas.RotateDegrees(90);
                                canvas.Translate(Margin, -point.X + (bounds.Height / 2));

                                canvas.DrawText(text, 0, 0, paint);
                            }
                        }
                    }
                }
            }
        }

        protected float CalculateFooterHeight(IEnumerable<Entry> serie, SKRect[] valueLabelSizes)
        {
            var result = Margin;

            if (serie.Any(e => !string.IsNullOrEmpty(e.Label)))
            {
                result += LabelTextSize + Margin;
            }

            return result;
        }

        protected float CalculateHeaderHeight(SKRect[] valueLabelSizes)
        {
            var result = Margin;

            if (Series.Any())
            {
                var maxValueWidth = valueLabelSizes.Max(x => x.Width);
                if (maxValueWidth > 0)
                {
                    result += maxValueWidth + Margin;
                }
            }

            return result;
        }

        protected SKRect[] MeasureValueLabels(IEnumerable<Entry> serie)
        {
            using (var paint = new SKPaint())
            {
                paint.TextSize = LabelTextSize;
                return serie.Select(e =>
                {
                    if (string.IsNullOrEmpty(e.ValueLabel))
                    {
                        return SKRect.Empty;
                    }

                    var bounds = new SKRect();
                    var text = e.ValueLabel;
                    paint.MeasureText(text, ref bounds);
                    return bounds;
                }).ToArray();
            }
        }

        #endregion
    }
}
