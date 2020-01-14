using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Core.Reflection
{
	[Serializable]
	public class MethodCaches : ICoreConfig<MethodCaches>
	{
        private List<MethodCache> mechodCaches = new List<MethodCache>();

        public List<MethodCache> MechodCaches
		{
			get { return mechodCaches; }
			set { mechodCaches = value; }
		}
	}
}
