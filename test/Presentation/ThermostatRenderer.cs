using SkiaSharp;
using SkiaSharp.Views.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace test.Presentation;

public class ThermostatRenderer : UserControl
{
	public static readonly DependencyProperty ProgressProperty =
		DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ThermostatRenderer),
			new PropertyMetadata(0.0, OnPropertyChanged));

	public static readonly DependencyProperty CurrentTemperatureProperty =
		DependencyProperty.Register(nameof(CurrentTemperature), typeof(double), typeof(ThermostatRenderer),
			new PropertyMetadata(21.8, OnPropertyChanged));

	public static readonly DependencyProperty StatusTextProperty =
		DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(ThermostatRenderer),
			new PropertyMetadata("Heating to 24°", OnPropertyChanged));

	private SKXamlCanvas _canvas;
	private float _centerX;
	private float _centerY;
	private float _radius;

	public double Progress
	{
		get => (double)GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	public double CurrentTemperature
	{
		get => (double)GetValue(CurrentTemperatureProperty);
		set => SetValue(CurrentTemperatureProperty, value);
	}

	public string StatusText
	{
		get => (string)GetValue(StatusTextProperty);
		set => SetValue(StatusTextProperty, value);
	}

	public ThermostatRenderer()
	{
		_canvas = new SKXamlCanvas();
		_canvas.PaintSurface += OnPaintSurface;
		_canvas.PointerPressed += OnPointerPressed;
		_canvas.PointerMoved += OnPointerMoved;
		_canvas.PointerReleased += OnPointerReleased;
		Content = _canvas;
	}

	private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ThermostatRenderer renderer)
		{
			renderer._canvas.Invalidate();
		}
	}

	private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Transparent);

		var info = e.Info;
		_centerX = info.Width / 2f;
		_centerY = info.Height / 2f;
		_radius = Math.Min(_centerX, _centerY) - 40;

		// Draw concentric circles for depth
		DrawConcentricCircles(canvas, _centerX, _centerY, _radius);

		// Draw background arc
		DrawBackgroundArc(canvas, _centerX, _centerY, _radius);

		// Draw progress arc with gradient
		DrawProgressArc(canvas, _centerX, _centerY, _radius);

		// Draw handle
		DrawHandle(canvas, _centerX, _centerY, _radius);

		// Draw center text
		DrawCenterText(canvas, _centerX, _centerY);
	}

	private void DrawConcentricCircles(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 1,
			Color = SKColor.Parse("#2A2A3E").WithAlpha(40),
			IsAntialias = true
		};

		// Draw 3 concentric circles
		for (int i = 1; i <= 3; i++)
		{
			var circleRadius = radius - (i * 30);
			if (circleRadius > 0)
			{
				canvas.DrawCircle(centerX, centerY, circleRadius, paint);
			}
		}
	}

	private void DrawCenterCircle(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		var innerRadius = radius - 10;
		
		var colors = new[]
		{
			SKColor.Parse("#0A1A24"),
			SKColor.Parse("#1A2A34")
		};

		using var shader = SKShader.CreateRadialGradient(
			new SKPoint(centerX, centerY),
			innerRadius,
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

		canvas.DrawCircle(centerX, centerY, innerRadius, paint);
	}

	private void DrawBackgroundArc(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 20,
			Color = SKColor.Parse("#2A2A3E"),
			IsAntialias = true,
			StrokeCap = SKStrokeCap.Round
		};

		var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
		canvas.DrawArc(rect, 135, 270, false, paint);
	}

	private void DrawProgressArc(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		var sweepAngle = (float)(270 * Progress);

		// Create gradient colors
		var colors = new[]
		{
			SKColor.Parse("#00D9FF"), // Cyan
			SKColor.Parse("#0099FF"), // Blue
			SKColor.Parse("#6B4FBB"), // Purple
			SKColor.Parse("#FF8C42"), // Orange
			SKColor.Parse("#FFD700")  // Yellow
		};

		var positions = new[] { 0f, 0.25f, 0.5f, 0.75f, 1.0f };

		var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);

		using var shader = SKShader.CreateSweepGradient(
			new SKPoint(centerX, centerY),
			colors,
			positions,
			SKShaderTileMode.Clamp,
			135,
			135 + sweepAngle
		);

		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 20,
			Shader = shader,
			IsAntialias = true,
			StrokeCap = SKStrokeCap.Round
		};

		canvas.DrawArc(rect, 135, sweepAngle, false, paint);
	}

	private void DrawHandle(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		var angle = 135 + (270 * Progress);
		var radians = angle * Math.PI / 180;

		var handleX = centerX + (float)(radius * Math.Cos(radians));
		var handleY = centerY + (float)(radius * Math.Sin(radians));

		using var paint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.White,
			IsAntialias = true
		};

		canvas.DrawCircle(handleX, handleY, 12, paint);
	}

	private void DrawCenterText(SKCanvas canvas, float centerX, float centerY)
	{
		// Draw home icon + "INDOOR" label
		using (var paint = new SKPaint
		{
			Color = SKColor.Parse("#A0A0A0"),
			TextSize = 14,
			IsAntialias = true,
			TextAlign = SKTextAlign.Center,
			Typeface = SKTypeface.FromFamilyName("Segoe MDL2 Assets", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
		})
		{
			// Draw home icon (U+E80F)
			canvas.DrawText("\uE80F", centerX - 30, centerY - 30, paint);
		}
		
		using (var paint = new SKPaint
		{
			Color = SKColor.Parse("#A0A0A0"),
			TextSize = 12,
			IsAntialias = true,
			TextAlign = SKTextAlign.Center
		})
		{
			canvas.DrawText("INDOOR", centerX + 10, centerY - 30, paint);
		}

		// Draw temperature
		using (var paint = new SKPaint
		{
			Color = SKColors.White,
			TextSize = 58,
			IsAntialias = true,
			TextAlign = SKTextAlign.Center,
			Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Light, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
		})
		{
			canvas.DrawText($"{CurrentTemperature:F1}°", centerX, centerY + 20, paint);
		}

		// Draw status text
		using (var paint = new SKPaint
		{
			Color = SKColor.Parse("#FF8C42"),
			TextSize = 12,
			IsAntialias = true,
			TextAlign = SKTextAlign.Center
		})
		{
			canvas.DrawText(StatusText, centerX, centerY + 80, paint);
		}
	}

	private bool _isTracking;

	private void OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		_isTracking = true;
		UpdateProgressFromPointer(e);
		((UIElement)sender).CapturePointer(e.Pointer);
	}

	private void OnPointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		if (_isTracking)
		{
			UpdateProgressFromPointer(e);
		}
	}

	private void OnPointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		_isTracking = false;
		((UIElement)sender).ReleasePointerCapture(e.Pointer);
	}

	private void UpdateProgressFromPointer(Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(_canvas);
		var x = (float)point.Position.X;
		var y = (float)point.Position.Y;

		// Calculate angle from center
		var dx = x - _centerX;
		var dy = y - _centerY;
		var angleRad = Math.Atan2(dy, dx);
		var angleDeg = angleRad * 180 / Math.PI;

		// Normalize angle to 0-360
		if (angleDeg < 0)
			angleDeg += 360;

		// Convert to progress (135° to 405° maps to 0 to 1)
		// 135° is the start, 405° (45°) is the end
		if (angleDeg >= 135)
		{
			Progress = (angleDeg - 135) / 270.0;
		}
		else if (angleDeg <= 45)
		{
			Progress = (angleDeg + 225) / 270.0;
		}
		else
		{
			// In the dead zone (45° to 135°), snap to nearest end
			if (angleDeg < 90)
				Progress = 1.0;
			else
				Progress = 0.0;
		}

		// Clamp to 0-1
		Progress = Math.Clamp(Progress, 0.0, 1.0);
	}
}
