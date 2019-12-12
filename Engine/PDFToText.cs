using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Serilog;
using DataMinerAPI.Models;

namespace DataMinerAPI.Engine
{
    public class PDFToText
	{
		public PDFToText()
		{

		}  

		public ResponseEntity ConvertPDFToText(string conversionSource, 
									Guid requestGuid) 
		{
			ResponseEntity respEntity = new ResponseEntity();
			respEntity.RequestID = requestGuid;
			try
			{ 
				string textFileName = Path.ChangeExtension(conversionSource, ".txt");

				string converter = @"Engine/pdftotext";

				RunProcess(converter, $" -layout {conversionSource} {textFileName}");

				respEntity.DocumentContent = File.ReadAllText(textFileName,Encoding.UTF8);
				respEntity.Success = true;
				respEntity.Message = "Conversion ok";
			}
			catch (Exception ex)
			{
				Log.Error(ex, "In Convert.PDF");
				respEntity.Success = false;
				respEntity.Message = "Conversion failed";
				respEntity.DocumentContent = ex.Message;
			}
			return respEntity;
		}


		public ResponseEntity ConvertImagePDF(byte[] bytes)
		{
			List<string> tempFiles = new List<string>();
			ResponseEntity era = new ResponseEntity();

			try
			{
				string pdfContent = string.Empty;

				if (pdfContent.Trim().Length == 0)
				{
					era.Success = false;
					era.Message = "Conversion from Image failed - no content could be extracted";
					era.DocumentContent = string.Empty; ;
				}
				else
				{
					era.Success = true;
					era.Message = "Conversion from Image complete";
					era.DocumentContent = pdfContent;
				}
			}
			catch (Exception ex)
			{
				era.Success = false;
				era.Message = "Conversion failed";
				era.DocumentContent = ex.Message;
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


	/* 	private string ImageToText(string pdfImageName)
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
		} */
		private bool RunProcess(string command, string arguments)
		{
			StringBuilder outputBuilder = new StringBuilder();
			Process process = new Process();

			try{

			ProcessStartInfo info = new ProcessStartInfo()
			{
				FileName = command,
				Arguments = $@"{arguments}",
				CreateNoWindow = false,
				RedirectStandardOutput = false,
				UseShellExecute = false
			};

			process.StartInfo = info;
			process.EnableRaisingEvents = true;
			
			process.Start();			
			process.WaitForExit();	

			return true;
			}
			catch(Exception ex)
			{
				Log.Error("RunProcess", ex);
				return false;
			}
		}

		private string GetFileContent(string data)
		{
			var result = System.Text.Encoding.Unicode.GetBytes(data);
			return Convert.ToBase64String(result);
		}
	}
}


