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
            Touch += OnTouch;

        }

        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            base.OnTouch(e);
            var loc = e.Location;
            var elements = GetChildElements(loc.ToFormsPoint());
            //Chart.Entries.First().First().
            //elements[0].
            var lineChart = (LineChart) Chart;
            var point = lineChart.Points.First(x =>
                SKPoint.Distance(x, loc) == lineChart.Points.Min(y => SKPoint.Distance(y, loc)));
            lineChart.SelectedPoint = point;
            this.InvalidateSurface();

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
