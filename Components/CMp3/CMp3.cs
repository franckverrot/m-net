/*---------------------------------------------------------------------
 *	@file:CMp3.cs
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
using Gtk;
using Glade;

namespace MediaNET.Components
{
        [Serializable]
                public class CMp3 : CAudio
                {
                        // MediaNET related
                        protected static string  m_sTypeIdentifier = "MP3 Music File";
                        protected static string  m_sExtension = ".mp3";
                        protected static string s_sDescription="This type handle all Mp3 music file.\nComment? Suggestion? root@dev-fr.com";
                        protected static string  m_sPlayer = "mpg123";


                        // MP3 Related
                        protected bool    hasID3Tag;
                        protected string  id3Album;
                        protected string  id3Year;
                        protected string  id3Comment;
                        protected byte    id3TrackNumber;
                        protected byte    id3Genre;
                        protected int intBitRate;
                        protected long lngFileSize;
                        protected int intFrequency;
                        protected string strMode;
                        protected int intLength;
                        protected string strLengthFormatted;
                        protected ulong bithdr;
                        protected bool boolVBitRate;
                        protected int intVFrames;

                        
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

                        public bool    hasTag 
                        {
                                get { return hasID3Tag; }
                                set { hasID3Tag = value; }
                        }

                        public string  Album 
                        {
                                get { return id3Album; }
                                set { id3Album = value; }
                        }
                        public string  Year 
                        {
                                get { return id3Year; }
                                set { id3Year = value; }
                        }
                        public string  Comment 
                        {
                                get { return id3Comment; }
                                set { id3Comment = value; }
                        }
                        public byte TrackNumber 
                        {
                                get { return  id3TrackNumber; }
                                set {  id3TrackNumber = value; }
                        }

                        public byte GenreId 
                        {
                                get { return id3Genre; }
                                set { id3Genre = value; }
                        }

                        public string Genre 
                        {
                                get { return genres[id3Genre]; }
                        }

                        public CMp3(): this("")
                        {
                        }

                        public CMp3(string fullname) 
                        {
                                Path     = fullname;
                                hasID3Tag    = false;
                                m_sTitle   = null;
                                m_sAuthor    = null;
                                id3Album   = null;
                                id3Year    = null;
                                id3Comment   = null;
                                id3TrackNumber = 0;
                                id3Genre   = 0;
                        } 

                        ~CMp3()
                        {
                        }

                        public int Bitrate
                        {
                                get
                                {
                                        return intBitRate;
                                }
                        }
                        
                        public long Size
                        {
                                get
                                {
                                        return lngFileSize;
                                }
                        }
                        public int Frequency
                        {
                                get
                                {
                                        return intFrequency;
                                }
                        }
                        public string Mode
                        {
                                get
                                {
                                        return strMode;
                                }
                        }
                        public int FileLength
                        {
                                get
                                {
                                        return intLength;
                                }
                        }
                        public string LengthFormatted
                        {
                                get
                                {
                                        return strLengthFormatted;
                                }
                        }
                        public ulong Bithdr
                        {
                                get
                                {
                                        return bithdr;
                                }
                        }
                        public bool VBitRate
                        {
                                get
                                {
                                        return boolVBitRate;
                                }
                        }
                        public int VFrames
                        {
                                get
                                {
                                        return intVFrames;
                                }
                        }

                        public bool ReadMP3Information(string FileName)
                        {
                                FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);

                                // Set the file size
                                lngFileSize = fs.Length;

                                byte[] bytHeader = new byte[4];
                                byte[] bytVBitRate = new byte[12];
                                int intPos = 0;

                                // Keep reading 4 bytes from the header until we know for sure that in 
                                // fact it's an MP3
                                do
                                {
                                        fs.Position = intPos;
                                        fs.Read(bytHeader,0,4);
                                        intPos++;
                                        LoadMP3Header(bytHeader);
                                }
                                while(!IsValidHeader() && (fs.Position!=fs.Length));

                                // If the current file stream position is equal to the length, 
                                // that means that we've read the entire file and it's not a valid MP3 file
                                if(fs.Position != fs.Length)
                                {
                                        intPos += 3;

                                        if(getVersionIndex() == 3)    // MPEG Version 1
                                        {
                                                if(getModeIndex() == 3)    // Single Channel
                                                {
                                                        intPos += 17;
                                                }
                                                else
                                                {
                                                        intPos += 32;
                                                }
                                        }
                                        else                        // MPEG Version 2.0 or 2.5
                                        {
                                                if(getModeIndex() == 3)    // Single Channel
                                                {
                                                        intPos += 9;
                                                }
                                                else
                                                {
                                                        intPos += 17;
                                                }
                                        }

                                        // Check to see if the MP3 has a variable bitrate
                                        fs.Position = intPos;
                                        fs.Read(bytVBitRate,0,12);
                                        boolVBitRate = LoadVBRHeader(bytVBitRate);

                                        // Once the file's read in, then assign the properties of the file to the public variables
                                        intBitRate = getBitrate();
                                        intFrequency = getFrequency();
                                        strMode = getMode();
                                        intLength = getLengthInSeconds();
                                        strLengthFormatted = getFormattedLength();
                                        fs.Close();
                                        return true;
                                }
                                return false;
                        }

                        protected void LoadMP3Header(byte[] c)
                        {
                                // this thing is quite interesting, it works like the following
                                // c[0] = 00000011
                                // c[1] = 00001100
                                // c[2] = 00110000
                                // c[3] = 11000000
                                // the operator << means that we'll move the bits in that direction
                                // 00000011 << 24 = 00000011000000000000000000000000
                                // 00001100 << 16 =         000011000000000000000000
                                // 00110000 << 24 =                 0011000000000000
                                // 11000000       =                         11000000
                                //                +_________________________________
                                //                  00000011000011000011000011000000
                                bithdr = (ulong)(((c[0] & 255) << 24) | ((c[1] & 255) << 16) | ((c[2] & 255) <<  8) | ((c[3] & 255))); 
                        }

                        protected bool LoadVBRHeader(byte[] inputheader)
                        {
                                // If it's a variable bitrate MP3, the first 4 bytes will read 'Xing'
                                // since they're the ones who added variable bitrate-edness to MP3s
                                if(inputheader[0] == 88 && inputheader[1] == 105 && 
                                                inputheader[2] == 110 && inputheader[3] == 103)
                                {
                                        int flags = (int)(((inputheader[4] & 255) << 24) | ((inputheader[5] & 255) << 16) | ((inputheader[6] & 255) <<  8) | ((inputheader[7] & 255)));
                                        if((flags & 0x0001) == 1)
                                        {
                                                intVFrames = (int)(((inputheader[8] & 255) << 24) | ((inputheader[9] & 255) << 16) | ((inputheader[10] & 255) <<  8) | ((inputheader[11] & 255)));
                                                return true;
                                        }
                                        else
                                        {
                                                intVFrames = -1;
                                                return true;
                                        }
                                }
                                return false;
                        }

                        protected bool IsValidHeader() 
                        {
                                return (((getFrameSync()      & 2047)==2047) &&
                                                ((getVersionIndex()   &    3)!=   1) &&
                                                ((getLayerIndex()     &    3)!=   0) && 
                                                ((getBitrateIndex()   &   15)!=   0) &&
                                                ((getBitrateIndex()   &   15)!=  15) &&
                                                ((getFrequencyIndex() &    3)!=   3) &&
                                                ((getEmphasisIndex()  &    3)!=   2)    );
                        }

                        protected int getFrameSync()     
                        {
                                return (int)((bithdr>>21) & 2047); 
                        }

                        protected int getVersionIndex()  
                        { 
                                return (int)((bithdr>>19) & 3);  
                        }

                        protected int getLayerIndex()    
                        { 
                                return (int)((bithdr>>17) & 3);  
                        }

                        protected int getProtectionBit() 
                        { 
                                return (int)((bithdr>>16) & 1);  
                        }

                        protected int getBitrateIndex()  
                        { 
                                return (int)((bithdr>>12) & 15); 
                        }

                        protected int getFrequencyIndex()
                        { 
                                return (int)((bithdr>>10) & 3);  
                        }

                        protected int getPaddingBit()    
                        { 
                                return (int)((bithdr>>9) & 1);  
                        }

                        protected int getPrivateBit()    
                        { 
                                return (int)((bithdr>>8) & 1);  
                        }

                        protected int getModeIndex()     
                        { 
                                return (int)((bithdr>>6) & 3);  
                        }

                        protected int getModeExtIndex()  
                        { 
                                return (int)((bithdr>>4) & 3);  
                        }

                        protected int getCoprightBit()   
                        { 
                                return (int)((bithdr>>3) & 1);  
                        }

                        protected int getOrginalBit()    
                        { 
                                return (int)((bithdr>>2) & 1);  
                        }

                        protected int getEmphasisIndex() 
                        { 
                                return (int)(bithdr & 3);  
                        }

                        protected double getVersion() 
                        {
                                double[] table = {2.5, 0.0, 2.0, 1.0};
                                return table[getVersionIndex()];
                        }

                        protected int getLayer() 
                        {
                                return (int)(4 - getLayerIndex());
                        }

                        protected int getBitrate() 
                        {
                                // If the file has a variable bitrate, then we return an integer average bitrate,
                                // otherwise, we use a lookup table to return the bitrate
                                if(boolVBitRate)
                                {
                                        double medFrameSize = (double)lngFileSize / (double)getNumberOfFrames();
                                        return (int)((medFrameSize * (double)getFrequency()) / (1000.0 * ((getLayerIndex()==3) ? 12.0 : 144.0)));
                                }
                                else
                                {
                                        int[,,] table =        {
                                                { // MPEG 2 & 2.5
                                                        {0,  8, 16, 24, 32, 40, 48, 56, 64, 80, 96,112,128,144,160,0}, // Layer III
                                                        {0,  8, 16, 24, 32, 40, 48, 56, 64, 80, 96,112,128,144,160,0}, // Layer II
                                                        {0, 32, 48, 56, 64, 80, 96,112,128,144,160,176,192,224,256,0}  // Layer I
                                                },
                                                { // MPEG 1
                                                        {0, 32, 40, 48, 56, 64, 80, 96,112,128,160,192,224,256,320,0}, // Layer III
                                                        {0, 32, 48, 56, 64, 80, 96,112,128,160,192,224,256,320,384,0}, // Layer II
                                                        {0, 32, 64, 96,128,160,192,224,256,288,320,352,384,416,448,0}  // Layer I
                                                }
                                        };

                                        return table[getVersionIndex() & 1, getLayerIndex()-1, getBitrateIndex()];
                                }
                        }

                        protected int getFrequency() 
                        {
                                int[,] table =    {    
                                        {32000, 16000,  8000}, // MPEG 2.5
                                        {    0,     0,     0}, // reserved
                                        {22050, 24000, 16000}, // MPEG 2
                                        {44100, 48000, 32000}  // MPEG 1
                                };

                                return table[getVersionIndex(), getFrequencyIndex()];
                        }

                        protected string getMode() 
                        {
                                switch(getModeIndex()) 
                                {
                                        default:
                                                return "Stereo";
                                        case 1:
                                                return "Joint Stereo";
                                        case 2:
                                                return "Dual Channel";
                                        case 3:
                                                return "Single Channel";
                                }
                        }

                        protected int getLengthInSeconds() 
                        {
                                // "intKilBitFileSize" made by dividing by 1000 in order to match the "Kilobits/second"
                                int intKiloBitFileSize = (int)((8 * lngFileSize) / 1000);
                                return (int)(intKiloBitFileSize/getBitrate());
                        }

                        protected string getFormattedLength() 
                        {
                                // Complete number of seconds
                                int s  = getLengthInSeconds();

                                // Seconds to display
                                int ss = s%60;

                                // Complete number of minutes
                                int m  = (s-ss)/60;

                                // Minutes to display
                                int mm = m%60;

                                // Complete number of hours
                                int h = (m-mm)/60;

                                // Make "hh:mm:ss"
                                return h.ToString("D2") + ":" + mm.ToString("D2") + ":" + ss.ToString("D2");
                        }

                        protected int getNumberOfFrames() 
                        {
                                // Again, the number of MPEG frames is dependant on whether it's a variable bitrate MP3 or not
                                if (!boolVBitRate) 
                                {
                                        double medFrameSize = (double)(((getLayerIndex()==3) ? 12 : 144) *((1000.0 * (float)getBitrate())/(float)getFrequency()));
                                        return (int)(lngFileSize/medFrameSize);
                                }
                                else 
                                        return intVFrames;
                        }

                        public override void loadFileCharacteristics()
                        {
                                ReadMP3Information(CompleteFileName);
                                // Read the 128 byte ID3 tag into a byte array
                                FileStream oFileStream;
                                oFileStream = new FileStream(CompleteFileName , FileMode.Open, FileAccess.Read);
                                m_sLength = ((float)(oFileStream.Length/(1024*1024))).ToString() + " MiB";
                                byte[] bBuffer = new byte[128];
                                oFileStream.Seek(-128, SeekOrigin.End);
                                oFileStream.Read(bBuffer,0, 128);
                                oFileStream.Close();
                                oFileStream = null;

                                // Convert the Byte Array to a String
                                Encoding  instEncoding = new ASCIIEncoding();   // NB: Encoding is an Abstract class
                                string id3Tag = instEncoding.GetString(bBuffer);

                                // If there is an attched ID3 v1.x TAG then read it 
                                if (id3Tag .Substring(0,3) == "TAG") 
                                {
                                        m_sTitle       = id3Tag.Substring(  3, 30).Trim("\0".ToCharArray());
                                        m_sAuthor      = id3Tag.Substring( 33, 30).Trim("\0".ToCharArray());
                                        id3Album       = id3Tag.Substring( 63, 30).Trim("\0".ToCharArray());
                                        id3Year        = id3Tag.Substring( 93, 4).Trim("\0".ToCharArray());
                                        id3Comment     = id3Tag.Substring( 97,28).Trim("\0".ToCharArray());
                                        
                                        m_sTitle       = id3Tag.Substring(  3, 30).TrimEnd(" ".ToCharArray());
                                        m_sAuthor      = id3Tag.Substring( 33, 30).TrimEnd(" ".ToCharArray());
                                        id3Album       = id3Tag.Substring( 63, 30).TrimEnd(" ".ToCharArray());
                                        id3Year        = id3Tag.Substring( 93, 4).TrimEnd( " ".ToCharArray());
                                        id3Comment     = id3Tag.Substring( 97,28).TrimEnd( " ".ToCharArray());


                                        // Get the track number if TAG conforms to ID3 v1.1
                                        if (id3Tag[125]==0)
                                                id3TrackNumber = bBuffer[126];
                                        else
                                                id3TrackNumber = 0;
                                        id3Genre = bBuffer[127];
                                        hasID3Tag     = true;
                                        // ********* IF USED IN ANGER: ENSURE to test for non-numeric year
                                }
                                else 
                                {
                                        // ID3 Tag not found so create an empty TAG in case the user saces later
                                        m_sTitle       = "";
                                        m_sAuthor      = "";
                                        id3Album       = "";
                                        id3Year        = "";
                                        id3Comment     = "";
                                        id3TrackNumber = 0;
                                        id3Genre       = 0;
                                        hasID3Tag      = false;
                                }
                        }

                        public override void saveFileCharacteristics()
                        {
                                // Trim any whitespace
                                m_sTitle   = m_sTitle.Trim();
                                m_sAuthor  = m_sAuthor.Trim();
                                id3Album   = id3Album.Trim();
                                id3Year    = id3Year.Trim();
                                id3Comment = id3Comment.Trim();

                                // Ensure all properties are correct size
                                if (m_sTitle.Length > 30)   m_sTitle    = m_sTitle.Substring(0,30);
                                if (m_sAuthor.Length > 30)  m_sAuthor   = m_sAuthor.Substring(0,30);
                                if (id3Album.Length > 30)   id3Album    = id3Album.Substring(0,30);
                                if (id3Year.Length > 4)     id3Year     = id3Year.Substring(0,4);
                                if (id3Comment.Length > 28) id3Comment  = id3Comment.Substring(0,28);

                                // Build a new ID3 Tag (128 Bytes)
                                byte[] tagByteArray = new byte[128];
                                for ( int i = 0; i < tagByteArray.Length; i++ ) tagByteArray[i] = 0; // Initialise array to nulls

                                // Convert the Byte Array to a String
                                Encoding  instEncoding = new ASCIIEncoding();   // NB: Encoding is an Abstract class // ************ To DO: Make a shared instance of ASCIIEncoding so we don't keep creating/destroying it
                                // Copy "TAG" to Array
                                byte[] workingByteArray = instEncoding.GetBytes("TAG"); 
                                Array.Copy(workingByteArray, 0, tagByteArray, 0, workingByteArray.Length);
                                // Copy Title to Array
                                workingByteArray = instEncoding.GetBytes(m_sTitle);
                                Array.Copy(workingByteArray, 0, tagByteArray, 3, workingByteArray.Length);
                                // Copy Artist to Array
                                workingByteArray = instEncoding.GetBytes(m_sAuthor);
                                Array.Copy(workingByteArray, 0, tagByteArray, 33, workingByteArray.Length);
                                // Copy Album to Array
                                workingByteArray = instEncoding.GetBytes(id3Album);
                                Array.Copy(workingByteArray, 0, tagByteArray, 63, workingByteArray.Length);
                                // Copy Year to Array
                                workingByteArray = instEncoding.GetBytes(id3Year);
                                Array.Copy(workingByteArray, 0, tagByteArray, 93, workingByteArray.Length);
                                // Copy Comment to Array
                                workingByteArray = instEncoding.GetBytes(id3Comment);
                                Array.Copy(workingByteArray, 0, tagByteArray, 97, workingByteArray.Length);
                                // Copy Track and Genre to Array
                                tagByteArray[126] = id3TrackNumber;
                                tagByteArray[127] = id3Genre;

                                // SAVE TO DISK: Replace the final 128 Bytes with our new ID3 tag
                                FileStream oFileStream = new FileStream(CompleteFileName , FileMode.Open);
                                if (hasID3Tag)
                                        oFileStream.Seek(-128, SeekOrigin.End);
                                else
                                        oFileStream.Seek(0, SeekOrigin.End);
                                oFileStream.Write(tagByteArray,0, 128);
                                oFileStream.Close();
                                hasID3Tag  = true;
                        }

                        public override string Length
                        {
                                get
                                {
                                        return LengthFormatted;
                                }
                        }
                        public override Array Summary 
                        {
                                get 
                                {
                                        if(Title == "")
                                        {
                                                Title = System.IO.Path.GetFileName(Path);
                                        }
                                        if(Album == "" || Album == null || Album == " ")
                                        {
                                                Album = "Unknown"; //System.IO.Path.GetDirectoryName(Path);
                                        }
                                        
                                        string[] s =  {Album.Trim("\0".ToCharArray())+" - "+String.Format("{0:00}",TrackNumber)+ " - " +Title,Author,m_sTypeIdentifier,LengthFormatted,"unused",GID};
                                        return s;
                                }
                        }
                        public override Gtk.Widget Display
                        {
                                get 
                                {
                                        try {
                                                int H=200,W=200;
                                                Glade.XML m_displayXML = new Glade.XML(null,"cmp3.glade","ShowWindow",null);
                                                m_displayXML.Autoconnect(this);
                                                ((Gtk.Entry)m_displayXML["entryTitle"]).Text = Title;
                                                ((Gtk.Entry)m_displayXML["entryAuthor"]).Text = Author;
                                                ((Gtk.Entry)m_displayXML["entryAlbum"]).Text = Album;
                                                ((Gtk.Entry)m_displayXML["entryComment"]).Text = Comment;
                                                ((Gtk.Entry)m_displayXML["entryDate"]).Text = Year;
                                                ((Gtk.Entry)m_displayXML["entryTrackNumber"]).Text = TrackNumber.ToString();
                                                if(id3Genre >= 0 && id3Genre < genres.Length)
                                                        ((Gtk.Entry)m_displayXML["entryGenre"]).Text = Genre;

                                                string [] list =  {
                                                        "cover.jpg",
                                                        Title.Replace(".mp3",".jpg"),
                                                        Title.Replace(".mp3",".JPG"),
                                                        Title.Replace(".mp3",".png"),
                                                        Title.Replace(".mp3",".PNG")
                                                };
                                                foreach(string alt in list)
                                                {
                                                        string cover = System.IO.Path.GetDirectoryName(CompleteFileName)+System.IO.Path.DirectorySeparatorChar.ToString()+alt;
                                                        if (File.Exists(cover)) 
                                                        {
                                                                Gtk.Image image = new Gtk.Image(cover);
                                                                int DimensionsY = image.Pixbuf.Height;
                                                                int DimensionsX = image.Pixbuf.Width;
                                                                ((Gtk.Image)m_displayXML["image"]).Pixbuf = (new Gtk.Image(cover)).Pixbuf.ScaleSimple 
                                                                        (W*DimensionsX/DimensionsY,H,Gdk.InterpType.Bilinear);
                                                                break;
                                                        }
                                                }
                                                return m_displayXML.GetWidget("showFrame") as Gtk.Widget;
                                        } catch(Exception e) {
                                                Console.WriteLine("CMp3 Display: "+e.Message);
                                        }
                                        return null;
                                }
                        }

                        public override Gtk.Widget Edit
                        {
                                get 
                                {
                                        try {
                                                Glade.XML m_editXML = new Glade.XML(null,"cmp3.glade","EditWindow",null);
                                                if(m_editXML == null) {
                                                        Console.WriteLine("Edit load ce con.. ");
                                                        m_editXML = new Glade.XML(null,"cmp3.glade","EditWindow",null);
                                                }
                                                m_editXML.Autoconnect(this);
                                                ((Gtk.Entry)m_editXML["editFilePath"]).Text = CompleteFileName;
                                                ((Gtk.Entry)m_editXML["itemTitle"]).Text = Title;
                                                ((Gtk.Entry)m_editXML["itemAuthor"]).Text = Author;
                                                ((Gtk.Entry)m_editXML["itemAlbum"]).Text = Album;
                                                ((Gtk.Entry)m_editXML["itemComment"]).Text = Comment;
                                                ((Gtk.Entry)m_editXML["itemDate"]).Text = Year;
                                                ((Gtk.Entry)m_editXML["itemTrackNumber"]).Text = TrackNumber.ToString();
                                                ((Gtk.Label)m_editXML["labelRate"]).Text = Bitrate.ToString();
                                                ((Gtk.Label)m_editXML["labelFreq"]).Text = Frequency.ToString();
                                                ((Gtk.Label)m_editXML["labelSize"]).Text = Length.ToString();
                                                ((Gtk.Label)m_editXML["labelLength"]).Text = LengthFormatted.ToString();
                                                ((Gtk.Label)m_editXML["labelMode"]).Text = Mode.ToString();
                                                if(id3Genre >= 0 && id3Genre < genres.Length)
                                                        ((Gtk.Entry)m_editXML["itemGenre"]).Text = Genre;
                                                return m_editXML.GetWidget("editFrame") as Gtk.Widget;
                                        } catch(Exception e) {
                                                Console.WriteLine("CMp3 Edit: "+e.Message);
                                        }
                                        return new Label("Loading Error");
                                }
                        }

                        static public string Description
                        {
                                get
                                {
                                        return s_sDescription;
                                }
                        }

                        public void OnPlayButton(object o,EventArgs args)
                        {
                        }

                        
                }
        internal class FileCommands 
        {
                public static void readMP3Tag (ref MP3 paramMP3) 
                {
                        // Read the 128 byte ID3 tag into a byte array
                        FileStream oFileStream;
                        oFileStream = new FileStream(paramMP3.fileComplete , FileMode.Open);
                        byte[] bBuffer = new byte[128];
                        oFileStream.Seek(-128, SeekOrigin.End);
                        oFileStream.Read(bBuffer,0, 128);
                        oFileStream.Close();

                        // Convert the Byte Array to a String
                        Encoding  instEncoding = new ASCIIEncoding();   // NB: Encoding is an Abstract class
                        string id3Tag = instEncoding.GetString(bBuffer);

                        // If there is an attched ID3 v1.x TAG then read it 
                        if (id3Tag .Substring(0,3) == "TAG") 
                        {
                                paramMP3.id3Title       = id3Tag.Substring(  3, 30).Trim();
                                paramMP3.id3Artist      = id3Tag.Substring( 33, 30).Trim();
                                paramMP3.id3Album       = id3Tag.Substring( 63, 30).Trim();
                                paramMP3.id3Year        = id3Tag.Substring( 93, 4).Trim();
                                paramMP3.id3Comment     = id3Tag.Substring( 97,28).Trim();

                                // Get the track number if TAG conforms to ID3 v1.1
                                if (id3Tag[125]==0)
                                        paramMP3.id3TrackNumber = bBuffer[126];
                                else
                                        paramMP3.id3TrackNumber = 0;
                                paramMP3.id3Genre = bBuffer[127];
                                paramMP3.hasID3Tag     = true;
                                // ********* IF USED IN ANGER: ENSURE to test for non-numeric year
                        }
                        else 
                        {
                                // ID3 Tag not found so create an empty TAG in case the user saces later
                                paramMP3.id3Title       = "";
                                paramMP3.id3Artist      = "";
                                paramMP3.id3Album       = "";
                                paramMP3.id3Year        = "";
                                paramMP3.id3Comment     = "";
                                paramMP3.id3TrackNumber = 0;
                                paramMP3.id3Genre       = 0;
                                paramMP3.hasID3Tag      = false;
                        }
                }

                public static void updateMP3Tag (ref MP3 paramMP3) 
                {
                        // Trim any whitespace
                        paramMP3.id3Title   = paramMP3.id3Title.Trim();
                        paramMP3.id3Artist  = paramMP3.id3Artist.Trim();
                        paramMP3.id3Album   = paramMP3.id3Album.Trim();
                        paramMP3.id3Year    = paramMP3.id3Year.Trim();
                        paramMP3.id3Comment = paramMP3.id3Comment.Trim();

                        // Ensure all properties are correct size
                        if (paramMP3.id3Title.Length > 30)   paramMP3.id3Title    = paramMP3.id3Title.Substring(0,30);
                        if (paramMP3.id3Artist.Length > 30)  paramMP3.id3Artist   = paramMP3.id3Artist.Substring(0,30);
                        if (paramMP3.id3Album.Length > 30)   paramMP3.id3Album    = paramMP3.id3Album.Substring(0,30);
                        if (paramMP3.id3Year.Length > 4)     paramMP3.id3Year     = paramMP3.id3Year.Substring(0,4);
                        if (paramMP3.id3Comment.Length > 28) paramMP3.id3Comment  = paramMP3.id3Comment.Substring(0,28);

                        // Build a new ID3 Tag (128 Bytes)
                        byte[] tagByteArray = new byte[128];
                        for ( int i = 0; i < tagByteArray.Length; i++ ) tagByteArray[i] = 0; // Initialise array to nulls

                        // Convert the Byte Array to a String
                        Encoding  instEncoding = new ASCIIEncoding();   // NB: Encoding is an Abstract class // ************ To DO: Make a shared instance of ASCIIEncoding so we don't keep creating/destroying it
                        // Copy "TAG" to Array
                        byte[] workingByteArray = instEncoding.GetBytes("TAG"); 
                        Array.Copy(workingByteArray, 0, tagByteArray, 0, workingByteArray.Length);
                        // Copy Title to Array
                        workingByteArray = instEncoding.GetBytes(paramMP3.id3Title);
                        Array.Copy(workingByteArray, 0, tagByteArray, 3, workingByteArray.Length);
                        // Copy Artist to Array
                        workingByteArray = instEncoding.GetBytes(paramMP3.id3Artist);
                        Array.Copy(workingByteArray, 0, tagByteArray, 33, workingByteArray.Length);
                        // Copy Album to Array
                        workingByteArray = instEncoding.GetBytes(paramMP3.id3Album);
                        Array.Copy(workingByteArray, 0, tagByteArray, 63, workingByteArray.Length);
                        // Copy Year to Array
                        workingByteArray = instEncoding.GetBytes(paramMP3.id3Year);
                        Array.Copy(workingByteArray, 0, tagByteArray, 93, workingByteArray.Length);
                        // Copy Comment to Array
                        workingByteArray = instEncoding.GetBytes(paramMP3.id3Comment);
                        Array.Copy(workingByteArray, 0, tagByteArray, 97, workingByteArray.Length);
                        // Copy Track and Genre to Array
                        tagByteArray[126] = paramMP3.id3TrackNumber;
                        tagByteArray[127] = paramMP3.id3Genre;

                        // SAVE TO DISK: Replace the final 128 Bytes with our new ID3 tag
                        FileStream oFileStream = new FileStream(paramMP3.fileComplete , FileMode.Open);
                        if (paramMP3.hasID3Tag)
                                oFileStream.Seek(-128, SeekOrigin.End);
                        else
                                oFileStream.Seek(0, SeekOrigin.End);
                        oFileStream.Write(tagByteArray,0, 128);
                        oFileStream.Close();
                        paramMP3.hasID3Tag  = true;
                }

        }

        struct MP3 
        {
                public string  filePath;
                public string  fileFileName;
                public string  fileComplete;
                public bool    hasID3Tag;
                public string  id3Title;
                public string  id3Artist;
                public string  id3Album;
                public string  id3Year;
                public string  id3Comment;
                public byte    id3TrackNumber;
                public byte    id3Genre;

                // Required struct constructor
                public MP3(string path, string name) 
                {
                        this.filePath     = path;
                        this.fileFileName = name;
                        this.fileComplete = path + "/" + name;
                        this.hasID3Tag    = false;
                        this.id3Title   = null;
                        this.id3Artist    = null;
                        this.id3Album   = null;
                        this.id3Year    = null;
                        this.id3Comment   = null;
                        this.id3TrackNumber = 0;
                        this.id3Genre   = 0;
                }
        } 

}

