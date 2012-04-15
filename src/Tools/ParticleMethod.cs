using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Tachycardia.src.Tools
{
    class ParticleMethod
    {
        private static ParticleMethod instance;
        private ArrayList staticParticleList = new ArrayList();
        private ParticleMethod() { }

        public static ParticleMethod Instance
        {
          get 
          {
             if (instance == null)
             {
                 instance = new ParticleMethod();
             }
             return instance;
          }
       }

       public void addStaticParticleName2List(String name)
       {
           staticParticleList.Add(name);
       }

       public bool checkStaticParticleName(string name)
       {
           foreach (Object obj in staticParticleList)
           {
               if (obj == name)
                   return true;
           }
           
           return false;
       }

    }
}
