﻿using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot.Methods
{
    class PingHandler
    {
        static Random Random = new Random();

        static List<string> lines = new List<string>()
        {
            "My wife told me to stop impersonating a flamingo. I had to put my foot down. ",
            "I went to buy some camo pants but couldn’t find any.",
            "I failed math so many times at school, I can’t even count.",
            "I used to have a handle on life, but then it broke.",
            "I was wondering why the frisbee kept getting bigger and bigger, but then it hit me.",
            "I heard there were a bunch of break-ins over at the car park. That is wrong on so many levels.",
            "I want to die peacefully in my sleep, like my grandfather… Not screaming and yelling like the passengers in his car.",
            "When life gives you melons, you might be dyslexic.",
            "Don’t you hate it when someone answers their own questions? I do.",
            "It takes a lot of balls to golf the way I do.",
            "I told him to be himself; that was pretty mean, I guess. ",
            "I know they say that money talks, but all mine says is ‘Goodbye.’",
            "My father has schizophrenia, but he’s good people.",
            "The problem with kleptomaniacs is that they always take things literally.",
            "I can’t believe I got fired from the calendar factory. All I did was take a day off.",
            "Most people are shocked when they find out how bad I am as an electrician.",
            "Never trust atoms; they make up everything.",
            "My wife just found out I replaced our bed with a trampoline. She hit the ceiling! ",
            "I was addicted to the hokey pokey, but then I turned myself around.",
            "I used to think I was indecisive. But now I’m not so sure.",
            "Russian dolls are so full of themselves.",
            "The easiest time to add insult to injury is when you’re signing someone’s cast.",
            "Light travels faster than sound, which is the reason that some people appear bright before you hear them speak. ",
            "My therapist says I have a preoccupation for revenge. We’ll see about that.",
            "A termite walks into the bar and asks, ‘Is the bar tender here?’",
            "A told my girlfriend she drew her eyebrows too high. She seemed surprised.",
            "People who use selfie sticks really need to have a good, long look at themselves. ",
            "Two fish are in a tank. One says, ‘How do you drive this thing?’",
            "I always take life with a grain of salt. And a slice of lemon. And a shot of tequila.",
            "Just burned 2,000 calories. That’s the last time I leave brownies in the oven while I nap.",
            "Always borrow money from a pessimist. They’ll never expect it back.",
            "Build a man a fire and he’ll be warm for a day. Set a man on fire and he’ll be warm for the rest of his life.",
            "I don’t suffer from insanity—I enjoy every minute of it.",
            "The last thing I want to do is hurt you; but it’s still on the list.",
            "The problem isn’t that obesity runs in your family. It’s that no one runs in your family.",
            "Today a man knocked on my door and asked for a small donation toward the local swimming pool. I gave him a glass of water.",
            "I’m reading a book about anti-gravity. It’s impossible to put down.",
            "‘Doctor, there’s a patient on line one that says he’s invisible.’ ‘Well, tell him I can’t see him right now.’",
            "Atheism is a non-prophet organization.",
            "A recent study has found that women who carry a little extra weight live longer than the men who mention it.",
            "The future, the present, and the past walk into a bar. Things got a little tense.",
            "Before you criticize someone, walk a mile in their shoes. That way, when you do criticize them, you’re a mile away and you have their shoes.",
            "Last night my girlfriend was complaining that I never listen to her… or something like that.",
            "Maybe if we start telling people their brain is an app, they’ll want to use it.",
            "If a parsley farmer gets sued, can they garnish his wages?",
            "I got a new pair of gloves today, but they’re both ‘lefts,’ which on the one hand is great, but on the other, it’s just not right.",
            "I didn’t think orthopedic shoes would help, but I stand corrected.",
            "I was riding a donkey the other day when someone threw a rock at me and I fell off. I guess I was stoned off my ass.",
            "People who take care of chickens are literally chicken tenders.",
            "It was an emotional wedding. Even the cake was in tiers.",
            "I just got kicked out of a secret cooking society. I spilled the beans.",
            "What’s a frog’s favorite type of shoes? Open toad sandals.",
            "Blunt pencils are really pointless.",
            "6:30 is the best time on a clock, hands down.",
            "Two wifi engineers got married. The reception was fantastic.",
            "Just got fired from my job as a set designer. I left without making a scene.",
            "What’s the difference between ignorance and apathy? I don’t know and I don’t care.",
            "One of the cows didn’t produce milk today. It was an udder failure.",
            "Adam & Eve were the first ones to ignore the Apple terms and conditions.",
            "Refusing to go to the gym is a form of resistance training.",
            "If attacked by a mob of clowns, go for the juggler.",
            "The man who invented Velcro has died. RIP.",
            "Despite the high cost of living, it remains popular.",
            "A dung beetle walks into a bar and asks, ‘Is this stool taken?’",
            "I can tell when people are being judgmental just by looking at them.",
            "The rotation of Earth really makes my day.",
            "Well, to be Frank with you, I’d have to change my name.",
            "My friend was explaining electricity to me, but I was like, ‘Watt?’",
            "What if there were no hypothetical questions?",
            "Are people born with photographic memories, or does it take time to develop?",
            "The world champion tongue twister got arrested. I hear they’re going to give him a tough sentence.",
            "Pollen is what happens when flowers can’t keep it in their plants.",
            "A book fell on my head the other day. I only have my shelf to blame though.",
            "Communist jokes aren’t funny unless everyone gets them.",
            "Geology rocks, but geography’s where it’s at.",
            "I buy all my guns from a guy called T-Rex. He’s a small arms dealer.",
            "My friend’s bakery burned down last night. Now his business is toast.",
            "Four fonts walk into a bar. The bartender says, ‘Hey! We don’t want your type in here!’",
            "If you don’t pay your exorcist, do you get repossessed?",
            "When the cannibal showed up late to the buffet, they gave him the cold shoulder.",
            "A Mexican magician tells the audience he will disappear on the count of three. He says, ‘Uno, dos…” and poof! He disappeared without a tres.",
            "Fighting for peace is like screwing for virginity.",
            "A ghost walked into a bar and ordered a shot of vodka. The bartender said, ‘Sorry, we don’t serve spirits here.’",
            "The man who invented knock-knock jokes should get a no bell prize.",
            "I bought the world’s worst thesaurus yesterday. Not only is it terrible, it’s also terrible.",
            "A blind man walked into a bar… and a table… and a chair…",
            "A Freudian slip is when you mean one thing and mean your mother.",
            "I went to a seafood disco last week, but ended up pulling a mussel.",
            "The first time I got a universal remote control, I thought to myself, ‘This changes everything.’",
            "How do you make holy water? You boil the hell out of it.",
            "I saw a sign the other day that said, ‘Watch for children,’ and I thought, ‘That sounds like a fair trade.’",
            "Whiteboards are remarkable.",
            "I threw a boomerang a couple years ago; I know live in constant fear.",
            "I put my grandma on speed dial the other day. I call it insta-gram.",
            "I have a few jokes about unemployed people, but none of them work.",
            "‘I have a split personality,’ said Tom, being Frank.",
            "My teachers told me I’d never amount to much because I procrastinate so much. I told them, “Just you wait!”",
            "Will glass coffins be a success? Remains to be seen.",
            "Did you hear about the guy whose whole left side got amputated? He’s all right now.",
            "The man who survived both mustard gas and pepper spray is a seasoned veteran now.",
            "Have you heard about the new restaurant called ‘Karma?’ There’s no menu—you get what you deserve.",
        };

        public async static Task ReactToPing(SocketUserMessage message, SocketCommandContext context)
        {
            await message.DeleteAsync();

            var msg = await context.Channel.SendMessageAsync(lines[Random.Next(lines.Count)]);

            await Task.Delay(7500);

            await msg.DeleteAsync();
        }
    }
}
