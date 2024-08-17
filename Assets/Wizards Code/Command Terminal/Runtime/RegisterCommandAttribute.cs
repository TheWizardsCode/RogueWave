using System;
using System.Diagnostics;
using System.Reflection;

namespace WizardsCode.CommandTerminal
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterCommandAttribute : Attribute
    {
        int _MinArgs = 0;
        int _MaxArgs = -1;
        public string _Group = string.Empty;
        private int _RuntimeLevel = 99999;

        public string Group
        {
            get {
                return _Group;
            }
            set { _Group = value; }
        }

        public int MinArgCount {
            get { return _MinArgs; }
            set { _MinArgs = value; }
        }

        public int MaxArgCount {
            get { return _MaxArgs; }
            set { _MaxArgs = value; }
        }

        /// <summary>
        /// The RuntimeLevel is used to set what level someone needs to be in 
        /// order to be able to use this comman at runtime. A RuntimeLevel of
        /// 0 will mean nobody can use it at runtime, other than in debug builds.
        /// </summary>
        public int RuntimeLevel
        {
            get { return _RuntimeLevel; }
            set { _RuntimeLevel = value; }
        }

        public string Name { get; set; }
        public string Help { get; set; }
        
        public RegisterCommandAttribute(string command_name = null) {
            Name = command_name;
        }
    }
}
