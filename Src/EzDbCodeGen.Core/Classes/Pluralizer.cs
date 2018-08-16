using System;
using System.Collections.Generic;
using System.Text;

namespace EzDbCodeGen.Core
{
    public class Pluralizer
    {
        public Dictionary<string, string> SingularToPlural = new Dictionary<string, string>();
        public Dictionary<string, string> PluralToSingular = new Dictionary<string, string>();
        public void AddWord(string singleForm, string pluralForm)
        {
            if (!SingularToPlural.ContainsKey(singleForm))
                SingularToPlural.Add(singleForm, pluralForm);

            if (!PluralToSingular.ContainsKey(pluralForm))
                PluralToSingular.Add(pluralForm, singleForm);
        }

        public bool IsSingular(string word)
        {
            return (new Pluralize.NET.Pluralizer().Singularize(word)).Equals(word);
        }
        public bool IsPlural(string word)
        {
            return (new Pluralize.NET.Pluralizer().Pluralize(word)).Equals(word);
        }
        public string Singularize(string word)
        {
            return (new Pluralize.NET.Pluralizer().Singularize(word));
        }
        public string Pluralize(string word)
        {
            return (new Pluralize.NET.Pluralizer().Pluralize(word));
        }
        public static Pluralizer instance;

        public Pluralizer()
        {
        }
        public static Pluralizer Instance
        {
            get
            {

                if (instance == null)
                {
                    instance = new Pluralizer();
                }
                return instance;
            }
        }
    }
}
