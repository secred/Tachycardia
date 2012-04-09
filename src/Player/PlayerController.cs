using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class LogicState
    {
        //maximum force 
        public float m_MaxForce;
        //maximum speed of moving forward 
        public float m_MaxSpd;
        //maximum speed of moving backward with shift
        public float m_MaxSpdBoost;
        //maximum speed of moving backward 
        public float m_MaxSpdBack;
        //maximum speed of turning left or right
        public float m_spdTurn;
        //maximum speed of turning left or right in boost
        public float m_spdTurnBoost;
        //maximum breaking force 
        public float m_spdStop;
        //collision shape for this state
        public MogreNewt.ConvexCollision m_collision;
        public float getSpdTurn()
        {
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT))
            {//boost speed
                return m_spdTurnBoost;
            }
            else
            {//normal state speed
                return m_spdTurn;
            }
        }

        public float getMaxSpd()
        {
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT))
            {//boost speed
                return m_MaxSpdBoost;
            }
            else
            {//normal state speed
                return m_MaxSpd;
            }
        }

        public virtual void willJump(){}
        
    };

    class PlayerController
    {
        public enum CharacterState
        {//opis stanow a zarazem jakie animacje beda wymagane
            //od grafikow dla naszego ludzika
            IDLE,//stoi i nigdzie sie nie wybiera (mozna odpalic jakas prosta animacje machanai rekami czy cos dla urozmaicenia)
            IDLE_ACTIVE,//stan active IDLE nasz nowy controler uzywa tego stanu gdy dąży do utrzymania sie w danej pozycji
            //np stoi na schodach, sila grawitacji ciagnie go w dol to bedzie sie lekko wiercic aby sie utrzymac
            //przepycha go jakis obiekt a gracz nie naciska klawiszy, postac reaguje siłą na obiekt ją przepychajacy
            //ale jezeli nie jest w stanie go zatrzymac bedzie sie cofac
            WALK,//zwykly marsz
            BACKWALK,//zwykly tylny marsz ( mozna to zmienic w wolny bieg w tyl chyba bedzie lepiej wygladac nawet)
            RUN,//bieg
            CROUCH_IDLE, //w miejscu kucając
            CROUCH,//idzie kucajac
            JUMP,//wyskoczyl (to trzeba bardziej rozbudwoac)
            CRAWL,//czolga sie?
            CATCHING,//probuje sie zlapac ( lecac juz w powietrzu np. wyciagniete rece do przodu or smth)
            HANGING,//wisi na czyms, lina, sciana etc. mozna bedzie rozrozniac jak bedzie potrzebne i bedzie czas
            CLIMB//wspina sie po scianie, a raczej podciaga (wczesniej wisial na krawedzi sciany)
        //podczas chodzenia mozna dodac obracanie glowa w tym samym kierunku w ktorym patrzy kamera
        //dzieki temu bedzie efekt ze player chce skrecic w danym kierunku a w trybie z oczu 
            //bedzie mozna zostawic kamere taka jak trybie zza plecow tylko z zerowa odelgloscia
            //czyt: (ludzie obracaja glowa nie patrza tylko i wylacznie przed siebie
        
        };


        public MogreNewt.Body m_MainBody;
        public MogreNewt.Body m_SecondBody;
        public Mogre.Node m_PlayerNode;

        public Mogre.Quaternion m_GoTo;//miejsce w ktore skierowana jest obecnie sila
        
        public float m_forceJump;

        public CharacterState m_State;
        public Dictionary<string,LogicState> m_myStates;

        private MogreNewt.Joint player_join;
        
        public LogicState m_currentState;

        public PlayerController(Mogre.Node node,Mogre.Entity entity,float mass)
        {
            //init logic states for controler
            initLogicStates();
            //force direction
            m_GoTo = Mogre.Quaternion.IDENTITY;
            //node to update
            m_PlayerNode = node;
            //Ball
            MogreNewt.ConvexCollision collision = new MogreNewt.CollisionPrimitives.Ellipsoid(
                    Core.Singleton.m_NewtonWorld,
                    new Mogre.Vector3(0.25f, 0.25f, 0.25f),
                    new Mogre.Quaternion(new Mogre.Radian(1.57f), new Mogre.Vector3(0, 1, 0)),
                    Core.Singleton.GetUniqueBodyId());
            Mogre.Vector3 inertia, offset;
            
            collision.CalculateInertialMatrix(out inertia, out offset);
            inertia *= mass;//mass

            m_MainBody = new MogreNewt.Body(Core.Singleton.m_NewtonWorld, collision, true);
            m_MainBody.SetMassMatrix(mass, inertia);
            m_MainBody.AutoSleep = false;
            
            m_MainBody.LinearDamping = 1.0f;
            m_MainBody.Transformed += BodyTransformCallback;
            m_MainBody.ForceCallback += BodyForceCallback;

            collision.Dispose();

            m_MainBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Player");
            //Ball end

            m_currentState = m_myStates["walk"];
            ///Second helper body
            Mogre.Vector3 inertia2, offset2;

            m_currentState.m_collision.CalculateInertialMatrix(out inertia2, out offset2);
            inertia2 *= 1;
            m_SecondBody = new MogreNewt.Body(Core.Singleton.m_NewtonWorld, m_currentState.m_collision, true);
            m_SecondBody.SetMassMatrix(1, inertia2);
            m_SecondBody.AutoSleep = false;
            m_SecondBody.IsGravityEnabled = false;
            m_SecondBody.SetPositionOrientation(new Mogre.Vector3(0, 1f, 0), Mogre.Quaternion.IDENTITY);
            m_SecondBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Player");

            //set Y joint for second body
            MogreNewt.Joint upVector = new MogreNewt.BasicJoints.UpVector(
            Core.Singleton.m_NewtonWorld, m_SecondBody, Mogre.Vector3.UNIT_Y);

            //connections between player bodies!
            player_join = new MogreNewt.BasicJoints.BallAndSocket(Core.Singleton.m_NewtonWorld, m_MainBody, m_SecondBody, new Mogre.Vector3(0, 0, 0));
        }

        //position buffor
        private Mogre.Vector3 _position = new Mogre.Vector3(0, 0.65f, 0);
        void BodyTransformCallback(MogreNewt.Body sender, Mogre.Quaternion orientation, Mogre.Vector3 position, int threadIndex)
        {//calling each physics iteration
            //odczytywac ze stanu wysokosc kamery
            m_PlayerNode.Position = position + _position;

            //obracanie zgodnie z kierunkiem ruchu lub jezeli 0 to z kierunkiem goto
            if(m_MainBody.Velocity.Length < 0.1f)
            {
                m_State = CharacterState.IDLE;
                m_PlayerNode.Orientation = m_GoTo;
            }
            else
            {
                if (m_MainBody.Velocity.Length > 3.5)
                    m_State = CharacterState.RUN;
                else
                    m_State = CharacterState.WALK;
                
                Mogre.Vector3 nowy1 = new Mogre.Vector3(0,0,1);
                Mogre.Vector3 nowy2 = m_MainBody.Velocity * new Mogre.Vector3(1,0,1);
                m_PlayerNode.Orientation = nowy1.GetRotationTo(nowy2);
            }
        }

        public void BodyForceCallback(MogreNewt.Body body, float timeStep, int threadIndex)
        {//calling each physics iteration
            KeyboardUpdate();
            /*
            if (m_State != CharacterState.IDLE)
            {
                //mass
                _force = m_Velocity * body.Mass * 5;
                if (body.Velocity.Length < m_MaxSpeed && body.Velocity.Length > m_MaxSpeed * -1)
                    body.AddForce(_force);
            }
            else
            {
                //stan IDLE w ktorym probujemy stac caigle w meijscu uzywajac kontr sily
                //nie jest on teraz stanem zatrzymania sie,
                //w tym stanie postac stara sie utrzymac w danym miejscu niwelujac predkosci jakie na nia dzialaja
                if (body.Velocity.Length > 0.001)
                {
                    //System.Console.WriteLine("idle niweluje" + m_Body.Velocity.Length);
                    body.AddForce(body.Velocity * -3 * m_MainBody.Mass * new Mogre.Vector3(1, 0, 1));

                }
            }*/
        }

        private float _BufforMaxSpeed;
        private Mogre.Vector3 _Force;
        public void KeyboardUpdate()
        {
            //JUMP
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_SPACE))
            {//ograniczenie czasowe skokow!
                
                //if crouch or crawl >>!TRY!<< stand up if standing - jump

                //I FLY? CATCH EDGE!
                //if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_C))
                {//i want catch smth

                }

                //dodac wyjatek dla pochylosci czyli kolizje z materialem ground
                if (m_MainBody.Velocity.y < 0.001f && m_MainBody.Velocity.y > -0.001f)
                {//set is in air?
                    m_MainBody.AddImpulse(new Mogre.Vector3(0, 10, 0), m_MainBody.Position);
                }
            }

            //MOVE FORWARD
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_W))
            {//i want to go forward, can i?
                //m_currentState.willForward();
                if (m_MainBody.Velocity.Length < m_currentState.getMaxSpd() && m_MainBody.Velocity.Length > m_currentState.getMaxSpd() * -1)
                {
                    _Force = m_GoTo * Mogre.Vector3.UNIT_Z;
                    _Force *= m_MainBody.Mass * m_currentState.m_MaxForce;
                    m_MainBody.AddForce(_Force);
                }
            }

            //MOVE BACKWARD 
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_S))
            {//i want to go backward its possible?
                /*m_MaxSpeed = 3.5f;
                //BUGGG
                //if (m_MainBody.Velocity.Length > m_MaxSpeed)
                //Mogre.Quaternion nowy = m_GoTo.Inverse();
                m_Force = -m_GoTo * Mogre.Vector3.UNIT_Z;
                m_Force *= m_Force * m_MainBody.Mass * 12;
                m_MainBody.AddForce(m_Force);*/
                
            }

            //TURN ME LEFT!
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_A))
            {//i want turn left, without exception?
                m_GoTo *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Mogre.Vector3(m_currentState.getSpdTurn() * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
            }

            //TURN ME RIGHT! 
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_D))
            {//i want turn right, possible always?
                m_GoTo *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Mogre.Vector3(-m_currentState.getSpdTurn() * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
            }

            //CROUCH
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_C))
            {//i want crouch, possible? 1. no contact with water or smth?
                if (m_currentState != m_myStates["crouch"])
                {
                    m_currentState = m_myStates["crouch"];
                    m_SecondBody.Collision = m_currentState.m_collision;
                }
            }

            //CRAWL
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_Z))
            {//i want to crawl, possible? 1. no contact with water or smth?

            }
        }

        private void initLogicStates()
        {
            m_myStates = new Dictionary<string, LogicState>();
            //walk
            LogicState walk = new LogicState();
            walk.m_MaxForce = 15;
            walk.m_MaxSpd = 3;
            walk.m_MaxSpdBoost = 8;
            walk.m_MaxSpdBack = 2;
            walk.m_spdStop = 2;
            walk.m_spdTurn = 5;
            walk.m_spdTurnBoost = 3;
            walk.m_collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                0.20f,
                1.25f,
                Mogre.Vector3.UNIT_X.GetRotationTo(Mogre.Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());
            m_myStates.Add("walk", walk);

            //crouch
            LogicState crouch = new LogicState();
            crouch.m_MaxForce = 8f;
            crouch.m_MaxSpd = 1.5f;
            crouch.m_MaxSpdBoost = 2;
            crouch.m_MaxSpdBack = 1;
            crouch.m_spdStop = 1;
            crouch.m_spdTurn = 2;
            crouch.m_spdTurnBoost = 3;
            crouch.m_collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                0.20f,
                0.5f,
                Mogre.Vector3.UNIT_X.GetRotationTo(Mogre.Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());
            m_myStates.Add("crouch", crouch);

            //fly
            LogicState fly = new LogicState();
            fly.m_MaxForce = 0;
            fly.m_MaxSpd = 0;
            fly.m_MaxSpdBoost = 0;
            fly.m_MaxSpdBack = 0;
            fly.m_spdStop = 0;
            fly.m_spdTurn = 0;
            fly.m_collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                0.20f,
                1.25f,
                Mogre.Vector3.UNIT_X.GetRotationTo(Mogre.Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());
            m_myStates.Add("fly", fly);

        }

    }
}
