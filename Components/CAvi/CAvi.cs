/*---------------------------------------------------------------------
 *	@file:CAvi.cs
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
using System.Text;
using System.IO;

namespace MediaNET.Components
{
        [Serializable]
                public class CAvi : CVideo
                {               
                        static string m_sTypeIdentifier;
                        static string m_sPlayer = "mplayer";
                        static string m_sExtension = ".avi";
                        static string m_sDescription = "AVI Video plugin!";

                        public CAvi() : this("")
                        {
                        }

                        public CAvi(string fullname)
                        {
                                m_sPath = fullname;
                                m_sTypeIdentifier="AVI Video File";
                                try
                                {
                                        Path = System.IO.Path.GetFullPath(fullname);
                                        Title = System.IO.Path.GetFileName(Path);
                                }
                                catch(Exception e)
                                {
                                        //DO JUST NOTHING                                        
                                        //Console.WriteLine("CAvi::CAvi error: " + e.Message);
                                }
                                debug = false;
                        }


                        public static string Extension
                        {
                                get { return m_sExtension; }
                                set { m_sExtension = value; }
                        }

                        public static string Player
                        {
                                get { return m_sPlayer; }
                                set { m_sPlayer = value; }
                        }
                                
                        public static string TypeIdentifier
                        {
                                get { return m_sTypeIdentifier; }
                                set { m_sTypeIdentifier = value; }
                        }

                        public override Array Summary 
                        {
                                get 
                                {
                                        string[] s = {Title,Author, TypeIdentifier ,VideoDuration.ToString(),VideoFPS.ToString(), GID};
                                        return s;
                                }
                        }
                        /**
                         * Parse the current file and load its characteristics.
                         */
                        public override void loadFileCharacteristics()
                        {
                                byte [] buffer;
                                uint pos;
                                int c;
                                int size;
                                int fcc=0;
                                uint a_c=0;
                                uint v_c=0;
                                int type=0;
                                int scale=0;
                                int rate=0;
                                int length=0;
                                uint cont=1;
                                double fps=0;
                                int readed_bytes=0;


                                Encoding asciiEncoding = new ASCIIEncoding();
                                CAudio audioTrack = null;
                                Byte[] scan;
                                //int i;
                                //byte octet;
                                FileStream fileStream;


                                //Read some bytes from the AVI file
                                scan = new byte[15];
                                try
                                {
                                        fileStream = new FileStream(CompleteFileName , FileMode.Open, FileAccess.Read);
                                        readed_bytes = fileStream.Read(scan,0, 12);
                                }
                                catch(Exception e)
                                {
                                        Console.WriteLine("ERROR exception : "+e.Message);
                                        return;
                                }

                                //Testing if the file is really an AVI file
                                if(!compareString(scan, avi_signature))
                                        return;


                                //Looking for AVI data
                                while(cont != 0)
                                {
                                        readed_bytes = fileStream.Read(scan,0, 12);
                                        if(readed_bytes == 0)
                                                break;
                                        b2DW(out size,scan,4);
                                        if(size<4) 
                                                size=4;
                                        for(c = 0; c < 4; c++)
                                                if(compareString(scan, patterns[c]))
                                                        break;
                                        switch(c)
                                        {
                                                case 0:
                                                        //header reading
                                                        if(size > max_allowed_avi_header_size)
                                                                return; //header too big
                                                        buffer = new byte[size + safe_pad - 4 + 10000];
                                                        readed_bytes = fileStream.Read(buffer, 0, size - 4 + 10000);
                                                        if(readed_bytes == 0)
                                                                break;
                                                        pos = 76;
                                                        b2DWi(out fcc,buffer,pos);
                                                        while(pos < size)
                                                        {
                                                                switch(fcc) 
                                                                {
                                                                        case (int)fourcc.FOURCC_strh:
                                                                                b2DW(out type,buffer,pos+8);
                                                                                b2DW(out scale,buffer,pos+28);
                                                                                b2DW(out rate,buffer,pos+32);
                                                                                b2DW(out length,buffer,pos+40);
                                                                                fps=(double)rate/(double)(scale != 0?scale:0x7FFFFFFF);
                                                                                length=(int)(length/fps);
                                                                                pos+=64;
                                                                                fcc=0;
                                                                                break;
                                                                        case (int)fourcc.FOURCC_strf:
                                                                                switch(type)
                                                                                {
                                                                                        //Found the video
                                                                                        case (int)fourcc.FOURCC_vids:
                                                                                                VideoFPS = (float)fps;
                                                                                                VideoDuration = new TimeSpan(0, 0, length);
                                                                                                VideoCodec = (string)codecs[b2DWv(buffer,pos+24)];
                                                                                                DimensionsX = b2DWv(buffer,pos+12);
                                                                                                DimensionsY = b2DWv(buffer,pos+16);
                                                                                                if(debug == true)
                                                                                                {
                                                                                                        Console.WriteLine("Scale: " + scale);
                                                                                                        Console.WriteLine("Rate: " + rate);
                                                                                                        Console.WriteLine("Length: " + length + " s");
                                                                                                        Console.WriteLine("FPS: " + fps);
                                                                                                        Console.WriteLine("X: " + b2DWv(buffer,pos+12));
                                                                                                        Console.WriteLine("Y:" + b2DWv( buffer,pos+16));
                                                                                                        Console.WriteLine("Codec: " + VideoCodec);
                                                                                                }
                                                                                                //pos+=40;
                                                                                                //ARGGGHHH, gros bidouillage!!
                                                                                                try
                                                                                                {
                                                                                                        pos +=  System.UInt32.Parse(((int)(0x1074 - 24 + size - 8830)).ToString());
                                                                                                }
                                                                                                catch(Exception e)
                                                                                                {
                                                                                                        Console.WriteLine("CAvi::LoadFileCharacteristics() - parse error " + e.Message);
                                                                                                        return;
                                                                                                }
                                                                                                if(pos > size + safe_pad - 4 + 10000)
                                                                                                {
                                                                                                        size = 0;
                                                                                                        pos = 0;
                                                                                                }

                                                                                                v_c++;
                                                                                                break;
                                                                                        case (int)fourcc.FOURCC_auds:
                                                                                                //Found the audio
                                                                                                //TODO: find the codec!! Phoque!
                                                                                                //if(codec == "mp3")
                                                                                                audioTrack = new CMp3();
                                                                                                audioTrack.AudioBitRate = rate;
                                                                                                audioTrack.nbOfChannels = b2Wv(buffer,pos+10);
                                                                                                audioTrack.Duration = new TimeSpan(0, 0, length);

                                                                                                if(debug == true)
                                                                                                {
                                                                                                        Console.WriteLine("rate: " + rate + "Hz");
                                                                                                        Console.WriteLine("Speakers: "+ b2Wv(buffer,pos+10));
                                                                                                        //TODO : find this gayzou!
                                                                                                        //Console.WriteLine("Codec: "+ b2Wv(buffer,pos+10));
                                                                                                }

                                                                                                AddAudioFlux(audioTrack);
                                                                                                a_c++;
                                                                                                size = 0;
                                                                                                break;
                                                                                }
                                                                                fcc=0;
                                                                                break;
                                                                }
                                                                //Damn it!!!
                                                                //INSb(ref fcc,buffer,pos);
                                                                b2DWi(out fcc,buffer,pos);
                                                        }
                                                        continue;
                                                case 1:
                                                        //INFO reading
                                                        /** Let's do it later...*/
                                                case 3:
                                                        //idx1 processing
                                                        /**TODO ?? */
                                                        cont=0;
                                                        break;
                                        }
                                        fileStream.Seek(size-4, SeekOrigin.Current);
                                }
                                fileStream.Close();              
                        }

                        /**
                         * Save the current file and its characteristics.
                         */
                        public override void saveFileCharacteristics()
                        {
                                //TODO
                        }


                        private void b2DW(out int value, byte[] buffer, uint offset)
                        {
                                value=buffer[offset]+(buffer[offset+1]<<8)+(buffer[offset+2]<<16)+(buffer[offset+3]<<24);
                        }

                        private int b2DWv(byte[] buffer, uint offset)
                        {
                                return buffer[offset]+(buffer[offset+1]<<8)+(buffer[offset+2]<<16)+(buffer[offset+3]<<24);
                        }

                        private int b2Wv(byte[] buffer, uint offset)
                        {
                                return buffer[offset]+(buffer[offset+1]<<8);
                        }

                        private void b2DWi(out int value, byte[] buffer, uint offset) 
                        {
                                value=buffer[offset++];
                                value+=buffer[offset++]<<8;
                                value+=buffer[offset++]<<16;
                                value+=buffer[offset++]<<24;
                        }

                        private void INSb(ref int value, byte[] buffer, uint offset)
                        {
                                value = ((value>>8)+((buffer[offset++])<<24));
                        }

                        /**
                         * @return Returns true if the chars in str == the ones in pattern, if the ones in pattern != 0
                         */
                        private bool compareString(byte [] str, string pattern)
                        {
                                int i;
                                for(i = 0; i < 12; i++)
                                {
                                        if(pattern[i] != '\0' && pattern[i] != str[i])
                                                return false;
                                }
                                return true;
                        }

                        public override Gtk.Widget Display
                        {
                                get
                                {
                                        try {
                                                Glade.XML m_xXML = new Glade.XML(null,"cavi.glade","ShowWindow",null);
                                                CAudio audio = GetAudioFlux()[0] as CAudio;
                                                ((Gtk.Label)m_xXML["Debit"]).Text += audio.AudioBitRate;
                                                ((Gtk.Label)m_xXML["Freq"]).Text += "?";
                                                ((Gtk.Label)m_xXML["Canaux"]).Text += audio.nbOfChannels;
                                                ((Gtk.Label)m_xXML["Duree"]).Text += audio.Duration;
                                                ((Gtk.Label)m_xXML["Codec"]).Text += VideoCodec;
                                                ((Gtk.Label)m_xXML["FPS"]).Text += VideoFPS.ToString();
                                                ((Gtk.Label)m_xXML["Dimensions"]).Text += DimensionsX.ToString() + "*" + DimensionsY.ToString();
                                                ((Gtk.Label)m_xXML["Duree2"]).Text += VideoDuration.ToString();
                                                ((Gtk.Label)m_xXML["Bitrate"]).Text += "?";
                                                return m_xXML.GetWidget("showFrame") as Gtk.Widget;
                                        }
                                        catch(Exception e) 
                                        {
                                                Console.WriteLine("CAvi exception: "+e.Message);
                                        }
                                        return null;
                                }
                        }
                        public override Gtk.Widget Edit
                        {
                                get
                                {
                                        try {
                                                Glade.XML xml = new Glade.XML(null,"cavi.glade","EditWindow",null);
                                                return xml.GetWidget("editFrame") as Gtk.Widget;
                                        }
                                        catch(Exception e) 
                                        {
                                                Console.WriteLine("CAvi exception: "+e.Message);
                                        }
                                        return null;
                                }
                                set {}
                        }


                        private static string [] patterns =
                        {
                                "LIST\0\0\0\0hdrl",
                                "LIST\0\0\0\0INFO",
                                "LIST\0\0\0\0movi",
                                "idx1\0\0\0\0\0\0\0\0"
                        };

                        private static string avi_signature = "RIFF\0\0\0\0AVI\0";

                        internal enum fourcc
                        {
                                FOURCC_strl=0x6C727473,
                                FOURCC_strh=0x68727473,
                                FOURCC_strf=0x66727473,
                                FOURCC_vids=0x73646976,
                                FOURCC_auds=0x73647561
                        }

                        private bool debug;
                        private static int max_allowed_avi_header_size = 131072;
                        private static int safe_pad = 128;
                        //private static int frame_size = 65536;

                        static public string Description
                        {
                                get
                                {
                                        return m_sDescription;
                                }
                        }
                }
}
