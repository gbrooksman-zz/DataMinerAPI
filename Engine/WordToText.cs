using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;  
using DocumentFormat.OpenXml.Wordprocessing;  


namespace DataMinerAPI.Engine
{
    public class WordToText
	{
        public bool ConvertWordToText(string fileName, Guid requestGuid)
        {
            string inputDir = @"files/";
			string workingDir = @"work/";

            string fileExtension = System.IO.Path.GetExtension(fileName);

			string conversionTarget = $"{workingDir}{requestGuid}{fileExtension}";

			File.Copy($"{inputDir}{fileName}", conversionTarget);	

            const string wordmlNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

            StringBuilder textBuilder = new StringBuilder();
            using (WordprocessingDocument wdDoc = WordprocessingDocument.Open(conversionTarget, false))
            {
                // Manage namespaces to perform XPath queries.  
                NameTable nt = new NameTable();
                XmlNamespaceManager nsManager = new XmlNamespaceManager(nt);
                nsManager.AddNamespace("w", wordmlNamespace);

                // Get the document part from the package.  
                // Load the XML in the document part into an XmlDocument instance.  
                XmlDocument xdoc = new XmlDocument(nt);
                xdoc.Load(wdDoc.MainDocumentPart.GetStream());

                XmlNodeList paragraphNodes = xdoc.SelectNodes("//w:p", nsManager);
                foreach (XmlNode paragraphNode in paragraphNodes)
                {
                    XmlNodeList textNodes = paragraphNode.SelectNodes(".//w:t", nsManager);
                    foreach (System.Xml.XmlNode textNode in textNodes)
                    {
                        textBuilder.Append(textNode.InnerText);
                    }
                    textBuilder.Append(Environment.NewLine);
                }

            }

            string textFileName = conversionTarget.Replace(".docx", ".txt");

            File.WriteAllText($"{textFileName}", textBuilder.ToString());

            return true;
        }
    }
}