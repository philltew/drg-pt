using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RugTech1.Framework.Input
{
	public static class InputHandler
    {
        public delegate void MouseCallBack(Point delta, bool altClick);
		public delegate void KeyCallBack(Keys keys, Keys modifiers, bool keyDown);

        public static event MouseCallBack OnMouseCallBack;
        public static event KeyCallBack OnKeyCallback;

        public static void AddMouseListener(MouseCallBack mouseCallBack)
        {
            OnMouseCallBack += mouseCallBack; 
        }

        public static void CallMouseCallBack(Point delta, bool altClick)
        {
            if (OnMouseCallBack != null)
            {
                OnMouseCallBack(delta, altClick);
            }
        }

        public static void AddKeyListener(KeyCallBack keyCallBack)
        {
            OnKeyCallback += keyCallBack;
        }

        public static void CallKeyCallback(Keys keys, Keys modifiers, bool keyDown)
        {
            if (OnKeyCallback != null)
            {
				OnKeyCallback(keys, modifiers, keyDown);
            }
        }
    }
}
