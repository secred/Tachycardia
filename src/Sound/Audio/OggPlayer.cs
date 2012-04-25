using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Tachycardia.Sound.BGM
{
    public enum OggPlayerStatus { Waiting = 0, Error, Stopped, Playing, Paused, Buffering }
    public enum OggPlayerStateChanger { NoChange = 0, UserRequest, EndOfFile, Error, Internal }
    public enum OggTags { Bitrate, Length }

    public struct OggBufferSegment
    {
        public byte[] Buffer;
        public int BufferLength, ReturnValue, RateHz;
    }

	public class OggPlayer
	{
        private uint[] m_Buffers;
        private int m_BufferSize;
        private OggFile m_CurrentFile;
        private uint m_Source;
        private OggPlayerStatus m_PlayerState;
        private event OggPlayerStateChangedHandler StateChanged;

        private int BufferSize { get { return m_BufferSize; } }
        private OggFile CurrentFile { get { return m_CurrentFile; } }
        private bool SetCurrentFile(string NewFilename) { return SetCurrentFile(new OggFile(NewFilename)); }
        private OggPlayerStatus PlayerState { get { return m_PlayerState; } }

        public bool SetCurrentFile(OggFile NewFile){
            if (!((m_PlayerState == OggPlayerStatus.Stopped) || (m_PlayerState == OggPlayerStatus.Waiting))) { return false; }
            m_CurrentFile = NewFile;
            StateChange(OggPlayerStatus.Stopped, OggPlayerStateChanger.UserRequest);
            return true;
        }

        public OggPlayer() {
            m_BufferSize = 4096;
            m_Buffers = new uint[32];	 
            m_PlayerState = OggPlayerStatus.Waiting;	
            InitSource();
        }

        ~OggPlayer() {
            AL.DeleteBuffers(m_Buffers);
            AL.DeleteSource(ref m_Source);
            if (m_CurrentFile != null) { m_CurrentFile.Dispose(); m_CurrentFile = null; }
        }

        private void StateChange(OggPlayerStatus NewState) { StateChange(NewState, OggPlayerStateChanger.Internal); }
        private void StateChange(OggPlayerStatus NewState, OggPlayerStateChanger Reason){
			if (StateChanged!=null) { StateChanged(this, new OggPlayerStateChangedArgs(m_PlayerState, NewState, Reason)); }
			m_PlayerState = NewState;
		}	

        public void Play() {
            if (m_PlayerState == OggPlayerStatus.Stopped)
            {
                StateChange(OggPlayerStatus.Buffering, OggPlayerStateChanger.UserRequest);

                for (int i = 0; i < m_Buffers.Length; i++)
                {
                    lock (OALLocker)
                    {
                        OggBufferSegment obs = m_CurrentFile.GetBufferSegment(0);
                        if (obs.ReturnValue > 0)
                        {
                            AL.GenBuffer(out m_Buffers[i]);
                            AL.BufferData((int)m_Buffers[i], m_CurrentFile.Format, obs.Buffer, obs.ReturnValue, obs.RateHz);
                        }
                        else
                            throw new Exception("Read error or EOF within initial buffer segment");
                    }
                }
                lock (OALLocker)
                {
                    AL.SourceQueueBuffers(m_Source, m_Buffers.Length, m_Buffers);
                    AL.SourcePlay(m_Source);
                }
                StateChange(OggPlayerStatus.Playing, OggPlayerStateChanger.UserRequest);
                new Thread(new ThreadStart(PlayThread)).Start();
                return;
            }
            else if (m_PlayerState == OggPlayerStatus.Paused) UnPause();
        }

        public void Stop() {
            if (!((m_PlayerState == OggPlayerStatus.Paused) || (m_PlayerState == OggPlayerStatus.Playing))) { return; }
            lock (OALLocker)
            {
                AL.SourceStop(m_Source);
                int nBuffers;
                AL.GetSource(m_Source, ALGetSourcei.BuffersQueued, out nBuffers);
                if (nBuffers > 0) { AL.SourceUnqueueBuffers((int)m_Source, nBuffers); }
                m_CurrentFile.ResetFile();
                for (int i = 0; i < m_Buffers.Length; i++)
                    AL.DeleteBuffer(ref m_Buffers[i]);
                m_Buffers = new uint[32];
            }
            StateChange(OggPlayerStatus.Stopped, OggPlayerStateChanger.UserRequest);
        }

        public void Pause() {
            if (!(m_PlayerState == OggPlayerStatus.Playing)) { return; }
            lock (OALLocker) { AL.SourcePause(m_Source); }
            StateChange(OggPlayerStatus.Paused, OggPlayerStateChanger.UserRequest);
        }

        private void UnPause() {
            if (!(m_PlayerState == OggPlayerStatus.Paused)) { return; }
            lock (OALLocker) { AL.SourcePlay(m_Source); }
            StateChange(OggPlayerStatus.Playing, OggPlayerStateChanger.UserRequest);
        }

		private bool InitSource() {
			try 
			{
				AL.GenSource(out m_Source);
				AL.Source(m_Source, ALSource3f.Position, 0.0f, 0.0f, 0.0f);
				AL.Source(m_Source, ALSource3f.Velocity, 0.0f, 0.0f, 0.0f);
				AL.Source(m_Source, ALSource3f.Direction, 0.0f, 0.0f, 0.0f);
				AL.Source(m_Source, ALSourcef.RolloffFactor, 0.0f);
				AL.Source(m_Source, ALSourceb.SourceRelative, true);	
				return true;
			}
			catch (Exception ex) { return false; }
		}

        private bool DestroySource() {
			try
			{
				if ( (AL.GetSourceState(m_Source) == ALSourceState.Paused) 
                    || (AL.GetSourceState(m_Source) == ALSourceState.Playing) )
					AL.SourceStop(m_Source);	
				AL.DeleteSource(ref m_Source);
				return true;
			}
			catch (Exception ex){ return false; }
		}	
		
        private static readonly object StateLocker = new object();
        private static object OALLocker = new object();

        public void Dispose() {
            this.Stop();
            AL.DeleteBuffers(m_Buffers);
            DestroySource();
            if (m_CurrentFile != null) { m_CurrentFile.Dispose(); m_CurrentFile = null; }
        }

        private void PlayThread() {
            bool Running = true; bool ReachedEOF = false; bool UnderRun = false;
            while (Running)
            {
                if (m_PlayerState == OggPlayerStatus.Playing)
                {
                    int QueuedBuffers = 0;
                    AL.GetSource(m_Source, ALGetSourcei.BuffersQueued, out QueuedBuffers);
                    if (ReachedEOF)
                    {
                        if (QueuedBuffers > 0) { }
                        else
                        {
                            lock (OALLocker)
                            {
                                Running = false;
                                if (AL.GetSourceState(m_Source) != ALSourceState.Stopped) { AL.SourceStop(m_Source); }
                                m_CurrentFile.ResetFile();

                                for (int i = 0; i < m_Buffers.Length; i++)
                                {
                                    AL.DeleteBuffer(ref m_Buffers[i]);
                                }
                                m_Buffers = new uint[32];
                            }
                            StateChange(OggPlayerStatus.Stopped, OggPlayerStateChanger.EndOfFile);
                            return;
                        }
                    }

                    if ((!ReachedEOF) && (QueuedBuffers > 0) && (AL.GetError() == ALError.NoError))
                        if (AL.GetSourceState(m_Source) != ALSourceState.Playing)
                            AL.SourcePlay(m_Source);

                    int ProcessedBuffers = 0; 
                    uint BufferRef = 0;
                    lock (OALLocker)
                    {
                        AL.GetSource(m_Source, ALGetSourcei.BuffersProcessed, out ProcessedBuffers);
                    }
                    if (ProcessedBuffers >= 32) UnderRun = true;
                    else UnderRun = false;

                    while (ProcessedBuffers > 0)
                    {
                        OggBufferSegment obs;
                        lock (OALLocker)
                        {
                            AL.SourceUnqueueBuffers(m_Source, 1, ref BufferRef);
                            if (ReachedEOF) { --ProcessedBuffers; continue; }	// If we're at the EOF loop to the next buffer here - we don't want to be trying to fill any more
                            obs = m_CurrentFile.GetBufferSegment(m_BufferSize);	// Get chunk of tasty buffer data with the configured segment
                        }

                        if (obs.ReturnValue > 0)
                        {
                            lock (OALLocker)
                            {
                                AL.BufferData((int)BufferRef, m_CurrentFile.Format, obs.Buffer, obs.ReturnValue, obs.RateHz);
                                AL.SourceQueueBuffers(m_Source, 1, ref BufferRef);
                            }
                        }
                        else
                        {
                            if (obs.ReturnValue == 0) { ReachedEOF = true; break; }
                            else
                            {
                                lock (OALLocker)
                                {
                                    m_PlayerState = OggPlayerStatus.Error;
                                    AL.SourceStop(m_Source);
                                    Running = false;
                                }
                                break;
                            }
                        }

                        if ( AL.GetError()!=ALError.NoError ) {
                            StateChange(OggPlayerStatus.Error, OggPlayerStateChanger.Error);
                            lock (OALLocker) { AL.SourceStop(m_Source); }
                            Running = false;
                            break;
                        }
                        --ProcessedBuffers;
                    }

                    if (UnderRun) { lock (OALLocker) { AL.SourcePlay(m_Source); } }
                }
                else if (m_PlayerState == OggPlayerStatus.Paused) { }
                else { Running = false; }
                Thread.Sleep(10);
            }
        }
	}
	
	public delegate void OggPlayerStateChangedHandler(object sender, OggPlayerStateChangedArgs e);
	
	public class OggPlayerStateChangedArgs
	{
		private OggPlayerStatus m_OldState, m_NewState;
		private OggPlayerStateChanger m_Changer;
		
		public OggPlayerStateChangedArgs(OggPlayerStatus eOldState, OggPlayerStatus eNewState, OggPlayerStateChanger eChanger){ m_OldState = eOldState; m_NewState = eNewState; m_Changer = eChanger; }
        public OggPlayerStateChanger Changer { get { return m_Changer; } }
        public OggPlayerStatus OldState { get { return m_OldState; } }
		public OggPlayerStatus NewState { get { return m_NewState; } }
	}
}
