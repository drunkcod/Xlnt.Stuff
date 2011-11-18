using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace Xlnt.Web.Mvc
{
    public interface IUrlFactory 
    {
        string Absolute(string relPath);
        string Action(string method, RouteValueDictionary routeValues);
    }

    public static class UrlHelperExtensions
    {
        class UrlHelperUrlFactory : IUrlFactory
        {
            readonly UrlHelper url;

            public UrlHelperUrlFactory(UrlHelper url) {
                this.url = url;
            }

            public string Absolute(string relPath) {
                //this is a complete train-wreck.. somewhere demeter is crying.
                return url.RequestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + relPath;
            }
    
            public string Action(string method, RouteValueDictionary routeValues) {
                return url.Action(method, routeValues);
            }
        }

        public static IUrlFactory AsUrlFactory(this UrlHelper self) {
            return new UrlHelperUrlFactory(self); 
        }

        public static string Action(this IUrlFactory self, Expression<Action> expr) {
            var body = (MethodCallExpression)expr.Body;
            var method = body.Method;
            return self.Action(method.Name, GetRouteValues(method.GetParameters(), body.Arguments));
        }

        static RouteValueDictionary GetRouteValues(ParameterInfo[] parameters, ReadOnlyCollection<Expression> arguments) {
            var routeValues = new RouteValueDictionary();
            for(int i = 0; i != parameters.Length; ++i) {
                var arg = arguments[i];
                routeValues.Add(parameters[i].Name, Value(arg));
            }
            return routeValues;
        }

        static object Value(Expression expr) {
            if(expr == null)
                return null;

            switch(expr.NodeType) {
                case ExpressionType.Constant: return (expr as ConstantExpression).Value;
                case ExpressionType.MemberAccess:
                    var memberExpression = expr as MemberExpression;
                    var field = (FieldInfo)memberExpression.Member;
                    return field.GetValue(Value(memberExpression.Expression));
                default: throw new NotSupportedException("Unsupported NodeType: {0} ({1})".Format(expr.NodeType, expr));
            }
        }
    }
}
