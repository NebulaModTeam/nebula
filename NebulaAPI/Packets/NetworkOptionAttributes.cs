using System;

namespace NebulaAPI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NetworkOptionsAttribute : Attribute 
    {
        public bool Reliable = true;
    }
}
