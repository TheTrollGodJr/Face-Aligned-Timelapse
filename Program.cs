using FaceONNX;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using FaceManipulator;

class Program {

	static void Main() {
		var fd = new FaceDetector();
		var landmarks = new Face68LandmarksExtractor();
		var face = new Face("images/img-2.jpg");

		Console.WriteLine("Press any key to start processing");
		Console.ReadKey();

		face.Process(fd, landmarks, 700, 3000, 5000, new Point(1500, 2500));

		var map = face.GetFaceOutput();
		map.Save("img.png", ImageFormat.Png);
	}
}
