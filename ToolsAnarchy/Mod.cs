
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsAnarchy
{
    public class ToolAnarchyMod : IUserMod
    {
        public string Name
        {
            get
            {
                return "Road Anarchy";
            }
        }
        public string Description
        {
            get
            {
                return "Remove restrictions for all roads and railway tracks.";
            }
        }
    }
}
