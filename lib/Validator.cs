using Saxon.Api;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace LantanaGroup.XmlHarvester
{
    public class Validator
    {
        private string schemaPath;
        private string schematronPath;
        private Processor processor = new Processor();
        private XmlReaderSettings readerSettings;
        private DocumentBuilder builder;
        private string phase1;

        public bool WillValidate
        {
            get
            {
                return !string.IsNullOrEmpty(schemaPath) || !string.IsNullOrEmpty(schematronPath);
            }
        }

        public bool WillValidateSchema
        {
            get
            {
                return !string.IsNullOrEmpty(schemaPath);
            }
        }

        public bool WillValidateSchematron
        {
            get
            {
                return !string.IsNullOrEmpty(schematronPath);
            }
        }

        public Validator(string schemaPath, string schematronPath)
        {
            this.schemaPath = schemaPath;
            this.schematronPath = schematronPath;

            if (!String.IsNullOrEmpty(this.schemaPath))
            {
                XmlDocument xsdDoc = new XmlDocument();
                xsdDoc.Load(this.schemaPath);

                if (xsdDoc.DocumentElement != null && xsdDoc.DocumentElement.Attributes != null && xsdDoc.DocumentElement.Attributes.GetNamedItem("targetNamespace") != null)
                {
                    XmlAttribute targetNamespace = (XmlAttribute)xsdDoc.DocumentElement.Attributes.GetNamedItem("targetNamespace");

                    readerSettings = new XmlReaderSettings();
                    readerSettings.Schemas.Add(targetNamespace.Value, this.schemaPath);
                    readerSettings.ValidationType = ValidationType.Schema;
                }
            }

            if (!string.IsNullOrEmpty(this.schematronPath))
            {
                processor.XmlResolver = new ValidatorResolver(new FileInfo(schematronPath).DirectoryName);
                builder = processor.NewDocumentBuilder();
                builder.BaseUri = new Uri("file://");
            }
        }

        public bool ValidateSchema(string filePath)
        {
            bool isValid = true;

            // Validate XSD
            if (readerSettings != null)
            {
                readerSettings.ValidationEventHandler += delegate (object sender, ValidationEventArgs e)
                {
                    if (e.Severity == XmlSeverityType.Error) isValid = false;
                };
                XmlReader books = XmlReader.Create(filePath, readerSettings);

                while (books.Read()) { }
            }

            return isValid;
        }

        private string Transform(string stylesheetContent, string xmlPath, bool addErrorsPhase = false)
        {
            FileInfo xmlInfo = new FileInfo(xmlPath);
            XdmNode stylesheet = builder.Build(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(stylesheetContent))));

            XsltCompiler compiler = processor.NewXsltCompiler();
            XsltExecutable exec = compiler.Compile(stylesheet);

            XsltTransformer transformer = exec.Load();
            DomDestination dest = new DomDestination();

            using (var inputStream = xmlInfo.OpenRead())
            {
                if (addErrorsPhase)
                    transformer.SetParameter(new QName("phase"), XdmValue.MakeValue("errors"));

                transformer.SetInputStream(inputStream, new Uri(xmlInfo.DirectoryName));
                transformer.Run(dest);
            }

            using (StringWriter sw = new StringWriter())
            {
                if (dest.XmlDocument == null)
                    throw new Exception("Failed to execute Schematron validation. The SCH file selected may not be the ISO version of Schematron.");

                dest.XmlDocument.Save(sw);
                return sw.ToString();
            }
        }

        public bool ValidateSchematron(string filePath)
        {
            if (string.IsNullOrEmpty(schematronPath)) return true;

            if (string.IsNullOrEmpty(phase1))
            {
                string schSkeletonContent;

                using (Stream schSkeletonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LantanaGroup.XmlHarvester.iso-sch-conformance1-5.xsl"))
                {
                    using (StreamReader schSkeletonReader = new StreamReader(schSkeletonStream))
                    {
                        schSkeletonContent = schSkeletonReader.ReadToEnd();
                    }
                }

                phase1 = Transform(schSkeletonContent, schematronPath, true);
            }

            string phase2 = Transform(phase1, filePath);

            XdmNode phase2Results = builder.Build(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(phase2))));

            XPathCompiler xpathCompiler = processor.NewXPathCompiler();
            XPathExecutable xpathExec = xpathCompiler.Compile("//failed-assert");
            XPathSelector xpathSelector = xpathExec.Load();
            xpathSelector.ContextItem = phase2Results;
            var xpathResults = xpathSelector.Evaluate();

            return xpathResults.Count == 0;
        }
    }
}
