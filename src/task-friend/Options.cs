using CommandLine;
using CommandLine.Text;

namespace task_friend {
  public class Options {
    [Option( 'i', "input", Required = true,
      HelpText = "Input file to be processed. Each line in the file should be a command line input." )]
    public string InputFile { get; set; }

    [Option( 'c', "concurrent", Required = false, HelpText = "Number of concurrent tasks", DefaultValue = 10)]
    public int Threads { get; set; }

    [Option( 't', "timeout", Required = false, HelpText = "Timeout in milliseconds", DefaultValue = 60000 )]
    public int Timeout { get; set; }

    [Option( 'd', "debug", Required = false, HelpText = "Outputs debug information" )]
    public bool Debug { get; set; }

    [Option( "debug-timeout", Required = false, HelpText = "Outputs timeout information" )]
    public bool DebugTimeout { get; set; }

    [Option( 's', "silent", Required = false, HelpText = "No command line output" )]
    public bool Silent { get; set; }

    [Option( 'b', "break", Required = false,
      HelpText =
        "Break on errors - when enabled Task Friend don't resume task processing if a process returns an error (default is false)",
      DefaultValue = false )]
    public bool BreakOnErrors { get; set; }

    [HelpOption]
    public string GetUsage() {
      var help = new HelpText {
        Heading = new HeadingInfo( "Task Friend", "1.0.0" ),
        Copyright = new CopyrightInfo( "Your Friend, Morten", 2015 ),
        AdditionalNewLineAfterOption = true,
        AddDashesToOption = true
      };
      help.AddPreOptionsLine( "Usage: task-friend.exe -i c:\\myinput.txt -c 10 [-d] [-s] [-b] [-t milliseconds] [--debug-timeout]" );
      help.AddPreOptionsLine(
        "Runs tasks from an input file (-i) on a number of concurrent tasks (-c) until all tasks have been processed" );
      help.AddOptions( this );
      return help;
    }
  }
}