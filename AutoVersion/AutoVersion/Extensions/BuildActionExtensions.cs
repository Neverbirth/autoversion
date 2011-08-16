using System;
using System.Collections.Generic;
using System.Text;

namespace AutoVersion.Extensions
{
    static class BuildActionExtensions
    {
        public static bool EqualsType(this BuildAction action, BuildActionType actionType)
        {
            return (actionType == BuildActionType.Both ||
                  (actionType == BuildActionType.Build && action == BuildAction.Building) ||
                  (actionType == BuildActionType.Testing && action == BuildAction.Testing));

        }
    }
}
