using System.Linq;
using System.Net.Http;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using PaSharperExtension.Analyzers.HttpClientMethodCall.Context;
using PaSharperExtension.Extensions;

namespace PaSharperExtension.Analyzers.HttpClientMethodCall
{
    /// <summary>
    /// Variables declaration/assignment processor
    /// </summary>
    public sealed class VariablesProcessor
    {
        /// <summary>
        /// Variables context
        /// </summary>
        public VariablesInfoContext VariablesInfoContext { get; }

        /// <summary>
        /// "BaseAddress" property name to search base address declaration
        /// </summary>
        private const string HttpClientBaseAddressPropertyName = nameof(HttpClient.BaseAddress);

        public VariablesProcessor(VariablesInfoContext variablesInfoContext = null)
        {
            VariablesInfoContext = variablesInfoContext ?? new VariablesInfoContext();
        }

        /// <summary>
        /// Process assignment to reference
        /// </summary>
        /// <param name="assignmentExpression">Assignment expression</param>
        /// <param name="referenceExpression">Reference expression</param>
        public void ProcessAssignmentToReferenceExpression(IAssignmentExpression assignmentExpression, IReferenceExpression referenceExpression)
        {
            if (assignmentExpression.GetExpressionType().ToIType().IsUri()
                && referenceExpression.ConditionalQualifier is IReferenceExpression httpClientExpression
                && httpClientExpression.GetExpressionType().ToIType().IsHttpClient())
            {
                if (!(assignmentExpression.Dest is IReferenceExpression httpClientPropertyReferenceExpression))
                {
                    return;
                }

                if (httpClientPropertyReferenceExpression.NameIdentifier.Name != HttpClientBaseAddressPropertyName)
                {
                    return;
                }

                var httpClientVariableName = httpClientExpression.NameIdentifier.Name;

                var httpClientInfo = VariablesInfoContext.HttpClientInfos
                    .SingleOrDefault(i => i.VariableName == httpClientVariableName);

                if (httpClientInfo == null)
                {
                    return;
                }

                httpClientInfo.RootUriVariableInfo = assignmentExpression.Source switch
                {
                    IObjectCreationExpression objectCreationExpression => ProcessUriCreationExpression(objectCreationExpression),
                    IReferenceExpression uriReferenceExpression => VariablesInfoContext.UriVariables.SingleOrDefault(v => v.VariableName == uriReferenceExpression.NameIdentifier.Name),
                    _ => null
                };

                return;
            }

            if (assignmentExpression.GetExpressionType().ToIType().IsString())
            {
                var variableInfo = VariablesInfoContext.StringVariables
                    .SingleOrDefault(v => v.VariableName == referenceExpression.NameIdentifier.Name);

                if (variableInfo != null)
                {
                    switch (assignmentExpression.AssignmentType)
                    {
                        case AssignmentType.PLUSEQ:
                            if (!variableInfo.IsUnknown)
                            {
                                variableInfo.VariableValue += (string) assignmentExpression.Source.ConstantValue.Value;
                            }

                            break;
                        case AssignmentType.EQ:
                            variableInfo.VariableValue = (string) assignmentExpression.Source.ConstantValue.Value;
                            variableInfo.IsUnknown = false;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Process declaration
        /// </summary>
        public void ProcessDeclarationStatement(IDeclarationStatement declarationStatement)
        {
            foreach (var variableDeclaration in declarationStatement.VariableDeclarations)
            {
                if (variableDeclaration.Type.IsString())
                {
                    ProcessStringVariableDeclaration(variableDeclaration);
                    continue;
                }

                if (variableDeclaration.Type.IsUri())
                {
                    ProcessUriVariableDeclaration(variableDeclaration);
                    continue;
                }

                if (variableDeclaration.Type.IsHttpClient())
                {
                    ProcessHttpClientVariableDeclaration(variableDeclaration);
                    continue;
                }
            }
        }

        /// <summary>
        /// Process string variable declaration
        /// </summary>
        public void ProcessStringVariableDeclaration(ILocalVariableDeclaration variableDeclaration)
        {
            var stringVariableInfo = variableDeclaration.Initial is IExpressionInitializer {Value: ICSharpLiteralExpression literalExpression}
                ? ProcessStringLiteralExpression(literalExpression)
                : new StringVariableInfo();

            stringVariableInfo.VariableName = variableDeclaration.DeclaredName;
            stringVariableInfo.VariableDeclarationNode = variableDeclaration;

            VariablesInfoContext.StringVariables.Add(stringVariableInfo);
        }

        /// <summary>
        /// Process string constant declaration
        /// </summary>
        public void ProcessStringConstantDeclaration(IConstantDeclaration constantDeclaration)
        {
            var stringVariableInfo = constantDeclaration.ValueExpression is ICSharpLiteralExpression literalExpression
                ? ProcessStringLiteralExpression(literalExpression)
                : new StringVariableInfo();

            stringVariableInfo.VariableName = constantDeclaration.DeclaredName;
            stringVariableInfo.VariableDeclarationNode = constantDeclaration;

            VariablesInfoContext.StringVariables.Add(stringVariableInfo);
        }

        /// <summary>
        /// Process string literal expression
        /// </summary>
        public static StringVariableInfo ProcessStringLiteralExpression(ICSharpLiteralExpression literalExpression)
        {
            var stringVariableInfo = new StringVariableInfo
            {
                VariableValue = (string) literalExpression.ConstantValue.Value
            };

            return stringVariableInfo;
        }

        /// <summary>
        /// Process Uri variable declaration
        /// </summary>
        private void ProcessUriVariableDeclaration(ILocalVariableDeclaration variableDeclaration)
        {
            // TODO: declaration and initialization in different places
            if (!(variableDeclaration.Initial is IExpressionInitializer {Value: IObjectCreationExpression uriCreationExpression})
                || uriCreationExpression.Arguments.Count != 1)
            {
                return;
            }

            var uriVariableInfo = ProcessUriCreationExpression(uriCreationExpression);

            if (uriVariableInfo == null)
            {
                return;
            }

            uriVariableInfo.VariableName = variableDeclaration.DeclaredName;

            VariablesInfoContext.UriVariables.Add(uriVariableInfo);
        }

        /// <summary>
        /// Process Uri creation
        /// </summary>
        private UriVariableInfo ProcessUriCreationExpression(IObjectCreationExpression uriCreationExpression)
        {
            if (!uriCreationExpression.GetExpressionType().ToIType().IsUri()
                || uriCreationExpression.Arguments.Count != 1)
            {
                return null;
            }

            var uriCreationArgument = uriCreationExpression.Arguments[0];

            var uriStringVariableInfo = uriCreationArgument.Expression switch
            {
                ICSharpLiteralExpression cSharpLiteralExpression => ProcessStringLiteralExpression(cSharpLiteralExpression),
                IReferenceExpression referenceExpression => VariablesInfoContext.StringVariables
                    .SingleOrDefault(v => v.VariableName == referenceExpression.NameIdentifier.Name),
                _ => null
            };

            if (uriStringVariableInfo == null)
            {
                return null;
            }

            var uriVariableInfo = new UriVariableInfo
            {
                UriStringVariableInfo = uriStringVariableInfo
            };

            return uriVariableInfo;
        }

        /// <summary>
        /// Process HttpClient variable declaration
        /// </summary>
        public void ProcessHttpClientVariableDeclaration(ILocalVariableDeclaration variableDeclaration)
        {
            // TODO: declaration and initialization in different places
            if (!(variableDeclaration.Initial is IExpressionInitializer expressionInitializer))
            {
                return;
            }

            var varName = variableDeclaration.DeclaredName;

            var httpClientInfo = new HttpClientVariableInfo
            {
                VariableName = varName
            };

            VariablesInfoContext.HttpClientInfos.Add(httpClientInfo);

            // Check if inline initialization exists
            if (!(expressionInitializer.Value is IObjectCreationExpression
            {
                Initializer: IObjectInitializer objectInitializer
            }) || !objectInitializer.MemberInitializers.Any())
            {
                return;
            }

            // Check if BaseAddress is setting up during init
            var baseAddressPropertyInit = objectInitializer.MemberInitializers
                .OfType<IPropertyInitializer>()
                .SingleOrDefault(i => i.MemberName.Equals(HttpClientBaseAddressPropertyName));

            // HttpClient() { BaseAddress = new Uri(baseAddress)}
            //TODO: save all Uri variable initialization
            if (!(baseAddressPropertyInit?.Expression is IObjectCreationExpression baseAddressUriCreationExpression)
                || !baseAddressUriCreationExpression.GetExpressionType().ToIType().IsUri())
            {
                return;
            }

            httpClientInfo.RootUriVariableInfo = ProcessUriCreationExpression(baseAddressUriCreationExpression);
        }
    }
}
