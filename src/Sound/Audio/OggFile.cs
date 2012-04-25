// 
//  OggFile.cs
//  
//  Author:
//       dragon@the-dragons-nest.co.uk
// 
//  Copyright (c) 2010 Matthew Harris
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Collections.Generic;
using csvorbis;
using TagLib;
using OpenTK.Audio.OpenAL;

namespace Tachycardia.Sound.BGM
{
	/*
	 *	OggFile Class
	 *	Combines the csvorbis, System.IO and Taglib functionality into one class
	 *	Designed for use with OggPlayer or OggPlaylist
	 */

	public class OggFile : IDisposable 
	{
		
		private string m_Filename;			// Filename
		
		private VorbisFile m_CSVorbisFile; 	// CSVorbis file object
		private TagLib.File m_TagLibFile;	// TagLibSharp file object
		
		private int m_Streams;				// Number of Vorbis streams in the file
		private int m_Bitrate;				// ABR/NBR of the file
		private int m_LengthTime;			// Number of seconds in the file
		private Info[] m_Info;				// OggVorbis file info object					
		private ALFormat m_Format;			// Format of the file
		
		private const int _BIGENDIANREADMODE = 0;		// Big Endian config for read operation: 0=LSB;1=MSB
		private const int _WORDREADMODE = 1;			// Word config for read operation: 1=Byte;2=16-bit Short
		private const int _SGNEDREADMODE = 0;			// Signed/Unsigned indicator for read operation: 0=Unsigned;1=Signed
		private const int _SEGMENTLENGTH = 4096;		// Default number of segments to read if unspecified (Segment type is determined by _WORDREADMODE)
		
		public ALFormat Format { get { return m_Format; } }
		
		public OggFile (string Filename)
		{
			// Check that the file exists
            if (!(System.IO.File.Exists(Filename))) { System.Console.WriteLine("File not found", Filename); }
			// Load the relevant objects
			m_Filename = Filename;
			try
			{
				m_CSVorbisFile = new VorbisFile(m_Filename);
			}
			catch (Exception ex)
			{
                System.Console.WriteLine("Unable to open file for data reading\n" + ex.Message, Filename);
			}
			try
			{
				m_TagLibFile = TagLib.File.Create(m_Filename);
			}
			catch (Exception ex)
			{
                System.Console.WriteLine("Unsupported format (not an ogg?)\n" + ex.Message, Filename);
			}

			
			// Populate some other info shizzle and do a little bit of sanity checking
			m_Streams = m_CSVorbisFile.streams();
            if (m_Streams <= 0) { System.Console.WriteLine("File doesn't contain any logical bitstreams", Filename); }
			// Assuming <0 is for whole file and >=0 is for specific logical bitstreams
			m_Bitrate = m_CSVorbisFile.bitrate(-1);
			m_LengthTime = (int)m_CSVorbisFile.time_total(-1);
			// Figure out the ALFormat of the stream
			m_Info = m_CSVorbisFile.getInfo();	// Get the info of the first stream, assuming all streams are the same? Dunno if this is safe tbh
            if (m_Info[0] == null) { System.Console.WriteLine("Unable to determine Format{FileInfo.Channels} for first bitstream", Filename); }
			if (m_TagLibFile.Properties.AudioBitrate==16) {
				m_Format = (m_Info[0].channels)==1 ? ALFormat.Mono16 : ALFormat.Stereo16; // This looks like a fudge, but I've seen it a couple of times (what about the other formats I wonder?)
			}
			else 
			{
				m_Format = (m_Info[0].channels)==1 ? ALFormat.Mono8 : ALFormat.Stereo8;
			}
		}

        public string GetQuickTag(OggTags TagID)
        {
            switch (TagID)
            {
                case OggTags.Bitrate: return m_Bitrate.ToString();
                case OggTags.Length: return m_LengthTime.ToString();
                default: return null;
            }

        }
		
		
		public OggBufferSegment GetBufferSegment(int SegmentLength)
		{
			if (SegmentLength<=0) { SegmentLength = _SEGMENTLENGTH; }	// If segment length is invalid, use default segment length
			OggBufferSegment retVal; // Declare the buffer segment structure
			retVal.BufferLength = SegmentLength;
			retVal.Buffer = new Byte[retVal.BufferLength];	// Init buffer
			retVal.ReturnValue = m_CSVorbisFile.read(retVal.Buffer, retVal.BufferLength, _BIGENDIANREADMODE, _WORDREADMODE, _SGNEDREADMODE, null);
			retVal.RateHz = m_TagLibFile.Properties.AudioSampleRate; //m_Info[0].rate;
			return retVal;
		}
			
		public void ResetFile()
		{
			try
			{
				m_CSVorbisFile = null;
				m_CSVorbisFile = new VorbisFile(m_Filename);	// No point reloading anything else 'cos it shouldn't have changed	
				m_TagLibFile = null;
				m_TagLibFile = TagLib.File.Create(m_Filename);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to reload OggFile [" + m_Filename + "]", ex);	
			}
		}
		
		public float GetTime()
		{
			return m_CSVorbisFile.time_tell();
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			m_TagLibFile.Dispose();
			m_TagLibFile = null;
			m_CSVorbisFile.Dispose();
			m_CSVorbisFile = null;
		}
		
		#endregion
	}

}
