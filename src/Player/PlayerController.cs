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

        public PlayerController m_Control;

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

        public virtual void willTurnLeft()
        {
            m_Control.m_GoTo *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Mogre.Vector3(getSpdTurn() * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
        }

        public virtual void willTurnRight() 
        {
            m_Control.m_GoTo *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Mogre.Vector3(-getSpdTurn() * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
        }

        public virtual void willForward()
        {
            if (m_Control.m_MainBody.Velocity.Length < getMaxSpd() && m_Control.m_MainBody.Velocity.Length > getMaxSpd() * -1)
            {
                Mogre.Vector3 _force = m_Control.m_GoTo * Mogre.Vector3.UNIT_Z;
                _force *= m_Control.m_MainBody.Mass * m_MaxForce;
                m_Control.m_MainBody.AddForce(_force);
            }
        }

        public virtual void willBackward()
        {
            if (m_Control.m_MainBody.Velocity.Length < getMaxSpd() && m_Control.m_MainBody.Velocity.Length > getMaxSpd() * -1)
            {
                Mogre.Vector3 _force = m_Control.m_GoTo * Mogre.Vector3.UNIT_Z;
                _force *= m_Control.m_MainBody.Mass * m_MaxForce;
                m_Control.m_MainBody.AddForce(-_force);
            }
        }
    };

    class Normal : LogicState
    {
        public Normal()
        {
            m_MaxForce = 15;
            m_MaxSpd = 3;
            m_MaxSpdBoost = 8;
            m_MaxSpdBack = 2;
            m_spdStop = 2;
            m_spdTurn = 5;
            m_spdTurnBoost = 3;
            m_collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                0.25f,
                1.25f,
                Mogre.Vector3.UNIT_X.GetRotationTo(Mogre.Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());
        }

        public override void willJump()
        {
            //addimpulse()
            System.Console.WriteLine("IMPULSE");

            m_Control.m_MainBody.AddImpulse(new Mogre.Vector3(0, 8, 0), m_Control.m_MainBody.Position);
            m_Control.m_Pose = m_Control.m_myPoses["fly"];
            m_Control.m_jumpLimit = (int)Core.m_FixedFPS;//ograniczenie mozliwosci skoku na jedna sekunde
            //m_Control.changestateto(""fly)/
        }

        /*public override void willTurnLeft()
        {
            //normal turn
        }

        public override void willTurnRight()
        {
            //normal turn
        }

        public override void willForward() 
        {
            //normal forward
        }*/
    }

    class Fly : LogicState
    {
        public Fly()
        {
            m_MaxForce = 0;
            m_MaxSpd = 0;
            m_MaxSpdBoost = 0;
            m_MaxSpdBack = 0;
            m_spdStop = 0;
            m_spdTurn = 0;
            m_collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                0.30f,
                1.25f,
                Mogre.Vector3.UNIT_X.GetRotationTo(Mogre.Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());
        }

        public override void willJump()
        {
            //jump impossible, check if can I catch smth
        }

        public override void willTurnLeft()
        {
            //normal turn
        }

        public override void willTurnRight()
        {
            //normal turn
        }

        public override void willForward()
        {
            //forward impossible?
        }
    }
    
    class Crouch : LogicState
    {
        public Crouch()
        {
            m_MaxForce = 8f;
            m_MaxSpd = 1.5f;
            m_MaxSpdBoost = 2;
            m_MaxSpdBack = 1;
            m_spdStop = 1;
            m_spdTurn = 2;
            m_spdTurnBoost = 3;

            m_collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                0.25f,
                0.5f,
                Mogre.Vector3.UNIT_X.GetRotationTo(Mogre.Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());
        }

        public override void willJump()
        {
            //jump impossible, check if can I change state to normal if yes do it
            m_Control.m_Pose = m_Control.m_myPoses["walk"];
            m_Control.m_SecondBody.Collision = m_Control.m_Pose.m_collision;
        }

        /*public override void willTurnLeft()
        {
            //normal turn
        }

        public override void willTurnRight()
        {
            //normal turn
        }

        public override void willForward()
        {
            //forward with my paramaters
        }*/
    }

    class Hanging : LogicState
    {
        public override void willJump()
        {
            //jump impossible, loose join
        }

        public override void willTurnLeft()
        {
            //normal turn
        }

        public override void willTurnRight()
        {
            //normal turn
        }

        public override void willForward()
        {
            //gives control to handle object?
        }
    }

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

        Mogre.Vector3 m_stayHere;
        //public float m_forceJump;

        public CharacterState m_State;
        public Dictionary<string,LogicState> m_myPoses;

        private MogreNewt.Joint player_join;
        
        public LogicState m_Pose;

        public int m_jumpLimit;

        public PlayerController(Mogre.Node node,Mogre.Entity entity,float mass)
        {
            m_stayHere = Mogre.Vector3.ZERO;
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
            m_MainBody.UserData = this;
            m_MainBody.Type = (int)PhysicsManager.BodyTypes.PLAYER;

            collision.Dispose();

            m_MainBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Player");
            //Ball end

            m_Pose = m_myPoses["normal"];
            ///Second helper body
            Mogre.Vector3 inertia2, offset2;

            m_Pose.m_collision.CalculateInertialMatrix(out inertia2, out offset2);
            inertia2 *= 1;
            m_SecondBody = new MogreNewt.Body(Core.Singleton.m_NewtonWorld, m_Pose.m_collision, true);
            m_SecondBody.SetMassMatrix(1, inertia2);
            m_SecondBody.AutoSleep = false;
            m_SecondBody.IsGravityEnabled = false;
            m_SecondBody.SetPositionOrientation(new Mogre.Vector3(0, 1f, 0), Mogre.Quaternion.IDENTITY);
            m_SecondBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Player");
            m_SecondBody.UserData = null;
            
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
                if (m_Pose == m_myPoses["normal"])
                {
                    if (m_MainBody.Velocity.Length > 3.5)
                        m_State = CharacterState.RUN;
                    else
                        m_State = CharacterState.WALK;
                }
                else if (m_Pose == m_myPoses["fly"])
                {
                    m_State = CharacterState.JUMP;
                }

                Mogre.Vector3 xyVector = new Mogre.Vector3(0,0,1);
                Mogre.Vector3 velocityxy = m_MainBody.Velocity * new Mogre.Vector3(1,0,1);
                m_PlayerNode.Orientation = xyVector.GetRotationTo(velocityxy);
            }
        }

        public void BodyForceCallback(MogreNewt.Body body, float timeStep, int threadIndex)
        {//calling each physics iteration

            //jezeli niedawno skoczyl to obniz
            //to zmiany bo jak zeskoczymy z czegos to nadal mozna bedzie podskoczycz lecac
            if (m_jumpLimit > 0)
                m_jumpLimit--;

            KeyboardUpdate();
            
        }
        
        public void KeyboardUpdate()
        {

            if (Core.Singleton.m_StateManager.IsActiveState("Game"))
            {
                bool activateidle = true;
                //JUMP
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_SPACE) && m_jumpLimit == 0)
                {//ograniczenie czasowe skokow!
                    m_Pose.willJump();
                    //if crouch or crawl >>!TRY!<< stand up if standing - jump

                    //I FLY? CATCH EDGE!
                    //if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_C))
                    {//i want catch smth

                    }
                    activateidle = false;
                }

                //MOVE FORWARD
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_W))
                {//i want to go forward, can i in this position?
                    m_Pose.willForward();
                    activateidle = false;
                }

                //MOVE BACKWARD 
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_S))
                {//i want to go backward its possible?
                    m_Pose.willBackward();
                    activateidle = false;
                }

                //TURN ME LEFT!
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_A))
                {//i want turn left, without exception?
                    m_Pose.willTurnLeft();
                }

                //TURN ME RIGHT! 
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_D))
                {//i want turn right, possible always?
                    m_Pose.willTurnRight();
                }

                //CROUCH
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_C))
                {//i want crouch, possible? 1. no contact with water or smth?
                    //m_Pose.willCrouch();
                    
                    if (m_Pose != m_myPoses["crouch"])
                    {
                        m_Pose = m_myPoses["crouch"];
                        m_SecondBody.Collision = m_Pose.m_collision;
                    }
                }

                //CRAWL
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_Z))
                {//i want to crawl, possible? 1. no contact with water or smth?

                }

                //proba zatrzymania typa w miejscu w ktorym ostatnio puscil klawisze
                if (activateidle == true)
                {
                    if (!m_MainBody.Velocity.IsZeroLength)
                    {//nie zatrzymal sie jeszcze to go zatrzymujemy
                        Mogre.Vector3 xyVector = new Mogre.Vector3(0, 0, 1);
                        Mogre.Vector3 velocityxy = m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1);
                        Mogre.Quaternion ForceDirection = xyVector.GetRotationTo(velocityxy);
                        //System.Console.WriteLine(kierunek);
                        Mogre.Vector3 StoppingForce = -ForceDirection * Mogre.Vector3.UNIT_Z * m_MainBody.Mass * 4;
                        m_MainBody.AddForce(-StoppingForce);
                    }
                }
            }
        }

        private void initLogicStates()
        {
            m_myPoses = new Dictionary<string, LogicState>();
            
            //walk
            LogicState normal = new Normal();//LogicState();
            normal.m_Control = this;//przyda sie uchwyt do swojego kontrolera
            m_myPoses.Add("normal", normal);//dodanie obiektu

            //crouch
            LogicState crouch = new Crouch();
            crouch.m_Control = this;//przyda sie uchwyt do swojego kontrolera
            m_myPoses.Add("crouch", crouch);//dodanie obiektu

            //fly
            LogicState fly = new Fly();
            fly.m_Control = this;//przyda sie uchwyt do swojego kontrolera
            m_myPoses.Add("fly", fly);//dodanie obiektu

        }

    }
}
