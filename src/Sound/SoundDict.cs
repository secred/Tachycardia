using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using Tachycardia.Sound.BGM;
using Tachycardia;

namespace Tachycardia.Sound
{
    class SoundDict
    {
        private OggPlayer m_player;
        private float m_BGMVolume;
        private float m_Pitch;

        public bool m_IsBulletTime;

        private string basename;
        private int m_channels;
        private int m_ambients;
        private int[] m_sourceChannels;
        private int[] m_ambientChannels;
        private AudioContext m_AudioContext;
        private Dictionary<string, int> m_Dictionary;
        private bool m_isMuted;

        private int m_HeartBeatChannel;
        private int m_HeartBeatBuff;
        private float m_Pulse;
        private float m_PulseRatio;

        public SoundDict()
        {
            basename = "Media/sfx/";
            m_channels = 8;
            m_ambients = 1;
            m_sourceChannels = new int[m_channels];
            m_ambientChannels = new int[m_ambients];
            m_AudioContext = new AudioContext();
            m_Dictionary = new Dictionary<string, int>();

            // background music player
            m_player = new OggPlayer();

            m_HeartBeatChannel = AL.GenSource();

            for (int i = 0; i < m_channels; i++)
                m_sourceChannels[i] = AL.GenSource();

            for (int i = 0; i < m_ambients; i++)
                m_sourceChannels[i] = AL.GenSource();
            m_isMuted = false;

            Initialize();
        }

        private void Initialize()
        {
            m_player.SetCurrentFile("Media\\bgm\\main_menu.ogg");
            m_BGMVolume = 1.0f;
            m_Pitch = 1.0f;
            m_Pulse = 83.0f; // prawidłowo 65-85, a wg próbki 83 bpm
            m_PulseRatio = 1.0f; ; // z zakresu 1-2, bo tętno wysiłkowe może mieć ponad 150 bpm


            m_IsBulletTime = false;

            int tempbuf, channels, bits_per_sample, sample_rate;
            Stream stream = File.Open("Media\\sfx\\player\\heartbeat.wav", FileMode.Open);
            byte[] sound_data = LoadWave(stream, out channels, out bits_per_sample, out sample_rate);
            m_HeartBeatBuff = AL.GenBuffer();
            AL.BufferData(m_HeartBeatBuff, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);

            float[] temp = { 0, 0, 1, 0, 1, 0 };
            AL.Listener(ALListenerfv.Orientation, ref temp);
            AL.Listener(ALListener3f.Position, 0, 0, 0);
            Insert("die_01.wav");
            Insert("player/step_concrete_01.wav");
            Insert("player/step_concrete_02.wav");
            Insert("player/step_concrete_03.wav");
            Insert("player/step_concrete_04.wav");
            Insert("player/step_concrete_05.wav");
            Insert("player/step_gravel_01.wav");
            Insert("player/step_gravel_02.wav");
            Insert("player/step_gravel_03.wav");
            Insert("player/step_gravel_04.wav");
            Play("player/step_concrete_01.wav", new Mogre.Vector3(0, 0, 0));
        }

        private byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        private ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }

        private bool Insert(string filename)
        {
            int tempbuf;
            int channels, bits_per_sample, sample_rate;
            if (m_Dictionary.ContainsKey(filename))
            {
                Console.WriteLine("SoundDict Error. Już jest taki klucz. " + basename + filename);
                return false;
            }
            Stream stream = File.Open(basename + filename, FileMode.Open);
            if (stream == null)
            {
                Console.WriteLine("SoundDict Error. Nie ma takiego pliku: " + basename + filename);
                return false;
            }
            byte[] sound_data = LoadWave(stream, out channels, out bits_per_sample, out sample_rate);
            tempbuf = AL.GenBuffer();
            AL.BufferData(tempbuf, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);

            m_Dictionary.Add(filename, tempbuf);
            return true;
        }

        public void Play(string filename, Mogre.Vector3 pos)
        {
            if (m_isMuted) return;
            int tempbuf;
            if (!m_Dictionary.TryGetValue(filename, out tempbuf)) return;
            int state, ktoryteraz = -1;
            Mogre.Vector3 tvec, lis;
            AL.GetListener(ALListener3f.Position, out lis.x, out lis.y, out lis.z);
            float plis = (pos - lis).SquaredLength;
            float maxlen = 0.0f;
            for (int i = 0; i < m_channels; i++)
            {

                AL.GetSource(m_sourceChannels[i], ALGetSourcei.SourceState, out state);
                if ((ALSourceState)state == ALSourceState.Playing)
                {
                    //wytypuj ten najdalszy
                    AL.GetSource(m_sourceChannels[i], ALSource3f.Position, out tvec.x, out tvec.y, out tvec.z);

                    tvec = tvec - lis;
                    if (tvec.SquaredLength > maxlen)
                    {
                        maxlen = tvec.SquaredLength;
                        ktoryteraz = i;
                    }
                }
                else
                {
                    ktoryteraz = i;
                    maxlen = float.MaxValue;
                    //po porstu odtworz
                }
            }
            if (plis > maxlen) return;
            if (ktoryteraz == -1) return;
            AL.SourceStop(m_sourceChannels[ktoryteraz]);
            AL.Source(m_sourceChannels[ktoryteraz], ALSourcei.Buffer, tempbuf);
            AL.Source(m_sourceChannels[ktoryteraz], ALSource3f.Position, pos.x, pos.y, pos.z);
            AL.SourcePlay(m_sourceChannels[ktoryteraz]);
            //m_ktoryteraz++;
            //if (m_ktoryteraz >= m_channels) m_ktoryteraz = 0;
        }

        public void Update()
        {
            Mogre.Vector3 t = Core.Singleton.m_Camera.RealPosition;
            Mogre.Vector3 w = Core.Singleton.m_Camera.Direction;
            //w.Normalise();
            Mogre.Vector3 u = Core.Singleton.m_Camera.Up;
            //u.Normalise();
            AL.Listener(ALListener3f.Position, t.x, t.y, t.z);
            float[] temp = new float[6];
            temp[0] = w.x;
            temp[1] = w.y;
            temp[2] = w.z;
            temp[3] = u.x;
            temp[4] = u.y;
            temp[5] = u.z;
            AL.Listener(ALListenerfv.Orientation, ref temp);

            Character m_Player = (Character)Core.Singleton.m_ObjectManager.Find("player");
            if (m_Player.m_Control.m_bBoost && m_Pulse < 165) m_Pulse += 0.1f;
            else if (m_Pulse > 83) m_Pulse -= 0.1f;

            m_PulseRatio = 0.90f + (float)Math.Sin((m_Pulse - 83.0f) / 83.0f);

            ////////////////////////////////////////
            // obsługa stanu spowolnionego tempa gry
            if (m_IsBulletTime)
            {
                if (m_Pitch > 0.35f)
                    m_Pitch *= 0.95f;
                else
                    m_Pitch = 0.35f;
            }
            else
            {
                if (m_Pitch < 1.0f)
                    m_Pitch /= 0.95f;
                else
                    m_Pitch = 1.0f;
            }

            AL.Source(m_player.m_Source, ALSourcef.Pitch, m_Pitch);

            for (int i = 0; i < m_channels; i++)
                AL.Source(m_sourceChannels[i], ALSourcef.Pitch, m_Pitch);

            for (int i = 0; i < m_ambients; i++)
                AL.Source(m_sourceChannels[i], ALSourcef.Pitch, m_Pitch);

            AL.Source(m_HeartBeatChannel, ALSourcef.Pitch, m_Pitch * m_PulseRatio);
            ///////////////////////////////////////

            // odtwarzanie bicia serca (narazie parametr tempo zmienia pitch, 
            // a docelowo tempo ma się zmieniać bez zmiany wysokości dźwięków)
            int state;
            AL.GetSource(m_HeartBeatChannel, ALGetSourcei.SourceState, out state);
            if ((ALSourceState)state != ALSourceState.Playing)
            {

                AL.Source(m_HeartBeatChannel, ALSourcei.Buffer, m_HeartBeatBuff);
                AL.SourcePlay(m_HeartBeatChannel);
            }

            // tu będzie obsługa zapętlenia odtwarzania muzyki
            //player.Play();
        }

        /**
         * Obsługa odtwarzania OGG
        */
        public void PlayBGM() { m_player.Playback(); }
        public void StopBGM() { m_player.Stop(); }
        public void PauseBGM() { m_player.Pause(); }

        /**
         * Ustawia poziom głośności muzyki w tle na wartości z zakresu 0-1
        */
        public void BGMVolumeUp()
        {
            if (m_BGMVolume < 1.0) m_BGMVolume += 0.1f;
            AL.Source(m_player.m_Source, ALSourcef.Gain, m_BGMVolume);
        }

        public void BGMVolumeDown()
        {
            if (m_BGMVolume > 0.0) m_BGMVolume -= 0.1f;
            AL.Source(m_player.m_Source, ALSourcef.Gain, m_BGMVolume);
        }

    }


}
