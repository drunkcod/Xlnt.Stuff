using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Xlnt.Stuff;

namespace Xlnt.Web.Mvc
{
    public class BasicControllerFactory : IControllerFactory
    {
        static readonly Regex NameMangling = new Regex("Controller$", RegexOptions.Compiled);
        readonly Dictionary<string, Func<IController>> controllers = new Dictionary<string, Func<IController>>(StringComparer.InvariantCultureIgnoreCase);

        public IController CreateController(RequestContext requestContext, string controllerName) {
            Func<IController> controller;
            if (controllers.TryGetValue(controllerName, out controller))
                return controller();
            return new MissingController();
        }

        public void RegisterController(string name, Func<IController> getController) {
            controllers.Add(name, getController);
        }

        public void RegisterController(string name, Type type) {
            RegisterController(name, (Func<IController>)Expression.Lambda(
                typeof(Func<IController>),
                Expression.New(type))
                    .Compile());
        }

        public void Register(IEnumerable<Type> types) {
            foreach (var item in types.Where(x => x.IsTypeOf<IController>()))
                RegisterController(NameFor(item), item);
        }

        string NameFor(Type controllerType) { 
            return NameMangling.Replace(controllerType.Name, string.Empty);
        }

        public void ReleaseController(IController controller) {
            var disposable = controller as IDisposable;
            if(disposable != null)
                disposable.Dispose();
        }


        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName) {
            return SessionStateBehavior.Default;
        }
    }
}
