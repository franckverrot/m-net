/*---------------------------------------------------------------------
 *	@file:CMkv.cs
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
using System.IO;
using System.Text;

namespace MediaNET.Components
{
   [Serializable]
   public class CMkv : CVideo
   {
        public CMkv() : this("")
        {
        }
        
        ~CMkv()
        {
                // TODO: implement
        }
        
        /**static void Main(string[] args)
        {
                CMkv boo = new CMkv("/home/ec4/src/MediaNET/MediaNET/src/fileType", args[0]);
                boo.loadFileCharacteristics();
        }*/
        
        public CMkv(string fullname)
        {
                m_sPath     = fullname;
                debug = false;

        }
        
        public override void Play()
        {
                // TODO: implement
        }
        
        public override void Stop()
        {
                // TODO: implement
        }
   
      /**
       * Parse the current file and load its characteristics.
       */
        public override void loadFileCharacteristics()
        {
                FileStream fileStream;
                byte[] buffer;
                int offs;
                Encoding  asciiEncoding = new ASCIIEncoding();
                CAudio audioTrack = null;
                string tmpCodec;
                
                int p; //pointer in buffer
                int c;//,c2; //counter in some loops
                int readed_bytes;
                uint eID; //element ID
                Int64 eSize; //Size of element content
                
                //int DefaultDuration;
                int TrackType=0;
                int pvt_look_flag=0;
                int curr_c=-1;
                int a_c=-1;
                int v_c=-1;
                int t_c=-1;
                int value=0;

                //Read some bytes from the MKV file
                buffer = new byte[buffer_size + 128];
                try
                {
                        fileStream = new FileStream(CompleteFileName , FileMode.Open, FileAccess.Read);
                        readed_bytes = fileStream.Read(buffer,1, buffer_size);
                }
                catch(Exception e)
                {
                        Console.WriteLine("ERROR exception : "+e.Message);
                        return;
                }
                fileStream.Close();
                
                p = 0;
                while(buffer[p] != (int)type.MKVID_FILE_BEGIN)
                {
                        p++;
                        if(p >= readed_bytes)
                                return;
                }
        
                do{
                        //Try to read an MKV Element
                        offs=ElementRead(buffer,p,readed_bytes, out eID, out eSize);
                        p+=offs;
                        
                        if(offs == 0 || p>=readed_bytes) break;
                        
                        for(c=0;c<6;c++)
                                if(MKV_Parse_list[c]==eID) {
                                        break;
                        }
                        if(c<6) continue;
                        if(p+eSize>readed_bytes) break; 
                        if(eSize==4||eSize==8||eSize==1||eSize==2)
                                value=(int)GetInt(buffer,p,(int)eSize);
                        switch(eID){
                                case (int)type.MKVID_TrackType: //detect a stream type (video/audio/text)
                                        TrackType=value;
                                        pvt_look_flag=0;
                                        switch(TrackType){
                                                case (int)medias.MKV_Track_video: 
                                                        v_c++;
                                                        curr_c=v_c;
                                                        break;
                                                case (int)medias.MKV_Track_audio: 
                                                        a_c++;
                                                        curr_c=a_c;
                                                        break;
                                                case (int)medias.MKV_Track_subtitle_orig: 
                                                        t_c++;
                                                        TrackType=(int)medias.MKV_Track_subtitle; 
                                                        curr_c=t_c;
                                                        break;
                                        }
                                        break;

                                /**
                                //Language
                                case (int)type.MKVID_Language: //stream language
                                        if(curr_c>=0&&TrackType<4&&eSize<max_string_size)
                                                Console.WriteLine(asciiEncoding.GetString(buffer, p, (int)eSize));
                                        break;*/
                                //Found the codec name
                                case (int)type.MKVID_CodecName:
                                case (int)type.MKVID_CodecID:
                                        if(curr_c>=0&&TrackType<4&&eSize<max_string_size){
                                                if(TrackType == (int)medias.MKV_Track_video)
                                                {
                                                        VideoCodec = asciiEncoding.GetString(buffer, p, (int)eSize);
                                                        if(debug && VideoCodec != "V_MS/VFW/FOURCC")
                                                                Console.WriteLine("Video Codec : " + VideoCodec);
                                                        if(VideoCodec == "V_MS/VFW/FOURCC")
                                                                pvt_look_flag=1;
                                                }
                                                else if(TrackType == (int)medias.MKV_Track_audio)
                                                {
                                                        tmpCodec = asciiEncoding.GetString(buffer, p, (int)eSize);
                                                        if(tmpCodec == "A_MPEG/L3" || tmpCodec.ToLower() == "mp3")
                                                                audioTrack = new CMp3();
                                                        //else if(tmpCodec.ToLower().IndexOf("ogg") != -1)
                                                        //       audioTrack = new COgg();
                                                        if(debug)
                                                                Console.WriteLine("Audio Codec : " +  tmpCodec);
                                     
                                                }
                                        }
                                        break;
                                case (int)type.MKVID_CodecPrivate:
                                        if(pvt_look_flag == 1 && v_c>=0 && eSize>=24){
                                                pvt_look_flag=0;
                                                int aa = (buffer[p+16]<<24)+(buffer[p+17]<<16)+(buffer[p+18]<<8)+buffer[p+19];
                                                VideoCodec = (string)codecs[aa];
                                                if(debug)
                                                        Console.WriteLine("Video Codec : " + VideoCodec);
                                        }
                                        break;
                                //Found the width
                                case (int)type.MKVID_PixelWidth: 
                                case (int)type.MKVID_DisplayWidth:
                                        if(v_c >= 0)
                                        {
                                                DimensionsX = value;
                                                if(debug)
                                                        Console.WriteLine("X : " + DimensionsX);
                                        }
                                        break;
                                //Found the height
                                case (int)type.MKVID_PixelHeight: 
                                case (int)type.MKVID_DisplayHeight:
                                        if(v_c >= 0)
                                        {
                                                DimensionsY = value;
                                                if(debug)
                                                        Console.WriteLine("Y : " + DimensionsY);
                                        }
                                        break;
                                //Found the audio frequency
                                case (int)type.MKVID_OutputSamplingFrequency:
                                case (int)type.MKVID_SamplingFrequency:
                                        if(a_c>=0)
                                        {
                                          //TODO : fix GetFloat
                                          //Console.WriteLine(GetFloat(buffer,p,(int)eSize));
                                        }
                                        break;
                                //Found the number of channels
                                case (int)type.MKVID_Channels:
                                        if(a_c>=0 && audioTrack != null)
                                        {
                                                audioTrack.nbOfChannels = value;
                                                if(debug)
                                                        Console.WriteLine("Nb of channels : " + value);
                                        }
                                        break;
                                //Found the title
                                case (int)type.MKVID_Title: 
                                        if(eSize>max_string_size)
                                                break;
                                        Title = asciiEncoding.GetString(buffer, p, (int)eSize);
                                        if(debug)
                                                Console.WriteLine("titre : " + Title);
                                        break;
                        }
                        p+=(int)eSize;
                }while(true);
                
                //add the audioTrack into the current instance
                //TODO: support several audio tracks
                AddAudioFlux(audioTrack);
        }
        
        /**
        * Save the current file and its characteristics.
        */
        public override void saveFileCharacteristics()
        {
                //TODO
                //too hard :/
        }
        
        private int VINTparse(byte[] buffer, int start, int end, out Int64 result, int flag)
        {
                byte[] mask={0x80,0x40,0x20,0x10,0x8,0x4,0x2,0x1};
                byte[] imask={0x7F,0x3F,0x1F,0xF,0x7,0x3,0x1,00};
                int VINT_WIDTH;	
                int c;
                if(end-start<2)
                {
                        Console.WriteLine("debug: oops, out of buffer(" + start + ")\n");
                        result = 0;
                        return 0; /*boo*/
                }
                VINT_WIDTH=0;
                for(c=0;c<8;c++)
                { 
                        if((buffer[start]&mask[c]) == 0x00)
                                VINT_WIDTH++; 
                        else
                                break;
                }
                if(VINT_WIDTH>=8 || VINT_WIDTH+start+1>=end) 
                {
                        Console.WriteLine("debug: oops2\n");
                        result = 0;
                        return 0;
                }
                result=0;
                for(c=0;c<VINT_WIDTH;c++)
                        result+=buffer[start+VINT_WIDTH-c]<<(c*8);
                if(flag != 0)
                        result+=(buffer[start]&imask[VINT_WIDTH])<<(VINT_WIDTH*8);
                else 
                        result+=(buffer[start])<<(VINT_WIDTH*8);
                return VINT_WIDTH+1;
        }
        
        /**
         * Read an MKV element.
         */
        private int ElementRead(byte[] buffer, int start, int end, out uint ID, out Int64 size)
        {
                Int64 tempID;
                Int64 tempsize;
                int ID_offset,size_offset;
                ID_offset=VINTparse(buffer,start,end, out tempID,0);
                if(ID_offset == 0)
                {
                        ID = 0;
                        size = 0;
                        return 0;
                }
                size_offset=VINTparse(buffer,start+ID_offset,end, out tempsize,1);
                if(size_offset == 0)
                {
                        ID = 0;
                        size = 0;
                        return 0;
                }
                ID=(uint)tempID; /*id must be <4 and must to feet in uint*/
                size=tempsize;
                return ID_offset+size_offset;
        }

        /**
         * Get a integer from a byte array from start to start+size
         */
        private Int64 GetInt(byte[] buffer, int start, int size)
        {
                Int64 result=0;
                int c;
                for(c=1;c<=size;c++){
                        result+=buffer[start+c-1]<<(8*(size-c));
                }
                return result;
        }
        
        /**
         * Get a float from a byte array from start to start+size
         */
        private float GetFloat(byte[] buffer, int start, int size){
                float result=0;
                byte[] tmp = new byte[4];
                if(size == 4)
                {
                        tmp[0]=buffer[start+3];
                        tmp[1]=buffer[start+2];
                        tmp[2]=buffer[start+1];
                        tmp[3]=buffer[start];
                        //FIXME : convert byte[] into a float !!
                        //result= Convert.ChangeType(tmp, typeof(Int32));
                }
                return result;
        }
        
        private static int buffer_size = 0x4000;
        private static int max_string_size = 1024;
        private static bool debug;
      
        
        enum medias
        {
                MKV_Track_video=1,
                MKV_Track_audio=2,
                MKV_Track_subtitle=3,
                MKV_Track_subtitle_orig=0x11
        }
        
        //List of MKV entries
        enum type
        {
                MKVID_OutputSamplingFrequency=0x78B5,
                MKVID_FILE_BEGIN=0x1A,
                MKVID_EBML=0x1A45DFA3,
                MKVID_Segment=0x18538067,
                MKVID_Info=0x1549A966,
                MKVID_Tracks=0x1654AE6B,
                MKVID_TrackEntry=0xAE,
                MKVID_TrackType=0x83,
                MKVID_DefaultDuration=0x23E383,
                MKVID_Language=0x22B59C,
                MKVID_CodecID=0x86,
                MKVID_CodecPrivate=0x63A2,
                MKVID_PixelWidth=0xB0,
                MKVID_PixelHeight=0xBA,
                MKVID_TimeCodeScale=0x2AD7B1,
                MKVID_Duration=0x4489,
                MKVID_Channels=0x9F,
                MKVID_BitDepth=0x6264,
                MKVID_SamplingFrequency=0xB5,
                MKVID_Title=0x7BA9,
                MKVID_Tags=0x1254C367,
                MKVID_SeekHead=0x114D9B74,
                MKVID_Video=0xE0,
                MKVID_Audio=0xE1,
                MKVID_CodecName=0x258688,
                MKVID_DisplayHeight=0x54BA,
                MKVID_DisplayWidth=0x54B0
        }
        
        //List of MKV sections
        private static uint [] MKV_Parse_list =
        {
                (uint)type.MKVID_Segment,
                (uint)type.MKVID_Info,
                (uint)type.MKVID_Video,
                (uint)type.MKVID_Audio,
                (uint)type.MKVID_TrackEntry,
                (uint)type.MKVID_Tracks
        };
         
   }
}
