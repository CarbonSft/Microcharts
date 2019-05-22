﻿// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microcharts
{
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Line.png)
    /// 
    /// Line chart.
    /// </summary>
    public class LineChart : PointChart
    {
        #region Constructors

        public LineChart()
        {
            PointSize = 10;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the size of the line.
        /// </summary>
        /// <value>The size of the line.</value>
        public float LineSize { get; set; } = 3;

        /// <summary>
        /// Gets or sets the line mode.
        /// </summary>
        /// <value>The line mode.</value>
        public LineMode LineMode { get; set; } = LineMode.Spline;

        /// <summary>
        /// Gets or sets the alpha of the line area.
        /// </summary>
        /// <value>The line area alpha.</value>
        public byte LineAreaAlpha { get; set; } = 32;

        /// <summary>
        /// Returns points of chart
        /// </summary>
        public SKPoint[] Points { get; private set; }

        public SKPoint SelectedPoint { get; set; } = SKPoint.Empty;

        #endregion

        #region Methods

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {


            foreach (var serie in Entries)
            {
                var entries = serie as Entry[] ?? serie.ToArray();
                var valueLabelSizes = MeasureValueLabels(entries);
                var footerHeight = CalculateFooterHeight(entries, valueLabelSizes);
                var headerHeight = CalculateHeaderHeight(valueLabelSizes);
                var itemSize = CalculateItemSize(entries, width, height, footerHeight, headerHeight);
                var origin = CalculateYOrigin(itemSize.Height, headerHeight);

                Points = CalculatePoints(itemSize, origin, headerHeight, entries);

                DrawArea(entries, canvas, Points, itemSize, origin);
                DrawLine(entries, canvas, Points, itemSize);
                DrawPoints(entries, canvas, Points, SelectedPoint);
                DrawFooter(entries, canvas, Points, itemSize, height, footerHeight);
                DrawValueLabel(entries, canvas, Points, itemSize, height, valueLabelSizes);
            }

        }

        protected void DrawLine(IEnumerable<Entry> serie, SKCanvas canvas, SKPoint[] points, SKSize itemSize)
        {
            if (points.Length > 1 && LineMode != LineMode.None)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.White,
                    StrokeWidth = LineSize,
                    IsAntialias = true,
                })
                {
                    var entries = serie as Entry[] ?? serie.ToArray();
                    using (var shader = CreateGradient(entries, points))
                    {
                        paint.Shader = shader;

                        var path = new SKPath();

                        path.MoveTo(points.First());

                        var last = (LineMode == LineMode.Spline) ? points.Length - 1 : points.Length;
                        for (int i = 0; i < last; i++)
                        {
                            if (LineMode == LineMode.Spline)
                            {
                                var cubicInfo = CalculateCubicInfo(points, i, itemSize);
                                path.CubicTo(cubicInfo.control, cubicInfo.nextControl, cubicInfo.nextPoint);
                            }
                            else if (LineMode == LineMode.Straight)
                            {
                                path.LineTo(points[i]);
                            }
                        }

                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        protected void DrawArea(IEnumerable<Entry> serie, SKCanvas canvas, SKPoint[] points, SKSize itemSize, float origin)
        {
            if (LineAreaAlpha > 0 && points.Length > 1)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.White,
                    IsAntialias = true,
                })
                {
                    var entries = serie as Entry[] ?? serie.ToArray();
                    using (var shader = CreateGradient(entries, points, LineAreaAlpha))
                    {
                        paint.Shader = shader;

                        var path = new SKPath();

                        path.MoveTo(points.First().X, origin);
                        path.LineTo(points.First());

                        var last = LineMode == LineMode.Spline ? points.Length - 1 : points.Length;
                        for (int i = 0; i < last; i++)
                        {
                            if (LineMode == LineMode.Spline)
                            {
                                var cubicInfo = CalculateCubicInfo(points, i, itemSize);
                                path.CubicTo(cubicInfo.control, cubicInfo.nextControl, cubicInfo.nextPoint);
                            }
                            else if (LineMode == LineMode.Straight)
                            {
                                path.LineTo(points[i]);
                            }
                        }

                        path.LineTo(points.Last().X, origin);

                        path.Close();

                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        private (SKPoint point, SKPoint control, SKPoint nextPoint, SKPoint nextControl) CalculateCubicInfo(SKPoint[] points, int i, SKSize itemSize)
        {
            var point = points[i];
            var nextPoint = points[i + 1];
            var controlOffset = new SKPoint(itemSize.Width * 0.8f, 0);
            var currentControl = point + controlOffset;
            var nextControl = nextPoint - controlOffset;
            return (point, currentControl, nextPoint, nextControl);
        }

        private SKShader CreateGradient(IEnumerable<Entry> serie, SKPoint[] points, byte alpha = 255)
        {
            var startX = points.First().X;
            var endX = points.Last().X;

            return SKShader.CreateLinearGradient(
                new SKPoint(startX, 0),
                new SKPoint(endX, 0),
                serie.Select(x => x.Color.WithAlpha(alpha)).ToArray(),
                null,
                SKShaderTileMode.Clamp);
        }

        #endregion
    }
}