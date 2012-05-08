using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects.Actions
{
    class AddAdrenaline : BaseAction
    {
        public int m_addValue;
        
        public override void Go()
        {
            //getting pointer to player
            Tachycardia.Character _Player = (Tachycardia.Character)Tachycardia.Core.Singleton.ObjectManager.Find("player");
            //added at each physics loop, must be a limit, time or smth
            //if(time > lastUsed+1000)
            //_Player.adrenaline += m_addValue;
        }
    }
}
