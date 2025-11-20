using SkiaSharp;
using SkiaSharp.Views.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace test.Presentation;

public class ChartRenderer : UserControl
{
	public static readonly DependencyProperty DataProperty =
		DependencyProperty.Register(nameof(Data), typeof(ObservableCollection<double>), typeof(ChartRenderer),
			new PropertyMetadata(null, OnPropertyChanged));

	private SKXamlCanvas _canvas;

	public ObservableCollection<double> Data
	{
		get => (ObservableCollection<double>)GetValue(DataProperty);
		set => SetValue(DataProperty, value);
	}

	public ChartRenderer()
	{
		_canvas = new SKXamlCanvas();
		_canvas.PaintSurface += OnPaintSurface;
		Content = _canvas;
		Height = 150;
	}

	private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ChartRenderer renderer)
		{
			renderer._canvas.Invalidate();
		}
	}

	private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Transparent);

		if (Data == null || Data.Count < 2)
			return;

		var info = e.Info;
		var padding = 20f;
		var width = info.Width - (padding * 2);
		var height = info.Height - (padding * 2);

		// Calculate points
		var points = new SKPoint[Data.Count];
		var maxValue = Data.Max();
		var stepX = width / (Data.Count - 1);

		for (int i = 0; i < Data.Count; i++)
		{
			var x = padding + (i * stepX);
			var normalizedValue = (float)(Data[i] / maxValue);
			var y = padding + height - (normalizedValue * height);
			points[i] = new SKPoint(x, y);
		}

		// Draw filled area
		DrawFilledArea(canvas, points, padding, height);

		// Draw line
		DrawLine(canvas, points);
	}

	private void DrawFilledArea(SKCanvas canvas, SKPoint[] points, float padding, float height)
	{
		using var path = new SKPath();
		path.MoveTo(points[0].X, padding + height);
		path.LineTo(points[0].X, points[0].Y);

		for (int i = 1; i < points.Length; i++)
		{
			var prevPoint = points[i - 1];
			var currentPoint = points[i];
			var controlX = (prevPoint.X + currentPoint.X) / 2;

			path.CubicTo(controlX, prevPoint.Y, controlX, currentPoint.Y, currentPoint.X, currentPoint.Y);
		}

		path.LineTo(points[points.Length - 1].X, padding + height);
		path.Close();

		var colors = new[]
		{
			SKColor.Parse("#4ADE80").WithAlpha(100),
			SKColor.Parse("#4ADE80").WithAlpha(0)
		};

		using var shader = SKShader.CreateLinearGradient(
			new SKPoint(0, padding),
			new SKPoint(0, padding + height),
			colors,
			null,
			SKShaderTileMode.Clamp
		);

		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Shader = shader,
			IsAntialias = true
		};

		canvas.DrawPath(path, paint);
	}

	private void DrawLine(SKCanvas canvas, SKPoint[] points)
	{
		using var path = new SKPath();
		path.MoveTo(points[0]);

		for (int i = 1; i < points.Length; i++)
		{
			var prevPoint = points[i - 1];
			var currentPoint = points[i];
			var controlX = (prevPoint.X + currentPoint.X) / 2;

			path.CubicTo(controlX, prevPoint.Y, controlX, currentPoint.Y, currentPoint.X, currentPoint.Y);
		}

		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColor.Parse("#4ADE80"),
			StrokeWidth = 3,
			IsAntialias = true
		};

		canvas.DrawPath(path, paint);
	}
}
