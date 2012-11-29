using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xlnt
{
	public static class EventHandlerExtensions
	{
		public static void Raise<T>(this EventHandler<T> self, object sender, T args) where T : EventArgs {
			if(self == null)
				return;
			self(sender, args);
		}
	}
}
