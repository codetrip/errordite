using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using CodeTrip.Core.Dynamic;
using Newtonsoft.Json;
using ProtoBuf;
using Formatting = System.Xml.Formatting;

namespace CodeTrip.Core
{
    /// <summary>
    /// Non typed version of the helper class for type anonymous methods
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// This will indent format the xml string supplied
        /// </summary>
        /// <param name="unformattedXml">The unformatted xml string to format</param>
        /// <returns>An indent formatted xml string</returns>
        public static string Format(string unformattedXml)
        {
            string xml;
            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb))
            {
                using (XmlTextWriter xtr = new XmlTextWriter(sw))
                {
                    xtr.Formatting = Formatting.Indented;

                    // loading into DOM triggers the entire 
                    // document to be formatted on the write
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(unformattedXml);
                    xmlDoc.WriteTo(xtr);
                    xtr.Flush();

                    xml = sw.ToString();
                }
            }

            return xml;
        }

        /// <summary>
        /// This will serialise the object. The default namespace and 
        /// PI (including encoding) will be included
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <returns>The serialized object</returns>
        public static string Serialize<T>(T obj)
        {
            return SerializationHelper<T>.Serialize(obj);
        }

        /// <summary>
        /// This will serialise the object. The output will NOT be indent formatted
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="removeProcessingInstruction">removes the XML processing instruction if set to true</param>
        /// <param name="removeNamespace">If true removes the default namespace</param>
        /// <returns>The serialized object</returns>
        public static string Serialize<T>(T obj, bool removeProcessingInstruction, bool removeNamespace)
        {
            return SerializationHelper<T>.Serialize(obj, removeProcessingInstruction, removeNamespace);
        }

        /// <summary>
        /// This will serialise the object. Allows control over namespace, PI and formatting
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="removeProcessingInstruction">removes the XML processing instruction if set to true</param>
        /// <param name="removeNamespace">If true removes the default namespace</param>
        /// <param name="formatted">If true the output string is indent formatted</param>
        /// <returns>The serialized object</returns>
        public static string Serialize<T>(T obj, bool removeProcessingInstruction, bool removeNamespace, bool formatted)
        {
            return SerializationHelper<T>.Serialize(obj, removeProcessingInstruction, removeNamespace, formatted);
        }
        
        public static string Serialize<T>(T obj, XmlSerializerNamespaces xmlSerializerNamespaces)
        {
            return SerializationHelper<T>.Serialize(obj, xmlSerializerNamespaces);
        }

        /// <summary>
        /// Deserialise JSON to a dymanic object
        /// </summary>
        /// <param name="json">The json to deserialize.</param>
        /// <returns></returns>
        public static dynamic DeserializeJsonToDynamic(string json)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            return serializer.Deserialize(json, typeof(object));
        }

        /// <summary>
        /// This will serialise the object to a string and will include the namespaces 
        /// specified in the XmlSerializerNamespaces parameter in the serialized string
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <returns>The serialized object</returns>
        public static string Serialize(object obj)
        {
            StringBuilder stringBuilder = new StringBuilder();
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)))
            {
                xmlSerializer.Serialize(xmlTextWriter, obj);
            }

            return stringBuilder.ToString();
        }
        
        /// <summary>
        /// Deserialises an object of the given type.
        /// </summary>
        /// <remarks>If we cannot cast to T we will get an exception.  The driver
        /// for this method is XmlSerialisationInheritedTypesPackerUnpacker.</remarks>
        /// <param name="xml">The xml to deserialise.</param>
        /// <param name="type">The type of the serialised object.</param>
        /// <returns>The deserialised object.</returns>
        public static object Deserialize(string xml, Type type)
        {
            using (XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(xml)))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(type);
                return xmlSerializer.Deserialize(xmlTextReader);
            }
        }
        
        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="deserializedObject">The deserialized object.</param>
        /// <returns></returns>
        public static byte[] BinarySerialize(object deserializedObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //Serialize the object to the stream
                new BinaryFormatter().Serialize(stream, deserializedObject);

                //instantiate the byte[] to hold the serialized data
                byte[] bytes = new byte[stream.Length];

                //read the stream into the byte[]
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytes, 0, (int)stream.Length);

                //return the bytes
                return bytes;
            }
        }

        /// <summary>
        /// This will deserialize an object for the type <see cref="T"/>
        /// This method does not close/dispose of the stream, calling code must do this
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// An instance of the type <see cref="T"/>
        /// </returns>
        public static T BinaryDeserialize<T>(byte[] serializedObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //read the serialized object into the stream
                stream.Write(serializedObject, 0, serializedObject.Length);
                stream.Seek(0, SeekOrigin.Begin);

                //instantiate the BinaryFormatter and deserialize the object
                return (T)new BinaryFormatter().Deserialize(stream);
            }
        }

        #region Protobuf Serialization

        public static byte[] ProtobufSerialize<T>(T entity)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, entity);
                return stream.ToArray();
            }
        }

        public static T[] ProtobufDeserializeArray<T>(byte[][] byteArray)
        {
            var ret = new T[byteArray.Length];

            using (var stream = new MemoryStream())
            {
                int ii = 0;
                foreach (var bytes in byteArray)
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    ret[ii++] = Serializer.Deserialize<T>(stream);
                }
            }

            return ret;
        }

        public static T ProtobufDeserialize<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return Serializer.Deserialize<T>(stream);
            }
        }

        #endregion
    }

    /// <summary>
    /// This is a helper class used to serialize/deserialize objects using both binary and xml serialization
    /// </summary>
    /// <typeparam name="T">Object type to serialize</typeparam>
    public static class SerializationHelper<T>
    {
        #region Xml Serialization methods

        /// <summary>
        /// This will serialise the object. The output will NOT be indent formatted
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <returns>The serialized object</returns>
        public static T XmlClone(T obj)
        {
            return Deserialize(Serialize(obj, false, false));
        }

        /// <summary>
        /// This will serialise the object. The default namespace and 
        /// PI (including encoding) will be included
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <returns>The serialized object</returns>
        public static string Serialize(T obj)
        {
            return Serialize(obj, false, false);
        }

        /// <summary>
        /// This will serialise the object to a string and will include the namespaces 
        /// specified in the XmlSerializerNamespaces parameter in the serialized string
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="xmlSerializerNamespaces">The XML serializer namespaces.</param>
        /// <returns>The serialized object</returns>
        public static string Serialize(T obj, XmlSerializerNamespaces xmlSerializerNamespaces)
        {
            StringBuilder stringBuilder = new StringBuilder();
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)))
            {
                xmlSerializer.Serialize(xmlTextWriter, obj, xmlSerializerNamespaces);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// This will serialise the object. The output will NOT be indent formatted
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="removeProcessingInstruction">removes the XML processing instruction if set to true</param>
        /// <param name="removeNamespace">If true removes the default namespace</param>
        /// <returns>The serialized object</returns>
        public static string Serialize(T obj, bool removeProcessingInstruction, bool removeNamespace)
        {
            return Serialize(obj, removeProcessingInstruction, removeNamespace, false);
        }

        /// <summary>
        /// This will serialise the object. Allows control over namespace, PI and formatting
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="removeProcessingInstruction">removes the XML processing instruction if set to true</param>
        /// <param name="removeNamespace">If true removes the default namespace</param>
        /// <param name="formatted">If true the output string is indent formatted</param>
        /// <returns>The serialized object</returns>
        public static string Serialize(T obj, 
            bool removeProcessingInstruction, 
            bool removeNamespace,
            bool formatted)
        {
            StringBuilder stringBuilder = new StringBuilder();
            //GT: we use obj.GetType() rather than typeof(T) here as T may be the base class of obj.
            //Driver for this was XmlSerialisationInheritedTypesPackerUnpacker.
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)))
            {
                if (removeProcessingInstruction)
                {
                    xmlTextWriter.Formatting = Formatting.None;
                    xmlTextWriter.WriteRaw(string.Empty);
                }
                if (removeNamespace)
                {      
                    XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                    xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                    xmlSerializer.Serialize(xmlTextWriter, obj, xmlSerializerNamespaces);
                }
                else
                {
                    xmlSerializer.Serialize(xmlTextWriter, obj);
                }
            }

            if (formatted)
                return SerializationHelper.Format(stringBuilder.ToString());
            else
                return stringBuilder.ToString();
        }

        /// <summary>
        /// Serialises an object to a file, overwriting any existing file that may be there.
        /// </summary>
        /// <param name="obj">The object to serialise.</param>
        /// <param name="file">The filename (and location) to serialise to.</param>
        public static void SerializeToFile(T obj, string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// Deserialises an object from XML in a file.
        /// </summary>
        /// <param name="file">The file containing the XmlSerialised object.</param>
        /// <returns>The deserialised object.</returns>
        public static T DeserializeFromFile(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(fs);
            }
        }

        /// <summary>
        /// This will deserialize an object for the type <see cref="T"/>
        /// </summary>
        /// <param name="xml">The xml to deserialize</param>
        /// <returns>An instance of the type <see cref="T"/></returns>
        public static T Deserialize(string xml)
        {
            return Deserialize(xml, typeof (T));
        }

        /// <summary>
        /// Deserialises an object of the given type.
        /// </summary>
        /// <remarks>If we cannot cast to T we will get an exception.  The driver
        /// for this method is XmlSerialisationInheritedTypesPackerUnpacker.</remarks>
        /// <param name="xml">The xml to deserialise.</param>
        /// <param name="type">The type of the serialised object.</param>
        /// <returns>The deserialised object.</returns>
        public static T Deserialize(string xml, Type type)
        {
            T obj;

            using (XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(xml)))
            {
                obj = Deserialize(xmlTextReader, type);
            }

            return obj;
        }

        /// <summary>
        /// This will deserialize an object for the type <see cref="T"/>
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <returns></returns>
        public static T Deserialize(XmlReader xmlReader)
        {
            return Deserialize(xmlReader, typeof (T));
        }

        /// <summary>
        /// Deserialises an object of the given type.
        /// </summary>
        /// <remarks>If we cannot cast to T we will get an exception.  The driver
        /// for this method is XmlSerialisationInheritedTypesPackerUnpacker.</remarks>
        /// <param name="xmlReader">An XmlReader reading xml containing the serialised object.</param>
        /// <param name="type">The type of the serialised object.</param>
        /// <returns>The deserialised object.</returns>
        public static T Deserialize(XmlReader xmlReader, Type type)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            return (T)xmlSerializer.Deserialize(xmlReader);
        }

        #endregion

        #region Binary Serialization Methods

        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="deserializedObject">The deserialized object.</param>
        /// <returns></returns>
        public static byte[] BinarySerialize(T deserializedObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //Serialize the object to the stream
                new BinaryFormatter().Serialize(stream, deserializedObject);

                //instantiate the byte[] to hold the serialized data
                byte[] bytes = new byte[stream.Length];

                //read the stream into the byte[]
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytes, 0, (int)stream.Length);

                //return the bytes
                return bytes;
            }
        }

        /// <summary>
        /// This will deserialize an object for the type <see cref="T"/>
        /// This method does not close/dispose of the stream, calling code must do this
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// An instance of the type <see cref="T"/>
        /// </returns>
        public static T BinaryDeserialize(byte[] serializedObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //read the serialized object into the stream
                stream.Write(serializedObject, 0, serializedObject.Length);
                stream.Seek(0, SeekOrigin.Begin);

                //instantiate the BinaryFormatter and deserialize the object
                return (T) new BinaryFormatter().Deserialize(stream);
            }
        }

        /// <summary>
        /// Performs a deep copy of the object parameter using the BinaryFormatter
        /// </summary>
        /// <param name="deserializedObject">The deserialized object.</param>
        /// <returns></returns>
        public static T BinaryClone(T deserializedObject)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //instantiate the BinaryFormatter
                IFormatter formatter = new BinaryFormatter();

                //Serialize the object to the memory stream
                formatter.Serialize(stream, deserializedObject);

                //reset the stream position
                stream.Position = 0;

                //de-serialize the stream back to the original type
                return (T)formatter.Deserialize(stream);
            }
        }

        #endregion

        #region DataContract Serialization

        /// <summary>
        /// Serialize the object using the DataContractSerializer
        /// </summary>
        /// <param name="encoding">The encoding to use when serializing.</param>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns></returns>
        public static string DataContractSerialize(Encoding encoding, T entity)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, entity);
                return encoding.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Serialize the object using the DataContractSerializer, defaults to using UTF8 encoding
        /// </summary>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns></returns>
        public static string DataContractSerialize(T entity)
        {
            return DataContractSerialize(Encoding.UTF8, entity);
        }

        /// <summary>
        /// Deserialize the object using the DataContractSerializer
        /// </summary>
        /// <param name="encoding">The encoding to use when deserializing.</param>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T DataContractDeserialize(Encoding encoding, string entity)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(entity)))
            {
                return (T)serializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// Deserialize the object using the DataContractSerializer, defaults to using UTF8 encoding
        /// </summary>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T DataContractDeserialize(string entity)
        {
            return DataContractDeserialize(Encoding.UTF8, entity);
        }

        /// <summary>
        /// Perform a deep clone of the object using the DataContractSerializer, defaults to using UTF8 encoding
        /// </summary>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T DataContractClone(T entity)
        {
            return DataContractClone(Encoding.UTF8, entity);
        }

        /// <summary>
        /// Perform a deep clone of the object using the DataContractSerializer
        /// </summary>
        /// <param name="encoding">The encoding to use when deserializing.</param>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T DataContractClone(Encoding encoding, T entity)
        {
            return DataContractDeserialize(encoding, DataContractSerialize(encoding, entity));
        }

        #endregion

        #region NetDataContract Serialization

        /// <summary>
        /// Serialize the object using the DataContractSerializer
        /// </summary>
        /// <param name="encoding">The encoding to use when serializing.</param>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns></returns>
        public static string NetDataContractSerialize(Encoding encoding, T entity)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, entity);
                return encoding.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Serialize the object using the DataContractSerializer, defaults to using UTF8 encoding
        /// </summary>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns></returns>
        public static string NetDataContractSerialize(T entity)
        {
            return NetDataContractSerialize(Encoding.UTF8, entity);
        }

        /// <summary>
        /// Deserialize the object using the DataContractSerializer
        /// </summary>
        /// <param name="encoding">The encoding to use when deserializing.</param>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T NetDataContractDeserialize(Encoding encoding, string entity)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();

            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(entity)))
            {
                return (T)serializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// Deserialize the object using the DataContractSerializer, defaults to using UTF8 encoding
        /// </summary>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T NetDataContractDeserialize(string entity)
        {
            return NetDataContractDeserialize(Encoding.UTF8, entity);
        }

        /// <summary>
        /// Perform a deep clone of the object using the DataContractSerializer, defaults to using UTF8 encoding
        /// </summary>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T NetDataContractClone(T entity)
        {
            return NetDataContractClone(Encoding.UTF8, entity);
        }

        /// <summary>
        /// Perform a deep clone of the object using the DataContractSerializer
        /// </summary>
        /// <param name="encoding">The encoding to use when deserializing.</param>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T NetDataContractClone(Encoding encoding, T entity)
        {
            return NetDataContractDeserialize(encoding, NetDataContractSerialize(encoding, entity));
        }

        #endregion

        #region Data Contract JSON Serialziation

        /// <summary>
        /// Serialize the object using the DataContractSerializer
        /// This is a test
        /// </summary>
        /// <param name="encoding">The encoding to use when serializing.</param>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns></returns>
        public static string DataContractJsonSerialize(Encoding encoding, T entity)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, entity);
                return encoding.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Serialize the object using the DataContractSerializer, defaults to using UTF8 encoding
        /// </summary>
        /// <param name="entity">The entity to serialize.</param>
        /// <returns></returns>
        public static string DataContractJsonSerialize(T entity)
        {
            return DataContractJsonSerialize(Encoding.UTF8, entity);
        }

        /// <summary>
        /// Deserialize the object using the DataContractSerializer
        /// </summary>
        /// <param name="encoding">The encoding to use when deserializing.</param>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T DataContractJsonDeserialize(Encoding encoding, string entity)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(entity)))
            {
                return (T)serializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// Deserialize the object using the DataContractSerializer, defaults to using UTF8 encoding
        /// </summary>
        /// <param name="entity">The entity to deserialize.</param>
        /// <returns></returns>
        public static T DataContractJsonDeserialize(string entity)
        {
            return DataContractDeserialize(Encoding.UTF8, entity);
        }

        #endregion
    }
}