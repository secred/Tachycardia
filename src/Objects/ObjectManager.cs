using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class ObjectManager
    {
        List<GameObject> m_Objects;

        public ObjectManager()
        {
            m_Objects = new List<GameObject>();
        }

        public void Add(GameObject gameObject)
        {
            m_Objects.Add(gameObject);
        }

        public void Update()
        {
            foreach (GameObject gameObject in m_Objects)
                gameObject.Update();
        }

    }
}
