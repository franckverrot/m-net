/*---------------------------------------------------------------------
 *	@file:CPdf.cs
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
using System.Collections;

namespace MediaNET.Components
{
        [Serializable]
        public class CPdf: CImage
        {
                #region private properties
                #endregion				

                static string s_sTypeIdentifier = "Pdf files";
                static string s_sPlayer = "xpdf";
                static string s_sExtension = ".pdf";
                static string s_sDescription="This type handle all PDF files.\nComment? Suggestion? root@dev-fr.com";


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


                public override Gtk.Widget Display
                {
                        get 
                        {
                                int H=200,W=200;
                                try
                                {
                                        Glade.XML m_displayXML = new Glade.XML(null,"cpdf.glade","ShowWindow",null);
                                        m_displayXML.Autoconnect(this);
                                        ((Gtk.Label)m_displayXML["Informations"]).Text = Title+" ("+Author+")";
                                        return m_displayXML.GetWidget("showFrame") as Gtk.Widget;
                                }
                                catch(Exception e)
                                {
                                        Console.WriteLine("CPdf Display: "+e.Message);
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
                                        Glade.XML m_displayXML = new Glade.XML(null,"cpdf.glade","EditWindow",null);
                                        m_displayXML.Autoconnect(this);
                                        /*
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
                                        */
                                        return m_displayXML.GetWidget("editFrame") as Gtk.Widget;
                                }
                                catch(Exception e)
                                {
                                        Console.WriteLine("CPdf Edit: "+e.Message);
                                }
                                return null;
                        }
                        set {}
                }

                public override Array Summary 
                {
                        get 
                        {
                                string[] s = {Title,Author,TypeIdentifier,Length,"unused",GID};
                                return s;
                        }
                }

                private System.Text.UTF8Encoding td = new System.Text.UTF8Encoding();


                public override void saveFileCharacteristics() {
                }

                public CPdf() : this("")
                {

                }

                public CPdf(string fullname)
                {
                        m_sPath=fullname;
                }

                public override void loadFileCharacteristics(){
                        FileStream fs = new FileStream(CompleteFileName,FileMode.Open,FileAccess.Read);
                        {
                                if (fs.Length == 0)
                                        throw (new Exception("File empty"));
                                m_sLength = ((float)(fs.Length/(1024*1024))).ToString() + " MiB";

                }

        }
}
