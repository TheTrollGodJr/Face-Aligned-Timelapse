using FaceAlignment;
//using FilePicker;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using FaceONNX;

class Program {

    private static string title = "  ______            _ _         _ _             \n |  ____|          ( | )  /\\   | (_)            \n | |__ __ _  ___ ___V V  /  \\  | |_  __ _ _ __  \n |  __/ _` |/ __/ _ \\   / /\\ \\ | | |/ _` | '_ \\ \n | | | (_| | (_|  __/  / ____ \\| | | (_| | | | |\n |_|  \\__,_|\\___\\___| /_/    \\_\\_|_|\\__, |_| |_|\n                                     __/ |      \n                                    |___/       \n";
    
    
    static void Main() {
		
		Intro();
        Dialogue.RunDialogue();
	}

    static void Intro() {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Clear();
        Console.WriteLine(title);
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

static class Dialogue {

    public static string path { get; private set; } = "";
    public static string outDir { get; private set; } = "";
    public static string outRes { get; private set; } = "";
    public static string? videoName { get; private set; } = null;
    public static string? videoFormatStr { get; private set; } = null;
    public static string? imgFormatStr { get; private set; } = null;

    public static int? fps { get; private set; } = null;
    public static int eyeDelta { get; private set; } = 0;
    public static int outMethod { get; private set; } = 0;
    public static int videoFormat { get; private set; } = 0;
    public static int imgFormat { get; private set; } = 0;

    public static bool verbose { get; private set; } = false;

    private static string[] allowedExtensions = {".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"};

    public static void RunDialogue() {

        InputDialogue();
        OutMethodDialogue();

        if (outMethod == 1) ImgFormatDialogue();
        else VideoDialogue();

        OutputDirDialogue();
        OutputResDialogue();
        EyeDeltaDialogue();
        EnableVerboseDialogue();
        
        ProceedDialogue();
    }

    private static void ProceedDialogue() {
        
        Print("\n/g- Do You Want to Proceed?\n/y 1. /GYes\n/y 2. /GNo\n/y > ", false);
        int proceed = GetIntInRange(1, 2, out int removeLines);
        if (proceed == 2) Environment.Exit(0);
        RemoveLines(removeLines+2, "Yes");
    }

    private static void EnableVerboseDialogue() {
        
        Print("\n/g- /wEnable Verbose Logging?\n/y 1. /GYes\n/y 2. /GNo\n/y > ", false);
        int inp = GetIntInRange(1, 2, out int removeLines);
        if (inp == 1) {
            verbose = true;
            RemoveLines(removeLines+2, "Yes");
        }
        else {
            verbose = false;
            RemoveLines(removeLines+2, "No");
        }
    }

    private static void EyeDeltaDialogue() {
        
        Print("\n/g- /wInput Distance Between Eyes /G(in px):\nUsed to resize each face to the same size\nRecommended is 700\nInput a number\n/y > ", false);
        eyeDelta = GetIntInRange(10, 20000, out int removeLines);
        RemoveLines(removeLines+3, $"{eyeDelta.ToString()} px");
    }

    private static void OutputDirDialogue() {
        
        Print("\n/g- /wInput Output Directory:\n/y > ", false);
        string dir = GetDirOrFile(out int removeLines);
        if (Directory.Exists(dir)) outDir = dir;
        else outDir = Path.GetDirectoryName(Path.GetFullPath(dir));
        RemoveLines(removeLines, outDir);
    }

    private static void OutputResDialogue() {

        int w, h = 0;
        
        if (outMethod == 1) Print("\n/g- /wInput Frame Size:");
        else Print("\n/g- /wInput Video Resolution:");
        Print("/GRecommended is 3000x5000\n/g- /wWidth:\n/y > ", false);

        w = GetIntInRange(100, 20000, out int rl1);
        RemoveLines(rl1+1, "");

        Print("/g- /wHeight:\n/y > ", false);
        h = GetIntInRange(100, 20000, out int rl2);
        outRes = $"{w.ToString()}x{h.ToString()}";
        RemoveLines(rl2+2, outRes);
    }

    private static void ImgFormatDialogue() {
        
        Print("\n/g- /wSelect Image Format:\n/GInput a number\n/y 1. /GPNG\n/y 2. /GJPG\n/y 3. /GTIF\n/y 4. /GBMP\n/y > ", false);
        imgFormat = GetIntInRange(1, 4, out int removeLines);

        switch (imgFormat) {
            
            case 1:
                imgFormatStr = "PNG";
                break;
            case 2:
                imgFormatStr = "JPG";
                break;
            case 3:
                imgFormatStr = "TIFF";
                break;
            case 4:
                imgFormatStr = "BMP";
                break;
            default:
                imgFormatStr = "Not Specified";
                break;
        }

        RemoveLines(removeLines+5, imgFormatStr);
    }

    private static void VideoDialogue() {
        
        Print("\n/g- /wInput Video FPS:\n /y> ", false);
        fps = GetIntInRange(1, 500, out int rl1);
        RemoveLines(rl1+2, ""); 

        Print("\n/g- /wInput Video Name:\n /y> ", false);
        videoName =  GetString(out int rl2);
        RemoveLines(rl2+2, "");

        Print("\n/g- /wSelect File Format\n/G Input a number\n/y 1. /GMP4 (Recommended)\n/y 2. /GAVI\n/y 3. /GMOV\n/y 4. /GMKV\n/y 5. /GWebM\n/y > ", false);
        videoFormat = GetIntInRange(1, 5, out int rl3);

        switch (videoFormat) {
            
            case 1:
                videoFormatStr = "MP4";
                break;
            case 2:
                videoFormatStr = "AVI";
                break;
            case 3:
                videoFormatStr = "MOV";
                break;
            case 4:
                videoFormatStr = "MKV";
                break;
            case 5:
                videoFormatStr = "WebM";
                break;
            default:
                videoFormatStr = "Not Specified";
                break;
        }
        RemoveLines(rl3+7, "");

        Print($"/g- /wVideo Info:\n/b > FPS: {fps.ToString()}\n > Name: {videoName}\n > File Format: {videoFormatStr}");
    }

    private static void InputDialogue() {
        
        Print("/g- /wInput File or Directory:\n /y> ", false);
        path = GetDirOrFile(out int removeLines).Replace("/", "//");
        RemoveLines(removeLines, path);

        if (File.Exists(path)) Print("/b - File Count: /g1");
        else {
            int fileCount = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                .Count(file => allowedExtensions.Contains(Path.GetExtension(file).ToLower()));
            if (fileCount == 0) {
                Print("/rNo Files to Process");
                //throw new InvalidOperationException($"The directory {path} contains no img files.\nThe formats supported are .png, .jpg, .jpeg, .bmp, .tif, .tiff, and .ico");
                Environment.Exit(1);
            }
            Print("/b > File Count: /g" + fileCount.ToString());
        }

    }

    private static void OutMethodDialogue() {
        Print("\n/g- /wSelect Output Method:\n/G Input a number\n/y 1. /GFrames\n/y 2. /GVideo\n /y> ", false);
        outMethod = GetIntInRange(1, 2, out int removeLines);

        string replacement;
        if (outMethod == 1) replacement = "Frames";
        else replacement = "Video";

        RemoveLines(removeLines+3, replacement);
    }

    private static void RemoveLines(int lineCount, string replacement) {
        
        for (int i = 0; i < lineCount; i++) {

            Console.SetCursorPosition(0, Console.CursorTop-1);
            ClearCurrentLine();
        }
        ClearCurrentLine();
        if (replacement != "") Print($"/b > {replacement}");
    }

    private static void ClearCurrentLine() {
        
        int currLine = Console.CursorTop;
        Console.SetCursorPosition(0, currLine);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currLine);
    }

    private static int GetIntInRange(int lower, int upper, out int removeLines) {
        
        removeLines = 1;

        string? inp = Console.ReadLine();

        while (!int.TryParse(inp, out int selection) || selection < lower || selection > upper) {

            Console.SetCursorPosition(0, Console.CursorTop-1);
            Print("\r/rInvalid Selection, Try Again\n /y> ", false);
            removeLines++;
            inp = Console.ReadLine();
        }
        //if (int.TryParse(inp, out selection)) {

        int.TryParse(inp, out int returnSelection);
        return returnSelection;
    }

    private static string GetString(out int removeLines) {

        removeLines = 1;

        string? inp = Console.ReadLine();

        while (string.IsNullOrEmpty(inp)) {

            Console.SetCursorPosition(0, Console.CursorTop-1);
            Print("\r/rInvalid Input, Try Again\n /y> ", false);
            removeLines++;
            inp = Console.ReadLine();
        }
        
        return inp;
    }


    private static string GetDirOrFile(out int removeLines) {
        
        removeLines = 1;

        string? inp = Console.ReadLine();
        
        while (!(Directory.Exists(inp) || File.Exists(inp))) {
            
            Console.SetCursorPosition(0, Console.CursorTop-1);
            Print("\r/rInvalid Path, Try Again\n /y> ", false);
            removeLines++;
            inp = Console.ReadLine();
        }

        return inp;
    }

    private static void Print(string display, bool newLine=true) {
        
        bool changeColor = false;

        foreach (char c in display) {
            
            if (c == '/' && !changeColor) {

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
                    case 'G':
                        Console.ForegroundColor = ConsoleColor.Gray;
                        continue;
                    case '/':
                        Console.Write("/");
                        continue;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                }
            }

            Console.Write(c);
        }
        if (newLine) Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
    }

}