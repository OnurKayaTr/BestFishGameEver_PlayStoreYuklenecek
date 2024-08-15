using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GIGA.PixelCableRenderer.Demo
{


    public static class CableRendererReflectionHelper 
    {
        public static FieldInfo[] GetFields<T>(T type,string[] namesFilter) where T : CableRenderer
        {
            var fields = type.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<FieldInfo> filteredFields = new List<FieldInfo>();
            List<string> filters = new List<string>(namesFilter);
            foreach (var f in fields)
                if (filters.IndexOf(f.Name) >= 0)
                    if(filters.IndexOf(f.Name) < filteredFields.Count)
                        filteredFields.Insert(filters.IndexOf(f.Name),f);
					else
                        filteredFields.Add(f);

            return filteredFields.ToArray();
        }

        public static void IncreaseValue<T>(FieldInfo field, T cableRenderer,float delta,float minValue= 0,float maxValue = 1,bool decrease = false) where T : CableRenderer
        {
            float direction = decrease ? -1 : 1;
            object currentValue = field.GetValue(cableRenderer);
            object newValue = null;
            if (currentValue is float)
                newValue = Mathf.Clamp((float)currentValue + delta * direction,minValue,maxValue);
            else if (currentValue is int)
                newValue = (int)Mathf.Clamp((int)currentValue + Mathf.Max(1,delta) * direction, minValue, maxValue);
            else if (currentValue is bool)
                newValue = (bool)((bool)currentValue ? false : true);

            field.SetValue(cableRenderer, newValue);
        }
    }
}
