using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CommandLine;

using task_friend.Exceptions;
using task_friend.Logging;

namespace task_friend {
  internal class Program {
    private static readonly object Mutex = new object();
    private static readonly object Mutex_TaskScope = new object();
    private static readonly object Mutex_ProcessScope = new object();
    private static Logger _logger;
    private static int _numberOfThreads;
    private static IList<Task> _tasks;
    private static List<Process> _processes;
    private static CancellationTokenSource _tokenSource;

    [STAThread]
    private static void Main( string[] args ) {
      AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomainOnAssemblyResolve;
      try {
        ProcessInternal( args );
      } catch ( Exception ex ) {
        Console.WriteLine( ex.Message + Environment.NewLine + Environment.NewLine + ex );
      }
    }

    private static void ProcessInternal( string[] args ) {
      var options = new Options();
      if ( !Parser.Default.ParseArguments( args, options ) ) {
        return;
      }

      if ( !File.Exists( options.InputFile ) ) {
        Console.WriteLine( "File {0} does not exist.", options.InputFile );
        return;
      }
      if ( options.Threads < 1 ) {
        Console.WriteLine( "Specify more than zero concurrent tasks if you want me to work for you!" );
        return;
      }
      if ( options.Timeout < 100 ) {
        Console.WriteLine( "Gimme some space, man - set a timout value of at least 100ms if you want me to work for you!" );
        return;
      }

      _logger = new Logger( options );

      var commands = GetCommands( options );

      Stopwatch globalStopwatch = Stopwatch.StartNew();
      PrintStartMessage();

      _tokenSource = new CancellationTokenSource();
      CancellationToken cancellationToken = _tokenSource.Token;

      _numberOfThreads = 0;

      _tasks = new List<Task>();
      _processes = new List<Process>();

      foreach ( Command cmd in commands ) {
        if( !File.Exists( cmd.Executable ) ) {
          Console.WriteLine(
            "[{0}] I can't find the executable {1} - are you sure you got the path right?",
            DateTime.UtcNow.ToShortTimeString(),
            cmd.Executable );
          if ( TryBreakOnErrors( options ) ) {
            break;
          }
          Console.WriteLine( "    I'll ignore this one, but continue with the other tasks." );
          continue;
        }
        var process = CreateProcess( cmd );
        var task = new Task( () => RunProcess( process, options ), cancellationToken, TaskCreationOptions.LongRunning );
        _tasks.Add( task );
      }

      Task.Factory.StartNew( () => HandleUserExit( _tokenSource ), TaskCreationOptions.LongRunning );

      while ( !cancellationToken.IsCancellationRequested ) {
        if ( ReadyForNewTask( options ) ) {
          StartTask();
        }
        if ( !ThreadIsAvailable( options ) ) {
          WaitForAvailableThread();
        }
        if ( !TaskIsAvailable() ) {
          break;
        }
      }
      Task.WaitAll( _tasks.Where( IsAvailable ).ToArray() );
      _logger.AddDebug( "[{0}] Completed in {1}ms", DateTime.UtcNow, globalStopwatch.ElapsedMilliseconds );
    }

    private static void WaitForAvailableThread() {
      Task.WaitAny( _tasks.ToArray() );
    }

    private static bool ThreadIsAvailable( Options options ) {
      return _numberOfThreads < options.Threads;
    }

    private static bool ReadyForNewTask( Options options ) {
      return ThreadIsAvailable( options ) && TaskIsAvailable();
    }

    private static bool TaskIsAvailable() {
      return _tasks.Any( IsAvailable );
    }

    private static bool IsAvailable( Task t) {
      return !t.IsCompleted && !t.IsCanceled && !t.IsFaulted;
    }

    private static void StartTask() {
      lock ( Mutex ) {
        _numberOfThreads++;

        Task task = _tasks.FirstOrDefault( t => t.Status == TaskStatus.Created );
        if ( task != null ) {
          _logger.AddDebug( "[{0}] Preparing to start new thread...", DateTime.UtcNow.ToShortTimeString() );
          task.Start();
        }
      }
    }

    private static IEnumerable<Command> GetCommands( Options options ) {
      var commands = new List<Command>();
      try {
        commands =
          File.ReadAllLines( options.InputFile )
            .Where( l => !String.IsNullOrWhiteSpace( l ) )
            .Select( l => new Command( l ) )
            .ToList();
      } catch ( InputException ex ) {
        Console.WriteLine( ex.Message );
      }
      return commands;
    }

    private static void HandleUserExit( CancellationTokenSource tokenSource ) {
      while ( _tokenSource.IsCancellationRequested || Console.ReadKey().Key != ConsoleKey.Q ) {
        Thread.Sleep( 100 );
      }
      Console.WriteLine( "" );
      Console.WriteLine( "[{0}] Cancellation was detected - waiting for running tasks to complete...", DateTime.UtcNow.ToShortTimeString() );
      tokenSource.Cancel( true );
      foreach ( Process process in _processes ) {
        lock ( Mutex_ProcessScope ) {
          if ( !process.HasExited ) {
            process.Kill();
          }
        }
      }
    }

    private static void RunProcess( Process process, Options options ) {
      lock ( Mutex_TaskScope ) {
        _logger.AddDebug( "[{0}] Starting process of task {1}", DateTime.UtcNow.ToShortTimeString(), Task.CurrentId );
        _logger.AddDebug( "        Executable {0}", process.StartInfo.FileName );
        _logger.AddDebug( "        Parameters {0}", process.StartInfo.Arguments );
      }

      Stopwatch sw = Stopwatch.StartNew();

      _processes.Add( process );
      process.Start();
      if ( !process.WaitForExit( options.Timeout ) ) {
        lock ( Mutex_TaskScope ) {
          Console.WriteLine(
            "[{0}] Task {1} timed out after {2}ms",
            DateTime.UtcNow.ToShortTimeString(),
            Task.CurrentId,
            sw.ElapsedMilliseconds );
          _logger.AddDebugTimeout(
            "[{0}] Process Information on Task {1}:", DateTime.UtcNow.ToShortTimeString(), Task.CurrentId );
          _logger.AddDebugTimeout( "        Executable {0}", process.StartInfo.FileName );
          _logger.AddDebugTimeout( "        Parameters {0}", process.StartInfo.Arguments );
        }
        _logger.AddDebug( "        Consider increasing the timeout by adding a -t parameter to task-friend. Run task-friend.exe --help for more info." );
        if ( TryBreakOnErrors( options ) ) {
          return;
        }
      }

      while ( !process.StandardOutput.EndOfStream ) {
        string line = process.StandardOutput.ReadLine();
        _logger.AddDebug( "[{0}] Task {1} says: {2}", DateTime.UtcNow.ToShortTimeString(), Task.CurrentId, line );
      }

      if ( process.ExitCode != 0 && !_tokenSource.IsCancellationRequested ) {
        string err = process.StandardError.ReadToEnd();
        _logger.Log( "[{0}] AN ERROR OCCURRED IN TASK {1}", DateTime.UtcNow.ToShortTimeString(), Task.CurrentId );
        _logger.Log( err );
        TryBreakOnErrors( options );
      }

      _numberOfThreads--;
      //_processes.Remove( process );
      _logger.AddDebug( "[{2}] Finished task {0} in {1}ms", Task.CurrentId, sw.ElapsedMilliseconds, DateTime.UtcNow.ToShortTimeString() );
    }

    private static bool TryBreakOnErrors( Options options ) {
      if ( options.BreakOnErrors ) {
        Console.WriteLine( "[{0}] Break On Errors is enabled. Task Friend exits.", DateTime.UtcNow.ToShortTimeString() );
        _tokenSource.Cancel( true );
      }
      return options.BreakOnErrors;
    }

    private static Process CreateProcess( Command cmd ) {
      var workingDir = Path.GetDirectoryName( cmd.Executable );

      var processStartInfo = new ProcessStartInfo {
        FileName = cmd.Executable,
        Arguments = cmd.Parameters,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };
      if ( !String.IsNullOrEmpty( workingDir ) ) {
        processStartInfo.WorkingDirectory = workingDir;
      }
      var process = new Process();
      process.StartInfo = processStartInfo;
      process.EnableRaisingEvents = true;
      return process;
    }

    private static void PrintStartMessage() {
      Console.WriteLine( "" );
      _logger.AddDebug( "[{0}] Started task-friend.", DateTime.UtcNow );
      _logger.Log( "Doing work for you..." );
      _logger.Log( "---------------------------------------" );
      _logger.Log( "Press 'q' to abort all tasks and quit" );
      _logger.AddDebug( "---------------------------------------" );
    }
  }
}