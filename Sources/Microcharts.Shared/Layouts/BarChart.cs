// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Linq;

    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Bar.png)
    /// 
    /// A bar chart.
    /// </summary>
    public class BarChart : PointChart
    {
        #region Constructors

        public BarChart()
        {
            PointSize = 0;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the bar background area alpha.
        /// </summary>
        /// <value>The bar area alpha.</value>
        public byte BarAreaAlpha { get; set; } = 32;

        #endregion

        #region Methods

        /// <summary>
        /// Draws the content of the chart onto the specified canvas.
        /// </summary>
        /// <param name="canvas">The output canvas.</param>
        /// <param name="width">The width of the chart.</param>
        /// <param name="height">The height of the chart.</param>
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
                var points = CalculatePoints(itemSize, origin, headerHeight, entries);

                DrawBarAreas(canvas, points, itemSize, headerHeight);
                DrawBars(canvas, points, itemSize, origin, headerHeight);
                DrawPoints(entries, canvas, points, SKPoint.Empty);
                DrawFooter(entries, canvas, points, itemSize, height, footerHeight);
                DrawValueLabel(entries, canvas, points, itemSize, height, valueLabelSizes);
            }
        }

        /// <summary>
        /// Draws the value bars.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="points">The points.</param>
        /// <param name="itemSize">The item size.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="headerHeight">The Header height.</param>
        protected void DrawBars(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float origin, float headerHeight)
        {
            const float minBarHeight = 4;
            foreach (var serie in Entries)
            {                
                var entries = serie as Entry[] ?? serie.ToArray();
                if (points.Length > 0)
                {
                    for (int i = 0; i < entries.Length; i++)
                    {
                        var entry = entries.ElementAt(i);
                        var point = points[i];

                        using (var paint = new SKPaint
                        {
                            Style = SKPaintStyle.Fill,
                            Color = entry.Color,
                        })
                        {
                            var x = point.X - (itemSize.Width / 2);
                            var y = Math.Min(origin, point.Y);
                            var height = Math.Max(minBarHeight, Math.Abs(origin - point.Y));
                            if (height < minBarHeight)
                            {
                                height = minBarHeight;
                                if (y + height > Margin + itemSize.Height)
                                {
                                    y = headerHeight + itemSize.Height - height;
                                }
                            }

                            var rect = SKRect.Create(x, y, itemSize.Width, height);
                            canvas.DrawRect(rect, paint);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the bar background areas.
        /// </summary>
        /// <param name="canvas">The output canvas.</param>
        /// <param name="points">The entry points.</param>
        /// <param name="itemSize">The item size.</param>
        /// <param name="headerHeight">The header height.</param>
        protected void DrawBarAreas(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float headerHeight)
        {
            foreach (var serie in Entries)
            {
                var entries = serie as Entry[] ?? serie.ToArray();
                if (points.Length > 0 && PointAreaAlpha > 0)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        var entry = entries.ElementAt(i);
                        var point = points[i];

                        using (var paint = new SKPaint
                        {
                            Style = SKPaintStyle.Fill,
                            Color = entry.Color.WithAlpha(BarAreaAlpha),
                        })
                        {
                            var max = entry.Value > 0 ? headerHeight : headerHeight + itemSize.Height;
                            var height = Math.Abs(max - point.Y);
                            var y = Math.Min(max, point.Y);
                            canvas.DrawRect(SKRect.Create(point.X - (itemSize.Width / 2), y, itemSize.Width, height),
                                paint);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
