using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAC.UISystem
{
	public delegate void UIEventDelegate();

	public struct UIEvent
	{
		public UIEventDelegate del;
		public UIEvent(UIEventDelegate del)
		{
			this.del = del;
		}
	}

	public static class UIEventQueue
	{
		public static Queue<UIEventDelegate> EventQueue = new();
		public static void PushEvent(UIEventDelegate item) => EventQueue.Enqueue(item);
	}
}
