namespace Curiosity.Chat.Server.net
{
    static class ProfanityFilter
    {
        // METHOD: containsProfanity
        static public bool ContainsProfanity(this string checkStr)
        {
            bool badwordpresent = false;

            string[] inStrArray = checkStr.Split(new char[] { ' ' });

            string[] words = ProfanityArray();

            // LOOP THROUGH WORDS IN MESSAGE
            for (int x = 0; x < inStrArray.Length; x++)
            {
                // LOOP THROUGH PROFANITY WORDS
                for (int i = 0; i < words.Length; i++)
                {
                    // IF WORD IS PROFANITY, SET FLAG AND BREAK OUT OF LOOP
                    //if (inStrArray[x].toString().toLowerCase().equals(words[i]))
                    if (inStrArray[x].ToLower() == words[i].ToLower())
                    {
                        badwordpresent = true;
                        break;
                    }
                }
                // IF FLAG IS SET, BREAK OUT OF OUTER LOOP
                if (badwordpresent == true) break;
            }

            return badwordpresent;
        }
        // ************************************************************************
        
        // ************************************************************************
        // METHOD: profanityArray()
        // METHOD OF PROFANITY WORDS
        static public string[] ProfanityArray()
        {
            // THESE WERE UPDATED TO USE THE SAME BADWORDS FROM FACESOFMBCFBAPP
            string[] words = { "nigger", "niggar", "nigga", "n1gga", "nigg4", "n1gg4", "n1g4", "nigg", "n1gg", "niga", "n1g", "nig" };
            return words;
        }
    }
}