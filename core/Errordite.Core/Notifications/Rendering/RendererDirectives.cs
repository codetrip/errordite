
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Errordite.Core.Extensions;
using Errordite.Core.Notifications.Exceptions;

namespace Errordite.Core.Notifications.Rendering
{
    public partial class EmailRenderer
    {
        private Directive GetDirective()
        {
            string param = _bufferSb.ToString();
            if (_iteratorBlock != null && param.Suffix() == "End" && param.Prefix() == _iteratorBlock) //we have come to the end of an iterator
                return new RenderBlock(_iteratorBlock, _harvestingSb.ToString());

            if (param.Prefix() == "Placeholder" && !param.Suffix().IsNullOrEmpty())
                return new ChangePlaceholder(param.Suffix());

            if (_iteratorBlock != null) //we are harvesting, just pump the param back in
                return new OutputParam(param);

            if (param.Prefix() == "Master")
                return new MasterTemplate(param.Suffix());

            if (param.Suffix() == "Begin")
                return new BeginIterator(param.Prefix());

            if (param.Prefix() == "If")
                return new Conditional(param.Suffix());

            if (param == "Else")
                return new Else();

            if (param == "EndIf")
                return new EndConditional();

            return new ResolveParam(param);
        }

        public class ChangePlaceholder : Directive
        {
            private readonly string _placeholderName;

            public ChangePlaceholder(string placeholderName)
            {
                _placeholderName = placeholderName;
            }

            public override void MutateState(EmailRenderer renderer)
            {
                renderer.ChangeToPlaceholder(_placeholderName);
            }
        }

        public class BeginIterator : Directive
        {
            private readonly string _blockName;

            public BeginIterator(string blockName)
            {
                _blockName = blockName;
            }

            public override void MutateState(EmailRenderer renderer)
            {
                renderer._iteratorBlock = _blockName;
                renderer._harvestingSb.Clear();
                renderer._outputSb = renderer._harvestingSb;
            }
        }

        public class MasterTemplate : Directive
        {
            private readonly string _masterTemplateName;

            public MasterTemplate(string masterTemplateName)
            {
                _masterTemplateName = masterTemplateName;
            }

            public override void MutateState(EmailRenderer renderer)
            {
                renderer._masterTemplateName = _masterTemplateName;
            }
        }

        public class OutputParam : Directive
        {
            private readonly string _param;

            public OutputParam(string param)
            {
                _param = param;
            }

            public override void MutateState(EmailRenderer renderer)
            {
                renderer._outputSb.Append("$({0})".FormatWith(_param));
            }
        }

        public class RenderBlock : Directive
        {
            private readonly string _blockName;
            private readonly string _blockTemplate;

            public RenderBlock(string blockName, string blockTemplate)
            {
                _blockName = blockName;
                _blockTemplate = blockTemplate;
            }

            public override void MutateState(EmailRenderer renderer)
            {
                var iteratorParamGroups = renderer._emailParams.Where(x => IsIteratorParam(x.Key))
                    .Select(x => new { param = x.Key.Prefix("::"), group = x.Key.Suffix("::"), value = x.Value})
                    .GroupBy(x => x.group); 

                //this gives us groups of parameters by iteration.. 
                //param = e.g. URL or Person.Name

                renderer._outputSb = renderer._finalSb;

                foreach (var iteratorParamGroup in iteratorParamGroups)
                {
                    var iteratorRenderer = new EmailRenderer(renderer._config);

                    var emailParamsForIterator = renderer._emailParams.Concat(iteratorParamGroup
                        .Select(iteratorParam => new KeyValuePair<string, string>("{0}:Item{1}".FormatWith(_blockName, GetPropertyForIterator(iteratorParam.param)), iteratorParam.value)))
                        .ToDictionary(x => x.Key, x => x.Value);

                    var renderedIteration = iteratorRenderer.RenderFromTemplate(_blockTemplate, emailParamsForIterator);
                    
                    renderer._outputSb.Append(renderedIteration);
                }

                renderer._iteratorBlock = null;
            }

            private string GetPropertyForIterator(string paramName)
            {
                return paramName.Substring(_blockName.Length);
            }

            private bool IsIteratorParam(string paramName)
            {
                return paramName.StartsWith(_blockName + "::") || paramName.StartsWith(_blockName + ".");
            }
        }

        public class ResolveParam : Directive
        {
            private string _param;

            public ResolveParam(string param)
            {
                _param = param;
            }

            public override void MutateState(EmailRenderer renderer)
            {
                if (renderer._iteratorBlock == null)
                {
                    string value;

                    if (!renderer._emailParams.TryGetValue(_param, out value))
                        return;
                        //we were throwing here but it makes it to annoying in terms of placeholders
                        //that are not always required

                    renderer._outputSb.Append(value);
                }
            }
        }

        public class Conditional : Directive
        {
            private readonly string _param;
            internal StringBuilder PreviousSb;
            internal StringBuilder ElseSb;

            public Conditional(string param)
            {
                _param = param;
            }

            public override void MutateState(EmailRenderer renderer)
            {
                string conditionalValue;
                if (!renderer._emailParams.TryGetValue(_param, out conditionalValue))
                    throw new ErrorditeEmailParameterNotFoundException(_param);

                PreviousSb = renderer._outputSb;

                renderer._conditionals.Push(this);

                if (conditionalValue.IsNullOrEmpty() || conditionalValue.ToLower() == "false")
                {
                    renderer._outputSb = new StringBuilder();
                    ElseSb = PreviousSb;
                }
                else
                {
                    ElseSb = new StringBuilder();
                }
            }
        }

        public class Else : Directive
        {
            public override void MutateState(EmailRenderer renderer)
            {
                var conditional = renderer._conditionals.Peek();

                renderer._outputSb = conditional.ElseSb;
            }
        }

        public class EndConditional : Directive
        {
            public override void MutateState(EmailRenderer renderer)
            {
                renderer._outputSb = renderer._conditionals.Pop().PreviousSb;
            }
        }

        public abstract class Directive
        {
            public abstract void MutateState(EmailRenderer renderer);
        }
    }
}