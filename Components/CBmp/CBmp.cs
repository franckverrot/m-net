/*---------------------------------------------------------------------
 *	@file:CBmp.cs
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
using Gtk;
using Glade;

namespace MediaNET.Components
{
        [Serializable]
                public class CBmp : CPicture
                {
                        static string s_sTypeIdentifier = "BMP Image File";
                        static string s_sPlayer = "gqview";
                        static string s_sExtension = ".bmp";
                        static string s_sDescription="This type handle all BMP image file.\nComment? Suggestion? root@dev-fr.com";

                        public CBmp(): this("")
                        {
                        }
                        public CBmp(string fullName)
                        {
                                Path = fullName;
                                DimensionsX = 0;
                                DimensionsY = 0;
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
                        
                        public override void loadFileCharacteristics()
                        {	
                                // We used path but in a wrong way... 
                                Path = System.IO.Path.GetFullPath(Path);
                                Gtk.Image image = new Gtk.Image(Path);
                                DimensionsY = image.Pixbuf.Height;
                                DimensionsX = image.Pixbuf.Width;
                                Title = System.IO.Path.GetFileName(Path);
                                Author = "";
                                FileStream oFileStream;
                                oFileStream = new FileStream(Path , FileMode.Open, FileAccess.Read);
                                Length = DimensionsX.ToString()+"x"+DimensionsY.ToString();
                                image.Dispose();
                                image = null;
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
                                                Glade.XML m_displayXML = new Glade.XML(null,"cbmp.glade","ShowWindow",null);
                                                m_displayXML.Autoconnect(this);
                                                ((Gtk.Label)m_displayXML["Information"]).Text = Title+" ("+Author+")";
                                                ((Gtk.Image)m_displayXML["showImage"]).Pixbuf = (new Gtk.Image(Path)).Pixbuf.ScaleSimple 
                                                        (W*DimensionsX/DimensionsY,H,Gdk.InterpType.Bilinear);
                                                return m_displayXML.GetWidget("showFrame") as Gtk.Widget;
                                        }
                                        catch(Exception e)
                                        {
                                                Console.WriteLine("CBmp Display: "+e.Message);
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
                                                Glade.XML m_displayXML = new Glade.XML(null,"cbmp.glade","EditWindow",null);
                                                m_displayXML.Autoconnect(this);
                                                ((Gtk.Entry)m_displayXML["editFilePath"]).Text = Path;
                                                ((Gtk.Entry)m_displayXML["itemTitle"]).Text = Title;
                                                ((Gtk.Entry)m_displayXML["itemAuthor"]).Text = Author;
                                                return m_displayXML.GetWidget("editFrame") as Gtk.Widget;
                                        }
                                        catch(Exception e)
                                        {
                                                Console.WriteLine("CBmp Edit: "+e.Message);
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
                        public void OnShowButton(object o, EventArgs args)
                        {
                                try
                                {
                                        Glade.XML m_displayXML = new Glade.XML(null,"cbmp.glade","FullscreenWindow",null);
                                        m_displayXML.Autoconnect(this);
                                        ((Gtk.Image)m_displayXML["showImage"]).Pixbuf = (new Gtk.Image(Path)).Pixbuf;
                                        ((Gtk.Window)m_displayXML["FullscreenWindow"]).Visible = true;
                                        ((Gtk.Window)m_displayXML["FullscreenWindow"]).Fullscreen();
                                        ((Gtk.EventBox)m_displayXML["eventbox1"]).ButtonPressEvent += new ButtonPressEventHandler (OnCloseFullScreen);
                                }
                                catch(Exception e)
                                {
                                        Console.WriteLine("CBmp Fullscreen aborted: "+e.Message);
                                }
                        }
                        
                        private void OnCloseFullScreen(object o, ButtonPressEventArgs args)
                        {
                                        ((Gtk.Window)((Gtk.EventBox)o).Parent).Hide();
                                        ((Gtk.Window)((Gtk.EventBox)o).Parent).Destroy();
                        }
                        
                        public static string Description
                        {
                                get
                                {
                                        return s_sDescription;
                                }
                        }
                }
}
