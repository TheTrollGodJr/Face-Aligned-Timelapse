using FaceAlignment;
using FilePicker;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using FaceONNX;

class Program {

    private static string title = "  ______            _ _         _ _             \n |  ____|          ( | )  /\\   | (_)            \n | |__ __ _  ___ ___V V  /  \\  | |_  __ _ _ __  \n |  __/ _` |/ __/ _ \\   / /\\ \\ | | |/ _` | '_ \\ \n | | | (_| | (_|  __/  / ____ \\| | | (_| | | | |\n |_|  \\__,_|\\___\\___| /_/    \\_\\_|_|\\__, |_| |_|\n                                     __/ |      \n                                    |___/       ";
    private List<string> buffer = new List<string>();

    [STAThread]
    static async Task Main() {

	Console.WriteLine($"Framework loaded: {typeof(Dialog).Assembly.FullName}");
	Console.WriteLine($"WINDOWS defined: " +
#if WINDOWS
	"YES"
#else
	"NO" 
#endif
);

        var path = Dialog.PickFileAsync("Select a file");
	if (path != null) Console.WriteLine("Selected file: " + path);
	else Console.WriteLine("Path is null");
    }

    static async void Start() {

        Console.Clear();
        Console.WriteLine(title);

        string? path = await Dialog.PickFileAsync("Select a file");
        Console.WriteLine("Selected file: " + path);
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
