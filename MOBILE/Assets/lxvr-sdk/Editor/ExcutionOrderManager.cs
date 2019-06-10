using System;
using UnityEditor;

namespace Looxid
{
    [InitializeOnLoad]
    public class ExcutionOrderManager
    {
        static ExcutionOrderManager()
        {
            foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (monoScript.GetClass() != null)
                {
                    foreach (var attribute in Attribute.GetCustomAttributes(monoScript.GetClass(), typeof(ExcutionOrderAttribute)))
                    {
                        var currentOrder = MonoImporter.GetExecutionOrder(monoScript);
                        var newOrder = ((ExcutionOrderAttribute)attribute).order;
                        if (currentOrder != newOrder)
                            MonoImporter.SetExecutionOrder(monoScript, newOrder);
                    }
                }
            }
        }
    }
}
