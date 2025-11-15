using FaceAlignment;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using FaceONNX;

class Program {

    private List<string> buffer = new List<string>();

    static void Main() {
        
        Print("this /ris /ga /btest/w!"); 
        //Dialogue();
    }

    private static void Diaglogue() {
        
        Console.WriteLine();
    }

    private static int Selection(int inputType, string display, List<string> options) {
        return 0;
    }

    private static void Print(string display) {
        
        bool changeColor = false;

        foreach (char c in display) {
            
            if (c == '/') {

                changeColor = true;
                continue;
            }
            else if (changeColor) {
                
                changeColor = false;
                switch (c) {
                    
                    case 'r':
                        Console.ForegroundColor = ConsoleColor.Red;
                        continue;
                    case 'g':
                        Console.ForegroundColor = ConsoleColor.Green;
                        continue;
                    case 'b':
                        Console.ForegroundColor = ConsoleColor.Blue;
                        continue;
                    case 'y':
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        continue;
                    case 'w':
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                }
            }

            Console.Write(c);
        }
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
    }

	static void old() {
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
