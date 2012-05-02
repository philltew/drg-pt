using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RugTech1.Framework.Objects.UI
{
	public interface IUiControlContainer
	{
		void AttachDynamicControls();
		void DetachDynamicControls();
	}
}
