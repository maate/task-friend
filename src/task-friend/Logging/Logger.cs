using System;

namespace task_friend.Logging {
  internal class Logger {
    private readonly bool _debug;

    private readonly bool _silent;

    public Logger( bool debug, bool silent ) {
      _debug = debug;
      _silent = silent;
    }

    public void AddDebug( string message, params object[] args ) {
      if ( _debug && !_silent ) {
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