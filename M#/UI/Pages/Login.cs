using MSharp;
using Domain;

public class LoginPage : RootPage
{
    public LoginPage()
    {
        Route(@"login
            [#EMPTY#]");

        Layout(Layouts.Blank);

        Add<Modules.LoginForm>();

        MarkupTemplate("<div class=\"login-content\"><div class=\"card login\"><div class=\"card-body\">[#1#]</div></div></div>");

        OnStart(x =>
        {
            x.If("Request.IsAjaxPost()").CSharp("return Redirect(Url.CurrentUri().OriginalString);");
            x.If("User.Identity.IsAuthenticated").Go<Login.DispatchPage>().RunServerSide();
            x.If("Url.ReturnUrl().IsEmpty()").Go("/login").RunServerSide()
                .Send("ReturnUrl", ValueContext.Static, "/login");
        });
    }
}
