using System;

namespace task_friend.Logging {
  internal class Logger {
    private readonly ILogOptions _options;

   public Logger( ILogOptions options ) {
      _options = options;
    }

    public void AddDebug( string message, params object[] args ) {
      if ( _options.Debug && !_options.Silent ) {
        Console.WriteLine( message, args );
        Flush();
      }
    }

    public void AddDebugTimeout( string message, params object[] args ) {
      if ( ( _options.Debug || _options.DebugTimeout ) && !_options.Silent ) {
        Console.WriteLine( message, args );
      }
    }

    public void Log( string message, params object[] args ) {
      if ( !_options.Silent ) {
        Console.WriteLine( message, args );
        Flush();
      }
    }

    private void Flush() {
      if ( _options.Flush ) {
        Console.Out.Flush();
      }
    }
  }
}
