using System.Collections.Generic;

namespace Curiosity.Missions.Client.net.DataClasses.Police
{
    class LinesOfSpeech
    {
        // Officer ID Check
        static public List<string> OfficerNormalQuotes = new List<string> { "Can i see some ID?", "ID, please.", "License and registration." };
        static public List<string> OfficerAggresiveQuotes = new List<string> { "Show me your ID.", "Give me your ID.", "Give me your fucking license.", "Show me your info." };

        // Driver Response to ID Check
        static public List<string> DriverResponseNormalIdentity = new List<string> { "Yeah, sure.", "Okay, here you go.", "There.", "Take it, just hurry up please.", "*Gives ID*", "Okay, here you go.", "Sure thing!", "Alright, no problem.", "Yep, there it is." };
        static public List<string> DriverResponseRushedIdentity = new List<string> { "Take it, just hurry up please.", "I really don't have the time for this...", "What was I stopped for again?", "Sure thing, did I do something wrong?", "Is this necessary?" };
        static public List<string> DriverResponseAngeredIdentity = new List<string> { "Is it because I'm black?", "Just take it already...", "Uhm.. Sure... Here.", "But I've done nothing wrong, sir!", "Pig.", "Why dont you go and fight real crime?" };
    }
}
