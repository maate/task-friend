using System;
using System.Threading;

namespace lazy_friend {
  /// <summary>
  ///   Lazy Friend sleeps
  /// </summary>
  internal class Program {
    private static void Main( string[] args ) {
      if ( args.Length > 0 && args[0] == "ERROR" ) {
        throw new InvalidOperationException( "Throwing an error - like you told me!" );
      }
      int gimmeanumber = new Random().Next( 10, 100 );
      Console.Write( "Running lazy-friend with arguments " );
      var argsString = "";
      foreach ( string arg in args ) {
        argsString += arg;
      }
      Console.WriteLine( argsString );
      Console.WriteLine( gimmeanumber );
      for ( int i = 0; i < gimmeanumber; i = i + 1000 ) {
        Console.WriteLine( "Thread with args '{0}' is still sleeping... - Come back later!", argsString );
        Thread.Sleep( 1000 );
      }
    }
  }
}