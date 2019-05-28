using System;

namespace Safester.Models
{
	public class BaseResult
	{
		public string status { get; set; }
		public string errorMessage { get; set; }
        public string exceptionStackTrace { get; set; }
    }
}
