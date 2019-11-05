using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace DataMiner.Engine
{
    public class PDFToText
	{
		private readonly string appFolder;
		private readonly string tempFolder;

		public PDFToText()
		{
			IConfiguration config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", true, true)
				.Build();

			appFolder = config.GetSection("Tesseract").GetValue<string>("EXEPath");
			tempFolder = config.GetSection("Tesseract").GetValue<string>("TempPath");
		}  

		public EngineReturnArgs ConvertFilePDF(string fileName)
		{
			byte[] bytes = File.ReadAllBytes(fileName);

			return ConvertTextPDF(bytes);
		}

		public EngineReturnArgs ConvertTextPDF(byte[] bytes) 
		{
			EngineReturnArgs era = new EngineReturnArgs();

			try
			{  
				string pdfContent = string.Empty;
				//BitMiracle.Docotic.LicenseManager.AddLicenseData("6C2M7-UOMI1-N940I-85GVF-QLJR0");     //geoff's laptop license only 

				//	this BitMiracle component does almost all of the work here. It was selected for use in this service
				//	because it quikly and accuratley convert the pdf into well-formatted text.
				//	retaining the correct formatting/positioning of text is important because other services that work
				//	with the text may rely on positional data to parse content

				/* using (PdfDocument pdf = new PdfDocument(bytes))   
				{
					pdfContent = pdf.GetText();
					bool isOK = pdfContent.Trim(Environment.NewLine.ToCharArray()).Length > 100;
					if (isOK)
					{
						era.Success = true;
						era.Message = "Conversion complete";
						era.Content = pdfContent;
					}
					else
					{

					} */
				//}
			}
			catch (Exception ex)
			{
				//Log.Error(ex, "In Convert.PDF");
				era.Success = false;
				era.Message = "Conversion failed";
				era.Content = ex.Message;
			}
			return era;
		}


		public EngineReturnArgs ConvertImagePDF(byte[] bytes)
		{
			List<string> tempFiles = new List<string>();
			EngineReturnArgs era = new EngineReturnArgs();

			try
			{
				string pdfContent = string.Empty;

			/* 	using (PdfDocument pdf = new PdfDocument(bytes))
				{
					//int x = 0;
					foreach (PdfImage pdfimage in pdf.GetImages())
					{
						//do something here
					}
				} */

				if (pdfContent.Trim().Length == 0)
				{
					era.Success = false;
					era.Message = "Conversion from Image failed - no content could be extracted";
					era.Content = string.Empty; ;
				}
				else
				{
					era.Success = true;
					era.Message = "Conversion from Image complete";
					era.Content = pdfContent;
				}
			}
			catch (Exception ex)
			{
				//Log.Error(ex, "In ConvertImagePDF.PDF");
				era.Success = false;
				era.Message = "Conversion failed";
				era.Content = ex.Message;
			}
			finally
			{
				DeleteTempFiles(tempFiles);
			}
			return era;
		}


		private void DeleteTempFiles(List<string> tempFiles)
		{
			foreach (string tempFile in tempFiles)
			{
				System.IO.File.Delete(tempFile);
			}
		}


		private string ImageToText(string pdfImageName)
		{
			StringBuilder outputBuilder = new StringBuilder();

			Process process = new Process();

			ProcessStartInfo info = new ProcessStartInfo()
			{
				FileName = $@"{appFolder}tesseract.exe",
				Arguments = $@"{pdfImageName} stdout -l ENG --tessdata-dir {appFolder}tessdata",
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};

			process.StartInfo = info;
			process.EnableRaisingEvents = true;

			process.OutputDataReceived += new DataReceivedEventHandler
			(
				delegate (object sender, DataReceivedEventArgs e)
				{
					outputBuilder.Append(e.Data);
				}
			);

			process.Start();
			process.BeginOutputReadLine();
			process.WaitForExit();
			process.CancelOutputRead();

			return outputBuilder.ToString();
		}

		//private string GetFileContent(string data)
		//{
		//	var result = System.Text.Encoding.Unicode.GetBytes(data);
		//	return Convert.ToBase64String(result);

		//}
	}
}


