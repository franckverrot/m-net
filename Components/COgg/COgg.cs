/*---------------------------------------------------------------------
 *	@file:COgg.cs
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
//using System.Collections;

namespace MediaNET.Components
{
        /// <summary>
        /// Reads the standard tags of an .ogg file
        /// </summary>
        [Serializable]
                public class COgg: CAudio
                {
                        #region private properties
                        //http://www.xiph.org/ogg/vorbis/doc/v-comment.html
                        private string version = "";
                        private string album = "";
                        private string tracknumber = "";
                        private string performer = "";
                        private string copyright = "";
                        private string license = "";
                        private string organization = "";
                        private string description = "";
                        private string genre = "";
                        private string date = "";
                        private string location = "";
                        private string contact = "";
                        private string iscr = "";
                        //winamp
                        private string comment = "";		
                        #endregion				

                        static string s_sTypeIdentifier = "Ogg music file";
                        static string s_sPlayer = "ogg123";
                        static string s_sExtension = ".ogg";
                        static string s_sDescription="This type handle all Ogg music file.\nComment? Suggestion? root@dev-fr.com";


                        public static string Extension
                        {
                                get { return s_sExtension; }
                                set { s_sExtension = value; }
                        }

                        public static string Player
                        {
                                get { return s_sPlayer; }
                                set { s_sPlayer = value; }
                        }

                        public static string TypeIdentifier
                        {
                                get { return s_sTypeIdentifier; }
                                set { s_sTypeIdentifier = value; }
                        }

                        public static string Description
                        {
                                get { return s_sDescription; }
                                set { s_sDescription = value; }
                        }

                        public override Gtk.Widget Display
                        {
                                get 
                                {
                                        int H=200,W=200;
                                        try
                                        {
                                                Glade.XML m_displayXML = new Glade.XML(null,"cogg.glade","ShowWindow",null);
                                                m_displayXML.Autoconnect(this);
                                                ((Gtk.Entry)m_displayXML["entryTitle"]).Text = Title;
                                                ((Gtk.Entry)m_displayXML["entryAuthor"]).Text = Author;
                                                ((Gtk.Entry)m_displayXML["entryTrackNumber"]).Text = TrackNumber;
                                                ((Gtk.Entry)m_displayXML["entryAlbum"]).Text = Album;
                                                ((Gtk.Entry)m_displayXML["entryDate"]).Text = Date;
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
                                        }
                                        catch(Exception e)
                                        {
                                                Console.WriteLine("COgg Display: "+e.Message);
                                        }
                                        return null;
                                }
                        }


                        public override Gtk.Widget Edit
                        {
                                get 
                                {
                                        try
                                        {
                                                Glade.XML m_displayXML = new Glade.XML(null,"cogg.glade","EditWindow",null);
                                                m_displayXML.Autoconnect(this);
                                                ((Gtk.Entry)m_displayXML["itemPath"]).Text = Path;
                                                ((Gtk.Entry)m_displayXML["itemTitle"]).Text = Title;
                                                ((Gtk.Entry)m_displayXML["itemAuthor"]).Text = Author;
                                                ((Gtk.Entry)m_displayXML["itemTrackNumber"]).Text = TrackNumber;
                                                ((Gtk.Entry)m_displayXML["itemAlbum"]).Text = Album;
                                                ((Gtk.Entry)m_displayXML["itemDate"]).Text = Date;
                                                ((Gtk.Entry)m_displayXML["itemGenre"]).Text = Genre;
                                                ((Gtk.Entry)m_displayXML["itemComment"]).Text = Comment;
                                                ((Gtk.Entry)m_displayXML["itemCopyright"]).Text = Copyright;
                                                ((Gtk.Entry)m_displayXML["itemUrl"]).Text = Location;
                                                ((Gtk.Entry)m_displayXML["itemEncodedby"]).Text = Contact;
                                                return m_displayXML.GetWidget("editFrame") as Gtk.Widget;
                                        }
                                        catch(Exception e)
                                        {
                                                Console.WriteLine("COgg Edit: "+e.Message);
                                        }
                                        return null;
                                }
                                set {}
                        }

                        public override Array Summary 
                        {
                                get 
                                {
                                        string displayable = +Album+" - "+String.Format("{0:00}",int.Parse(TrackNumber))+" - "+ Title;
                                        string[] s = {displayable,Author,TypeIdentifier,Length,"unused",GID};
                                        return s;
                                }
                        }

                        //	private ArrayList other = new ArrayList();

                        private System.Text.UTF8Encoding td = new System.Text.UTF8Encoding();


                        public override void saveFileCharacteristics() {
                        }

                        public COgg() : this("")
                        {

                        }

                        public COgg(string fullname)
                        {
                                m_sPath=fullname;
                        }

                        public override void loadFileCharacteristics(){
                                FileStream fs = new FileStream(CompleteFileName,FileMode.Open,FileAccess.Read);
                                {
                                        if (fs.Length == 0)
                                                throw (new Exception("File empty"));
                                        m_sLength = ((float)(fs.Length/(1024*1024))).ToString() + " MiB";

                                        byte[] buffer = new byte[3];
                                        fs.Read(buffer,0,3);
                                        if (td.GetString(buffer).ToLower() != "ogg")
                                                throw (new Exception("Not Ogg file"));

                                        //					            v     o         
                                        byte[] end1 = { 0x01, 0x05, 0x76, 0x6F };
                                        //					  r     b     i     s
                                        byte[] end2 = { 0x72, 0x62, 0x69, 0x73 };

                                        buffer = new byte[4];
                                        do
                                        {
                                                //check if it's time to exit. It shouldn't be never used, but who knows!
                                                if (fs.Position == 2048 || fs.Position > fs.Length -10)
                                                        break;

                                                //go back of 3 bytes
                                                fs.Position -= 3;
                                                //load the buffer with the next 4 bytes
                                                fs.Read(buffer, 0, 4);

                                                //check if it's time to exit
                                                if (buffer == end1)
                                                {
                                                        fs.Read(buffer, 0, 4);
                                                        if (buffer == end2)
                                                                break;
                                                }

                                                //check if buffer contains a known tag
                                                switch (td.GetString(buffer).ToUpper())
                                                {
                                                        case "TITL":
                                                                //check that it really is the Title tag (no "TitleOld" or others)
                                                                if (fullTagName(fs) == "TITLE")
                                                                        //set the Title property with the Title tag
                                                                        this.Title = this.readNext(fs);
                                                        break;
                                                        case "VERS":
                                                                if (fullTagName(fs) == "VERSION")
                                                                        this.version = readNext(fs);
                                                        break;
                                                        case "ALBU":
                                                                if (fullTagName(fs) == "ALBUM")
                                                                        this.album = readNext(fs);
                                                        break;
                                                        case "TRAC":
                                                                if (fullTagName(fs) == "TRACKNUMBER")
                                                                        this.tracknumber = readNext(fs);
                                                        break;
                                                        case "ARTI":
                                                                if (fullTagName(fs) == "ARTIST")
                                                                        this.m_sAuthor = readNext(fs);
                                                        break;
                                                        case "PERF":
                                                                if (fullTagName(fs) == "PERFORMER")
                                                                        this.performer = readNext(fs);
                                                        break;
                                                        case "COPY":
                                                                if (fullTagName(fs) == "COPYRIGHT")
                                                                        this.copyright = readNext(fs);
                                                        break;
                                                        case "LICE":
                                                                if (fullTagName(fs) == "LICENSE")
                                                                        this.license = readNext(fs);
                                                        break;
                                                        case "ORGA":
                                                                if (fullTagName(fs) == "ORGANIZATION")
                                                                        this.organization = readNext(fs);
                                                        break;
                                                        case "DESC":
                                                                if (fullTagName(fs) == "DESCRIPTION")
                                                                        this.description = readNext(fs);
                                                        break;
                                                        case "GENR":
                                                                if (fullTagName(fs) == "GENRE")
                                                                        this.genre = readNext(fs);
                                                        break;
                                                        case "DATE":
                                                                fs.Position++;  //avoid to read =
                                                        this.date = readNext(fs);
                                                        break;
                                                        case "LOCA":
                                                                if (fullTagName(fs) == "LOCATION")
                                                                        this.location = readNext(fs);
                                                        break;							
                                                        case "CONT":
                                                                if (fullTagName(fs) == "CONTACT")
                                                                        this.contact = readNext(fs);
                                                        break;
                                                        case "ISCR":
                                                                fs.Position++;  //avoid to read =
                                                        this.iscr = readNext(fs);
                                                        break;							
                                                        case "COMM":
                                                                if (fullTagName(fs) == "COMMENT")
                                                                        this.comment = readNext(fs);
                                                        break;
                                                        //					default:
                                                        //						other.Add(fullTagName(fs) + "=" + readNext(fs));
                                                        //						break;
                                                }
                                        }while (true);

                                        //		Console.WriteLine("done");
                                }
                                fs.Close();
                                fs = null;
                        }

                        /// <summary>
                        /// reads the tag content
                        /// </summary>
                        /// <param name="fs">filestream of the ogg file (Position must be after the =)</param>
                        /// <returns>the text of the current tag</returns>
                        private string readNext(FileStream fs)
                        {
                                string text = "";

                                byte[] byt = new byte[1];

                                do
                                {
                                        fs.Read(byt,0,1);
                                        if (byt[0] == 0x00) //the tag is finished: all tags are followed by 0? and one or more 00
                                        {
                                                text = text.Remove(text.Length-1, 1);		//remove the 0? byte
                                                break;
                                        }
                                        else if (byt[0] == 0x01) //0x01 is at the end of all tags. no more tags in the file
                                                break;

                                        text += td.GetString(byt);
                                } while (true);
                                return text;
                        }


                        /// <summary>
                        /// reads the full name of the tag
                        /// </summary>
                        /// <param name="fs">FileStream of the ogg file (position must be 4 chars after the start of the tag name)</param>
                        /// <returns>the full name of the tag</returns>
                        private string fullTagName(FileStream fs)
                        {
                                string tag = "";
                                fs.Seek(-4, SeekOrigin.Current);		//go where the tag name starts

                                byte[] byt = new byte[1];

                                do
                                {
                                        fs.Read(byt,0,1);
                                        if (byt[0] == 0x3D)//an = always follows tha tag name. //hex: 3D    decimal: 61    string: =
                                                break;
                                        tag += td.GetString(byt);
                                } while(true);
                                return tag;
                        }


#region public properties
                        public string Version
                        {
                                get { return this.version; }
                        }
                        
                        public string Album
                        {
                                get { return this.album; }
                        }

                        public string TrackNumber
                        {
                                get { return this.tracknumber; }
                        }
                        
                        public string Artist
                        {
                                get { return this.m_sAuthor; }
                        }
                        public string Performer
                        {
                                get { return this.performer; }
                        }
                        public string Copyright
                        {
                                get { return this.copyright; }
                        }
                        public string License
                        {
                                get { return this.license; }
                        }
                        public string Organization
                        {
                                get { return this.organization; }
                        }
                        public string OggDescription
                        {
                                get { return this.description; }
                        }
                        public string Genre
                        {
                                get { return this.genre; }
                        }
                        public string Date
                        {
                                get { return this.date; }
                        }
                        public string Location
                        {
                                get { return this.location; }
                        }
                        public string Contact
                        {
                                get { return this.contact; }
                        }
                        public string Iscr
                        {
                                get { return this.iscr; }
                        }
                        public string Comment
                        {
                                get { return this.comment; }
                        }
#endregion

                }
}
