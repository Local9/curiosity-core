using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Curiosity.Callouts.Client
{
    public class Plugin : BaseScript
    {
        private static string resourceName = API.GetCurrentResourceName();
        private string resourcePath = $"resources/{resourceName}/callouts/";

        public Plugin()
        {
            //List<Base> objects = new List<Base>();
            //DirectoryInfo dir = new DirectoryInfo(resourcePath);

            //foreach (FileInfo file in dir.GetFiles("*.net.dll"))
            //{
            //    Assembly assembly = Assembly.LoadFrom(file.FullName);
            //    foreach (Type type in assembly.GetTypes())
            //    {
            //        if (type.IsSubclassOf(typeof(Base)) && type.IsAbstract == false)
            //        {
            //            Base b = type.InvokeMember(null,
            //                                       BindingFlags.CreateInstance,
            //                                       null, null, null) as Base;
            //            objects.Add(b);
            //        }
            //    }
            //}
        }
    }
}
