using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EFCoreSamples.StabilityAndPerformance.Api.Utils
{
    public static class WithTagsExtensions
    {
        public static IQueryable<T> TagWithContext<T>(this IQueryable<T> queryable, string message = "", [CallerFilePath] string callerFileName = "", [CallerMemberName] string callerName = "")
        {
            string logScopeName = GenerateLogScopeName(message, callerFileName, callerName);
            return queryable.TagWith(logScopeName);
        }

        private static string GenerateLogScopeName(string message = null, string callerFileName = "", string callerName = "")
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                message = "-" + message;
            }

            string className = Path.GetFileNameWithoutExtension(callerFileName);
            return className + "-" + callerName + message;
        }
    }
}
