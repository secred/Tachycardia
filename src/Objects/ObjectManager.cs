using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class ObjectManager
    {
        Dictionary<string, GameObject> m_Objects;

        public ObjectManager()
        {
            m_Objects = new Dictionary<string, GameObject>();
        }

        public void Add(string objectName, GameObject gameObject)
        {
            m_Objects.Add(objectName, gameObject);
        }

        public GameObject Find(string objectName)
        {
            if (m_Objects.ContainsKey(objectName))
            {
                return m_Objects[objectName];
            }
            return null;
        }

        public void Update()
        {
            foreach (KeyValuePair<string, GameObject> gameObjectPair in m_Objects)
                gameObjectPair.Value.Update();
        }

    }
}
