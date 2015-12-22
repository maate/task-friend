using System;

namespace task_friend.Logging {
  internal class Logger {
    private readonly bool _debug;

    private readonly bool _silent;
    private bool _debugTimeout;

    public Logger( Options options ) {
      _debug = options.Debug;
      _silent = options.Silent;
      _debugTimeout = options.DebugTimeout;
    }

    public void AddDebug( string message, params object[] args ) {
      if ( _debug && !_silent ) {
        Console.WriteLine( message, args );
      }
    }

    public void AddDebugTimeout( string message, params object[] args ) {
      if ( ( _debug || _debugTimeout ) && !_silent ) {
        Console.WriteLine( message, args );
      }
    }

    public void Log( string message, params object[] args ) {
      if ( !_silent ) {
        Console.WriteLine( message, args );
      }
    }
  }
}