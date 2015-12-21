using System;
using System.Runtime.Serialization;

namespace task_friend.Exceptions {
  [Serializable]
  public class InputException : Exception {
    public InputException() {
    }

    public InputException( string message )
      : base( message ) {
    }

    public InputException( string message, Exception inner )
      : base( message, inner ) {
    }

    protected InputException(
      SerializationInfo info,
      StreamingContext context )
      : base( info, context ) {
    }
  }
}