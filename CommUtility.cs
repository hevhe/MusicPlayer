
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MusicPlayer
{

    public static class CommUtility
    {

        public static void SetValue(object entity, string fieldName, object fieldValue)
        {
            Type entityType = entity.GetType();
            PropertyInfo propertyInfo = entityType.GetProperty(fieldName);
            if (propertyInfo == null) return;
            propertyInfo.SetValue(entity, fieldValue, null);
        }

        public static object GetValue(object entity, string fieldName)
        {
            Type entityType = entity.GetType();
            PropertyInfo property = entityType.GetProperty(fieldName);
            if (property == null) return null;
            object o = property.GetValue(entity, null);
            return o;
        }

        public static Type GetPropertyType(object entity, string fieldName)
        {
            Type entityType = entity.GetType();
            PropertyInfo property = entityType.GetProperty(fieldName);
            if (property == null) return null;
            return property.PropertyType;
        }

        public static bool CheckHasProperty(object entity, string propertyName)
        {
            Type entityType = entity.GetType();
            PropertyInfo property = entityType.GetProperty(propertyName);
            if (property == null) return false;
            return true;
        }


        public static Dictionary<string, string> GetDescriptionAttr(Type t)
        {
            var result = new Dictionary<string, string>();

            foreach (var propertyInfo in t.GetProperties())
            {
                var attrs = propertyInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                if (attrs != null)
                {
                    result.Add(propertyInfo.Name, ((DescriptionAttribute)attrs).Description);
                }
            }

            return result;
        }

        public static T GetAttributes<T>(Type t)
        {
            var attrs = t.GetCustomAttributes(typeof(T), true).FirstOrDefault();
            return (T)attrs;
        }

        public static Dictionary<string, string> GetJsonPropertyAttr(Type t)
        {
            var result = new Dictionary<string, string>();

            foreach (var propertyInfo in t.GetProperties())
            {
                var attrs = propertyInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault();
                if (attrs != null)
                {
                    result.Add(propertyInfo.Name, ((JsonPropertyAttribute)attrs).PropertyName);
                }
            }

            return result;
        }
        public static TResult ParentAutoCopyToChild<TSource, TResult>(TSource source)
        where TResult : TSource, new()
        {
            TResult child = new TResult();
            var parentType = typeof(TSource);
            var properties = parentType.GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(child, propertyInfo.GetValue(source, null), null);
                }
            }
            return child;
        }

        public static TResult ChildAutoCopyToParent<TSource, TResult>(TSource source)
            where TSource : TResult, new()
            where TResult : new()
        {
            TResult parent = new TResult();
            var parentType = typeof(TResult);
            var childType = typeof(TSource);
            var properties = childType.GetProperties();
            var parentProperties = parentType.GetProperties();
            foreach (var propertyInfo in parentProperties)
            {
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(parent, propertyInfo.GetValue(source, null), null);
                }
            }
            return parent;
        }

        public static T Copy<T>(T source, List<string> ignoreProps) where T : new()
        {
            T res = new T();
            var type = typeof(T);
            var properties = type.GetProperties();
            foreach (var p in properties)
            {
                if (ignoreProps != null && ignoreProps.Contains(p.Name))
                    continue;
                if (p.CanRead && p.CanWrite)
                {
                    p.SetValue(res, p.GetValue(source, null), null);
                }
            }
            return res;
        }


        public static object GetFieldValue(object entity, string fieldName)
        {
            Type entityType = entity.GetType();
            var fieldInfo = entityType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (fieldInfo == null) return null;
            object o = fieldInfo.GetValue(entity);
            return o;
        }

        public static void SetFieldValue(object entity, string fieldName, string val)
        {
            Type entityType = entity.GetType();
            var fieldInfo = entityType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (fieldInfo == null) return;
            fieldInfo.SetValue(entity, fieldName);
        }

        public static Dictionary<string, FieldInfo> GetFields(object entity)
        {
            var result = new Dictionary<string, FieldInfo>();
            Type entityType = entity.GetType();
            return GetFields(entityType);
        }

        public static Dictionary<string, FieldInfo> GetFields(Type t)
        {
            var result = new Dictionary<string, FieldInfo>();

            var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in fields)
            {
                result.Add(field.Name, field);
            }
            return result;
        }

        public static void SetPropertyRange(object entity, object property, string methodName, IList<object> rangeValue)
        {
            Type entityType = ((PropertyInfo)entity).PropertyType;
            //object e = ((PropertyInfo)property).GetValue(property, null);
            var methods = entityType.GetMethods();
            if (methods == null)
                return;
            else
            {
                var mi = methods.FirstOrDefault(c => c.Name == methodName);
                foreach (var item in rangeValue)
                {
                    mi.Invoke(property, new object[] { item });
                }
            }

        }

        public static object GetProperty(object entity, string fieldName)
        {
            Type entityType = entity.GetType();
            PropertyInfo property = entityType.GetProperty(fieldName);
            if (property == null) return null;
            return property;
        }

        public static Dictionary<string, object> GetPropertys(object entity, List<string> ignoreProps = null)
        {
            var type = entity.GetType();
            var properties = type.GetProperties();
            var res = new Dictionary<string, object>();
            foreach (var p in properties)
            {
                if (ignoreProps != null && ignoreProps.Contains(p.Name))
                    continue;
                if (p.CanRead && p.CanWrite)
                {
                    res.Add(p.Name, GetValue(entity, p.Name));
                }
            }
            return res;
        }


        public static Dictionary<string, object> GetPropertysWithJsonProp(object entity, List<string> ignoreProps = null)
        {
            var type = entity.GetType();
            var properties = type.GetProperties();
            var jsonProperties = GetJsonPropertyAttr(type);
            var res = new Dictionary<string, object>();
            foreach (var p in properties)
            {
                if (ignoreProps != null && ignoreProps.Contains(p.Name))
                    continue;
                if (p.CanRead && p.CanWrite)
                {
                    if (jsonProperties.ContainsKey(p.Name))
                    {
                        var jp = jsonProperties[p.Name];
                        res.Add(jp, GetValue(entity, p.Name));
                    }
                }
            }
            return res;
        }

        public static PropertyInfo[] GetPropertyInfos(Type t)
        {
            return t.GetProperties();
        }

    }
}
