using System;

namespace Safester.Models
{	
	public class KeyInfo : BaseResult
    {
		public string publicKey { get; set; }
        public string privateKey { get; set; }
    }
}
