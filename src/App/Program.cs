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

/// <summary>
/// As part of the demo this class prompts the user with options to easily use the library
/// This class only holds the dialogue and variables with the choice made
/// </summary>
static class Dialogue {

    /// Global var
    private static string[] allowedExtensions = {".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"};

    /*
            ###########################
            Intitilize Class Properties
            ###########################
    */

    #region Properties

    /// Strings
    
    public static string path { get; private set; } = ""; // Input path to imgs
    public static string outDir { get; private set; } = ""; // Output path for processed imgs/timelapse video
    public static string outRes { get; private set; } = ""; // resolution of timelapse video/process imgs
    public static string? videoName { get; private set; } = null; // Name of the timelapse video (If applicable)
    public static string? videoFormatStr { get; private set; } = null; // Format type of timelapse video as a string (if applicable)
    public static string? imgFormatStr { get; private set; } = null; // Image format type as a string (if applicable)

    /// Ints

    public static int? fps { get; private set; } = null; // Timelapse fps (if appliacable)
    public static int? videoFormat { get; private set; } = null; // Format type of the timelapse video
    public static int? imgFormat { get; private set; } = null; // Image format type
    public static int eyeDelta { get; private set; } = 0; // Desired distance between eyes for processing
    public static int outMethod { get; private set; } = 0; // Desired output; 1 for exporting as processed frame; 2 for exporting as a timelapse video
    
    /// Bools

    public static bool verbose { get; private set; } = false; // Enable verbose console logging; false be default

    #endregion

    /*
            ################
            Dialogue Options
            ################
    */

    #region Dialogue Options

    /// <summary>
    /// Main function to get user settings
    /// Prompts the user with all dialogue options in order
    /// </summary>
    public static void RunDialogue() {

        InputDialogue(); // Get input dir
        OutMethodDialogue(); // Get Output method; export as frames or video

        // Depending on output method, prompt with relavent information
        if (outMethod == 1) ImgFormatDialogue(); // Get img export data
        else VideoDialogue(); // Get video export data

        OutputDirDialogue(); // Get output dir for frames/video
        OutputResDialogue(); // Get resolution for frames/video
        EyeDeltaDialogue(); // Get desired eye delta (distance between eyes)
        EnableVerboseDialogue(); // define bool for verbose logging
        
        ProceedDialogue(); // Ask if the user wants to proceed
    }

    /// <summary>
    /// Prompts the user to input a file or directory
    /// Only valid paths will be allowed
    /// </summary>
    private static void InputDialogue() {
        
        // Print Dialogue and get user data
        Print("/g- /wInput File or Directory:\n /y> ", false); // Print colored text -- no newline
        path = GetDirOrFile(out int removeLines).Replace("/", "//"); // Get a valid directory or file and save to global 'path' property
        RemoveLines(removeLines, path); // Format console output to cleanly show user data

        // Display the number of files in the given dir (or 1 if a file is given)
        if (File.Exists(path)) {

            Print("/b - File Count: /g1"); // Single file given; count is 1
        }
        else { // If given a dir
            
            int fileCount = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly) // Get top level files in give dir
                .Count(file => allowedExtensions.Contains(Path.GetExtension(file).ToLower())); // Get number of files with allowed file extensions (from the global array)
            
            // Error handling for no valid file in dir
            if (fileCount == 0) {

                Print("/rNo Files to Process"); // Print error message
                Environment.Exit(1); // Exit program
            }

            Print("/b > File Count: /g" + fileCount.ToString()); // Print number of files
        }
    }

    /// <summary>
    /// Prompt user for desired output method
    /// Choose between exorting as frames (1) or as video (2)
    /// </summary>
    private static void OutMethodDialogue() {

        // Print dialogue and get user input
        Print("\n/g- /wSelect Output Method:\n/G Input a number\n/y 1. /GFrames\n/y 2. /GVideo\n /y> ", false); // Print colord text -- no newline
        outMethod = GetIntInRange(1, 2, out int removeLines); // Get an int between 1 and 2; get the remove lines value

        // Convert int choice to string output
        string replacement; // temp var
        if (outMethod == 1) replacement = "Frames";
        else replacement = "Video";

        RemoveLines(removeLines+3, replacement); // Clear lines and display formatted result
    }

    /// <summary>
    /// Get the desired img format for processed images
    /// Only runs when the user chooses the frame output method (outMethod = 1)
    /// </summary>
    private static void ImgFormatDialogue() {
        
        // Print dialogue and get user input
        Print("\n/g- /wSelect Image Format:\n/GInput a number\n/y 1. /GPNG\n/y 2. /GJPG\n/y 3. /GTIF\n/y 4. /GBMP\n/y > ", false); // Print colors text -- no newline
        imgFormat = GetIntInRange(1, 4, out int removeLines); // Get int between 1 and 4; get remove lines value to clear input later

        // Defne the image format string value based off the img format int choosen above
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

        // Clear lines and display formatted result
        RemoveLines(removeLines+5, imgFormatStr);
    }

    /// <summary>
    /// Prompt the user for video output information
    /// This will define the class properties: fps, videoName, videoFormat, and videoFormatStr
    /// </summary>
    private static void VideoDialogue() {
        
        // Print fps prompt and get user input
        Print("\n/g- /wInput Video FPS:\n /y> ", false); // Print colored text -- no newline
        fps = GetIntInRange(1, 500, out int rl1); // Get int between 1 and 500; get remove line int (rl1)
        RemoveLines(rl1+2, "");  // Clear lines of prompt entirely

        // Print video name prompt and get user input
        Print("\n/g- /wInput Video Name:\n /y> ", false); // Print colored text -- no newline
        videoName =  GetString(out int rl2); // Get string value; get remove line int (rl2)
        RemoveLines(rl2+2, ""); // Clear lines of promp entirely

        // Print video format prompt with listed options and get user input
        Print("\n/g- /wSelect File Format\n/G Input a number\n/y 1. /GMP4 (Recommended)\n/y 2. /GAVI\n/y 3. /GMOV\n/y 4. /GMKV\n/y 5. /GWebM\n/y > ", false); // Print colored text -- no newline
        videoFormat = GetIntInRange(1, 5, out int rl3); // get option int between 1 and 5; get remove line int (rl3)

        // Define video format string value based off of the video format value given by the user
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
        // Clear previous prompt entirely
        RemoveLines(rl3+7, "");
        
        // Print out all data formatted nicely; fps, video name, and video format
        Print($"/g- /wVideo Info:\n/b > FPS: {fps.ToString()}\n > Name: {videoName}\n > File Format: {videoFormatStr}");
    }


    /// <summary>
    /// Prompt the user to input the output directory for the exported frames or timelapse video
    /// </summary>
    private static void OutputDirDialogue() {
        
        Print("\n/g- /wInput Output Directory:\n/y > ", false); // Print prompt -- no newline
        string dir = GetDirOrFile(out int removeLines); // Get a valid file or directory path

        // Verify the dir is a dir and not a file
        // The function to define dir accepts either a dir or file path
        if (Directory.Exists(dir)) outDir = dir; // If dir, save normally
        else outDir = Path.GetDirectoryName(Path.GetFullPath(dir)); // If file, get dir path and save

        // Reformat prompt and display dir
        RemoveLines(removeLines, outDir);
    }

    /// <summary>
    /// Prompt the user to input video/frame resolution
    /// For the timelapse video this will determine the video resolution
    /// For the exported frames, it will determine the size of every processed image
    /// </summary>
    private static void OutputResDialogue() {

        // Initilize temp height and width values
        int w, h = 0;
        
        // Print prompt
        if (outMethod == 1) Print("\n/g- /wInput Frame Size:"); // If exporting frames, rephrase accordingly
        else Print("\n/g- /wInput Video Resolution:"); // If generating timelapse, rephrase accordingly
        Print("/GRecommended is 3000x5000\n/g- /wWidth:\n/y > ", false); // Print extra information

        // Get user input and reformat
        w = GetIntInRange(100, 20000, out int rl1); // Get int between 100 and 20,000 for width
        RemoveLines(rl1+1, "");

        // Print second prompt for height and get user input
        Print("/g- /wHeight:\n/y > ", false);
        h = GetIntInRange(100, 20000, out int rl2); // Get int between 100 and 20,000 for height
        outRes = $"{w.ToString()}x{h.ToString()}"; // Convert w and height to a string format

        RemoveLines(rl2+2, outRes); // Reformat and display string resolution
    }

    /// <summary>
    /// Prompt the user to input a desired eye delta value (in px)
    /// This is the distance between the eyes in the image
    /// The face will be resized such that the distance between the eyes in every image matches this value
    /// </summary>
    private static void EyeDeltaDialogue() {
        
        // Print promt and get user input
        Print("\n/g- /wInput Distance Between Eyes /G(in px):\nUsed to resize each face to the same size\nRecommended is 700\nInput a number\n/y > ", false); // Print promp -- no newline
        eyeDelta = GetIntInRange(10, 20000, out int removeLines); // Get int between 10 and 20,000
        RemoveLines(removeLines+3, $"{eyeDelta.ToString()} px"); // Reformat and display
    }

    /// <summary>
    /// Prompt the user to enable or disable verbose console logging
    /// </summary>
    private static void EnableVerboseDialogue() {
        
        // Print prompt and get user input
        Print("\n/g- /wEnable Verbose Logging?\n/y 1. /GYes\n/y 2. /GNo\n/y > ", false); 
        int inp = GetIntInRange(1, 2, out int removeLines); // Get int between 1 (yes) and 2 (no)

        // handle selection
        if (inp == 1) {
            verbose = true;
            RemoveLines(removeLines+2, "Yes");
        }
        else {
            verbose = false;
            RemoveLines(removeLines+2, "No");
        }
    }

    /// <summary>
    /// Ask the user if they want to continue with image processing
    /// This is prompted after the user has finished inputting information
    /// </summary>
    private static void ProceedDialogue() {
        
        // Print prompt and get user input
        Print("\n/g- Do You Want to Proceed?\n/y 1. /GYes\n/y 2. /GNo\n/y > ", false);
        int proceed = GetIntInRange(1, 2, out int removeLines); // Get int between 1 (yes) and 2 (no)
        
        if (proceed == 2) Environment.Exit(0); // Exit program if no
        RemoveLines(removeLines+2, "Yes"); // Reformat and display if yes
    }

    #endregion

    /*
            ##################
            Console Formatting
            ##################
    */

    #region Console Formatting

    /// <summary>
    /// Clears a given number of lines in the console
    /// It replaces the last cleared line with a replacement string that is given
    /// If the replacement string it empty then it will only clear the lines
    /// </summary>
    /// <param name="lineCount"><c>int</c> value specifying how manny lines to clear</param>
    /// <param name="replacement"><c>string</c> value to be displayed after clearing lines</param>
    /// <remarks>
    /// The console prints with the cursor from the top-left down so clearing lines will move the cursor up vertically
    /// </remarks>
    private static void RemoveLines(int lineCount, string replacement) {
        
        // Clear lines
        for (int i = 0; i < lineCount; i++) { 

            Console.SetCursorPosition(0, Console.CursorTop-1); // Set the cursor up one line at the start
            ClearCurrentLine(); // Clear the line
        }

        ClearCurrentLine(); // Ensure current line is cleared

        // Print replacement string with specific formatting
        // If the replacement string is empty, leave the line blank
        if (replacement != "") Print($"/b > {replacement}");
    }

    /// <summary>
    /// Clears the line in the console where the cursor is currently located
    /// </summary>
    private static void ClearCurrentLine() {
        
        int currLine = Console.CursorTop; // Get current cursor row
        Console.SetCursorPosition(0, currLine); // Set cursor to the start (left) of the line
        Console.Write(new string(' ', Console.WindowWidth)); // Write a blank string the width of the console
        Console.SetCursorPosition(0, currLine); // Reset cursor back to the beginning of the line
    }

    /// <summary>
    /// Print a string value with support for in-string color changing
    /// Color is changed when <c>/</c> is used in the string
    /// The following character will determine the change in color
    /// If the character is not defined it will default to white
    /// </summary>
    /// <param name="display"><c>string</c> value to be printed in color</param>
    /// <param name="newLine"><c>bool</c> value determining if a newline is printed. The default value is <c>true</c></param>
    private static void Print(string display, bool newLine=true) {
        
        bool changeColor = false; // Initilize bool to change color

        // Loop through each character in the display string
        foreach (char c in display) {
            
            // If the character is '/' set the changeColor bool to true
            // This will read the next character in the string as a new color
            if (c == '/' && !changeColor) {

                changeColor = true;
                continue; // Skip to next item in the display string
            }
            // Change the text color given there was a '/' 
            else if (changeColor) {
                
                changeColor = false; // Reset colorChange var

                // Choose color
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
                    case '/': // Print a '/' given '//'; same idea as using '\\' to print '\'
                        Console.Write("/");
                        continue;
                    default: // Set any other undefined character as white
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                }
            }

            // If not '/' and colorChange is false, print the selected char
            Console.Write(c);
        }
        // Print a newline character if newLine is true
        if (newLine) Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White; // Reset text color to white
    }

    #endregion

    /*
            ################
            Input Validation
            ################
    */

    #region Input Validation

    /// <summary>
    /// Gets an int value from the user
    /// It will continue to prompt the user for a valid number if the input is outside of the specified range
    /// </summary>
    /// <param name="lower">Lower <c>int</c> bounds of range</param>
    /// <param name="upper">Upper <c>int</c> bounds of range</param>
    /// <param name="removeLines">
    /// Total number of lines used for displaying errors and user input.
    /// <c>int</c> value returned to be used later to clear these lines.
    /// </param>
    private static int GetIntInRange(int lower, int upper, out int removeLines) {
        
        removeLines = 1; // Initilize remove lines as 1; remove the input line

        string? inp = Console.ReadLine(); // Get user input

        // While the input is not valid, prompt again for valud input
        // Checks if the input is an int and if it is within range
        while (!int.TryParse(inp, out int selection) || selection < lower || selection > upper) {

            Console.SetCursorPosition(0, Console.CursorTop-1); // Reset Cursor to beginning of the line
            Print("\r/rInvalid Selection, Try Again\n /y> ", false); // Print an error and prompt for a new input -- no newline
            removeLines++; // Increment remove lines
            inp = Console.ReadLine(); // Read new input
        }
        // Get the int value from the string
        int.TryParse(inp, out int returnSelection);
        return returnSelection; // Return the int value
    }

    /// <summary>
    /// Get a string value from the user
    /// It will prompt the user with an error and to retry if the input is empty
    /// </summary>
    /// <param name="removeLines">
    /// Total number of lines used for displaying errors and user input.
    /// <c>int</c> value returned to be used later to clear these lines.
    /// </param>
    private static string GetString(out int removeLines) {

        removeLines = 1; // Initilize remove lines at 1; remove the input line

        string? inp = Console.ReadLine(); // Get user input

        // Verify input if not null or empty
        while (string.IsNullOrEmpty(inp)) {

            Console.SetCursorPosition(0, Console.CursorTop-1); // Reset cursor to beginning of the line
            Print("\r/rInvalid Input, Try Again\n /y> ", false); // Print an error and prompt for a new input -- no newline
            removeLines++; // Increment remove lines
            inp = Console.ReadLine(); // Read new input
        }
        
        return inp; // Return non-empty string
    }

    /// <summary>
    /// Get a file path or directory path from the user
    /// It will prompt the user with an error and to retry if the input is not a valid path
    /// </summary>
    /// <param name="removeLines">
    /// Total number of lines used for displaying errors and user input.
    /// <c>int</c> value returned to be used later to clear these lines.
    /// </param>
    private static string GetDirOrFile(out int removeLines) {
        
        removeLines = 1; // Initilize remove lines at 1; remove the input line

        string? inp = Console.ReadLine(); // Get user input
        
        // While the input is not a valid file or directory path, reprompt the user
        while (!(Directory.Exists(inp) || File.Exists(inp))) {
            
            Console.SetCursorPosition(0, Console.CursorTop-1); // Reset cursor to beginning of the line
            Print("\r/rInvalid Path, Try Again\n /y> ", false); // Print an error and prompt for a new input -- no newline
            removeLines++; // Increment remove lines
            inp = Console.ReadLine(); // Read new input
        }

        return inp; // Return valid path
    }

    #endregion
    
}