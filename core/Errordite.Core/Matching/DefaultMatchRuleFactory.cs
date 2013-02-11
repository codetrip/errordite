using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Domain.Error;
using Errordite.Core.Resources;

namespace Errordite.Core.Matching
{
    public interface IMatchRuleFactory
    {
        IEnumerable<IMatchRule> Create(Error instance);
        IEnumerable<IMatchRule> CreateEmpty();
        string Description { get; }
        string Name { get; }
        string Id { get; }
    }

    public class MethodAndTypeMatchRuleFactory : IMatchRuleFactory
    {
        public IEnumerable<IMatchRule> Create(Error instance)
        {
            yield return new PropertyMatchRule("MethodName", StringOperator.Equals, instance.ExceptionInfos.First().MethodName);
            yield return new PropertyMatchRule("Type", StringOperator.Equals, instance.ExceptionInfos.First().Type);
        }

        public IEnumerable<IMatchRule> CreateEmpty()
        {
            yield return new PropertyMatchRule("MethodName", StringOperator.Equals, string.Empty);
            yield return new PropertyMatchRule("Type", StringOperator.Equals, string.Empty);
        }

        public string Description
        {
            get { return CoreResources.MethodAndTypeMatchRuleFactory_Description; }
        }

        public string Name
        {
            get { return CoreResources.MethodAndTypeMatchRuleFactory_Name; }
        }

        public string Id
        {
            get { return "1"; }
        }
    }

    public class ModuleAndTypeMatchRuleFactory : IMatchRuleFactory
    {
        public IEnumerable<IMatchRule> Create(Error instance)
        {
            yield return new PropertyMatchRule("Module", StringOperator.Equals, instance.ExceptionInfos.First().Module);
            yield return new PropertyMatchRule("Type", StringOperator.Equals, instance.ExceptionInfos.First().Type);
        }

        public IEnumerable<IMatchRule> CreateEmpty()
        {
            yield return new PropertyMatchRule("Module", StringOperator.Equals, string.Empty);
            yield return new PropertyMatchRule("Type", StringOperator.Equals, string.Empty);
        }

        public string Description
        {
            get { return CoreResources.ModuleAndTypeMatchRuleFactory_Description; }
        }

        public string Name
        {
            get { return CoreResources.ModuleAndTypeMatchRuleFactory_Name; }
        }

        public string Id
        {
            get { return "2"; }
        }
    }
}