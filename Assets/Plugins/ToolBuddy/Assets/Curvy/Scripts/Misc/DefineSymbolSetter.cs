// =====================================================================
// Copyright © 2013 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Makes sure the define symbols are updated to include the ones from Curvy Splines.
    /// </summary>
    [InitializeOnLoad]
    internal class DefineSymbolSetter
    {
        static DefineSymbolSetter()
        {
            string[] curvySplineDefineSymbols =
            {
                CompilationSymbols.CurvySplines,
                $"{CompilationSymbols.CurvySplines}_{AssetInformation.Version.Replace('.','_')}"
            };

            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;


            IEnumerable<string> existingSymbols =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup)
                    .Split(';')
                    .Select(s => s.Trim())
                    .ToList();

            IEnumerable<string> symbolsExceptCurvy =
                existingSymbols.Where(s => s.StartsWith(CompilationSymbols.CurvySplines) == false);

            IEnumerable<string> symbolsIncludingCurvy = symbolsExceptCurvy.Concat(curvySplineDefineSymbols).ToList();

            bool areSymbolsDifferent = new HashSet<string>(existingSymbols).SetEquals(symbolsIncludingCurvy) == false;

            if (areSymbolsDifferent)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    buildTargetGroup,
                    string.Join(
                        ";",
                        symbolsIncludingCurvy
                    )
                );
        }
    }
}

#endif