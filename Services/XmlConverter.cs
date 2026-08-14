using System;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using BetriebsmittelPublisher.Models;

namespace BetriebsmittelPublisher.Services
{
    public class ExecutionXmlData
    {
        public string PgNumber { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string MotorNumber { get; set; } = string.Empty;
        public string Modul { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string Quitk { get; set; } = "R";
        public string Tv { get; set; } = "37191";
        public string Ma { get; set; } = "0004808061";
        public string Bauart { get; set; } = "2013";
        public string ToolPosition { get; set; } = "1";
        public string ConnectTimeout { get; set; } = "10000";
        public string Dmc { get; set; } = "";
    }

    public class XmlConverter
    {
        public string GenerateExecutionXml(ExecutionXmlData data)
        {
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
                IndentChars = "    ",
                NewLineChars = "\n",
                OmitXmlDeclaration = false,
                WriteEndDocumentOnClose = true
            };

            var sb = new StringBuilder();
            using (var stringWriter = new Utf8StringWriter(sb))
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                xmlWriter.WriteStartDocument(standalone: true);

                xmlWriter.WriteStartElement("execution", "TExecution", "http://www.de-gmbh.com/workDesc/data/execution");

                WriteNamespace(xmlWriter, "mappingArtice", "http://www.de-gmbh.com/mappingArticle");
                WriteNamespace(xmlWriter, "mail", "http://www.de-gmbh.com/mail");
                WriteNamespace(xmlWriter, "table", "http://www.de-gmbh.com/table");
                WriteNamespace(xmlWriter, "layout", "http://www.de-gmbh.com/layout");
                WriteNamespace(xmlWriter, "parameterdefinition", "http://www.de-gmbh.com/parameterdefinition");
                WriteNamespace(xmlWriter, "pickHardware", "http://www.de-gmbh.com/pickHardware");
                WriteNamespace(xmlWriter, "balance", "http://www.de-gmbh.com/balance");
                WriteNamespace(xmlWriter, "camera", "http://www.de-gmbh.com/camera");
                WriteNamespace(xmlWriter, "ns9", "http://www.de-gmbh.com/markup");
                WriteNamespace(xmlWriter, "testrigxml", "http://www.de-gmbh.com/testrigxml");
                WriteNamespace(xmlWriter, "ns11", "http://www.de-gmbh.com/sound");
                WriteNamespace(xmlWriter, "reports", "http://www.de-gmbh.com/reports");
                WriteNamespace(xmlWriter, "scanner", "http://www.de-gmbh.com/scanner");
                WriteNamespace(xmlWriter, "rfid", "http://www.de-gmbh.com/rfid");
                WriteNamespace(xmlWriter, "ns15", "http://www.de-gmbh.com/browser");
                WriteNamespace(xmlWriter, "laser", "http://www.de-gmbh.com/laser");
                WriteNamespace(xmlWriter, "bde", "http://www.de-gmbh.com/bde");
                WriteNamespace(xmlWriter, "ns18", "http://www.de-gmbh.com/pickHardwareLaser");
                WriteNamespace(xmlWriter, "mappingAddress", "http://www.de-gmbh.com/mappingAddress");
                WriteNamespace(xmlWriter, "config", "http://www.de-gmbh.com/configuration");
                WriteNamespace(xmlWriter, "library", "http://www.de-gmbh.com/library");
                WriteNamespace(xmlWriter, "StateEngineConf", "http://www.de-gmbh.com/StateEngineConfig-1.0.0");
                WriteNamespace(xmlWriter, "dbConfig", "http://www.de-gmbh.com/DEDatabaseConfig");
                WriteNamespace(xmlWriter, "classpath", "http://www.de-gmbh.com/classpath");
                WriteNamespace(xmlWriter, "execution", "http://www.de-gmbh.com/workDesc/data/execution");
                WriteNamespace(xmlWriter, "module", "http://www.de-gmbh.com/modules");

                var ns = "http://www.de-gmbh.com/workDesc/data/execution";

                xmlWriter.WriteStartElement("tasks", ns);

                xmlWriter.WriteStartElement("task", ns);
                xmlWriter.WriteAttributeString("id", Guid.NewGuid().ToString());
                xmlWriter.WriteAttributeString("modul", data.Modul);
                xmlWriter.WriteAttributeString("toolPosition", data.ToolPosition);
                xmlWriter.WriteAttributeString("feature", data.PgNumber);

                WriteParameter(xmlWriter, ns, "QUITK", data.Quitk);
                WriteParameter(xmlWriter, ns, "requestTopic", data.Topic);
                WriteParameter(xmlWriter, ns, "TV", data.Tv);
                WriteParameter(xmlWriter, ns, "MA", data.Ma);
                WriteParameter(xmlWriter, ns, "bauart", data.Bauart);
                WriteParameter(xmlWriter, ns, "port", data.Port);
                WriteParameter(xmlWriter, ns, "host", data.Host);
                WriteParameter(xmlWriter, ns, "connectTimeout", data.ConnectTimeout);
                WriteParameter(xmlWriter, ns, "responseTopic", data.Topic);
                WriteParameter(xmlWriter, ns, "DMC", data.Dmc);
                WriteParameter(xmlWriter, ns, "motorNr", data.MotorNumber);

                xmlWriter.WriteEndElement(); // task
                xmlWriter.WriteEndElement(); // tasks
                xmlWriter.WriteEndElement(); // TExecution
                xmlWriter.WriteEndDocument();
            }

            return sb.ToString();
        }

        private static void WriteNamespace(XmlWriter writer, string prefix, string uri)
        {
            writer.WriteAttributeString("xmlns", prefix, null, uri);
        }

        private static void WriteParameter(XmlWriter writer, string ns, string name, string value)
        {
            writer.WriteStartElement("parameter", ns);
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("value", value ?? string.Empty);
            writer.WriteEndElement();
        }

        public bool ValidateXml(string xml, out string? error)
        {
            error = null;

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                if (!xml.StartsWith("<?xml version="))
                {
                    error = "XML-Deklaration fehlt am Anfang";
                    return false;
                }

                var nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("execution", "http://www.de-gmbh.com/workDesc/data/execution");

                var taskNode = doc.SelectSingleNode("//execution:task", nsManager);
                if (taskNode == null)
                {
                    error = "Kein execution:task Element gefunden";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"XML validation failed: {ex.Message}";
                return false;
            }
        }

        private class Utf8StringWriter : System.IO.StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }
            public override Encoding Encoding => new UTF8Encoding(false);
        }
    }
}