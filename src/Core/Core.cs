using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    abstract class Core
    {
        private static Core m_Instance;
        public static Core Singleton()
        {
            return m_Instance;
        }

        public abstract Mogre.Root GetRoot();
        public abstract Mogre.Log GetLog();
        public abstract MOIS.Mouse GetMouse();
        public abstract MOIS.Keyboard GetKeyboard();
        public abstract Mogre.RenderWindow GetRenderWindow();
    }
}
