using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Reflection;

namespace WebApi
{
    public class SerilogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
        {
            var skipFrames = 3;
            MethodBase method = null;

            while (method == null && skipFrames < 15) // go a bit deeper
            {
                var frame = new StackFrame(skipFrames, false);
                method = frame.GetMethod();

                var ns = method?.DeclaringType?.Namespace ?? "";

                // Skip all internal logging and async plumbing namespaces
                if (ns.StartsWith("Serilog")
                    || ns.StartsWith("Microsoft.Extensions.Logging")
                    || ns.StartsWith("System.Threading.Tasks"))
                {
                    method = null;
                    skipFrames++;
                    continue;
                }
                break;
            }

            if (method != null)
            {
                var caller = $"{method.DeclaringType?.FullName}.{method.Name}";
                logEvent.AddPropertyIfAbsent(factory.CreateProperty("CallerReal", caller));
            }
        }
    }
}