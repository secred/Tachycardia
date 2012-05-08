using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mogre;

namespace Tachycardia
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Core.Singleton.Initialise())
            {
                Core.Singleton.StateManager.RegisterState("Intro", new IntroState());
                Core.Singleton.StateManager.RegisterState("Game", new GameState());

                Core.Singleton.StateManager.PushState("Intro");
                Core.Singleton.Go();
            }
            else
            {
                throw new Exception("Cannot initialize Tachycardia.Core object.");
            } 
        }
    }
}
