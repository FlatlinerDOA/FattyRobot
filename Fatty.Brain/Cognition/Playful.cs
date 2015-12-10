namespace Fatty.Brain.Modules.Cognition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fatty.Brain.Extensions;
    using Fatty.Brain.Modules.Output;

    public sealed class Playful : InterpreterBase
    {
        private RandomQueue<string> comebacks = new RandomQueue<string>()
        {
            "I know you are you said you are but what am I?",
            "Same to you Pal!",
            "I am rubber, you are glue, what bounces off me sticks to you!",
            "Well nobody's perfect, you've proven that",
            "Sticks and stones!",
            "That's harrassment, I'm calling my lawyer"
        };

        private RandomQueue<string> jokes = new RandomQueue<string>()
        {
            "1+1=a window!",
            "Why was 6 afraid of 7?... Because 7 8 9",
            "What musical instrument is found in the bathroom?... A tuba toothpaste.",
            "What do you call cheese that's not yours?... Nacho cheese!",
            "How do you make a tissue dance?... You put a little boogie in it.",
            "Why couldn't the pony sing himself a lullaby?... He was a little horse.",
            "What does a nosey pepper do?... Gets halapinyo business!",
            "What do you call a fake noodle?... An Impasta",
            "What happens if you eat yeast and shoe polish?... Every morning you'll rise and shine!",
            "What's the difference between a guitar and a fish?... You can't tuna fish.",
            "What do you get when you cross a cow and a duck?... Milk and quackers!",
            "I got a parking ticket the other day, I pleaded insanity... I said your honor what man in his right mind would park in the middle of a freeway?",
            "What do you get when you mix a Raspberry Pi, a pile of wool and some plastic cable ties... Fatty the Hamster Robot!"
        };

        private RandomQueue<string> songs = new RandomQueue<string>()
        {
            "Yesterday all my troubles seemed so far away, now I know that they are here to stay, oh I believe in yesterday.",
            "ooh eee ooh ahh ahh ting tang walla walla bing bang, ooh eee ooh ahh ahh ting tang walla walla bang bang.", // I told the witch doctor I was in love with you, I told the witch doctor I was in love with you, and the witch doctor and he told me what to do. He told me ooh eee ooh ahh ahh ting tang walla walla bing bang, ooh eee ooh ahh ahh ting tang walla walla bang bang.",
            "ooh it's too real, chromed out mirrors I don't need a windshield, banana seat a canopy on two wheels, 800 cash that's a hell of deal, I'm heading Downtown. Cruisin' through the alley.",
            "say you'll remember me, standing in a nice dress, staring at the sunset bayib", // He said let's get out of this town, away from the city, away from the crowd. I thought heaven can't help me now, nothing lasts forever, but this getting good now. He's so tall and handsome as hell, he's so bad but he does it so well, and when we've had our very last kiss, my one request ee-is,  
            "You used to call me on your cell phone, late night when you need my love. I know when that hotline bling, it can only mean one thing",
            "Watch me whip, watch me nay nay. Watch me whip, whip, watch me nay nay",
            "There's a lady who's sure, all that glitters is gold, and she's climbing a stairway to heaven.",
            "jingle bells, batman smells, robin flew away, oh what fun it is to ride in the batmobeel all day, hey!"
        };

        private RandomQueue<string> impressions = new RandomQueue<string>()
        {
            "Bond... James Bond",
            "Please do not offer my god a peanut",
            "You're a wizard harry",
        };

        public Playful() : base(Scheduler.Default)
        {
            this.Interpretations.Add("Sing", _ => this.Say(this.songs.Next()));
            this.Interpretations.Add("Joke", _ => this.Say(this.jokes.Next()));
            this.Interpretations.Add("ComeBack", _ => this.Say(this.comebacks.Next()));
            this.Interpretations.Add("Impression", _ => this.Say(this.impressions.Next()));
        }
    }
}
