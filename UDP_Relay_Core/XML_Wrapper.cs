using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace UDP_Relay_Core
{
    /// <summary>
    /// Class for reading and writing XML files
    /// </summary>
    public class XML_Wrapper
    {
        protected XmlDocument xmlDocument;

        /// <summary>
        /// Constructor for creating a new XML file.
        /// </summary>
        /// <param name="documentPath">Either relative or full path and filename for XML file.</param>
        public XML_Wrapper(string documentPath)
        {
            xmlDocument = new XmlDocument();
            xmlDocument.Load(documentPath);
        }

        /// <summary>
        /// Gets a node from the XML file.
        /// </summary>
        /// <param name="xPath">XML path for the node to get.</param>
        /// <returns>The first XmlNode that matches the XPath query or null if no matching node is found.</returns>
        public XmlNode GetNode(string xPath)
        {
            return xmlDocument.SelectSingleNode(xPath);
        }

        /// <summary>
        /// Gets the value of a XML node.
        /// </summary>
        /// <param name="xPath">XML path for the node to get.</param>
        /// <returns></returns>
        public string GetNodeValue(string xPath)
        {
            return GetNode(xPath).InnerText;
        }

        /// <summary>
        /// Sets the value of a XML node.
        /// </summary>
        /// <param name="xPath">XML path for the node to get.</param>
        /// <param name="value"></param>
        public void SetNodeValue(string xPath, object value)
        {
            GetNode(xPath).InnerText = Convert.ToString(value);
        }

        /// <summary>
        /// Saves changes to the XML file.
        /// </summary>
        public void Save()
        {
            xmlDocument.Save(xmlDocument.BaseURI);
        }

        /// <summary>
        /// Gets a node value as an integer.
        /// </summary>
        /// <param name="xPath">XML path for the node to get.</param>
        /// <returns></returns>
        public int GetInt(string xPath)
        {
            return Convert.ToInt32(GetNodeValue(xPath));
        }

        /// <summary>
        /// Sets a node value as an integer.
        /// </summary>
        /// <param name="xPath">XML path for the node to get.</param>
        /// <param name="value"></param>
        public void SetInt(string xPath, int value)
        {
            SetNodeValue(xPath, value.ToString());
        }

        /// <summary>
        /// Gets a node value as an IPAddress.
        /// </summary>
        /// <param name="xPath">XML path for the node to get.</param>
        /// <returns></returns>
        public IPAddress GetIPAddress(string xPath)
        {
            return IPAddress.Parse(GetNodeValue(xPath));
        }

        /// <summary>
        /// Sets a node value as an IPAddress.
        /// </summary>
        /// <param name="xPath">XML path for the node to get.</param>
        /// <param name="value"></param>
        public void SetIPAddress(string xPath, IPAddress value)
        {
            SetNodeValue(xPath, value.ToString());
        }

        /// <summary>
        /// Gets a node value as an IPEndPoint.
        /// </summary>
        /// <param name="xpath">XML path for the node to get.</param>
        /// <returns></returns>
        public IPEndPoint GetIPEndPoint(string ipAddressXPath, string portXPath)
        {
            IPAddress ip = GetIPAddress(ipAddressXPath);
            int port = GetInt(portXPath);
            return new IPEndPoint(ip, port);
        }
    }
}
