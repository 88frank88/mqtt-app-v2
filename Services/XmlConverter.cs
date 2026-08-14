using System;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using BetriebsmittelPublisher.Models;

namespace BetriebsmittelPublisher.Services
{
    public class XmlConverter
    {
        public string GenerateXml(List<PgTableRow> tableRows)
        {
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false
            };

            using (var stringWriter = new System.IO.StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Betriebsmittel");
                
                xmlWriter.WriteStartElement("PG-Numbers");
                
                foreach (var row in tableRows)
                {
                    if (!string.IsNullOrEmpty(row.PgNumber))
                    {
                        xmlWriter.WriteStartElement("PG");
                        
                        xmlWriter.WriteElementString("MotorNumber", row.MotorNumber);
                        xmlWriter.WriteElementString("PgNumber", row.PgNumber);
                        xmlWriter.WriteElementString("Timestamp", row.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                        xmlWriter.WriteElementString("Status", row.Status);
                        
                        if (!string.IsNullOrEmpty(row.Error))
                        {
                            xmlWriter.WriteElementString("Error", row.Error);
                        }
                        
                        xmlWriter.WriteEndElement();
                    }
                }
                
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                
                return stringWriter.ToString();
            }
        }

        public bool ValidateXml(string xml, out string? error)
        {
            error = null;
            
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                
                var rootNode = doc.SelectSingleNode("Betriebsmittel");
                if (rootNode == null)
                {
                    error = "Missing root element: Betriebsmittel";
                    return false;
                }
                
                var pgNumbersNode = rootNode.SelectSingleNode("PG-Numbers");
                if (pgNumbersNode == null)
                {
                    error = "Missing PG-Numbers element";
                    return false;
                }
                
                var pgNodes = pgNumbersNode.SelectNodes("PG");
                if (pgNodes == null || pgNodes.Count == 0)
                {
                    error = "No PG elements found";
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
    }
}