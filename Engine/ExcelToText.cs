using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;  
using DocumentFormat.OpenXml.Spreadsheet;  
using System.Linq;


namespace DataMinerAPI.Engine
{
    public class ExcelToText
	{
        public  string ConvertExcelToText(string fileName)
        {
            string ret = string.Empty;

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fs, false))
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    SharedStringTable sst = sstpart.SharedStringTable;

                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    Worksheet sheet = worksheetPart.Worksheet;

                   // var cells = sheet.Descendants<Cell>();
                    var rows = sheet.Descendants<Row>();

                    // Or... via each row
                    foreach (Row row in rows)
                    {
                        foreach (Cell c in row.Elements<Cell>())
                        {
                            if ((c.DataType != null) && (c.DataType ==  CellValues.SharedString))
                            {
                                int ssid = int.Parse(c.CellValue.Text);
                                string str = sst.ChildElements[ssid].InnerText;
                                Console.WriteLine("Shared string {0}: {1}", ssid, str);
                            }
                            else if (c.CellValue != null)
                            {
                                Console.WriteLine("Cell contents: {0}", c.CellValue.Text);
                                ret += c.CellValue.Text;
                            }
                        }
                    }
                }
            }

            return ret;
        }    
    }
}