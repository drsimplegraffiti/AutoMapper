using System;
using System.Globalization;

namespace Techie.Modal
{
	public class ApiResponse
	{
		public int ResponseCode { get; set; }
		public string Result { get; set; } = default!;
		public string? ErrorMessage { get; set; }
	}
}

