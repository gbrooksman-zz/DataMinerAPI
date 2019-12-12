using System;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;  
using DocumentFormat.OpenXml.Spreadsheet;  
using System.Linq;
using Serilog;
using DataMinerAPI.Models;

namespace DataMinerAPI.Engine
{
    public class ExcelToText
	{
        public ResponseEntity ConvertExcelToText(string conversionSource, Guid requestGuid)
        {     
            ResponseEntity respEntity = new ResponseEntity();
            respEntity.RequestID = requestGuid;

            try
            {  
                string textFileName = Path.ChangeExtension(conversionSource, ".txt");
            
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(conversionSource, false))
                {
                    foreach (Sheet sheet in doc.WorkbookPart.Workbook.Descendants<Sheet>())
                    {
                        WorksheetPart sheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(sheet.Id);
                        Worksheet workSheet = sheetPart.Worksheet;

                        SharedStringTablePart sharedStringPart = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
                        SharedStringItem[] sharedStringItem = sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                        using (var outputFile = File.CreateText(textFileName))
                        {
                            foreach (var row in workSheet.Descendants<Row>())
                            {
                                StringBuilder sb = new StringBuilder();
                                foreach (Cell cell in row)
                                {
                                    string cellText = string.Empty;
                                    if (cell.CellValue != null)
                                    {
                                        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                                        {
                                            cellText = sharedStringItem[int.Parse(cell.CellValue.Text)].InnerText;
                                        }
                                        else
                                        {
                                            cellText = cell.CellValue.Text;
                                        }
                                    }
                                    sb.Append(string.Format("{0},", cellText.Trim()));
                                }                                
                                outputFile.WriteLine(sb.ToString().TrimEnd(','));
                            }
                        }
                    }
                }

                respEntity.DocumentContent = File.ReadAllText(textFileName,Encoding.UTF8);
			    respEntity.Success = true;
			    respEntity.Message = "Conversion ok";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "In ConvertExcelToText");
				respEntity.Success = false;
				respEntity.Message = "Conversion failed";
				respEntity.DocumentContent = ex.Message;
            }       
          
            return respEntity;
        }    
    }
}