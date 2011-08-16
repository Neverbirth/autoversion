using System;
using System.Collections.Generic;
using System.Text;

namespace AutoVersion.Extensions
{
    static class BuildExtensions
    {
        public static bool EqualsType(this BuildAction action, BuildActionType actionType)
        {
            return (actionType == BuildActionType.Both ||
                  (actionType == BuildActionType.Build && action == BuildAction.Building) ||
                  (actionType == BuildActionType.Testing && action == BuildAction.Testing));

        }

        public static bool IsEqualToTraceValue(this BuildConfiguration configuration, bool traceEnabled)
        {
            return (configuration == BuildConfiguration.Any ||
                  (configuration == BuildConfiguration.Build && !traceEnabled) ||
                  (configuration == BuildConfiguration.Debug && traceEnabled));

        }
    }
}
