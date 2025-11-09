using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using FaceONNX;
using UMapx.Imaging;

namespace FaceManipulator;

class Face {
	
	private string? filePath = null;
	private string? fileName = null;
	private Point[]? eyes = null;
	private Point? delta = null;
	//private Bitmap? faceBitmap = null;
	private Bitmap? faceOutput = null;

	public Face() {}

	public Face(string path) {
		
		if (File.Exists(path)) {
			filePath = path;
			fileName = Path.GetFileName(path);
			faceOutput = new Bitmap(filePath);
		}
		else {
			Error($"File path '{path}' is invalid");
		}
	}

	public void SetFilepath(string path) {
		filePath = path;
		fileName = Path.GetFileName(path);
		faceOutput = new Bitmap(filePath);
	}

	public Point[]? GetEyeCoords() {
		if (eyes == null) {
			Error("eyes are NULL; run function CalcEyeCoords before retreiving");
			return null;
		}
		return eyes;
	}
	/*
	public Bitmap GetFaceBitmap() {
		return faceBitmap;
	}
	*/
	public Bitmap GetFaceOutput() {
		return faceOutput;
	}

	/*
	public void CalcEyeCoords() {
		
		if (filePath == null) {
			Error($"Variable 'filePath' is NULL; Cannot process face in function 'GetEyeCoords'");
			return;
		}

		using var faceDetector = new FaceDetector();
		using var faceLandmarkExtractor = new Face68LandmarksExtractor();

		var face = faceDetector.Forward(faceOutput)[0];
		var box = face.Box;
		using var cropped = BitmapTransform.Crop(faceOutput, box);
		var points = faceLandmarkExtractor.Forward(cropped);
		var leftEye = GetIris(points.LeftEye, box);
		var rightEye = GetIris(points.RightEye, box);

		eyes = [leftEye, rightEye];
	}
	*/

	public bool CalcEyeCoords(FaceDetector fd, Face68LandmarksExtractor landmarks) {
		
		if (faceOutput == null) {
			Error("faceOutput is null");
			return false;
		}

		using var small = ResizeForDetection(faceOutput, 512, out double scale);
		var faces = fd.Forward(small);
		if (faces.Length == 0) {
			Error($"No faces detected in {filePath}");
			return false;
		}
		else if (faces.Length > 1) {
			Error($"Too many faces detected in {filePath}");
			return false;
		}

		var box = faces[0].Box;
		using var cropped = BitmapTransform.Crop(small, box);
		var points = landmarks.Forward(cropped);
		var leftEye = GetIris(points.LeftEye, box);
		var rightEye = GetIris(points.RightEye, box);

		leftEye.X = (int)(leftEye.X / scale);
		leftEye.Y = (int)(leftEye.Y / scale);
		rightEye.X = (int)(rightEye.X / scale);
		rightEye.Y = (int)(rightEye.Y / scale);

		eyes = new[] {leftEye, rightEye};
		return true;
	}

	public double? AlignEyes() {
		
		if (eyes == null) {
			Error("eyes are null; cannot continue");
			return null;
		}
		if (faceOutput == null) {
			Error("faceOutput is null; cannot continue");
			return null;
		}

		delta = new Point(eyes![1].X - eyes![0].X, eyes![1].Y - eyes![0].Y);

		double angle = (Math.Atan2(delta!.Value.Y, delta!.Value.X) * 180.0 / Math.PI) * -1;

		int w = faceOutput.Width;
		int h = faceOutput.Height;

		Bitmap rotated = new Bitmap(w, h);

		using (Graphics g = Graphics.FromImage(rotated)) {
			
			g.Clear(Color.Transparent);

			g.TranslateTransform(w / 2f, h / 2f);
			g.RotateTransform((float)angle);
			g.TranslateTransform(-w / 2f, -h / 2f);

			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(faceOutput, new Point(0, 0));
		}

		faceOutput = rotated;
		return angle;

	}

	public double? ResizeFace(int newDistance) {
		
		if (faceOutput == null) {
			Error("faceBitmap variable is null");
			return null;
		}
		if (delta == null) {
			Error("Face Delta point variable is null");
			return null;
		}
		
		double dist2 = Math.Sqrt((delta!.Value.X * delta!.Value.X) + (delta!.Value.Y * delta!.Value.Y));
		double ratio = (double)newDistance / dist2;

		int newW = (int)(faceOutput.Width * ratio);
		int newH = (int)(faceOutput.Height * ratio);

		Bitmap resized = new Bitmap(newW, newH);

		using (Graphics g = Graphics.FromImage(resized)) {
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(faceOutput, 0, 0, newW, newH);
		}

		faceOutput = resized;
		return ratio;
	}

	public void TransformEyes(double angleDegrees, double scale) {
		
		if (eyes == null || faceOutput == null) {
			Error("Cannot transform eyes; null data");
			return;
		}

		double angle = angleDegrees * Math.PI / 180.0;
		double cosA = Math.Cos(angle);
		double sinA = Math.Sin(angle);

		Point center = new Point(faceOutput.Width / 2, faceOutput.Height / 2);

		for (int i = 0; i < eyes.Length; i++) {
			double dx = eyes![i].X - center.X;
			double dy = eyes![i].Y - center.Y;

			double rx = (dx * cosA - dy * sinA) * scale;
			double ry = (dx * sinA + dy * cosA) * scale;

			eyes![i].X = (int)(center.X + rx);
			eyes![i].Y = (int)(center.Y + ry);
		}
	}

	private Bitmap ResizeForDetection(Bitmap source, int max, out double scale) {
		
		int w = source.Width;
		int h = source.Height;

		if (w <= max && h <= max) {
			scale = 1.0;
			return new Bitmap(source);
		}

		scale = (double)max / Math.Max(w, h);
		int newW = (int)(w * scale);
		int newH = (int)(h * scale);

		Bitmap resized = new Bitmap(newW, newH);
		using (Graphics g = Graphics.FromImage(resized)) {
			
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(source, 0, 0, newW, newH);
		}
		return resized;
	}

	public void DrawEyeLine() {
		if (eyes == null) {
			Error("Face eyes variable is null");
			return;
		}
		
		using (Graphics g = Graphics.FromImage(faceOutput)) {
			using (Pen pen = new Pen(Color.Red, 8)) {
				g.DrawLine(pen, eyes[0], eyes[1]);
			}
		}
	}

	public void StandardizeBitmapSize(int w, int h, Point? eyePos = null) {

		if (faceOutput == null) {
			Error("faceOutput is null; please run alignEyes() and resizeFace() before standardizing bitmap size");
			return;
		}
		
		Bitmap background = new Bitmap(w, h);

		using (Graphics g = Graphics.FromImage(background)) {
			
			g.Clear(Color.Black);
			int x, y;

			if (eyePos == null) {
				x = (background.Width - faceOutput.Width) / 2;
				y = (background.Height - faceOutput.Height) / 2;

			}
			else {

				x = eyePos.Value.X - eyes![0].X;
				y = eyePos.Value.Y - eyes![0].Y;
			}

			g.DrawImage(faceOutput, x, y, faceOutput!.Width, faceOutput!.Height);
		}
		faceOutput = background;
	}

	public void Process(FaceDetector fd, Face68LandmarksExtractor landmarks, int newDistance, int bgWidth, int bgHeight, Point? eyePos = null) {
		
		CalcEyeCoords(fd, landmarks);
		var angle = AlignEyes();
		var ratio = ResizeFace(newDistance);

		if (ratio == null || angle == null) {
			Error("Could not process face");
			return;
		}
		
		TransformEyes((double)angle, (double)ratio);
		StandardizeBitmapSize(bgWidth, bgHeight, eyePos);
	}

	private void Error(string msg) {
		
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine("ERROR: " + msg);
		Console.ForegroundColor = ConsoleColor.White;
	}

	private Point GetIris(Point[] points, Rectangle offset) {

		int x = 0;
		int y = 0;

		foreach (var point in points) {
			x += point.X;
			y += point.Y;
		}
		x /= points.Length;
		y /= points.Length;

		Point p = new Point();
		p.X = x + offset.X;
		p.Y = y + offset.Y;
		return p;
	}
}
