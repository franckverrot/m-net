/*---------------------------------------------------------------------
 *	@file:CrashScreen.cs
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
using MediaNET;
using Gtk;
using Gdk;

public class CrashScreen
{
    [Glade.Widget("CrashScreenWindow")] Gtk.Window m_cWindow;
    [Glade.Widget] Gtk.TextView tv;
    Gtk.TextBuffer tb;
    
    public Gtk.Window Window
    {
        get
        {
            return m_cWindow;
        }
    }
    
    public CrashScreen(string err)
    {
        Glade.XML glade = new Glade.XML(null,MediaNET.MediaNET.GladeCommonFilename, "CrashScreenWindow", null);
        TextBuffer tb = new TextBuffer(new TextTagTable());
        tb.SetText(err);
        ((Gtk.TextView)glade["text"]).Buffer = tb;
        glade.BindFields(this);
        glade.Autoconnect(this);
        m_cWindow.Show();
        while (Gtk.Application.EventsPending()) {
            Gtk.Application.RunIteration();
        }
        System.Threading.Thread.Sleep(2000);
    }
}
