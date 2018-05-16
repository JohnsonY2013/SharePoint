using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace jy.utilities
{
    public static class JyClone
    {
        /// <summary>
        /// Deep copy, by Serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone_Serialization<T>(T source)
        {
            T returnValue;

            var fileStream = new FileStream("Temp.dat", FileMode.Create);
            var formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fileStream, source);
            }
            catch (SerializationException ex)
            {
                throw new Exception(string.Format("Failed to serialize - {0}", ex.Message));
            }
            finally
            {
                fileStream.Close();
            }

            try
            {
                fileStream = new FileStream("Temp.dat", FileMode.Open);
                returnValue = (T)formatter.Deserialize(fileStream);
            }
            catch (SerializationException ex)
            {
                throw new Exception(string.Format("Failed to deserialize - {0}", ex.Message));
            }
            finally
            {
                fileStream.Close();

                try { File.Delete("Temp.dat"); }
                catch { }
            }

            return returnValue;
        }

        /// <summary>
        /// Deep copy, by Reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iSource"></param>
        /// <returns></returns>
        public static T Clone_Reflection<T>(T source)
        {
            T returnValue;

            var targetType = source.GetType();

            if (targetType.IsValueType)
            {
                returnValue = source;
                return returnValue;
            }

            returnValue = (T)Activator.CreateInstance(targetType);

            foreach (var member in targetType.GetMembers())
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            var field = (FieldInfo)member;
                            var fieldValue = field.GetValue(source);

                            if (fieldValue is ICloneable)
                                field.SetValue(returnValue, (fieldValue as ICloneable).Clone());
                            else
                                field.SetValue(returnValue, Clone_Reflection(fieldValue));
                        }
                        break;
                    case MemberTypes.Property:
                        {
                            var property = (PropertyInfo)member;

                            if (property.GetSetMethod(false) != null)
                            {
                                var propertyValue = property.GetValue(source, null);

                                if (propertyValue is ICloneable)
                                    property.SetValue(source, (propertyValue as ICloneable).Clone(), null);
                                else property.SetValue(source, Clone_Reflection(propertyValue), null);
                            }
                        }
                        break;
                }
            }

            return returnValue;
        }

    }
}