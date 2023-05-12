using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.File.Options
{
    /// <summary>
    /// Options related to EventStoreFile
    /// </summary>
    public class EventStoreFileOptions
    {
        /// <summary>
        /// The folder where event instances will be serialized.
        /// </summary>
        public string Folder { get; set; } = "./";
    }
}
