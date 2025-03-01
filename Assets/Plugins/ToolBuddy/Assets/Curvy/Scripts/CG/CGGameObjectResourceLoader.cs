// =====================================================================
// Copyright © 2013 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// GameObject resource loader class
    /// </summary>
    public class CGGameObjectResourceLoader : ICGResourceLoader
    {
#if UNITY_2021_3_OR_NEWER
        [EnvironmentAgnosticInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
#endif
        protected static void InitializeOnLoad() =>
            CGResourceHandler.RegisterResourceLoader(
                "GameObject",
                new CGGameObjectResourceLoader()
            );

        public Component Create(CGModule cgModule, string context)
            => cgModule.Generator.PoolManager.GetPrefabPool(context).Pop().transform;

        public void Destroy(CGModule cgModule, Component obj, string context, bool kill)
        {
            if (obj != null)
            {
                if (kill)
                    obj.gameObject.Destroy(
                        false,
                        false
                    );
                else
                    cgModule.Generator.PoolManager.GetPrefabPool(context).Push(obj.gameObject);
            }
        }
    }
}