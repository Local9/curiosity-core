namespace Curiosity.Core.Client.Environment.Data
{
    public static class BlipInfo
    {
        static Dictionary<string, int> _blips = new()
        {
            { "akula", 602 },
            { "avenger", 589 },
            { "besra", 424 },
            { "blazer5", 512 },
            { "chernobog", 603 },
            { "taxi", 56 },
            { "nightshark", 225 },
            { "rhino", 421 },
            { "lazer", 424 },
            { "hydra", 424 },
            { "insurgent", 426 },
            { "insurgent2", 426 },
            { "insurgent3", 426 },
            { "limo2", 460 },
            { "phantom2", 528 },
            { "boxville5", 529 },
            { "ruiner2", 530 },
            { "dune4", 531 },
            { "dune5", 531 },
            { "wastelander", 532 },
            { "voltic2", 533 },
            { "technical2", 534 },
            { "technical3", 534 },
            { "technical", 534 },
            { "apc", 558 },
            { "oppressor", 559 },
            { "oppressor2", 559 },
            { "halftrack", 560 },
            { "dune3", 561 },
            { "tampa3", 562 },
            { "trailersmall2", 563 },
            { "alphaz1", 572 },
            { "bombushka", 573 },
            { "havok", 574 },
            { "howard", 575 },
            { "hunter", 576 },
            { "microlight", 577 },
            { "mogul", 578 },
            { "molotok", 579 },
            { "nokota", 580 },
            { "pyro", 581 },
            { "rogue", 582 },
            { "starling", 583 },
            { "seabreeze", 584 },
            { "tula", 585 },
            { "stromberg", 595 },
            { "deluxo", 596 },
            { "thruster", 597 },
            { "khanjali", 598 },
            { "riot2", 599 },
            { "volatol", 600 },
            { "barrage", 601 },
        };

        public static int GetBlipSprite(this Model model)
        {
            string modelName = $"{model}";
            if (_blips.ContainsKey(modelName))
                return _blips[modelName];

            if (model.IsBike)
                return 348;

            if (model.IsBoat)
                return 427;

            if (model.IsHelicopter)
                return 422;

            if (model.IsPlane)
                return 423;

            return 255;
        }
    }
}
