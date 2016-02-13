using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using NUnit.Framework;

namespace task_friend_tests {
  [TestFixture]
  public class InputTests {

    [Test]
    public void UTF8EncodedInput() {
      File.WriteAllLines( "input.txt", new[] { "lazy-friend.exe", "lazy-friend.exe" }, Encoding.UTF8 );
      Run( "-s" );
    }

    [Test]
    public void ASCIIEncodedInput() {
      File.WriteAllLines( "input.txt", new[] { "lazy-friend.exe", "lazy-friend.exe" }, Encoding.ASCII );
      Run( "-s" );
    }

    [TestCase( 3, 3 )]
    [TestCase( 1, 3 )]
    [TestCase( 3, 1 )]
    [TestCase( 11, 3 )]
    public void WhenRunningTasks_ThenAllTasksAreRun( int numberOfTasks, int concurrent ) {
      var tasks = new List<string>();
      for ( int i = 0; i < numberOfTasks; i++ ) {
        tasks.Add( "lazy-friend.exe task_" + i );
      }
      File.WriteAllLines( "input.txt", tasks.ToArray() );
      var output = Run( "-d -c " + concurrent );
      for ( int i = 0; i < numberOfTasks; i++ ) {
        Assert.That( output.Contains( "task_" + i ), "Could not find task_" + i );
      }
    }

    [Test]
    public void Abort() {
      var tasks = new List<string>();
      for ( int i = 0; i < 10; i++ ) {
        tasks.Add( "lazy-friend.exe task_" + i );
      }
      File.WriteAllLines( "input.txt", tasks.ToArray() );
      var output = Run( "-d -c 3", p => {
        p.StandardInput.AutoFlush = true;
        Thread.Sleep( 100 );
        p.StandardInput.WriteLine( 'q' );
      } );
      Console.WriteLine(output);
    }

    private static string Run( string args = "", Action<Process> delayed = null ) {
      var processStartInfo = new ProcessStartInfo {
        FileName = "task-friend.exe",
        Arguments = "-i input.txt " + args,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };
      var process = new Process();
      process.StartInfo = processStartInfo;
      process.EnableRaisingEvents = true;
      process.Start();

      if( delayed != null ) {
        delayed( process );
      }

      process.WaitForExit();

      var output = "";
      while ( !process.StandardOutput.EndOfStream ) {
        string line = process.StandardOutput.ReadLine();
        output += line;
      }

      if ( process.ExitCode != 0 ) {
        string err = process.StandardError.ReadToEnd();
        Console.WriteLine( err );
      }
      return output;
    }
  }
}