/*---------------------------------------------------------------------
 *	@file:AboutScreen.cs
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

public class AboutScreen
{
	[Glade.Widget("AboutScreenWindow")]
	Gtk.Window m_cWindow;
    
	public Gtk.Window Window
	{
		get
		{
			return m_cWindow;
		}
	}
    
	public AboutScreen()
	{
		Glade.XML glade = new Glade.XML(null,MediaNET.MediaNET.GladeCommonFilename, "AboutScreenWindow", null);
		glade.BindFields(this);
		glade.Autoconnect(this);
        Gdk.Pixbuf pb = Gdk.Pixbuf.LoadFromResource("Franck.jpg");
        int DimensionsY = pb.Height;
        int DimensionsX = pb.Width;
        int W = 200,H=200;
        ((Gtk.Image)glade["image"]).Pixbuf = pb.ScaleSimple(W*DimensionsX/DimensionsY,H,Gdk.InterpType.Bilinear);
		m_cWindow.Show();
		while (Gtk.Application.EventsPending()) 
		{
			Gtk.Application.RunIteration();
		}
	}
}
