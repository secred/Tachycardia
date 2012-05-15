using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;


namespace Tachycardia
{

    public class PredicateRaycast : MogreNewt.Raycast
    {
        public class ContactInfo
        {
            public float Distance;
            public MogreNewt.Body Body;
            public Vector3 Normal;
        }

        public Predicate<MogreNewt.Body> Predicate;
        public List<ContactInfo> Contacts;

        public PredicateRaycast(Predicate<MogreNewt.Body> pred)
        {
            Predicate = pred;
            Contacts = new List<ContactInfo>();
        }

        public override bool UserPreFilterCallback(MogreNewt.Body body)
        {
            return Predicate(body);
        }

        public void SortContacts()
        {
            Contacts.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        public override bool UserCallback(MogreNewt.Body body, float distance, Vector3 normal, int collisionID)
        {
            ContactInfo contact = new ContactInfo();
            contact.Distance = distance;
            contact.Body = body;
            contact.Normal = normal;
            Contacts.Add(contact);
            return true;
        }
    }

    class GameCamera
    {
        public Character Character;
        public Degree Angle;
        public float Distance;
        public SceneNode m_Node;
        public int m_Type;
        public float m_Tight;
        public Vector3 m_Height;
        public Vector3 InterPosition;
        public Mogre.Quaternion orientation;

        public GameCamera()
        {
            //float mass = 1;
            m_Node = Core.Singleton.SceneManager.RootSceneNode.CreateChildSceneNode();
            orientation = Mogre.Quaternion.IDENTITY;
        
        }

        /*public void Update()
        {
            Vector3 offset =
            Character.m_Node.Orientation * (-Vector3.UNIT_Z +
                (Vector3.UNIT_Y)
                ).NormalisedCopy * Distance;

            Vector3 head = Character.m_Node.Position + Character.m_HeadOffset;
            Vector3 desiredPosition = head + offset;

            Vector3 newPosition = (Core.Singleton.Camera.Position +
                    (desiredPosition - Core.Singleton.Camera.Position) * m_Tight);

            m_Node.SetPosition(newPosition.x, newPosition.y, newPosition.z);

            Core.Singleton.Camera.Position = m_Node.Position + m_Height;

            Core.Singleton.Camera.LookAt(head);
        }*/

        public void Update()
        {
            Vector3 offset =
                /*Character.m_Node.Orientation*/orientation * (-Vector3.UNIT_Z +
                (Vector3.UNIT_Y * (float)System.Math.Tan(Angle.ValueRadians))
                ).NormalisedCopy * Distance;

            Vector3 head = Character.m_Node.Position + Character.m_HeadOffset;
            Vector3 desiredPosition = head + offset;

            InterPosition += (desiredPosition - InterPosition) * 0.1f;

            PredicateRaycast raycast = new PredicateRaycast((b => !(b.UserData is Tachycardia.Objects.Trigger || b.UserData is Character)));
            raycast.Go(Core.Singleton.NewtonWorld, head, InterPosition);
            if (raycast.Contacts.Count != 0)
            {
                raycast.SortContacts();
                Core.Singleton.Camera.Position = head + (InterPosition - head) * raycast.Contacts[0].Distance
                    + raycast.Contacts[0].Normal * 0.15f;
            }
            else
                Core.Singleton.Camera.Position = InterPosition;

            Core.Singleton.Camera.LookAt(head);
        }

        public void Cam1()
        {
            m_Type = 0;
            Distance = 1.0f;
            m_Tight = 0.05f;
            m_Height = new Vector3(0, -0.035f, 0);
        }
        public void Cam2()
        {
            m_Type = 1;
            Distance = 4.0f;
            m_Tight = 0.5f;
            m_Height = new Vector3(0, 1.0f, 0);
        }
        public void Cam3()
        {
            m_Type = 2;
            Distance = 0.0f;
            m_Tight = 0.05f;
            m_Height = new Vector3(0, 0.5f, 0);
        }
        public void Cam4()
        {
            m_Type = 3;
            Distance = 4.0f;
            m_Tight = 0.1f;
            m_Height = new Vector3(0, 0, 0);
        }
    }
}
