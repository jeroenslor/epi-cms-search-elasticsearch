using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EPiServer.ServiceLocation;

namespace EPi.Cms.Search.Elasticsearch.Indexing.TypeMap
{
    [ServiceConfiguration(typeof(IIndexableTypeMapperResolver), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TypeScanIndexableTypeMapperResolver : IIndexableTypeMapperResolver
    {
        public IEnumerable<IIndexableTypeMapper> GetAll()
        {
            return GetTypesChildOf<IIndexableTypeMapper>().Select(Activator.CreateInstance).OfType<IIndexableTypeMapper>();
        }

        public static IEnumerable<Type> GetTypesChildOf<T>()
        {
            var types = new List<Type>();

            foreach (var assembly in GetAssemblies())
            {
                types.AddRange(GetTypesChildOfInAssembly(typeof(T), assembly));
            }

            return types;
        }

        protected static IEnumerable<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        private static IEnumerable<Type> GetTypesChildOfInAssembly(Type type, Assembly assembly)
        {
            try
            {
                return assembly.GetTypes().Where(t => type.IsAssignableFrom(t) && t.IsClass);
            }
            catch (Exception)
            {
                // there could be situations when type could not be loaded
                // this may happen if we are visiting *all* loaded assemblies in application domain
                return Enumerable.Empty<Type>();
            }
        }
    }
}
