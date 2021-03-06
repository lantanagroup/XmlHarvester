// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code++. Version 4.2.0.72
//    <NameSpace>LantanaGroup.XmlHarvester</NameSpace><Collection>List</Collection><codeType>CSharp</codeType><EnableDataBinding>False</EnableDataBinding><GenerateCloneMethod>False</GenerateCloneMethod><GenerateDataContracts>False</GenerateDataContracts><DataMemberNameArg>OnlyIfDifferent</DataMemberNameArg><DataMemberOnXmlIgnore>False</DataMemberOnXmlIgnore><CodeBaseTag>Net20</CodeBaseTag><InitializeFields>All</InitializeFields><GenerateUnusedComplexTypes>False</GenerateUnusedComplexTypes><GenerateUnusedSimpleTypes>False</GenerateUnusedSimpleTypes><GenerateXMLAttributes>True</GenerateXMLAttributes><OrderXMLAttrib>False</OrderXMLAttrib><EnableLazyLoading>False</EnableLazyLoading><VirtualProp>False</VirtualProp><PascalCase>True</PascalCase><AutomaticProperties>True</AutomaticProperties><PropNameSpecified>None</PropNameSpecified><PrivateFieldName>StartWithUnderscore</PrivateFieldName><PrivateFieldNamePrefix></PrivateFieldNamePrefix><EnableRestriction>False</EnableRestriction><RestrictionMaxLenght>False</RestrictionMaxLenght><RestrictionRegEx>False</RestrictionRegEx><RestrictionRange>False</RestrictionRange><ValidateProperty>False</ValidateProperty><ClassNamePrefix>Mapping</ClassNamePrefix><ClassLevel>Public</ClassLevel><PartialClass>True</PartialClass><ClassesInSeparateFiles>False</ClassesInSeparateFiles><ClassesInSeparateFilesDir></ClassesInSeparateFilesDir><TrackingChangesEnable>False</TrackingChangesEnable><GenTrackingClasses>False</GenTrackingClasses><HidePrivateFieldInIDE>False</HidePrivateFieldInIDE><EnableSummaryComment>False</EnableSummaryComment><EnableAppInfoSettings>False</EnableAppInfoSettings><EnableExternalSchemasCache>False</EnableExternalSchemasCache><EnableDebug>False</EnableDebug><EnableWarn>False</EnableWarn><ExcludeImportedTypes>False</ExcludeImportedTypes><ExpandNesteadAttributeGroup>False</ExpandNesteadAttributeGroup><CleanupCode>False</CleanupCode><EnableXmlSerialization>True</EnableXmlSerialization><SerializeMethodName>Serialize</SerializeMethodName><DeserializeMethodName>Deserialize</DeserializeMethodName><SaveToFileMethodName>SaveToFile</SaveToFileMethodName><LoadFromFileMethodName>LoadFromFile</LoadFromFileMethodName><EnableEncoding>False</EnableEncoding><EnableXMLIndent>False</EnableXMLIndent><IndentChar>Indent2Space</IndentChar><NewLineAttr>False</NewLineAttr><OmitXML>False</OmitXML><Encoder>UTF8</Encoder><Serializer>XmlSerializer</Serializer><sspNullable>False</sspNullable><sspString>False</sspString><sspCollection>False</sspCollection><sspComplexType>False</sspComplexType><sspSimpleType>False</sspSimpleType><sspEnumType>False</sspEnumType><XmlSerializerEvent>False</XmlSerializerEvent><BaseClassName>EntityBase</BaseClassName><UseBaseClass>False</UseBaseClass><GenBaseClass>False</GenBaseClass><CustomUsings></CustomUsings><AttributesToExlude></AttributesToExlude>
//  </auto-generated>
// ------------------------------------------------------------------------------
#pragma warning disable
namespace LantanaGroup.XmlHarvester
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;


    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, TypeName = "config")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "config")]
    public partial class MappingConfig
    {

        private List<MappingNamespace> _namespace;

        private List<MappingColumn> _column;

        private List<MappingGroup> _group;

        private string _tableName;

        private static XmlSerializer serializer;

        public MappingConfig()
        {
            this._group = new List<MappingGroup>();
            this._column = new List<MappingColumn>();
            this._namespace = new List<MappingNamespace>();
        }

        [System.Xml.Serialization.XmlElementAttribute("namespace", ElementName = "namespace")]
        public List<MappingNamespace> Namespace
        {
            get
            {
                return this._namespace;
            }
            set
            {
                this._namespace = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("column", ElementName = "column")]
        public List<MappingColumn> Column
        {
            get
            {
                return this._column;
            }
            set
            {
                this._column = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("group", ElementName = "group")]
        public List<MappingGroup> Group
        {
            get
            {
                return this._group;
            }
            set
            {
                this._group = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "tableName")]
        public string TableName
        {
            get
            {
                return this._tableName;
            }
            set
            {
                this._tableName = value;
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if ((serializer == null))
                {
                    serializer = new XmlSerializerFactory().CreateSerializer(typeof(MappingConfig));
                }
                return serializer;
            }
        }

        #region Serialize/Deserialize
        /// <summary>
        /// Serializes current MappingConfig object into an XML string
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            System.IO.StreamReader streamReader = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                memoryStream = new System.IO.MemoryStream();
                System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
                System.Xml.XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
                Serializer.Serialize(xmlWriter, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new System.IO.StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                if ((streamReader != null))
                {
                    streamReader.Dispose();
                }
                if ((memoryStream != null))
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes workflow markup into an MappingConfig object
        /// </summary>
        /// <param name="input">string workflow markup to deserialize</param>
        /// <param name="obj">Output MappingConfig object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool Deserialize(string input, out MappingConfig obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MappingConfig);
            try
            {
                obj = Deserialize(input);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool Deserialize(string input, out MappingConfig obj)
        {
            System.Exception exception = null;
            return Deserialize(input, out obj, out exception);
        }

        public static MappingConfig Deserialize(string input)
        {
            System.IO.StringReader stringReader = null;
            try
            {
                stringReader = new System.IO.StringReader(input);
                return ((MappingConfig)(Serializer.Deserialize(XmlReader.Create(stringReader))));
            }
            finally
            {
                if ((stringReader != null))
                {
                    stringReader.Dispose();
                }
            }
        }

        public static MappingConfig Deserialize(System.IO.Stream s)
        {
            return ((MappingConfig)(Serializer.Deserialize(s)));
        }
        #endregion

        /// <summary>
        /// Serializes current MappingConfig object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        public virtual bool SaveToFile(string fileName, out System.Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (System.Exception e)
            {
                exception = e;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            System.IO.StreamWriter streamWriter = null;
            try
            {
                string xmlString = Serialize();
                System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
                streamWriter = xmlFile.CreateText();
                streamWriter.WriteLine(xmlString);
                streamWriter.Close();
            }
            finally
            {
                if ((streamWriter != null))
                {
                    streamWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes xml markup from file into an MappingConfig object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output MappingConfig object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool LoadFromFile(string fileName, out MappingConfig obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MappingConfig);
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out MappingConfig obj)
        {
            System.Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static MappingConfig LoadFromFile(string fileName)
        {
            System.IO.FileStream file = null;
            System.IO.StreamReader sr = null;
            try
            {
                file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new System.IO.StreamReader(file);
                string xmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();
                return Deserialize(xmlString);
            }
            finally
            {
                if ((file != null))
                {
                    file.Dispose();
                }
                if ((sr != null))
                {
                    sr.Dispose();
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, TypeName = "namespace")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "namespace")]
    public partial class MappingNamespace
    {

        private string _prefix;

        private string _uri;

        private static XmlSerializer serializer;

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "prefix")]
        public string Prefix
        {
            get
            {
                return this._prefix;
            }
            set
            {
                this._prefix = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "uri")]
        public string Uri
        {
            get
            {
                return this._uri;
            }
            set
            {
                this._uri = value;
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if ((serializer == null))
                {
                    serializer = new XmlSerializerFactory().CreateSerializer(typeof(MappingNamespace));
                }
                return serializer;
            }
        }

        #region Serialize/Deserialize
        /// <summary>
        /// Serializes current MappingNamespace object into an XML string
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            System.IO.StreamReader streamReader = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                memoryStream = new System.IO.MemoryStream();
                System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
                System.Xml.XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
                Serializer.Serialize(xmlWriter, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new System.IO.StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                if ((streamReader != null))
                {
                    streamReader.Dispose();
                }
                if ((memoryStream != null))
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes workflow markup into an MappingNamespace object
        /// </summary>
        /// <param name="input">string workflow markup to deserialize</param>
        /// <param name="obj">Output MappingNamespace object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool Deserialize(string input, out MappingNamespace obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MappingNamespace);
            try
            {
                obj = Deserialize(input);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool Deserialize(string input, out MappingNamespace obj)
        {
            System.Exception exception = null;
            return Deserialize(input, out obj, out exception);
        }

        public static MappingNamespace Deserialize(string input)
        {
            System.IO.StringReader stringReader = null;
            try
            {
                stringReader = new System.IO.StringReader(input);
                return ((MappingNamespace)(Serializer.Deserialize(XmlReader.Create(stringReader))));
            }
            finally
            {
                if ((stringReader != null))
                {
                    stringReader.Dispose();
                }
            }
        }

        public static MappingNamespace Deserialize(System.IO.Stream s)
        {
            return ((MappingNamespace)(Serializer.Deserialize(s)));
        }
        #endregion

        /// <summary>
        /// Serializes current MappingNamespace object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        public virtual bool SaveToFile(string fileName, out System.Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (System.Exception e)
            {
                exception = e;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            System.IO.StreamWriter streamWriter = null;
            try
            {
                string xmlString = Serialize();
                System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
                streamWriter = xmlFile.CreateText();
                streamWriter.WriteLine(xmlString);
                streamWriter.Close();
            }
            finally
            {
                if ((streamWriter != null))
                {
                    streamWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes xml markup from file into an MappingNamespace object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output MappingNamespace object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool LoadFromFile(string fileName, out MappingNamespace obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MappingNamespace);
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out MappingNamespace obj)
        {
            System.Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static MappingNamespace LoadFromFile(string fileName)
        {
            System.IO.FileStream file = null;
            System.IO.StreamReader sr = null;
            try
            {
                file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new System.IO.StreamReader(file);
                string xmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();
                return Deserialize(xmlString);
            }
            finally
            {
                if ((file != null))
                {
                    file.Dispose();
                }
                if ((sr != null))
                {
                    sr.Dispose();
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, TypeName = "column")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "column")]
    public partial class MappingColumn
    {

        private string _name;

        private bool _isNarrative;

        private string _heading;

        private string _value;

        private static XmlSerializer serializer;

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "name")]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "isNarrative")]
        public bool IsNarrative
        {
            get
            {
                return this._isNarrative;
            }
            set
            {
                this._isNarrative = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "heading")]
        public string Heading
        {
            get
            {
                return this._heading;
            }
            set
            {
                this._heading = value;
            }
        }

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if ((serializer == null))
                {
                    serializer = new XmlSerializerFactory().CreateSerializer(typeof(MappingColumn));
                }
                return serializer;
            }
        }

        #region Serialize/Deserialize
        /// <summary>
        /// Serializes current MappingColumn object into an XML string
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            System.IO.StreamReader streamReader = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                memoryStream = new System.IO.MemoryStream();
                System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
                System.Xml.XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
                Serializer.Serialize(xmlWriter, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new System.IO.StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                if ((streamReader != null))
                {
                    streamReader.Dispose();
                }
                if ((memoryStream != null))
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes workflow markup into an MappingColumn object
        /// </summary>
        /// <param name="input">string workflow markup to deserialize</param>
        /// <param name="obj">Output MappingColumn object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool Deserialize(string input, out MappingColumn obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MappingColumn);
            try
            {
                obj = Deserialize(input);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool Deserialize(string input, out MappingColumn obj)
        {
            System.Exception exception = null;
            return Deserialize(input, out obj, out exception);
        }

        public static MappingColumn Deserialize(string input)
        {
            System.IO.StringReader stringReader = null;
            try
            {
                stringReader = new System.IO.StringReader(input);
                return ((MappingColumn)(Serializer.Deserialize(XmlReader.Create(stringReader))));
            }
            finally
            {
                if ((stringReader != null))
                {
                    stringReader.Dispose();
                }
            }
        }

        public static MappingColumn Deserialize(System.IO.Stream s)
        {
            return ((MappingColumn)(Serializer.Deserialize(s)));
        }
        #endregion

        /// <summary>
        /// Serializes current MappingColumn object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        public virtual bool SaveToFile(string fileName, out System.Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (System.Exception e)
            {
                exception = e;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            System.IO.StreamWriter streamWriter = null;
            try
            {
                string xmlString = Serialize();
                System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
                streamWriter = xmlFile.CreateText();
                streamWriter.WriteLine(xmlString);
                streamWriter.Close();
            }
            finally
            {
                if ((streamWriter != null))
                {
                    streamWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes xml markup from file into an MappingColumn object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output MappingColumn object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool LoadFromFile(string fileName, out MappingColumn obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MappingColumn);
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out MappingColumn obj)
        {
            System.Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static MappingColumn LoadFromFile(string fileName)
        {
            System.IO.FileStream file = null;
            System.IO.StreamReader sr = null;
            try
            {
                file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new System.IO.StreamReader(file);
                string xmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();
                return Deserialize(xmlString);
            }
            finally
            {
                if ((file != null))
                {
                    file.Dispose();
                }
                if ((sr != null))
                {
                    sr.Dispose();
                }
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, TypeName = "group")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "group")]
    public partial class MappingGroup
    {

        private List<MappingColumn> _column;

        private List<MappingGroup> _group1;

        private string _tableName;

        private string _columnPrefix;

        private string _context;

        private static XmlSerializer serializer;

        public MappingGroup()
        {
            this._group1 = new List<MappingGroup>();
            this._column = new List<MappingColumn>();
        }

        [System.Xml.Serialization.XmlElementAttribute("column", ElementName = "column")]
        public List<MappingColumn> Column
        {
            get
            {
                return this._column;
            }
            set
            {
                this._column = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("group", ElementName = "group")]
        public List<MappingGroup> Group
        {
            get
            {
                return this._group1;
            }
            set
            {
                this._group1 = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "tableName")]
        public string TableName
        {
            get
            {
                return this._tableName;
            }
            set
            {
                this._tableName = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "columnPrefix")]
        public string ColumnPrefix
        {
            get
            {
                return this._columnPrefix;
            }
            set
            {
                this._columnPrefix = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "context")]
        public string Context
        {
            get
            {
                return this._context;
            }
            set
            {
                this._context = value;
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if ((serializer == null))
                {
                    serializer = new XmlSerializerFactory().CreateSerializer(typeof(MappingGroup));
                }
                return serializer;
            }
        }

        #region Serialize/Deserialize
        /// <summary>
        /// Serializes current MappingGroup object into an XML string
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            System.IO.StreamReader streamReader = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                memoryStream = new System.IO.MemoryStream();
                System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
                System.Xml.XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
                Serializer.Serialize(xmlWriter, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new System.IO.StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                if ((streamReader != null))
                {
                    streamReader.Dispose();
                }
                if ((memoryStream != null))
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes workflow markup into an MappingGroup object
        /// </summary>
        /// <param name="input">string workflow markup to deserialize</param>
        /// <param name="obj">Output MappingGroup object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool Deserialize(string input, out MappingGroup obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MappingGroup);
            try
            {
                obj = Deserialize(input);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool Deserialize(string input, out MappingGroup obj)
        {
            System.Exception exception = null;
            return Deserialize(input, out obj, out exception);
        }

        public static MappingGroup Deserialize(string input)
        {
            System.IO.StringReader stringReader = null;
            try
            {
                stringReader = new System.IO.StringReader(input);
                return ((MappingGroup)(Serializer.Deserialize(XmlReader.Create(stringReader))));
            }
            finally
            {
                if ((stringReader != null))
                {
                    stringReader.Dispose();
                }
            }
        }

        public static MappingGroup Deserialize(System.IO.Stream s)
        {
            return ((MappingGroup)(Serializer.Deserialize(s)));
        }
        #endregion

        /// <summary>
        /// Serializes current MappingGroup object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        public virtual bool SaveToFile(string fileName, out System.Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (System.Exception e)
            {
                exception = e;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            System.IO.StreamWriter streamWriter = null;
            try
            {
                string xmlString = Serialize();
                System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
                streamWriter = xmlFile.CreateText();
                streamWriter.WriteLine(xmlString);
                streamWriter.Close();
            }
            finally
            {
                if ((streamWriter != null))
                {
                    streamWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes xml markup from file into an MappingGroup object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output MappingGroup object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool LoadFromFile(string fileName, out MappingGroup obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MappingGroup);
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out MappingGroup obj)
        {
            System.Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static MappingGroup LoadFromFile(string fileName)
        {
            System.IO.FileStream file = null;
            System.IO.StreamReader sr = null;
            try
            {
                file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new System.IO.StreamReader(file);
                string xmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();
                return Deserialize(xmlString);
            }
            finally
            {
                if ((file != null))
                {
                    file.Dispose();
                }
                if ((sr != null))
                {
                    sr.Dispose();
                }
            }
        }
    }
}
#pragma warning restore
