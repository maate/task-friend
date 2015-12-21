using System;
using System.IO;
using System.Reflection;

namespace task_friend {
  internal class AssemblyHelper {
    public static Assembly CurrentDomainOnAssemblyResolve( object sender, ResolveEventArgs args ) {
      String resourceName = typeof(Program).Namespace + "." + new AssemblyName( args.Name ).Name + ".dll";

      using ( Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream( resourceName ) ) {
        if ( stream == null ) {
          Console.WriteLine( "Internal Error during resolution of {0} as {1}", args.Name, resourceName );
          return null;
        }
        var assemblyData = new Byte[stream.Length];

        stream.Read( assemblyData, 0, assemblyData.Length );

        return Assembly.Load( assemblyData );
      }
    }
  }
}