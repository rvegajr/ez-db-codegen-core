using EzDbSchema.Core.Enums;

namespace EzDbCodeGen.Core
{
    public class CodeGenerator : CodeGenBase, ITemplateRenderer
    {
        public CodeGenerator(string configurationFileName) : base()
        {
            this.ConfigurationFileName = configurationFileName;
        }
    }
}
