using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using FaceONNX;
using UMapx.Imaging;

namespace FaceAlignment;

public class Face {
	
	private string? filePath = null;
	private string? fileName = null;
	private Point[]? eyes = null;
	private Point? delta = null;
	private Bitmap? faceOutput = null;
    private List<string> skipped = new List<string>();

	public Face() {}

	public Face(string path) {
		
		if (File.Exists(path)) {
			filePath = path;
			fileName = Path.GetFileName(path);
			faceOutput = new Bitmap(filePath);
		}
		else throw new FileNotFoundException($"File path '{path}' is invalid");
	}


	/*
			#############
			Get Functions
			#############
	*/

	/// <summary>
	/// Returns the Point values of both eyes in the given bitmap
	/// </summary>
	/// <returns>[Point leftEye, Point rightEye]; <c>null</c> if the variable is not initilized</returns>
	public Point[]? GetEyeCoords() {
		return eyes;
	}
    
    /// <summary>
    /// Returns the bitmap of the current face
    /// </summary>
    /// <returns>Face bitmap OR <c>null</c> if there is no face</returns>
	public Bitmap? GetFaceOutput() {
		return faceOutput;
	}

    /// <summary>
    /// Returns the filepath to the original face bitmap
    /// </summary>
    /// <returns>String path value; <c>null</c> if there is no value</returns>
	public string? GetFilepath() {
		return filePath;
	}
    
    /// <summary>
    /// Returns the file name of the filepath
    /// </summary>
    /// <returns>String file name; <c>null</c> if there is no value</returns>
	public string? GetFilename() {
		return fileName;
	}

    /// <summary>
    /// Returns a delta value of the difference in eye coordinates
    /// </summary>
    /// <returns>Point value; <c>null</c> if there is no value</returns>
	public Point? GetDelta() {
		return delta;
	}

    /// <summary>
    /// Return a list of all skipped files
    /// File are skipped if more than one face was detected
    /// </summary>
    /// <returns><c>List<string></c> of all paths</returns>
    public List<string> GetSkippedPaths() {
        return skipped;
    }
    
    /*
        ####################
        Set Global Variables
        ####################
    */ 
    
    /// <summary>
    /// Set the global string file path.
    /// Used to load new faces for processing
    /// </summary>
    /// <param name="path">String path value</param>
	public void SetFilepath(string path) {
		filePath = path; // Assign new path
		fileName = Path.GetFileName(path); // Assign new file name
		faceOutput = new Bitmap(filePath); // Assign new image bitmap
	}
	
    /*
        ###############################################
        Face Processing and Global Variable Assignments
        ###############################################
    */
    
    /// <summary>
    /// Uses FaceONNX face detection landmarks to calculate the coordinates of the eyes found in an image bitmap
    /// Saves the coordinate values into the global class variable <c>Point[]? eyes</c>
    /// Requires FaceONNX FaceDetector and Face68LandmarksExtractor models to be preloaded and pass as parameters
    /// </summary>
    /// <param name="fd">FaceDetector FaceONNX model to process the face</param>
    /// <param name="landmarks">Face68LandmarksExtractor FaceONNX model go process face landmarks</param>
    /// <returns>
    /// bool <c>true</c> if successful; bool <c>false</c> if unsuccessful.
    /// <c>false</c> would only be returned because there were too many faces detected.
    /// Use method <c>GetSkippedPaths()</c> to see all skipped files.
    /// </param>
	public bool CalcEyeCoords(FaceDetector fd, Face68LandmarksExtractor landmarks) {
		
		if (faceOutput == null) throw new NullReferenceException("faceOutput is null"); // Throw error if there is no face loaded
        
        // Shrink face bitmap for quicker processing then find all faces
		using var small = ResizeForDetection(faceOutput, 512, out double scale); // Resize bitmap
		var faces = fd.Forward(small); // Get face count

        // Catch if there is an invalid number of faces detected
        if (faces.Length == 0) throw new InvalidOperationException($"No faces detected in {filePath}"); // Return an error if no faces are found
        else if (faces.Length > 1) {
            // Skip and save the filepaths for images  with more than one face so they can be resovled later
            skipped.Add(filePath);
            Console.WriteLine($"File {filePath} skipped because too many faces were detected");
            return false; // Eye calculation unsucessful; too many faces
        }
    
        // Process face
		var box = faces[0].Box; // Get face size
		using var cropped = BitmapTransform.Crop(small, box); // Get face only bitmap
		var points = landmarks.Forward(cropped); // Process face

        // Get eyec coordinates
		var leftEye = GetIris(points.LeftEye, box);
		var rightEye = GetIris(points.RightEye, box);
    
        // Scale coordinates back to the original bitmap size from the shrunk bitmap
		leftEye.X = (int)(leftEye.X / scale);
		leftEye.Y = (int)(leftEye.Y / scale);
		rightEye.X = (int)(rightEye.X / scale);
		rightEye.Y = (int)(rightEye.Y / scale);

		eyes = new[] {leftEye, rightEye}; // Set global eyes variable with eye coordinates
		return true; // Processing successful
	}

    /// <summary>
    /// Rotate the face bitmap to align the eyes horizontally across a horizontal axis
    /// Saves the face delta value to the global variable <c>delta</c>
    /// </summary>
    /// <returns><c>Double</c> of the rotation angle needed to align the eyes horizontally</returns>
	public double AlignEyes() {
		
        // Error handling
		if (eyes == null) throw new NullReferenceException("eyes are null");
		if (faceOutput == null) throw new NullReferenceException("faceOutput is null");
        
        // Get face delta; the difference between the eye coordinates
		delta = new Point(eyes![1].X - eyes![0].X, eyes![1].Y - eyes![0].Y);
        
        // Calculate the angle of rotation needed to align the eyes horizontally
		double angle = (Math.Atan2(delta!.Value.Y, delta!.Value.X) * 180.0 / Math.PI) * -1;
        
        // Get face dimensions
		int w = faceOutput.Width;
		int h = faceOutput.Height;
        
        // Create new temp bitmap for the rotated face
		Bitmap rotated = new Bitmap(w, h);
        
        // Rotate the face
		using (Graphics g = Graphics.FromImage(rotated)) {
			
			g.Clear(Color.Transparent); // Zero out the rotated bitmap var
            
            // Transform the bitmap
			g.TranslateTransform(w / 2f, h / 2f);
			g.RotateTransform((float)angle);
			g.TranslateTransform(-w / 2f, -h / 2f);
            
            // Add the original face bitmap to the rotated bitmap
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(faceOutput, new Point(0, 0));
		}

		faceOutput = rotated; // Update the face bitmap to the rotated face
		return angle; // Return the rotation angle
	}
    
    /// <summary>
    /// Resize the face bitmap
    /// Resizes according to a speificed distance between eye coordintes using the param newDistance
    /// </summary>
    /// <param name="newDistance">Desired distance (px) between the eyes</param>
    /// <returns><c>Double</c> ratio between the original face delta and the <c>newDistance</c> param delta value</returns>
	public double ResizeFace(int newDistance) {
	    
        // Error handling	
		if (faceOutput == null) throw new NullReferenceException("faceOutput is null");
		if (delta == null) throw new NullReferenceException("Face Delta is null");
		
        // Calculate distance ratio between original eye delta and new eye delta
		double dist2 = Math.Sqrt((delta!.Value.X * delta!.Value.X) + (delta!.Value.Y * delta!.Value.Y));
		double ratio = (double)newDistance / dist2; // Calculate ratio
        
        // Get new bitmap size
		int newW = (int)(faceOutput.Width * ratio);
		int newH = (int)(faceOutput.Height * ratio);

        // Create a temp bitmap var
		Bitmap resized = new Bitmap(newW, newH);

        // Resize face bitmap
		using (Graphics g = Graphics.FromImage(resized)) {
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(faceOutput, 0, 0, newW, newH);
		}

		faceOutput = resized; // Update face bitmap to resized face
		return ratio; // Return ratio
	}

    /// <summary>
    /// Updates the <c>eyes</c> variable with coordinates matching the previous resizing and rotations
    /// Use the return values from <c>ResizeFace()</c> and <c>AlignEyes()</c> as parameters so the eye coordinates properly match the current face bitmap
    /// </summary>
    /// <param name="angleDegrees">
    /// Rotation transformation for eye coordinates.
    /// Use return value from <c>AlignEyes()</c> as this value
    /// </param>
    /// <param name="scale">
    /// Scale transformation for the eye coordinates.
    /// Use the return value from <c>ResizeFace()</c> as this calue
    /// </param>
	public void TransformEyes(double angleDegrees, double scale) {
	    
        // Error handling	
		if (faceOutput == null) throw new NullReferenceException("faceOutput is null");
		if (eyes == null) throw new NullReferenceException("eyes are null");

        // Get angle values
		double angle = angleDegrees * Math.PI / 180.0; // convert angle to degrees
		double cosA = Math.Cos(angle); // Get cos value of angle
		double sinA = Math.Sin(angle); // Get sin value of angle

        // Get center coordinate of the face bitmap
		Point center = new Point(faceOutput.Width / 2, faceOutput.Height / 2);

        // Loop through eye coordinates and update them
		for (int i = 0; i < eyes.Length; i++) {
            // Calculate x and y deltas
			double dx = eyes![i].X - center.X;
			double dy = eyes![i].Y - center.Y;

            // Calculate x and y rotation values
			double rx = (dx * cosA - dy * sinA) * scale;
			double ry = (dx * sinA + dy * cosA) * scale;

            // Update eye coordinates rotationally around the center of the image
			eyes![i].X = (int)(center.X + rx);
			eyes![i].Y = (int)(center.Y + ry);
		}
	}

    /// <summary>
    /// Resize the face bitmap to a different size
    /// Used to shrink an image for face detection because face detection is quicker on smaller images
    /// </summary>
    /// <param name="source">Source bitmap to be resized</param>
    /// <param name="max">
    /// Max allowed dimension for resizing; smaller values result in smaller images.
    /// This is used to calculate scale.
    /// </param>
    /// <param name="scale">
    /// (Output) The scale factor applied to the source.
    /// calculates using max, a value of 1.0 means to scaling occured.
    /// </param>
	private Bitmap ResizeForDetection(Bitmap source, int max, out double scale) {
	    
        // Get source dimensions	
		int w = source.Width;
		int h = source.Height;

        // Check if max would upscale the image
		if (w <= max && h <= max) {
            // Return unmodified source and 1.0 scale value if max would result in upscaling
            // The function is meant to shrink, not upscale
			scale = 1.0;
			return new Bitmap(source);
		}

        // Calculate scale and new dimensions
		scale = (double)max / Math.Max(w, h);
		int newW = (int)(w * scale);
		int newH = (int)(h * scale);

        // Create new temp bitmap for resizing
		Bitmap resized = new Bitmap(newW, newH);

        // Resize the source bitmap
		using (Graphics g = Graphics.FromImage(resized)) {
			
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(source, 0, 0, newW, newH);
		}

		return resized; // Return the resized bitmap
	}

    /// <summary>
    /// Draws a line between the two eye coordinates in <c>eyes</c>.
    /// Used to visually check if alignment is working.
    /// For proper use, use immediately after running <c>CalcEyeCoords()</c>.
    /// </summary>
	public void DrawEyeLine() {

        // Error handling
		if (faceOutput == null) throw new NullReferenceException("faceOutput is null");
		if (eyes == null) throw new NullReferenceException("eyes are null");
	
        // Draw line on the face bitmap	
		using (Graphics g = Graphics.FromImage(faceOutput)) {
			using (Pen pen = new Pen(Color.Red, 8)) {
				g.DrawLine(pen, eyes[0], eyes[1]);
			}
		}
	}

    /// <summary>
    /// Set the face bitmap to a specific size.
    /// Used to output images of the same size for video compilation.
    /// </summary>
    /// <param name="w">Desired width</param>
    /// <param name="h">Desired height</param>
    /// <param name="eyePos">
    /// Specify the coordinates of the left eye on the resized image.
    /// Without this variable it will center the image and eyes may not align.
    /// (Default value is <c>null</c>)
    /// </param>
	public void StandardizeBitmapSize(int w, int h, Point? eyePos = null) {
    
        // Error handling
		if (faceOutput == null) throw new NullReferenceException("faceOutput is null");
		if (eyes == null) throw new NullReferenceException("eyes are null");
	
        // Create new temp bitmap	
		Bitmap background = new Bitmap(w, h);

        // Paste the source face bitmap onto the new bitmap
		using (Graphics g = Graphics.FromImage(background)) {
			
			g.Clear(Color.Black); // Zero out all data
			int x, y; // Declare x and y values for the source

            // Paste source in the center if there is no specified coordinate for eyePos
			if (eyePos == null) {
				x = (background.Width - faceOutput.Width) / 2;
				y = (background.Height - faceOutput.Height) / 2;

			}
            // Paste source such that the left eye coordinate matche the coordinate given in eyePos
			else {

				x = eyePos.Value.X - eyes![0].X;
				y = eyePos.Value.Y - eyes![0].Y;
			}

			g.DrawImage(faceOutput, x, y, faceOutput!.Width, faceOutput!.Height); // Paste source
		}

		faceOutput = background; // Update face bitmap
	}

    /// <summary>
    /// Automates the process of transforming a face alignment.
    /// Use function <c>GetFaceOutput()</c> to get the face bitmap
    /// </summary>
    /// <param name="fd">
    /// Preloaded FaceDetector FaceONNX model for image face detection.
    /// Used as a parameter in <c>CalcEyeCoords()</c>.
    /// </param>
    /// <param name="landmarks">
    /// Preloaded Face68LandmarksExtractor FaceONNX model for finding face landmarks.
    /// Used as a parameter in <c>CalcEyeCoords()</c>.
    /// </param>
    /// <param name="newDistance">
    /// Desired distance (px) between the eyes for resizing.
    /// Used as the parameter in <c>ResizeFace()</c>.
    /// </param>
    /// <param name="bgWidth">
    /// Desired width for final image.
    /// Used as the width parameter in <c>StandardizeBitmapSize()</c>.
    /// </param>
    /// <param name="bgHeight">
    /// Desire height for the final image.
    /// Used as the height parameter in <c>StandardizeBitmapSize()</c>.
    /// </param>
    /// <param name="eyePos">
    /// Coordinates for the left eye to be placed on in the final images.
    /// Used to align the eyes for multiple images of the same output size
    /// (Default value is <c>null</c>; this will center the image which may or may not align the yes
    /// </param>
	public void Process(FaceDetector fd, Face68LandmarksExtractor landmarks, int newDistance, int bgWidth, int bgHeight, Point? eyePos = null) {
	    
        // Calculate eyes	
		CalcEyeCoords(fd, landmarks);

        // Perform transformations
        var ratio = ResizeFace(newDistance);
		var angle = AlignEyes();
	    	
		TransformEyes((double)angle, (double)ratio); // Update eye coords
		StandardizeBitmapSize(bgWidth, bgHeight, eyePos); // Set a specific size for the face bitmap
	}

    /// <summary>
    /// Given an array of coordinates around the eye, it calculates the average position
    /// </summary>
    /// <param name="points"><c>Point</c> array of all points around the eye</param>
    /// <param name="offset">
    /// Used to calculate eye coordinates relative to the source bitmap.
    /// The points given are relative only to the box of the face.
    /// Set this to the box value of the face
    /// </param>
    /// <returns><c>Point<c> value of the eye averaged from all surrounding points</returns>
	private Point GetIris(Point[] points, Rectangle offset) {

        // Initilize x,y values
		int x = 0;
		int y = 0;

        // Loop through all points
		foreach (var point in points) {
            // get the sum of all x and y values
			x += point.X;
			y += point.Y;
		}
        // Divide x and y values by the amount of points to get the average
		x /= points.Length;
		y /= points.Length;

        // Create a new point to return and assign its values
		Point p = new Point();
		p.X = x + offset.X;
		p.Y = y + offset.Y;

		return p; // Return point
	}
}
