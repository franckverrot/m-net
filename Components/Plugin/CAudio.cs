/*---------------------------------------------------------------------
 *	@file:CAudio.cs
 *---------------------------------------------------------------------
 * Copyright (C) 2004  Franck Verrot
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *---------------------------------------------------------------------*/
using System;

namespace MediaNET.Components
{
	[Serializable]
	public abstract class CAudio : CMedia //, ICPlay
	{
		public CAudio()
		{

		}
      
		~CAudio()
		{
		
		}
      
		public CAudio(CAudio oldCAudio)
		{
			this.AudioCodec = oldCAudio.AudioCodec;
			this.m_iAudioCodec = oldCAudio.m_iAudioCodec;
			this.AudioBitRate = oldCAudio.AudioBitRate;
			this.m_fAudioBitRate = oldCAudio.m_fAudioBitRate;
			this.Duration = oldCAudio.Duration;
			this.m_tpDuration = oldCAudio.m_tpDuration;
		}
      
		protected int m_iAudioCodec = 0;
		protected float m_fAudioBitRate = 0;
		protected TimeSpan m_tpDuration;
		protected int m_nbOfChannels = 0;
   
		public int AudioCodec
		{
			get
			{
				return m_iAudioCodec;
			}
			set
			{
				if (this.m_iAudioCodec != value)
					this.m_iAudioCodec = value;
			}
		}
      
		public float AudioBitRate
		{
			get
			{
				return m_fAudioBitRate;
			}
			set
			{
				if (this.m_fAudioBitRate != value)
					this.m_fAudioBitRate = value;
			}
		}
      
		public int nbOfChannels
		{
			get
			{
				return m_nbOfChannels;
			}
			set
			{
				if(this.m_nbOfChannels != value)
					this.m_nbOfChannels = value;
			}
		}
      
		public TimeSpan Duration
		{
			get
			{
				return m_tpDuration;
			}
			set
			{
				if (this.m_tpDuration != value)
					this.m_tpDuration = value;
			}
		}
   
		public static string [] genres = { 
											 "Blues", "Classic Rock", "Country", "Dance", "Disco", "Funk", 
											 "Grunge", "Hip-Hop", "Jazz", "Metal", "New Age", "Oldies", "Other", 
											 "Pop", "R&B", "Rap", "Reggae", "Rock", "Techno", "Industrial", 
											 "Alternative", "Ska", "Death Metal", "Pranks", "Soundtrack", 
											 "Euro-Techno", "Ambient", "Trip-Hop", "Vocal", "Jazz+Funk", "Fusion", 
											 "Trance", "Classical", "Instrumental", "Acid", "House", "Game", 
											 "Sound Clip", "Gospel", "Noise", "Alt. Rock", "Bass", "Soul", 
											 "Punk", "Space", "Meditative", "Instrum. Pop", "Instrum. Rock", 
											 "Ethnic", "Gothic", "Darkwave", "Techno-Indust.", "Electronic", 
											 "Pop-Folk", "Eurodance", "Dream", "Southern Rock", "Comedy", 
											 "Cult", "Gangsta", "Top 40", "Christian Rap", "Pop/Funk", "Jungle", 
											 "Native American", "Cabaret", "New Wave", "Psychadelic", "Rave", 
											 "Showtunes", "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz", 
											 "Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock", "Folk", 
											 "Folk/Rock", "National Folk", "Swing", "Fusion", "Bebob", "Latin", 
											 "Revival", "Celtic", "Bluegrass", "Avantgarde", "Gothic Rock", 
											 "Progress. Rock", "Psychadel. Rock", "Symphonic Rock", "Slow Rock", 
											 "Big Band", "Chorus", "Easy Listening", "Acoustic", "Humour", 
											 "Speech", "Chanson", "Opera", "Chamber Music", "Sonata", "Symphony", 
											 "Booty Bass", "Primus", "Porn Groove", "Satire", "Slow Jam", 
											 "Club", "Tango", "Samba", "Folklore", "Ballad", "Power Ballad", 
											 "Rhythmic Soul", "Freestyle", "Duet", "Punk Rock", "Drum Solo", 
											 "A Capella", "Euro-House", "Dance Hall", "Goa", "Drum & Bass", 
											 "Club-House", "Hardcore", "Terror", "Indie", "BritPop", "Negerpunk", 
											 "Polsk Punk", "Beat", "Christian Gangsta Rap", "Heavy Metal", 
											 "Black Metal", "Crossover", "Contemporary Christian", "Christian Rock",
											 "Merengue", "Salsa", "Thrash Metal", "Anime", "Jpop", "Synthpop" 
										 };

	}
}
