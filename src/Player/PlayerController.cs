using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class LogicState
    {
        public string m_name;
        //maximum force of moving forward without boost 
        public float m_ForwardForce;
        //maximum speed of moving forward with boost
        public float m_ForwardForceBoost;
        //maximum force of moving backward without boost 
        public float m_BackwardForce;
        //maximum speed of moving backward with boost
        public float m_BackwardForceBoost;

        //maximum speed of turning left or right
        public Mogre.Degree m_spdTurn;
        //maximum speed of turning left or right with boost
        public float m_spdTurnBoost;
        //maximum breaking force 
        public float m_ForceStop;
        //damping value helps to stop body but prevents in the fly
        public float m_LinearDampingValue;
        
        //collision shape for this state
        public MogreNewt.ConvexCollision m_collision;
        
        public Mogre.Vector3 m_HeadPosition;

        //my controler instance
        public PlayerController m_Control;
        public LogicState()
        {
            m_name = "unknown";
            m_HeadPosition = new Mogre.Vector3(0, 0.65f, 0);
            //forward move
            m_ForwardForce = 20;
            m_ForwardForceBoost = 40;
            
            //backwardmove
            m_BackwardForce = 10;
            m_BackwardForceBoost = 20;
            
            //turning angle in degree
            m_spdTurn = 25;
            m_spdTurnBoost = 15;

            //stopping power
            m_ForceStop = 6;
            m_LinearDampingValue = 1.0f;
        }

        public Mogre.Degree getSpdTurn()
        {
            if (m_Control.m_bBoost)
            {//boost speed
                return m_spdTurnBoost;
            }
            else
            {//normal state speed
                return m_spdTurn;
            }
        }

        public float getForwardForce()
        {
            if (m_Control.m_bBoost)
            {//boost speed
                return m_ForwardForceBoost;
            }
            else
            {//normal state speed
                return m_ForwardForce;
            }
        }

        public float getBackwardForce()
        {
            if (m_Control.m_bBoost)
            {//boost speed
                return m_BackwardForceBoost;
            }
            else
            {//normal state speed
                return m_BackwardForce;
            }
        }
        public virtual void willJump() {/* control passed to specific Pose object */ }
        public virtual void willCrouch() {/* control passed to specific Pose object */ }

        public virtual void willTurnLeft()
        {//turning left
            if (m_Control.m_MainBody.Velocity.Length > 0.1f)
            {
                m_Control.m_GoTo = m_Control.m_PlayerNode.Orientation * new Mogre.Quaternion(new Mogre.Degree(getSpdTurn()), Mogre.Vector3.UNIT_Y);
            }
            else
            {//
                m_Control.m_PlayerNode.Orientation *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Mogre.Vector3(3 * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
            }
        }

        public virtual void willTurnRight()
        {//turning right
            if (m_Control.m_MainBody.Velocity.Length > 0.1f)
            {//
                m_Control.m_GoTo = m_Control.m_PlayerNode.Orientation * new Mogre.Quaternion(new Mogre.Degree(-getSpdTurn()), Mogre.Vector3.UNIT_Y);
            }
            else
            {//
                m_Control.m_PlayerNode.Orientation *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Mogre.Vector3(-3 * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
            }
        }

        public virtual void willForward()
        {
            Mogre.Vector3 _force = m_Control.m_GoTo * Mogre.Vector3.UNIT_Z;
            _force *= m_Control.m_MainBody.Mass * getForwardForce() * m_Control.m_adrenaline;
            m_Control.m_MainBody.AddForce(_force);
           
            //pseudo air resistance and other forces which stopping us in moving, determined experimentally
            m_Control.m_MainBody.AddForce(-m_Control.m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1) * 4 * m_Control.m_MainBody.Mass);
        }

        public virtual void willBackward()
        {
            Mogre.Vector3 _force = m_Control.m_PlayerNode.Orientation * Mogre.Vector3.UNIT_Z;
            _force *= m_Control.m_MainBody.Mass * getBackwardForce() * m_Control.m_adrenaline;
            m_Control.m_MainBody.AddForce(-_force);
            
            //pseudo air resistance and other forces which stopping us in moving, determined experimentally
            m_Control.m_MainBody.AddForce(-m_Control.m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1) * 4 * m_Control.m_MainBody.Mass);
        }

        public virtual void Activate()
        {
            m_Control.m_MainBody.LinearDamping = m_LinearDampingValue;
            m_Control.m_SecondBody.Collision = m_collision;
            //zmiana jointa rowniez np zeby byl nizszy niz wczesniej
            m_Control.m_Pose = this;
        }
    };

    class Normal : LogicState
    {
        public Normal()
        {
            m_name = "normal";
            m_ForwardForce = 20;
            m_ForwardForceBoost = 40;

            m_collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                0.30f,
                1.25f,
                Mogre.Vector3.UNIT_X.GetRotationTo(Mogre.Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());
        }

        public override void willJump()
        {
            m_Control.m_MainBody.AddImpulse(new Mogre.Vector3(0, m_Control.m_jumpForce * m_Control.m_adrenaline, 0), m_Control.m_MainBody.Position);
            m_Control.ChangePoseTo("fly");
            m_Control.m_jumpLimit = (int)Core.m_FixedFPS;//ograniczenie mozliwosci skoku na jedna sekunde
        }

        public override void willCrouch()
        {
            m_Control.ChangePoseTo("crouch");
        }
    }

    class Fly : LogicState
    {
        public Fly()
        {
            m_name = "fly";
            m_ForwardForce = 0;
            m_ForwardForceBoost = 0;
            m_ForceStop = 0;//nie zatzrymuj go w powietrzu
            //lekki opór powietrza
            m_LinearDampingValue = 0.3f;
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
            m_ForwardForce = 12;
            m_ForwardForceBoost = 16;
            m_name = "crouch";
            m_collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                0.25f,
                0.5f,
                Mogre.Vector3.UNIT_X.GetRotationTo(Mogre.Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());
        }

        public override void willJump()
        {
            m_Control.ChangePoseTo("normal");
            //jump impossible, check if can I change state to normal if yes do it
        }
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
        /* Button Flags */
        public bool m_bForward;
        public bool m_bBackward;
        public bool m_bLeft;
        public bool m_bRight;
        public bool m_bBoost;
        
        public MogreNewt.Body m_MainBody;
        public MogreNewt.Body m_SecondBody;
        public Mogre.Node m_PlayerNode;


        public Mogre.Quaternion m_GoTo;//miejsce w ktore skierowana jest obecnie sila
        
        public CharacterState m_State;
        public Dictionary<string, LogicState> m_myPoses;

        protected MogreNewt.Joint player_join;

        public float m_adrenaline;

        public LogicState m_Pose;

        //
        public int m_Onground;
        public int m_jumpLimit;
        public float m_jumpForce;
        

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
        public virtual void BodyTransformCallback(MogreNewt.Body sender, Mogre.Quaternion orientation, Mogre.Vector3 position, int threadIndex)
        {//calling each physics iteration
            //odczytywac ze stanu wysokosc kamery
            m_PlayerNode.Position = position + m_Pose.m_HeadPosition;

            //obracanie zgodnie z kierunkiem ruchu lub jezeli 0 to z kierunkiem goto
            if (m_MainBody.Velocity.Length < 0.1f)
            {
                m_State = CharacterState.IDLE;
            }
            else
            {
                if (m_Pose.m_name == "normal")
                {
                    if (m_MainBody.Velocity.Length > 3.5)
                        m_State = CharacterState.RUN;
                    else
                        m_State = CharacterState.WALK;
                }
                else if (m_Pose.m_name == "fly")
                {
                    m_State = CharacterState.JUMP;
                }

                if (!m_bBackward)
                {//dla wstecznego
                    Mogre.Vector3 velocityxy = m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1);
                    m_PlayerNode.Orientation = Mogre.Vector3.UNIT_Z.GetRotationTo(velocityxy);
                }
            }
        }

        public virtual void BodyForceCallback(MogreNewt.Body body, float timeStep, int threadIndex)
        {//calling each physics iteration

            //testuje jak dawno dotykal gruntu jezeli wartosc przekracza 5 lub 10 iteracji fiyzki
            //zmienia pozycje na lot
            if (m_Onground < 10)
                m_Onground++;
            else//jest w locie - zmieniamy mu stan 
                ChangePoseTo("fly");
            
            if (m_jumpLimit > 0)
                m_jumpLimit--;
            
            //obnizanie poziomu adrenaliny o staly czynnik
            if (m_adrenaline > 1)
                m_adrenaline -= 0.01f * timeStep;
            else
                m_adrenaline = 1;

            Update();

            //trying to stop body in one position
            if (!m_bBackward && !m_bForward && !m_bLeft && !m_bRight)
            {
                if (!m_MainBody.Velocity.IsZeroLength)
                {//nie zatrzymal sie jeszcze to go zatrzymujemy
                    //dla bardzo malych predksoci dac cos w stylu velocity 0 ... albo wprowadzic
                    //punkt zatrzymania (x,y,z) dla velocity < 0.00001
                    Mogre.Vector3 xyVector = new Mogre.Vector3(0, 0, 1);
                    Mogre.Vector3 velocityxy = m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1);
                    Mogre.Quaternion ForceDirection = xyVector.GetRotationTo(velocityxy);
                    Mogre.Vector3 StoppingForce = -ForceDirection * Mogre.Vector3.UNIT_Z * m_MainBody.Mass * m_Pose.m_ForceStop;
                    m_MainBody.AddForce(-StoppingForce);
                }
            }
        }

        //change position
        public void ChangePoseTo(string poseName)
        {
            if(m_Pose.m_name!=poseName)
            {
                m_myPoses[poseName].Activate();
            }
        }

        public virtual void Update()
        {
                //TURN ME LEFT!
                if (m_bLeft)
                    m_Pose.willTurnLeft();
                
                //TURN ME RIGHT! 
                if (m_bRight)m_Pose.willTurnRight();
            
                //MOVE FORWARD
                if ( m_bForward || 
                    (m_State != CharacterState.IDLE && (m_bRight==true || m_bLeft==true ))
                    )
                {//i want to go forward, can i in this position?
                    m_Pose.willForward();
                }

                //MOVE BACKWARD 
                if (m_bBackward)
                    m_Pose.willBackward();
                
                //zerowanie wektoras kretu
                if (!m_bLeft && !m_bRight && !m_bForward && !m_bBackward)
                {//jezeli nie ma przycisnietego przcyisku to keiruj sie zawsze tam gdzie wskazuje
                    //looking at czyli grafika
                    m_GoTo = m_PlayerNode.Orientation;
                }
        }

        protected virtual void initLogicStates()
        {
            m_myPoses = new Dictionary<string, LogicState>();

            m_bForward      = false;
            m_bBackward     = false;
            m_bLeft         = false;
            m_bRight        = false;
            m_bBoost        = false;
            
            //m_adrenaline
            m_adrenaline    = 1;
            //sila wyskoku
            m_jumpForce     = 6;
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
        
        public void ForwardButtonPressed()
        {
            m_bForward = true;
        }
        public void ForwardButtonReleased()
        {
            m_bForward = false;
        }

        public void BackwardButtonPressed()
        {
            m_bBackward = true;
        }
        public void BackwardButtonReleased()
        {
            m_bBackward = false;
        }

        public void LeftButtonPressed()
        {
            m_bLeft = true;
        }
        public void LeftButtonReleased()
        {
            m_bLeft = false;
        }

        public void RightButtonPressed()
        {
           m_bRight = true;
        }
        public void RightButtonReleased()
        {
            m_bRight = false;
        }

        public void BoostButtonPressed()
        {
            m_bBoost = true;
        }
        public void BoostButtonReleased()
        {
            m_bBoost = false;
        }

        public void CrouchButtonPressed()
        {
            m_Pose.willCrouch();
        }

        public void JumpButtonPressed()
        {
            m_Pose.willJump();
        }
    }
}
