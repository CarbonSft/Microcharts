// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using SkiaSharp;

namespace Microcharts.Forms
{
	using Xamarin.Forms;
	using SkiaSharp.Views.Forms;

    public class ChartView : SKCanvasView
	{
		public ChartView()
		{
			BackgroundColor = Color.Transparent;
			PaintSurface += OnPaintCanvas;
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            base.OnTouch(e);
            var lineChart = (PointChart) Chart;
            lineChart.SelectNearestPoint(e.Location);
            InvalidateSurface();
        }

        public static readonly BindableProperty ChartProperty = BindableProperty.Create(
	        nameof(Chart), 
	        typeof(Chart),
	        typeof(ChartView), 
	        null, BindingMode.OneWay, null, 
	        Target
            );

	    private static void Target(BindableObject bindable, object oldvalue, object newvalue)
	    {
	        ((ChartView)bindable).InvalidateSurface();
        }

	    public Chart Chart
		{
			get => (Chart)GetValue(ChartProperty);
	        set => SetValue(ChartProperty, value);
	    }

	    private void OnPaintCanvas(object sender, SKPaintSurfaceEventArgs e)
	    {
	        Chart?.Draw(e.Surface.Canvas, e.Info.Width, e.Info.Height);
	    }
	}
}
