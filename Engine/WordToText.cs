
using System;
using System.IO;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;  
using Serilog;
using DataMinerAPI.Models;

namespace DataMinerAPI.Engine
{
    public class WordToText
	{
        public ResponseEntity ConvertWordToText(string conversionSource, Guid requestGuid)
        {
            ResponseEntity era = new ResponseEntity();
            era.RequestID = requestGuid;

            try
            {               

                const string wordmlNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

                StringBuilder sb = new StringBuilder();
                using (WordprocessingDocument wdDoc = WordprocessingDocument.Open(conversionSource, false))
                {
 
                    NameTable nt = new NameTable();
                    XmlNamespaceManager nsManager = new XmlNamespaceManager(nt);

                    nsManager.AddNamespace("w", wordmlNamespace);

                    XmlDocument xdoc = new XmlDocument(nt);
                    xdoc.Load(wdDoc.MainDocumentPart.GetStream());

                    XmlNodeList paragraphNodes = xdoc.SelectNodes("//w:p", nsManager);

                    foreach (XmlNode paragraphNode in paragraphNodes)
                    {
                        XmlNodeList textNodes = paragraphNode.SelectNodes(".//w:t", nsManager);
                        foreach (System.Xml.XmlNode textNode in textNodes)
                        {
                            sb.Append(textNode.InnerText);
                        }
                        sb.Append(Environment.NewLine);
                    }   
                }

                string textFileName = Path.ChangeExtension(conversionSource, ".txt");

                string textResults = sb.ToString();
                
                File.WriteAllText($"{textFileName}", textResults);

                era.DocumentContent = textResults;
				era.Success = true;
				era.Message = "Conversion ok";
            }
            catch(Exception ex)
            {
                Log.Error(ex, "In ConvertWordToText");
				era.Success = false;
				era.Message = "Conversion failed";
				era.DocumentContent = ex.Message;
            }

            return era;
        }
    }
}