/*---------------------------------------------------------------------
 *	@file:COdb.cs
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
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Gtk;
using Glade;

namespace MediaNET.Components
{
        [Serializable]
                public class COdb : CPicture
                {
                        static string s_sTypeIdentifier = "M-NET ObjDb";
                        static string s_sPlayer = "medianet";
                        static string s_sExtension = ".odb";
                        static string s_sDescription="This type handle all MediaNET's object databse.\nComment? Suggestion? root@dev-fr.com";

                        private string m_sCreatedOn;
                        private string m_sLastModifiedOn;
                        private string m_sStoredFiles;
                        private string m_sFileTypes;

                                
                        public COdb(): this("")
                        {
                        }
                        public COdb(string fullName)
                        {
                                Path = fullName;
                        }

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
                        
                        public bool Load(string s)
                        {
                                FileStream myStream = null;
                                IFormatter formatter;
                                ArrayList al;
                                int i = 0;
                                try
                                {
                                        myStream = File.OpenRead(s);
                                }
                                catch
                                {
                                        throw new ArgumentException("File not found");
                                }

                                if (myStream != null)
                                {
                                        formatter = new BinaryFormatter();
                                        al = (ArrayList)formatter.Deserialize(myStream);
                                        foreach(object o in al)
                                        {
                                                i++;
                                        }
                                        StoredFiles = String.Format("{0}",i);
                                        CreatedOn = (File.GetCreationTime(s)).ToString();
                                        LastModifiedOn = (File.GetLastWriteTime(s)).ToString();
                                        Length = ((float)(myStream.Length/(1024*1024))).ToString() + " MiB";
                                        myStream.Close();
                                        return true;
                                }
                                myStream.Close();
                                return false;
                        }

                        public override void loadFileCharacteristics()
                        {	
                                Path = System.IO.Path.GetFullPath(Path);
                                Load(Path);
                                Title = "Odb["+StoredFiles+"]";
                                Author = "MediaNET";
                                FileStream oFileStream;
                                oFileStream = new FileStream(Path , FileMode.Open, FileAccess.Read);
                        }

                        public override void saveFileCharacteristics()
                        {

                        }

                        public override Gtk.Widget Display
                        {
                                get 
                                {
                                        int H=200,W=200;
                                        try
                                        {
                                                Glade.XML m_displayXML = new Glade.XML(null,"codb.glade","ShowWindow",null);
                                                m_displayXML.Autoconnect(this);
                                                ((Gtk.Entry)m_displayXML["entryPath"]).Text = Path;
                                                ((Gtk.Entry)m_displayXML["entryTitle"]).Text = Title;
                                                ((Gtk.Entry)m_displayXML["entryAuthor"]).Text = Author;
                                                ((Gtk.Entry)m_displayXML["entryCreatedOn"]).Text = CreatedOn;
                                                ((Gtk.Entry)m_displayXML["entryLastModifiedOn"]).Text = LastModifiedOn;
                                                ((Gtk.Entry)m_displayXML["entryStoredFiles"]).Text = StoredFiles;
                                                ((Gtk.Entry)m_displayXML["entryFileTypes"]).Text = FileTypes;
                                                return m_displayXML.GetWidget("showFrame") as Gtk.Widget;
                                        }
                                        catch(Exception e)
                                        {
                                                Console.WriteLine("COdb Display: "+e.Message);
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
                                                Glade.XML m_displayXML = new Glade.XML(null,"codb.glade","EditWindow",null);
                                                m_displayXML.Autoconnect(this);
                                                ((Gtk.Entry)m_displayXML["editFilePath"]).Text = Path;
                                                ((Gtk.Entry)m_displayXML["itemTitle"]).Text = Title;
                                                ((Gtk.Entry)m_displayXML["itemAuthor"]).Text = Author;
                                                return m_displayXML.GetWidget("editFrame") as Gtk.Widget;
                                        }
                                        catch(Exception e)
                                        {
                                                Console.WriteLine("COdb Edit: "+e.Message);
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

                        public void OnTitleEntered(object o, EventArgs args)
                        {
                                Title = ((Gtk.Entry)o).Text ;
                        } 
                        public void OnAuthorEntered(object o, EventArgs args)
                        {
                                Author = ((Gtk.Entry)o).Text ;
                        }
                        
                        public static string Description
                        {
                                get
                                {
                                        return s_sDescription;
                                }
                        }
                        
                        public string CreatedOn
                        {
                                get { return m_sCreatedOn; }
                                set { m_sCreatedOn = value; }
                        }
                        public string LastModifiedOn
                        {
                                get { return m_sLastModifiedOn; }
                                set { m_sLastModifiedOn = value; }
                        }
                        public string StoredFiles
                        {
                                get { return m_sStoredFiles; }
                                set { m_sStoredFiles = value; }
                        }
                        public string FileTypes
                        {
                                get { return m_sFileTypes; }
                                set { m_sFileTypes = value; }
                        }

                }
}
