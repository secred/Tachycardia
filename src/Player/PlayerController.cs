using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class LogicState
    {
        //maximum force 
        //public float m_MaxForce;
        //maximum speed of moving forward 
        public float m_MaxForceNormal;
        //maximum speed of moving backward with shift
        public float m_MaxForceBoost;
        //maximum speed of moving backward 
        //public float m_MaxSpdBack;
        //maximum speed of turning left or right
        public float m_spdTurn;
        //maximum speed of turning left or right in boost
        public float m_spdTurnBoost;
        //maximum breaking force 
        public float m_spdStop;
        //collision shape for this state

        public float m_MaxSpdBoost;
        public float m_MaxSpd;

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
        public float getMaxForce()
        {
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT))
            {//boost speed
                return m_MaxForceBoost;
            }
            else
            {//normal state speed
                return m_MaxForceNormal;
            }
        }

        public virtual void willJump() { }

        public virtual void willTurnLeft()
        {//pokracznie troche ale jest
            if (m_Control.m_MainBody.Velocity.Length > 0.1f)
                m_Control.m_GoTo = m_Control.m_LookingAt * new Mogre.Quaternion(new Mogre.Degree(35), Mogre.Vector3.UNIT_Y);
            else
                m_Control.m_LookingAt *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Mogre.Vector3(3 * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
        }

        public virtual void willTurnRight()
        {//pokracznie troche ale jest
            if (m_Control.m_MainBody.Velocity.Length > 0.1f)
                m_Control.m_GoTo = m_Control.m_LookingAt * new Mogre.Quaternion(new Mogre.Degree(-35), Mogre.Vector3.UNIT_Y);
            else
                m_Control.m_LookingAt *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Mogre.Vector3(-3 * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
        }

        public virtual void willForward()
        {
            Mogre.Vector3 _force = m_Control.m_GoTo * Mogre.Vector3.UNIT_Z;
            _force *= m_Control.m_MainBody.Mass * getMaxForce() * m_Control.m_adrenaline;
            m_Control.m_MainBody.AddForce(_force);
            //przeciwsila im wieksza predkosc tym wieksza sila stopujaca
            m_Control.m_MainBody.AddForce(-m_Control.m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1) * 4 * m_Control.m_MainBody.Mass);
        }

        public virtual void willBackward()
        {
            /*if (m_Control.m_MainBody.Velocity.Length < getMaxSpd() && m_Control.m_MainBody.Velocity.Length > getMaxSpd() * -1)
            {
                Mogre.Vector3 _force = m_Control.m_GoTo * Mogre.Vector3.UNIT_Z;
                _force *= m_Control.m_MainBody.Mass * m_MaxForce * m_Control.m_adrenaline;
                m_Control.m_MainBody.AddForce(-_force);
            }*/
            Mogre.Vector3 _force = m_Control.m_LookingAt * Mogre.Vector3.UNIT_Z;
            _force *= m_Control.m_MainBody.Mass * getMaxForce() * m_Control.m_adrenaline;
            m_Control.m_MainBody.AddForce(-_force);
            //przeciwsila im wieksza predkosc tym wieksza sila stopujaca
            m_Control.m_MainBody.AddForce(-m_Control.m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1) * 4 * m_Control.m_MainBody.Mass);

        }
    };

    class Normal : LogicState
    {
        public Normal()
        {
            m_MaxForceNormal = 20;
            m_MaxForceBoost = 40;
            //m_MaxSpdBack = 2;
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
            System.Console.WriteLine("FLY + IMPULSE");

            m_Control.m_MainBody.AddImpulse(new Mogre.Vector3(0, m_Control.m_jumpForce * m_Control.m_adrenaline, 0), m_Control.m_MainBody.Position);
            m_Control.m_Pose = m_Control.m_myPoses["fly"];
            m_Control.m_MainBody.LinearDamping = 0.3f;
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
            m_MaxForceNormal = 0;
            m_MaxForceBoost = 0;

            m_MaxSpd = 0;
            m_MaxSpdBoost = 0;
            //m_MaxSpdBack = 0;
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
            m_MaxForceNormal = 12;
            m_MaxForceBoost = 16;
            m_MaxSpd = 1.5f;
            m_MaxSpdBoost = 2;
            //m_MaxSpdBack = 1;
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
        public Mogre.Quaternion m_LookingAt;//miejsce w ktore skierowana jest obecnie postac

        public CharacterState m_State;
        public Dictionary<string, LogicState> m_myPoses;

        protected MogreNewt.Joint player_join;

        public float m_adrenaline;

        public LogicState m_Pose;

        //
        public int m_Onground;
        public int m_jumpLimit;
        public float m_jumpForce;
        public bool m_backward;
        

        public PlayerController(Mogre.Node node, Mogre.Entity entity, float mass)
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
        protected Mogre.Vector3 _position = new Mogre.Vector3(0, 0.65f, 0);
        public virtual void BodyTransformCallback(MogreNewt.Body sender, Mogre.Quaternion orientation, Mogre.Vector3 position, int threadIndex)
        {//calling each physics iteration
            //odczytywac ze stanu wysokosc kamery
            m_PlayerNode.Position = position + _position;

            //obracanie zgodnie z kierunkiem ruchu lub jezeli 0 to z kierunkiem goto
            if (m_MainBody.Velocity.Length < 0.1f)
            {
                m_State = CharacterState.IDLE;
                m_PlayerNode.Orientation = m_LookingAt;
                m_backward = false;
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

                if (!m_backward)
                {//dla wstecznego
                    Mogre.Vector3 xyVector = new Mogre.Vector3(0, 0, 1);
                    Mogre.Vector3 velocityxy = m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1);
                    m_LookingAt = m_PlayerNode.Orientation = xyVector.GetRotationTo(velocityxy);
                }
            }
        }

        public virtual void BodyForceCallback(MogreNewt.Body body, float timeStep, int threadIndex)
        {//calling each physics iteration

            //testuje jak dawno dotykal gruntu jezeli wartosc nie przekracza 5 lub 10 to moze wybic sie
            //w przeciwnym wypadku oznacza to ze dawno nie dotykal ziemi
            if (m_Onground < 10)
            {
                m_Onground++;
            }
            else
            {//jest w locie - zmieniamy mu stan 
                m_Pose = m_myPoses["fly"];
                m_MainBody.LinearDamping = 0.3f;
            }
            if (m_jumpLimit > 0)
                m_jumpLimit--;
            //obnizanie poziomu adrenaliny o staly czynnik
            if (m_adrenaline > 1)
                m_adrenaline -= 0.01f * timeStep;
            else
                m_adrenaline = 1;

            Update();

        }

        public virtual void Update()
        {
            if (Core.Singleton.m_StateManager.IsActiveState("Game"))
            {
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_X))
                {
                    Core.Singleton.m_Log.LogMessage("Pozycja X = " + Core.Singleton.m_Camera.Position.x + " Y = " + Core.Singleton.m_Camera.Position.y + " Z  = " + Core.Singleton.m_Camera.Position.z);
                }
                bool activateidle = true;
                bool turning = false;
                //JUMP
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_SPACE) && m_jumpLimit == 0)
                {//ograniczenie czasowe skokow!
                    m_Pose.willJump();
                    activateidle = false;
                }

                //MOVE FORWARD
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_W))
                {//i want to go forward, can i in this position?
                    m_Pose.willForward();
                    activateidle = false;
                    m_backward = false;
                }

                //MOVE BACKWARD 
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_S))
                {//i want to go backward its possible?
                    m_Pose.willBackward();
                    m_backward = true;
                    activateidle = false;
                }

                //TURN ME LEFT!
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_A))
                {//i want turn left, without exception?
                    /*if (activateidle)
                    {
                        m_Pose.willTurnLeftWithForce();
                    }
                    else
                    {*/
                        m_Pose.willTurnLeft();
                    //}
                    turning = true;
                }

                //TURN ME RIGHT! 
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_D))
                {//i want turn right, possible always?
                    /*if (activateidle)
                    {
                        m_Pose.willTurnRightWithForce();
                    }
                    else
                    {*/
                        m_Pose.willTurnRight();
                    //}
                    turning = true;
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

                //zeroanie wektoras kretu
                if (!turning)
                {
                    m_GoTo = m_LookingAt;
                }

                //proba zatrzymania typa w miejscu w ktorym ostatnio puscil klawisze
                if (activateidle == true && m_Pose == m_myPoses["normal"])
                {
                    if (!m_MainBody.Velocity.IsZeroLength)
                    {//nie zatrzymal sie jeszcze to go zatrzymujemy
                        Mogre.Vector3 xyVector = new Mogre.Vector3(0, 0, 1);
                        Mogre.Vector3 velocityxy = m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1);
                        Mogre.Quaternion ForceDirection = xyVector.GetRotationTo(velocityxy);
                        Mogre.Vector3 StoppingForce = -ForceDirection * Mogre.Vector3.UNIT_Z * m_MainBody.Mass * 6;
                        m_MainBody.AddForce(-StoppingForce);
                    }
                }
            }
        }

        protected virtual void initLogicStates()
        {
            m_myPoses = new Dictionary<string, LogicState>();
            
            m_backward = false;
            //m_adrenaline
            m_adrenaline = 1;
            //sila wyskoku
            m_jumpForce = 6;
            //pozycja wyprostowana
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
