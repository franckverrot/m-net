/*---------------------------------------------------------------------
 *	@file:CPicture.cs
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

namespace MediaNET.Components
{
	[Serializable]
	public abstract class CPicture : CMedia
	{
		public CPicture()
		{

		}
      
		~CPicture()
		{

		}
   
		private int m_iDimensionsX = 0;
		private int m_iDimensionsY = 0;
   
		public int DimensionsX
		{
			get
			{
				return m_iDimensionsX;
			}
			set
			{
				if (this.m_iDimensionsX != value)
					this.m_iDimensionsX = value;
			}
		}
      
		public int DimensionsY
		{
			get
			{
				return m_iDimensionsY;
			}
			set
			{
				if (this.m_iDimensionsY != value)
					this.m_iDimensionsY = value;
			}
		}
      
	}
}
