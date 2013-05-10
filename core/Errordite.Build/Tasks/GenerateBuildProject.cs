using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using System.Linq;
using Microsoft.Build.Utilities;

namespace Errordite.Build.Tasks
{
    public class GenerateBuildProject : Task
    {
        private string _properties;

        #region properties

        /// <summary>
        /// Gets or sets the config file which is used to read in the properties.
        /// </summary>
        /// <value>
        /// The config file.
        /// </value>
        [Required]
        public string ConfigFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>
        /// The working directory.
        /// </value>
        public string WorkingDirectory
        {
            get;
            set;
        }

        [Required]
        public string Environment { get; set; }

        [Required]
        public string SourceProjectPath { get; set; }

        [Required]
        public string OutputProjectPath { get; set; }

        /// <summary>
        /// Gets or sets the properties that are passed to the build.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public string Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value.Replace("\r\n", "").Trim();
            }
        }

        #endregion

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            Log.LogMessage("Current working directory={0}", Directory.GetCurrentDirectory());

            if (!string.IsNullOrEmpty(WorkingDirectory))
            {
                try
                {
                    Directory.SetCurrentDirectory(WorkingDirectory);
                }
                catch (Exception ex)
                {
                    Log.LogMessage(ex.Message);
                }
            }

            try
            {
                Log.LogMessage("Loading Source Project file {0}", SourceProjectPath);

                XDocument xmlDoc = XDocument.Load(SourceProjectPath);

                XNamespace rootNamespace = xmlDoc.Root.Name.NamespaceName;

                XElement propertyNode = new XElement(rootNamespace + "PropertyGroup");

                Log.LogMessage("Loading Config file {0}", ConfigFile);

                XDocument xmlConfigDoc = XDocument.Load(ConfigFile);

                var parent = xmlConfigDoc.Descendants("Replacements");

                foreach (var node in parent.Descendants())
                {
                    string propertyValue = string.Empty;

                    if (node.Attributes(Environment).FirstOrDefault() == null)
                    {
                        if (node.Attributes("theRest").FirstOrDefault() != null)
                        {
                            propertyValue = node.Attributes("theRest").First().Value;
                        }
                    }
                    else
                    {
                        propertyValue = node.Attributes(Environment).First().Value;
                    }

                    propertyNode.Add(new XElement(rootNamespace + node.Name.ToString(), propertyValue));
                    Log.LogMessage("Adding property: {0}={1}", node.Name, propertyValue);
                }

                List<string> buildParams = Properties.Split(';').ToList();

                foreach (string p in buildParams)
                {
                    if (!string.IsNullOrEmpty(p))
                    {
                        string[] prop = p.Split('=');

                        prop[0] = prop[0].Trim();
                        prop[1] = prop[1].Trim();

                        Log.LogMessage("Adding property: {0}={1}", prop[0], prop[1]);

                        propertyNode.Add(new XElement(rootNamespace + prop[0], prop[1]));
                    }
                }

                xmlDoc.Root.AddFirst(propertyNode);
                xmlDoc.Save(OutputProjectPath);
            }
            catch (Exception e)
            {
                Log.LogMessage("Error:" + e);
            }

            return true;
        }
    }
}
