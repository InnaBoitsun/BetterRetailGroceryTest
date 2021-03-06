﻿namespace Orckestra.Composer.ViewEngine.HandleBarsBlockHelpers
{
    internal class IfNotEqualsBlockHelper : CompareBlockHelper<string>
    {
        public override string HelperName { get { return "if_neq"; } }

        protected override bool Compare(string a, string b)
        {
            return !(a.Equals(b));
        }

        protected override bool TryParse(object o, out string v)
        {
            v = (o ?? "").ToString();
            return true;
        }
    }
}
