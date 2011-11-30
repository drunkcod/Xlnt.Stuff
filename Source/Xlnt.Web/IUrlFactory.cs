using System.Web.Routing;

namespace Xlnt.Web
{
    public interface IUrlFactory 
    {
        string Absolute(string relPath);
        string Action(string method, RouteValueDictionary routeValues);
    }
}
