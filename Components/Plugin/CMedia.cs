/*---------------------------------------------------------------------
 *	@file:CMedia.cs
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
using Gtk;
using Glade;

// Defines the main "Components" namespace
namespace MediaNET.Components
{
	/// Abstrac Media Class
	[Serializable]
	public abstract class CMedia : IShowable
	{
        protected string m_sTitle = "";
        protected string m_sPath = "";
		protected Guid m_iGuid = Guid.NewGuid();
		protected string m_sAuthor = "";
		protected string m_sLength = "";
        
        // Default contructor
		public CMedia()
		{
		}

        // Default destructor
		~CMedia()
		{
		}

        /** Parse the current file and load its characteristics. **/
        abstract public void loadFileCharacteristics();

        /** Save the current file and its characteristics. **/
        abstract public void saveFileCharacteristics();

		public virtual string Title
		{
			get
			{
				return m_sTitle;
			}
			set
			{
				if (this.m_sTitle != value)
					this.m_sTitle = value;
			}
		}

		public virtual string Path
		{
			get
			{
				return m_sPath;
			}
			set
			{
				if (this.m_sPath != value)
					this.m_sPath = value;
			}
		}

		public virtual Guid ID
		{
			get
			{
				return m_iGuid;
			}
			set
			{
				if (this.m_iGuid != value)
					this.m_iGuid = value;
			}
		}
		public virtual string GID
		{
			get
			{
				return (string)m_iGuid.ToString();
			}
		}

		public virtual string Author
		{
			get
			{
				return m_sAuthor;
			}
			set
			{
				if (this.m_sAuthor != value)
					this.m_sAuthor = value;
			}
		}

		public virtual string Length
		{
			get
			{
				return m_sLength;
			}
			set
			{
				if (this.m_sLength != value)
					this.m_sLength = value;
			}
		}


		public virtual string CompleteFileName 
		{
			get { return m_sPath; }
            set { m_sPath = value; }
		}

                        
		public virtual Array Summary  
		{
			get 
			{
				string [] s = {"This","is","a","virtual","class","dumbass","!"};
				return s;
			}
		} 

		public virtual Gtk.Widget Display 
		{
			get 
			{
				return null;
			}
		}
		public virtual Gtk.Widget Edit
		{
			get { return null;}
            set { }
		}
	}
}
