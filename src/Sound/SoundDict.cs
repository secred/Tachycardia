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
        //public int m_ktoryteraz;
        public AudioContext m_AudioContext;
        public Dictionary<string, int> m_Dictionary;
        public bool m_isMuted;

        public int[] BgBuffers;
        public int BgSource;
        public int BgState;
        public int BgChannels;
        public int BgBits_per_sample;
        public int BgSample_rate;
        public int processed_count;
        public int queued_count;
        public bool IsPlaying;
        //OggVorbisFileStream oggStream;


        public SoundDict()
        {
            basename = "Media/sfx/";
            m_channels=8;
            m_ambients=1;
            m_sourceChannels = new int[m_channels];
            m_ambientChannels = new int[m_ambients];
            //m_ktoryteraz = 0;
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
            BgBuffers = AL.GenBuffers(2);
            BgSource = AL.GenSource();
            //alGenBuffers(2, buffers);
            //alGenSources(1, &source);
            IsPlaying = false;

            Initialize();
        }
        public void Initialize()
        {
            Console.WriteLine("Loading test.ogg");
            //oggStream = new OggVorbisFileStream("test.ogg");
            Console.WriteLine("Loaded test.ogg");
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
            if (plis > maxlen)return;
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
            /*if (IsPlaying)
            {
                update_ogg();
                if (!playing())
                {
                    //playback();
                    if (!playback())
                        Console.WriteLine("Ogg stopped");
                    else
                        Console.WriteLine("Ogg stream was interrupted.\n");
                }
            }*/
            Mogre.Vector3 t = Core.Singleton.m_Camera.RealPosition;
            Mogre.Vector3 w = Core.Singleton.m_Camera.Direction;
            //w.Normalise();
            Mogre.Vector3 u = Core.Singleton.m_Camera.Up;
            //u.Normalise();
            AL.Listener(ALListener3f.Position, t.x, t.y, t.z);
            float []temp = new float[6];
            temp[0] = w.x;
            temp[1] = w.y;
            temp[2] = w.z;
            temp[3] = u.x;
            temp[4] = u.y;
            temp[5] = u.z;
            AL.Listener(ALListenerfv.Orientation, ref temp);
        }

        /*public void StartOGG()
        {
            //byte[] sound_data = LoadWave(File.Open("die_02.wav", FileMode.Open), out BgChannels, out BgBits_per_sample, out BgSample_rate);
            // AL.BufferData(BgBuffer, GetSoundFormat(BgChannels, BgBits_per_sample), sound_data, sound_data.Length, BgSample_rate);
            //AL.SourceQueueBuffers(BgSource, 4, BgBuffers);
            //  AL.Source(BgSource, ALSourcei.Buffer, BgBuffer);
            // AL.SourcePlay(BgSource);
            IsPlaying = true;

            playback();
        }

        public void StopOGG()
        {
            IsPlaying = false;
            release();
        }

        public bool playback()
        {
            if (playing())
                return true;

            if (!stream(BgBuffers[0]))
                return false;

            if (!stream(BgBuffers[1]))
                return false;

            // alSourceQueueBuffers(BgSource, 2, BgBuffers);
            AL.SourceQueueBuffers(BgSource, 2, BgBuffers);
            //alSourcePlay(BgSource);
            AL.SourcePlay(BgSource);

            return true;
        }

        public bool playing()
        {
            int state;// = AL.GetSourceState(BgSource); //
            AL.GetSource(BgSource, ALGetSourcei.SourceState, out state);
            //alGetSourcei(source, AL_SOURCE_STATE, &state);

            return ((ALSourceState)state == ALSourceState.Playing);
        }

        public bool update_ogg()
        {
            int processed;
            bool active = true;

            AL.GetSource(BgSource, ALGetSourcei.SourceState, out processed);
            //int buffer;
            while ((processed--) != 0)
            {


                // alSourceUnqueueBuffers(source, 1, buffer);
                int buffer = AL.SourceUnqueueBuffer(BgSource);
                //check();

                active = stream(buffer);

                //alSourceQueueBuffers(source, 1, &buffer);
                AL.SourceQueueBuffer(BgSource, buffer);
                //check();
            }

            return active;
        }

        public bool stream(int buffer)
        {
            //int BUFFER_SIZE = (4096 * 8);
            byte[] data = new byte[32768];
            int size = 0;
            int section;
            int result;

            while (size < 32768)
            {
                //result = ov_read(&oggStream, data + size, 32768 - size, 0, 2, 1, ref section);
                result = oggStream.Read(data, size, 32768 - size);
                if (result > 0)
                    size += result;
                else
                    if (result == 0) break;
                // throw oggString(result);
                //else

            }

            if (size == 0)
                return false;

            //alBufferData(buffer, format, data, size, vorbisInfo->rate);
            AL.BufferData(buffer, ALFormat.Stereo8, data, size, oggStream.Info.Rate);

            //check();

            return false;
        }

        public void empty()
        {
            int queued;

            //alGetSourcei(source, AL_BUFFERS_QUEUED, &queued);
            AL.GetSource(BgSource, ALGetSourcei.BuffersQueued, out queued);

            while (queued-- != 0)
            {
                //int buffer;

                //alSourceUnqueueBuffers(source, 1, &buffer);
                int[] buffer = AL.SourceUnqueueBuffers(BgSource, 1);
                //check();
            }
        }

        public void release()
        {
            AL.SourceStop(BgSource);
            empty();
            AL.DeleteSources(1, ref BgSource);
            //check();
            AL.DeleteBuffers(BgBuffers);
            //ov_clear(&oggStream);
            oggStream.Flush();
        }*/
    }
}
