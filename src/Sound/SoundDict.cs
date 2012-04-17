using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;

namespace Tachycardia.src.Sound
{
    class SoundDict
    {
        //static readonly string filename = Path.Combine(Path.Combine("Data", "Audio"), "the_ring_that_fell.wav");
        public string basename;
        public int m_channels;
        public int m_ambients;
        public int[] m_sourceChannels;
        public int[] m_ambientChannels;
        public int m_ktoryteraz;
        public AudioContext m_AudioContext;
        public Dictionary<string, int> m_Dictionary;
        public bool m_isMuted;

        public SoundDict()
        {
            basename = "Media/sfx/";
            int m_channels=8;
            int m_ambients=8;
            m_sourceChannels = new int[m_channels];
            m_ambientChannels = new int[m_ambients];
            m_ktoryteraz = 0;
            m_AudioContext = new AudioContext();
            m_Dictionary = new Dictionary<string, int>();
            for (int i = 0; i < m_channels; i++)
            {
                m_sourceChannels[i] = AL.GenSource();
            }
            for (int i = 0; i < m_ambients; i++)
            {
                m_sourceChannels[i] = AL.GenSource();
            }
            m_isMuted = false;
            Initialize();
        }
        public void Initialize()
        {
            Insert("die_01.wav");
            Insert("player/step_concrete_01.wav");
            Insert("player/step_concrete_02.wav");
            Insert("player/step_concrete_03.wav");
            Insert("player/step_concrete_04.wav");
            Insert("player/step_concrete_05.wav");
            Play("die_01.wav", new Mogre.Vector3(0,0,0));
        }
        public byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
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
        public ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
        /*public static void costam()
        {
            //using (AudioContext context = new AudioContext())
            {
                int state;
                m_AudioContext = new AudioContext();
                m_buffer = AL.GenBuffer();
                m_source = AL.GenSource();

                int channels, bits_per_sample, sample_rate;
                byte[] sound_data = LoadWave(File.Open(filename, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
                AL.BufferData(m_buffer, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);
                AL.Source(m_source, ALSourcei.Buffer, m_buffer);
                AL.SourcePlay(m_source);
                //Console.Write("UUUOOOEYYYYRBleybleybleyble");
                // Query the source to find out when it stops playing.
                /*do
                {
                    //Thread.Sleep(250);
                    //Trace.Write(".");
                    AL.GetSource(source, ALGetSourcei.SourceState, out state);
                }
                while ((ALSourceState)state == ALSourceState.Playing);*
                //System.Threading.Thread.Sleep(500);
                //AL.SourceStop(source);
                //AL.DeleteSource(source);
                //AL.DeleteBuffer(buffer);
            }
        }*/
        public bool Insert(string filename)
        {
            int tempbuf;
            int channels, bits_per_sample, sample_rate;
            if(m_Dictionary.ContainsKey(filename))
            {
                Console.WriteLine("SoundDict Error. Już jest taki klucz. "+basename+filename);
                return false;
            }
            Stream stream = File.Open(basename+filename, FileMode.Open);
            if (stream == null)
            {
                Console.WriteLine("SoundDict Error. Nie ma takiego pliku: "+basename+filename);
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
            AL.SourceStop(m_sourceChannels[m_ktoryteraz]);
            AL.Source(m_sourceChannels[m_ktoryteraz], ALSourcei.Buffer, tempbuf);
            AL.Source(m_sourceChannels[m_ktoryteraz], ALSource3f.Position, pos.x, pos.y, pos.z);
            AL.SourcePlay(m_sourceChannels[m_ktoryteraz]);
            m_ktoryteraz++;
            if (m_ktoryteraz >= m_channels) m_ktoryteraz = 0;
        }

        public void Update()
        {
            /*
             * TODO: streamowanie oggow bedzie potrzebowalo update'a.
             */
            AL.Listener(ALListener3f.Position, Core.Singleton.m_Camera.Position.x, Core.Singleton.m_Camera.Position.y, Core.Singleton.m_Camera.Position.z);
            float []temp = new float[6];
            temp[0] = Core.Singleton.m_Camera.Orientation.x;
            temp[1] = Core.Singleton.m_Camera.Orientation.y;
            temp[2] = Core.Singleton.m_Camera.Orientation.z;
            temp[3] = Core.Singleton.m_Camera.Up.x;
            temp[4] = Core.Singleton.m_Camera.Up.y;
            temp[5] = Core.Singleton.m_Camera.Up.z;
            AL.Listener(ALListenerfv.Orientation, ref temp);
        }
    }
}
