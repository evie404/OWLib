using System;
using System.Collections;
using System.Collections.Generic;

namespace DataTool.Helper {
    public static class LinqExtensions {
        public static IEnumerable<object> OfTypes(this IEnumerable source, params Type[] types) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return OfTypesIterator(source, types);
        }

        private static IEnumerable<object> OfTypesIterator(IEnumerable source, params Type[] types) {
            foreach (var obj in source) {
                var objType = obj.GetType();
                foreach (var type in types)
                    if (type?.IsAssignableFrom(objType) == true)
                        yield return obj;
            }
        }
    }
}
