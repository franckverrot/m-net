/*---------------------------------------------------------------------
 *	@file:CVideo.cs
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
        /// Abstract class video
        [Serializable]
                public abstract class CVideo : CMedia 
                {
                        public CVideo()
                        {
                                //load the ID - codec name hast table
                                codecs = new System.Collections.Hashtable();
                                codecs.Add(1482049860, "DivX");
                                codecs.Add(861292868, "DivX3");
                                codecs.Add(1145656920, "XviD");
                                codecs.Add(0x2047474F, "Vorbis");
                                codecs.Add(808802372, "DivX5");
                                codecs.Add(0x3147504D, "MPEG1");
                                codecs.Add(0x3247504D, "MPEG2");
                        }

                        ~CVideo()
                        {
                                // TODO: implement
                        }

                        public CVideo(CVideo oldCVideo)
                        {
                                this.VideoCodec = oldCVideo.VideoCodec;
                                this.m_sVideoCodec = oldCVideo.m_sVideoCodec;
                                this.DimensionsX = oldCVideo.DimensionsX;
                                this.m_sDimensionsX = oldCVideo.m_sDimensionsX;
                                this.DimensionsY = oldCVideo.DimensionsY;
                                this.m_sDimensionsY = oldCVideo.m_sDimensionsY;
                                this.VideoBitRate = oldCVideo.VideoBitRate;
                                this.m_fVideoBitRate = oldCVideo.m_fVideoBitRate;
                                this.VideoDuration = oldCVideo.VideoDuration;
                                this.m_tpVideoDuration = oldCVideo.m_tpVideoDuration;
                                this.Genre = oldCVideo.Genre;
                                this.m_sGenre = oldCVideo.m_sGenre;
                        }

                        private System.Collections.ArrayList AudioFlux;

                        public System.Collections.ArrayList GetAudioFlux()
                        {
                                if (AudioFlux == null)
                                        AudioFlux = new System.Collections.ArrayList();
                                return AudioFlux;
                        }

                        public void SetAudioFlux(System.Collections.ArrayList newAudioFlux)
                        {
                                RemoveAllAudioFlux();
                                foreach (CAudio oCAudio in newAudioFlux)
                                        AddAudioFlux(oCAudio);
                        }

                        public void AddAudioFlux(CAudio newCAudio)
                        {
                                if (newCAudio == null)
                                        return;
                                if (this.AudioFlux == null)
                                        this.AudioFlux = new System.Collections.ArrayList();
                                if (!this.AudioFlux.Contains(newCAudio))
                                        this.AudioFlux.Add(newCAudio);
                        }

                        public void RemoveAudioFlux(CAudio oldCAudio)
                        {
                                if (oldCAudio == null)
                                        return;
                                if (this.AudioFlux != null)
                                        if (this.AudioFlux.Contains(oldCAudio))
                                                this.AudioFlux.Remove(oldCAudio);
                        }

                        public void RemoveAllAudioFlux()
                        {
                                if (AudioFlux != null)
                                        AudioFlux.Clear();
                        }

                        private string m_sVideoCodec = "Default Codec";
                        private int m_sDimensionsX = 0;
                        private int m_sDimensionsY = 0;
                        private float m_fVideoBitRate = 0;
                        private float m_fVideoFPS = 0;
                        private TimeSpan m_tpVideoDuration;
                        private string m_sGenre = "Default Genre";
                        protected static System.Collections.Hashtable codecs;

                        public string VideoCodec
                        {
                                get
                                {
                                        return m_sVideoCodec;
                                }
                                set
                                {
                                        if (this.m_sVideoCodec != value)
                                                this.m_sVideoCodec = value;
                                }
                        }

                        public float VideoFPS
                        {
                                get
                                {
                                        return m_fVideoFPS;
                                }
                                set
                                {
                                        if(this.m_fVideoFPS != value)
                                                this.m_fVideoFPS = value;
                                }
                        }

                        public int DimensionsX
                        {
                                get
                                {
                                        return m_sDimensionsX;
                                }
                                set
                                {
                                        if (this.m_sDimensionsX != value)
                                                this.m_sDimensionsX = value;
                                }
                        }

                        public int DimensionsY
                        {
                                get
                                {
                                        return m_sDimensionsY;
                                }
                                set
                                {
                                        if (this.m_sDimensionsY != value)
                                                this.m_sDimensionsY = value;
                                }
                        }

                        public float VideoBitRate
                        {
                                get
                                {
                                        return m_fVideoBitRate;
                                }
                                set
                                {
                                        if (this.m_fVideoBitRate != value)
                                                this.m_fVideoBitRate = value;
                                }
                        }

                        public TimeSpan VideoDuration
                        {
                                get
                                {
                                        return m_tpVideoDuration;
                                }
                                set
                                {
                                        if (this.m_tpVideoDuration != value)
                                                this.m_tpVideoDuration = value;
                                }
                        }

                        public string Genre
                        {
                                get
                                {
                                        return m_sGenre;
                                }
                                set
                                {
                                        if (this.m_sGenre != value)
                                                this.m_sGenre = value;
                                }
                        }

                        public static string [] genres = { "DivX","DivX3","XviD","Vorbis","DivX5","MPEG1","MPEG2" };
                }
}
