﻿using Curiosity.Shared.Server.net.Helpers;

namespace Curiosity.Server.net.Helpers
{
    class Logger
    {
        public static void Debug(string message)
        {
            if (Server.DEBUG)
                Log.Debug(message);
        }
    }
}
