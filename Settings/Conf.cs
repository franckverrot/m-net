/*---------------------------------------------------------------------
 *	@file:Conf.cs
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
namespace MediaNET
{
        using System;
        using System.Xml;
        using System.Collections;

		///<summary>
		/// Conf is a clone to the (currently) non portable GConf#
		/// configuration system. He use to store all it internals data
		/// to an XML file, as GConf, and could be a good replacement 
		///</summary>
        public class Conf
        {
                static protected volatile Conf s_vInstance;
                protected static string s_sLock = "Conf lock";
                Hashtable m_Matches = new Hashtable ();

				///<summary>
				/// Default constructor
				/// It tries to read a file named configuration.xml
				/// You should not be able to change the file's path
				/// cause you can't do it with GConf# either
				///</summary>				
                protected Conf()
                {
                        try
                        {
                                XmlDocument doc = new XmlDocument();
                                doc.Load("configuration.xml");
                                
                                string s = null;
                                XmlNodeList list = null;
                                list = doc.GetElementsByTagName("entry");
                                m_Matches.Clear();
                                foreach(XmlNode n in list)
                                {
                                        m_Matches.Add(n["key"].InnerText,n["value"].InnerText);
                                }
                        }
                        catch(Exception e)
                        {
                                //Console.WriteLine("Can't load configuration... "+e.Message);
                        }
                }
                
                ///<summary>
				/// Client() returns a instance on the Conf singleton
				/// It's a basic multithreaded-safe double lock singleton 
				///</summary>
                public static Conf Client()
                {
                        if (s_vInstance == null) 
                        {
                                lock (s_sLock) 
                                {
                                        if (s_vInstance == null) 
                                                s_vInstance = new Conf();
                                }
                        }
                        return s_vInstance;
                }

				///<summary>
				/// Set assign a key ( string ) to another string value named val 
				///</summary>
                public void Set (string key, string val)
                {
                        m_Matches[key] = val;
                }

				///<summary>
				/// Get returns the value referenced by its key, else NULL object
				///</summary>
                public object Get (string key)
                {
                        if(!m_Matches.Contains(key)) m_Matches[key] = key; 
                        return m_Matches[key];
                }

				///<summary>
				/// SyggestSync is a goodies to ask a proper sync of what's
				/// present in RAM, and what's present in the XML file.
				///</summary>
                public void SuggestSync ()
                {
                        XmlTextWriter writer = new System.Xml.XmlTextWriter("configuration.xml",null);
                        writer.Formatting = Formatting.Indented;
                        writer.WriteStartDocument();
                        writer.WriteStartElement("entries");

                        foreach(string key in m_Matches.Keys)
                        {
                                writer.WriteStartElement("entry");
                                writer.WriteElementString("key", key);
                                writer.WriteElementString("value", (string)m_Matches[key]);
                                writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                        writer.Flush();
                        writer.Close();
                }
        }

		///<summary>
		/// NoSuchKeyException is an exception raised
		/// It is not used here cause GConf# actually doesn't raise it, it prefers
		/// returning the same string instead, but this behaviour is not
		/// really standart.
		///</summary>
        public class NoSuchKeyException : Exception
        {
                public NoSuchKeyException ()
                        : base ("The requested Conf key was not found")
                        {
                        }

                public NoSuchKeyException (string key)
                        : base ("Key '" + key + "' not found in Conf")
                        {
                        }
        }
}

