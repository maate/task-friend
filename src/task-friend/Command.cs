using System;
using System.Text.RegularExpressions;

using task_friend.Exceptions;

namespace task_friend {
  class Command {
    static readonly Regex Regex = new Regex( @"(?<cmd>^""[^""]*""|\S*)\s*(?<prm>.*)?" );

    public Command( string input ) {
      var match = Regex.Match( input );
      if ( !match.Groups["cmd"].Success ) {
        throw new InputException(
          String.Format(
            "I didn't understand this input -- to me, it seems like it doesn't have an executable: {0}",
            input ) );
      }
      Executable = match.Groups["cmd"].Value.Trim();
      Parameters = match.Groups["prm"].Value.Trim();
    }

    public string Executable { get; set; }
    public string Parameters { get; set; }
  }
}
