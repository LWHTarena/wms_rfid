﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Security.Principal;
using System.Diagnostics;
using Authority.Controllers;
using THOK.Common;
using SignalR;
using THOK.Wms.SignalR;
using THOK.Wms.SignalR.Connection;
using System.IO.Compression;
using THOK.Security;
using Wms.Security;
namespace Wms
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SystemEventLogAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapConnection<AutomotiveSystemsConnection>("automotiveSystems", "task/automotiveSystems/{*operation}");
            routes.MapConnection<AllotStockInConnection>("allotStockIn", "allotStockIn/{*operation}");
            routes.MapConnection<AllotStockOutConnection>("allotStockOut", "allotStockOut/{*operation}");
            routes.MapConnection<DispatchSortWorkConnection>("allotSortWork", "allotSortWork/{*operation}");
            routes.MapRoute(
                "Default", // 路由名称
                "{controller}/{action}/{SystemID}", // 带有参数的 URL
                new { controller = "Home", action = "Index", SystemID = UrlParameter.Optional } // 参数默认值
            );            
        }

        public static void RegisterIocUnityControllerFactory()
        {
            //Set for Controller Factory
            IControllerFactory controllerFactory = new UnityControllerFactory();
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
            GlobalHost.DependencyResolver = new UnityConnectionDependencyResolver();
        }

        void Application_Start()
        {
            UserServiceFactory userserviceFactory = new UserServiceFactory();
            ControllerBuilder.Current.SetControllerFactory(userserviceFactory);
            RegisterIocUnityControllerFactory();
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);           
            RegisterRoutes(RouteTable.Routes);
        }

        void Application_Error()
        {
            SystemEventLogFactory EventLogFactory = new SystemEventLogFactory();
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                Response.Clear();
                HttpException httpException = exception as HttpException;

                RouteData routeData = new RouteData();
                routeData.Values.Add("controller", "Home");

                string ModuleName = "1";
                string ModuleNam ="/"+ Context.Request.RequestContext.RouteData.Values["controller"].ToString()+"/";
                string FunctionName = Context.Request.RequestContext.RouteData.Values["action"].ToString();
                string ExceptionalType = exception.Message;
                string ExceptionalDescription = exception.ToString();
                string State = "1";
                if (httpException == null)
                {

                    if (Context.Request.RequestContext.RouteData.Values["action"].ToString() == "Index")
                    {
                        Session["ErrorLog"] = exception.Message;
                        routeData.Values.Add("action", "Error");
                    }
                    else
                    {
                        Session["AjaxErrorLog"] = exception.Message;
                        routeData.Values.Add("action", "AjaxError");
                    }
                    if (exception != null)
                    {
                        Trace.TraceError("Error occured and caught in Global.asax - {0}", exception.ToString());
                    }
                }
                else
                {
                    switch (httpException.GetHttpCode())
                    {
                        case 404:
                            if (Context.Request.RequestContext.RouteData.Values["action"].ToString() == "Index")
                            {
                                Session["PageNotFoundLog"] = exception.Message;
                                routeData.Values.Add("action", "PageNotFound");
                            }
                            else
                            {
                                Session["AjaxPageNotFoundLog"] = exception.Message;
                                routeData.Values.Add("action", "AjaxPageNotFound");
                            }
                            break;
                        case 500:
                            if (Context.Request.RequestContext.RouteData.Values["action"].ToString() == "Index")
                            {
                                Session["ServerErrorLog"] = exception.Message;
                                routeData.Values.Add("action", "ServerError");
                            }
                            else
                            {
                                Session["AjaxServerErrorLog"] = exception.Message;
                                routeData.Values.Add("action", "AjaxServerError");
                            }
                            Trace.TraceError("Server Error occured and caught in Global.asax - {0}", exception.ToString());
                            break;
                        default:
                            if (Context.Request.RequestContext.RouteData.Values["action"].ToString() == "Index")
                            {
                                Session["Error"] = exception.Message;
                                routeData.Values.Add("action", "Error");
                            }
                            else
                            {
                                Session["AjaxError"] = exception.Message;
                                routeData.Values.Add("action", "AjaxError");
                            }
                            Trace.TraceError("Error occured and caught in Global.asax - {0}", exception.ToString());
                            break;
                    }
                }
                if (ModuleName != ModuleNam)
                {
                    EventLogFactory.ExceptionalLogService.CreateExceptionLog(ModuleNam, FunctionName, ExceptionalType, ExceptionalDescription, State);
                    ModuleName = ModuleNam;
                }
                Server.ClearError();
                Response.TrySkipIisCustomErrors = true;
                IController errorController = new HomeController();
                errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            }
        }

        void Session_Start()
        {

        }

        void Session_End()
        {
            UserServiceFactory UserFactory = new UserServiceFactory();
            UserFactory.userService.DeleteUserIp(Session["username"].ToString());
            UserFactory.SystemEventLogService.UpdateLoginLog(Session["username"].ToString(),DateTime.Now.ToString());
        }        

        void Application_AuthenticateRequest1(object sender, EventArgs e)
        {
            bool enableGzip = this.Request.Headers["Content-Encoding"] == "gzip";
            if (enableGzip)
            {
                this.Response.Filter = new GZipStream(this.Response.Filter, CompressionMode.Compress);
                this.Response.AppendHeader("Content-Encoding", "gzip");
            }

            if (Context.User == null)
            {
                var oldTicket = ExtractTicketFromCookie(Context, FormsAuthentication.FormsCookieName);
                if (oldTicket != null && !oldTicket.Expired)
                {
                    var ticket = oldTicket;
                    if (FormsAuthentication.SlidingExpiration)
                    {
                        ticket = FormsAuthentication.RenewTicketIfOld(oldTicket);
                        if (ticket == null)
                        {
                            return;
                        }
                    }
                    string[] roles = new string[] { "Administrator" };
                    Context.User = new GenericPrincipal(new FormsIdentity(ticket), roles);
                    if (ticket != oldTicket)
                    {
                        string cookieValue = FormsAuthentication.Encrypt(ticket);
                        var cookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName] ?? new HttpCookie(FormsAuthentication.FormsCookieName, cookieValue) { Path = ticket.CookiePath };
                        if (ticket.IsPersistent)
                        {
                            cookie.Expires = ticket.Expiration;
                        }
                        cookie.Value = cookieValue;
                        cookie.Secure = FormsAuthentication.RequireSSL;
                        cookie.HttpOnly = true;
                        if (FormsAuthentication.CookieDomain != null)
                        {
                            cookie.Domain = FormsAuthentication.CookieDomain;
                        }
                        Context.Response.Cookies.Remove(cookie.Name);
                        Context.Response.Cookies.Add(cookie);
                    }
                }
            }
        }

        private static FormsAuthenticationTicket ExtractTicketFromCookie(HttpContext context, string name)
        {
            FormsAuthenticationTicket ticket = null;
            string encryptedTicket = null;

            var cookie = context.Request.Cookies[name];
            if (cookie != null)
            {
                encryptedTicket = cookie.Value;
            }

            if (!string.IsNullOrEmpty(encryptedTicket))
            {
                try
                {
                    ticket = FormsAuthentication.Decrypt(encryptedTicket);
                }
                catch
                {
                    context.Request.Cookies.Remove(name);
                }

                if (ticket != null && !ticket.Expired)
                {
                    return ticket;
                }

                context.Request.Cookies.Remove(name);
            }

            return null;
        }
    }
}