using System;

namespace DataMinerAPI.Models
{
	/// <summary>
	/// 
	/// </summary>
	public class TextProcessorReturnArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public TextProcessorReturnArgs() { }

		/// <summary>
		/// 
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Exception Exception { get; set; }

	}
}
